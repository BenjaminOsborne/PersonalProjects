using System.Collections.Immutable;
using System.Text;
using System.Text.Json.Serialization;

namespace AccountProcessor.Components.Services;

public interface ITransactionCategoriserScoped
{
    T PerformOnScope<T>(Func<ITransactionCategoriser, T> fnPerform);
}

public class TransactionCategoriserScoped : ITransactionCategoriserScoped
{
    public T PerformOnScope<T>(Func<ITransactionCategoriser, T> fnPerform)
    {
        var result = fnPerform(new TransactionCategoriser());
        return _RoundTripReadyForControllerSerialisation(result);
    }

    /// <summary> Temporary: Confirms all types being used are ready to be JSON serialised over the wire (when experiment with Client Blazor) </summary>
    private static T _RoundTripReadyForControllerSerialisation<T>(T result)
    {
        try
        {
            var json1 = JsonHelper.Serialise(result);
            var roundTrip = JsonHelper.Deserialise<T>(json1);
            var json2 = JsonHelper.Serialise(roundTrip);
            if (json1 != json2)
            {
                throw new Exception("JSON Round trip failed: Type not ready for controller serialisation");
            }
            return roundTrip!;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JSON Serialisation roundtrip error. Type: {typeof(T).FullName}\n{ex}");
            throw;
        }
    }
}

public interface ITransactionCategoriser
{
    bool CanCategoriseTransactions();

    SelectorData GetSelectorData(DateOnly month);
    CategorisationResult Categorise(CategoriseRequest request);

    WrappedResult<SectionHeader> AddSection(CategoryHeader categoryHeader, string sectionName, DateOnly? matchMonthOnly);

    Result ApplyMatch(Transaction transaction, SectionHeader section, string matchOn, string? overrideDescription);
    Result MatchOnce(Transaction transaction, SectionHeader header, string? matchOn, string? overrideDescription);

    Result DeleteMatch(SectionHeader section, Match match);
}

/// <summary>
/// Class is stateful (hence instance bound to an explicit "scope").
/// <see cref="_singleModel"/> provides a cached <see cref="MatchModel"/> for the duration of the scope so that
/// a sequence of method calls can perform mutations.
/// Note; UI layer gets a fresh scope for each operation... so behaves like the "scope" of a controller operation.
/// </summary>
public class TransactionCategoriser : ITransactionCategoriser
{
    public bool CanCategoriseTransactions() => ModelPersistence.CanLoadModel();

    public SelectorData GetSelectorData(DateOnly month)
    {
        if (!ModelPersistence.CanLoadModel())
        {
            return new SelectorData(false, null, null);
        }

        var model = _GetModel();
        var categories = model.Categories
            .Select(x => x.Header)
            .OrderBy(x => x.Order)
            .ToImmutableArray();
        var sections = model.Categories
            .SelectMany(x => x.Sections)
            .Where(x => x.Section.CanUseInMonth(month))
            .Select(x => new SectionUsage(Header: x.Section, LastUsed: GetSectionLastUsedAt(x.Matches)))
            .OrderBy(x => x.Header.Parent.Order)
            .ThenBy(x => x.Header.Order)
            .ToImmutableArray();
        return new SelectorData(true, categories, sections);

        static DateTime? GetSectionLastUsedAt(IEnumerable<Match> matches) =>
            matches
                .Select(x => x.CreatedAt)
                .Where(x => x.HasValue)
                .OrderByDescending(x => x!.Value)
                .FirstOrDefault();
    }

    public CategorisationResult Categorise(CategoriseRequest request)
    {
        var model = _GetModel();
        var sections = model.Categories
            .SelectMany(c => c.Sections)
            .ToImmutableArrayMany(s => s.Matches.Select(m => new { Section = s, Match = m }));

        var withMatch = request.Transactions
            .Where(trans => trans.IsInMonth(request.Month))
            .ToImmutableArray(trans =>
            {
                var matches = sections
                    .Select(s => (Section: s, Match: s.Match.Matches(trans)))
                    .Where(s => s.Match != MatchType.NoMatch)
                    .Partition(x => x.Match == MatchType.MatchExact)
                    .Map(x => x.Section);
                return new { Transaction = trans, ExactMatches = matches.PredicateTrue, HistoricMatches = matches.PredicateFalse };
            });

        var parition = withMatch.Partition(x => x.ExactMatches.Any());
        var matched = parition.PredicateTrue
            .ToImmutableArray(x =>
                new MatchedTransaction(
                    x.Transaction,
                    sectionMatches: x.ExactMatches
                        .ToImmutableArray(m => new SectionMatch(m.Section.Section, m.Match))
                ));

        var unmatched = parition.PredicateFalse
            .ToImmutableArray(unMatched =>
            {
                var firstSuggest = unMatched.HistoricMatches
                    .Select(x => new { x.Section.Section, x.Match.Pattern })
                    .FirstOrDefault(x => x.Section.CanUseInMonth(request.Month));
                var suggestedSection = firstSuggest?.Section;
                var suggestedPattern = firstSuggest?.Pattern;
                return new UnMatchedTransaction(unMatched.Transaction, suggestedSection, suggestedPattern);
            });
        return new CategorisationResult(matched, unmatched);
    }

    public WrappedResult<SectionHeader> AddSection(CategoryHeader categoryHeader, string sectionName, DateOnly? matchMonthOnly)
    {
        var category = _FindModelHeaderFor(categoryHeader);
        if (category == null)
        {
            return WrappedResult.Fail<SectionHeader>($"Could not find matching Category for: {categoryHeader.Name}");
        }

        var created = category.AddSection(sectionName, matchMonthOnly);
        if (created == null)
        {
            return WrappedResult.Fail<SectionHeader>($"Already have section for: {sectionName}");
        }
            
        _WriteModel();

        return WrappedResult.Create(created);
    }

    public Result ApplyMatch(Transaction transaction, SectionHeader section, string matchOn, string? overrideDescription)
    {
        var match = new Match(DateTime.UtcNow, matchOn, overrideDescription, null);
        return _AddNewMatch(transaction, section, match);
    }

    /// <remarks> Note: It is valid to specify <see cref="matchOn"/> (which may have a wild-card) as this can mean future transactions get a "suggestion", even if need to be confirmed. </remarks>
    public Result MatchOnce(Transaction transaction, SectionHeader section, string? matchOn, string? overrideDescription)
    {
        var match = new Match(DateTime.UtcNow, matchOn ?? transaction.Description, overrideDescription, transaction.Date);
        return _AddNewMatch(transaction, section, match);
    }

    public Result DeleteMatch(SectionHeader section, Match match)
    {
        var sectionMatches = _FindSection(section);
        if (!sectionMatches.IsSuccess)
        {
            return sectionMatches;
        }
        var foundMatch = sectionMatches.Result!.Matches
            .SingleOrDefault(m => m.IsSameMatch(match));
        if (foundMatch == null)
        {
            return Result.Fail("Could not find model match to delete");
        }
        var result = sectionMatches.Result!.DeleteMatch(foundMatch);
        _WriteModel();
        return result;
    }

    private Result _AddNewMatch(Transaction transaction, SectionHeader section, Match match)
    {
        var valid = match.GetIsValidResult();
        if (!valid.IsSuccess)
        {
            return valid;
        }
            
        var sectionMatches = _FindSection(section);
        if (!sectionMatches.IsSuccess)
        {
            return sectionMatches;
        }

        if (match.Matches(transaction) != MatchType.MatchExact)
        {
            return Result.Fail("Does not match transaction!");
        }

        sectionMatches.Result!.AddMatch(match);
        _WriteModel();

        return Result.Success;
    }

    private WrappedResult<SectionMatches> _FindSection(SectionHeader section)
    {
        var category = _FindModelHeaderFor(section.Parent);
        if (category == null)
        {
            return WrappedResult.Fail<SectionMatches>($"Could not find matching Category for: {section.Name}");
        }
        var found = category.Sections.FirstOrDefault(s => s.Section.Name == section.Name);
        if (found == null)
        {
            return WrappedResult.Fail<SectionMatches>($"Could not find matching Section for: {section.Name}");
        }

        return WrappedResult.Create(found);
    }

    private Category? _FindModelHeaderFor(CategoryHeader header) =>
        _GetModel().Categories
            .FirstOrDefault(x => x.Header.Name == header.Name);

    private MatchModel _GetModel() =>
        _singleModel.Value;

    /// <summary> Assumes stable model instance for lifetime of calling scope. If this changes, would need to pass a model instance around. </summary>
    private void _WriteModel() =>
        ModelPersistence.WriteModel(_GetModel());

    /// <summary>
    /// Current use case; only 1 instance linked to 1 file.
    /// No intention to support multiple concurrent sessions distributed with many Clients.
    /// This would require persistence, client sessions etc.
    /// This instance is kept for lifetime of scope, which is created fresh for each action - so file implicitly re-loads for each UI action.
    /// </summary>
    private readonly Lazy<MatchModel> _singleModel = LazyHelper.Create(ModelPersistence.LoadModel);
}

public record CategoriseRequest(ImmutableArray<Transaction> Transactions, DateOnly Month);

public record Transaction(DateOnly Date, string Description, decimal Amount)
{
    public bool IsInMonth(DateOnly month) =>
        Date.Year == month.Year && Date.Month == month.Month;

    public string DateDisplay => Date.ToString("ddd dd/MM");
}

public record SectionUsage(SectionHeader Header, DateTime? LastUsed);

public record SelectorData(bool IsModelLoaded, ImmutableArray<CategoryHeader>? Categories, ImmutableArray<SectionUsage>? Sections);

public class UnMatchedTransaction
{
    public UnMatchedTransaction(Transaction transaction, SectionHeader? suggestedSection, string? suggestedPattern)
    {
        Transaction = transaction;
        SuggestedSection = suggestedSection;
        SuggestedPattern = suggestedPattern;
    }

    public Transaction Transaction { get; }
    public SectionHeader? SuggestedSection { get; }
    public string? SuggestedPattern { get; }
}

public class MatchedTransaction
{
    public MatchedTransaction(Transaction transaction, ImmutableArray<SectionMatch> sectionMatches)
    {
        Transaction = transaction;
        SectionMatches = sectionMatches;
    }

    public Transaction Transaction { get; }

    /// <summary> Preference ordered matches. Always at least 1 in collection. </summary>
    public ImmutableArray<SectionMatch> SectionMatches { get; }
        
    /// <summary> As preference order, the current match is the first in the collection </summary>
    public SectionMatch SectionMatch => SectionMatches[0];
}

public record SectionMatch(SectionHeader Section, Match Match);

public record CategorisationResult(ImmutableArray<MatchedTransaction> Matched, ImmutableArray<UnMatchedTransaction> UnMatched);

public record MatchModel(ImmutableArray<Category> Categories);

public abstract class Block
{
    protected Block(int order, string name)
    {
        Order = order;
        Name = name;
    }
    public int Order { get; }
    public string Name { get; }

    public virtual IComparable GetKey() => (Order, Name);

    public bool AreSame(Block other) =>
        GetType() == other.GetType() && GetKey().Equals(other.GetKey());
}

public class CategoryHeader : Block
{
    [JsonConstructor]
    private CategoryHeader(int order, string name) : base(order, name)
    {
    }

    #region Fixed Categories - all sections/transactions must fit in one of these

    public static CategoryHeader Income { get; } = _Create(0, "Income");
    public static CategoryHeader Bills { get; } = _Create(1, "Bills");
    public static CategoryHeader Giving { get; } = _Create(2, "Giving");
    public static CategoryHeader House { get; } = _Create(3, "House");
    public static CategoryHeader Supermarkets { get; } = _Create(4, "Supermarkets");
    public static CategoryHeader Restaurants { get; } = _Create(5, "Restaurants");
    public static CategoryHeader TravelTrips { get; } = _Create(6, "Travel & Trips");
    public static CategoryHeader InternetShops { get; } = _Create(7, "Internet & Shops");
    public static CategoryHeader Manual { get; } = _Create(8, "Manual");

    public static ImmutableArray<CategoryHeader> AllValues { get; } = typeof(CategoryHeader)
        .GetAllStaticPublicPropertiesOfType<CategoryHeader>()
        .ToImmutableArray(x => x.value);

    #endregion

    private static CategoryHeader _Create(int order, string name) =>
        new (order, name);
}

public class Category
{
    public Category(CategoryHeader header, ImmutableArray<SectionMatches> sections)
    {
        Header = header;
        Sections = sections;
    }

    public CategoryHeader Header { get; }

    public ImmutableArray<SectionMatches> Sections { get; private set; }

    public SectionHeader? AddSection(string sectionName, DateOnly? matchMonthOnly)
    {
        var next = Sections.Max(s => s.Section.Order) + 1;
        var section = new SectionHeader(next, sectionName, Header, matchMonthOnly);

        if (Sections.Any(x => x.Section.IsClashing(section)))
        {
            return null;
        }
            
        var sectionMatches = new SectionMatches(section, []);
        Sections = Sections.Add(sectionMatches);
        return section;            
    }
}

public class SectionHeader : Block
{
    public SectionHeader(int order, string name, CategoryHeader parent, DateOnly? month) : base(order, name)
    {
        Parent = parent;
        Month = month;
    }

    public CategoryHeader Parent { get; }

    /// <summary> If set; specifies the specific month/year this section applies to. </summary>
    /// <remarks> Day component should always be "1". Only Month/Year relevant. </remarks>
    public DateOnly? Month { get; }

    public override IComparable GetKey() =>
        (Order, Name, Month, ParentKey: Parent.GetKey());

    /// <summary>
    /// Clashing if Name matches and EITHER
    /// - <see cref="Month"/> not set (i.e. general section)
    /// OR
    /// - <see cref="Month"/> set and same (i.e. defining for same month).
    /// 
    /// I.e. Can only have identical Name added if for set months which are different.
    /// </summary>
    public bool IsClashing(SectionHeader newSection) =>
        Name == newSection.Name && (Month == null || Month == newSection.Month);

    public bool CanUseInMonth(DateOnly month) =>
        Month == null || Month == month;
}

public class SectionMatches
{
    public SectionMatches(SectionHeader section, ImmutableArray<Match> matches)
    {
        Section = section;
        Matches = matches;
            
    }
    public SectionHeader Section { get; }
    public ImmutableArray<Match> Matches { get; private set; }

    /// <summary> Order when modifying - so ensures already ordered when used. </summary>
    public void AddMatch(Match match) =>
        Matches = Matches
            .ConcatItem(match)
            .OrderBy(x => x.GetOrderKey())
            .ToImmutableArray();

    public Result DeleteMatch(Match match)
    {
        var snap = Matches;
        Matches = snap.Remove(match);
        return snap != Matches
            ? Result.Success
            : Result.Fail("Could not find Match to delete");
    }
}

public enum MatchType
{
    NoMatch,
    MatchExact,
    MatchPreviousTransaction
}

public class Match
{
    private readonly Lazy<System.Text.RegularExpressions.Regex> _regex;
    private readonly int _wildCardCount;

    public Match(DateTime? createdAt, string pattern, string? overrideDescription, DateOnly? exactDate)
    {
        CreatedAt = createdAt;
        Pattern = pattern;
        OverrideDescription = overrideDescription;
        ExactDate = exactDate;
        _wildCardCount = pattern.Count(c => c == '*');
        _regex = LazyHelper.Create(() => _BuildRegex(pattern));
    }

    /// <summary>
    /// - <see cref="Pattern"/> must contain at least 3 letters or numbers (i.e. not just whitespace / wildcard)
    /// - <see cref="OverrideDescription"/> can be null, but if set, must be at least 3 characters (i.e. not just whitespace / wildcard)
    /// </summary>
    public Result GetIsValidResult()
    {
        if (Pattern.IsNullOrEmpty())
        {
            return Result.Fail("Pattern must be defined");
        }
        if (_wildCardCount > 0 && Pattern.Count(char.IsLetterOrDigit) < 3)
        {
            return Result.Fail("Match Pattern with wildcard(s) should contain at least 3 characters");
        }
        if (OverrideDescription != null && OverrideDescription.Count(char.IsLetterOrDigit) < 3)
        {
            return Result.Fail("If Match Override Description defined, must be at least 3 characters");
        }
        return Result.Success;
    }

    public bool IsSameMatch(Match other) =>
        Pattern == other.Pattern &&
        OverrideDescription == other.OverrideDescription &&
        ExactDate == other.ExactDate;

    /// <summary> Null for older entries. Used to suggest preferences for picking categories </summary>
    public DateTime? CreatedAt { get; }

    public string Pattern { get; }

    /// <summary> If left null, <see cref="Pattern"/> used for the Transaction Description. Else this is used. </summary>
    public string? OverrideDescription { get; }

    /// <summary> If set - only applies to specific transaction. <see cref="Pattern"/> should be exact Transaction title at this point. </summary>
    public DateOnly? ExactDate { get; }

    public string GetDescription() => OverrideDescription ?? Pattern;

    /// <remarks> "False" is before "True" for OrderBy. Prefer overriden dates & non-wild-card matches first. </remarks>
    public IComparable GetOrderKey() => (!ExactDate.HasValue, _wildCardCount, Pattern.Length);

    public MatchType Matches(Transaction trans)
    {
        var doesMatch = _regex.Value.IsMatch(trans.Description.ToLower());
        if (!doesMatch)
        {
            return MatchType.NoMatch;
        }
        return ExactDate.HasValue && ExactDate != trans.Date
            ? MatchType.MatchPreviousTransaction
            : MatchType.MatchExact;
    }

    private static System.Text.RegularExpressions.Regex _BuildRegex(string pattern)
    {
        var builder = new StringBuilder();
        builder.Append("^");

        var wildLiteral = "\\*"; //support escaping * so can match
        var splitWildLiteral = pattern.ToLower().Split(wildLiteral);
        for (var outNx = 0; outNx < splitWildLiteral.Length; outNx++)
        {
            var outer = splitWildLiteral[outNx];
            if(outNx > 0)
            {
                builder.Append(wildLiteral);
            }

            var spl = outer.ToLower().Split('*');
            for (var inNx = 0; inNx < spl.Length; inNx++)
            {
                if (inNx > 0)
                {
                    builder.Append(".*"); //Inset regex wildcard between sections
                }
                //Escape regex (as text could contain other non-supported regex characters)
                builder.Append(System.Text.RegularExpressions.Regex.Escape(spl[inNx]));
            }
        }

        builder.Append("$");
        return new System.Text.RegularExpressions.Regex(builder.ToString());
    }
}