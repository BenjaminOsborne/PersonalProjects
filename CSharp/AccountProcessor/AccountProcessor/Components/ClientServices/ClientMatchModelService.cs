using AccountProcessor.Components.Services;

namespace AccountProcessor.Components.ClientServices;

public interface IClientMatchModelService
{
    Task<WrappedResult<IReadOnlyList<ModelMatchItem>>> GetAllModelMatchesAsync();
    Task<WrappedResult<string>> DisplayRawModelJsonSearchResultAsync(string? search);
    Task<Result> DeleteMatchItemAsync(ModelMatchItem row);
}

public class ClientMatchModelService(HttpClient httpClient) : IClientMatchModelService
{
    public Task<WrappedResult<IReadOnlyList<ModelMatchItem>>> GetAllModelMatchesAsync() =>
        httpClient.GetAsync("matchmodel/all")
            .MapJsonAsync<IReadOnlyList<ModelMatchItem>>();

    public Task<WrappedResult<string>> DisplayRawModelJsonSearchResultAsync(string? search) =>
        httpClient.GetAsync($"matchmodel/search/{search}")
            .MapJsonAsync<string>();

    public Task<Result> DeleteMatchItemAsync(ModelMatchItem row) =>
        httpClient.PostAsJsonAsync("matchmodel/deletematchitem", row)
            .MapBasicResultAsync();
}