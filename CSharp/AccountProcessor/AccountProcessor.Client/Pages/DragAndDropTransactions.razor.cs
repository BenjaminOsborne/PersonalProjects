using System.Collections.Immutable;
using AccountProcessor.Core.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AccountProcessor.Client.Pages;

public partial class DragAndDropTransactions
{
    public static readonly string UnMatchedDropZoneId = _GetUniqueId();

    private static string _GetUniqueId() => Guid.NewGuid().ToString();

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
                        .Select(s => new SectionDropZone(s, ToDropZoneId(cat, s)))
                        .ToImmutableArray());
            })
            .OrderBy(x => x.Category.Order)
            .ToImmutableArray();

        var matched = result.MatchedRows
            .Select(mr => new TransactionDropItem(_GetUniqueId(), mr.MatchDescription, mr.Transaction, ToDropZoneId(mr.Category, mr.Section)))
            .ToImmutableList();
        var unmatched = result.UnMatchedRows
            .Select(r => new TransactionDropItem(_GetUniqueId(), r.Transaction.Description, r.Transaction, UnMatchedDropZoneId))
            .ToImmutableList();
        return new(categories, [.. matched.AddRange(unmatched)]);

        static string ToDropZoneId(CategoryHeader category, SectionHeader section) =>
            $"{category.Name}|{category.Order}:{section.Name}|{section.Order}|{section.Month}";
}

    public record CategorySummary(CategoryHeader Category, ImmutableArray<SectionDropZone> Sections);

    public record SectionDropZone(SectionHeader Section, string DropZoneId);

    public record TransactionDropItem(string UniqueItemId,
        string Description,
        Transaction Transaction,
        string DropZoneId)
    {
        public string SectionDropZoneId { get; set; } = DropZoneId;
    }

    public record ViewModel(
        IReadOnlyList<CategorySummary> Categories,
        IReadOnlyList<TransactionDropItem> Transactions);

    [Parameter]
    public required ViewModel Model { get; init; }
    
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