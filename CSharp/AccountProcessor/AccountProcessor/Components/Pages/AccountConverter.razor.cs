using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Immutable;
using AccountProcessor.ClientServices;
using AccountProcessor.Services;

namespace AccountProcessor.Components.Pages;

public partial class AccountConverter
{
    private record AccountTypeData(AccountType Type, string Name, string FileAccept, string Description)
    {
        public string Id => Type.ToString();

        /// <summary> Required for selector display </summary>
        public override string ToString() => Name;
    }

    [Inject] private Microsoft.JSInterop.IJSRuntime _jsInterop { get; init; } = null!;
    [Inject] private IClientExcelFileService _excelFileService { get; init; } = null!;

    private static readonly ImmutableArray<AccountTypeData> SelectableAccountTypes = ImmutableArray.Create(
        new AccountTypeData(AccountType.Empty, "Choose Account Type", "", Description:
            "Please select account type"),
        new AccountTypeData(AccountType.CoopBank, "Coop Bank", ".csv", Description:
            "Takes a raw CSV from Co-op Bank, reverses the transactions and downloads in standard format"),
        new AccountTypeData(AccountType.SantanderCreditCard, "Santander Credit Card", ".xlsx", Description:
            "Takes a converted xlsx from Santander and downloads in standard format. (Must be saved to xlsx manually)"));

    private AccountTypeData SelectedAccountType = SelectableAccountTypes[0];
    private bool IsAccountTypeSelected => SelectedAccountType.Type != AccountType.Empty;
    
    [Parameter]
    public required Func<WrappedResult<byte[]>, Task> OnAccountFileConverted { get; init; }

    private async Task ProcessAccountFile(IBrowserFile? bf)
    {
        var accountType = SelectedAccountType.Type;
        if (bf == null || accountType == AccountType.Empty)
        {
            return;
        }

        var result = await _excelFileService.ExtractTransactionsAsync(bf.OpenReadStream(), contentType: bf.ContentType, accountType);
        var uniqueStamp = DateTime.MinValue.Add(DateTime.Now - new DateTime(2024, 11, 1)).Ticks;
        await _OnFileResultDownloadBytes(
            result: result,
            fileName: $"{accountType}_Extract_{bf.Name}_{uniqueStamp}{FileConstants.ExtractedTransactionsFileExtension}"); //Note: The ".extract." aspect enables further limitation just to these files on the file picker!
    }

    private async Task _OnFileResultDownloadBytes(WrappedResult<byte[]> result, string fileName)
    {
        if (result.IsSuccess)
        {
            await _jsInterop.SaveAsFileAsync(fileName, result.Result!);
        }

        await OnAccountFileConverted.Invoke(result);
    }
}