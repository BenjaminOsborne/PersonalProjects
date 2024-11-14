using CsvHelper;
using OfficeOpenXml;
using System.Collections.Immutable;
using System.Globalization;

namespace AccountProcessor.Components.Services
{
    public interface IExcelFileRunner
    {
        Task<WrappedResult<byte[]>> ReverseCsvTransactions(Stream inputCsv);
        Task<WrappedResult<ImmutableArray<Transaction>>> LoadTransactions(Stream inputExcel);
    }

    public class ExcelFileRunner : IExcelFileRunner
    {
        private class TransactionRow
        {
            public const string DateFormat = "dd/MM/yyyy";

            [CsvHelper.Configuration.Attributes.Format(DateFormat)]
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

        private static WrappedResult<ImmutableArray<TransactionRow>> _ExtractRows(Stream inputCsv)
        {
            try
            {
                var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = _ => { },
                };

                using var reader = new StreamReader(inputCsv);
                using var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<TransactionRow>().ToImmutableArray();

                TransactionRow? previous = null;
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

            static WrappedResult<ImmutableArray<TransactionRow>> Fail(string error) =>
                WrappedResult.Fail<ImmutableArray<TransactionRow>>(error);

            static WrappedResult<ImmutableArray<TransactionRow>> FailOnRow(string error, int rowNumber) =>
                WrappedResult.Fail<ImmutableArray<TransactionRow>>($"{error}. Row: {rowNumber}");
        }

        private static readonly ImmutableArray<string> _expectedHeaders = ["Date", "Description", "Type", "Money In", "Money Out", "Balance"];

        private static async Task<byte[]> _WriteRowsToExcel(ImmutableArray<TransactionRow> allRows)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var excel = new ExcelPackage();

            var added = excel.Workbook.Worksheets.Add("Default");

            _expectedHeaders
                .SelectWithIndexes()
                .ForEach(p => added.Cells[1, p.Index + 1].Value = p.Value);

            for (int r = 0; r < allRows.Length; r++)
            {
                var row = allRows[r];
                var rowNum = r + 2;
                var col = 0;
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

        public async Task<WrappedResult<ImmutableArray<Transaction>>> LoadTransactions(Stream inputExcel)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var excel = new ExcelPackage();
                await excel.LoadAsync(inputExcel);
                
                var found = excel.Workbook.Worksheets.Single();

                var titles = Enumerable.Range(1, 6).Select(col => found.Cells[1, col].Value).ToArray();
                if(!titles.SequenceEqual(_expectedHeaders))
                {
                    return FailWith($"Title row should be: {_expectedHeaders.ToJoinedString(",")}");
                }

                var transactions = new List<Transaction>();

                //found.Row

                foreach (var rowNum in Enumerable.Range(2, found.Dimension.Rows - 1))
                {
                    var date = _TryParseDate(found.Cells[rowNum, 1].Value);
                    var description = found.Cells[rowNum, 2].Value?.ToString();
                    var moneyIn = _TryParseDecimal(found.Cells[rowNum, 4].Value);
                    var moneyOut = _TryParseDecimal(found.Cells[rowNum, 5].Value);
                    if (date.HasValue == false || description.IsNullOrEmpty() || (moneyIn.HasValue == false && moneyOut.HasValue == false))
                    {
                        return FailWith($"Row {rowNum} has invalid data");
                    }
                    transactions.Add(new Transaction(date.Value, description!, moneyIn ?? (-moneyOut!.Value)));
                }

                var ordered = transactions.OrderBy(x => x.Date).ToImmutableArray();
                return WrappedResult.Create(ordered);
                
            }
            catch (Exception ex)
            {
                return FailWith($"Exception: {ex}");
            }

            static WrappedResult<ImmutableArray<Transaction>> FailWith(string error) =>
                WrappedResult.Fail<ImmutableArray<Transaction>>(error);
        }

        private static DateOnly? _TryParseDate(object val) =>
            DateOnly.TryParseExact(val?.ToString(), TransactionRow.DateFormat, out var date) ? date : null;

        private static decimal? _TryParseDecimal(object val) =>
            decimal.TryParse(val?.ToString(), out var moneyIn) ? moneyIn : null;
    }
}
