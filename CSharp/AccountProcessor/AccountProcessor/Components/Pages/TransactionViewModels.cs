using System.Collections.Immutable;
using AccountProcessor.Components.Services;

namespace AccountProcessor.Components.Pages
{
    public record TransactionBlock(string Display, ImmutableArray<TransactionRow> Rows)
    {
        public static ImmutableArray<TransactionBlock> CreateFromResult(CategorisationResult result)
        {
            var blocks = new List<TransactionBlock>();

            var unmatchedTransactions = result.UnMatched.Select(x => new TransactionRow(x, null)).ToImmutableArray();
            blocks.Add(new TransactionBlock("Uncategorised", unmatchedTransactions));

            var matched = result.Matched
                .Select(x =>
                {
                    var section = x.SectionMatches.First().Section;
                    var category = section.Parent;
                    return (x.Transaction, x.SectionMatches, Section: section, Category: category, CategoryKey: category.Order);
                })
                .GroupBy(x => x.CategoryKey)
                .Select(grp =>
                {
                    var category = grp.First().Category;
                    var rows = grp.Select(t => new TransactionRow(t.Transaction, t.Section)).ToImmutableArray();
                    return new TransactionBlock(category.Name, rows);
                }).ToImmutableArray();
            blocks.AddRange(matched);

            return blocks.ToImmutableArray();
        }
    }

    public record TransactionRow(Transaction Transaction, SectionHeader? Section)
    {
        public string? MatchOn { get; set; }
        public string? OverrideDescription { get; set; }
        public string? SelectionId { get; set; }
    }
}
