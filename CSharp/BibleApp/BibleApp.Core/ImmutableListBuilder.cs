using System.Collections.Immutable;

namespace BibleApp.Core;

public static class ImmutableListBuilder
{
    public static ImmutableList<T>.Builder ToImmutableListBuilder<T>(this IEnumerable<T> items) =>
        _CreateWithRange(items);

    public static ImmutableList<T>.Builder Create<T>() =>
        ImmutableList.CreateBuilder<T>();

    public static ImmutableList<T>.Builder Create<T>(T item)
    {
        var builder = ImmutableList.CreateBuilder<T>();
        builder.Add(item);
        return builder;
    }

    public static ImmutableList<T>.Builder Create<T>(params T[] items) =>
        _CreateWithRange(items);

    public static ImmutableList<T>.Builder Create<T>(IEnumerable<T> items) =>
        _CreateWithRange(items);

    /// <summary> Adds item and returns the builder </summary>
    public static ImmutableList<T>.Builder AddFluent<T>(this ImmutableList<T>.Builder builder, T item)
    {
        builder.Add(item);
        return builder;
    }

    private static ImmutableList<T>.Builder _CreateWithRange<T>(IEnumerable<T> items)
    {
        var builder = ImmutableList.CreateBuilder<T>();
        builder.AddRange(items);
        return builder;
    }
}