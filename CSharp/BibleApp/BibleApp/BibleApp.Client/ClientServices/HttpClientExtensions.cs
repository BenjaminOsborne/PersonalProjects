using BibleApp.Core;

namespace BibleApp.Client.ClientServices;

public static class HttpClientExtensions
{
    /// <summary> If controller returns <see cref="T"/>, must handle as <see cref="Result"/> as transport layer could fail </summary>
    public static Task<Result<T>> MapJsonAsync<T>(this Task<HttpResponseMessage> taskMessage) where T : class =>
        _MapResultAsync<T, Result<T>>(taskMessage,
            fnMapResult: Result.CreateSuccess,
            fnCreateFailure: Result.Fail<T>);

    private static async Task<TResult> _MapResultAsync<T, TResult>(Task<HttpResponseMessage> taskMessage,
        Func<T, TResult> fnMapResult,
        Func<string, TResult> fnCreateFailure)
    {
        var message = await taskMessage;
        if (!message.IsSuccessStatusCode)
        {
            var error = await message.Content.ReadAsStringAsync();
            var context = $"{error} [{message.RequestMessage?.RequestUri?.AbsolutePath ?? "<UNKNOWN_PATH>"}]";
            return fnCreateFailure($"{message.StatusCode}: {context.Trim()}");
        }
        var json = await message.Content.ReadAsStringAsync();
        var type = JsonHelper.Deserialise<T>(json, ignoreCase: true);
        return type != null
            ? fnMapResult(type)
            : fnCreateFailure("Could not parse response");
    }
}