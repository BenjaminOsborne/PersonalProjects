using System.Collections.Immutable;
using System.Text.Json;

namespace AccountProcessor.Components.Services
{
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
        public ImmutableList<T> PredicateTrue { get; init; }
        public ImmutableList<T> PredicateFalse { get; init; }

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

        public static string ToJoinedString(this IEnumerable<string> items, string separator) =>
            string.Join(separator, items);

        public static string ToJoinedString<T>(this IEnumerable<T> items, string separator, Func<T, string> fnToString) =>
            string.Join(separator, items.Select(fnToString));
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
        public static string Serialise<T>(T data, bool writeIndented = false) =>
            JsonSerializer.Serialize(data,
                new JsonSerializerOptions { IncludeFields = true, WriteIndented = writeIndented });

        public static T? Deserialise<T>(string json) => JsonSerializer.Deserialize<T>(json);
    }

    public static class LazyHelper
    {
        public static Lazy<T> Create<T>(Func<T> fnCreate) => new Lazy<T>(fnCreate);
    }
}
