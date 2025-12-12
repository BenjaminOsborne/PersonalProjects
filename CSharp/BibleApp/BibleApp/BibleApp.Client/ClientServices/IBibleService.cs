using BibleApp.Core;

namespace BibleApp.Client.ClientServices;

public interface IBibleService
{
    Task<Result<IReadOnlyList<TranslationId>>> GetTranslationsAsync();
    Task<Result<BibleStructure>> GetBibleAsync(TranslationId translation);
    Task<Result<Book>> GetBookAsync(TranslationId translation, BookId book);
}

public class BibleService(HttpClient httpClient) : IBibleService
{
    public Task<Result<IReadOnlyList<TranslationId>>> GetTranslationsAsync() =>
        httpClient.GetAsync("bible/translations")
            .MapJsonAsync<IReadOnlyList<TranslationId>>();

    public Task<Result<BibleStructure>> GetBibleAsync(TranslationId translation) =>
        httpClient.GetAsync($"bible/bible/{translation.Translation}")
            .MapJsonAsync<BibleStructure>();

    public Task<Result<Book>> GetBookAsync(TranslationId translation, BookId book) =>
        httpClient.GetAsync($"bible/book/{translation.Translation}/{book.BookName}")
            .MapJsonAsync<Book>();

}