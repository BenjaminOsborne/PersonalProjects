using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Immutable;

namespace AccountProcessor.Components.Pages;

public partial class Home
{
    /// <summary> Display message after any action invoked </summary>
    private string? LastActionResult;

    private DateOnly? Month;

    private ImmutableArray<CategoryHeader>? Categories;
    private ImmutableArray<SectionSelectorRow>? AllSections;

    private string? NewSectionCategoryName;
    private string? NewSectionName;

    private ImmutableArray<Transaction>? LoadedTransactions;

    private CategorisationResult? LatestCategorisationResult;
    private TransactionResultViewModel? TransactionResultViewModel;

    protected override Task OnInitializedAsync()
    {
        Month = _InitialiseMonth();
        _RefreshCategories();

        return Task.CompletedTask;
    }

    private bool TransactionsFullyLoaded() =>
        Categories.HasValue && AllSections.HasValue && TransactionResultViewModel != null;

    private static DateOnly _InitialiseMonth()
    {
        var now = DateTime.Now;
        return new DateOnly(now.Year, now.Month, 1).AddMonths(-1);
    }

    private void _RefreshCategories()
    {
        if (Month.HasValue == false)
        {
            return;
        }
        var allData = Categoriser.GetSelectorData(Month!.Value);
        Categories = allData.Categories;
        AllSections = allData.Sections
            .ToImmutableArray(s => new SectionSelectorRow(s, _ToDisplay(s), Guid.NewGuid().ToString())); //Arbitrary Id

        static string _ToDisplay(SectionHeader s) => $"{s.Parent.Name}: {s.Name}";
    }

    private void OnSetMonth(string? yearAndMonth)
    {
        if (DateOnly.TryParseExact(yearAndMonth, "yyyy-MM", out var parsed))
        {
            _SetMonth(parsed);
        }
    }

    private void SkipMonth(int months) =>
        _SetMonth(Month?.AddMonths(months) ?? _InitialiseMonth());

    private void _SetMonth(DateOnly month)
    {
        Month = month;
        _RefreshCategoriesAndMatchedTransactions();
    }

    private void _RefreshCategoriesAndMatchedTransactions()
    {
        _RefreshCategories();
        _RefreshMatchedTransactions();
    }

    private void CreateNewSection()
    {
        var category = Categories?.SingleOrDefault(x => x.Name == NewSectionCategoryName);
        if (category == null || NewSectionName.IsNullOrWhiteSpace())
        {
            return;
        }
        Categoriser.AddSection(category, NewSectionName!, matchMonthOnly: Month);

        _RefreshCategoriesAndMatchedTransactions();

        NewSectionCategoryName = null;
        NewSectionName = null;
    }

    private async Task PerformMatch(TransactionRowUnMatched row)
    {
        var found = AllSections?.SingleOrDefault(x => x.Id == row.SelectionId);
        var header = found?.Header;
        if (header == null)
        {
            LastActionResult = "Could not find Section";
            return;
        }

        Result result;
        if (row.AddOnlyForTransaction)
        {
            result = Categoriser.MatchOnce(row.Transaction, header!, row.MatchOn, row.OverrideDescription);
        }
        else
        {
            if (row.MatchOn.IsNullOrEmpty())
            {
                LastActionResult = "Match On must be defined";
                return;
            }
            result = Categoriser.ApplyMatch(row.Transaction, header!, row.MatchOn!, row.OverrideDescription);
        }
        _UpdateLastActionResult(result);
        if (!result.IsSuccess)
        {
            return;
        }

        //Triggers total table refresh - task yield required to enable re-render
        LatestCategorisationResult = null;
        TransactionResultViewModel = null;

        StateHasChanged();
        await Task.Yield();

        _RefreshMatchedTransactions();
    }

    private void ClearMatch(TransactionRowMatched row)
    {
        if (row.Section == null || row.LatestMatch == null)
        {
            LastActionResult = "Empty section or empty matches";
            return;
        }

        var result = Categoriser.DeleteMatch(row.Section!, row.LatestMatch!);
        _UpdateLastActionResult(result);
        _RefreshMatchedTransactions();
    }

    private Task CoopBankReverseFile(InputFileChangeEventArgs e) =>
        _ProcessBankFileExtraction(e,
            fnProcess: s => ExcelFileHandler.CoopBank_ExtractCsvTransactionsToExcel(s),
            filePrefix: "CoopBank_Extract");

    private Task SantanderProcessFile(InputFileChangeEventArgs e) =>
        _ProcessBankFileExtraction(e,
            fnProcess: s => ExcelFileHandler.Santander_ExtractExcelTransactionsToExcel(s),
            filePrefix: "Santander_Extract");

    private async Task _ProcessBankFileExtraction(
    InputFileChangeEventArgs e,
        Func<Stream, Task<WrappedResult<byte[]>>> fnProcess,
        string filePrefix)
    {
        LastActionResult = "Running...";

        using var inputStream = await _CopyToMemoryStreamAsync(e);
        var result = await fnProcess(inputStream);
        _UpdateLastActionResult(result);
        if (!result.IsSuccess)
        {
            return;
        }
        await _DownloadBytes(
            fileName: $"{filePrefix}_{e.File.Name}.xlsx",
            bytes: result.Result!);
    }

    private async Task ExportCategorisedTransactions()
    {
        if (LatestCategorisationResult == null)
        {
            return;
        }

        var result = await ExcelFileHandler.ExportCategorisedTransactionsToExcel(LatestCategorisationResult!);
        _UpdateLastActionResult(result);
        if (!result.IsSuccess)
        {
            return;
        }
        await _DownloadBytes(
            fileName: $"CategorisedTransactions_{Month:yyyy-MM}.xlsx",
            bytes: result.Result!);
    }

    private async Task _DownloadBytes(string fileName, byte[] bytes) =>
        await JS.InvokeAsync<object>(
            "jsSaveAsFile",
            args: [fileName, Convert.ToBase64String(bytes)]);

    private async Task Categorise(InputFileChangeEventArgs e)
    {
        LoadedTransactions = null;
        LatestCategorisationResult = null;
        TransactionResultViewModel = null;

        using var inputStream = await _CopyToMemoryStreamAsync(e);
        var transactionResult = await ExcelFileHandler.LoadTransactionsFromExcel(inputStream);
        _UpdateLastActionResult(transactionResult);
        if (!transactionResult.IsSuccess)
        {
            return;
        }
        LoadedTransactions = transactionResult.Result;
        _RefreshMatchedTransactions();
    }

    private void _RefreshMatchedTransactions()
    {
        if (Month.HasValue == false || LoadedTransactions.HasValue == false || AllSections.HasValue == false)
        {
            return;
        }
        var categorisationResult = Categoriser.Categorise(LoadedTransactions!.Value, month: Month!.Value);
        LatestCategorisationResult = categorisationResult;
        TransactionResultViewModel = TransactionResultViewModel.CreateFromResult(categorisationResult, AllSections!.Value);
    }

    /// <summary>
    ///  Must copy to memory stream as otherwise can raise:
    ///  "System.NotSupportedException: Synchronous reads are not supported."
    ///  when passing to service
    ///  </summary>
    private async Task<MemoryStream> _CopyToMemoryStreamAsync(InputFileChangeEventArgs e)
    {
        var inputStream = new MemoryStream();
        var stream = e.File.OpenReadStream();
        await stream.CopyToAsync(inputStream);
        inputStream.Position = 0;
        return inputStream;
    }

    private void _UpdateLastActionResult(Result result) =>
        LastActionResult = result.IsSuccess
            ? "Success!"
            : $"Failed: {result.Error}";
}
