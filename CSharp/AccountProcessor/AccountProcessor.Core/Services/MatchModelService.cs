using System.Collections.Immutable;

namespace AccountProcessor.Core.Services;

public interface IMatchModelService
{
    ModelJson DisplayRawModelJsonSearchResult(string? search);
    MatchModelResult GetAllModelMatches();

    bool DeleteMatchItem(ModelMatchItem row);
    bool DeleteSelection(SectionHeader section);
}

public record ModelJson(string Json);

public record MatchModelResult(ImmutableArray<SectionAndMatches> Sections);

public record SectionAndMatches(SectionHeader Section, ImmutableArray<ModelMatchItem> MatchItems);

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
    public string? MatchOnceDate => Match.ExactDate?.ToString("yyyy/MM/dd");

    public string? SectionMonthDisplay => SectionMonth?.ToString("yyyy/MM");

    public bool MatchesSearch(string searchString) =>
        SearchHelper.MatchesSearch(
            lowerSearch: searchString.ToLower(),
            Header.Name,
            Section.Name,
            Pattern,
            Match.OverrideDescription,
            SectionMonthDisplay
        );
}

public static class SearchHelper
{
    public static bool MatchesSearch(string lowerSearch, params string?[] matches) =>
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

    public MatchModelResult GetAllModelMatches() =>
        new(ModelPersistence.LoadModel()
            .Categories
            .SelectMany(cat => cat.Sections
                .Select(sec => new SectionAndMatches(
                    sec.Section,
                    sec.Matches
                        .Select(mat => new ModelMatchItem(cat.Header, sec.Section, mat))
                        .ToImmutableArray()))
            )
            .ToImmutableArray());

    public bool DeleteMatchItem(ModelMatchItem row) =>
        _Perform(row, (sm, match) => sm.DeleteMatch(match));

    public bool DeleteSelection(SectionHeader section)
    {
        var model = ModelPersistence.LoadModel();
        var sec = model.Categories
            .SelectMany(cat => cat.Sections.Select(sm => (cat, sm)))
            .Where(s => s.sm.Section.GetKey().Equals(section.GetKey()))
            .Select(x => x.AsNullable())
            .SingleOrDefault();
        var deleted = sec?.cat.DeleteSection(sec.Value.sm) ?? false;
        if (deleted)
        {
            ModelPersistence.WriteModel(model);
        }
        return deleted;
    }

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