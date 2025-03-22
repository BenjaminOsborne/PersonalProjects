using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccountProcessor.Components.Controllers;

[ApiController]
[Route("[controller]")]
public class MatchModelController(IMatchModelService modelService) : ControllerBase
{
    [HttpGet("all")]
    public IReadOnlyList<ModelMatchItem> GetAllModelMatches() =>
        modelService.GetAllModelMatches();

    [HttpGet("search/{search}")]
    public string DisplayRawModelJsonSearchResult([FromRoute] string? search) =>
        modelService.DisplayRawModelJsonSearchResult(search);
    
    [HttpPost("deletematchitem")]
    public Result DeleteMatchItem(ModelMatchItem item)
    {
        var result = modelService.DeleteMatchItem(item);
        return result
            ? Result.Success
            : Result.Fail("Could not find Match Item");
    }
}