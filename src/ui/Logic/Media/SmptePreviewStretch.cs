using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// SMPTE preview timing: the subtitle's time codes count 30/24 frames per second while the
/// video runs at 29.97/23.976, so everything handed to the player has to run 0.1 % slower
/// to stay in sync with the picture. The factor and both ways of applying it live here so
/// the mpv and VLC reloaders, and the secondary subtitle, cannot drift apart.
/// </summary>
public static class SmptePreviewStretch
{
    public const double Factor = 1.001;

    /// <summary>
    /// Stretches every paragraph in place. Only for a preview copy - the caller's live
    /// subtitle must never see this.
    /// </summary>
    public static void Apply(Subtitle subtitle)
    {
        foreach (var paragraph in subtitle.Paragraphs)
        {
            Apply(paragraph);
        }
    }

    public static void Apply(Paragraph paragraph)
    {
        paragraph.StartTime.TotalMilliseconds *= Factor;
        paragraph.EndTime.TotalMilliseconds *= Factor;
    }

    /// <summary>
    /// A stretched copy, for paragraphs that are shared and must stay untouched.
    /// </summary>
    public static Paragraph Stretched(Paragraph paragraph)
    {
        var copy = new Paragraph(paragraph, false);
        Apply(copy);
        return copy;
    }
}
