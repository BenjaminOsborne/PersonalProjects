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
        public Section Section { get; init; }
        public Match Match { get; init; }
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
        public DateOnly? ExtactDate { get; init; }
    }
}
