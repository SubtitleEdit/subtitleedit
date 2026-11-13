using System;

namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

/// <summary>
/// The bookmark tag both waveform renderers draw on a bookmarked line (SE 4 parity): a rounded
/// tag in the bookmark color, a ribbon glyph and the first line of the note, placed just below
/// the middle of the text area so it stays clear of the subtitle text and the number footer.
/// </summary>
internal static class WaveformBookmark
{
    public const int MaxLabelLength = 40;
    public const double Padding = 4;
    public const double RibbonWidth = 7;
    public const double Gap = 4;
    public const double CornerRadius = 3;
    public const double MinHeight = 16;

    /// <summary>The first non-empty line of the note, cut to <see cref="MaxLabelLength"/>.</summary>
    public static string GetLabel(string? bookmark)
    {
        if (string.IsNullOrWhiteSpace(bookmark))
        {
            return string.Empty;
        }

        foreach (var line in bookmark.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            return trimmed.Length <= MaxLabelLength
                ? trimmed
                : trimmed.Substring(0, MaxLabelLength - 1).TrimEnd() + "…";
        }

        return string.Empty;
    }

    /// <summary>Dark ribbon and text on a light tag, light on a dark one.</summary>
    public static bool UseDarkForeground(byte r, byte g, byte b) => (0.299 * r + 0.587 * g + 0.114 * b) / 255.0 > 0.55;

    /// <summary>Top of the tag inside a text area of <paramref name="height"/> starting at <paramref name="top"/>.</summary>
    public static double GetTop(double top, double height) => Math.Round(top + height / 2 + 2);

    /// <summary>
    /// The ribbon glyph (a bookmark with a V notch) as six points, inside the box
    /// (<paramref name="x"/>, <paramref name="y"/>, <see cref="RibbonWidth"/>, <paramref name="height"/>).
    /// </summary>
    public static (double X, double Y)[] GetRibbon(double x, double y, double height) =>
    [
        (x, y),
        (x + RibbonWidth, y),
        (x + RibbonWidth, y + height),
        (x + RibbonWidth / 2, y + height - RibbonWidth / 2),
        (x, y + height),
        (x, y),
    ];
}
