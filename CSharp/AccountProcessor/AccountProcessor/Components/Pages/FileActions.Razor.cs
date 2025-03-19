using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AccountProcessor.Components.Pages;

public enum FileActionType
{
    None = 0,
    ConvertBankTransactionsFile,
    LoadTransactions,
    ExportTransactions
}

public partial class FileActions
{
    [Inject]
    private IExcelFileHandler _excelFileHandler { get; init; } = null!;
    [Inject]
    private Microsoft.JSInterop.IJSRuntime _jsInterop { get; init; } = null!;

    [Parameter]
    public required HomeViewModel Model { get; init; }
    [Parameter]
    public required Action<(FileActionType, Result)> OnFileActionFinished { get; init; }

    private bool CanCategoriseTransactions() =>
        Model.CanCategoriseTransactions();

    private bool TransactionsAreFullyLoaded() =>
        Model.TransactionsAreFullyLoaded();

    public async Task OnAccountFileConverted(WrappedResult<byte[]> result)
    {
        Model.OnFileExtractResult(result);
        OnFileActionFinished((FileActionType.ConvertBankTransactionsFile, result));

        //If converted successfully - immediately load transactions for categorisation
        if (result.IsSuccess)
        {
            using var stream = new MemoryStream(result.Result!);
            await _LoadTransactionsAndCategorise(stream);
        }
    }

    private async Task LoadTransactionsAndCategorise(IBrowserFile? bf)
    {
        if (bf == null)
        {
            return;
        }
        using var inputStream = await bf.CopyToMemoryStreamAsync();
        await _LoadTransactionsAndCategorise(inputStream);
    }

    private async Task _LoadTransactionsAndCategorise(Stream inputStream)
    {
        var result = await _excelFileHandler.LoadTransactionsFromExcel(inputStream);
        Model.LoadTransactionsAndCategorise(result);
        OnFileActionFinished((FileActionType.LoadTransactions, result));
    }

    private async Task ExportCategorisedTransactions()
    {
        var transactions = Model.GetLoadedTransactions();
        var month = Model.Month;
        if (transactions == null || month == null)
        {
            return;
        }

        var result = await _OnFileResultDownloadBytes(
            result: await _excelFileHandler.ExportCategorisedTransactionsToExcel(new (transactions.Value, month.Value)),
            fileName: $"CategorisedTransactions_{Model.Month!.Value:yyyy-MM}.xlsx");
        
        OnFileActionFinished((FileActionType.ExportTransactions, result));
    }

    private async Task<Result> _OnFileResultDownloadBytes(WrappedResult<byte[]> result, string fileName)
    {
        Model.OnFileExtractResult(result);
        await (result.IsSuccess
            ? _jsInterop.SaveAsFileAsync(fileName, result.Result!)
            : Task.CompletedTask);
        return result;
    }
}