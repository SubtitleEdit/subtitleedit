using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaProgressBar;

/// <summary>
/// Progress bar shapes. Each is one ASSA drawing across the whole width that \K fills from left
/// to right over the video's duration, so a shape made of many pieces (segments, dots) fills
/// piece by piece. Drawn in a width x height box with 0,0 top-left.
/// </summary>
public static class ProgressBarStyles
{
    public const int Square = 0;
    public const int Rounded = 1;

    public static IReadOnlyList<string> Names
    {
        get
        {
            var l = Se.Language.Assa;
            return
            [
                l.ProgressBarSquareCorners,
                l.ProgressBarRoundedCorners,
                l.ProgressBarStyleCutCorners,
                l.ProgressBarStyleArrow,
                l.ProgressBarStyleSlanted,
                l.ProgressBarStyleSegments,
                l.ProgressBarStyleRoundedSegments,
                l.ProgressBarStyleDots,
                l.ProgressBarStyleDiamonds,
                l.ProgressBarStyleChevrons,
                l.ProgressBarStyleStripes,
                l.ProgressBarStyleWave,
                l.ProgressBarStyleZigzag,
                l.ProgressBarStyleTapered,
                l.ProgressBarStyleRuler,
                l.ProgressBarStyleDoubleLine,
                l.ProgressBarStyleEqualizer,
                l.ProgressBarStyleStars,
            ];
        }
    }

    /// <summary>
    /// The drawing commands (no tags) of a style.
    /// </summary>
    public static string Drawing(int style, int width, int height)
    {
        var w = Math.Max(10, width);
        var h = Math.Max(2, height);
        return style switch
        {
            1 => RoundedRect(0, 0, w, h, h / 2.0),
            2 => CutCorners(w, h),
            3 => $"m 0 0 l {N(w - h / 2.0)} 0 {w} {N(h / 2.0)} {N(w - h / 2.0)} {h} 0 {h}",
            4 => $"m {N(h * 0.6)} 0 l {w} 0 {N(w - h * 0.6)} {h} 0 {h}",
            5 => Segments(w, h, rounded: false),
            6 => Segments(w, h, rounded: true),
            7 => Dots(w, h),
            8 => Diamonds(w, h),
            9 => Chevrons(w, h),
            10 => Stripes(w, h),
            11 => Wave(w, h),
            12 => Zigzag(w, h),
            13 => $"m 0 {N(h * 0.7)} l {w} 0 {w} {h} 0 {h}",
            14 => Ruler(w, h),
            15 => Rect(0, 0, w, h * 0.3) + " " + Rect(0, h * 0.7, w, h),
            16 => Equalizer(w, h),
            17 => Stars(w, h),
            _ => $"m 0 0 l {w} 0 {w} {h} 0 {h}",
        };
    }

    private static string N(double value) => Math.Round(value, 1).ToString("0.#", CultureInfo.InvariantCulture);

    private static string Rect(double left, double top, double right, double bottom) =>
        $"m {N(left)} {N(top)} l {N(right)} {N(top)} {N(right)} {N(bottom)} {N(left)} {N(bottom)}";

    internal static string RoundedRect(double left, double top, double right, double bottom, double radius)
    {
        var r = Math.Min(radius, Math.Min((right - left) / 2, (bottom - top) / 2));
        if (r < 0.5)
        {
            return Rect(left, top, right, bottom);
        }

        // Quarter circles as cubic beziers (control points at 0.552 of the radius)
        var k = r * 0.5523;
        return $"m {N(left + r)} {N(top)} l {N(right - r)} {N(top)} " +
               $"b {N(right - r + k)} {N(top)} {N(right)} {N(top + r - k)} {N(right)} {N(top + r)} " +
               $"l {N(right)} {N(bottom - r)} " +
               $"b {N(right)} {N(bottom - r + k)} {N(right - r + k)} {N(bottom)} {N(right - r)} {N(bottom)} " +
               $"l {N(left + r)} {N(bottom)} " +
               $"b {N(left + r - k)} {N(bottom)} {N(left)} {N(bottom - r + k)} {N(left)} {N(bottom - r)} " +
               $"l {N(left)} {N(top + r)} " +
               $"b {N(left)} {N(top + r - k)} {N(left + r - k)} {N(top)} {N(left + r)} {N(top)}";
    }

    private static string CutCorners(int w, int h)
    {
        var c = h / 3.0;
        return $"m {N(c)} 0 l {N(w - c)} 0 {w} {N(c)} {w} {N(h - c)} {N(w - c)} {h} {N(c)} {h} 0 {N(h - c)} 0 {N(c)}";
    }

    /// <summary>
    /// Even pieces across the width: a piece is about twice as wide as high, a gap a quarter of the height.
    /// </summary>
    private static (int Count, double PieceWidth, double Gap) Pieces(int w, double pieceWidth, double gap)
    {
        var count = Math.Max(2, (int)Math.Round((w + gap) / (pieceWidth + gap)));
        var width = (w + gap) / count - gap;
        return (count, Math.Max(1, width), gap);
    }

    private static string Segments(int w, int h, bool rounded)
    {
        var (count, pieceWidth, gap) = Pieces(w, h * 2.0, Math.Max(2, h / 4.0));
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            var x = i * (pieceWidth + gap);
            sb.Append(rounded ? RoundedRect(x, 0, x + pieceWidth, h, h / 2.0) : Rect(x, 0, x + pieceWidth, h)).Append(' ');
        }

        return sb.ToString().TrimEnd();
    }

    private static string Dots(int w, int h)
    {
        var (count, diameter, gap) = Pieces(w, h, Math.Max(2, h / 3.0));
        var r = Math.Min(diameter, h) / 2.0;
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            var cx = i * (diameter + gap) + diameter / 2;
            sb.Append(RoundedRect(cx - r, h / 2.0 - r, cx + r, h / 2.0 + r, r)).Append(' ');
        }

        return sb.ToString().TrimEnd();
    }

    private static string Diamonds(int w, int h)
    {
        var (count, size, gap) = Pieces(w, h, Math.Max(1, h / 6.0));
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            var x = i * (size + gap);
            sb.Append($"m {N(x + size / 2)} 0 l {N(x + size)} {N(h / 2.0)} {N(x + size / 2)} {h} {N(x)} {N(h / 2.0)} ");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// A row of "&gt;" arrows pointing the way the bar fills.
    /// </summary>
    private static string Chevrons(int w, int h)
    {
        var half = h / 2.0;
        var thickness = h * 0.4;
        var (count, pieceWidth, gap) = Pieces(w, thickness + half, Math.Max(2, h / 5.0));
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            var x = i * (pieceWidth + gap);
            var t = pieceWidth - half;
            sb.Append($"m {N(x)} 0 l {N(x + t)} 0 {N(x + t + half)} {N(half)} {N(x + t)} {h} {N(x)} {h} {N(x + half)} {N(half)} ");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Slanted blocks, like a barber pole.
    /// </summary>
    private static string Stripes(int w, int h)
    {
        var slant = h * 0.6;
        var (count, pieceWidth, gap) = Pieces(w - (int)slant, h * 0.9, Math.Max(2, h / 3.0));
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            var x = slant + i * (pieceWidth + gap);
            sb.Append($"m {N(x)} 0 l {N(x + pieceWidth)} 0 {N(x + pieceWidth - slant)} {h} {N(x - slant)} {h} ");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// A band that rises and falls: top and bottom edges are the same wave, half the height apart.
    /// </summary>
    private static string Wave(int w, int h)
    {
        var amplitude = h / 4.0;
        var thickness = h / 2.0;
        var halfWaves = Math.Max(4, (int)Math.Round(w / (h * 1.5)));
        if (halfWaves % 2 == 1)
        {
            halfWaves++;
        }

        var step = w / (double)halfWaves;
        var control = amplitude * 4 / 3;
        var sb = new StringBuilder($"m 0 {N(amplitude)} ");
        for (var i = 0; i < halfWaves; i++)
        {
            var x = i * step;
            var y = amplitude + (i % 2 == 0 ? -control : control);
            sb.Append($"b {N(x + step / 3)} {N(y)} {N(x + step * 2 / 3)} {N(y)} {N(x + step)} {N(amplitude)} ");
        }

        sb.Append($"l {w} {N(amplitude + thickness)} ");
        for (var i = halfWaves - 1; i >= 0; i--)
        {
            var x = i * step;
            var y = amplitude + thickness + (i % 2 == 0 ? -control : control);
            sb.Append($"b {N(x + step * 2 / 3)} {N(y)} {N(x + step / 3)} {N(y)} {N(x)} {N(amplitude + thickness)} ");
        }

        return sb.ToString().TrimEnd();
    }

    private static string Zigzag(int w, int h)
    {
        var amplitude = h / 3.0;
        var thickness = h - amplitude;
        var teeth = Math.Max(4, (int)Math.Round(w / (h * 1.0)));
        var step = w / (double)teeth;
        var sb = new StringBuilder("m 0 0 l ");
        for (var i = 1; i <= teeth; i++)
        {
            sb.Append($"{N(i * step)} {N(i % 2 == 1 ? amplitude : 0)} ");
        }

        for (var i = teeth; i >= 0; i--)
        {
            sb.Append($"{N(i * step)} {N((i % 2 == 1 ? amplitude : 0) + thickness)} ");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Vertical bars of varying height standing on the bottom edge, like an audio level meter.
    /// The heights follow a fixed pattern, so the bar looks the same every time.
    /// </summary>
    private static string Equalizer(int w, int h)
    {
        var (count, barWidth, gap) = Pieces(w, Math.Max(2, h * 0.35), Math.Max(1, h * 0.2));
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            var x = i * (barWidth + gap);
            var level = 0.3 + 0.7 * Math.Abs(Math.Sin(i * 0.9) * 0.6 + Math.Sin(i * 0.37) * 0.4);
            sb.Append(Rect(x, h - h * Math.Min(1, level), x + barWidth, h)).Append(' ');
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// A row of five-pointed stars.
    /// </summary>
    private static string Stars(int w, int h)
    {
        var (count, size, gap) = Pieces(w, h, Math.Max(1, h / 5.0));
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            var cx = i * (size + gap) + size / 2;
            var outer = Math.Min(size, h) / 2.0;
            var cy = outer * 1.05; // the five points sit a little high - nudge down to fill the height
            sb.Append('m');
            for (var p = 0; p < 10; p++)
            {
                var angle = (-90 + 36 * p) * Math.PI / 180;
                var r = p % 2 == 0 ? outer : outer * 0.42;
                sb.Append(' ').Append(N(cx + r * Math.Cos(angle))).Append(' ').Append(N(Math.Min(h, cy + r * Math.Sin(angle))));
                if (p == 0)
                {
                    sb.Append(" l");
                }
            }

            sb.Append(' ');
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// A low bar with a tick every 5% - the ticks fill too.
    /// </summary>
    private static string Ruler(int w, int h)
    {
        var baseTop = h * 0.6;
        var tick = Math.Max(2, h / 10.0);
        var sb = new StringBuilder(Rect(0, baseTop, w, h));
        for (var i = 0; i <= 20; i++)
        {
            var x = Math.Min(w - tick, i * w / 20.0);
            var top = i % 2 == 0 ? 0 : h * 0.3;
            sb.Append(' ').Append(Rect(x, top, x + tick, baseTop + 1));
        }

        return sb.ToString();
    }
}
