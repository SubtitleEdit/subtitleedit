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

    /// <summary>URL of the plugin .zip. The zip must contain the plugin's folder (with plugin.json inside).</summary>
    public string DownloadUrl { get; set; } = string.Empty;
}
