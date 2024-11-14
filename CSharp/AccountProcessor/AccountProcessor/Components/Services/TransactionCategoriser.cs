using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace AccountProcessor.Components.Services
{
    public interface ITransactionCategoriser
    {
        SelectorData GetSelectorData();
        CategorisationResult Categorise(ImmutableArray<Transaction> transactions, DateOnly now);
    }

    public class TransactionCategoriser : ITransactionCategoriser
    {
        /// <summary>
        /// Current use case; only 1 instance linked to 1 file.
        /// No intention to support multiple concurrent sessions distributed with many Clients.
        /// This would require persistence, client sessions etc.
        /// </summary>
        public static readonly Lazy<MatchModel> _singleModel = LazyHelper.Create(_InitialiseModel);

        public SelectorData GetSelectorData()
        {
            var model = _singleModel.Value;
            var categories = model.Categories
                .Select(x => x.Header)
                .OrderBy(x => x.Order)
                .ToImmutableArray();
            var sections = model.Categories
                .SelectMany(x => x.Sections)
                .Select(x => x.Section)
                .OrderBy(x => x.Parent.Order)
                .ThenBy(x => x.Order)
                .ToImmutableArray();
            return new SelectorData(categories, sections);
        }

        public CategorisationResult Categorise(ImmutableArray<Transaction> transactions, DateOnly now)
        {
            var model = _singleModel.Value;
            
            var sections = model.Categories
                .SelectMany(x => x.Sections)
                .SelectMany(s => s.Matches.Select(m => new { Section = s, Match = m }))
                .ToImmutableArray();

            var withMatch = transactions
                .Select(trans =>
                {
                    var matches = sections
                    .Where(s => s.Match.Matches(trans))
                    .OrderBy(x => x)
                    .ToImmutableArray();
                    return new { Transaction = trans, Matches = matches };
                })
                .ToImmutableArray();

            var parition = withMatch.Partition(x => x.Matches.Any());
            var matched = parition.PredicateTrue
                .Select(x =>
                    new MatchedTransaction(
                        x.Transaction,
                        sectionMatches: x.Matches
                            .Select(m => new SectionMatch(m.Section.Section, m.Match))
                            .ToImmutableArray()
                    ))
                .ToImmutableArray();

            var unmatched = parition.PredicateFalse
                .Select(x => x.Transaction)
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
            var found = category.Sections.FirstOrDefault(s => s.Section.Name == sectionName);
            if (found != null)
            {
                return WrappedResult.Fail<SectionHeader>($"Already have section for: {sectionName}");
            }
            
            var next = category.Sections.Max(s => s.Section.Order) + 1;
            var created = new SectionHeader(next, sectionName, categoryHeader, matchMonthOnly);
            category.Sections.Add(new SectionMatches(created, []));
            return WrappedResult.Create(created);
        }

        public WrappedResult<SectionMatches> AddMatch(CategoryHeader header, Block section, Match match)
        {
            var category = _FindModelHeaderFor(header);
            if (category == null)
            {
                return WrappedResult.Fail<SectionMatches>($"Could not find matching Category for: {header.Name}");
            }
            var found = category.Sections.FirstOrDefault(s => s.Section.Name == section.Name);
            if (found == null)
            {
                return WrappedResult.Fail<SectionMatches>($"Could not find matching Section for: {section.Name}");
            }
            found.Matches.Add(match);
            return WrappedResult.Create(found);
        }

        private static Category? _FindModelHeaderFor(CategoryHeader header) =>
            _singleModel.Value.Categories
            .FirstOrDefault(x => x.Header.Name == header.Name);

        private static MatchModel _InitialiseModel()
        {
            var processPath = Process.GetCurrentProcess().MainModule!.FileName;
            var json = Path.Combine(new FileInfo(processPath).Directory!.FullName, "Components", "Services", "MatchModel.json");
            return JsonHelper.Deserialise<MatchModel>(File.ReadAllText(json))
                ?? throw new ApplicationException("Could not initialise model from json file");
        }
    }

    public record Transaction(DateOnly Date, string Description, decimal Amount);

    public record SelectorData(ImmutableArray<CategoryHeader> Categories, ImmutableArray<SectionHeader> Sections);

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

    public record CategorisationResult(ImmutableArray<MatchedTransaction> Matched, ImmutableArray<Transaction> UnMatched);

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
        public static CategoryHeader IGNORE { get; } = _Create(8, "IGNORE");

        #endregion

        private static CategoryHeader _Create(int order, string name) =>
            new CategoryHeader(order, name);
    }

    public record Category(CategoryHeader Header, List<SectionMatches> Sections);

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
    }

    public class SectionMatches
    {
        public SectionMatches(SectionHeader section, List<Match> matches)
        {
            Section = section;
            Matches = matches;
            
        }
        public SectionHeader Section { get; }
        public List<Match> Matches { get; }
    }

    public class Match
    {
        public static Match FromPatternOnly(string pattern) => new (pattern, null, null);

        public Match(string pattern, string? overrideDescription, DateOnly? exactDate)
        {
            Pattern = pattern;
            OverrideDescription = overrideDescription;
            ExactDate = exactDate;
        }

        public string Pattern { get; }

        /// <summary> If left null, <see cref="Pattern"/> used for the Transaction Description. Else this is used. </summary>
        public string? OverrideDescription { get; }

        /// <summary> If set - only applies to specific transaction. <see cref="Pattern"/> should be exact Transaction title at this point. </summary>
        public DateOnly? ExactDate { get; }

        /// <remarks> "False" is before "True" for OrderBy. Prefer overriden dates. </remarks>
        public IComparable OrderKey => (!ExactDate.HasValue, Pattern.Length);

        public bool Matches(Transaction trans)
        {
            if (ExactDate.HasValue && ExactDate != trans.Date)
            {
                return false;
            }

            //TODO: Consider handling "*" wild cards (or similar)
            return trans.Description.ToLower().Contains(Pattern.ToLower());
        }
    }
}
