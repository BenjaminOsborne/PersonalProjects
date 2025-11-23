using System.Text.Json;

namespace BibleApp.Core;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions _writeNormal = new() { IncludeFields = true, WriteIndented = false };
    private static readonly JsonSerializerOptions _writeIndented = new() { IncludeFields = true, WriteIndented = true };

    private static readonly JsonSerializerOptions _deserialiseIgnoreCase = new() { PropertyNameCaseInsensitive = true };

    public static string Serialise<T>(T data, bool writeIndented = false) =>
        JsonSerializer.Serialize(data, writeIndented ? _writeIndented : _writeNormal);

    public static T? Deserialise<T>(string json, bool ignoreCase = false) => ignoreCase
        ? JsonSerializer.Deserialize<T>(json, options: _deserialiseIgnoreCase)
        : JsonSerializer.Deserialize<T>(json);

    public static T? Deserialise<T>(Stream stream) => JsonSerializer.Deserialize<T>(stream);
}