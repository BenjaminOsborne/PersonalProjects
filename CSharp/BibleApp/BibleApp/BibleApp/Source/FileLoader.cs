using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using BibleApp.Core;

namespace BibleApp.Source;

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

    private static Bible _ToBible(string translation, JsonDocument node)
    {
        var id = new BibleId(translation);
        return new(Id: id, Books: _MapElementItems(id, node.RootElement, _ToBook));
    }

    private static Book _ToBook(BibleId parentId, JsonProperty b)
    {
        var id = new BookId(b.Name);
        return new(BibleId: parentId, Id: id, Chapters: _MapElementItems(id, b.Value, _ToChapter));
    }

    private static Chapter _ToChapter(BookId parentId, JsonProperty c)
    {
        var id = new ChapterId(parentId, ChapterNumber: _NameAsNumber(c));
        return new(Id: id, Verses: _MapElementItems(id, c.Value, _ToVerse));
    }

    private static Verse _ToVerse(ChapterId parentId, JsonProperty v) =>
        new(new VerseId(parentId, VerseNumber: _NameAsNumber(v)), Text: v.Value.GetString()!);

    private static int _NameAsNumber(JsonProperty c) =>
        int.Parse(c.Name);

    private static IReadOnlyList<T> _MapElementItems<TId, T>(TId parentId, JsonElement el, Func<TId, JsonProperty, T> mapper) =>
        el.EnumerateObject().Select(x => mapper(parentId, x)).ToArray();

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