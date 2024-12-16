using System.Collections.Immutable;
using System.Diagnostics;

namespace AccountProcessor.Components.Services
{
    public static class ModelPersistence
    {
        public static bool CanLoadModel() =>
            _GetModelJsonFilePath() != null;

        public static MatchModel LoadModel()
        {
            var jsonPath = _GetModelJsonFilePath() ?? throw new ApplicationException("Could not find MathModel.json");
            var loaded = JsonHelper.Deserialise<ModelData>(File.ReadAllText(jsonPath))
                ?? throw new ApplicationException("Could not initialise model from json file");
            return _MapToDomainModel(loaded);
        }

        public static void WriteModel(MatchModel model)
        {
            var jsonPath = _GetModelJsonFilePath() ?? throw new ApplicationException("Could not find MathModel.json");
            var persistenceData = _MapToPersistence(model);
            var content = JsonHelper.Serialise(persistenceData, writeIndented: true);
            File.WriteAllText(jsonPath, content);
        }

        /// <summary> Searches from current process path (running in "bin") up to the source directory, then down to a known path in source control. </summary>
        private static string? _GetModelJsonFilePath()
        {
            var processPath = Process.GetCurrentProcess().MainModule!.FileName;
            var dirPath = new FileInfo(processPath).Directory!.FullName;
            var dirInfo = new DirectoryInfo(dirPath);
            while (dirInfo.Name != "CSharp")
            {
                dirInfo = dirInfo.Parent!;
            }
            var jsonPath = Path.Combine(dirInfo.FullName, "AccountProcessor", "AccountProcessor", "Components", "Services", "MatchModel.json");
            return File.Exists(jsonPath)
                ? jsonPath
                : null;
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
                            .OrderBy(s => s.Section.Month.HasValue) //Permananent sections (Month is null) first
                            .ThenBy(s => s.Section.Order) //Then keep order of creation (NOT by alphabet)
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
