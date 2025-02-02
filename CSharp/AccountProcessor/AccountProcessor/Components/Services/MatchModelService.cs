namespace AccountProcessor.Components.Services;

public interface IMatchModelService
{
    string LoadRawModelJson();
    string DisplaySearchResult(string search);
}

public class MatchModelService : IMatchModelService
{
    public string LoadRawModelJson() =>
        ModelPersistence.GetRawJson();

    public string DisplaySearchResult(string search) =>
        search.IsNullOrEmpty()
            ? ModelPersistence.GetRawJson()
            : ModelPersistence.GetRawJsonWithSearchFilter(search);
}