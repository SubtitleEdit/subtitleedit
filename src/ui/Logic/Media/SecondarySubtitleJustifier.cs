using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Gives the secondary subtitle its own "Justify lines" (left/center/right), independent of
/// both the primary subtitle and the video player's own global "Text Justify" setting (#14842).
///
/// mpv's line-justify (sub-justify/sub-ass-justify) is a single player-wide option that overrides
/// line justification for every ASS event currently being rendered - including, if applied per
/// line via a bare `\an` override, the secondary's own. So this never asks mpv to justify
/// anything: for a multi-line paragraph whose justify is not "auto", each physical line becomes
/// its own single-line dialogue event with its own `\an7\pos(x,y)` - sub-ass-justify has nothing
/// to arrange within a one-line event, regardless of what the global setting is. The block's
/// overall position still comes from the paragraph's own alignment (top/middle/bottom,
/// left/center/right), exactly as it would without this - only which corner of the block
/// individual lines line up against changes.
///
/// This measures text with Skia, which uses different font metrics than libass, and does not
/// attempt to replicate libass's own line spacing - so the block this computes is close to, but
/// not pixel-identical to, what an unjustified render of the same lines would produce. It also
/// means `\pos` replaces libass's own collision handling for these events: two secondary
/// paragraphs that overlap in time will overlap on screen instead of stacking.
/// </summary>
public static class SecondarySubtitleJustifier
{
    /// <summary>
    /// Returns a new list: paragraphs that do not need to change are copied through unchanged
    /// (never the original objects - callers that go on to mutate Extra/Layer, or a caller that
    /// merges this into a preview refreshed many times a second, must not share paragraphs with
    /// the subtitle this was built from).
    /// </summary>
    public static List<Paragraph> Apply(IEnumerable<Paragraph> paragraphs, SsaStyle style, string justifyCode, decimal playResX, decimal playResY)
    {
        var result = new List<Paragraph>();

        if (string.IsNullOrEmpty(justifyCode) || justifyCode == "auto")
        {
            result.AddRange(paragraphs.Select(p => new Paragraph(p)));
            return result;
        }

        var vertical = VerticalOf(style.Alignment);
        var horizontal = HorizontalOf(style.Alignment);
        var targetHorizontal = ParseHorizontal(justifyCode);
        var weight = style.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;

        foreach (var p in paragraphs)
        {
            var lines = p.Text.SplitToLines();
            if (lines.Count < 2)
            {
                result.Add(new Paragraph(p)); // nothing to justify a single line against
                continue;
            }

            var lineMetrics = lines
                .Select(line => TextMeasurer.MeasureString(Utilities.RemoveSsaTags(line), style.FontName, (float)style.FontSize, weight))
                .ToList();

            // The outline extends the visible glyph bounds on both sides; without it the block
            // (and so every justified line) would sit slightly narrower than what actually
            // renders. Still an approximation - Skia's own font metrics differ from libass's.
            var outline = 2 * style.OutlineWidth;
            var lineWidths = lineMetrics.Select(m => (decimal)m.Width + outline).ToList();
            var blockWidth = lineWidths.Max();
            var lineHeight = (decimal)lineMetrics.Max(m => m.Height);
            var blockHeight = lineHeight * lines.Count;

            var blockLeftX = BlockLeftX(horizontal, playResX, style.MarginLeft, style.MarginRight, blockWidth);
            var blockTopY = BlockTopY(vertical, playResY, style.MarginVertical, blockHeight);

            for (var i = 0; i < lines.Count; i++)
            {
                var lineX = LineX(targetHorizontal, blockLeftX, blockWidth, lineWidths[i]);
                var lineY = blockTopY + i * lineHeight;

                var line = new Paragraph(p)
                {
                    Text = "{\\an7\\pos(" +
                           lineX.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                           lineY.ToString("0.###", CultureInfo.InvariantCulture) + ")}" + lines[i],
                };
                result.Add(line);
            }
        }

        return result;
    }

    private enum Horizontal { Left, Center, Right }

    private enum Vertical { Top, Middle, Bottom }

    private static Horizontal HorizontalOf(string alignmentCode)
    {
        switch (alignmentCode)
        {
            case "1":
            case "4":
            case "7":
                return Horizontal.Left;
            case "3":
            case "6":
            case "9":
                return Horizontal.Right;
            default: // "2", "5", "8" and any unexpected value
                return Horizontal.Center;
        }
    }

    private static Vertical VerticalOf(string alignmentCode)
    {
        switch (alignmentCode)
        {
            case "7":
            case "8":
            case "9":
                return Vertical.Top;
            case "4":
            case "5":
            case "6":
                return Vertical.Middle;
            default: // "1", "2", "3" and any unexpected value
                return Vertical.Bottom;
        }
    }

    private static Horizontal ParseHorizontal(string justifyCode)
    {
        switch (justifyCode)
        {
            case "left":
                return Horizontal.Left;
            case "right":
                return Horizontal.Right;
            default: // "center" and any unexpected value
                return Horizontal.Center;
        }
    }

    private static decimal BlockLeftX(Horizontal horizontal, decimal playResX, decimal marginLeft, decimal marginRight, decimal blockWidth)
    {
        switch (horizontal)
        {
            case Horizontal.Left:
                return marginLeft;
            case Horizontal.Right:
                return playResX - marginRight - blockWidth;
            default:
                return (playResX - blockWidth) / 2m;
        }
    }

    private static decimal BlockTopY(Vertical vertical, decimal playResY, decimal marginVertical, decimal blockHeight)
    {
        switch (vertical)
        {
            case Vertical.Top:
                return marginVertical;
            case Vertical.Bottom:
                return playResY - marginVertical - blockHeight;
            default:
                return (playResY - blockHeight) / 2m;
        }
    }

    private static decimal LineX(Horizontal targetHorizontal, decimal blockLeftX, decimal blockWidth, decimal lineWidth)
    {
        switch (targetHorizontal)
        {
            case Horizontal.Left:
                return blockLeftX;
            case Horizontal.Right:
                return blockLeftX + blockWidth - lineWidth;
            default:
                return blockLeftX + (blockWidth - lineWidth) / 2m;
        }
    }
}
