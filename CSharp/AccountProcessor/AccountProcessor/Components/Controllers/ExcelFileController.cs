using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Mvc;

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

    private const string _excelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpPost("extracttransactions/{bankType}")]
    public async Task<ActionResult> ExtractTransactions([FromForm] IFormFile file, [FromRoute] string bankType) =>
        bankType switch
        {
            Banks.CoopBank => await _MapResult(file, excelFileHandler.CoopBank_ExtractCsvTransactionsToExcel,
                error: "Could not extract Coop transactions"),
            Banks.Santander => await _MapResult(file, excelFileHandler.Santander_ExtractExcelTransactionsToExcel,
                error: "Could not extract Santander transactions"),
            _ => BadRequest($"Unknown bank: {bankType}")
        };

    private async Task<ActionResult> _MapResult(IFormFile file, Func<Stream, Task<WrappedResult<byte[]>>> fnGetTask, string error)
    {
        var result = await fnGetTask(file.OpenReadStream());
        return result.IsSuccess
            ? File(new MemoryStream(result.Result!), _excelContentType)
            : BadRequest($"{error}: {result.Error}");
    }
}