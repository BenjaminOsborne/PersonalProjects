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
                .ToImmutableArray(x =>
                {
                    var found = x.SuggestedSection != null
                        ? selectorOptions.FirstOrDefault(s => s.Header.AreSame(x.SuggestedSection!))
                        : null;
                    return new TransactionRow(x.Transaction, Section: null, LatestMatch: null)
                    {
                        SelectionId = found?.Id,
                        MatchOn = x.Transaction.Description,
                        OverrideDescription = x.Transaction.Description.ToCamelCase()
                    };
                });
            if (unmatchedTransactions.Any())
            {
                blocks.Add(new TransactionBlock("Uncategorised", IsUnmatched: true, unmatchedTransactions));
            }

            var matched = result.Matched
                .Select(x =>
                {
                    var section = x.SectionMatch.Section;
                    var match = x.SectionMatch.Match;
                    var category = section.Parent;
                    return (x.Transaction, Matches: match, Section: section, Category: category, CategoryKey: category.Order);
                })
                .GroupBy(x => x.CategoryKey)
                .OrderBy(x => x.Key)
                .ToImmutableArray(grp =>
                {
                    var category = grp.First().Category;
                    var rows = grp
                        .OrderBy(x => x.Section.Order)
                        .ThenBy(x => x.Transaction.Date)
                        .ToImmutableArray(t => new TransactionRow(t.Transaction, t.Section, t.Matches));
                    return new TransactionBlock(category.Name, IsUnmatched: false, rows);
                });
            blocks.AddRange(matched);

            return blocks.ToImmutableArray();
        }
    }

    public record TransactionRow(Transaction Transaction, SectionHeader? Section, Match? LatestMatch)
    {
        public string? MatchPattern => LatestMatch?.Pattern;
        public string? MatchDescription => LatestMatch?.GetDescription();

        public string? SelectionId { get; set; }
        public string? MatchOn { get; set; }
        public string? OverrideDescription { get; set; }
        public bool AddOnlyForTransaction { get; set; }
    }
}
