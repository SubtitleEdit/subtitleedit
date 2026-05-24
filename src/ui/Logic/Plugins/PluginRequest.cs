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

    /// <summary>Total duration of the video in seconds. Null when no video is loaded.</summary>
    public double? VideoDurationSeconds { get; set; }

    /// <summary>Video frame width in pixels. Null when no video is loaded.</summary>
    public int? VideoWidth { get; set; }

    /// <summary>Video frame height in pixels. Null when no video is loaded.</summary>
    public int? VideoHeight { get; set; }

    /// <summary>Current UI language name, e.g. "English".</summary>
    public string UiLanguage { get; set; } = string.Empty;

    /// <summary>Current theme, e.g. "Dark" or "Light".</summary>
    public string Theme { get; set; } = string.Empty;

    /// <summary>Colors of the active theme so the plugin's own UI can match.</summary>
    public PluginThemeColors? ThemeColors { get; set; }

    public string SeVersion { get; set; } = string.Empty;

    /// <summary>The plugin's own settings as last persisted by Subtitle Edit (null on first run).</summary>
    public JsonElement? Settings { get; set; }

    /// <summary>
    /// Schema version the plugin attached to <see cref="Settings"/> in its last response.
    /// Null when the plugin has never persisted settings (or persisted them with no version).
    /// The plugin owns the value; SE just round-trips it so the plugin can migrate or
    /// reset stale settings when it changes its own schema.
    /// </summary>
    public int? SettingsVersion { get; set; }
}
