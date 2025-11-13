using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;

namespace BibleApp.Source;

public record Bible(string Translation, IReadOnlyList<Book> Books);

public record Book(string Name, IReadOnlyList<Chapter> Chapters);

public record Chapter(int Number, IReadOnlyList<Verse> Verses);

public record Verse(int Number, string Text);

public class FileLoader
{
    public static async Task<IReadOnlyList<Bible>> LoadAllAsync()
    {
        var lookupPath = GetPathRelativeToExecuting(Path.Combine("Source", "Files"));
        var filePaths = Directory.GetFiles(lookupPath, "*.json", new EnumerationOptions {  RecurseSubdirectories = true });
        var bibles = ImmutableList.CreateBuilder<Bible>();
        foreach (var path in filePaths)
        {
            bibles.Add(_ToBible(
                translation: new FileInfo(path).Name.Replace("_bible", ""),
                node: await JsonDocument.ParseAsync(File.OpenRead(path))));
        }
        return bibles;
    }

    private static Bible _ToBible(string translation, JsonDocument node) =>
        new(Translation: translation,
            Books: _MapElementItems(node.RootElement, _ToBook));

    private static Book _ToBook(JsonProperty b) =>
        new(Name: b.Name, Chapters: _MapElementItems(b.Value, _ToChapter));

    private static Chapter _ToChapter(JsonProperty c) =>
        new(Number: _NameAsNumber(c), Verses: _MapElementItems(c.Value, _ToVerse));

    private static Verse _ToVerse(JsonProperty v) =>
        new(Number: _NameAsNumber(v), Text: v.Value.GetString()!);

    private static int _NameAsNumber(JsonProperty c) =>
        int.Parse(c.Name);

    private static IReadOnlyList<T> _MapElementItems<T>(JsonElement el, Func<JsonProperty, T> mapper) =>
        el.EnumerateObject().Select(mapper).ToArray();

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
}