﻿using AccountProcessor.Core;
using AccountProcessor.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccountProcessor.Controllers;

[ApiController]
[Route("[controller]")]
public class CategoriserController(ITransactionCategoriser categoriser) : ControllerBase
{
    [HttpGet("cancategorise")]
    public Result CanCategorise()
    {
        var result = categoriser.CanCategoriseTransactions();
        return result
            ? Result.Success
            : Result.Fail("Could not load model");
    }

    [HttpGet("getselectordata/{month}")]
    public SelectorData DisplayRawModelJsonSearchResult([FromRoute] string month) =>
        categoriser.GetSelectorData(DateOnly.Parse(month));

    [HttpPost("categorise")]
    public CategorisationResult Categorise([FromBody] CategoriseRequest request) =>
        categoriser.Categorise(request);

    [HttpPost("addsection")]
    public ActionResult<SectionHeader> AddSection([FromBody] AddSectionRequest request)
    {
        var result = categoriser.AddSection(request);
        return result.IsSuccess
            ? Ok(result.Result)
            : BadRequest(result.Error);
    }

    [HttpPost("applymatch")]
    public Result ApplyMatch([FromBody] MatchRequest request) =>
        categoriser.ApplyMatch(request);

    [HttpPost("matchonce")]
    public Result MatchOnce([FromBody] MatchRequest request) =>
        categoriser.MatchOnce(request);

    [HttpPost("deletematch")]
    public Result DeleteMatch([FromBody] DeleteMatchRequest request) =>
        categoriser.DeleteMatch(request);
}