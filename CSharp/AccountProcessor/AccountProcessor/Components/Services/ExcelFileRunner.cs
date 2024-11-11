using CsvHelper;
using OfficeOpenXml;
using System.Collections.Immutable;
using System.Globalization;

namespace AccountProcessor.Components.Services
{
    public interface IExcelFileRunner
    {
        Task<WrappedResult<byte[]>> ReverseCsvTransactions(Stream inputCsv);
        Task Categorise(Stream inputExcel);
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

        public async Task<WrappedResult<byte[]>> ReverseCsvTransactions(Stream inputCsv)
        {
            var rows = _ExtractRows(inputCsv);
            if (!rows.IsSuccess)
            {
                return rows.MapFail<byte[]>();
            }
            var allRows = rows.Result!.Reverse().ToImmutableArray();
            var resultBytes = await _WriteRowsToExcel(allRows);
            return WrappedResult.Create(resultBytes);
        }

        private static WrappedResult<ImmutableArray<CsvRow>> _ExtractRows(Stream inputCsv)
        {
            try
            {
                var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = _ => { },
                };

                using var reader = new StreamReader(inputCsv);
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

        private static async Task<byte[]> _WriteRowsToExcel(ImmutableArray<CsvRow> allRows)
        {
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

            //Autofit for easy viewing
            added.Columns.ForEach(x => x.AutoFit());

            return await excel.GetAsByteArrayAsync();
        }

        public async Task Categorise(Stream inputExcel)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var excel = new ExcelPackage();
                await excel.LoadAsync(inputExcel);
                
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
