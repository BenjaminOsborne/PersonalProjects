using AccountProcessor.Components.Controllers;
using System.Collections.Immutable;
using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace AccountProcessor.Components.ClientServices;

public interface IClientExcelFileService
{
    Task<WrappedResult<byte[]>> ExtractTransactionsAsync(IBrowserFile bf, string bank);
    Task<WrappedResult<ImmutableArray<Transaction>>> LoadTransactionsAsync(Stream inputStream);
    Task<WrappedResult<byte[]>> CategoriseTransactionsAsync(CategoriseRequest request);
}

public class ClientExcelFileService(HttpClient httpClient) : IClientExcelFileService
{
    public async Task<WrappedResult<byte[]>> ExtractTransactionsAsync(IBrowserFile bf, string bank)
    {
        var message = await httpClient.PostFileToUrlAsync(bf, relativeUrl: $"excelfile/extracttransactions/{bank}");
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
        return await message.MapJsonAsync<ImmutableArray<Transaction>>();
    }

    public async Task<WrappedResult<byte[]>> CategoriseTransactionsAsync(CategoriseRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("excelfile/exporttransactions", request);
        return await response.MapFileByesAsync();
    }
}