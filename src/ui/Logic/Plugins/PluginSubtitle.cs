using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>
/// A subtitle payload exchanged with a plugin. In a request <see cref="Native"/>,
/// <see cref="SubRip"/>, <see cref="Header"/> and <see cref="Paragraphs"/> are populated; in a
/// response a plugin sets either <see cref="Format"/> + <see cref="Native"/>, or
/// <see cref="Paragraphs"/> (plus <see cref="Header"/> to change it).
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

    /// <summary>
    /// The subtitle's in-memory header: the EBU STL GSI block (1024 characters), the ASSA
    /// [Script Info]/[V4+ Styles] sections, etc. In a response, null keeps the current header.
    /// </summary>
    public string? Header { get; set; }

    /// <summary>
    /// The lines as Subtitle Edit holds them - the only lossless way to exchange a binary format
    /// (EBU STL, PAC, ...), whose <see cref="Native"/> text is not available. In a response,
    /// non-null paragraphs take precedence over <see cref="Native"/> and keep the current format.
    /// </summary>
    public List<PluginParagraph>? Paragraphs { get; set; }
}
