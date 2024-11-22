using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Immutable;

namespace AccountProcessor.Components.Pages
{
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
        }

        [Inject]
        private IExcelFileHandler _excelFileHandler { get; init; }
        [Inject]
        private Microsoft.JSInterop.IJSRuntime _jsInterop { get; init; }

        private static readonly ImmutableArray<AccountTypeData> SelectableAccountTypes = ImmutableArray.Create(
                new AccountTypeData(AccountType.InitialSelect, "Choose Account Type", "", Description:
                    "Please select account type"),
                new AccountTypeData(AccountType.CoopBank, "Coop Bank", ".csv", Description:
                    "Takes a raw CSV from Co-op Bank, reverses the transactions and downloads in standard format"),
                new AccountTypeData(AccountType.SantanderCreditCard, "Santander Credit Card", ".xlsx", Description:
                    "Takes a converted xlsx from Santander and downloads in standard format. (Must be saved to xlsx manually)"));

        private AccountTypeData SelectedAccountType = SelectableAccountTypes[0];

        [Parameter]
        public required Action<Result> OnAccountFileConverted { get; init; }
        
        private void SetAccountType(string id) =>
            SelectedAccountType = SelectableAccountTypes.Single(x => x.Id == id);

        private Task ProcessAccountFile(InputFileChangeEventArgs e) =>
            (SelectedAccountType?.Type) switch
            {
                AccountType.CoopBank => _ProcessBankFileExtraction(e,
                    fnProcess: _excelFileHandler.CoopBank_ExtractCsvTransactionsToExcel,
                    filePrefix: "CoopBank_Extract"),
                AccountType.SantanderCreditCard => _ProcessBankFileExtraction(e,
                    fnProcess: _excelFileHandler.Santander_ExtractExcelTransactionsToExcel,
                    filePrefix: "Santander_Extract"),
                _ => Task.CompletedTask,
            };

        private async Task _ProcessBankFileExtraction(
            InputFileChangeEventArgs e,
            Func<Stream, Task<WrappedResult<byte[]>>> fnProcess,
            string filePrefix)
        {
            using var inputStream = await e.CopyToMemoryStreamAsync();
            var uniqueStamp = DateTime.MinValue.Add(DateTime.Now - new DateTime(2024, 11, 1)).Ticks;
            await _OnFileResultDownloadBytes(
                result: await fnProcess(inputStream),
                fileName: $"{filePrefix}_{e.File.Name}_{uniqueStamp}{FileConstants.ExtractedTransactionsFileExtension}"); //Note: The ".extract." aspect enables further limitation just to these files on the file picker!
        }

        private Task _OnFileResultDownloadBytes(WrappedResult<byte[]> result, string fileName)
        {
            OnAccountFileConverted.Invoke(result);
            return result.IsSuccess
                ? _jsInterop.SaveAsFileAsync(fileName, result.Result!)
                : Task.CompletedTask;
        }
    }
}
