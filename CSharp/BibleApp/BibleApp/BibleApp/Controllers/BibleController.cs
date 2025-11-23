using Microsoft.AspNetCore.Mvc;

namespace BibleApp.Controllers;

[ApiController]
[Route("[controller]")]
public class BibleController : ControllerBase
{
    [HttpGet("books")]
    public IReadOnlyList<string> GetBooks()
    {
        return ["Hey!"];
    }
}