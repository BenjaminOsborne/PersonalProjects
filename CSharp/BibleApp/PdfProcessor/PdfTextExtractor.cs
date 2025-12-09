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
        IReadOnlyList<LineExtract> Lines);

    private record LineExtract(
        string Text,
        IReadOnlyList<Word> Words)
    {
        /// <summary> Line height is the maximum height of the words in the line </summary>
        public double? Height { get; } = _GetMaxHeight(heights: Words
            .Where(x => x.Text.Trim().Length > 0)
            .Select(x => x.BoundingBox.Height));
    }

    private record LinePostProcess(bool IsLikelyHeader, string Text, bool IsLikelyFooter);

    public static async Task ExtractSpuregonSermonAsync()
    {
        var root = @"C:\PdfDownload\";
        if (!Directory.Exists(root))
        {
            throw new ArgumentException($"Cannot find Directory: {root}");
        }

        var extractDir = Path.Combine(root, "TextExtract");
        if (!Directory.Exists(extractDir))
        {
            Directory.CreateDirectory(extractDir); //Create extract directory if not there
        }

        var paths = Directory.GetFiles(root, searchPattern: "*.pdf");
        foreach (var filePath in paths
                     .Where(x => x.Contains("chstop") == false) //Tabular PDFs - not useful
                     //.SkipWhile(x => x.Contains("chs") == false)
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

            var lines = _ExtractSpuregonSermon(filePath);
            await File.WriteAllLinesAsync(txtPath, lines);
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
        var modalHeight = _GetModalHeight(heights: pages
                              .SelectMany(x => x.Lines)
                              .Where(x => x.Height.HasValue)
                              .Select(x => x.Height!.Value))
                          ?? throw new Exception($"No page has any lines with height: {filePath}");
        var processed = pages
            .Select(p => new { page = p, lines = _ProcessPage(p, modalHeight).ToArray() })
            .ToArray();

        var missingHeader = processed
            .Where(x => x.page.Number > 1)
            .FirstOrDefault(p => p.lines.Any(l => l.IsLikelyHeader) == false);
        if (missingHeader != null)
        {
            throw new Exception($"NO HEADER: {filePath}");
        }

        var foundFooter = false;
        var lines = ImmutableList.CreateBuilder<string>();
        foreach (var lpp in processed.SelectMany(x => x.lines))
        {
            foundFooter = foundFooter || lpp.IsLikelyFooter;
            if (lpp.IsLikelyHeader || foundFooter)
            {
                continue;
            }

            var txt = lpp.Text;
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

        if (foundFooter == false)
        {
            throw new Exception($"Cannot find footer: {filePath}");
        }

        var trimmedLines = lines
            .Select(x => x.Trim())
            .ToArray();
        return trimmedLines;
    }

    private static IEnumerable<LinePostProcess> _ProcessPage(PageExtract page, double modalHeight)
    {
        if (!page.Lines.Any())
        {
            yield break;
        }

        var likelyHeaderHeight = modalHeight * 0.88;
        var isHeader = true;
        foreach (var line in page.Lines)
        {
            isHeader = isHeader &&
                       (IsLikelyHeaderHeight() || IsLikelyHeaderText());
            var isLikelyFooter = IsLikelyFileFooterBegin(line.Text);

            yield return new LinePostProcess(
                IsLikelyHeader: isHeader,
                Text: line.Text,
                IsLikelyFooter: isLikelyFooter);

            bool IsLikelyHeaderHeight() =>
                line.Height.HasValue &&
                line.Height.Value <= likelyHeaderHeight; //MUST be less than modal (comes into play of header height within 5% of modal height!)

            bool IsLikelyHeaderText()
            {
                var lineText = line.Text;
                if (_IsNumber(lineText)) //Likely page number
                {
                    return true;
                }
                if (lineText.Contains("Sermon #") || lineText.Contains("Sermons #"))
                {
                    return true;
                }
                if (lineText.Contains("Volume") && lineText.Contains("www.spurgeongems.org"))
                {
                    return true;
                }

                var words = _SplitToWords(lineText)
                    .Where(x => _IsNumber(x) == false)
                    .ToArray();
                if (words.All(x => x == "Volume"))
                {
                    return true;
                }
                
                return false;
            }

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
    }

    private static double? _GetModalHeight(IEnumerable<double> heights)
    {
        var ordered = heights
            .OrderBy(x => x)
            .ToArray();
        if (ordered.Length == 0)
        {
            return null;
        }
        return ordered
            .Skip(ordered.Length / 2) //Mode is the middle number
            .First();
    }

    private static double? _GetMaxHeight(IEnumerable<double> heights)
    {
        var arr = heights.ToArray();
        return arr.Any() ? arr.Max() : null;
    }
}