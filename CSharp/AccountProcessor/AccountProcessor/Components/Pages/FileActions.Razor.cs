using AccountProcessor.ClientServices;
using AccountProcessor.Services;
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
    [Inject] private IClientExcelFileService _excelFileService { get; init; } = null!;
    [Inject] private Microsoft.JSInterop.IJSRuntime _jsInterop { get; init; } = null!;

    [Parameter]
    public required HomeViewModel Model { get; init; }
    [Parameter]
    public required Action<(FileActionType, Result)> OnFileActionFinished { get; init; }

    private bool _canCategoriseTransactions;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _canCategoriseTransactions = await Model.CanCategoriseTransactionsAsync();
    }
    
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
        var result = await _excelFileService.LoadTransactionsAsync(inputStream);
        await Model.LoadTransactionsAndCategoriseAsync(result);
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

        var fileBytes = await _excelFileService.CategoriseTransactionsAsync(new CategoriseRequest(transactions.Value, month.Value));
        var result = await _OnFileResultDownloadBytes(result: fileBytes,
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