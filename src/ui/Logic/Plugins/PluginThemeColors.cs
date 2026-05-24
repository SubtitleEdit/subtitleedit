namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>
/// Colors of the active Subtitle Edit theme, passed to plugins so their own UI
/// can match. All values are <c>#AARRGGBB</c> hex strings.
/// </summary>
public class PluginThemeColors
{
    /// <summary>True when the active theme is dark.</summary>
    public bool IsDark { get; set; }

    /// <summary>Main window/control background.</summary>
    public string BackgroundColor { get; set; } = string.Empty;

    /// <summary>Main text color.</summary>
    public string ForegroundColor { get; set; } = string.Empty;

    /// <summary>Accent color used for focused/default buttons.</summary>
    public string AccentColor { get; set; } = string.Empty;

    /// <summary>Slightly lighter than <see cref="BackgroundColor"/> (hover/lift variant).</summary>
    public string BackgroundColorLighter { get; set; } = string.Empty;

    /// <summary>Header/menu background (lighter than <see cref="BackgroundColor"/>).</summary>
    public string BackgroundColorHeader { get; set; } = string.Empty;

    /// <summary>Bookmark highlight color.</summary>
    public string BookmarkColor { get; set; } = string.Empty;
}
