using System.Net.Http.Json;
using AccountProcessor.Core;
using AccountProcessor.Core.Services;

namespace AccountProcessor.Client.ClientServices;

public interface IClientMatchModelService
{
    Task<WrappedResult<IReadOnlyList<ModelMatchItem>>> GetAllModelMatchesAsync();
    Task<WrappedResult<ModelJson>> DisplayRawModelJsonSearchResultAsync(string? search);
    Task<Result> DeleteMatchItemAsync(ModelMatchItem row);
}

public class ClientMatchModelService(HttpClient httpClient) : IClientMatchModelService
{
    public Task<WrappedResult<IReadOnlyList<ModelMatchItem>>> GetAllModelMatchesAsync() =>
        httpClient.GetAsync("matchmodel/all")
            .MapJsonAsync<IReadOnlyList<ModelMatchItem>>();

    public Task<WrappedResult<ModelJson>> DisplayRawModelJsonSearchResultAsync(string? search) =>
        httpClient.GetAsync($"matchmodel/search?term={search}")
            .MapJsonAsync<ModelJson>();

    public Task<Result> DeleteMatchItemAsync(ModelMatchItem row) =>
        httpClient.PostAsJsonAsync("matchmodel/deletematchitem", row)
            .MapBasicResultAsync();
}