using AccountProcessor.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace AccountProcessor.ClientServices;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> PostFileToUrlAsync(this HttpClient httpClient,
        IBrowserFile browserFile,
        string relativeUrl,
        string apiParameter = "file")
    {
        await using var inputStream = browserFile.OpenReadStream();
        return await httpClient.PostToUrlWithStreamContentAsync(
            relativeUrl: relativeUrl,
            apiParameter: apiParameter,
            inputStream,
            fileName: browserFile.Name,
            contentType: browserFile.ContentType
        );
    }
    public static async Task<HttpResponseMessage> PostToUrlWithStreamContentAsync(this HttpClient httpClient,
        string relativeUrl,
        string apiParameter,
        Stream stream,
        string fileName,
        string contentType)
    {
        using var streamContent = new StreamContent(stream);
        var mediaType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);
        streamContent.Headers.ContentType = mediaType;

        using var multiPartContent = new MultipartFormDataContent
        {
            { streamContent, apiParameter, fileName }
        };
        using var message = new HttpRequestMessage(HttpMethod.Post, relativeUrl);
        message.Content = multiPartContent;
        return await httpClient.SendAsync(message);
    }

    public static async Task<WrappedResult<byte[]>> MapFileByesAsync(this Task<HttpResponseMessage> taskMessage)
    {
        var message = await taskMessage;
        if (!message.IsSuccessStatusCode)
        {
            var error = await message.Content.ReadAsStringAsync();
            return WrappedResult.Fail<byte[]>($"{message.StatusCode}: {error}");
        }
        var bytes = await message.Content.ReadAsByteArrayAsync();
        return WrappedResult.Create(bytes);
    }

    /// <summary> If controller returns <see cref="T"/>, must handle as <see cref="WrappedResult{T}"/> as transport layer could fail </summary>
    public static Task<WrappedResult<T>> MapJsonAsync<T>(this Task<HttpResponseMessage> taskMessage) where T : class =>
        _MapResultAsync<T, WrappedResult<T>>(taskMessage,
            fnMapResult: WrappedResult.Create,
            fnCreateFailure: WrappedResult.Fail<T>);

    /// <summary>
    /// If controller returns <see cref="T"/>, must handle as <see cref="WrappedResult{T}"/> as transport layer could fail.
    /// Also defines <see cref="fnIsValid"/> as structs initialise empty so further checking may be required to determine if object valid.
    /// </summary>
    public static Task<WrappedResult<T>> MapJsonStructAsync<T>(this Task<HttpResponseMessage> taskMessage, Func<T, bool> fnIsValid) where T : struct =>
        _MapResultAsync<T, WrappedResult<T>>(taskMessage,
            fnMapResult: x => fnIsValid(x) ? WrappedResult.Create(x) : WrappedResult.Fail<T>("Could not map type"),
            fnCreateFailure: WrappedResult.Fail<T>);

    /// <summary> If controller returns <see cref="Result"/> </summary>
    public static Task<Result> MapBasicResultAsync(this Task<HttpResponseMessage> taskMessage) =>
        _MapResultAsync<Result, Result>(taskMessage,
            fnMapResult: x => x,
            fnCreateFailure: Result.Fail);

    /// <summary> If controller returns <see cref="WrappedResult{T}"/> </summary>
    public static Task<WrappedResult<T>> MapWrappedResultAsync<T>(this Task<HttpResponseMessage> taskMessage) =>
        _MapResultAsync<WrappedResult<T>, WrappedResult<T>>(taskMessage,
            fnMapResult: x => x,
            fnCreateFailure: WrappedResult.Fail<T>);

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