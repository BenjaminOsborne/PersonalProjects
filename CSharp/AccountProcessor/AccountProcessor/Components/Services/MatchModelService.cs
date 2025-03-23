using System.Collections.Immutable;

namespace AccountProcessor.Components.Services;

public interface IMatchModelService
{
    ModelJson DisplayRawModelJsonSearchResult(string? search);
    ImmutableArray<ModelMatchItem> GetAllModelMatches();

    bool DeleteMatchItem(ModelMatchItem row);
}

public record ModelJson(string Json);

public record ModelMatchItem(
    CategoryHeader Header,
    SectionHeader Section,
    Match Match)
{
    public string HeaderName => Header.Name;
    public string SectionName => Section.Name;
    public DateOnly? SectionMonth => Section.Month;
    public string Pattern => Match.Pattern;
    public string? OverrideDescription => Match.OverrideDescription;
    public bool MatchOnce => Match.ExactDate.HasValue;
    
    public string? SectionMonthDisplay => SectionMonth?.ToString("yyyy/MM");

    public bool MatchesSearch(string searchString) =>
        _MatchesSearch(
            lowerSearch: searchString.ToLower(),
            Header.Name,
            Section.Name,
            Pattern,
            Match.OverrideDescription,
            SectionMonthDisplay
        );

    static bool _MatchesSearch(string lowerSearch, params string?[] matches) =>
        matches
            .Where(x => !x.IsNullOrEmpty())
            .Select(x => x!.ToLower())
            .Any(m => m.Contains(lowerSearch));
}

public class MatchModelService : IMatchModelService
{
    public ModelJson DisplayRawModelJsonSearchResult(string? search) =>
        new(search.IsNullOrEmpty()
            ? ModelPersistence.GetRawJson()
            : ModelPersistence.GetRawJsonWithSearchFilter(search!));

    public ImmutableArray<ModelMatchItem> GetAllModelMatches()
    {
        var model = ModelPersistence.LoadModel();
        return model.Categories
            .SelectMany(cat => cat.Sections
                .SelectMany(sec => sec.Matches
                    .Select(mat => new ModelMatchItem(cat.Header, sec.Section, mat))))
            .ToImmutableArray();
    }

    public bool DeleteMatchItem(ModelMatchItem row) =>
        _Perform(row, (sm, match) => sm.DeleteMatch(match));

    private static bool _Perform(ModelMatchItem row, Func<SectionMatches, Match, Result> fnPerform)
    {
        var model = ModelPersistence.LoadModel();
        var sm = model.Categories
            .Where(c => c.Header.AreSame(row.Header))
            .SelectMany(x => x.Sections)
            .SingleOrDefault(s => s.Section.AreSame(row.Section));
        var match = sm?.Matches.SingleOrDefault(m => m.IsSameMatch(row.Match));
        if (match == null)
        {
            return false;
        }
        var result = fnPerform(sm!, match);
        if (result.IsSuccess == false)
        {
            return false;
        }
        ModelPersistence.WriteModel(model);
        return true;
    }
}