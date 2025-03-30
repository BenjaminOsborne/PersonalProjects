using AccountProcessor.Core;
using AccountProcessor.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccountProcessor.Controllers;

[ApiController]
[Route("[controller]")]
public class MatchModelController(IMatchModelService modelService) : ControllerBase
{
    [HttpGet("all")]
    public MatchModelResult GetAllModelMatches() =>
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
    
    [HttpPost("deletesection")]
    public Result DeleteSection(SectionHeader section)
    {
        var result = modelService.DeleteSelection(section);
        return result
            ? Result.Success
            : Result.Fail("Could not find Section");
    }
}