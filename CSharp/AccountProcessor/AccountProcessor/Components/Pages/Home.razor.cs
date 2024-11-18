using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Immutable;

namespace AccountProcessor.Components.Pages;

public static class SelectorConstants
{
    public static readonly string ChooseSectionId = Guid.NewGuid().ToString();
}

public partial class Home
{
    /// <summary> Display message after any action invoked </summary>
    private Result? LastActionResult;

    private StateModel Model;

    private class StateModel
    {
        public DateOnly Month;

        public ImmutableArray<CategoryHeader>? Categories { get; private set; }
        public ImmutableArray<SectionSelectorRow>? AllSections { get; private set; }

        public ImmutableArray<Transaction>? LoadedTransactions { get; private set; }

        public CategorisationResult? LatestCategorisationResult { get; private set; }
        public TransactionResultViewModel? TransactionResultViewModel { get; private set; }

        public void RefreshCategories(SelectorData allData)
        {
            Categories = allData.Categories;
            AllSections = allData.Sections
                .ToImmutableArray(s => new SectionSelectorRow(s, _ToDisplay(s), Guid.NewGuid().ToString())); //Arbitrary Id

            static string _ToDisplay(SectionHeader s) => $"{s.Parent.Name}: {s.Name}";
        }

        public void ClearTransactionsAndCategorisations()
        {
            LoadedTransactions = null;
            LatestCategorisationResult = null;
            TransactionResultViewModel = null;
        }

        internal void UpdateLoadedTransactions(ImmutableArray<Transaction> result) =>
            LoadedTransactions = result;

        public void UpdateFromCategorisationResult(CategorisationResult categorisationResult)
        {
            LatestCategorisationResult = categorisationResult;
            TransactionResultViewModel = TransactionResultViewModel.CreateFromResult(categorisationResult, AllSections!.Value);
        }
    }

    private string? NewSectionCategoryName;
    private string? NewSectionName;
    
    protected override Task OnInitializedAsync()
    {
        Model = new StateModel { Month = _InitialiseMonth() };
        _RefreshCategories();

        return Task.CompletedTask;
    }

    private bool TransactionsFullyLoaded() =>
        Model.Categories.HasValue && Model.AllSections.HasValue && Model.TransactionResultViewModel != null;

    private static DateOnly _InitialiseMonth()
    {
        var now = DateTime.Now;
        return new DateOnly(now.Year, now.Month, 1).AddMonths(-1);
    }

    private void _RefreshCategories() =>
        Model.RefreshCategories(Categoriser.GetSelectorData(Model.Month));

    private void OnSetMonth(string? yearAndMonth)
    {
        if (DateOnly.TryParseExact(yearAndMonth, "yyyy-MM", out var parsed))
        {
            _SetMonth(parsed);
        }
    }

    private void SkipMonth(int months) =>
        _SetMonth(Model.Month.AddMonths(months));

    private void _SetMonth(DateOnly month)
    {
        Model.Month = month;
        _RefreshCategoriesAndMatchedTransactions();
    }

    private void _RefreshCategoriesAndMatchedTransactions()
    {
        _RefreshCategories();
        _RefreshMatchedTransactions();
    }

    private void CreateNewSection()
    {
        var category = Model.Categories?.SingleOrDefault(x => x.Name == NewSectionCategoryName);
        if (category == null || NewSectionName.IsNullOrWhiteSpace())
        {
            return;
        }
        Categoriser.AddSection(category, NewSectionName!, matchMonthOnly: Model.Month);

        _RefreshCategoriesAndMatchedTransactions();

        NewSectionCategoryName = null;
        NewSectionName = null;
    }

    private async void AddNewMatchForRow(TransactionRowUnMatched row)
    {
        var found = Model.AllSections?.SingleOrDefault(x => x.Id == row.SelectionId);
        var header = found?.Header;
        if (header == null)
        {
            _UpdateLastActionResult("Could not find Section");
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
                _UpdateLastActionResult("Match On must be defined");
                return;
            }
            result = Categoriser.ApplyMatch(row.Transaction, header!, row.MatchOn!, row.OverrideDescription);
        }
        _UpdateLastActionResult(result);
        if (!result.IsSuccess)
        {
            return;
        }

        _RefreshMatchedTransactions();
    }

    private void ClearMatch(TransactionRowMatched row)
    {
        if (row.Section == null || row.LatestMatch == null)
        {
            _UpdateLastActionResult("Empty section or empty matches");
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
        if (Model.LatestCategorisationResult == null)
        {
            return;
        }

        var result = await ExcelFileHandler.ExportCategorisedTransactionsToExcel(Model.LatestCategorisationResult!);
        _UpdateLastActionResult(result);
        if (!result.IsSuccess)
        {
            return;
        }
        await _DownloadBytes(
            fileName: $"CategorisedTransactions_{Model.Month:yyyy-MM}.xlsx",
            bytes: result.Result!);
    }

    private async Task _DownloadBytes(string fileName, byte[] bytes) =>
        await JS.InvokeAsync<object>(
            "jsSaveAsFile",
            args: [fileName, Convert.ToBase64String(bytes)]);

    private async Task LoadTransactionsAndCategorise(InputFileChangeEventArgs e)
    {
        Model.ClearTransactionsAndCategorisations();
        
        using var inputStream = await _CopyToMemoryStreamAsync(e);
        var transactionResult = await ExcelFileHandler.LoadTransactionsFromExcel(inputStream);
        _UpdateLastActionResult(transactionResult);
        if (!transactionResult.IsSuccess)
        {
            return;
        }
        Model.UpdateLoadedTransactions(transactionResult.Result);
        _RefreshMatchedTransactions();
    }

    private void _RefreshMatchedTransactions()
    {
        if (Model.LoadedTransactions.HasValue == false || Model.AllSections.HasValue == false)
        {
            return;
        }
        var result = Categoriser.Categorise(Model.LoadedTransactions!.Value, month: Model.Month);
        Model.UpdateFromCategorisationResult(result);
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

    private void _UpdateLastActionResult(string error) =>
        LastActionResult = Result.Fail(error);

    private void _UpdateLastActionResult(Result result) =>
        LastActionResult = result;
}
