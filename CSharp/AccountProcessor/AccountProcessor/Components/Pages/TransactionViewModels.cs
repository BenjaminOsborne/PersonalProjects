using System.Collections.Immutable;
using AccountProcessor.Components.Services;

namespace AccountProcessor.Components.Pages
{
    public record SectionSelectorRow(SectionHeader Header, string Display, string Id);

    public record TransactionResult(
        ImmutableArray<TransactionRowUnMatched> UnMatchedRows,
        ImmutableArray<TransactionRowMatched> MatchedRows)
    {
        public static TransactionResult CreateFromResult(CategorisationResult result, ImmutableArray<SectionSelectorRow> selectorOptions)
        {
            var unmatched = result.UnMatched
                .ToImmutableArray(x =>
                {
                    var found = x.SuggestedSection != null
                        ? selectorOptions.FirstOrDefault(s => s.Header.AreSame(x.SuggestedSection!))
                        : null;
                    var tr = x.Transaction;
                    return new TransactionRowUnMatched(tr, DisplayAmount(tr), StyleColor(tr))
                    {
                        SelectionId = found?.Id,
                        MatchOn = tr.Description,
                        OverrideDescription = tr.Description.ToCamelCase()
                    };
                });

            var matched = result.Matched
                .Select(x =>
                {
                    var section = x.SectionMatch.Section;
                    var match = x.SectionMatch.Match;
                    var category = section.Parent;
                    return (Tr: x.Transaction, Matches: match, Section: section, Category: category, CategoryKey: category.Order);
                })
                .GroupBy(x => x.CategoryKey)
                .OrderBy(x => x.Key)
                .ToImmutableArrayMany(grp =>
                {
                    var category = grp.First().Category;
                    return grp
                        .OrderBy(x => x.Section.Order)
                        .ThenBy(x => x.Tr.Date)
                        .Select((t, nx) => new TransactionRowMatched(
                            t.Section.Parent,
                            IsFirstRowForCategory: nx == 0,
                            t.Section,
                            t.Tr,
                            DisplayAmount(t.Tr),
                            StyleColor(t.Tr),
                            t.Matches));
                });

            return new TransactionResult(unmatched, matched);

            static string DisplayAmount(Transaction tr) =>
                tr.Amount < 0 ? $"-£{-tr.Amount:F2}" : $"£{tr.Amount:F2}";
            
            static string StyleColor(Transaction tr) =>
                tr.Amount < 0 ? "color:red" : "color:black";
        }
    }

    public record TransactionRowUnMatched(
        Transaction Transaction,
        string DisplayAmount,
        string StyleColor)
    {
        public string? SelectionId { get; set; }
        public string? MatchOn { get; set; }
        public string? OverrideDescription { get; set; }
        public bool AddOnlyForTransaction { get; set; }
    }

    public record TransactionRowMatched(
        CategoryHeader Category,
        bool IsFirstRowForCategory,
        SectionHeader Section,
        Transaction Transaction,
        string DisplayAmount,
        string StyleColor,
        Match LatestMatch)
    {
        public string MatchPattern => LatestMatch.Pattern;
        public string MatchDescription => LatestMatch.GetDescription();
    }
}
