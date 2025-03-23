using System.Collections.Immutable;
using AccountProcessor.Controllers;
using AccountProcessor.Services;

namespace AccountProcessor.ClientServices;

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
                ExcelFileController.ExcelContentType)
            .MapJsonStructAsync<ImmutableArray<Transaction>>(fnIsValid: arr => !arr.IsDefault);

    public Task<WrappedResult<byte[]>> CategoriseTransactionsAsync(CategoriseRequest request) =>
        httpClient.PostAsJsonAsync("excelfile/exporttransactions", request)
            .MapFileByesAsync();
}