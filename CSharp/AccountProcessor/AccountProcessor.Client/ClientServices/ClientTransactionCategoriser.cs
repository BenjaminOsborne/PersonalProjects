using System.Net.Http.Json;
using AccountProcessor.Core;
using AccountProcessor.Core.Services;

namespace AccountProcessor.Client.ClientServices;

public interface IClientTransactionCategoriser
{
    Task<Result> CanCategoriseTransactionsAsync();
    Task<WrappedResult<SelectorData>> GetSelectorDataAsync(DateOnly month);
    Task<WrappedResult<CategorisationResult>> CategoriseAsync(CategoriseRequest request);

    Task<WrappedResult<SectionHeader>> AddSectionAsync(AddSectionRequest request);
    
    Task<Result> ApplyMatchAsync(MatchRequest request);
    Task<Result> MatchOnceAsync(MatchRequest request);
    
    Task<Result> DeleteMatchAsync(DeleteMatchRequest request);
}

public class ClientTransactionCategoriser(HttpClient httpClient) : IClientTransactionCategoriser
{
    public Task<Result> CanCategoriseTransactionsAsync() =>
        httpClient.GetAsync("categoriser/cancategorise")
            .MapBasicResultAsync();

    public Task<WrappedResult<SelectorData>> GetSelectorDataAsync(DateOnly month) =>
        httpClient.GetAsync($"categoriser/getselectordata/{month:yyyy-MM-dd}")
            .MapJsonAsync<SelectorData>();

    public Task<WrappedResult<CategorisationResult>> CategoriseAsync(CategoriseRequest request) =>
        httpClient.PostAsJsonAsync("categoriser/categorise", request)
            .MapJsonAsync<CategorisationResult>();

    public Task<WrappedResult<SectionHeader>> AddSectionAsync(AddSectionRequest request) =>
        httpClient.PostAsJsonAsync("categoriser/addsection", request)
            .MapJsonAsync<SectionHeader>();

    public Task<Result> ApplyMatchAsync(MatchRequest request) =>
        httpClient.PostAsJsonAsync("categoriser/applymatch", request)
            .MapBasicResultAsync();

    public Task<Result> MatchOnceAsync(MatchRequest request) =>
        httpClient.PostAsJsonAsync("categoriser/matchonce", request)
            .MapBasicResultAsync();

    public Task<Result> DeleteMatchAsync(DeleteMatchRequest request) =>
        httpClient.PostAsJsonAsync("categoriser/deletematch", request)
            .MapBasicResultAsync();
}