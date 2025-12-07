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
        IReadOnlyList<LineExtract> Lines
        );

    private record LineExtract(
        bool IsLikelyHeader,
        string Text,
        IReadOnlyList<Word> Words
        );

    public static async Task ExtractSpuregonSermonAsync()
    {
        var filePath = @"C:\PdfDownload\SpurgeonSermon_chs 566.pdf";
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

                bool IsLikelyHeader()
                {
                    if (lines.Any(x => !x.IsLikelyHeader))
                    {
                        return false;
                    }
                    if (int.TryParse(lineText.Trim(), out _)) //Likely page number
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
                    
                    var sermonTitle = pages.FirstOrDefault()?.Lines.FirstOrDefault(x => !x.IsLikelyHeader)?.Text;
                    if (sermonTitle != null && lineText.ToLower().Contains(sermonTitle.ToLower()))
                    {
                        return true;
                    }

                    return false;
                }

                lines.Add(new LineExtract(
                    IsLikelyHeader: IsLikelyHeader(),
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

        Console.WriteLine($"Extracted Lines: {pages.Sum(x => x.Lines.Count)} from {filePath}");

        var finalLines = _BuildFinalText(pages);

        await Task.Delay(10); //Will write to file once sanitised...
    }

    private static IReadOnlyList<string> _BuildFinalText(IReadOnlyList<PageExtract> pages)
    {
        var lines = ImmutableList.CreateBuilder<string>();
        foreach (var txt in pages.SelectMany(x => x.Lines)
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

        return lines
            .Select(x => x.Trim())
            .ToArray();
        //return lines
        //    .SelectMany(x => x.Split(". "))
        //    .Select(x => x.Trim())
        //    .ToArray();
    }
}