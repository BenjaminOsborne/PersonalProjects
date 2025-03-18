using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Immutable;

namespace AccountProcessor.Components.Pages;

public static class FileConstants
{
    public const string ExtractedTransactionsFileExtension = ".extract.xlsx";
}

/// <summary> Code-behind for Home.razor - gives logical split between html-render code and c# data processing </summary>
public partial class Home
{
    [Inject]
    private ITransactionCategoriserScoped _categoriser { get; init; } = null!;

    private HomeViewModel Model = null!;

    private UnMatchedRowsTable? UnMatchedRowsTable;
    private MatchedRowsTable? MatchedRowsTable;

    private DateTime? YearMonthBind
    {
        get => Model.Month?.ToDateTime(TimeOnly.MinValue);
        set => Model.SetMonth(value);
    }

    public bool FileActionsExpandedBind { get; set; } = true;

    public void OnFileActionFinished((FileActionType type, Result result) actionParams)
    {
        if (actionParams.result.IsSuccess && actionParams.type == FileActionType.LoadTransactions)
        {
            FileActionsExpandedBind = false; //Close to give more screen space to categorisation
        }

        StateHasChanged();
    }

    protected override Task OnInitializedAsync()
    {
        Model = new HomeViewModel(_categoriser,
            onStateChanged: _OnModelStateChangeRebuildChildren);
        return Task.CompletedTask;
    }

    private bool TransactionsAreFullyLoaded() =>
        Model.TransactionsAreFullyLoaded();

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
    private static readonly (string? CategoryName, string? Name, bool IsMonthSpecific) _newSectionDefault = (null, null, true);

    private readonly TransactionsModel _transactionsModel;

    public HomeViewModel(ITransactionCategoriserScoped categoriser, Action onStateChanged) =>
        _transactionsModel = new(
            categoriser,
            onActionHandleResult: _UpdateLastActionResult,
            onStateChanged);

    /// <summary> Display message after any action invoked </summary>
    public Result? LastActionResult { get; private set; }

    public bool CanCategoriseTransactions() =>
        _transactionsModel.CanCategoriseTransactions();

    /// <remarks> Initial Id should be the "Choose Category" option </remarks>
    private (string? CategoryName, string? Name, bool IsMonthSpecific) _newSection = _newSectionDefault;

    public string? NewSectionName
    {
        get => _newSection.Name;
        set => _newSection = _newSection with { Name = value };
    }

    public string? NewSectionCategory
    {
        get => _newSection.CategoryName;
        set => _newSection = _newSection with { CategoryName = value };
    }

    public bool NewSectionIsMonthSpecific
    {
        get => _newSection.IsMonthSpecific;
        set => _newSection = _newSection with { IsMonthSpecific = value };
    }

    public bool TransactionsAreFullyLoaded() =>
        Month.HasValue && Categories.HasValue && AllSections.HasValue && TransactionResultViewModel != null;

    public DateOnly? Month => _transactionsModel.Month;
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

    public void RefreshTransactions() =>
        _transactionsModel.RefreshTransactions();

    public void SetMonth(DateTime? yearAndMonth) =>
        _transactionsModel.UpdateMonth(yearAndMonth.HasValue
            ? WrappedResult.Create(DateOnly.FromDateTime(yearAndMonth.Value))
            : WrappedResult.Fail<DateOnly>($"Invalid date format: {yearAndMonth}"));

    public void SkipMonth(int months)
    {
        var update = _transactionsModel.Month!.Value.AddMonths(months); //UI only shows "skip" if Month already loaded!
        _transactionsModel.UpdateMonth(WrappedResult.Create(update));
    }

    public void CreateNewSection() =>
        _transactionsModel.ChangeMatchModel(
            fnPerform: cat =>
            {
                var category = _transactionsModel.Categories?.SingleOrDefault(x => x.Name == _newSection.CategoryName);
                return category != null && !_newSection.Name.IsNullOrWhiteSpace()
                    ? cat.AddSection(category, _newSection.Name!,
                        matchMonthOnly: _newSection.IsMonthSpecific ? _transactionsModel.Month : null)
                    : Result.Fail("Invalid Category or empty Section Name");
            },
            refreshCategories: true,
            onSuccess: () => _newSection = _newSectionDefault);

    public void AddNewMatchForRow(TransactionRowUnMatched row) =>
        _transactionsModel.ChangeMatchModel(
            fnPerform: cat =>
            {
                var header = _transactionsModel.AllSections
                    ?.SingleOrDefault(x => x.Id == row.SelectionId)
                    ?.Header;
                return header != null
                    ? row.AddOnlyForTransaction
                        ? cat.MatchOnce(row.Transaction, header, row.MatchOn, row.OverrideDescription)
                        : !row.MatchOn.IsNullOrEmpty()
                            ? cat.ApplyMatch(row.Transaction, header, row.MatchOn!, row.OverrideDescription)
                            : Result.Fail("'Match On' pattern empty")
                    : Result.Fail("Could not find Section");
            },
            refreshCategories: true); //Should refresh categories as will update the order in the "suggestions" in the picker

    public void ClearMatch(TransactionRowMatched row) =>
        _transactionsModel.ChangeMatchModel(
            fnPerform: cat => cat.DeleteMatch(row.Section, row.LatestMatch),
            refreshCategories: false); //Clearing a match does not change categories (or "suggestions" in the picker)

    public void LoadTransactionsAndCategorise(WrappedResult<ImmutableArray<Transaction>> result) =>
        _transactionsModel.UpdateLoadedTransactions(result);

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
        }

        public bool CanCategoriseTransactions() =>
            _categoriser.GetScope().CanCategoriseTransactions();

        public DateOnly? Month { get; private set; }

        public ImmutableArray<CategoryHeader>? Categories { get; private set; }
        public ImmutableArray<SectionSelectorRow>? AllSections { get; private set; }

        public ImmutableArray<Transaction>? LoadedTransactions { get; private set; }
        public DateOnly? EarliestTransaction { get; private set; }
        public DateOnly? LatestTransaction { get; private set; }

        public CategorisationResult? LatestCategorisationResult { get; private set; }
        public TransactionResultViewModel? TransactionResultViewModel { get; private set; }

        public UnMatchedRowsTable.ViewModel? UnMatchedModel { get; private set; }
        public MatchedRowsTable.ViewModel? MatchedModel { get; private set; }

        public void RefreshTransactions() =>
            _OnStateChange(
                fnGetResult: () => WrappedResult.Create(Unit.Instance),
                refreshCategories: true);

        public void UpdateMonth(WrappedResult<DateOnly> result) =>
            _OnStateChange(
                fnGetResult: () => result,
                refreshCategories: true, //categories can be month-specific so must refresh
                onSuccess: r => Month = r);

        public void UpdateLoadedTransactions(WrappedResult<ImmutableArray<Transaction>> result) =>
            _OnStateChange(
                fnGetResult: () => result,
                refreshCategories: true, //Categories loaded
                onSuccess: r =>
                {
                    Month = _InitialiseOrUpdateMonthFromTransactions(Month, r);
                    LoadedTransactions = r;
                    EarliestTransaction = r.Any() ? r.Select(x => x.Date).Min() : null;
                    LatestTransaction = r.Any() ? r.Select(x => x.Date).Max() : null;
                });

        public void ChangeMatchModel(Func<ITransactionCategoriser, Result> fnPerform,
            bool refreshCategories,
            Action? onSuccess = null) =>
                _OnStateChange(
                    fnGetResult: () => fnPerform(_GetCategoriser()).ToWrappedUnit(),
                    refreshCategories: refreshCategories,
                    onSuccess: _ => onSuccess?.Invoke());

        private void _OnStateChange<T>(
            Func<WrappedResult<T>> fnGetResult,
            bool refreshCategories,
            Action<T>? onSuccess = null)
        {
            var result = fnGetResult();
            _onActionHandleResult(result);
            if (!result.IsSuccess)
            {
                return;
            }
            onSuccess?.Invoke(result.Result!);

            //Month will be initialised once transactions loaded. Nothing else meaningful until transactions selected.
            //Note: Could revisit if ever want to add categories before transactions loaded!
            if (Month.HasValue == false)
            {
                return;
            }

            if (refreshCategories)
            {
                var allData = _GetCategoriser().GetSelectorData(Month!.Value);
                Categories = allData.Categories;
                AllSections = allData.Sections?
                    .GroupBy(s => s.Header.Parent.GetKey())
                    .SelectMany((grp, nx) => grp
                        .Select(s => new SectionSelectorRow(
                            s.Header,
                            Display: $"{s.Header.Parent.Name}: {s.Header.Name}",
                            Id: Guid.NewGuid().ToString(), //Arbitrary Id
                            LastUsed: s.LastUsed,
                            BackgroundColor: nx % 2 == 0 ? "#FFFDF6" : "#F7F6FF" //alternate colours by section
                            )))
                    .ToImmutableArray();
            }

            //Always try refresh (if Transactions and Sections loaded)
            var loadedTransactions = LoadedTransactions;
            var allSections = AllSections;
            if (loadedTransactions.HasValue == false || allSections.HasValue == false)
            {
                return;
            }

            var categorisationResult = _GetCategoriser().Categorise(loadedTransactions!.Value, Month!.Value);
            LatestCategorisationResult = categorisationResult;
            var trViewModel = TransactionResultViewModel.CreateFromResult(categorisationResult, allSections!.Value);
            TransactionResultViewModel = trViewModel;

            UnMatchedModel = trViewModel.UnMatchedRows.Any()
                ? new(GetTopSuggestions(allSections!.Value, limit: 4), allSections!.Value, trViewModel.UnMatchedRows)
                : null;
            MatchedModel = trViewModel.MatchedRows.Any()
                ? new(trViewModel.MatchedRows)
                : null;

            _onStateChanged();

            static ImmutableArray<SectionSelectorRow> GetTopSuggestions(ImmutableArray<SectionSelectorRow> allRows, int limit) =>
                allRows
                    .Where(x => x.LastUsed.HasValue)
                    .OrderByDescending(x => x.LastUsed!.Value)
                    .Take(limit)
                    .ToImmutableArray();
        }

        /// <summary>
        /// Attempts to be "smart" in initialising/updating Month based on transactions.
        /// [1] If no transactions, keep current, or initialise to month before "now"
        /// [2] If some transactions...
        /// - If month set AND exists in transactions, keep
        /// ELSE
        /// - If 1/2 months in transactions, take latest as likely bridging into current month
        /// - If 3 or more, take previous month if fully there, else take "penultimate" month as likely the complete one to analyse
        /// </summary>
        private static DateOnly _InitialiseOrUpdateMonthFromTransactions(DateOnly? currentMonth, ImmutableArray<Transaction> transactions)
        {
            var previousMonth = ToYearAndMonth(DateOnly.FromDateTime(DateTime.Now));
            if (transactions.IsEmpty)
            {
                return currentMonth ?? previousMonth; //Likely previous month in absence of all other info
            }
            var months = transactions
                .Select(x => ToYearAndMonth(x.Date))
                .Distinct().OrderBy(x => x)
                .ToImmutableArray();
            if (currentMonth != null && months.Contains(currentMonth.Value))
            {
                return currentMonth.Value; //Keep current month if contained in transactions
            }
            return months.Length < 3
                ? months.Last() //If 1 or 2 - take latest (as if 2, likely bridging over into current month)
                : months.Contains(previousMonth)
                    ? previousMonth //If last month fully contained, probably doing whole month
                    : months[months.Length - 2]; //Else take penultimate "full" month (assuming latest is partial)

            static DateOnly ToYearAndMonth(DateOnly dt) => new DateOnly(dt.Year, dt.Month, 1);
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
    public static Task<MemoryStream> CopyToMemoryStreamAsync(this IBrowserFile bf) =>
        _CopyToMemoryStreamAsync(bf.OpenReadStream());

    private static async Task<MemoryStream> _CopyToMemoryStreamAsync(Stream stream)
    {
        var inputStream = new MemoryStream();
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
