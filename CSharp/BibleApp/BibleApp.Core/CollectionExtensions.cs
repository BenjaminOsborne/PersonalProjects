namespace BibleApp.Core;

public static partial class CollectionExtensions
{
    public static IReadOnlyList<T> AsReadOnlyList<T>(this IReadOnlyList<T> items) =>
        items;

    public static IReadOnlyList<T> Materialise<T>(this IEnumerable<T> items) =>
        items.ToArray();

    public static IReadOnlyList<TOut> MaterialiseMap<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, TOut> fnMap) =>
        items.Select(fnMap).ToArray();

    public static bool IsEmpty<T>(this IEnumerable<T> items) =>
        items.Any() == false;

    public static bool IsEmpty<T>(this IReadOnlyCollection<T> items) =>
        items.Count == 0;

}