using System.Collections.Immutable;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace PdfProcessor;

public static class PdfTextExtractor
{
    private record PageExtract(
        int Number,
        IReadOnlyList<LineExtract> Lines)
    {
        public double? ModalHeight { get; } = Lines
            .Select(x => x.Height)
            .Where(x => x.HasValue)
            .Skip(Lines.Count / 2)
            .FirstOrDefault();
    }

    private record LineExtract(
        string Text,
        IReadOnlyList<Word> Words)
    {
        public double? Height { get; } = Words
            .FirstOrDefault(x => x.Text.Trim().Length > 0)?
            .BoundingBox.Height;
    }

    private record LinePostProcess(bool IsLikelyHeader, string Text);

    public static async Task ExtractSpuregonSermonAsync()
    {
        var root = @"C:\PdfDownload\";
        var paths = Directory.GetFiles(root, searchPattern: "*.pdf");
        foreach (var filePath in paths
                     .Where(x => x.Contains("chstop") == false) //Tabular PDFs - not useful
                     .OrderBy(x => x))
        {
            var fileName = new FileInfo(filePath);
            var txtName = fileName.Name.Replace(".pdf", ".txt");
            var txtPath = Path.Combine(root, "TextExtract", txtName);
            if (File.Exists(txtPath))
            {
                Console.WriteLine($"Skipping file (txt already extracted): {filePath}");
                continue;
            }

            Console.WriteLine($"Extracting txt from pdf: {filePath}");
            
            if (bool.Parse("false"))
            {
                var lines = _ExtractSpuregonSermon(filePath);
                await File.WriteAllLinesAsync(txtPath, lines);
            }
        }
    }

    public static IReadOnlyList<string> _ExtractSpuregonSermon(string filePath)
    {
        using var document = PdfDocument.Open(filePath);

        var pages = ImmutableList.CreateBuilder<PageExtract>();

        foreach (var page in document.GetPages())
        {
            var lines = ImmutableList.CreateBuilder<LineExtract>();

            var words = page.GetWords(NearestNeighbourWordExtractor.Instance);
            
            var lineBottom = (double?)null;
            var lineBuilder = new StringBuilder();
            var lineWords = ImmutableList.CreateBuilder<Word>();

            void TryAddLine()
            {
                if (lineBuilder.Length == 0)
                {
                    return;
                }
                var lineText = lineBuilder.ToString();
                if (lineText.IsWhiteSpace())
                {
                    return;
                }

                lines.Add(new LineExtract(
                    Text: lineText,
                    Words: lineWords.ToImmutable()));
            }

            foreach (var word in words)
            {
                var bottom = word.BoundingBox.Bottom;
                var wordTxt = word.Text;

                if (lineBottom == null || !lineBottom.Value.Equals(bottom)) //New line: Flush through
                {
                    TryAddLine();

                    //Reset
                    lineBottom = bottom;
                    lineBuilder.Clear();
                    lineWords.Clear();
                }

                lineBuilder.Append(wordTxt);
                lineWords.Add(word);
            }

            TryAddLine();

            pages.Add(new PageExtract(page.Number, lines.ToImmutable()));
        }

        return _BuildFinalText(pages, filePath);
    }

    private static bool _IsNumber(string lineText) =>
        int.TryParse(lineText.Trim(), out _);

    private static IReadOnlyList<string> _SplitToWords(string line) =>
        line
            .SplitOnWhitespace()
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToArray();

    private static IReadOnlyList<string> _BuildFinalText(IReadOnlyList<PageExtract> pages, string filePath)
    {
        var processed = pages
            .Select(p => new { page = p, lines = _ProcessPage(p).ToArray() })
            .ToArray();

        var missingHeader = processed
            .Where(x => x.page.Number > 1)
            .FirstOrDefault(p => p.lines.Any(l => l.IsLikelyHeader) == false);
        if (missingHeader != null)
        {
            throw new Exception($"NO HEADER: {filePath}");
        }

        var lines = ImmutableList.CreateBuilder<string>();
        foreach (var txt in processed
                     .SelectMany(x => x.lines)
                     .Where(x => !x.IsLikelyHeader)
                     .Select(x => x.Text))
        {
            if (lines.Any() &&
                txt.Length > 0 &&
                char.IsLower(txt[0]))
            {
                var head = lines.Last();
                if (head.EndsWith("-"))
                {
                    lines.RemoveAt(lines.Count - 1);
                    var removeDash = head.Substring(0, head.Length-1);
                    var fistSpace = txt.IndexOf(' ');

                    if (fistSpace > 0)
                    {
                        var pre = txt.Substring(0, fistSpace);
                        var post = txt.Substring(fistSpace+1);
                        lines.Add(removeDash + pre);
                        lines.Add(post);
                    }
                    else
                    {
                        lines.Add(removeDash + txt);
                    }
                    continue;
                }
            }

            lines.Add(txt);
        }

        var data = lines
            .Select(x => x.Trim())
            .Select(x => new { Line = x, IsFooterBegin = IsLikelyFileFooterBegin(x) })
            .ToArray();
        if (data.Any(x => x.IsFooterBegin) == false)
        {
            throw new Exception($"Cannot find footer: {filePath}");
        }

        return data
            .TakeWhile(x => !x.IsFooterBegin)
            .Select(x => x.Line)
            .ToArray();

        static bool IsLikelyFileFooterBegin(string line) =>
            line.StartsWith("Taken from The Metropolitan Tabernacle Pulpit C. H. Spurgeon Collection.") ||
            line.StartsWith("Taken from The Metropolitan tabernacle Pulpit C. H. Spurgeon Collection.") ||
            line.StartsWith("Taken from The metropolitan Tabernacle Pulpit C. H. Spurgeon Collection.") ||
            line.StartsWith("Taken from The C. H. Spurgeon Collection, Version 1.0, Ages Software.") ||
            line.StartsWith("Adapted from The C. H. Spurgeon Collection, Version 1.0, Ages Software.") ||
            line.StartsWith("Adapted from The C.H. Spurgeon Collection, Ages Software.") ||
            line.StartsWith("—Adapted from The C. H. Spurgeon Collection, Version 1.0, Ages Software,") ||
            line.StartsWith("END OF VOLUME 28.")
            ;
    }

    private static IEnumerable<LinePostProcess> _ProcessPage(PageExtract page)
    {
        var isHeader = true;
        foreach (var line in page.Lines)
        {
            isHeader = isHeader &&
                       (IsLikelyHeaderHeight() || IsLikelyHeaderText());
            yield return new LinePostProcess(IsLikelyHeader: isHeader, Text: line.Text);

            bool IsLikelyHeaderHeight() =>
                page.ModalHeight.HasValue &&
                line.Height.HasValue &&
                line.Height.Value < page.ModalHeight;


            bool IsLikelyHeaderText()
            {
                var lineText = line.Text;
                if (_IsNumber(lineText)) //Likely page number
                {
                    return true;
                }
                if (page.Number == 1 && lineText.StartsWith("Sermon #"))
                {
                    return true;
                }
                if (lineText.Contains("Volume") && lineText.Contains("www.spurgeongems.org"))
                {
                    return true;
                }
                return false;
                //var sermonTitle = pages.FirstOrDefault()?.Lines.FirstOrDefault(x => !x.IsLikelyHeader)?.Text;
                //if (sermonTitle != null)
                //{
                //    var setTitle = _SplitToWords(sermonTitle.ToLower())
                //        .Where(x => _IsNumber(x) == false)
                //        .ToImmutableHashSet();
                //    var splitLineWords = _SplitToWords(lineText.ToLower())
                //        .Where(x => _IsNumber(x) == false)
                //        .ToArray();
                //    if (splitLineWords.All(setTitle.Contains))
                //    {
                //        return true;
                //    }
                //}
            }
}
    }
}