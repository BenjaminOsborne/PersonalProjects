using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;

namespace AccountProcessor.Components.Services
{
    public interface ITransactionCategoriser
    {
        SelectorData GetSelectorData(DateOnly month);
        CategorisationResult Categorise(ImmutableArray<Transaction> transactions, DateOnly month);

        WrappedResult<SectionHeader> AddSection(CategoryHeader categoryHeader, string sectionName, DateOnly? matchMonthOnly);

        Result ApplyMatch(Transaction transaction, SectionHeader section, string matchOn, string? overrideDescription);
        Result MatchOnce(Transaction transaction, SectionHeader header, string? matchOn, string? overrideDescription);
    }

    public class TransactionCategoriser : ITransactionCategoriser
    {
        /// <summary>
        /// Current use case; only 1 instance linked to 1 file.
        /// No intention to support multiple concurrent sessions distributed with many Clients.
        /// This would require persistence, client sessions etc.
        /// </summary>
        public static readonly Lazy<MatchModel> _singleModel = LazyHelper.Create(_InitialiseModel);

        public SelectorData GetSelectorData(DateOnly month)
        {
            var model = _singleModel.Value;
            var categories = model.Categories
                .Select(x => x.Header)
                .OrderBy(x => x.Order)
                .ToImmutableArray();
            var sections = model.Categories
                .SelectMany(x => x.Sections)
                .Select(x => x.Section)
                .Where(x => x.Month == null || x.Month == month)
                .OrderBy(x => x.Parent.Order)
                .ThenBy(x => x.Order)
                .ToImmutableArray();
            return new SelectorData(categories, sections);
        }

        public CategorisationResult Categorise(ImmutableArray<Transaction> transactions, DateOnly month)
        {
            var model = _singleModel.Value;
            
            var sections = model.Categories
                .SelectMany(c => c.Sections)
                .SelectMany(s => s.Matches.Select(m => new { Section = s, Match = m }))
                .ToImmutableArray();

            var withMatch = transactions
                .Where(trans => trans.IsInMonth(month))
                .Select(trans =>
                {
                    var matches = sections
                        .Select(s => (Section: s, Match: s.Match.Matches(trans)))
                        .Where(s => s.Match != MatchType.NoMatch)
                        .Partition(x => x.Match == MatchType.MatchExact)
                        .Map(x => x.Section);
                    return new { Transaction = trans, ExactMatches = matches.PredicateTrue, HistoricMatches = matches.PredicateFalse };
                })
                .ToImmutableArray();

            var parition = withMatch.Partition(x => x.ExactMatches.Any());
            var matched = parition.PredicateTrue
                .Select(x =>
                    new MatchedTransaction(
                        x.Transaction,
                        sectionMatches: x.ExactMatches
                            .Select(m => new SectionMatch(m.Section.Section, m.Match))
                            .ToImmutableArray()
                    ))
                .ToImmutableArray();

            var unmatched = parition.PredicateFalse
                .Select(unMatched =>
                {
                    var suggestedSection = unMatched.HistoricMatches
                        .Select(x => x.Section.Section)
                        .FirstOrDefault();
                    var suggestedPattern = unMatched.HistoricMatches
                        .Select(x => x.Match.Pattern)
                        .FirstOrDefault();
                    return new UnMatchedTransaction(unMatched.Transaction, suggestedSection, suggestedPattern);
                })
                .ToImmutableArray();
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
            var match = new Match(matchOn, overrideDescription, null);
            return _ApplyMatch(transaction, section, match);
        }

        /// <remarks> Note: It is valid to specify <see cref="matchOn"/> (which may have a wild-card) as this can mean future transactions get a "suggestion", even if need to be confirmed. </remarks>
        public Result MatchOnce(Transaction transaction, SectionHeader section, string? matchOn, string? overrideDescription)
        {
            var match = new Match(matchOn ?? transaction.Description, overrideDescription, transaction.Date);
            return _ApplyMatch(transaction, section, match);
        }

        private static Result _ApplyMatch(Transaction transaction, SectionHeader section, Match match)
        {
            if (!Match.IsValidPattern(match.Pattern))
            {
                return Result.Fail("Match should contain at least 3 characters");
            }

            var category = _FindModelHeaderFor(section.Parent);
            if (category == null)
            {
                return Result.Fail($"Could not find matching Category for: {section.Name}");
            }
            var found = category.Sections.FirstOrDefault(s => s.Section.Name == section.Name);
            if (found == null)
            {
                return Result.Fail($"Could not find matching Section for: {section.Name}");
            }

            if (match.Matches(transaction) != MatchType.MatchExact)
            {
                return Result.Fail("Does not match transaction!");
            }

            found.AddMatch(match);
            _WriteModel();

            return Result.Success;
        }

        private static Category? _FindModelHeaderFor(CategoryHeader header) =>
            _singleModel.Value.Categories
            .FirstOrDefault(x => x.Header.Name == header.Name);

        private static MatchModel _InitialiseModel()
        {
            string dir = _GetProcessDirectoryPath();
            var json = Path.Combine(dir, "Components", "Services", "MatchModel.json");
            return JsonHelper.Deserialise<MatchModel>(File.ReadAllText(json))
                ?? throw new ApplicationException("Could not initialise model from json file");
        }

        private static void _WriteModel()
        {
            var model = _singleModel.Value;
            var processPath = Process.GetCurrentProcess().MainModule!.FileName;
            var outputPath = GetOutputPath();
            var content = JsonHelper.Serialise(model, writeIndented: true);
            File.WriteAllText(outputPath, content);

            static string GetOutputPath()
            {
                var dir = new DirectoryInfo(_GetProcessDirectoryPath());
                while (dir.Name != "CSharp")
                {
                    dir = dir.Parent!;
                }
                return Path.Combine(dir.FullName, "AccountProcessor", "AccountProcessor", "Components", "Services", "MatchModel.json");
            }
        }

        private static string _GetProcessDirectoryPath()
        {
            var processPath = Process.GetCurrentProcess().MainModule!.FileName;
            return new FileInfo(processPath).Directory!.FullName;
        }
    }

    public record Transaction(DateOnly Date, string Description, decimal Amount)
    {
        public bool IsInMonth(DateOnly month) =>
            Date.Year == month.Year && Date.Month == month.Month;
    }

    public record SelectorData(ImmutableArray<CategoryHeader> Categories, ImmutableArray<SectionHeader> Sections);

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
    }

    public class CategoryHeader : Block
    {
        [JsonConstructor]
        private CategoryHeader(int Order, string Name) : base(Order, Name)
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
        public static CategoryHeader IGNORE { get; } = _Create(9, "IGNORE");

        #endregion

        private static CategoryHeader _Create(int order, string name) =>
            new CategoryHeader(order, name);
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

        public bool AreSame(SectionHeader other) => other != null && _GetKey().Equals(other._GetKey());

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

        private IComparable _GetKey() => (Order, Name, Month, Parent.Order, Parent.Name);
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

        /// <summary> Must contain at least 3 letters or numbers (i.e. not whitespace or wildcard) </summary>
        public static bool IsValidPattern(string pattern) =>
            pattern?.Count(char.IsLetterOrDigit) >= 3;

        public static Match FromPatternOnly(string pattern) => new (pattern, null, null);

        public Match(string pattern, string? overrideDescription, DateOnly? exactDate)
        {
            Pattern = pattern;
            OverrideDescription = overrideDescription;
            ExactDate = exactDate;
            _wildCardCount = pattern.Count(c => c == '*');
            _regex = LazyHelper.Create(() => _BuildRegex(pattern));
        }

        public string Pattern { get; }

        /// <summary> If left null, <see cref="Pattern"/> used for the Transaction Description. Else this is used. </summary>
        public string? OverrideDescription { get; }

        /// <summary> If set - only applies to specific transaction. <see cref="Pattern"/> should be exact Transaction title at this point. </summary>
        public DateOnly? ExactDate { get; }

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
            builder.Append(pattern.ToLower().Replace("*", ".*"));
            builder.Append("$");
            return new System.Text.RegularExpressions.Regex(builder.ToString());
        }
    }
}
