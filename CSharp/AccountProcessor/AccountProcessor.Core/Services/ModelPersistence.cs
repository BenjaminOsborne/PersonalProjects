﻿using System.Collections.Immutable;
using System.Diagnostics;

namespace AccountProcessor.Core.Services;

public static class ModelPersistence
{
    public static bool CanLoadModel() =>
        _GetModelJsonFilePath() != null;

    public static MatchModel LoadModel() =>
        _MapToDomainModel(_LoadModel());

    public static string GetRawJson() =>
        _GetJsonForDisplay(_LoadModel());

    public static string GetRawJsonWithSearchFilter(string search)
    {
        var model = _LoadModel();
        var lowerSearch = search.ToLower();
        var catData = model.Categories
            .Select(c => c.WithFilter(lowerSearch))
            .Where(c => c.IsSuccess)
            .Select(x => x.Result!)
            .ToImmutableArray();
        return _GetJsonForDisplay(new(catData));
    }

    private static string _GetJsonForDisplay(ModelData model) =>
        JsonHelper.Serialise(model, writeIndented: true);

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
                        .OrderBy(s => s.Section.Month.HasValue) //Permanent sections (Month is null) first
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
    private record CategoryData(string CategoryName, ImmutableArray<SectionMatchData> Sections)
    {
        public WrappedResult<CategoryData> WithFilter(string lowerSearch)
        {
            if (_DoesMatch(CategoryName, lowerSearch))
            {
                return WrappedResult.Create(this);
            }
            var filtered = Sections
                    .Select(x => x.WithFilter(lowerSearch))
                    .Where(x => x.IsSuccess)
                    .Select(x => x.Result!)
                    .ToImmutableArray();
            return filtered.Any()
                ? WrappedResult.Create(this with { Sections = filtered })
                : WrappedResult.Fail<CategoryData>("No Matches");
        }
    }

    private record SectionMatchData(string SectionName, DateOnly? Month, ImmutableArray<MatchData> Matches)
    {
        public WrappedResult<SectionMatchData> WithFilter(string search)
        {
            if (_DoesMatch(SectionName, search))
            {
                return WrappedResult.Create(this);
            }

            var matches = Matches
                .Where(x => x.DoesMatch(search))
                .ToImmutableArray();
            return matches.Any()
                ? WrappedResult.Create(this with { Matches = matches })
                : WrappedResult.Fail<SectionMatchData>("No Matches");
        }
    }

    private record MatchData(DateTime? CreatedAt, string Pattern, string? OverrideDescription, DateOnly? ExactDate)
    {
        public bool DoesMatch(string search) =>
            _DoesMatch(Pattern, search) ||
            _DoesMatch(OverrideDescription, search);
    }

    private static bool _DoesMatch(string? data, string lowerSearch) =>
        data != null && data.ToLower().Contains(lowerSearch);
}

public static class DirectoryHelper
{
    /// <summary> Picks from: [drive]\My Drive\Finances\Accounting\AccountProcessor. Tries multiple locations. </summary>
    public static string? LoadModelFromGoogleDrive() =>
        new[] { "G:", "D:" }
        .Select(d => new DirectoryInfo(d))
        .Select(dir =>
        {
            var jsonPath = Path.Combine(dir.FullName, "My Drive", "Finances", "Accounting", "AccountProcessor", "MatchModel.json");
            return File.Exists(jsonPath)
                ? jsonPath
                : null;
        })
        .Where(x => x != null)
        .FirstOrDefault();

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