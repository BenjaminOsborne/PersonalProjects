using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace AccountProcessor.Components.Pages;

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

    public static async Task<WrappedResult<T>> MapJsonAsync<T>(this HttpResponseMessage message)
    {
        if (!message.IsSuccessStatusCode)
        {
            var error = await message.Content.ReadAsStringAsync();
            return WrappedResult.Fail<T>($"{message.StatusCode}: {error}");
        }
        var json = await message.Content.ReadAsStringAsync();
        return WrappedResult.Create(JsonHelper.Deserialise<T>(json, ignoreCase: true)!);
    }

    public static async Task<WrappedResult<byte[]>> MapFileByesAsync(this HttpResponseMessage message)
    {
        if (!message.IsSuccessStatusCode)
        {
            var error = await message.Content.ReadAsStringAsync();
            return WrappedResult.Fail<byte[]>($"{message.StatusCode}: {error}");
        }
        var bytes = await message.Content.ReadAsByteArrayAsync();
        return WrappedResult.Create(bytes);
    }
}