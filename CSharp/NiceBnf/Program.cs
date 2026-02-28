using NiceBnf;
using System.Text.Json;

using var scraper = new DataScraper();

// Scrape a single drug as a demo (amoxicillin has multiple indications, routes, and patient groups).
var drug = await scraper.ScrapeDrugAsync("/drugs/amoxicillin/");

if (drug is null)
{
    Console.WriteLine("Failed to scrape drug.");
    return;
}

var json = JsonSerializer.Serialize(drug, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine(json);
