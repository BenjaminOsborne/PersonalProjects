using System.Text.RegularExpressions;

namespace PdfProcessor;

public static class Extensions
{
    private static readonly Regex _regexWhitespace = new(@"\s+");

    public static IReadOnlyList<string> SplitOnWhitespace(this string text) =>
        _ReplaceWhitespace(text, " ").Split(' '); //regex to convert all whitespace to space, then split

    private static string _ReplaceWhitespace(string text, string replace) =>
        _regexWhitespace.Replace(text, replace);
}