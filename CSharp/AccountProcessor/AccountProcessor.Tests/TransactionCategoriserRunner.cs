using AccountProcessor.Components.Services;
using NUnit.Framework;
using System.Collections.Immutable;

namespace AccountProcessor.Tests
{
    public class TransactionCategoriserRunner
    {
        [Test, Ignore("ONLY RUN ONCE BEFORE LIVE EDITING MODEL")]
        public void Bootstrap_Json_from_scratch()
        {
            var outputPath = _GetOutputPath();
            var model = _CreateInitialModel();

            //Add example for Income<>Salary
            model.Categories[0].Sections[0].AddMatch(new Match("Accurx Limited Pay", "Accurx Salary", null));

            var content = _WriteModel(outputPath, model);

            var loaded = JsonHelper.Deserialise<MatchModel>(content);
            Assert.That(loaded!.Categories.Length, Is.EqualTo(model.Categories.Length));
        }

        [Test, Ignore("ONLY RUN TO ADD A NEW SECTION")]]
        public void Add_manual_section()
        {
            var outputPath = _GetOutputPath();
            MatchModel loaded;
            
            using(var fs = File.OpenRead(outputPath))
            {
                loaded = JsonHelper.Deserialise<MatchModel>(fs)!;
            }

            var catToMatch = CategoryHeader.TravelTrips.Name;
            var category = loaded.Categories.Single(x => x.Header.Name == catToMatch);
            var next = category.Sections.Max(s => s.Section.Order) + 1;
            category.AddSection(new SectionMatches(new SectionHeader(next, "Ad-hoc Trips", category.Header, null), []));

            _WriteModel(outputPath, loaded);
        }

        private MatchModel _CreateInitialModel()
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
                _CreateCategory(CategoryHeader.Manual, "Manual Sort"),
                _CreateCategory(CategoryHeader.IGNORE, "Internal Transactions", "Balance"),
            }.ToImmutableArray();
            return new MatchModel(categories);
        }

        private static Category _CreateCategory(CategoryHeader header, params string[] sections)
        {
            var sectionList = sections
                .Select((x,nx) => new SectionMatches(new SectionHeader(nx, x, header, month: null), []))
                .ToImmutableArray();
            return new Category(header, sectionList);
        }

        private static string _WriteModel(string outputPath, MatchModel model)
        {
            var content = JsonHelper.Serialise(model, writeIndented: true);
            File.WriteAllText(outputPath, content);
            return content;
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
