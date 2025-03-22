using AccountProcessor.Components.Controllers;
using System.Collections.Immutable;
using AccountProcessor.Components.Services;

namespace AccountProcessor.Components.ClientServices;

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
    public async Task<WrappedResult<byte[]>> ExtractTransactionsAsync(Stream inputStream, string contentType, AccountType accountType)
    {
        var message = await httpClient.PostToUrlWithStreamContentAsync(
            relativeUrl: $"excelfile/extracttransactions/{accountType}",
            apiParameter: "file",
            inputStream,
            "arbitraryFileName",
            contentType);
        return await message.MapFileByesAsync();
    }

    public async Task<WrappedResult<ImmutableArray<Transaction>>> LoadTransactionsAsync(Stream inputStream)
    {
        var message = await httpClient.PostToUrlWithStreamContentAsync(
            relativeUrl: "excelfile/loadtransactions",
            apiParameter: "file",
            inputStream,
            "arbitraryFileName",
            ExcelFileController.ExcelContentType);
        return await message.MapJsonStructAsync<ImmutableArray<Transaction>>(fnIsValid: arr => !arr.IsDefault);
    }

    public async Task<WrappedResult<byte[]>> CategoriseTransactionsAsync(CategoriseRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("excelfile/exporttransactions", request);
        return await response.MapFileByesAsync();
    }
}