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

    [HttpGet("search")]
    public ModelJson DisplayRawModelJsonSearchResult([FromQuery] string? term) =>
        modelService.DisplayRawModelJsonSearchResult(term);
    
    [HttpPost("deletematchitem")]
    public Result DeleteMatchItem(ModelMatchItem item)
    {
        var result = modelService.DeleteMatchItem(item);
        return result
            ? Result.Success
            : Result.Fail("Could not find Match Item");
    }
}