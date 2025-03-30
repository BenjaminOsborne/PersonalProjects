using System.Net.Http.Json;
using AccountProcessor.Core;
using AccountProcessor.Core.Services;

namespace AccountProcessor.Client.ClientServices;

public interface IClientMatchModelService
{
    Task<WrappedResult<MatchModelResult>> GetAllModelMatchesAsync();
    Task<WrappedResult<ModelJson>> DisplayRawModelJsonSearchResultAsync(string? search);
    Task<Result> DeleteMatchItemAsync(ModelMatchItem row);
    Task<Result> DeleteSectionAsync(SectionHeader section);
}

public class ClientMatchModelService(HttpClient httpClient) : IClientMatchModelService
{
    public Task<WrappedResult<MatchModelResult>> GetAllModelMatchesAsync() =>
        httpClient.GetAsync("matchmodel/all")
            .MapJsonAsync<MatchModelResult>();

    public Task<WrappedResult<ModelJson>> DisplayRawModelJsonSearchResultAsync(string? search) =>
        httpClient.GetAsync($"matchmodel/search?term={search}")
            .MapJsonAsync<ModelJson>();

    public Task<Result> DeleteMatchItemAsync(ModelMatchItem row) =>
        httpClient.PostAsJsonAsync("matchmodel/deletematchitem", row)
            .MapBasicResultAsync();

    public Task<Result> DeleteSectionAsync(SectionHeader section) =>
        httpClient.PostAsJsonAsync("matchmodel/deletesection", section)
            .MapBasicResultAsync();
}