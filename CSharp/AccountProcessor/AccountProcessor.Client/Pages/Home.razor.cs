using System.Collections.Immutable;
using AccountProcessor.Client.ClientServices;
using AccountProcessor.Core;
using AccountProcessor.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace AccountProcessor.Client.Pages;

public static class FileConstants
{
    public const string ExtractedTransactionsFileExtension = ".extract.xlsx";
}

/// <summary> Code-behind for Home.razor - gives logical split between html-render code and c# data processing </summary>
public partial class Home
{
    [Inject] private IClientTransactionCategoriser _categoriser { get; init; } = null!;

    private MudTabs? TabsRef { get; set; }
    private MudTabPanel? CategoriseTab { get; set; }

    private HomeViewModel Model = null!;

    public void OnFileActionFinished((FileActionType type, Result result) actionParams)
    {
        StateHasChanged(); //MUST refresh first otherwise cannot select disabled tab

        if (actionParams.result.IsSuccess && actionParams.type == FileActionType.LoadTransactions)
        {
            TabsRef?.ActivatePanel(CategoriseTab);
        }
    }

    public Task OnMonthAction() =>
         InvokeAsync(StateHasChanged); //Must trigger refresh in parent from child component action

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Model = new HomeViewModel(_categoriser, onStateChanged: StateHasChanged);
    }

    private bool TransactionsAreFullyLoaded() =>
        Model.TransactionsAreFullyLoaded();
    
    private async Task AddNewMatchForRowAsync(TransactionRowUnMatched row)
    {
        await Model.AddNewMatchForRowAsync(row);
        StateHasChanged(); //Must call: child component can trigger update to error state.
    }

    private async Task ClearMatchAsync(TransactionRowMatched row)
    {
        await Model.ClearMatchAsync(row);
        StateHasChanged(); //Must call: child component can trigger update to error state.
    }
}

/// <summary> Separate class from <see cref="Home"/> so that state transitions & data access can be controlled explicitly </summary>
public class HomeViewModel
{
    private static readonly (string? CategoryName, string? Name, bool IsMonthSpecific) _newSectionDefault = (null, null, true);

    private readonly TransactionsModel _transactionsModel;

    public HomeViewModel(IClientTransactionCategoriser categoriser, Action onStateChanged) =>
        _transactionsModel = new(
            categoriser,
            onActionHandleResult: _UpdateLastActionResult,
            onStateChanged);

    /// <summary> Display message after any action invoked </summary>
    public Result? LastActionResult { get; private set; }

    public Task<bool> CanCategoriseTransactionsAsync() =>
        _transactionsModel.CanCategoriseTransactionsAsync();

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

    /// <summary> Required to pass back to excel-service at end to generate final report. </summary>
    public ImmutableArray<Transaction>? GetLoadedTransactions() => _transactionsModel.LoadedTransactions;

    public UnMatchedRowsTable.ViewModel? UnMatchedModel => _transactionsModel.UnMatchedModel;
    public MatchedRowsTable.ViewModel? MatchedModel => _transactionsModel.MatchedModel;

    public DragAndDropTransactions.ViewModel? DragAndDropModel => _transactionsModel.DragAndDropModel;

    /// <summary> Only actions not managed by this model are the Excel file extracts - this method enables result to display </summary>
    public void OnFileExtractResult(Result result) =>
        _UpdateLastActionResult(result);

    public Task RefreshTransactionsAsync() =>
        _transactionsModel.RefreshTransactionsAsync();

    public Task SetMonthAsync(DateTime? yearAndMonth) =>
        _transactionsModel.UpdateMonthAsync(yearAndMonth.HasValue
            ? WrappedResult.Create(DateOnly.FromDateTime(yearAndMonth.Value))
            : WrappedResult.Fail<DateOnly>($"Invalid date format: {yearAndMonth}"));

    public Task SkipMonthAsync(int months)
    {
        var update = _transactionsModel.Month!.Value.AddMonths(months); //UI only shows "skip" if Month already loaded!
        return _transactionsModel.UpdateMonthAsync(WrappedResult.Create(update));
    }

    public Task CreateNewSectionAsync() =>
        _transactionsModel.ChangeMatchModelAsync(
            fnPerform: async cat =>
            {
                var category = _transactionsModel.Categories?.SingleOrDefault(x => x.Name == _newSection.CategoryName);
                if (category == null || _newSection.Name.IsNullOrWhiteSpace())
                {
                    return Result.Fail("Invalid Category or empty Section Name");
                }

                var request = new AddSectionRequest(category, _newSection.Name!,
                    MatchMonthOnly: _newSection.IsMonthSpecific ? _transactionsModel.Month : null);
                return await cat.AddSectionAsync(request);
            },
            refreshCategories: true,
            onSuccess: () => _newSection = _newSectionDefault);

    public Task AddNewMatchForRowAsync(TransactionRowUnMatched row) =>
        _transactionsModel.ChangeMatchModelAsync(
            fnPerform: async cat =>
            {
                if (!row.AddOnlyForTransaction && row.MatchOn.IsNullOrEmpty())
                {
                    return Result.Fail("'Match On' pattern empty");
                }

                if (row.SelectionId != null &&
                    (row.NewSectionCategory != null || row.NewSectionName != null))
                {
                    return Result.Fail("Ambiguous; Selected both existing Section and header/name for new?");
                }

                SectionHeader useHeader;
                if (row.NewSectionCategory != null)
                {
                    var category = _transactionsModel.Categories?
                        .SingleOrDefault(c => c.Name == row.NewSectionCategory);
                    if (category == null)
                    {
                        return Result.Fail("Could not find category to create new section");
                    }
                    if (row.NewSectionName.IsNullOrWhiteSpace())
                    {
                        return Result.Fail("Could not add new section name: empty");
                    }

                    var req = new AddSectionRequest(category, row.NewSectionName!,
                        MatchMonthOnly: row.NewSectionIsMonthSpecific ? row.Transaction.Date.TrimToMonth() : null);
                    var addedResult = await cat.AddSectionAsync(req);
                    if (!addedResult.IsSuccess)
                    {
                        return addedResult;
                    }
                    useHeader = addedResult.Result!;
                }
                else
                {
                    var header = _transactionsModel.AllSections
                        ?.SingleOrDefault(x => x.Id == row.SelectionId)
                        ?.Header;
                    if (header == null)
                    {
                        return Result.Fail("Could not find Section");
                    }
                    useHeader = header;
                }
                
                var request = new MatchRequest(row.Transaction, useHeader, row.MatchOn, row.OverrideDescription);
                return row.AddOnlyForTransaction
                    ? await cat.MatchOnceAsync(request)
                    : await cat.ApplyMatchAsync(request);
            },
            refreshCategories: true); //Should refresh categories as will update the order in the "suggestions" in the picker

    public Task ClearMatchAsync(TransactionRowMatched row) =>
        _transactionsModel.ChangeMatchModelAsync(
            fnPerform: cat => cat.DeleteMatchAsync(new(row.Section, row.LatestMatch)),
            refreshCategories: false); //Clearing a match does not change categories (or "suggestions" in the picker)

    public Task LoadTransactionsAndCategoriseAsync(WrappedResult<ImmutableArray<Transaction>> result) =>
        _transactionsModel.UpdateLoadedTransactionsAsync(result);

    private void _UpdateLastActionResult(Result result) =>
        LastActionResult = result;

    /// <summary> Class manages storage-of & changes-of state. Ensures transactions re-categorised when match-model changes etc. </summary>
    private class TransactionsModel
    {
        private readonly IClientTransactionCategoriser _categoriser;
        private readonly Action<Result> _onActionHandleResult;
        private readonly Action _onStateChanged;

        public TransactionsModel(IClientTransactionCategoriser categoriser,
            Action<Result> onActionHandleResult,
            Action onStateChanged)
        {
            _categoriser = categoriser;
            _onActionHandleResult = onActionHandleResult;
            _onStateChanged = onStateChanged;
        }

        public async Task<bool> CanCategoriseTransactionsAsync() =>
            (await _categoriser.CanCategoriseTransactionsAsync()).IsSuccess;

        public DateOnly? Month { get; private set; }

        public ImmutableArray<CategoryHeader>? Categories { get; private set; }
        public ImmutableArray<SectionSelectorRow>? AllSections { get; private set; }

        public ImmutableArray<Transaction>? LoadedTransactions { get; private set; }
        public DateOnly? EarliestTransaction { get; private set; }
        public DateOnly? LatestTransaction { get; private set; }

        public TransactionResultViewModel? TransactionResultViewModel { get; private set; }

        public UnMatchedRowsTable.ViewModel? UnMatchedModel { get; private set; }
        public MatchedRowsTable.ViewModel? MatchedModel { get; private set; }
        public DragAndDropTransactions.ViewModel? DragAndDropModel { get; private set; }

        public Task RefreshTransactionsAsync() =>
            _OnStateChangeAsync(
                result: WrappedResult.Create(Unit.Instance),
                refreshCategories: true);

        public Task UpdateMonthAsync(WrappedResult<DateOnly> result) =>
            _OnStateChangeAsync(
                result: result,
                refreshCategories: true, //categories can be month-specific so must refresh
                onSuccess: r => Month = r);

        public Task UpdateLoadedTransactionsAsync(WrappedResult<ImmutableArray<Transaction>> result) =>
            _OnStateChangeAsync(
                result: result,
                refreshCategories: true, //Categories loaded
                onSuccess: r =>
                {
                    Month = _InitialiseOrUpdateMonthFromTransactions(Month, r);
                    LoadedTransactions = r;
                    EarliestTransaction = r.Any() ? r.Select(x => x.Date).Min() : null;
                    LatestTransaction = r.Any() ? r.Select(x => x.Date).Max() : null;
                });

        public Task ChangeMatchModelAsync(Func<IClientTransactionCategoriser, Task<Result>> fnPerform,
            bool refreshCategories,
            Action? onSuccess = null) =>
                _OnStateChangeAsync(
                    fnGetResultAsync: async () => (await fnPerform(_categoriser)).ToWrappedUnit(),
                    refreshCategories: refreshCategories,
                    onSuccess: _ => onSuccess?.Invoke());

        private Task _OnStateChangeAsync<T>(
            WrappedResult<T> result,
            bool refreshCategories,
            Action<T>? onSuccess = null) =>
            _OnStateChangeAsync(fnGetResultAsync: () => Task.FromResult(result), refreshCategories, onSuccess);

        private async Task _OnStateChangeAsync<T>(
            Func<Task<WrappedResult<T>>> fnGetResultAsync,
            bool refreshCategories,
            Action<T>? onSuccess = null)
        {
            var result = await fnGetResultAsync();
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
                var allDataResult = await _categoriser.GetSelectorDataAsync(Month!.Value);
                if (!allDataResult.IsSuccess)
                {
                    _onActionHandleResult(allDataResult);
                    return;
                }
                var allData = allDataResult.Result!;
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

            var categorisationResult = await _categoriser.CategoriseAsync(new (loadedTransactions.Value, Month!.Value));
            if (!categorisationResult.IsSuccess)
            {
                _onActionHandleResult(categorisationResult);
                return;
            }


            //Clear models initially; ensures refresh is properly triggered
            _OnCategorisedSetModels(null, null, null, null);

            //Update models
            var trViewModel = TransactionResultViewModel.CreateFromResult(categorisationResult.Result!, allSections.Value);
            var categories = Categories?.Select(x => x.Name).ToImmutableArray() ?? [];
            _OnCategorisedSetModels(
                transactionModel: trViewModel,
                unmatchedModel: trViewModel.UnMatchedRows.Any()
                    ? new UnMatchedRowsTable.ViewModel(allSections.Value.GetTopSuggestions(limit: 4), allSections.Value, trViewModel.UnMatchedRows, categories)
                    : null,
                matchedModel: trViewModel.MatchedRows.Any()
                    ? new MatchedRowsTable.ViewModel(trViewModel.MatchedRows)
                    : null,
                dragAndDropModel: DragAndDropTransactions.CreateViewModel(allSections.Value, trViewModel));
        }

        private void _OnCategorisedSetModels(
            TransactionResultViewModel? transactionModel,
            UnMatchedRowsTable.ViewModel? unmatchedModel,
            MatchedRowsTable.ViewModel? matchedModel,
            DragAndDropTransactions.ViewModel? dragAndDropModel)
        {
            TransactionResultViewModel = transactionModel;
            UnMatchedModel = unmatchedModel;
            MatchedModel = matchedModel;
            DragAndDropModel = dragAndDropModel;
            _onStateChanged();
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
        bf.OpenReadStream().CopyToMemoryStreamAsync();

    public static async Task<MemoryStream> CopyToMemoryStreamAsync(this Stream stream)
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
