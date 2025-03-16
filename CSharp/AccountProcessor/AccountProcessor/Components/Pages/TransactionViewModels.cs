using System.Collections.Immutable;
using AccountProcessor.Components.Services;

namespace AccountProcessor.Components.Pages;

public record SectionSelectorRow(SectionHeader Header, string Display, string Id, DateTime? LastUsed, string? BackgroundColor);

public record TransactionResultViewModel(
    ImmutableArray<TransactionRowUnMatched> UnMatchedRows,
    ImmutableArray<TransactionRowMatched> MatchedRows)
{
    public static TransactionResultViewModel CreateFromResult(CategorisationResult result, ImmutableArray<SectionSelectorRow> selectorOptions)
    {
        var unmatched = result.UnMatched
            .ToImmutableArray(x =>
            {
                var found = x.SuggestedSection != null
                    ? selectorOptions.FirstOrDefault(s => s.Header.AreSame(x.SuggestedSection!))
                    : null;
                var tr = x.Transaction;
                var description = tr.Description;
                var hyperlink = _GenerateGoogleSearchForPhrase(description);

                return new TransactionRowUnMatched(tr, hyperlink, DisplayAmount(tr), StyleColor(tr))
                {
                    SelectionId = found?.Id, //If none suggested, leave empty
                    MatchOn = description,
                    OverrideDescription = description.ToCamelCase()
                };
            });

        var matched = result.Matched
            .Select(x =>
            {
                var section = x.SectionMatch.Section;
                var match = x.SectionMatch.Match;
                var category = section.Parent;
                return (Tr: x.Transaction, Match: match, Section: section, Category: category, CategoryKey: category.Order);
            })
            .GroupBy(x => x.CategoryKey)
            .OrderBy(x => x.Key)
            .ToImmutableArrayMany(grp =>
            {
                var grpArr = grp.ToImmutableArray();
                var category = grpArr[0].Category;
                return grp
                    .OrderBy(x => x.Section.Order)
                    .ThenBy(x => x.Tr.Date)
                    .Select((t, nx) => new TransactionRowMatched(
                        t.Section.Parent,
                        IsLastRowForCategory: nx == grpArr.Length - 1,
                        t.Section,
                        t.Tr,
                        DisplayAmount(t.Tr),
                        StyleColor(t.Tr),
                        t.Match));
            });

        return new TransactionResultViewModel(unmatched, matched);

        static string DisplayAmount(Transaction tr) =>
            tr.Amount < 0 ? $"-£{-tr.Amount:F2}" : $"£{tr.Amount:F2}";
            
        static string StyleColor(Transaction tr) =>
            tr.Amount < 0 ? "color:red" : "color:black";
    }

    private static string _GenerateGoogleSearchForPhrase(string phrase) =>
        $"https://www.google.com/search?q={Uri.EscapeDataString(phrase)}";
}

public record TransactionRowUnMatched(
    Transaction Transaction,
    string Hyperlink,
    string DisplayAmount,
    string StyleColor)
{
    public string? SelectionId { get; set; }
    public string? MatchOn { get; set; }
    public string? OverrideDescription { get; set; }
    public bool AddOnlyForTransaction { get; set; }

    public MudBlazor.Color AmountColor => Transaction.AmountColor();
}

public record TransactionRowMatched(
    CategoryHeader Category,
    bool IsLastRowForCategory,
    SectionHeader Section,
    Transaction Transaction,
    string DisplayAmount,
    string StyleColor,
    Match LatestMatch)
{
    public string CategorySectionDisplay => $"{Category.Name}: {Section.Name}";
    public string MatchPattern => LatestMatch.Pattern;
    public string MatchDescription => LatestMatch.GetDescription();
    public MudBlazor.Color AmountColor => Transaction.AmountColor();
}

public static class TransactionColorExtensions
{
    public static MudBlazor.Color AmountColor(this Transaction t) => t.Amount < 0 ? MudBlazor.Color.Inherit : MudBlazor.Color.Success;
}