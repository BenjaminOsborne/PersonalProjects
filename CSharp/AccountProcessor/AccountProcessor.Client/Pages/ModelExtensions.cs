using System.Collections.Immutable;

namespace AccountProcessor.Client.Pages;

public static class ModelExtensions
{
    public static ImmutableArray<SectionSelectorRow> GetTopSuggestions(this IEnumerable<SectionSelectorRow> allRows, int limit) =>
        allRows
            .Where(x => x.LastUsed.HasValue)
            .OrderByDescending(x => x.LastUsed!.Value)
            .Take(limit)
            .ToImmutableArray();
}