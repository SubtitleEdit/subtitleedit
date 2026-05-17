using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>The downloadable plugin index (see <see cref="PluginConstants.OnlineIndexUrl"/>).</summary>
public class PluginIndex
{
    public List<PluginIndexEntry> Plugins { get; set; } = new();
}

public class PluginIndexEntry
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? MinSeVersion { get; set; }

    /// <summary>
    /// Per-platform download URLs keyed by <see cref="PluginPlatform.GetCurrentKey"/>
    /// (e.g. "win-x64", "linux-arm64", "osx-arm64"). Each URL must point to a .zip
    /// containing the plugin's folder (with plugin.json inside). Keys are
    /// matched case-insensitively. A plugin without a key for the current
    /// platform is shown but cannot be installed.
    /// </summary>
    public Dictionary<string, string> Downloads { get; set; } = new();
}
