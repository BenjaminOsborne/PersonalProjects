using System.Collections.Immutable;
using System.Diagnostics;

namespace AccountProcessor.Components.Services;

public static class ModelPersistence
{
    public static bool CanLoadModel() =>
        _GetModelJsonFilePath() != null;

    public static MatchModel LoadModel() =>
        _MapToDomainModel(_LoadModel());

    public static string GetRawJsonForDisplay() => 
        JsonHelper.Serialise(_LoadModel(), writeIndented: true);

    private static ModelData _LoadModel()
    {
        var jsonPath = _GetModelJsonFilePath() ?? throw new ApplicationException("Could not find MathModel.json");
        return JsonHelper.Deserialise<ModelData>(File.ReadAllText(jsonPath))
               ?? throw new ApplicationException("Could not initialise model from json file");
    }
    
    public static void WriteModel(MatchModel model)
    {
        var jsonPath = _GetModelJsonFilePath() ?? throw new ApplicationException("Could not find MathModel.json");
        var persistenceData = _MapToPersistence(model);
        var content = JsonHelper.Serialise(persistenceData, writeIndented: true);
        File.WriteAllText(jsonPath, content);
    }

    /// <remarks> Loads from GoogleDrive. Used to use: <see cref="DirectoryHelper.LoadModelFromSourceRepo"/> to load from source repo. </remarks>
    private static string? _GetModelJsonFilePath() =>
        DirectoryHelper.LoadModelFromGoogleDrive();

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
                            new Match(m.CreatedAt, m.Pattern, m.OverrideDescription, m.ExactDate)
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
                                    m.CreatedAt,
                                    m.Pattern,
                                    m.OverrideDescription,
                                    m.ExactDate))
                        )))));

    private record ModelData(ImmutableArray<CategoryData> Categories);

    /// <summary> Relates to the fixed set of <see cref="CategoryHeader"/>s </summary>
    private record CategoryData(string CategoryName, ImmutableArray<SectionMatchData> Sections);
    private record SectionMatchData(string SectionName, DateOnly? Month, ImmutableArray<MatchData> Matches);
    private record MatchData(DateTime? CreatedAt, string Pattern, string? OverrideDescription, DateOnly? ExactDate);
}

public static class DirectoryHelper
{
    /// <summary> Picks from: G:\My Drive\Finances\Accounting\AccountProcessor </summary>
    public static string? LoadModelFromGoogleDrive()
    {
        var dir = new DirectoryInfo("G:");
        var jsonPath = Path.Combine(dir.FullName, "My Drive", "Finances", "Accounting", "AccountProcessor", "MatchModel.json");
        return File.Exists(jsonPath)
            ? jsonPath
            : null;
    }

    /// <summary> Legacy: When MatchModel.json used to be stored in source repo </summary>
    /// <remarks> Searches from current process path (running in "bin") up to the source directory, then down to a known path in source control. </remarks>
    public static string? LoadModelFromSourceRepo()
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
}