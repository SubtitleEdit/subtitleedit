using System.Collections.Generic;
using System.Text.Json;

namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>
/// Written by Subtitle Edit to a JSON file; the path of that file is passed as the
/// first command-line argument to the plugin process.
/// </summary>
public class PluginRequest
{
    public int ApiVersion { get; set; } = PluginConstants.ApiVersion;

    /// <summary>Currently always "run".</summary>
    public string RequestType { get; set; } = "run";

    /// <summary>Absolute path where the plugin must write its <see cref="PluginResponse"/>.</summary>
    public string ResponseFilePath { get; set; } = string.Empty;

    /// <summary>A scratch directory the plugin may use; deleted by Subtitle Edit after the run.</summary>
    public string TempDirectory { get; set; } = string.Empty;

    public PluginSubtitle Subtitle { get; set; } = new();

    /// <summary>Zero-based indices of the lines selected in the grid (empty if none).</summary>
    public List<int> SelectedIndices { get; set; } = new();

    public string VideoFileName { get; set; } = string.Empty;

    public double FrameRate { get; set; }

    /// <summary>Current UI language name, e.g. "English".</summary>
    public string UiLanguage { get; set; } = string.Empty;

    /// <summary>Current theme, e.g. "Dark" or "Light".</summary>
    public string Theme { get; set; } = string.Empty;

    public string SeVersion { get; set; } = string.Empty;

    /// <summary>The plugin's own settings as last persisted by Subtitle Edit (null on first run).</summary>
    public JsonElement? Settings { get; set; }
}
