using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Gives the secondary subtitle its own "Justify lines" (left/center/right), independent of
/// both the primary subtitle and the video player's own global "Text Justify" setting (#14842).
///
/// mpv's line-justify (sub-justify/sub-ass-justify) is a single player-wide option, and the
/// secondary subtitle is pushed to the same player as the primary (<see cref="SecondarySubtitleMerger"/>),
/// so there is no per-track mpv setting to flip for just the secondary. mpv's own secondary
/// subtitle track feature (--secondary-sid) has no justify equivalent either, and forces the
/// secondary to the top of the screen - not useful here even if this app used it.
///
/// Instead this bypasses mpv's justify entirely: for a multi-line paragraph whose requested
/// justify differs from what its alignment already produces on its own, it measures the widest
/// line (the paragraph's own authored line breaks - never re-wrapped by libass here, so the
/// measured lines are exactly what gets rendered) and adds a `\pos` override that keeps the
/// block anchored exactly where the alignment already puts it, re-anchored to the corner that
/// matches the requested justify. libass still lays the lines out itself from there - this only
/// changes which corner of the block the position refers to, not how the lines are spaced,
/// wrapped or shaped, so multi-line spacing and RTL/complex-script shaping stay libass's own.
/// </summary>
public static class SecondarySubtitleJustifier
{
    public static void Apply(IList<Paragraph> paragraphs, SsaStyle style, string justifyCode, decimal playResX, decimal playResY)
    {
        if (paragraphs.Count == 0 || string.IsNullOrEmpty(justifyCode) || justifyCode == "auto")
        {
            return;
        }

        var originalHorizontal = HorizontalOf(style.Alignment);
        var targetHorizontal = ParseHorizontal(justifyCode);
        if (originalHorizontal == targetHorizontal)
        {
            return; // the alignment alone already produces this justify - nothing to override
        }

        var vertical = VerticalOf(style.Alignment);
        var newAlignmentCode = ToAlignmentCode(vertical, targetHorizontal);
        var posY = VerticalAnchorY(vertical, playResY, style.MarginVertical);
        var anchorX = HorizontalAnchorX(originalHorizontal, playResX, style.MarginLeft, style.MarginRight);
        var weight = style.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;

        foreach (var p in paragraphs)
        {
            var lines = p.Text.SplitToLines();
            if (lines.Count < 2)
            {
                continue; // justify only ever affects how several lines relate to each other
            }

            var blockWidth = 0.0;
            foreach (var line in lines)
            {
                var lineWidth = TextMeasurer.MeasureString(Utilities.RemoveSsaTags(line), style.FontName, (float)style.FontSize, weight).Width;
                if (lineWidth > blockWidth)
                {
                    blockWidth = lineWidth;
                }
            }

            var posX = TargetAnchorX(originalHorizontal, targetHorizontal, anchorX, (decimal)blockWidth);
            p.Text = "{\\an" + newAlignmentCode + "\\pos(" +
                     posX.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                     posY.ToString("0.###", CultureInfo.InvariantCulture) + ")}" + p.Text;
        }
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

    private static string ToAlignmentCode(Vertical vertical, Horizontal horizontal)
    {
        switch (vertical)
        {
            case Vertical.Top:
                return horizontal == Horizontal.Left ? "7" : horizontal == Horizontal.Right ? "9" : "8";
            case Vertical.Middle:
                return horizontal == Horizontal.Left ? "4" : horizontal == Horizontal.Right ? "6" : "5";
            default:
                return horizontal == Horizontal.Left ? "1" : horizontal == Horizontal.Right ? "3" : "2";
        }
    }

    private static decimal VerticalAnchorY(Vertical vertical, decimal playResY, decimal marginVertical)
    {
        switch (vertical)
        {
            case Vertical.Top:
                return marginVertical;
            case Vertical.Bottom:
                return playResY - marginVertical;
            default:
                return playResY / 2m;
        }
    }

    private static decimal HorizontalAnchorX(Horizontal horizontal, decimal playResX, decimal marginLeft, decimal marginRight)
    {
        switch (horizontal)
        {
            case Horizontal.Left:
                return marginLeft;
            case Horizontal.Right:
                return playResX - marginRight;
            default:
                return playResX / 2m;
        }
    }

    /// <summary>
    /// The block's left/right edge comes from where the *original* alignment puts it (so the
    /// block does not move just because its lines get re-justified), then the requested justify
    /// picks which corner of that same block the new `\pos` anchors to.
    /// </summary>
    private static decimal TargetAnchorX(Horizontal originalHorizontal, Horizontal targetHorizontal, decimal originalAnchorX, decimal blockWidth)
    {
        var blockLeftX = originalHorizontal switch
        {
            Horizontal.Left => originalAnchorX,
            Horizontal.Right => originalAnchorX - blockWidth,
            _ => originalAnchorX - blockWidth / 2m,
        };

        return targetHorizontal switch
        {
            Horizontal.Left => blockLeftX,
            Horizontal.Right => blockLeftX + blockWidth,
            _ => blockLeftX + blockWidth / 2m,
        };
    }
}
