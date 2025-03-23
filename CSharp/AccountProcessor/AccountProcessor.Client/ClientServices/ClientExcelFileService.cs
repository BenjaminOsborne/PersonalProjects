using System.Collections.Immutable;
using System.Net.Http.Json;
using AccountProcessor.Core;
using AccountProcessor.Core.Services;

namespace AccountProcessor.Client.ClientServices;

public enum AccountType
{
    Empty = 0,
    CoopBank,
    SantanderCreditCard
}

public interface IClientExcelFileService
{
    Task<WrappedResult<byte[]>> ExtractTransactionsAsync(Stream inputStream, string contentType, AccountType accountType);
    Task<WrappedResult<ImmutableArray<Transaction>>> LoadTransactionsAsync(Stream inputStream);
    Task<WrappedResult<byte[]>> CategoriseTransactionsAsync(CategoriseRequest request);
}

public class ClientExcelFileService(HttpClient httpClient) : IClientExcelFileService
{
    public Task<WrappedResult<byte[]>> ExtractTransactionsAsync(Stream inputStream, string contentType, AccountType accountType) =>
        httpClient.PostToUrlWithStreamContentAsync(
            relativeUrl: $"excelfile/extracttransactions/{accountType}",
            apiParameter: "file",
            inputStream,
            "arbitraryFileName",
            contentType)
            .MapFileByesAsync();

    public Task<WrappedResult<ImmutableArray<Transaction>>> LoadTransactionsAsync(Stream inputStream) =>
        httpClient.PostToUrlWithStreamContentAsync(
                relativeUrl: "excelfile/loadtransactions",
                apiParameter: "file",
                inputStream,
                "arbitraryFileName",
                ContentConstants.ExcelContentType)
            .MapJsonStructAsync<ImmutableArray<Transaction>>(fnIsValid: arr => !arr.IsDefault);

    public Task<WrappedResult<byte[]>> CategoriseTransactionsAsync(CategoriseRequest request) =>
        httpClient.PostAsJsonAsync("excelfile/exporttransactions", request)
            .MapFileByesAsync();
}