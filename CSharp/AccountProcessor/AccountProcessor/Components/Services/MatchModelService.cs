using System.Collections.Immutable;

namespace AccountProcessor.Components.Services;

public interface IMatchModelService
{
    string DisplayRawModelJsonSearchResult(string? search);
    ImmutableArray<ModelMatchItem> GetAllModelMatches();
}

public record ModelMatchItem(
    string HeaderName,
    string SectionName,
    DateOnly? SectionMonth,
    string Pattern,
    string? OverrideDescription,
    DateOnly? ExactDate);

public class MatchModelService : IMatchModelService
{
    public string DisplayRawModelJsonSearchResult(string? search) =>
        search.IsNullOrEmpty()
            ? ModelPersistence.GetRawJson()
            : ModelPersistence.GetRawJsonWithSearchFilter(search!);

    public ImmutableArray<ModelMatchItem> GetAllModelMatches()
    {
        var model = ModelPersistence.LoadModel();
        return model.Categories
            .SelectMany(cat => cat.Sections
                .SelectMany(sec => sec.Matches
                    .Select(mat => new ModelMatchItem(cat.Header.Name, sec.Section.Name, sec.Section.Month, mat.Pattern, mat.OverrideDescription, mat.ExactDate))))
            .ToImmutableArray();

    }
}