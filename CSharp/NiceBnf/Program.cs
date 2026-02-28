using NiceBnf;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

using var scraper = new DataScraper();

var allDrugs = await scraper.GetDrugSlugsAsync();

Console.WriteLine("Drugs:");
foreach (var slug in allDrugs)
{
    Console.WriteLine(slug);
}

var checkDrugs = new[] { "/drugs/amoxicillin/" }
    .Concat(allDrugs.Take(3))
    .Concat(allDrugs.Reverse().Take(3))
    .ToArray();

foreach (var drugSlug in checkDrugs)
{
    // Scrape a single drug as a demo (amoxicillin has multiple indications, routes, and patient groups).
    var drug = await scraper.ScrapeDrugAsync(drugSlug) ?? throw new ArgumentException($"Could not load drug: {drugSlug}");
    var json = FormatJson(drug);
    var rawFileName = drugSlug.Split("/").Last(x => x.Any());
    var fileName = rawFileName[0].ToString().ToUpper() + rawFileName[1..];
    var filePath = Path.Combine(FileLoader.GetCurrentDir(), "Exports", $"{fileName}.json");
    await File.WriteAllTextAsync(filePath, json);
}

return;

static string FormatJson(Drug drug) =>
    JsonSerializer.Serialize(drug, JsonSerializerOptionsSettings.Indented)
        .Replace("\\u2009", "_")
        .Replace("\\u2002", "__")
        .Replace("\\u00A3", "£")
        .Replace("\\u00A0", "|")
        .Replace("\\u0026", "&")
        .Replace("\\u2013", "-")
        .Replace("\\u2014", "-")
        .Replace("\\u0027", "'")
    ;

public static class FileLoader
{
    public static string GetPathRelativeToExecuting(params string[] relativePath)
    {
        var assemblyLoc = Assembly.GetExecutingAssembly().Location;
        var execDir = Path.GetDirectoryName(assemblyLoc)!;
        return relativePath.Length switch
        {
            0 => execDir,
            1 => Path.Combine(execDir, relativePath[0]),
            _ => Path.Combine(new[] { execDir }.Concat(relativePath).ToArray())
        };
    }

    public static string GetCurrentDir()
    {
        var dir = new FileInfo(_GetCurrentFilePath()).Directory!;
        while (dir.Name != "NiceBnf")
        {
            dir = dir.Parent!;
        }
        return dir.FullName;
    }

    private static string _GetCurrentFilePath([CallerFilePath] string path = "") => path;
}

public static class JsonSerializerOptionsSettings
{
    public static JsonSerializerOptions Indented { get; } = new JsonSerializerOptions { WriteIndented = true };
}