using System.Collections.Immutable;
using AccountProcessor.Components.Services;

namespace AccountProcessor.Components.Pages
{
    public record SectionSelectorRow(SectionHeader Header, string Display, string Id);

    public record TransactionBlock(string Display, bool IsUnmatched, ImmutableArray<TransactionRow> Rows)
    {
        public static ImmutableArray<TransactionBlock> CreateFromResult(CategorisationResult result, ImmutableArray<SectionSelectorRow> selectorOptions)
        {
            var blocks = new List<TransactionBlock>();

            var unmatchedTransactions = result.UnMatched
                .Select(x =>
                {
                    var found = x.SuggestedSection != null
                        ? selectorOptions.FirstOrDefault(s => s.Header.AreSame(x.SuggestedSection!))
                        : null;
                    return new TransactionRow(x.Transaction, Section: null)
                    {
                        SelectionId = found?.Id,
                        MatchOn = x.Transaction.Description,
                        OverrideDescription = x.Transaction.Description.ToCamelCase()
                    };
                })
                .ToImmutableArray();
            blocks.Add(new TransactionBlock("Uncategorised", IsUnmatched: true, unmatchedTransactions));

            var matched = result.Matched
                .Select(x =>
                {
                    var section = x.SectionMatches.First().Section;
                    var category = section.Parent;
                    return (x.Transaction, x.SectionMatches, Section: section, Category: category, CategoryKey: category.Order);
                })
                .GroupBy(x => x.CategoryKey)
                .OrderBy(x => x.Key)
                .Select(grp =>
                {
                    var category = grp.First().Category;
                    var rows = grp
                        .OrderBy(x => x.Section.Order)
                        .ThenBy(x => x.Transaction.Date)
                        .Select(t => new TransactionRow(t.Transaction, t.Section))
                        .ToImmutableArray();
                    return new TransactionBlock(category.Name, IsUnmatched: false, rows);
                }).ToImmutableArray();
            blocks.AddRange(matched);

            return blocks.ToImmutableArray();
        }
    }

    public record TransactionRow(Transaction Transaction, SectionHeader? Section)
    {
        public string? SelectionId { get; set; }
        public string? MatchOn { get; set; }
        public string? OverrideDescription { get; set; }
        public bool AddOnlyForTransaction { get; set; }
    }
}
