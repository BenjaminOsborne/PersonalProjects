using CsvHelper;
using OfficeOpenXml;
using System.Collections.Immutable;
using System.Globalization;

namespace AccountProcessor.Components.Services
{
    public interface IExcelFileRunner
    {
        Task<string> ReverseAndGenerate();
        Task Run();
    }

    public class ExcelFileRunner : IExcelFileRunner
    {
        private class CsvRow
        {
            [CsvHelper.Configuration.Attributes.Format("dd/MM/yyyy")]
            public DateOnly Date { get; init; }

            public string Description { get; init; } = null!;
            
            public string Type { get; init; } = null!;

            [CsvHelper.Configuration.Attributes.Name("Money In")]
            public decimal? MoneyIn { get; init; }
            
            [CsvHelper.Configuration.Attributes.Name("Money Out")]
            public decimal? MoneyOut { get; init; }

            public decimal Balance { get; init; }
        }

        public async Task<string> ReverseAndGenerate()
        {
            var csvPath = @"C:\Users\Ben\Desktop\TransactionFiles\RawDownload.csv";
            var rows = _ExtractRows(csvPath);
            if(!rows.IsSuccess)
            {
                return rows.Error!;
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var excel = new ExcelPackage();

            var added = excel.Workbook.Worksheets.Add("Default");

            var col = 0;
            added.Cells[1, ++col].Value = "Date";
            added.Cells[1, ++col].Value = "Description";
            added.Cells[1, ++col].Value = "Type";
            added.Cells[1, ++col].Value = "Money In";
            added.Cells[1, ++col].Value = "Money Out";
            added.Cells[1, ++col].Value = "Balance";

            var allRows = rows.Result!.Reverse().ToImmutableArray();
            for (int r = 0; r < allRows.Length; r++)
            {
                var row = allRows[r];
                var rowNum = r + 2;
                col = 0;
                added.Cells[rowNum, ++col].Value = row.Date.ToString("dd/MM/yyyy");
                added.Cells[rowNum, ++col].Value = row.Description;
                added.Cells[rowNum, ++col].Value = row.Type;
                added.Cells[rowNum, ++col].Value = row.MoneyIn;
                added.Cells[rowNum, ++col].Value = row.MoneyOut;
                added.Cells[rowNum, ++col].Value = row.Balance;
            }

            var excelPath = @"C:\Users\Ben\Desktop\TransactionFiles\RawDownload.xlsx";
            await excel.SaveAsAsync(new FileInfo(excelPath));

            return "Success!";
        }

        private static WrappedResult<ImmutableArray<CsvRow>> _ExtractRows(string csvPath)
        {
            try
            {
                var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = _ => { },
                };

                using var reader = new StreamReader(csvPath);
                using var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<CsvRow>().ToImmutableArray();

                CsvRow? previous = null;
                for (int nx = 0; nx < records.Length; nx++)
                {
                    var row = nx + 1;
                    var record = records[nx];
                    if (record.Description.IsNullOrEmpty())
                    {
                        return FailOnRow("Empty Description", row);
                    }
                    if (record.MoneyIn == null && record.MoneyOut == null)
                    {
                        return FailOnRow("Money In and Out null", row);
                    }
                    if (previous != null && previous.Date < record.Date)
                    {
                        return FailOnRow("Expecting to be newest to oldest", row);
                    }

                    if (previous != null)
                    {
                        var expectedEarlierBalance = previous.Balance + (previous.MoneyOut ?? 0) - (previous.MoneyIn ?? 0);
                        if(expectedEarlierBalance != record.Balance)
                        {
                            return FailOnRow("Mismatch expected balance", row);
                        }
                    }

                    previous = record;
                }

                return WrappedResult.Create(records);
            }
            catch (Exception ex)
            {
                return Fail(ex.ToString());
            }

            static WrappedResult<ImmutableArray<CsvRow>> Fail(string error) =>
                WrappedResult.Fail<ImmutableArray<CsvRow>>(error);

            static WrappedResult<ImmutableArray<CsvRow>> FailOnRow(string error, int rowNumber) =>
                WrappedResult.Fail<ImmutableArray<CsvRow>>($"{error}. Row: {rowNumber}");
        }

        public async Task Run()
        {
            try
            {
                var excelPath = @"C:\Users\Ben\Desktop\TransactionFiles\RawDownloadExcel.xlsx";
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var excel = new ExcelPackage();
                await excel.LoadAsync(new FileInfo(excelPath));
                
                var found = excel.Workbook.Worksheets.Single();
                var rows = found.Rows.ToArray();
                var l = rows.Length;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
