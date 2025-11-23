namespace BibleApp.Core;

public static class CollectionExtensions
{
    public static IReadOnlyList<T> AsReadOnlyList<T>(this IReadOnlyList<T> items) =>
        items;

    public static IReadOnlyList<T> Materialise<T>(this IEnumerable<T> items) =>
        items.ToArray();

    public static IReadOnlyList<TOut> MaterialiseMap<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, TOut> fnMap) =>
        items.Select(fnMap).ToArray();

}