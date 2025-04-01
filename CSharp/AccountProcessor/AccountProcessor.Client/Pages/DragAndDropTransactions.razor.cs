using System.Collections.Immutable;
using AccountProcessor.Core.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AccountProcessor.Client.Pages;

public partial class DragAndDropTransactions
{
    private TransactionDropItem? _selectedItem;
    /// <summary> If true - only renders drop-zone, no items </summary>
    private bool _hideDropItems = true;

    public static ViewModel CreateViewModel(ImmutableArray<SectionSelectorRow> sections, TransactionResultViewModel result)
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
        return new(categories, [.. matched.AddRange(unmatched)]);
    }

    [Parameter]
    public required ViewModel Model { get; init; }

    public record ViewModel(
        IReadOnlyList<CategorySummary> Categories,
        IReadOnlyList<TransactionDropItem> Transactions)
    {
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

        public static TransactionDropItem FromUnmatchedRow(TransactionRowUnMatched r) =>
            new(_CreateUniqueId(),
                r.Transaction.Description,
                r.Transaction,
                _unMatchedDropZoneId,
                Section: null,
                Match: null);

        public string SectionDropZoneId { get; set; } = DropZoneId;
        
        public string AmountDisplay => Transaction.AmountDisplay;
        public string DateDisplay => Transaction.DateDisplay;

        public string AmountAndDescription => $"{Transaction.AmountDisplayAbsolute} {Description}";

        public bool IsCredit => Transaction.Amount >= 0;
    }

    private static readonly string _unMatchedDropZoneId = _CreateUniqueId();

    private static string _CreateUniqueId() => Guid.NewGuid().ToString();

    private static string _ToDropZoneId(CategoryHeader category, SectionHeader section) =>
        $"{category.Name}|{category.Order}:{section.Name}|{section.Order}|{section.Month}";

    private void UnMatchedItemSelected(string? itemId) =>
        _OnItemSelected(itemId);

    private void MatchedItemSelected(string? itemId) =>
        _OnItemSelected(itemId);

    private void _OnItemSelected(string? itemId)
    {
        var found = Model.Transactions.FirstOrDefault(x => x.UniqueItemId == itemId);
        _selectedItem = found;
    }

    private void ItemUpdated(MudItemDropInfo<TransactionDropItem> dropItem)
    {
        var di = dropItem.Item;
        if (di != null)
        {
            //TODO: Should execute move and rebuild
            di.SectionDropZoneId = dropItem.DropzoneIdentifier;
        }
    }
}