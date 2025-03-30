using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;

namespace AccountProcessor.Core;

public class Unit
{
    private Unit() { }
    public static Unit Instance { get; } = new Unit();
}

public static class WrappedResult
{
    public static WrappedResult<T> Create<T>(T result) =>
        new WrappedResult<T> { IsSuccess = true, Result = result };

    public static WrappedResult<T> Fail<T>(string error) =>
        new WrappedResult<T> { Error = error };
}

public class Result
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    public static Result Success { get; } = new Result { IsSuccess = true };
    public static Result Fail(string error) => new Result { Error = error };

    public WrappedResult<Unit> ToWrappedUnit() =>
        IsSuccess ? WrappedResult.Create(Unit.Instance) : WrappedResult.Fail<Unit>(Error!);
}

public class WrappedResult<T> : Result
{
    public T? Result { get; init; }

    public WrappedResult<TMap> MapFail<TMap>() =>
        !IsSuccess
            ? WrappedResult.Fail<TMap>(Error!)
            : throw new ArgumentException("MapFail called on Success object");
}

public class Partition<T>
{
    public ImmutableList<T> PredicateTrue { get; init; } = null!;
    public ImmutableList<T> PredicateFalse { get; init; } = null!;

    public Partition<U> Map<U>(Func<T, U> fnMap) => new Partition<U>
    {
        PredicateTrue = PredicateTrue.Select(fnMap).ToImmutableList(),
        PredicateFalse = PredicateFalse.Select(fnMap).ToImmutableList()
    };
}

public static class TypeExtensions
{
    public static T? AsNullable<T>(this T item) where T : struct => item;

    public static T? FirstOrDefaultStruct<T>(this IEnumerable<T> item) where T : struct =>
        item.Select(x => x.AsNullable()).FirstOrDefault();
}

public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? s) => string.IsNullOrEmpty(s);

    public static bool IsNullOrWhiteSpace(this string? s) => string.IsNullOrWhiteSpace(s);

    public static string ToCamelCase(this string value)
    {
        var split = new System.Text.RegularExpressions.Regex("\\s+")
            .Split(value);
        return split
            .Where(x => x.Length > 0)
            .Select(x => x.Length > 1
                ? $"{char.ToUpper(x[0])}{x.Skip(1).Select(char.ToLower).CreateString()}"
                : x.ToUpper())
            .ToJoinedString(" ");
    }

    public static string CreateString(this char[] chars) => new string(chars);

    public static string CreateString(this IEnumerable<char> chars) => new string(chars.ToArray());

    public static string ToJoinedString(this IEnumerable<string> items, string separator) =>
        string.Join(separator, items);

    public static string ToJoinedString<T>(this IEnumerable<T> items, string separator, Func<T, string> fnToString) =>
        string.Join(separator, items.Select(fnToString));
}

public static class DateExtensions
{
    public static DateOnly TrimToMonth(this DateOnly date) =>
        new(date.Year, date.Month, 1);
}

public static class EnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> items, Action<T> fnPerform)
    {
        foreach (var item in items)
        {
            fnPerform(item);
        }
    }

    public static Partition<T> Partition<T>(this IEnumerable<T> items, Func<T, bool> fnPredicate)
    {
        var predTrue = ImmutableList.CreateBuilder<T>();
        var predFalse = ImmutableList.CreateBuilder<T>();
        foreach (var item in items)
        {
            if (fnPredicate(item))
            {
                predTrue.Add(item);
            }
            else
            {
                predFalse.Add(item);
            }
        }
        return new Partition<T> { PredicateTrue = predTrue.ToImmutable(), PredicateFalse = predFalse.ToImmutable() };
    }

    public static IEnumerable<(T Value, int Index)> SelectWithIndexes<T>(this IEnumerable<T> items) =>
        items.Select((x, nx) => (x, nx));

    public static IEnumerable<T> ConcatItem<T>(this IEnumerable<T> items, T item) => items.Concat([item]);

    public static IEnumerable<ImmutableArray<T>> GroupIntoBlocks<T>(this IEnumerable<T> items, int groupSize)
    {
        ImmutableArray<T>.Builder? builder = null;
        foreach(var item in items)
        {
            builder ??= ImmutableArray.CreateBuilder<T>();
            builder.Add(item);

            if(builder.Count == groupSize)
            {
                yield return builder.ToImmutable();
                builder = null;
            }
        }

        if (builder != null && builder.Count > 0)
        {
            yield return builder.ToImmutable();
        }
    }
}

public static class ImmutableExtensions
{
    public static ImmutableArray<T> ToImmutableArray<TIn, T>(this IEnumerable<TIn> items, Func<TIn, T> fnMap) =>
        items.Select(fnMap).ToImmutableArray();

    public static ImmutableArray<T> ToImmutableArrayMany<TIn, T>(this IEnumerable<TIn> items, Func<TIn, IEnumerable<T>> fnMap) =>
        items.SelectMany(fnMap).ToImmutableArray();
}

public static class DictionaryExtensions
{
    public static T? TryGet<TKey, T>(this IReadOnlyDictionary<TKey, T> map, TKey key) where T : class =>
        map.TryGetValue(key, out var value) ? value : default;

    public static T? TryGetStruct<TKey, T>(this IReadOnlyDictionary<TKey, T> map, TKey key) where T : struct =>
        map.TryGetValue(key, out var value) ? value : null;
}

public static class StreamExtensions
{
    public static async Task<byte[]> ReadAllBytesAsync(this Stream input)
    {
        using var memory = new MemoryStream();
        await input.CopyToAsync(memory);
        return memory.ToArray();
    }
}

public static class JsonHelper
{
    private static readonly JsonSerializerOptions _writeNormal = new () { IncludeFields = true, WriteIndented = false };
    private static readonly JsonSerializerOptions _writeIndented = new () { IncludeFields = true, WriteIndented = true };
    
    private static readonly JsonSerializerOptions _deserialiseIgnoreCase = new () { PropertyNameCaseInsensitive = true };

    public static string Serialise<T>(T data, bool writeIndented = false) =>
        JsonSerializer.Serialize(data, writeIndented ? _writeIndented : _writeNormal);

    public static T? Deserialise<T>(string json, bool ignoreCase = false) => ignoreCase
        ? JsonSerializer.Deserialize<T>(json, options: _deserialiseIgnoreCase)
        : JsonSerializer.Deserialize<T>(json);

    public static T? Deserialise<T>(Stream stream) => JsonSerializer.Deserialize<T>(stream);
}

public static class LazyHelper
{
    public static Lazy<T> Create<T>(Func<T> fnCreate) => new Lazy<T>(fnCreate);
}

public static class ReflectionUtility
{
    public static ImmutableArray<(string fieldName, T value)> GetPublicConstFieldsOfType<T>(this Type type) =>
        type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && fi.IsInitOnly == false)
            .Select(x => x.GetRawConstantValue() is T rawCast ? (fieldName: x.Name, value: rawCast).AsNullable() : null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToImmutableArray();

    public static ImmutableArray<(string propertyName, T value)> GetAllStaticPublicPropertiesOfType<T>(this Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.PropertyType == typeof(T))
            .Select(x => (fieldName: x.Name, value: (T)x.GetValue(null)!))
            .ToImmutableArray();

    public static ImmutableArray<(string fieldName, T value)> GetAllStaticPublicFieldsOfType<T>(this Type type) =>
        type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.FieldType == typeof(T))
            .Select(x => (fieldName: x.Name, value: (T)x.GetValue(null)!))
            .ToImmutableArray();
}

public class FuncCreator
{
    public static Func<T> Build<T>(Func<T> func) => func;
}

public class FuncCreator<TIn>
{
    public static Func<TIn, T> Build<T>(Func<TIn, T> func) => func;
}