using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AccountProcessor.Components.Pages;

public partial class FileActions
{
    [Inject]
    private IExcelFileHandler _excelFileHandler { get; init; } = null!;
    [Inject]
    private Microsoft.JSInterop.IJSRuntime _jsInterop { get; init; } = null!;

    [Parameter]
    public required HomeViewModel Model { get; init; }
    [Parameter]
    public required Action OnFileActionFinished { get; init; }

    private bool CanCategoriseTransactions() =>
        Model.CanCategoriseTransactions();

    private bool TransactionsAreFullyLoaded() =>
        Model.TransactionsAreFullyLoaded();

    public void OnAccountFileConverted(Result result)
    {
        Model.OnFileExtractResult(result);
        OnFileActionFinished();
    }

    private async Task LoadTransactionsAndCategorise(IBrowserFile? bf)
    {
        if (bf == null)
        {
            return;
        }
        await Model.LoadTransactionsAndCategorise(fnLoad: async () =>
        {
            using var inputStream = await bf.CopyToMemoryStreamAsync();
            return await _excelFileHandler.LoadTransactionsFromExcel(inputStream);
        });
        OnFileActionFinished();
    }

    private async Task ExportCategorisedTransactions()
    {
        var categorisationResult = Model.GetLatestCategorisationResultForExport();
        if (categorisationResult == null)
        {
            return;
        }
        await _OnFileResultDownloadBytes(
            result: await _excelFileHandler.ExportCategorisedTransactionsToExcel(categorisationResult!),
            fileName: $"CategorisedTransactions_{Model.Month!.Value:yyyy-MM}.xlsx");
        OnFileActionFinished();
    }

    private Task _OnFileResultDownloadBytes(WrappedResult<byte[]> result, string fileName)
    {
        Model.OnFileExtractResult(result);
        return result.IsSuccess
            ? _jsInterop.SaveAsFileAsync(fileName, result.Result!)
            : Task.CompletedTask;
    }
}