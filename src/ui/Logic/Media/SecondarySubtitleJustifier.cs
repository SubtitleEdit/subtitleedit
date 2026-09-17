using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

            var lineWidths = new decimal[lines.Count];
            var lineHeight = 0m;
            for (var i = 0; i < lines.Count; i++)
            {
                var size = Nikse.SubtitleEdit.Logic.TextMeasurer.MeasureString(Utilities.RemoveSsaTags(lines[i]), style.FontName, (float)style.FontSize, weight);
                lineWidths[i] = (decimal)size.Width + 2 * style.OutlineWidth;
                lineHeight = Math.Max(lineHeight, (decimal)size.Height);
            }

            var blockWidth = lineWidths.Max();
            var blockHeight = lineHeight * lines.Count;
            var blockLeftX = BlockLeftX(HorizontalOf(style.Alignment), playResX, style.MarginLeft, style.MarginRight, blockWidth);
            var blockTopY = BlockTopY(VerticalOf(style.Alignment), playResY, style.MarginVertical, blockHeight);

            for (var i = 0; i < lines.Count; i++)
            {
                var lineX = LineX(targetHorizontal.Value, blockLeftX, blockWidth, lineWidths[i]);
                var lineY = blockTopY + i * lineHeight;
                var pos = "{\\an7\\pos(" + lineX.ToString("0.###", CultureInfo.InvariantCulture) + "," + lineY.ToString("0.###", CultureInfo.InvariantCulture) + ")}";
                result.Add(new Paragraph(p) { Text = pos + lines[i] });
            }
        }

        return result;
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
