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
        var all = await FileLoader.LoadAllAsync();
        var found = all.SingleOrDefault(x => x.Id.Translation == translation);
        return found != null
            ? Ok(found.ToStructure())
            : NotFound();
    }
}