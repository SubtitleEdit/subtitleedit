using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// What a dialog needs to draw the subtitle on its own video the way the main window draws it on
/// the main preview: the format and the header decide how it is rendered (ASSA keeps its styles,
/// and the header is where libass reads PlayRes from, so \pos lands where it does in the main
/// window), and SMPTE mode stretches the time codes by the same 0.1% the main preview uses - a
/// sync dialog showing the subtitle a second off from the main video would defeat its own purpose.
/// </summary>
/// <param name="Format">The format the subtitle is being edited as.</param>
/// <param name="Header">The subtitle's header, or null when the format has none.</param>
/// <param name="SmpteMode">Whether the main window is in SMPTE timing mode.</param>
public record VideoPreviewSubtitleContext(SubtitleFormat Format, string? Header, bool SmpteMode)
{
    /// <summary>Plain text at ordinary timing - what a dialog has before it is told otherwise.</summary>
    public static VideoPreviewSubtitleContext Default => new(new SubRip(), null, false);
}
