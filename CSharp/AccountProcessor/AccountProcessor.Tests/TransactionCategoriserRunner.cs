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
            var outputPath = _GetOutputPath();
            var model = _GetMatchModel();
            var content = JsonHelper.Serialise(model, writeIndented: true);
            File.WriteAllText(outputPath, content);

            var loaded = JsonHelper.Deserialise<MatchModel>(content);
            Assert.That(loaded.Categories.Length, Is.EqualTo(model.Categories.Length));
        }

        private MatchModel _GetMatchModel()
        {
            var categories = new[]
            {
                _CreateCategory(CategoryHeader.Income, "Salary"),
                _CreateCategory(CategoryHeader.Bills, "House", "Standing Order"),
                _CreateCategory(CategoryHeader.Giving, "Charity", "Tithe"),
                _CreateCategory(CategoryHeader.House, "House & Garage", "Childcare", "Exercise"),
                _CreateCategory(CategoryHeader.Supermarkets, "Supermarket"),
                _CreateCategory(CategoryHeader.Restaurants, "Restaurant", "Drink"),
                _CreateCategory(CategoryHeader.TravelTrips, "Transport", "Petrol"),
                _CreateCategory(CategoryHeader.InternetShops, "Online", "Cash", "Shops"),
                _CreateCategory(CategoryHeader.IGNORE, "Internal Transactions", "Balance"),
            }.ToImmutableArray();
            return new MatchModel(categories);
        }

        private static Category _CreateCategory(CategoryHeader header, params string[] sections)
        {
            var sectionList = sections
                .Select((x,nx) => new SectionMatches(new SectionHeader(nx, x, header, month: null), []))
                .ToList();
            return new Category(header, sectionList);
        }

        private static string _GetOutputPath()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir.Name != "CSharp")
            {
                dir = dir.Parent!;
            }
            return Path.Combine(dir.FullName, "AccountProcessor", "AccountProcessor", "Components", "Services", "MatchModel.json");
        }
    }
}
