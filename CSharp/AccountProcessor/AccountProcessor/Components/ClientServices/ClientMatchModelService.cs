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
    public async Task<WrappedResult<IReadOnlyList<ModelMatchItem>>> GetAllModelMatchesAsync()
    {
        var response = await httpClient.GetAsync("matchmodel/all");
        return await response.MapJsonAsync<IReadOnlyList<ModelMatchItem>>();
    }

    public async Task<WrappedResult<string>> DisplayRawModelJsonSearchResultAsync(string? search)
    {
        var response = await httpClient.GetAsync($"matchmodel/search/{search}");
        return await response.MapJsonAsync<string>();
    }

    public async Task<Result> DeleteMatchItemAsync(ModelMatchItem row)
    {
        var response = await httpClient.PostAsJsonAsync("matchmodel/deletematchitem", row);
        return await response.MapBasicResultAsync();
    }
}