using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Splits a multi-line secondary subtitle paragraph into one single-line event per line, each
/// with its own "\an7\pos(x,y)" override, so the lines can be left/center/right justified
/// independently of mpv's own "sub-ass-justify" option - which only ever arranges the lines
/// inside a single event, and is a single player-wide setting with no per-track equivalent.
/// </summary>
public static class SecondarySubtitleJustifier
{
    private enum Horizontal { Left, Center, Right }
    private enum Vertical { Top, Middle, Bottom }

    public static List<Paragraph> Apply(IEnumerable<Paragraph> paragraphs, SsaStyle style, string justifyCode, decimal playResX, decimal playResY)
    {
        var targetHorizontal = ParseHorizontal(justifyCode);
        if (targetHorizontal == null)
        {
            return paragraphs.Select(p => new Paragraph(p)).ToList();
        }

        var weight = style.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
        var result = new List<Paragraph>();
        foreach (var p in paragraphs)
        {
            var lines = (p.Text ?? string.Empty).SplitToLines();
            if (lines.Count < 2)
            {
                result.Add(new Paragraph(p));
                continue;
            }

            // One tag syntax: "<i>" and "<font>" become "{\i1}" and "{\c...}" now rather than at
            // save - on the whole text, where each tag still has its closing tag - so the
            // carried-over state below sees them too.
            lines = AdvancedSubStationAlpha.FormatText(p.Text!).SplitToLines();

            // libass sizes a font so its line height (ascent + descent) equals the style's font
            // size, while Skia's size is the em size - so scale Skia's widths down to libass's,
            // and step one font size per line, or the block drifts and the lines spread out.
            var lineWidths = new decimal[lines.Count];
            var lineHeight = (decimal)style.FontSize;
            for (var i = 0; i < lines.Count; i++)
            {
                var size = Nikse.SubtitleEdit.Logic.TextMeasurer.MeasureString(Utilities.RemoveSsaTags(lines[i]), style.FontName, (float)style.FontSize, weight);
                var scale = size.Height > 0 ? lineHeight / (decimal)size.Height : 1m;
                lineWidths[i] = (decimal)size.Width * scale;
            }

            var blockWidth = lineWidths.Max();
            var blockHeight = lineHeight * lines.Count;
            var blockLeftX = BlockLeftX(HorizontalOf(style.Alignment), playResX, style.MarginLeft, style.MarginRight, blockWidth);
            var blockTopY = BlockTopY(VerticalOf(style.Alignment), playResY, style.MarginVertical, blockHeight);

            // Each line becomes its own event, so a tag left open on an earlier line (italic
            // across "{\i1}one\Ntwo{\i0}") has to be repeated to still apply to the later ones.
            var carried = new StringBuilder();
            for (var i = 0; i < lines.Count; i++)
            {
                var lineX = LineX(targetHorizontal.Value, blockLeftX, blockWidth, lineWidths[i]);
                var lineY = blockTopY + i * lineHeight;
                var pos = "{\\an7\\pos(" + lineX.ToString("0.###", CultureInfo.InvariantCulture) + "," + lineY.ToString("0.###", CultureInfo.InvariantCulture) + ")" + carried + "}";
                result.Add(new Paragraph(p) { Text = pos + lines[i] });
                AppendCarriedTags(lines[i], carried);
            }
        }

        return result;
    }

    /// <summary>
    /// Appends the override tags in <paramref name="line"/> that keep applying to the text after
    /// them (style tags like \i, \b, \c, \fn, \r) - in order, so a later tag still wins, just
    /// as it would have in one event. Tags that belong to the whole event (\pos, \an, \fad,
    /// \clip, ...), karaoke timing, animations and drawings are left out.
    /// </summary>
    private static void AppendCarriedTags(string line, StringBuilder carried)
    {
        var blockStart = line.IndexOf('{');
        while (blockStart >= 0)
        {
            var blockEnd = line.IndexOf('}', blockStart + 1);
            if (blockEnd < 0)
            {
                return;
            }

            var block = line.Substring(blockStart + 1, blockEnd - blockStart - 1);
            var tagStart = block.IndexOf('\\');
            while (tagStart >= 0)
            {
                // A tag runs to the next backslash outside parentheses: "\t(\i1)" is one tag.
                var tagEnd = tagStart + 1;
                var depth = 0;
                while (tagEnd < block.Length && (depth > 0 || block[tagEnd] != '\\'))
                {
                    if (block[tagEnd] == '(')
                    {
                        depth++;
                    }
                    else if (block[tagEnd] == ')' && depth > 0)
                    {
                        depth--;
                    }

                    tagEnd++;
                }

                var tag = block.Substring(tagStart, tagEnd - tagStart).TrimEnd();
                if (IsCarriedTag(tag.Substring(1)))
                {
                    carried.Append(tag);
                }

                tagStart = tagEnd < block.Length ? tagEnd : -1;
            }

            blockStart = line.IndexOf('{', blockEnd + 1);
        }
    }

    private static bool IsCarriedTag(string tag)
    {
        if (tag.Length == 0 ||
            tag.StartsWith("pos", StringComparison.Ordinal) ||
            tag.StartsWith("move", StringComparison.Ordinal) ||
            tag.StartsWith("an", StringComparison.Ordinal) ||
            tag.StartsWith("org", StringComparison.Ordinal) ||
            tag.StartsWith("fad", StringComparison.Ordinal) ||
            tag.StartsWith("clip", StringComparison.Ordinal) ||
            tag.StartsWith("iclip", StringComparison.Ordinal) ||
            tag.StartsWith("t(", StringComparison.Ordinal) ||
            tag.StartsWith("k", StringComparison.OrdinalIgnoreCase) ||
            tag[0] == 'q')
        {
            return false;
        }

        // Legacy "\a5" alignment and "\p1" drawing mode (but not "\alpha" or "\pbo").
        if ((tag[0] == 'a' || tag[0] == 'p') && tag.Length > 1 && char.IsDigit(tag[1]))
        {
            return false;
        }

        return true;
    }

    private static Horizontal? ParseHorizontal(string justifyCode)
    {
        return justifyCode switch
        {
            "left" => Horizontal.Left,
            "center" => Horizontal.Center,
            "right" => Horizontal.Right,
            _ => null, // "auto" (or unset) leaves mpv's own sub-ass-justify setting in control.
        };
    }

    private static Horizontal HorizontalOf(string alignmentCode)
    {
        return alignmentCode switch
        {
            "1" or "4" or "7" => Horizontal.Left,
            "3" or "6" or "9" => Horizontal.Right,
            _ => Horizontal.Center,
        };
    }

    private static Vertical VerticalOf(string alignmentCode)
    {
        return alignmentCode switch
        {
            "1" or "2" or "3" => Vertical.Bottom,
            "7" or "8" or "9" => Vertical.Top,
            _ => Vertical.Middle,
        };
    }

    private static decimal BlockLeftX(Horizontal horizontal, decimal playResX, int marginLeft, int marginRight, decimal blockWidth)
    {
        return horizontal switch
        {
            Horizontal.Left => marginLeft,
            Horizontal.Right => playResX - marginRight - blockWidth,
            _ => (playResX - blockWidth) / 2,
        };
    }

    private static decimal BlockTopY(Vertical vertical, decimal playResY, int marginVertical, decimal blockHeight)
    {
        return vertical switch
        {
            Vertical.Top => marginVertical,
            Vertical.Bottom => playResY - marginVertical - blockHeight,
            _ => (playResY - blockHeight) / 2,
        };
    }

    private static decimal LineX(Horizontal horizontal, decimal blockLeftX, decimal blockWidth, decimal lineWidth)
    {
        return horizontal switch
        {
            Horizontal.Left => blockLeftX,
            Horizontal.Right => blockLeftX + (blockWidth - lineWidth),
            _ => blockLeftX + (blockWidth - lineWidth) / 2,
        };
    }
}
