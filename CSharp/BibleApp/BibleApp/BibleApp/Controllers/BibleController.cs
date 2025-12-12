using BibleApp.Core;
using BibleApp.Source;
using Microsoft.AspNetCore.Mvc;

namespace BibleApp.Controllers;

[ApiController]
[Route("[controller]")]
public class BibleController : ControllerBase
{
    [HttpGet("translations")]
    public async Task<IReadOnlyList<TranslationId>> GetTranslations()
    {
        var all = await FileLoader.LoadAllAsync();
        return all.MaterialiseMap(x => new TranslationId(x.Id.Translation));
    }

    [HttpGet("bible/{translation}")]
    public async Task<ActionResult<BibleStructure>> GetBible(string translation)
    {
        var found = await _GetBibleAsync(translation);
        return found != null
            ? Ok(found.ToStructure())
            : NotFound();
    }

    [HttpGet("book/{translation}/{book}")]
    public async Task<ActionResult<Book>> GetBook(string translation, string book)
    {
        var found = await _GetBibleAsync(translation)
            .UnWrapAsync(b => b?.Books.SingleOrDefault(x => x.Id.BookName == book));
        return found != null
            ? Ok(found)
            : NotFound();
    }

    private static Task<Bible?> _GetBibleAsync(string translation) =>
        FileLoader.LoadAllAsync()
            .UnWrapAsync(b =>
                b.SingleOrDefault(x => x.Id.Translation == translation));
}