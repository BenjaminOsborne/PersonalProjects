using AccountProcessor.Components.Services;
using NUnit.Framework;
using System.Collections.Immutable;

namespace AccountProcessor.Tests
{
    public class TransactionCategoriserRunner
    {
        [Test]
        public void BootstrapJson()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while(dir.Name != "CSharp")
            {
                dir = dir.Parent!;
            }

            var outputPath = Path.Combine(dir.FullName, "AccountProcessor", "AccountProcessor", "Components", "Services", "MatchModel.json");
            var content = JsonHelper.Serialise(_GetMatchModel(), writeIndented: true);
            File.WriteAllText(outputPath, content);
        }

        private MatchModel _GetMatchModel()
        {
            var headerCount = 0;

            return new MatchModel
            {
                Categories = new[]
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
                }.ToImmutableArray()
            };

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
                    .Select(x => new Section { Order = x.Order, Name = x.Name, Parent = header, Matches = [] })
                    .ToList()
            };
    }
}
