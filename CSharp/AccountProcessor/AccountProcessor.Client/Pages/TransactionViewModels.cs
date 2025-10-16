using System.Collections.Immutable;
using AccountProcessor.Core;
using AccountProcessor.Core.Services;

namespace AccountProcessor.Client.Pages;

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
                var suggested = x.SuggestedMatch != null
                    ? selectorOptions.TryFindMatch(x.SuggestedMatch.SuggestedSection)
                    : null;
                var suggestMatchOnce = suggested != null && x.SuggestedMatch!.SuggestedMatchOnce;

                var tr = x.Transaction;
                var description = tr.Description;
                var hyperlink = _GenerateGoogleSearchForPhrase(description);

                return new TransactionRowUnMatched(tr, hyperlink, tr.AmountDisplay)
                {
                    SelectionId = suggested?.Id, //If none suggested, leave empty
                    AddOnlyForTransaction = suggestMatchOnce,
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
                return grp
                    .OrderBy(x => x.Section.Order)
                    .ThenBy(x => x.Tr.Date)
                    .Select((t, nx) => new TransactionRowMatched(
                        t.Section.Parent,
                        IsLastRowForCategory: nx == grpArr.Length - 1,
                        t.Section,
                        t.Tr,
                        t.Tr.AmountDisplay,
                        t.Match));
            });

        return new TransactionResultViewModel(unmatched, matched);
    }

    private static string _GenerateGoogleSearchForPhrase(string phrase) =>
        $"https://www.google.com/search?q={Uri.EscapeDataString(phrase)}";
}

public record TransactionRowUnMatched(
    Transaction Transaction,
    string Hyperlink,
    string DisplayAmount)
{
    public string? SelectionId { get; set; }
    public string? MatchOn { get; set; }
    public string? OverrideDescription { get; set; }
    public bool AddOnlyForTransaction { get; set; }

    public MudBlazor.Color AmountColor => Transaction.AmountColor();

    public string? NewSectionCategory { get; set; }
    public string? NewSectionName { get; set; }
    public bool NewSectionIsMonthSpecific { get; set; } = true;
}

public record TransactionRowMatched(
    CategoryHeader Category,
    bool IsLastRowForCategory,
    SectionHeader Section,
    Transaction Transaction,
    string DisplayAmount,
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