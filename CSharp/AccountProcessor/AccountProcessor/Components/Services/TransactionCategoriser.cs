using System.Collections.Immutable;

namespace AccountProcessor.Components.Services
{
    public interface ITransactionCategoriser
    {
        ImmutableArray<Category> GetCategories();
    }

    public class TransactionCategoriser : ITransactionCategoriser
    {
        public ImmutableArray<Category> GetCategories()
        {
            var headerCount = 0;

            return new[]
            {
                _CreateCategory(Header("Income"), _ToBlocks("Salary")),
                _CreateCategory(Header("Bills"), _ToBlocks("House", "Standing Order")),
                _CreateCategory(Header("Giving"), _ToBlocks("Charity", "Tithe")),
                _CreateCategory(Header("House"), _ToBlocks("House & Garage", "Childcare", "Exercise")),
                _CreateCategory(Header("Supermarkets"), _ToBlocks("Supermarket")),
                _CreateCategory(Header("Restaurants"), _ToBlocks("Restaurant", "Drink")),
                _CreateCategory(Header("Travel & Trips"), _ToBlocks("Transport", "Petrol")),
                _CreateCategory(Header("Internet & Shops"), _ToBlocks("Online", "Cash", "Shops")),
                _CreateCategory(Header("IGNORE"), _ToBlocks("Internal Transactions", "Balance")),
            }.ToImmutableArray();

            CategoryHeader Header(string name) =>
                new CategoryHeader { Order = headerCount++, Name = name };
        }

        private Block[] _ToBlocks(params string[] names) =>
            names.Select((x, nx) => new Block { Order = nx, Name = x }).ToArray();

        private static Category _CreateCategory(CategoryHeader header, params Block[] sections) =>
            new Category
            {
                Header = header,
                Sections = sections
                    .Select(x => new Section { Order = x.Order, Name = x.Name, Parent = header })
                    .ToList()
            };
    }

    public class Block
    {
        public int Order { get; init; }
        public string Name { get; init; }
    }

    public class CategoryHeader : Block
    {
    }

    public class MatchModel
    {
        public ImmutableArray<Category> Categories { get; init; }
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
