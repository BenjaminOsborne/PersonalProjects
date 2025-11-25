using BibleApp.Core;

namespace BibleApp.Client.ClientServices;

public interface IBibleService
{
    Task<Result<IReadOnlyList<BibleStructure>>> GetBiblesAsync();
}

public class BibleService(HttpClient httpClient) : IBibleService
{
    public Task<Result<IReadOnlyList<BibleStructure>>> GetBiblesAsync() =>
        httpClient.GetAsync("bible/bibles")
            .MapJsonAsync<IReadOnlyList<BibleStructure>>();

}