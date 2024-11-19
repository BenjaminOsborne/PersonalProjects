using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Immutable;

namespace AccountProcessor.Components.Pages;

public static class SelectorConstants
{
    /// <summary> Stable Id for the "Choose Category" default option when creating a new Section </summary>
    public static readonly string ChooseCategoryDefaultId = Guid.NewGuid().ToString();

    /// <summary> Stable Id for the "Choose Section" default option when matching a row </summary>
    public static readonly string ChooseSectionDefaultId = Guid.NewGuid().ToString();
}

public partial class Home
{
    [Inject]
    private IExcelFileHandler _excellFileHandler { get; init; }
    [Inject]
    private ITransactionCategoriser _categoriser { get; init; }

    private HomeViewModel Model;

    protected override Task OnInitializedAsync()
    {
        Model = new HomeViewModel(_categoriser);
        Model.Initialise();
        return Task.CompletedTask;
    }

    private bool TransactionsAreFullyLoaded() =>
        Model.Categories.HasValue && Model.AllSections.HasValue && Model.TransactionResultViewModel != null;

    private async Task ExportCategorisedTransactions()
    {
        var categorisationResult = Model.GetLatestCategorisationResult();
        if (categorisationResult == null)
        {
            return;
        }

        var result = await _excellFileHandler.ExportCategorisedTransactionsToExcel(categorisationResult!);
        Model.UpdateLastActionResult(result);
        if (!result.IsSuccess)
        {
            return;
        }
        await _DownloadBytes(
            fileName: $"CategorisedTransactions_{Model.Month:yyyy-MM}.xlsx",
            bytes: result.Result!);
    }

    private Task LoadTransactionsAndCategorise(InputFileChangeEventArgs e) =>
        Model.LoadTransactionsAndCategorise(fnLoad: async () =>
        {
            using var inputStream = await e.CopyToMemoryStreamAsync();
            return await _excellFileHandler.LoadTransactionsFromExcel(inputStream);
        });

    private Task CoopBankReverseFile(InputFileChangeEventArgs e) =>
        _ProcessBankFileExtraction(e,
            fnProcess: s => _excellFileHandler.CoopBank_ExtractCsvTransactionsToExcel(s),
            filePrefix: "CoopBank_Extract");

    private Task SantanderProcessFile(InputFileChangeEventArgs e) =>
        _ProcessBankFileExtraction(e,
            fnProcess: s => _excellFileHandler.Santander_ExtractExcelTransactionsToExcel(s),
            filePrefix: "Santander_Extract");

    private async Task _ProcessBankFileExtraction(
        InputFileChangeEventArgs e,
        Func<Stream, Task<WrappedResult<byte[]>>> fnProcess,
        string filePrefix)
    {
        using var inputStream = await e.CopyToMemoryStreamAsync();
        var result = await fnProcess(inputStream);
        Model.UpdateLastActionResult(result);
        if (!result.IsSuccess)
        {
            return;
        }
        await _DownloadBytes(
            fileName: $"{filePrefix}_{e.File.Name}.xlsx",
            bytes: result.Result!);
    }

    private async Task _DownloadBytes(string fileName, byte[] bytes) =>
        await JS.InvokeAsync<object>(
            "jsSaveAsFile",
            args: [fileName, Convert.ToBase64String(bytes)]);
}

public class HomeViewModel
{
    private readonly ITransactionCategoriser Categoriser;
    private readonly TransactionsModel _transactionsModel;

    public HomeViewModel(ITransactionCategoriser categoriser)
    {
        Categoriser = categoriser;
        _transactionsModel = new TransactionsModel();
    }

    /// <summary> Display message after any action invoked </summary>
    public Result? LastActionResult { get; private set; }

    /// <remarks> Initial Id should be the "Choose Category" option </remarks>
    public string? NewSectionCategoryName { get; private set; } = SelectorConstants.ChooseCategoryDefaultId;
    public string? NewSectionName { get; private set; }

    public DateOnly Month => _transactionsModel.Month;
    public ImmutableArray<CategoryHeader>? Categories => _transactionsModel.Categories;
    public ImmutableArray<SectionSelectorRow>? AllSections => _transactionsModel.AllSections;
    public TransactionResultViewModel? TransactionResultViewModel => _transactionsModel.TransactionResultViewModel;

    private class TransactionsModel
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

    public CategorisationResult? GetLatestCategorisationResult() => _transactionsModel.LatestCategorisationResult;

    public void Initialise() =>
        _SetMonth(_InitialiseMonth());

    public void UpdateLastActionResult(Result result) =>
        _UpdateLastActionResult(result);

    public void SetNewSectionCategory(string? category) =>
        NewSectionCategoryName = category;

    public void SetNewSectionName(string? name) =>
        NewSectionName = name;

    public void CreateNewSection()
    {
        var category = _transactionsModel.Categories?.SingleOrDefault(x => x.Name == NewSectionCategoryName);
        if (category == null || NewSectionName.IsNullOrWhiteSpace())
        {
            return;
        }
        Categoriser.AddSection(category, NewSectionName!, matchMonthOnly: _transactionsModel.Month);

        _RefreshCategoriesAndMatchedTransactions();

        NewSectionCategoryName = SelectorConstants.ChooseCategoryDefaultId;
        NewSectionName = null;
    }

    public void OnSetMonth(string? yearAndMonth)
    {
        if (DateOnly.TryParseExact(yearAndMonth, "yyyy-MM", out var parsed))
        {
            _SetMonth(parsed);
        }
    }

    public void SkipMonth(int months) =>
        _SetMonth(_transactionsModel.Month.AddMonths(months));

    public void AddNewMatchForRow(TransactionRowUnMatched row)
    {
        var found = _transactionsModel.AllSections?.SingleOrDefault(x => x.Id == row.SelectionId);
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

        _ReRunTransactionMatching();
    }

    public void ClearMatch(TransactionRowMatched row)
    {
        if (row.Section == null || row.LatestMatch == null)
        {
            _UpdateLastActionResult("Empty section or empty matches");
            return;
        }

        var result = Categoriser.DeleteMatch(row.Section!, row.LatestMatch!);
        _UpdateLastActionResult(result);
        _ReRunTransactionMatching();
    }

    public async Task LoadTransactionsAndCategorise(Func<Task<WrappedResult<ImmutableArray<Transaction>>>> fnLoad)
    {
        _transactionsModel.ClearTransactionsAndCategorisations();
        
        var transactionResult = await fnLoad();
        _UpdateLastActionResult(transactionResult);
        if (!transactionResult.IsSuccess)
        {
            return;
        }
        _transactionsModel.UpdateLoadedTransactions(transactionResult.Result);
        _ReRunTransactionMatching();
    }

    private static DateOnly _InitialiseMonth()
    {
        var now = DateTime.Now;
        return new DateOnly(now.Year, now.Month, 1).AddMonths(-1);
    }

    private void _SetMonth(DateOnly month)
    {
        _transactionsModel.Month = month;
        _RefreshCategoriesAndMatchedTransactions();
    }

    private void _RefreshCategoriesAndMatchedTransactions()
    {
        _transactionsModel.RefreshCategories(Categoriser.GetSelectorData(_transactionsModel.Month));
        _ReRunTransactionMatching();
    }

    private void _ReRunTransactionMatching()
    {
        if (_transactionsModel.LoadedTransactions.HasValue == false || _transactionsModel.AllSections.HasValue == false)
        {
            return;
        }
        var result = Categoriser.Categorise(_transactionsModel.LoadedTransactions!.Value, month: _transactionsModel.Month);
        _transactionsModel.UpdateFromCategorisationResult(result);
    }

    private void _UpdateLastActionResult(string error) =>
        LastActionResult = Result.Fail(error);

    private void _UpdateLastActionResult(Result result) =>
        LastActionResult = result;
}

public static class HomeExstensions
{
    /// <summary>
    ///  Must copy to memory stream as otherwise can raise:
    ///  "System.NotSupportedException: Synchronous reads are not supported."
    ///  when passing to service
    ///  </summary>
    public static async Task<MemoryStream> CopyToMemoryStreamAsync(this InputFileChangeEventArgs e)
    {
        var inputStream = new MemoryStream();
        var stream = e.File.OpenReadStream();
        await stream.CopyToAsync(inputStream);
        inputStream.Position = 0;
        return inputStream;
    }
}
