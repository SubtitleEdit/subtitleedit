namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>
/// Read-only snapshot of the Subtitle Edit settings a checking/fixing plugin typically needs,
/// so it can follow the user's rules without reading (or racing writes to) Settings.json.
/// Changes are not read back - a plugin keeps its own options in <see cref="PluginRequest.Settings"/>.
/// </summary>
public class PluginRules
{
    public int SubtitleMinimumDisplayMilliseconds { get; set; }
    public int SubtitleMaximumDisplayMilliseconds { get; set; }
    public double SubtitleMaximumCharactersPerSeconds { get; set; }
    public double SubtitleOptimalCharactersPerSeconds { get; set; }
    public int SubtitleLineMaximumLength { get; set; }
    public int MaxNumberOfLines { get; set; }

    /// <summary>Minimum gap between two subtitles in milliseconds (frame-mode setting already converted).</summary>
    public int MinimumMillisecondsBetweenLines { get; set; }

    /// <summary>Minimum gap between two subtitles in frames, as set in the frame-mode setting.</summary>
    public int MinimumFramesBetweenLines { get; set; }

    /// <summary>True when Subtitle Edit shows and edits time codes as frames.</summary>
    public bool UseFrameMode { get; set; }

    /// <summary>EBU STL teletext: text is boxed (written with start/end box control codes).</summary>
    public bool EbuStlTeletextUseBox { get; set; }

    /// <summary>EBU STL teletext: text is double height, so every text line uses two teletext rows.</summary>
    public bool EbuStlTeletextUseDoubleHeight { get; set; }

    /// <summary>
    /// Video offset in milliseconds. Paragraph times are video-relative; the grid and saved files
    /// show (and contain) time + offset.
    /// </summary>
    public long VideoOffsetMs { get; set; }
}
