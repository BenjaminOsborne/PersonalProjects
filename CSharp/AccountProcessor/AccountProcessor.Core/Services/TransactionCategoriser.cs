using System.Collections.Immutable;
using System.Text;
using System.Text.Json.Serialization;

namespace AccountProcessor.Core.Services;

public record AddSectionRequest(CategoryHeader CategoryHeader, string SectionName, DateOnly? MatchMonthOnly);

public record MatchRequest(Transaction Transaction, SectionHeader Section, string? MatchOn, string? OverrideDescription);

public record DeleteMatchRequest(SectionHeader Section, Match Match);

public interface ITransactionCategoriser
{
    bool CanCategoriseTransactions();

    SelectorData GetSelectorData(DateOnly month);
    CategorisationResult Categorise(CategoriseRequest request);

    WrappedResult<SectionHeader> AddSection(AddSectionRequest request);

    Result ApplyMatch(MatchRequest request);
    Result MatchOnce(MatchRequest request);

    Result DeleteMatch(DeleteMatchRequest request);
}

/// <summary>
/// Class is stateful (hence instance bound to an explicit "scope").
/// <see cref="_singleModel"/> provides a cached <see cref="MatchModel"/> for the duration of the scope so that
/// a sequence of method calls can perform mutations.
/// Note; service is Scoped so fresh instance for each request
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
                    .Select(s => (Section: s, s.Match, TypeMatch: s.Match.Matches(trans)))
                    .Where(s => s.TypeMatch != MatchType.NoMatch)
                    .OrderBy(s => s.Section.Section.Section.Month == null) //Prefer month-specific (ensures specific-match on section takes transactions first)
                    .ThenBy(x => x.Match.GetOrderKey()) //Then order by Match OrderKey
                    .Partition(x => x.TypeMatch == MatchType.MatchExact)
                    .Map(x => x.Section);
                return new { Transaction = trans, ExactMatches = matches.PredicateTrue, HistoricMatches = matches.PredicateFalse };
            });

        var parition = withMatch.Partition(x => x.ExactMatches.Any());
        var matched = parition.PredicateTrue
            .ToImmutableArray(x =>
                new MatchedTransaction(
                    x.Transaction,
                    SectionMatches: x.ExactMatches
                        .ToImmutableArray(m => new SectionMatch(m.Section.Section, m.Match))
                ));

        var unmatched = parition.PredicateFalse
            .ToImmutableArray(unMatched =>
            {
                var firstSuggest = unMatched.HistoricMatches
                    .Select(x => new SuggestedMatch(x.Section.Section, x.Match.Pattern, SuggestedMatchOnce: x.Match.ExactDate.HasValue))
                    .FirstOrDefault(x => x.SuggestedSection.CanUseInMonth(request.Month)); //Filter to where applicable this month
                return new UnMatchedTransaction(unMatched.Transaction, firstSuggest);
            });
        return new CategorisationResult(matched, unmatched);
    }

    public WrappedResult<SectionHeader> AddSection(AddSectionRequest request)
    {
        var category = _FindModelHeaderFor(request.CategoryHeader);
        if (category == null)
        {
            return WrappedResult.Fail<SectionHeader>($"Could not find matching Category for: {request.CategoryHeader.Name}");
        }

        var created = category.AddSection(request.SectionName, request.MatchMonthOnly);
        if (created == null)
        {
            return WrappedResult.Fail<SectionHeader>($"Already have section for: {request.SectionName}");
        }
            
        _WriteModel();

        return WrappedResult.Create(created);
    }

    public Result ApplyMatch(MatchRequest request)
    {
        var match = new Match(DateTime.UtcNow, pattern: request.MatchOn ?? "", overrideDescription: request.OverrideDescription, exactDate: null);
        return _AddNewMatch(request.Transaction, request.Section, match);
    }

    /// <remarks> Note: It is valid to specify <see cref="MatchRequest.MatchOn"/> (which may have a wild-card) as this can mean future transactions get a "suggestion", even if need to be confirmed. </remarks>
    public Result MatchOnce(MatchRequest request)
    {
        var match = new Match(DateTime.UtcNow, request.MatchOn ?? request.Transaction.Description, request.OverrideDescription, request.Transaction.Date);
        return _AddNewMatch(request.Transaction, request.Section, match);
    }

    public Result DeleteMatch(DeleteMatchRequest request)
    {
        var sectionMatches = _FindSection(request.Section);
        if (!sectionMatches.IsSuccess)
        {
            return sectionMatches;
        }
        var foundMatch = sectionMatches.Result!.Matches
            .SingleOrDefault(m => m.IsSameMatch(request.Match));
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

    /// <remarks> Replace after F2 achieves: "£3.00" is "£3" but "£3.50" stays "£3.50" </remarks>
    public string AmountDisplay =>
        (Amount < 0 ? $"-£{-Amount:F2}" : $"£{Amount:F2}").Replace(".00", "");

    public string AmountDisplayAbsolute =>
        $"£{(Amount < 0 ? -Amount : Amount):F2}".Replace(".00", "");
}

public record SectionUsage(SectionHeader Header, DateTime? LastUsed);

public record SelectorData(bool IsModelLoaded, ImmutableArray<CategoryHeader>? Categories, ImmutableArray<SectionUsage>? Sections);

public record SuggestedMatch(SectionHeader SuggestedSection, string SuggestedPattern, bool SuggestedMatchOnce);

public record UnMatchedTransaction(Transaction Transaction, SuggestedMatch? SuggestedMatch);

/// <summary> <see cref="SectionMatches"/> is in preference-order. Always at least 1 in collection. </summary>
public record MatchedTransaction(Transaction Transaction, ImmutableArray<SectionMatch> SectionMatches)
{
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

    public Result DeleteSection(SectionMatches sm)
    {
        var snap = Sections;
        Sections = snap.Remove(sm);
        return snap != Sections //true if new array has changed, i.e. item removed.
            ? Result.Success
            : Result.Fail("Could not find Section to delete");
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
        return snap != Matches //true if new array has changed, i.e. item removed.
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
        _wildCardCount = _GetWildCardCountFromPattern(pattern);
        _regex = LazyHelper.Create(() => _BuildRegex(pattern));
    }

    /// <summary>
    /// - <see cref="Pattern"/> must contain at least 3 letters or numbers (i.e. not just whitespace / wildcard)
    /// - <see cref="OverrideDescription"/> can be null, but if set, must be at least 3 characters (i.e. not just whitespace / wildcard)
    /// </summary>
    public Result GetIsValidResult() =>
        _GetIsValidResult(Pattern, _wildCardCount, OverrideDescription);
    
    public static Result GetIsValidResult(string pattern, string? overrideDescription) =>
        _GetIsValidResult(pattern, _GetWildCardCountFromPattern(pattern), overrideDescription);

    private static int _GetWildCardCountFromPattern(string pattern) =>
        pattern.Count(c => c == '*');

    private static Result _GetIsValidResult(string pattern, int wildCardCount, string? overrideDescription)
    {
        if (pattern.IsNullOrEmpty())
        {
            return Result.Fail("Pattern must be defined");
        }
        if (wildCardCount > 0 && pattern.Count(char.IsLetterOrDigit) < 3)
        {
            return Result.Fail("Match Pattern with wildcard(s) should contain at least 3 characters");
        }

        if (overrideDescription != null && overrideDescription.Count(char.IsLetterOrDigit) < 3)
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