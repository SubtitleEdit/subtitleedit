namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>
/// A subtitle payload exchanged with a plugin. In a request both <see cref="Native"/>
/// and <see cref="SubRip"/> are populated; in a response a plugin only needs to set
/// <see cref="Format"/> and <see cref="Native"/> with its result.
/// </summary>
public class PluginSubtitle
{
    /// <summary>Friendly format name, e.g. "SubRip" or "Advanced Sub Station Alpha".</summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>Original file name (may be empty for an unsaved subtitle).</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Full subtitle text serialized in <see cref="Format"/>.</summary>
    public string Native { get; set; } = string.Empty;

    /// <summary>Full subtitle text serialized as SubRip (.srt) - always provided in requests.</summary>
    public string SubRip { get; set; } = string.Empty;
}
