using BibleApp.Core;

namespace BibleApp.Client.ClientServices;

public interface IBibleService
{
    Task<Result<IReadOnlyList<string>>> GetBooksAsync();
}

public class BibleService(HttpClient httpClient) : IBibleService
{
    public Task<Result<IReadOnlyList<string>>> GetBooksAsync() =>
        httpClient.GetAsync("bible/books")
            .MapJsonAsync<IReadOnlyList<string>>();

}