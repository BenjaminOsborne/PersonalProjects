using System.Collections.Immutable;
using System.Diagnostics;

namespace AccountProcessor.Components.Services
{
    public class Transaction
    {
        public DateOnly Date { get; init; }
        public string Description { get; init; }
        public decimal Amount { get; init; }
    }

    public class MatchedTransaction
    {
        public Transaction Transaction { get; init; }
        
        /// <summary> Preference ordered matches. Always at least 1 in collection. </summary>
        public ImmutableArray<(Section Section, Match Match)> SectionMatches { get; init; }
    }

    public class CategorisationResult
    {
        public ImmutableArray<MatchedTransaction> Matched { get; init; }
        public ImmutableArray<Transaction> UnMatched { get; init; }
    }

    public interface ITransactionCategoriser
    {

    }

    public class TransactionCategoriser : ITransactionCategoriser
    {
        /// <summary>
        /// Current use case; only 1 instance linked to 1 file.
        /// No intention to support multiple concurrent sessions distributed with many Clients.
        /// This would require persistence, client sessions etc.
        /// </summary>
        public static readonly Lazy<MatchModel> _singleModel = LazyHelper.Create(_InitialiseModel);

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
                    new MatchedTransaction
                    {
                        Transaction = x.Transaction,
                        SectionMatches = x.Matches
                            .Select(m => (m.Section, m.Match))
                            .ToImmutableArray()
                    })
                .ToImmutableArray();

            return new CategorisationResult
            {
                Matched = matched,
                UnMatched = parition.PredicateFalse
                    .Select(x => x.Transaction)
                    .ToImmutableArray()
            };
        }

        private static MatchModel _InitialiseModel()
        {
            var processPath = Process.GetCurrentProcess().MainModule!.FileName;
            var json = Path.Combine(new FileInfo(processPath).Directory!.FullName, "Components", "Services", "MatchModel.json");
            return JsonHelper.Deserialise<MatchModel>(File.ReadAllText(json))
                ?? throw new ApplicationException("Could not initialise model from json file");
        }
    }

    public class MatchModel
    {
        public ImmutableArray<Category> Categories { get; init; }
    }

    public class Block
    {
        public int Order { get; init; }
        public string Name { get; init; }
    }

    public class CategoryHeader : Block
    {
    }

    public class Category
    {
        public CategoryHeader Header { get; init; }
        public List<Section> Sections { get; init; }
    }

    public class Section : Block
    {
        public CategoryHeader Parent { get; init; }
        public List<Match> Matches { get; init; }
        /// <summary> If set; specifies the specific month/year this section applies to. </summary>
        /// <remarks> Day component should always be "1". Only Month/Year relevant. </remarks>
        public DateOnly? Month { get; init; }
    }

    public class Match
    {
        public string Pattern { get; init; }

        /// <summary> If left null, <see cref="Pattern"/> used for the Transaction Description. Else this is used. </summary>
        public string? OverrideDescription { get; init; }

        /// <summary> If set - only applies to specific transaction. <see cref="Pattern"/> should be exact Transaction title at this point. </summary>
        public DateOnly? ExactDate { get; init; }

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
