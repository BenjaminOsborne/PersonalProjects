namespace AccountProcessor.Components.Services
{
    public static class WrappedResult
    {
        public static WrappedResult<T> Create<T>(T result) =>
            new WrappedResult<T> { IsSuccess = true, Result = result };

        public static WrappedResult<T> Fail<T>(string error) =>
            new WrappedResult<T> { Error = error };
    }

    public class WrappedResult<T>
    {
        public bool IsSuccess { get; init; }
        public T? Result { get; init; }
        public string? Error { get; init; }
    }

    public static class TypeExtensions
    {
        public static T? AsNullable<T>(this T item) where T : struct => item;

        public static T? FirstOrDefaultStruct<T>(this IEnumerable<T> item) where T : struct =>
            item.Select(x => x.AsNullable()).FirstOrDefault();
    }

    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);
    }
}
