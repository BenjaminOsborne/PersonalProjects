using System.Collections.Immutable;
using System.Diagnostics;

namespace AccountProcessor.Components.Services
{
    public static class ModelPersistence
    {
        public static void WriteModel(MatchModel model)
        {
            var processPath = Process.GetCurrentProcess().MainModule!.FileName;
            var outputPath = GetOutputPath();
            var persistenceData = _MapToPersistence(model);
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

        public static MatchModel LoadModel()
        {
            string dir = _GetProcessDirectoryPath();
            var json = Path.Combine(dir, "Components", "Services", "MatchModel.json");
            var loaded = JsonHelper.Deserialise<ModelData>(File.ReadAllText(json))
                ?? throw new ApplicationException("Could not initialise model from json file");
            return _MapToDomainModel(loaded);
        }

        private static string _GetProcessDirectoryPath()
        {
            var processPath = Process.GetCurrentProcess().MainModule!.FileName;
            return new FileInfo(processPath).Directory!.FullName;
        }

        private static MatchModel _MapToDomainModel(ModelData data)
        {
            var categoryMap = CategoryHeader.AllValues.ToImmutableDictionary(x => x.Name);
            return new MatchModel(data.Categories
                .ToImmutableArray(c =>
                {
                    var category = categoryMap[c.CategoryName];
                    var sections = c.Sections
                        .Select((s, nx) => new SectionMatches(
                            new SectionHeader(nx, s.SectionName, category, s.Month),
                            s.Matches.ToImmutableArray(m =>
                                new Match(m.Pattern, m.OverrideDescription, m.ExactDate)
                                )))
                        .ToImmutableArray();
                    return new Category(category, sections);
                }));
        }

        private static ModelData _MapToPersistence(MatchModel model) =>
            new (model.Categories
                .OrderBy(x => x.Header.Order)
                .ToImmutableArray(c =>
                    new CategoryData(
                        c.Header.Name,
                        c.Sections
                            .OrderBy(s => s.Section.Order)
                            .ToImmutableArray(s => new SectionMatchData(
                                s.Section.Name,
                                s.Section.Month,
                                s.Matches
                                    .OrderBy(m => m.GetOrderKey())
                                    .ToImmutableArray(m => new MatchData(
                                        m.Pattern,
                                        m.OverrideDescription,
                                        m.ExactDate))
                                    )))));

        private record ModelData(ImmutableArray<CategoryData> Categories);

        /// <summary> Relates to the fixed set of <see cref="CategoryHeader"/>s </summary>
        private record CategoryData(string CategoryName, ImmutableArray<SectionMatchData> Sections);
        private record SectionMatchData(string SectionName, DateOnly? Month, ImmutableArray<MatchData> Matches);
        private record MatchData(string Pattern, string? OverrideDescription, DateOnly? ExactDate);
    }
}
