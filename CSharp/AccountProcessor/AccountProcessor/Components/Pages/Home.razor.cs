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

public static class FileConstants
{
    public const string ExtractedTransactionsFileExtension = ".extract.xlsx";
}

/// <summary> Code-behind for Home.razor - gives logical split between html-render code and c# data processing </summary>
public partial class Home
{
    [Inject]
    private IExcelFileHandler _excelFileHandler { get; init; }
    [Inject]
    private ITransactionCategoriserScoped _categoriser { get; init; }
    [Inject]
    private Microsoft.JSInterop.IJSRuntime _jsInterop { get; init; }

    private HomeViewModel Model;

    private UnMatchedRowsTable? UnMatchedRowsTable;
    private MatchedRowsTable? MatchedRowsTable;

    protected override Task OnInitializedAsync()
    {
        Model = new HomeViewModel(_categoriser,
            onStateChanged: _OnModelStateChangeRebuildChildren);
        return Task.CompletedTask;
    }

    private bool IsModelLocationKnown() =>
        _categoriser.GetScope().IsModelLocationKnown();

    private bool TransactionsAreFullyLoaded() =>
        Model.Categories.HasValue && Model.AllSections.HasValue && Model.TransactionResultViewModel != null;

    public void OnAccountFileConverted(Result result)
    {
        Model.OnFileExtractResult(result);
        StateHasChanged(); //Child view triggers change to Result label to be re-rendered
    }

    private Task LoadTransactionsAndCategorise(InputFileChangeEventArgs e) =>
        Model.LoadTransactionsAndCategorise(fnLoad: async () =>
        {
            using var inputStream = await e.CopyToMemoryStreamAsync();
            return await _excelFileHandler.LoadTransactionsFromExcel(inputStream);
        });

    private async Task ExportCategorisedTransactions()
    {
        var categorisationResult = Model.GetLatestCategorisationResultForExport();
        if (categorisationResult == null)
        {
            return;
        }
        await _OnFileResultDownloadBytes(
            result: await _excelFileHandler.ExportCategorisedTransactionsToExcel(categorisationResult!),
            fileName: $"CategorisedTransactions_{Model.Month:yyyy-MM}.xlsx");
    }

    private void AddNewMatchForRow(TransactionRowUnMatched row)
    {
        Model.AddNewMatchForRow(row);
        StateHasChanged(); //Must call: child component can trigger update to error state.
    }

    private void ClearMatch(TransactionRowMatched row)
    {
        Model.ClearMatch(row);
        StateHasChanged(); //Must call: child component can trigger update to error state.
    }

    private Task _OnFileResultDownloadBytes(WrappedResult<byte[]> result, string fileName)
    {
        Model.OnFileExtractResult(result);
        return result.IsSuccess
            ? _jsInterop.SaveAsFileAsync(fileName, result.Result!)
            : Task.CompletedTask;
    }

    private void _OnModelStateChangeRebuildChildren()
    {
        var unMatched = Model.UnMatchedModel;
        if (unMatched != null)
        {
            UnMatchedRowsTable?.UpdateModel(unMatched); //If class null, will be initialised correctly when view first renders
        }
        else
        {
            UnMatchedRowsTable = null;
        }

        var matched = Model.MatchedModel;
        if (matched != null)
        {
            MatchedRowsTable?.UpdateModel(matched); //If class null, will be initialised correctly when view first renders
        }
        else
        {
            MatchedRowsTable = null;
        }
    }
}

/// <summary> Separate class from <see cref="Home"/> so that state transitions & data access can be controlled explicitly </summary>
public class HomeViewModel
{
    private static readonly (string? CategoryName, string? Name, bool IsMonthSpecific) _newSectionDefault = (SelectorConstants.ChooseCategoryDefaultId, null, true);

    private readonly TransactionsModel _transactionsModel;

    public HomeViewModel(ITransactionCategoriserScoped categoriser, Action onStateChanged) =>
        _transactionsModel = new(
            categoriser,
            onActionHandleResult: _UpdateLastActionResult,
            onStateChanged);

    /// <summary> Display message after any action invoked </summary>
    public Result? LastActionResult { get; private set; }

    /// <remarks> Initial Id should be the "Choose Category" option </remarks>
    public (string? CategoryName, string? Name, bool IsMonthSpecific) NewSection { get; private set; } = _newSectionDefault;

    public DateOnly Month => _transactionsModel.Month;
    public DateOnly? EarliestTransaction => _transactionsModel.EarliestTransaction;
    public DateOnly? LatestTransaction => _transactionsModel.LatestTransaction;
    public ImmutableArray<CategoryHeader>? Categories => _transactionsModel.Categories;
    public ImmutableArray<SectionSelectorRow>? AllSections => _transactionsModel.AllSections;
    public TransactionResultViewModel? TransactionResultViewModel => _transactionsModel.TransactionResultViewModel;
    
    public UnMatchedRowsTable.ViewModel? UnMatchedModel => _transactionsModel.UnMatchedModel;
    public MatchedRowsTable.ViewModel? MatchedModel => _transactionsModel.MatchedModel;

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
        NewSection = NewSection with { CategoryName = category };

    public void SetNewSectionName(string? name) =>
        NewSection = NewSection with { Name = name };

    public void SetNewSectionIsMonthSpecific(bool isMonthSpecific) =>
        NewSection = NewSection with { IsMonthSpecific = isMonthSpecific };

    public void CreateNewSection() =>
        _transactionsModel.ChangeMatchModel(
            fnPerform: cat =>
            {
                var category = _transactionsModel.Categories?.SingleOrDefault(x => x.Name == NewSection.CategoryName);
                return category != null && !NewSection.Name.IsNullOrWhiteSpace()
                    ? cat.AddSection(category, NewSection.Name!,
                        matchMonthOnly: NewSection.IsMonthSpecific ? _transactionsModel.Month : null)
                    : Result.Fail("Invalid Category or empty Section Name");
            },
            refreshCategories: true,
            onSuccess: () => NewSection = _newSectionDefault);

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
        private readonly ITransactionCategoriserScoped _categoriser;
        private readonly Action<Result> _onActionHandleResult;
        private readonly Action _onStateChanged;

        public TransactionsModel(ITransactionCategoriserScoped categoriser,
            Action<Result> onActionHandleResult,
            Action onStateChanged)
        {
            _categoriser = categoriser;
            _onActionHandleResult = onActionHandleResult;
            _onStateChanged = onStateChanged;

            var now = DateTime.Now;
            Month = new DateOnly(now.Year, now.Month, 1).AddMonths(-1); //Initialise to last complete month (i.e. month before the current one)
        }

        public DateOnly Month { get; private set; }

        public ImmutableArray<CategoryHeader>? Categories { get; private set; }
        public ImmutableArray<SectionSelectorRow>? AllSections { get; private set; }

        public ImmutableArray<Transaction>? LoadedTransactions { get; private set; }
        public DateOnly? EarliestTransaction { get; private set; }
        public DateOnly? LatestTransaction { get; private set; }

        public CategorisationResult? LatestCategorisationResult { get; private set; }
        public TransactionResultViewModel? TransactionResultViewModel { get; private set; }

        public UnMatchedRowsTable.ViewModel? UnMatchedModel { get; private set; }
        public MatchedRowsTable.ViewModel? MatchedModel { get; private set; }

        public void UpdateMonth(WrappedResult<DateOnly> result) =>
            _OnStateChange(
                fnGetResult: () => result,
                onSuccess: r => Month = r,
                refreshCategories: true //categories can be month-specific so must refresh
                );

        public void UpdateLoadedTransactions(WrappedResult<ImmutableArray<Transaction>> result) =>
            _OnStateChange(
                fnGetResult: () => result,
                onSuccess: r =>
                {
                    LoadedTransactions = r;
                    EarliestTransaction = r.Any() ? r.Select(x => x.Date).Min() : null;
                    LatestTransaction = r.Any() ? r.Select(x => x.Date).Max() : null;
                },
                refreshCategories: true //Categories loaded
                );

        public void ChangeMatchModel(Func<ITransactionCategoriser, Result> fnPerform,
            Action? onSuccess = null,
            bool refreshCategories = false) =>
                _OnStateChange(
                    fnGetResult: () => fnPerform(_GetCategoriser()).ToWrappedUnit(),
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
                var allData = _GetCategoriser().GetSelectorData(Month);
                Categories = allData.Categories;
                AllSections = allData.Sections?.ToImmutableArray(secHead =>
                    new SectionSelectorRow(secHead, Display: $"{secHead.Parent.Name}: {secHead.Name}", Id: Guid.NewGuid().ToString())); //Arbitrary Id
            }

            //Always try refresh (if Transactions and Sections loaded)
            var loadedTransactions = LoadedTransactions;
            var allSections = AllSections;
            if (loadedTransactions.HasValue == false || allSections.HasValue == false)
            {
                return;
            }
            var categorisationResult = _GetCategoriser().Categorise(loadedTransactions!.Value, Month);
            LatestCategorisationResult = categorisationResult;
            var trViewModel = TransactionResultViewModel.CreateFromResult(categorisationResult, allSections!.Value);
            TransactionResultViewModel = trViewModel;

            UnMatchedModel = trViewModel.UnMatchedRows.Any() ? new(allSections!.Value, trViewModel.UnMatchedRows) : null;
            MatchedModel = trViewModel.MatchedRows.Any() ? new(trViewModel.MatchedRows) : null;

            _onStateChanged();
        }

        private ITransactionCategoriser _GetCategoriser() => _categoriser.GetScope();
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

    public static async Task SaveAsFileAsync(this Microsoft.JSInterop.IJSRuntime jsInterop,
        string fileName,
        byte[] file) =>
        await jsInterop.InvokeAsync<object>(
            "jsSaveAsFile",
            args: [fileName, Convert.ToBase64String(file)]);
}
