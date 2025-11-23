using BibleApp.Core;
using BibleApp.Source;
using Microsoft.AspNetCore.Mvc;

namespace BibleApp.Controllers;

[ApiController]
[Route("[controller]")]
public class BibleController : ControllerBase
{
    [HttpGet("books")]
    public async Task<IReadOnlyList<string>> GetBooks()
    {
        var all = await FileLoader.LoadAllAsync();
        return all[0].Books.Select(x => x.Name).Materialise();
    }
}