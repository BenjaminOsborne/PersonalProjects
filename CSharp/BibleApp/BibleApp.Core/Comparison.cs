namespace BibleApp.Core;

public static class Comparison
{
    public static bool EqualsSafe<T>(this T? a, T? b) where T : IEquatable<T> =>
        a != null
            ? b != null && a.Equals(b)
            : b == null;

    public static bool EqualsSafe<T>(this T? a, T? b) where T : struct, IEquatable<T> =>
        a.HasValue
            ? b.HasValue && a.Value.Equals(b.Value)
            : !b.HasValue;

    public static bool EqualsSafeEnum<T>(this T a, T b) where T : struct, IConvertible =>
        a.Equals(b);

    public static bool EqualsSafeEnum<T>(this T? a, T? b) where T : struct, IConvertible =>
        a.HasValue
            ? b.HasValue && a.Value.Equals(b.Value)
            : !b.HasValue;

    public static bool NotEqualsSafe<T>(this T? a, T? b) where T : IEquatable<T> =>
        !a.EqualsSafe(b);

    public static bool NotEqualsSafe<T>(this T? a, T? b) where T : struct, IEquatable<T> =>
        !a.EqualsSafe(b);

    public static bool NotEqualsSafeEnum<T>(this T a, T b) where T : struct, IConvertible =>
        !a.EqualsSafeEnum(b);

    public static bool NotEqualsSafeEnum<T>(this T? a, T? b) where T : struct, IConvertible =>
        !a.EqualsSafeEnum(b);

    public static bool SequenceEqualSafe<T>(this IEnumerable<T?> items, IEnumerable<T?> other) where T : IEquatable<T> =>
        items.SequenceEqual(other);

    public static bool ContainsSafe<T>(this IEnumerable<T?> items, T? item) where T : IEquatable<T> =>
        items.Contains(item);

    public static bool DoesNotContainSafe<T>(this IEnumerable<T?> items, T? item) where T : IEquatable<T> =>
        !items.Contains(item);
}

public static class OrderBySafeExtensions
{
    private const string OrderOnOrdered = "Attempt to re-order an ordered collection. Likely erroneous.";

    [Obsolete(OrderOnOrdered)]
    public static IOrderedEnumerable<TComp> OrderBySafe<TComp>(this IOrderedEnumerable<TComp> items) =>
        throw new NotImplementedException(OrderOnOrdered);

    [Obsolete(OrderOnOrdered)]
    public static IOrderedEnumerable<T> OrderBySafe<T, TComp>(this IOrderedEnumerable<T> items, Func<T, TComp> fnGetComparable) =>
        throw new NotImplementedException(OrderOnOrdered);

    [Obsolete(OrderOnOrdered)]
    public static IOrderedEnumerable<TComp> OrderByDescendingSafe<TComp>(this IOrderedEnumerable<TComp> items) =>
        throw new NotImplementedException(OrderOnOrdered);

    [Obsolete(OrderOnOrdered)]
    public static IOrderedEnumerable<T> OrderByDescendingSafe<T, TComp>(this IOrderedEnumerable<T> items, Func<T, TComp> fnGetComparable) =>
        throw new NotImplementedException(OrderOnOrdered);

    public static IOrderedEnumerable<TComp> OrderBySafe<TComp>(this IEnumerable<TComp> items)
        where TComp : IComparable =>
        items.OrderBy(x => x);

    public static IOrderedEnumerable<TComp?> OrderBySafe<TComp>(this IEnumerable<TComp?> items)
        where TComp : struct, IComparable =>
        items.OrderBy(x => x);

    public static IOrderedEnumerable<T> OrderBySafe<T, TComp>(this IEnumerable<T> items, Func<T, TComp> fnGetComparable)
        where TComp : IComparable =>
        items.OrderBy(fnGetComparable);

    public static IOrderedEnumerable<T> OrderBySafe<T, TComp>(this IEnumerable<T> items, Func<T, TComp?> fnGetComparable)
        where TComp : struct, IComparable =>
        items.OrderBy(fnGetComparable);

    public static IOrderedEnumerable<T> OrderByDescendingSafe<T, TComp>(this IEnumerable<T> items, Func<T, TComp> fnGetComparable)
        where TComp : IComparable =>
        items.OrderByDescending(fnGetComparable);

    public static IOrderedEnumerable<TComp> OrderByDescendingSafe<TComp>(this IEnumerable<TComp> items)
        where TComp : IComparable =>
        items.OrderByDescending(x => x);

    public static IOrderedEnumerable<T> OrderByDescendingSafe<T, TComp>(this IEnumerable<T> items, Func<T, TComp?> fnGetComparable)
        where TComp : struct, IComparable =>
        items.OrderByDescending(fnGetComparable);

    public static IOrderedEnumerable<T> ThenBySafe<T, TComp>(this IOrderedEnumerable<T> items, Func<T, TComp> fnGetComparable)
        where TComp : IComparable =>
        items.ThenBy(fnGetComparable);

    public static IOrderedEnumerable<T> ThenByDescendingSafe<T, TComp>(this IOrderedEnumerable<T> items, Func<T, TComp> fnGetComparable)
        where TComp : IComparable =>
        items.ThenByDescending(fnGetComparable);
}