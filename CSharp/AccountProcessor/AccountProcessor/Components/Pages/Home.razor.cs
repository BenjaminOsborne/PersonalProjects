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

/// <summary> Code-behind for Home.razor - gives logical split between html-render code and c# data processing </summary>
public partial class Home
{
    [Inject]
    private IExcelFileHandler _excelFileHandler { get; init; }
    [Inject]
    private ITransactionCategoriser _categoriser { get; init; }
    [Inject]
    private Microsoft.JSInterop.IJSRuntime _jsInterop { get; init; }

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
        var categorisationResult = Model.GetLatestCategorisationResultForExport();
        if (categorisationResult == null)
        {
            return;
        }

        var result = await _excelFileHandler.ExportCategorisedTransactionsToExcel(categorisationResult!);
        Model.OnFileExtractResult(result);
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
            return await _excelFileHandler.LoadTransactionsFromExcel(inputStream);
        });

    private Task CoopBankReverseFile(InputFileChangeEventArgs e) =>
        _ProcessBankFileExtraction(e,
            fnProcess: s => _excelFileHandler.CoopBank_ExtractCsvTransactionsToExcel(s),
            filePrefix: "CoopBank_Extract");

    private Task SantanderProcessFile(InputFileChangeEventArgs e) =>
        _ProcessBankFileExtraction(e,
            fnProcess: s => _excelFileHandler.Santander_ExtractExcelTransactionsToExcel(s),
            filePrefix: "Santander_Extract");

    private async Task _ProcessBankFileExtraction(
        InputFileChangeEventArgs e,
        Func<Stream, Task<WrappedResult<byte[]>>> fnProcess,
        string filePrefix)
    {
        using var inputStream = await e.CopyToMemoryStreamAsync();
        var result = await fnProcess(inputStream);
        Model.OnFileExtractResult(result);
        if (!result.IsSuccess)
        {
            return;
        }
        await _DownloadBytes(
            fileName: $"{filePrefix}_{e.File.Name}.xlsx",
            bytes: result.Result!);
    }

    private async Task _DownloadBytes(string fileName, byte[] bytes) =>
        await _jsInterop.InvokeAsync<object>(
            "jsSaveAsFile",
            args: [fileName, Convert.ToBase64String(bytes)]);
}

/// <summary> Separate class from <see cref="Home"/> so that state transitions & data access can be controlled explicitly </summary>
public class HomeViewModel
{
    private readonly TransactionsModel _transactionsModel;

    public HomeViewModel(ITransactionCategoriser categoriser) =>
        _transactionsModel = new(
            categoriser,
            onActionHandleResult: _UpdateLastActionResult);

    /// <summary> Display message after any action invoked </summary>
    public Result? LastActionResult { get; private set; }

    /// <remarks> Initial Id should be the "Choose Category" option </remarks>
    public string? NewSectionCategoryName { get; private set; } = SelectorConstants.ChooseCategoryDefaultId;
    public string? NewSectionName { get; private set; }

    public DateOnly Month => _transactionsModel.Month;
    public ImmutableArray<CategoryHeader>? Categories => _transactionsModel.Categories;
    public ImmutableArray<SectionSelectorRow>? AllSections => _transactionsModel.AllSections;
    public TransactionResultViewModel? TransactionResultViewModel => _transactionsModel.TransactionResultViewModel;

    public void Initialise()
    {
        var now = DateTime.Now;
        _transactionsModel.UpdateMonth(WrappedResult.Create(new DateOnly(now.Year, now.Month, 1).AddMonths(-1)));
    }

    public CategorisationResult? GetLatestCategorisationResultForExport() => _transactionsModel.LatestCategorisationResult;

    /// <summary> Only actions not managed by this model are the Excel file extracts - this method enables result to display </summary>
    public void OnFileExtractResult(Result result) =>
        _UpdateLastActionResult(result);

    public void SetMonth(string? yearAndMonth) =>
        _transactionsModel.UpdateMonth(
            DateOnly.TryParseExact(yearAndMonth, "yyyy-MM", out var parsed)
                ? WrappedResult.Create(parsed)
                : WrappedResult.Fail<DateOnly>($"Invalid date format: {yearAndMonth}"));

    public void SkipMonth(int months) =>
        _transactionsModel.UpdateMonth(WrappedResult.Create(_transactionsModel.Month.AddMonths(months)));

    public void SetNewSectionCategory(string? category) =>
        NewSectionCategoryName = category;

    public void SetNewSectionName(string? name) =>
        NewSectionName = name;

    public void CreateNewSection() =>
        _transactionsModel.ChangeMatchModel(
            fnPerform: cat =>
            {
                var category = _transactionsModel.Categories?.SingleOrDefault(x => x.Name == NewSectionCategoryName);
                return category != null && !NewSectionName.IsNullOrWhiteSpace()
                    ? cat.AddSection(category, NewSectionName!, matchMonthOnly: _transactionsModel.Month)
                    : Result.Fail("Invalid Category or empty Section Name");
            },
            refreshCategories: true,
            onSuccess: () =>
            {
                NewSectionCategoryName = SelectorConstants.ChooseCategoryDefaultId;
                NewSectionName = null;
            });

    public void AddNewMatchForRow(TransactionRowUnMatched row) =>
        _transactionsModel.ChangeMatchModel(
            fnPerform: cat =>
            {
                var header = _transactionsModel.AllSections
                    ?.SingleOrDefault(x => x.Id == row.SelectionId)
                    ?.Header;
                return header != null
                    ? row.AddOnlyForTransaction
                        ? cat.MatchOnce(row.Transaction, header!, row.MatchOn, row.OverrideDescription)
                        : !row.MatchOn.IsNullOrEmpty()
                            ? cat.ApplyMatch(row.Transaction, header!, row.MatchOn!, row.OverrideDescription)
                            : Result.Fail("'Match On' pattern empty")
                    : Result.Fail("Could not find Section");
            });

    public void ClearMatch(TransactionRowMatched row) =>
        _transactionsModel.ChangeMatchModel(
            fnPerform: c => c.DeleteMatch(row.Section, row.LatestMatch));

    public async Task LoadTransactionsAndCategorise(Func<Task<WrappedResult<ImmutableArray<Transaction>>>> fnLoad) =>
        _transactionsModel.UpdateLoadedTransactions(await fnLoad());

    private void _UpdateLastActionResult(Result result) =>
        LastActionResult = result;

    /// <summary> Class manages storage-of & changes-of state. Ensures transactions re-categorised when match-model changes etc. </summary>
    private class TransactionsModel
    {
        private readonly ITransactionCategoriser _categoriser;
        private readonly Action<Result> _onActionHandleResult;

        public TransactionsModel(ITransactionCategoriser categoriser,
            Action<Result> onActionHandleResult)
        {
            _categoriser = categoriser;
            _onActionHandleResult = onActionHandleResult;
        }

        public DateOnly Month { get; private set; }

        public ImmutableArray<CategoryHeader>? Categories { get; private set; }
        public ImmutableArray<SectionSelectorRow>? AllSections { get; private set; }

        public ImmutableArray<Transaction>? LoadedTransactions { get; private set; }

        public CategorisationResult? LatestCategorisationResult { get; private set; }
        public TransactionResultViewModel? TransactionResultViewModel { get; private set; }

        public void UpdateMonth(WrappedResult<DateOnly> result) =>
            _OnStateChange(
                fnGetResult: () => result,
                onSuccess: r => Month = r,
                refreshCategories: true //categories can be month-specific so must refresh
                );

        public void UpdateLoadedTransactions(WrappedResult<ImmutableArray<Transaction>> result) =>
            _OnStateChange(
                fnGetResult: () => result,
                onSuccess: r => LoadedTransactions = r,
                refreshCategories: false //No change to categories
                );

        public void ChangeMatchModel(Func<ITransactionCategoriser, Result> fnPerform,
            Action? onSuccess = null,
            bool refreshCategories = false) =>
                _OnStateChange(
                    fnGetResult: () => fnPerform(_categoriser).ToWrappedUnit(),
                    onSuccess: _ => onSuccess?.Invoke(),
                    refreshCategories: refreshCategories);

        private void _OnStateChange<T>(
            Func<WrappedResult<T>> fnGetResult,
            Action<T>? onSuccess = null,
            bool refreshCategories = false)
        {
            var result = fnGetResult();
            _onActionHandleResult(result);
            if (!result.IsSuccess)
            {
                return;
            }
            onSuccess?.Invoke(result.Result!);
            
            if (refreshCategories)
            {
                var allData = _categoriser.GetSelectorData(Month);
                Categories = allData.Categories;
                AllSections = allData.Sections
                    .ToImmutableArray(s => new SectionSelectorRow(s, ToDisplay(s), Guid.NewGuid().ToString())); //Arbitrary Id
                
                static string ToDisplay(SectionHeader s) => $"{s.Parent.Name}: {s.Name}";
            }

            //Always try refresh (if Transactions and Sections loaded)
            var loadedTransactions = LoadedTransactions;
            var allSections = AllSections;
            if (loadedTransactions.HasValue == false || allSections.HasValue == false)
            {
                return;
            }
            var categorisationResult = _categoriser.Categorise(loadedTransactions!.Value, month: Month);
            LatestCategorisationResult = categorisationResult;
            TransactionResultViewModel = TransactionResultViewModel.CreateFromResult(categorisationResult, allSections!.Value);
        }
    }
}

public static class HomeExstensions
{
    /// <summary>
    ///  Must copy to memory stream as otherwise consuming services can raise:
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
