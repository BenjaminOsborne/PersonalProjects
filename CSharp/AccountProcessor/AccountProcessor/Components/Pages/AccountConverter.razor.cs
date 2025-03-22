using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Immutable;
using AccountProcessor.Components.ClientServices;
using AccountProcessor.Components.Controllers;

namespace AccountProcessor.Components.Pages;

public partial class AccountConverter
{
    private enum AccountType
    {
        InitialSelect = 0,
        CoopBank,
        SantanderCreditCard
    }

    private record AccountTypeData(AccountType Type, string Name, string FileAccept, string Description)
    {
        public string Id => Type.ToString();

        /// <summary> Required for selector display </summary>
        public override string ToString() => Name;
    }

    [Inject] private Microsoft.JSInterop.IJSRuntime _jsInterop { get; init; } = null!;
    [Inject] private IClientExcelFileService _excelFileService { get; init; } = null!;

    private static readonly ImmutableArray<AccountTypeData> SelectableAccountTypes = ImmutableArray.Create(
        new AccountTypeData(AccountType.InitialSelect, "Choose Account Type", "", Description:
            "Please select account type"),
        new AccountTypeData(AccountType.CoopBank, "Coop Bank", ".csv", Description:
            "Takes a raw CSV from Co-op Bank, reverses the transactions and downloads in standard format"),
        new AccountTypeData(AccountType.SantanderCreditCard, "Santander Credit Card", ".xlsx", Description:
            "Takes a converted xlsx from Santander and downloads in standard format. (Must be saved to xlsx manually)"));

    private AccountTypeData SelectedAccountType = SelectableAccountTypes[0];

    [Parameter]
    public required Func<WrappedResult<byte[]>, Task> OnAccountFileConverted { get; init; }
        
    private Task ProcessAccountFile(IBrowserFile? bf) =>
        SelectedAccountType.Type switch
        {
            AccountType.CoopBank => _ProcessBankFileExtraction(bf, ExcelFileController.Banks.CoopBank),
            AccountType.SantanderCreditCard => _ProcessBankFileExtraction(bf, ExcelFileController.Banks.Santander),
            _ => Task.CompletedTask,
        };

    private async Task _ProcessBankFileExtraction(
        IBrowserFile? bf,
        string bank)
    {
        if (bf == null)
        {
            return;
        }
        
        var result = await _excelFileService.ExtractTransactionsAsync(bf, bank);
        var uniqueStamp = DateTime.MinValue.Add(DateTime.Now - new DateTime(2024, 11, 1)).Ticks;
        await _OnFileResultDownloadBytes(
            result: result,
            fileName: $"{bank}_Extract_{bf.Name}_{uniqueStamp}{FileConstants.ExtractedTransactionsFileExtension}"); //Note: The ".extract." aspect enables further limitation just to these files on the file picker!
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