using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DataModel;

public static class FileLoader
{
    public static string GetDefinitionsPath() =>
        Path.Combine(GetRootDirectory(), "DataModel", "Definitions");

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

    public static string GetRootDirectory()
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

public static class JsonHelper
{
    private static readonly JsonSerializerOptions _indented = new() { WriteIndented = true };

    public static string SerializeIndented<T>(T item) =>
        JsonSerializer.Serialize(item, _indented);

    public static async Task<T> DeserializeAsync<T>(Stream stream) =>
        await JsonSerializer.DeserializeAsync<T>(stream) ??
        throw new JsonException($"Cannot parse stream. Type: {typeof(T).FullName}");
}