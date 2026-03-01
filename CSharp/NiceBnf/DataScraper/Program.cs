using DataModel;

var loadedPre = await LoadDrugsAsync();
Console.WriteLine($"Loaded Pre: {loadedPre.Count}");

using var scraper = new DataScraper.DataScraper();

var allDrugs = await scraper.GetDrugSlugsAsync();

Console.WriteLine($"Drugs: {allDrugs.Count}");
//foreach (var slug in allDrugs) Console.WriteLine(slug);

var checkDrugs = new[] { "/drugs/amoxicillin/" }
    .Concat(allDrugs.Take(3))
    .Concat(allDrugs.Reverse().Take(3))
    .ToArray();

foreach (var drugSlug in checkDrugs)
{
    Console.WriteLine($"Fetching: {drugSlug}");

    // Scrape a single drug as a demo (amoxicillin has multiple indications, routes, and patient groups).
    var drug = await scraper.ScrapeDrugAsync(drugSlug) ?? throw new ArgumentException($"Could not load drug: {drugSlug}");
    
    var rawFileName = drugSlug.Split("/").Last(x => x.Any());
    var fileName = $"{Capitalise(rawFileName)}.json";
    
    var definitionPath = Path.Combine(FileLoader.GetDefinitionsPath(), fileName);
    await File.WriteAllTextAsync(definitionPath, FormatJson(drug, sanitiseCharacters: false));

    var exportPath = Path.Combine(FileLoader.GetRootDirectory(), "DataScraper", "Sanitised", fileName);
    await File.WriteAllTextAsync(exportPath, FormatJson(drug, sanitiseCharacters: true));
}

var loadedPost = await LoadDrugsAsync();
Console.WriteLine($"Loaded Post: {loadedPost.Count}");

return;

static string FormatJson(Drug drug, bool sanitiseCharacters)
{
    var json = JsonHelper.SerializeIndented(drug);
    return sanitiseCharacters
        ? json
            .Replace("\\u2009", "_")
            .Replace("\\u2002", "__")
            .Replace("POM__(", "POM (")
            .Replace("\\u00A3", "£")
            .Replace("\\u00A0", "|")
            .Replace("\\u0026", "&")
            .Replace("\\u2013", "-")
            .Replace("\\u2014", "-")
            .Replace("\\u0027", "'")
        : json;
}

static string Capitalise(string text) =>
    text[0].ToString().ToUpper() + text[1..];

static Task<IReadOnlyList<Drug>> LoadDrugsAsync() =>
    new ModelLoader().LoadDrugsAsync();