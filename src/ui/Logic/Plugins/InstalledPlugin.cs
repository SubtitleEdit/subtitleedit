namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>
/// A plugin discovered on disk: its manifest plus the resolved paths used to launch it
/// on the current operating system.
/// </summary>
public class InstalledPlugin
{
    public required PluginManifest Manifest { get; init; }

    /// <summary>The plugin's own folder (contains plugin.json and the executable).</summary>
    public required string FolderPath { get; init; }

    public required string ManifestPath { get; init; }

    /// <summary>
    /// For a native plugin: absolute path to the executable for this OS.
    /// For a "dotnet" runtime plugin: absolute path to the entry .dll.
    /// Null when the plugin cannot run on the current OS.
    /// </summary>
    public string? LaunchPath { get; init; }

    /// <summary>True when <see cref="LaunchPath"/> must be started via the "dotnet" host.</summary>
    public bool UsesDotnetRuntime { get; init; }

    public bool CanRun => !string.IsNullOrEmpty(LaunchPath);
}
