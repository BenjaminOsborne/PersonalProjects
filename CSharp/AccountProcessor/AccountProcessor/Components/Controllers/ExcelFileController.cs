using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;

namespace AccountProcessor.Components.Controllers;

[ApiController]
[Route("[controller]")]
public class ExcelFileController(IExcelFileHandler excelFileHandler) : ControllerBase
{
    public static class Banks
    {
        public const string CoopBank = nameof(CoopBank);
        public const string Santander = nameof(Santander);
    }

    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpPost("extracttransactions/{bankType}")]
    public async Task<ActionResult> ExtractTransactions([FromForm] IFormFile file, [FromRoute] string bankType) =>
        bankType switch
        {
            Banks.CoopBank => await _MapFileResult(file, excelFileHandler.CoopBank_ExtractCsvTransactionsToExcel,
                error: "Could not extract Coop transactions"),
            Banks.Santander => await _MapFileResult(file, excelFileHandler.Santander_ExtractExcelTransactionsToExcel,
                error: "Could not extract Santander transactions"),
            _ => BadRequest($"Unknown bank: {bankType}")
        };

    [HttpPost("loadtransactions")]
    public Task<ActionResult<ImmutableArray<Transaction>>> LoadTransactions([FromForm] IFormFile file) => 
        _MapResult(excelFileHandler.LoadTransactionsFromExcel(file.OpenReadStream()));

    [HttpPost("exporttransactions")]
    public async Task<ActionResult> ExportTransactions(CategoriseRequest request) =>
        _MapFileResult(
            result: await excelFileHandler.ExportCategorisedTransactionsToExcel(request),
            error: "Could not export transactions");

    private async Task<ActionResult<T>> _MapResult<T>(Task<WrappedResult<T>> task)
    {
        var result = await task;
        return result.IsSuccess
            ? Ok(result.Result)
            : BadRequest(result.Error);
    }

    private async Task<ActionResult> _MapFileResult(IFormFile file, Func<Stream, Task<WrappedResult<byte[]>>> fnGetTask, string error) =>
        _MapFileResult(result: await fnGetTask(file.OpenReadStream()), error);

    private ActionResult _MapFileResult(WrappedResult<byte[]> result, string error) =>
        result.IsSuccess
            ? File(new MemoryStream(result.Result!), ExcelContentType)
            : BadRequest($"{error}: {result.Error}");
}