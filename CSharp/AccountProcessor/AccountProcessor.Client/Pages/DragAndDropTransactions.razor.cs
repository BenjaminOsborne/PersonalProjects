using AccountProcessor.Client.ClientServices;
using AccountProcessor.Core;
using AccountProcessor.Core.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Immutable;

namespace AccountProcessor.Client.Pages;

public partial class DragAndDropTransactions
{
    [Inject] private IClientTransactionCategoriser _categoriser { get; init; } = null!;

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
        /// <summary> SelectionId refers to Ids for items in <see cref="ViewModel.AllSections"/> from selector set </summary>
        public string? SelectionId { get; set; }
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
                SelectionId = Model.AllSections.TryFindMatch(section)?.Id,
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
                return; //Invalid: Both should be defined
            }

            var result = await _categoriser.DeleteMatchAsync(new DeleteMatchRequest(section, match));
            if (!result.IsSuccess)
            {
                return; //DeleteMatch Failed
            }
        }
        else //Apply exactly match on section
        {
            var dropSec = Model.Categories
                .SelectMany(x => x.Sections)
                .FirstOrDefault(x => x.DropZoneId == dropZoneId);
            if (dropSec == null)
            {
                return; //Cannot find section for Drop Zone
            }

            var result = await _categoriser.MatchOnceAsync(new MatchRequest(
                di.Transaction,
                dropSec.Section,
                di.Transaction.Description,
                OverrideDescription: null));
            if (!result.IsSuccess)
            {
                return; //Update failed
            }
        }
            
        di.SectionDropZoneId = dropZoneId; //Update internal model

        //TODO: Trigger full refresh!?

        //TODO: Execute move and rebuild
        // - Assume it updates/creates the whole Match (so could move/update many items)
        // - Need workflow to "split" match when selected to break a single transaction off (create a specific match for it)

        //TODO: Display error if fails!
    }
}