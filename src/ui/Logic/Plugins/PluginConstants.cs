namespace Nikse.SubtitleEdit.Logic.Plugins;

public static class PluginConstants
{
    /// <summary>
    /// JSON contract version. Bump only on breaking changes to the request/response schema.
    /// </summary>
    public const int ApiVersion = 1;

    public const string ManifestFileName = "plugin.json";

    public const string RequestFileName = "request.json";
    public const string ResponseFileName = "response.json";

    /// <summary>JSON index of plugins available to download.</summary>
    public const string OnlineIndexUrl = "https://raw.githubusercontent.com/SubtitleEdit/plugins/main/se5-plugins.json";

    // Response status values.
    public const string StatusOk = "ok";
    public const string StatusCancelled = "cancelled";
    public const string StatusError = "error";

    // Manifest "menu" values - which top-level menu the plugin entry is added to.
    public const string MenuTools = "Tools";
    public const string MenuFile = "File";
    public const string MenuSync = "Sync";
    public const string MenuTranslate = "Translate";
    public const string MenuSpellCheck = "SpellCheck";
    public const string MenuAssa = "Assa";

    // Manifest "runtime" value - launch the entry via the shared "dotnet" host instead of a native exe.
    public const string RuntimeDotnet = "dotnet";
}
