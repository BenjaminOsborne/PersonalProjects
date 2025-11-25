using BibleApp.Core;
using BibleApp.Source;
using Microsoft.AspNetCore.Mvc;

namespace BibleApp.Controllers;

[ApiController]
[Route("[controller]")]
public class BibleController : ControllerBase
{
    [HttpGet("bibles")]
    public async Task<IReadOnlyList<BibleStructure>> GetBibles()
    {
        var all = await FileLoader.LoadAllAsync();
        return all.MaterialiseMap(x => x.ToStructure());
    }
}