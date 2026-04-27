using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Options.Settings.WaveformThemes;

/// <summary>
/// A lightweight preview-only control that draws a synthetic waveform
/// using the colors supplied by <see cref="WaveformThemesViewModel"/>.
/// It repaints whenever any color property on the view-model changes.
/// </summary>
public class WaveformPreviewControl : Control
{
    private readonly WaveformThemesViewModel _vm;

    // Synthetic waveform data generated once at construction time
    private readonly List<(double max, double min)> _peaks;
    private const int SampleCount = 400;

    // Paragraph overlay: show one representative paragraph block
    private const double ParagraphStartFraction = 0.22;
    private const double ParagraphEndFraction = 0.50;

    // Shot-change line positions (as fractions of width)
    private static readonly double[] ShotChangeFractions = [0.18, 0.54, 0.78];

    // Cursor position fraction
    private const double CursorFraction = 0.35;

    public WaveformPreviewControl(WaveformThemesViewModel vm)
    {
        _vm = vm;
        _peaks = GenerateSyntheticPeaks(SampleCount);

        // Repaint whenever any color on the view-model changes
        _vm.PropertyChanged += (_, _) => InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width <= 0 || height <= 0) return;

        var boundsRect = new Rect(0, 0, width, height);
        using var _ = context.PushClip(boundsRect);

        // ── Background ────────────────────────────────────────────────
        context.FillRectangle(new SolidColorBrush(_vm.BackgroundColor), boundsRect);

        // ── Paragraph block ───────────────────────────────────────────
        var pLeft = width * ParagraphStartFraction;
        var pRight = width * ParagraphEndFraction;
        var pWidth = pRight - pLeft;
        context.FillRectangle(new SolidColorBrush(_vm.ParagraphBackgroundColor),
            new Rect(pLeft, 0, pWidth, height));
        context.DrawLine(new Pen(new SolidColorBrush(_vm.ParagraphLeftColor), 2),
            new Point(pLeft, 0), new Point(pLeft, height));
        context.DrawLine(new Pen(new SolidColorBrush(_vm.ParagraphRightColor), 2),
            new Point(pRight, 0), new Point(pRight, height));

        // ── Subtitle text label inside the paragraph ──────────────────
        var textBrush = new SolidColorBrush(_vm.TextColor);
        var typeface = new Typeface(FontFamily.Default);
        var ft = new FormattedText("Sample subtitle text", System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight, typeface, 11, textBrush);
        using (context.PushClip(new Rect(pLeft + 2, 0, pWidth - 4, height)))
        {
            context.DrawText(ft, new Point(pLeft + 4, height * 0.15));
        }

        // ── Waveform lines ────────────────────────────────────────────
        var halfH = height / 2.0;
        var waveformPen = new Pen(new SolidColorBrush(_vm.WaveformColor), 1);
        var selectedPen = new Pen(new SolidColorBrush(_vm.SelectedColor), 1);
        var centerPen = new Pen(Brushes.DarkGray, 0.5);

        context.DrawLine(centerPen, new Point(0, halfH), new Point(width, halfH));

        for (var i = 0; i < SampleCount; i++)
        {
            var x = i / (double)SampleCount * width;
            var (max, min) = _peaks[i];
            var yMax = halfH - max * halfH * 0.9;
            var yMin = halfH - min * halfH * 0.9;

            // Clamp
            yMax = Math.Max(0, Math.Min(height, yMax));
            yMin = Math.Max(0, Math.Min(height, yMin));
            if (yMin < yMax) (yMin, yMax) = (yMax, yMin);
            if (yMin - yMax < 1) yMin = yMax + 1;

            // Use selected color inside the paragraph overlay region
            var isInParagraph = x >= pLeft && x <= pRight;
            var pen = isInParagraph ? selectedPen : waveformPen;
            context.DrawLine(pen, new Point(x, yMax), new Point(x, yMin));
        }

        // ── Shot-change lines ─────────────────────────────────────────
        var shotPen = new Pen(new SolidColorBrush(_vm.ShotChangeColor), 1);
        foreach (var frac in ShotChangeFractions)
        {
            var x = width * frac;
            context.DrawLine(shotPen, new Point(x, 0), new Point(x, height));
        }

        // ── Cursor ────────────────────────────────────────────────────
        var cursorX = width * CursorFraction;
        context.DrawLine(new Pen(new SolidColorBrush(_vm.CursorColor), 1),
            new Point(cursorX, 0), new Point(cursorX, height));

        // ── Border ────────────────────────────────────────────────────
        context.DrawRectangle(null, new Pen(Brushes.Gray, 1), boundsRect);
    }

    // ── Synthetic data ────────────────────────────────────────────────

    private static List<(double max, double min)> GenerateSyntheticPeaks(int count)
    {
        var rng = new Random(42);
        var peaks = new List<(double, double)>(count);
        var envelope = 0.3;
        for (var i = 0; i < count; i++)
        {
            // Slowly varying envelope to mimic real speech
            var t = i / (double)count;
            envelope += (rng.NextDouble() * 0.12 - 0.06);
            envelope = Math.Clamp(envelope, 0.1, 1.0);

            // Shape: louder in the middle third
            var shape = 1.0 - Math.Abs(t - 0.5) * 0.8;
            var amplitude = envelope * shape;

            var maxV = amplitude * (0.5 + rng.NextDouble() * 0.5);
            var minV = -amplitude * (0.5 + rng.NextDouble() * 0.5);
            peaks.Add((maxV, minV));
        }
        return peaks;
    }
}
