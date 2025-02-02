namespace AccountProcessor.Components.Services;

public interface IMatchModelService
{
    string LoadRawModelJson();
}

public class MatchModelService : IMatchModelService
{
    public string LoadRawModelJson() =>
        ModelPersistence.GetRawJsonForDisplay();
}