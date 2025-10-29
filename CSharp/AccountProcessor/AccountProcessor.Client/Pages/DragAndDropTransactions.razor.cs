using AccountProcessor.Core;
using AccountProcessor.Core.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Immutable;

namespace AccountProcessor.Client.Pages;

public record ExistingMatch(SectionHeader Section, Match Match);

public record UpdateMatchRequest(
    DeleteMatchRequest? DeleteExisting,
    MatchRequest ApplyMatch);

public partial class DragAndDropTransactions
{
    /// <summary> Selected drop item from any of the sections (Categorised and UnCategorised) </summary>
    private TransactionDropItem? _selectedDropItem;

    /// <summary> Linked to <see cref="_selectedDropItem"/> but with more state for editing. Kept in sync by setter on property. </summary>
    private SelectedItemViewModel? _selectedItem;

    private TransactionDropItem? SelectedDropItem
    {
        get => _selectedDropItem;
        set
        {
            _selectedDropItem = value;
            _selectedItem = _TryCreateSelectedItem(_selectedDropItem); //Update so always in sync with drop-item
        }
    }

    /// <summary> If true - only renders drop-zone, no items </summary>
    private bool _hideDropItems = false;

    public static ViewModel CreateViewModel(ImmutableArray<SectionSelectorRow> sections, TransactionResultViewModel result, ImmutableArray<SectionSelectorRow> allSelections)
    {
        var categories = sections
            .Select(x => x.Header)
            .GroupBy(x => x.Parent.GetKey())
            .Select(grp =>
            {
                var cat = grp.First().Parent;
                return new CategorySummary(
                    Category: cat,
                    Sections: grp
                        .Select(s => new SectionDropZone(s, _ToDropZoneId(cat, s)))
                        .ToImmutableArray());
            })
            .OrderBy(x => x.Category.Order)
            .ToImmutableArray();

        var matched = result.MatchedRows
            .Select(TransactionDropItem.FromMatchedRow)
            .ToImmutableList();
        var unmatched = result.UnMatchedRows
            .Select(TransactionDropItem.FromUnmatchedRow)
            .ToImmutableList();
        return new(categories, [.. matched.AddRange(unmatched)], allSelections);
    }

    [Parameter]
    public required ViewModel Model { get; init; }

    [Parameter]
    public required Func<ExistingMatch, Task> DeleteMatchAsync { get; init; }
    
    [Parameter]
    public required Func<Transaction, SectionHeader, Task> OnMoveToSectionAsync { get; init; }
    
    [Parameter]
    public required Func<UpdateMatchRequest, Task> UpdateMatchAsync { get; init; }

    [Parameter]
    public required Action<Result> RaiseError { get; init; }

    public record ViewModel(
        IReadOnlyList<CategorySummary> Categories,
        IReadOnlyList<TransactionDropItem> Transactions,
        ImmutableArray<SectionSelectorRow> AllSections
        )
    {
        public ImmutableArray<SectionSelectorRow> TopSuggestions { get; } = AllSections.GetTopSuggestions(limit: 4);

        public IReadOnlyList<TransactionDropItem> GetTransactionsForSection(SectionDropZone sec) =>
            Transactions.Where(x => x.SectionDropZoneId == sec.DropZoneId).ToImmutableList();
    }

    public record CategorySummary(CategoryHeader Category, ImmutableArray<SectionDropZone> Sections);

    public record SectionDropZone(SectionHeader Section, string DropZoneId);

    public record TransactionDropItem(string UniqueItemId,
        string Description,
        Transaction Transaction,
        SectionHeader? Section,
        Match? Match)
    {
        public static TransactionDropItem FromMatchedRow(TransactionRowMatched mr) =>
            new(_CreateUniqueId(),
                mr.MatchDescription,
                mr.Transaction,
                mr.Section,
                mr.LatestMatch)
            {
                SectionDropZoneId = _ToDropZoneId(mr.Category, mr.Section)
            };

        public static TransactionDropItem FromUnmatchedRow(TransactionRowUnMatched r)
        {
            var transaction = r.Transaction;
            var description = transaction.Description;
            return new(_CreateUniqueId(),
                description,
                transaction,
                Section: null,
                Match: null)
            {
                SectionDropZoneId = _unMatchedDropZoneId
            };
        }

        public string SectionDropZoneId { get; set; }
        
        public string AmountDisplay => Transaction.AmountDisplay;
        public string DateDisplay => Transaction.DateDisplay;

        public string AmountAndDescription => $"{Transaction.AmountDisplayAbsolute} {Description}";

        public bool IsCredit => Transaction.Amount >= 0;
    }

    public record SelectedItemViewModel(TransactionDropItem DropItem)
    {
        public bool CanDeleteMatch { get; } = DropItem.Match != null && DropItem.Section != null;

        public bool CanSave => SectionSelectionId != null &&
                               MatchPattern != null &&
                               Match.GetIsValidResult(MatchPattern!, MatchOverrideDescription).IsSuccess;

        /// <summary> SelectionId refers to Ids for items in <see cref="ViewModel.AllSections"/> from selector set </summary>
        public string? SectionSelectionId { get; set; }
        
        public string? MatchPattern { get; set; }
        public string? MatchOverrideDescription { get; set; }
        public bool AddOnlyForTransaction { get; set; }
    }

    private SelectedItemViewModel? _TryCreateSelectedItem(TransactionDropItem? selectedItem)
    {
        if (selectedItem == null)
        {
            return null;
        }

        var section = selectedItem.Section;
        var match = selectedItem.Match;
        var description = selectedItem.Transaction.Description;
        return section != null && match != null
            ? new SelectedItemViewModel(selectedItem)
            {
                SectionSelectionId = Model.AllSections.TryFindMatch(section)?.Id,
                MatchPattern = match.Pattern,
                MatchOverrideDescription = match.OverrideDescription,
                AddOnlyForTransaction = match.ExactDate.HasValue
            }
            : new SelectedItemViewModel(selectedItem)
            {
                MatchPattern = description, //Initialise to match on Description
                MatchOverrideDescription = description.ToCamelCase()
            };
    }

    private static readonly string _unMatchedDropZoneId = _CreateUniqueId();

    private static string _CreateUniqueId() => Guid.NewGuid().ToString();

    private static string _ToDropZoneId(CategoryHeader category, SectionHeader section) =>
        $"{category.Name}|{category.Order}:{section.Name}|{section.Order}|{section.Month}";

    private async Task ItemUpdatedAsync(MudItemDropInfo<TransactionDropItem> dropItem)
    {
        var di = dropItem.Item;
        if (di == null)
        {
            _RaiseError("Empty Item");
            return;
        }

        var dropZoneId = dropItem.DropzoneIdentifier;
        if (di.SectionDropZoneId == dropZoneId)
        {
            return; //No change
        }

        if (dropZoneId == _unMatchedDropZoneId) //If moving from Match to Unmatched -> should have Section & Match defined
        {
            var section = di.Section;
            var match = di.Match;
            if (section == null || match == null)
            {
                _RaiseError("Attempt to move item to undefined - but does not have Section or Match to clear");
                return;
            }

            await DeleteMatchAsync(new(section, match));
        }
        else //Apply exactly match on section
        {
            var section = _TryFindDropSectionForId(dropZoneId);
            if (section == null)
            {
                _RaiseError($"Attempt to move to section - cannot find: {dropZoneId}");
                return; //Cannot find section for Drop Zone
            }

            await OnMoveToSectionAsync(di.Transaction, section);
        }
            
        di.SectionDropZoneId = dropZoneId; //Update internal model (largely irrelevant as whole view currently refreshed)

        //TODO: Execute move and rebuild
        // - Assume it updates/creates the whole Match (so could move/update many items)
        // - Need workflow to "split" match when selected to break a single transaction off (create a specific match for it)

    }

    private SectionHeader? _TryFindDropSectionForId(string? dropZoneId) =>
        dropZoneId != null
            ? Model.Categories
                .SelectMany(x => x.Sections)
                .FirstOrDefault(x => x.DropZoneId == dropZoneId)
                ?.Section
            : null;

    private SectionHeader? _TryFindSectionForSelectionId(string? sectionSelectionId) =>
        sectionSelectionId != null
            ? Model.AllSections
                .FirstOrDefault(x => x.Id == sectionSelectionId)
                ?.Header
            : null;

    private async Task OnClickSaveMatchAsync(SelectedItemViewModel selectedItem)
    {
        //TODO: Impl...
        //Handle if no existing Match (i.e. create new match)
        //Handle if existing -> Should delete and add?
        //Add option to "split" out match

        var applySection = _TryFindSectionForSelectionId(selectedItem.SectionSelectionId);
        if (applySection == null)
        {
            _RaiseError($"ClickSave: Cannot find Section for {selectedItem.SectionSelectionId}");
            return;
        }
        
        var di = selectedItem.DropItem;

        var existingMatch = di is { Section: not null, Match: not null }
            ? new DeleteMatchRequest(di.Section, di.Match)
            : null;

        var apply = new MatchRequest(di.Transaction,
            applySection,
            selectedItem.MatchPattern,
            selectedItem.MatchOverrideDescription);
        
        await UpdateMatchAsync(new(existingMatch, apply));
    }

    private async Task OnClickDeleteMatchAsync(SelectedItemViewModel selectedItem)
    {
        var dropItem = selectedItem.DropItem;
        var section = dropItem.Section;
        var match = dropItem.Match;
        if (section == null || match == null)
        {
            _RaiseError("Delete Match - empty Section/Match to clear");
            return;
        }
        await DeleteMatchAsync(new(section, match));
    }

    private void _RaiseError(string error) =>
        RaiseError.Invoke(Result.Fail($"Drag&Drop: {error}"));
}