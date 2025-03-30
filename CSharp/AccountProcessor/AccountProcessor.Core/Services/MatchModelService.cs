using System.Collections.Immutable;

namespace AccountProcessor.Core.Services;

public interface IMatchModelService
{
    ModelJson DisplayRawModelJsonSearchResult(string? search);
    MatchModelResult GetAllModelMatches();

    bool DeleteMatchItem(ModelMatchItem matchItem);
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

    public bool DeleteMatchItem(ModelMatchItem matchItem) =>
        _Perform(matchItem, (model, mi) =>
        {
            var sm = model.Categories
                .Where(c => c.Header.AreSame(mi.Header))
                .SelectMany(x => x.Sections)
                .SingleOrDefault(s => s.Section.AreSame(mi.Section));
            var match = sm?.Matches.SingleOrDefault(m => m.IsSameMatch(mi.Match));
            return match != null &&
                   sm!.DeleteMatch(match).IsSuccess;
        });

    public bool DeleteSelection(SectionHeader section) =>
        _Perform(section, (model, match) =>
        {
            var catSec = model.Categories
                .SelectMany(cat => cat.Sections.Select(sm => (cat, sm)))
                .Where(s => s.sm.Section.GetKey().Equals(match.GetKey()))
                .Select(x => x.AsNullable())
                .SingleOrDefault();
            return catSec != null &&
                   catSec.Value.cat.DeleteSection(catSec.Value.sm).IsSuccess;
        });

    private static bool _Perform<T>(T item, Func<MatchModel, T, bool> fnPerform)
    {
        var model = ModelPersistence.LoadModel();
        var success = fnPerform(model, item);
        if (success)
        {
            ModelPersistence.WriteModel(model);
        }
        return success;
    }
}