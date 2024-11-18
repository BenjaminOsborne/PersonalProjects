using CsvHelper;
using OfficeOpenXml;
using System.Collections.Immutable;
using System.Drawing;
using System.Globalization;

namespace AccountProcessor.Components.Services
{
    public interface IExcelFileHandler
    {
        /// <summary> Takes input .csv stream (from co-op bank file) and returns excel byte[] for download with transactions in ascending order </summary>
        /// <remarks> Assumes the co-op CSV format is in reverse order </remarks>
        Task<WrappedResult<byte[]>> CoopBank_ExtractCsvTransactionsToExcel(Stream inputCsv);

        /// <summary> Takes input .xlsx stream (from santander bank file) and returns excel byte[] for download with transactions in ascending order </summary>
        Task<WrappedResult<byte[]>> Santander_ExtractExcelTransactionsToExcel(Stream inputExcel);
        
        /// <summary> Loads transactions from excel file with these columns: Date, Description, Type, Money In, Money Out </summary>
        Task<WrappedResult<ImmutableArray<Transaction>>> LoadTransactionsFromExcel(Stream inputExcel);

        /// <summary> Exports categorised transactions to excel with 2 columns ("Date - Description", "Amount") for each category (ordered section within)</summary>
        /// <remarks> Output is then ready to be pasted into LifeOrganisation summary sheet </remarks>
        Task<WrappedResult<byte[]>> ExportCategorisedTransactionsToExcel(CategorisationResult result);
    }

    public class ExcelFileHandler : IExcelFileHandler
    {
        /// <summary> Note: "Balance" included in co-op report, but not required for loading transactions out to process </summary>
        private static readonly ImmutableArray<string> _transactionsRequiredColumnHeaders = ["Date", "Description", "Type", "Money In", "Money Out"];
        
        private static readonly ImmutableArray<string> _writeTransactionColumnHeaders = _transactionsRequiredColumnHeaders.Add("Balance");
        
        private const string _dateFormatExcelExport = "dd/MM/yyyy";

        #region CoopBank_ReverseCsvTransactionsToExcel

        private class CoopBankCsvRow
        {
            [CsvHelper.Configuration.Attributes.Format("yyyy-MM-dd")]
            public DateOnly Date { get; init; }

            public string Description { get; init; } = null!;

            public string Type { get; init; } = null!;

            [CsvHelper.Configuration.Attributes.Name("Money In")]
            public decimal? MoneyIn { get; init; }

            [CsvHelper.Configuration.Attributes.Name("Money Out")]
            public decimal? MoneyOut { get; init; }

            public decimal Balance { get; init; }
        }

        public async Task<WrappedResult<byte[]>> CoopBank_ExtractCsvTransactionsToExcel(Stream inputCsv)
        {
            var rows = _CoopBank_ExtractRowsFromCsvStream(inputCsv);
            if (!rows.IsSuccess)
            {
                return rows.MapFail<byte[]>();
            }
            var allRows = rows.Result!
                .Reverse() //Coop has them newest to oldest - should be output in Date ascending order
                .ToImmutableArray(x => new TransactionRow(x.Date, x.Description, x.Type, x.MoneyIn, x.MoneyOut, x.Balance));
            var resultBytes = await _WriteTransactionRowsToExcel(allRows);
            return WrappedResult.Create(resultBytes);
        }

        private static WrappedResult<ImmutableArray<CoopBankCsvRow>> _CoopBank_ExtractRowsFromCsvStream(Stream inputCsv)
        {
            try
            {
                var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = _ => { },
                };

                using var reader = new StreamReader(inputCsv);
                using var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<CoopBankCsvRow>().ToImmutableArray();

                CoopBankCsvRow? previous = null;
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

            static WrappedResult<ImmutableArray<CoopBankCsvRow>> Fail(string error) =>
                WrappedResult.Fail<ImmutableArray<CoopBankCsvRow>>(error);

            static WrappedResult<ImmutableArray<CoopBankCsvRow>> FailOnRow(string error, int rowNumber) =>
                WrappedResult.Fail<ImmutableArray<CoopBankCsvRow>>($"{error}. Row: {rowNumber}");
        }

        private static (ExcelPackage excel, ExcelWorksheet worksheet) _CreateNewExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var excel = new ExcelPackage();
            var worksheet = excel.Workbook.Worksheets.Add("Default");
            return (excel, worksheet);
        }

        #endregion

        #region Santander_ExtractExcelTransactionsToExcel
        
        public async Task<WrappedResult<byte[]>> Santander_ExtractExcelTransactionsToExcel(Stream inputExcel)
        {
            var result = await _Santander_ExtractRowsFromExcelStream(inputExcel);
            if (!result.IsSuccess)
            {
                return result.MapFail<byte[]>();
            }

            var resultBytes = await _WriteTransactionRowsToExcel(result.Result);
            return WrappedResult.Create(resultBytes);
        }

        private static async Task<WrappedResult<ImmutableArray<TransactionRow>>> _Santander_ExtractRowsFromExcelStream(Stream inputExcel)
        {
            var excelFromStream = await _LoadFromExcelStream(inputExcel);
            var found = excelFromStream.Workbook.Worksheets.Single();

            var columnNums = ImmutableArray.Create(2, 4, 6, 8, 10);
            var expectedHeaders = ImmutableArray.Create("Date", "Card", "Description", "Money in", "Money Out");
            var titleRow = 4;
            var transactionsStartRow = 6;

            var titles = columnNums
                .ToImmutableArray(col => found.Cells[titleRow, col].Value.ToString());
            if (!titles.SequenceEqual(expectedHeaders))
            {
                return Fail($"Expecting headers: {expectedHeaders.ToJoinedString(", ")}");
            }

            var transactionRows = new List<TransactionRow>();
            foreach (var rowNum in Enumerable.Range(transactionsStartRow, found.Dimension.Rows - transactionsStartRow + 1))
            {
                var date = _TryParseDate(found.Cells[rowNum, columnNums[0]].Value);
                var description = found.Cells[rowNum, columnNums[2]].Value?.ToString();
                var moneyIn = _TryParseDecimal(found.Cells[rowNum, columnNums[3]].Value);
                var moneyOut = _TryParseDecimal(found.Cells[rowNum, columnNums[4]].Value);
                if (date == null && description == null && moneyIn == null && moneyOut == null)
                {
                    continue; //entirely empty row is OK
                }
                if (date.HasValue == false || description == null || (moneyIn == null && moneyOut == null))
                {
                    return Fail($"Invalid row - missing data: {rowNum}");
                }

                transactionRows.Add(new TransactionRow(date.Value, description, "Credit Card", moneyIn, moneyOut, Balance: null));
            }
            
            return WrappedResult.Create(transactionRows.ToImmutableArray());

            static WrappedResult<ImmutableArray<TransactionRow>> Fail(string error) =>
                WrappedResult.Fail<ImmutableArray<TransactionRow>>(error);
        }

        #endregion

        #region LoadTransactionsFromExcel

        public async Task<WrappedResult<ImmutableArray<Transaction>>> LoadTransactionsFromExcel(Stream inputExcel)
        {
            try
            {
                var excel = await _LoadFromExcelStream(inputExcel);

                var found = excel.Workbook.Worksheets.Single();

                var required = _transactionsRequiredColumnHeaders;
                var titles = Enumerable.Range(1, required.Length)
                    .ToImmutableArray(col => found.Cells[1, col].Value);
                if (!titles.SequenceEqual(required))
                {
                    return FailWith($"Title row should be: {required.ToJoinedString(",")}");
                }

                var transactions = new List<Transaction>();

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

        #endregion

        #region ExportCategorisedTransactionsToExcel

        public async Task<WrappedResult<byte[]>> ExportCategorisedTransactionsToExcel(CategorisationResult result)
        {
            try
            {
                var (excel, worksheet) = _CreateNewExcel();

                var catSummary = _ToWriteSummary(result);

                for (int catNx = 0; catNx < catSummary.Length; catNx++)
                {
                    var cat = catSummary[catNx];
                    var row = 1;
                    var col = catNx * 2 + 1;

                    var categoryTotal = cat.Sections
                        .SelectMany(x => x.Transactions)
                        .Sum(x => x.Transaction.Amount);
                    SetValue(row, col, cat.Category.Name, isBold: true);
                    SetValue(row++, col+1, categoryTotal, isBold: true, numberFormat: _excelCurrencyNumberFormat); //net amount

                    foreach (var sec in cat.Sections)
                    {
                        SetValue(row++, col, sec.Section.Name, isBold: true);

                        foreach (var ts in sec.Transactions)
                        {
                            var trans = ts.Transaction;
                            var description = ts.OverrideDescription ?? trans.Description;
                            SetValue(row, col, $"{trans.Date:dd}/{trans.Date:MM} - {description}");
                            SetValue(row++, col + 1, trans.Amount, numberFormat: _excelCurrencyNumberFormat);
                        }

                        row++; //Create space between sections
                    }

                    //Autofit for easy viewing
                    worksheet.Columns[col].AutoFit();
                    worksheet.Columns[col + 1].AutoFit();

                    //Set column borders
                    var border = worksheet.Columns[col + 1].Style.Border;
                    border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    border.Right.Color.SetColor(Color.Black);
                }

                //Net Total column (at end)
                var summaryCol = catSummary.Length * 2 + 1;
                var netOverall = catSummary
                    .SelectMany(x => x.Sections)
                    .SelectMany(x => x.Transactions)
                    .Sum(x => x.Transaction.Amount);
                SetValue(1, summaryCol, "Net", isBold: true);
                SetValue(1, summaryCol+1, netOverall, isBold: true, numberFormat: _excelCurrencyNumberFormat);
                worksheet.Columns[summaryCol].AutoFit();
                worksheet.Columns[summaryCol + 1].AutoFit();

                //Set first row border
                var rowBorder = worksheet.Rows[1].Style.Border;
                rowBorder.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                rowBorder.Bottom.Color.SetColor(Color.Black);

                void SetValue<T>(int rowNum, int colNum, T val, bool isBold = false, string? numberFormat = null)
                {
                    var cell = worksheet.Cells[rowNum, colNum];
                    cell.Value = val;
                    if (isBold)
                    {
                        cell.Style.Font.Bold = true;
                    }
                    if (numberFormat != null)
                    {
                        cell.Style.Numberformat.Format = numberFormat;
                    }
                }

                return WrappedResult.Create(await excel.GetAsByteArrayAsync());
            }
            catch (Exception ex)
            {
                return WrappedResult.Fail<byte[]>(ex.ToString());
            }
        }

        private ImmutableArray<CategorySummary> _ToWriteSummary(CategorisationResult result)
        {
            //All unmatched go as "Manual" section
            var manualHeader = CategoryHeader.Manual;
            var unmatched = result.UnMatched
                .Select(x => (
                    Transaction: new TransactionSummary(x.Transaction, OverrideDescription: null),
                    Section: new SectionHeader(int.MaxValue, "UnMatched", manualHeader, null)));
            var map = result.Matched
                .Select(x => (
                    Transaction: new TransactionSummary(x.Transaction, OverrideDescription: x.SectionMatch.Match.GetDescription()),
                    x.SectionMatch.Section))
                .Concat(unmatched)
                .GroupBy(x => x.Section.Parent.Name)
                .ToImmutableDictionary(x => x.Key, x => x.ToImmutableArray());
            
            //Ensures every category present, even if no transactions
            return CategoryHeader.AllValues
                .OrderBy(x => x.Order)
                .Select(c => (Cat: c, Vals: map.TryGetStruct(c.Name)))
                .ToImmutableArray(cat =>
                {
                    var category = cat.Cat;

                    var sections = cat.Vals?
                        .GroupBy(x => x.Section.GetKey())
                        .OrderBy(grp => grp.Key.order)
                        .ToImmutableArray(grp =>
                        {
                            var transactions = grp.ToImmutableArray(x => x.Transaction);
                            return new SectionSummary(grp.First().Section, transactions);
                        })
                        ?? ImmutableArray<SectionSummary>.Empty;

                    return new CategorySummary(category, sections);
                });
        }

        private const string _excelCurrencyNumberFormat = """
            "£"#,##0.00;[Red]\-"£"#,##0.00
            """;

        private record CategorySummary(CategoryHeader Category, ImmutableArray<SectionSummary> Sections);
        private record SectionSummary(SectionHeader Section, ImmutableArray<TransactionSummary> Transactions);
        private record TransactionSummary(Transaction Transaction, string? OverrideDescription);

        #endregion

        #region General Helpers

        private record TransactionRow(DateOnly Date, string Description, string Type, decimal? MoneyIn, decimal? MoneyOut, decimal? Balance);

        private static async Task<byte[]> _WriteTransactionRowsToExcel(ImmutableArray<TransactionRow> allRows)
        {
            var (excel, worksheet) = _CreateNewExcel();

            _writeTransactionColumnHeaders
                .SelectWithIndexes()
                .ForEach(p => worksheet.Cells[1, p.Index + 1].Value = p.Value);

            for (int r = 0; r < allRows.Length; r++)
            {
                var row = allRows[r];
                var rowNum = r + 2;
                var col = 0;
                worksheet.Cells[rowNum, ++col].Value = row.Date.ToString(_dateFormatExcelExport);
                worksheet.Cells[rowNum, ++col].Value = row.Description;
                worksheet.Cells[rowNum, ++col].Value = row.Type;
                worksheet.Cells[rowNum, ++col].Value = row.MoneyIn;
                worksheet.Cells[rowNum, ++col].Value = row.MoneyOut;
                worksheet.Cells[rowNum, ++col].Value = row.Balance;
            }

            //Autofit for easy viewing
            worksheet.Columns.ForEach(x => x.AutoFit());

            return await excel.GetAsByteArrayAsync();
        }

        private static DateOnly? _TryParseDate(object val) =>
            val is DateTime dt
                ? DateOnly.FromDateTime(dt)
                : DateOnly.TryParseExact(val?.ToString(), _dateFormatExcelExport, out var date) ? date : null;

        private static decimal? _TryParseDecimal(object val) =>
            decimal.TryParse(val?.ToString(), out var moneyIn) ? moneyIn : null;

        private static async Task<ExcelPackage> _LoadFromExcelStream(Stream inputExcel)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var excel = new ExcelPackage();
            await excel.LoadAsync(inputExcel);
            return excel;
        }

        #endregion
    }
}
