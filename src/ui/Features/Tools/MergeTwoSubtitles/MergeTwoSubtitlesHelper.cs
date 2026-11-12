using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.MergeTwoSubtitles;

/// <summary>
/// Builds one bilingual subtitle from two subtitles - used by Tools → Merge two subtitles and by
/// the batch convert function "Merge translation with original".
/// </summary>
public static class MergeTwoSubtitlesHelper
{
    public const string StyleName1 = "Style1";
    public const string StyleName2 = "Style2";

    /// <summary>
    /// Both texts in one subtitle: a line of <paramref name="sub1"/> gets the text of the first unused
    /// overlapping line of <paramref name="sub2"/> below its own text (and keeps its own time codes).
    /// Lines without a match are kept as they are.
    /// </summary>
    public static Subtitle BuildSubRipMerge(Subtitle sub1, Subtitle sub2)
    {
        var result = new Subtitle();
        var used2 = new bool[sub2.Paragraphs.Count];
        var firstUnused2 = 0;

        for (var i1 = 0; i1 < sub1.Paragraphs.Count; i1++)
        {
            var p1 = sub1.Paragraphs[i1];
            var matchIndex = FindOverlapping(p1, sub2, used2, ref firstUnused2);
            if (matchIndex >= 0)
            {
                used2[matchIndex] = true;
                var p2 = sub2.Paragraphs[matchIndex];
                var combined = new Paragraph(
                    p1.Text + Environment.NewLine + p2.Text,
                    p1.StartTime.TotalMilliseconds,
                    p1.EndTime.TotalMilliseconds);
                result.Paragraphs.Add(combined);
            }
            else
            {
                result.Paragraphs.Add(new Paragraph(p1));
            }
        }

        for (var i2 = 0; i2 < sub2.Paragraphs.Count; i2++)
        {
            if (used2[i2])
            {
                continue;
            }

            result.Paragraphs.Add(new Paragraph(sub2.Paragraphs[i2]));
        }

        result.Paragraphs.Sort((a, b) => a.StartTime.TotalMilliseconds.CompareTo(b.StartTime.TotalMilliseconds));
        return result;
    }

    /// <param name="firstUnused">
    /// Every index below it is used - so the search, which returns the first unused overlapping
    /// line, starts there instead of re-walking the used prefix for each line (quadratic).
    /// </param>
    private static int FindOverlapping(Paragraph p, Subtitle other, bool[] used, ref int firstUnused)
    {
        while (firstUnused < used.Length && used[firstUnused])
        {
            firstUnused++;
        }

        for (var i = firstUnused; i < other.Paragraphs.Count; i++)
        {
            if (used[i])
            {
                continue;
            }

            var q = other.Paragraphs[i];
            if (p.StartTime.TotalMilliseconds < q.EndTime.TotalMilliseconds &&
                q.StartTime.TotalMilliseconds < p.EndTime.TotalMilliseconds)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// An Advanced Sub Station Alpha subtitle (header with the two styles) with the lines of
    /// <paramref name="sub1"/> in <paramref name="style1"/> and the lines of <paramref name="sub2"/>
    /// in <paramref name="style2"/>.
    /// </summary>
    public static Subtitle BuildAssaMerge(Subtitle sub1, Subtitle sub2, SsaStyle style1, SsaStyle style2)
    {
        var header = AdvancedSubStationAlpha.DefaultHeader;
        header = AdvancedSubStationAlpha.UpdateOrAddStyle(header, style1);
        header = AdvancedSubStationAlpha.UpdateOrAddStyle(header, style2);
        // Remove the leftover "Default" style
        header = RemoveDefaultStyleFromHeader(header);

        var result = new Subtitle { Header = header };
        foreach (var p in sub1.Paragraphs)
        {
            // Different layer per track so the two streams don't collide/stack against each other when shown at the same time
            var copy = new Paragraph(p) { Extra = style1.Name, Layer = 0 };
            result.Paragraphs.Add(copy);
        }

        foreach (var p in sub2.Paragraphs)
        {
            var copy = new Paragraph(p) { Extra = style2.Name, Layer = 1 };
            result.Paragraphs.Add(copy);
        }

        result.Paragraphs.Sort((a, b) => a.StartTime.TotalMilliseconds.CompareTo(b.StartTime.TotalMilliseconds));
        return result;
    }

    private static string RemoveDefaultStyleFromHeader(string header)
    {
        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(header);
        var keep = styles.Where(p => !string.Equals(p.Name, "Default", StringComparison.OrdinalIgnoreCase)).ToList();
        if (keep.Count == styles.Count || keep.Count == 0)
        {
            return header;
        }

        return AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(header, keep);
    }

    public static SsaStyle BuildSsaStyle(string name, string fontName, int fontSize, bool bold, bool italic,
        Color primary, Color outline, decimal outlineWidth, decimal shadowWidth, bool alignTop)
    {
        return new SsaStyle
        {
            Name = name,
            FontName = fontName,
            FontSize = fontSize,
            Bold = bold,
            Italic = italic,
            Primary = primary.ToSkColor(),
            Secondary = SKColors.Yellow,
            Outline = outline.ToSkColor(),
            Background = SKColors.Black,
            OutlineWidth = outlineWidth,
            ShadowWidth = shadowWidth,
            Alignment = alignTop ? "8" : "2",
            BorderStyle = "1",
            MarginLeft = 10,
            MarginRight = 10,
            MarginVertical = 10,
            ScaleX = 100,
            ScaleY = 100,
            Spacing = 0,
            Angle = 0,
        };
    }
}
