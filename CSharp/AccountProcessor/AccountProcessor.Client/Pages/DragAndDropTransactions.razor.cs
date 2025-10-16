using System.Collections.Immutable;
using AccountProcessor.Core;
using AccountProcessor.Core.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AccountProcessor.Client.Pages;

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
        string DropZoneId,
        SectionHeader? Section,
        Match? Match)
    {
        public static TransactionDropItem FromMatchedRow(TransactionRowMatched mr) =>
            new(_CreateUniqueId(),
                mr.MatchDescription,
                mr.Transaction,
                _ToDropZoneId(mr.Category, mr.Section),
                mr.Section,
                mr.LatestMatch);

        public static TransactionDropItem FromUnmatchedRow(TransactionRowUnMatched r)
        {
            var transaction = r.Transaction;
            var description = transaction.Description;
            return new(_CreateUniqueId(),
                description,
                transaction,
                _unMatchedDropZoneId,
                Section: null,
                Match: null);
        }

        public string SectionDropZoneId { get; set; } = DropZoneId;
        
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

    private void ItemUpdated(MudItemDropInfo<TransactionDropItem> dropItem)
    {
        var di = dropItem.Item;
        if (di != null)
        {
            di.SectionDropZoneId = dropItem.DropzoneIdentifier;

            //TODO: Execute move and rebuild
            // - Assume it updates/creates the whole Match (so could move/update many items)
            // - Need workflow to "split" match when selected to break a single transaction off (create a specific match for it)
        }
    }
}