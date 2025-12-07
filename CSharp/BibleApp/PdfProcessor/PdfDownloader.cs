using System.Diagnostics;
using System.Net;

namespace PdfProcessor;

public static class SpurgeonSermonDownloader
{
    public static async Task DownloadSpuregonSermonsAsync()
    {
        var sw = Stopwatch.StartNew();

        var baseUrl = "https://www.spurgeongems.org/sermon";
        var filePrefix = "C:\\PdfDownload\\SpurgeonSermon_";

        //Contents page at baseUrl contains list of links
        var contents = await (await new HttpClient().GetAsync(baseUrl))
            .Content
            .ReadAsStringAsync();

        var pdfPaths = contents.Split('\n') //Split on newlines
            .Select(line =>
            {
                //Lines of interest look like: <li><a href="%231-A%20-%20Preface.pdf"> #1-A - Preface.pdf</a></li>
                //So can split on quote (") and take second element to get the path
                var splitOnQuote = line.Split("\"");
                return splitOnQuote.Length > 2 ? splitOnQuote[1] : null;
            })
            .Where(x => x != null && x.EndsWith(".pdf")) //Extract .pdf files
            .OrderBy(x => x) //Order for stability
            .ToArray();

        Console.WriteLine($"Total PDFs: {pdfPaths.Length}");

        Console.WriteLine("\nBegin Download...\n");
        
        await PdfDownloader.DownloadPdfsAsync(
            basePath: baseUrl,
            paths: pdfPaths,
            filePrefix: filePrefix);

        Console.WriteLine($"\nDownload complete: {sw.Elapsed.TotalSeconds:F1}(s)");
    }
}

public static class PdfDownloader
{
    public static async Task DownloadPdfsAsync(string basePath, IReadOnlyList<string> paths, string filePrefix)
    {
        using var client = new HttpClient();

        var notFound = new List<string>();

        foreach (var path in paths)
        {
            var fileSafePath = Uri.UnescapeDataString(path);
            var filePath = $"{filePrefix}{fileSafePath}";
            if (File.Exists(filePath))
            {
                Console.WriteLine($"Skipping: File exists [{filePath}]");
                continue;
            }

            var url = $"{basePath}/{path}";
            var resp = await client.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                if (resp.StatusCode == HttpStatusCode.NotFound)
                {
                    notFound.Add(path);
                    continue;
                }
                throw new Exception($"Get failed. {resp.StatusCode} [{url}]");
            }

            var bytes = await resp.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(filePath, bytes);

            Console.WriteLine($"Downloaded File: {filePath}. {bytes.Length} bytes");
        }

        Console.WriteLine($"\nNot Found [{notFound.Count}]\n");
        notFound.ForEach(Console.WriteLine);
    }
}
