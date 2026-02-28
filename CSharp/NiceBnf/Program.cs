using NiceBnf;
using System.Text.Json;

using var scraper = new DataScraper();

var allDrugs = await scraper.GetDrugSlugsAsync();

Console.WriteLine("Drugs:");
foreach (var slug in allDrugs)
{
    Console.WriteLine(slug);
}

if (bool.Parse("false"))
{
    // Scrape a single drug as a demo (amoxicillin has multiple indications, routes, and patient groups).
    var drug = await scraper.ScrapeDrugAsync("/drugs/amoxicillin/");
    var json = JsonSerializer.Serialize(drug!, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine(json);
}
