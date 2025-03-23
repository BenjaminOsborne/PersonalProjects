namespace AccountProcessor.Client;

public static class RenderModeUtility
{
    public static string GetRenderModeType() =>
        OperatingSystem.IsBrowser() ? "WASM" : "Server";

    /// <summary>
    /// TODO: This needs to be stored as Singleton Server-side and accessed via controller as "static" doesn't mean same thing in a Client context!!!
    /// </summary>
    public static bool UseServerMode { get; private set; } = true;

    public static void FlickMode() => UseServerMode = !UseServerMode;
}