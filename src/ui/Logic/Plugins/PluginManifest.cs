namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>
/// Deserialized "plugin.json" - lets Subtitle Edit list a plugin in the menus
/// without launching it.
/// </summary>
public class PluginManifest
{
    public int ApiVersion { get; set; } = PluginConstants.ApiVersion;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Plugin's own version, e.g. "1.0.0".</summary>
    public string Version { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Which menu the entry is added to. See <see cref="PluginConstants"/> Menu* values.
    /// Defaults to the Tools menu when empty.
    /// </summary>
    public string Menu { get; set; } = PluginConstants.MenuTools;

    /// <summary>Suggested shortcut, e.g. "Control+Shift+P". Optional.</summary>
    public string? Shortcut { get; set; }

    /// <summary>Minimum Subtitle Edit version required, e.g. "5.0.0". Optional.</summary>
    public string? MinSeVersion { get; set; }

    /// <summary>Icon file name relative to the plugin folder. Optional.</summary>
    public string? Icon { get; set; }

    /// <summary>
    /// When set to "dotnet" the plugin is launched as "dotnet &lt;Entry&gt;" using the
    /// shared runtime instead of a native executable from <see cref="Executables"/>.
    /// </summary>
    public string? Runtime { get; set; }

    /// <summary>Entry assembly file name (relative to the plugin folder) used with <see cref="Runtime"/>.</summary>
    public string? Entry { get; set; }

    /// <summary>Native executable file names per operating system.</summary>
    public PluginExecutables? Executables { get; set; }
}

public class PluginExecutables
{
    public string? Windows { get; set; }
    public string? Linux { get; set; }
    public string? Macos { get; set; }
}
