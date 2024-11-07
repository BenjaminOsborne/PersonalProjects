using CsvHelper;
using OfficeOpenXml;
using System.Globalization;

namespace AccountProcessor.Components.Services
{
    public interface IExcelFileRunner
    {
        Task Run();
    }

    public class ExcelFileRunner : IExcelFileRunner
    {
        private class CsvRow
        {
            [CsvHelper.Configuration.Attributes.Format("dd/MM/yyyy")]
            public DateOnly Date { get; init; }
            
            public string Description { get; init; }
            
            public string Type { get; init; }
            
            [CsvHelper.Configuration.Attributes.Name("Money In")]
            public decimal? MoneyIn { get; init; }
            
            [CsvHelper.Configuration.Attributes.Name("Money Out")]
            public decimal? MoneyOut { get; init; }

            public decimal Balance { get; init; }
        }

        public async Task Run()
        {
            try
            {
                var csvPath = @"C:\Users\Ben\Desktop\TransactionFiles\RawDownload.csv";
                var excelPath = @"C:\Users\Ben\Desktop\TransactionFiles\RawDownloadExcel.xlsx";

                var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = _ => { },
                };

                using (var reader = new StreamReader(csvPath))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<CsvRow>().ToArray();
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var excel = new ExcelPackage();
                await excel.LoadAsync(new FileInfo(excelPath));
                
                var found = excel.Workbook.Worksheets.Single();
                var rows = found.Rows.ToArray();
                var l = rows.Length;
                //await excel.LoadAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
