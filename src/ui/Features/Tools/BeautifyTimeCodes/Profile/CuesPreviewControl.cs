using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes.Profile;

/// <summary>
/// Lightweight custom-drawn preview: gray bg, two paragraph blocks straddling a
/// center shot-change line, with red/green zones overlaid. Mirrors the SE4
/// <c>CuesPreviewView</c> appearance pixel-for-pixel.
/// </summary>
public sealed class CuesPreviewControl : Control
{
    private const double WindowSeconds = 3.0;

    private double _frameRate = 25.0;
    private string _previewText = string.Empty;
    private int _leftGap;
    private int _leftRedZone = 7;
    private int _leftGreenZone = 12;
    private int _rightGap;
    private int _rightRedZone = 7;
    private int _rightGreenZone = 12;

    public double FrameRate { get => _frameRate; set { _frameRate = value; InvalidateVisual(); } }
    public string PreviewText { get => _previewText; set { _previewText = value; InvalidateVisual(); } }
    public int LeftGap { get => _leftGap; set { _leftGap = value; InvalidateVisual(); } }
    public int LeftRedZone { get => _leftRedZone; set { _leftRedZone = value; InvalidateVisual(); } }
    public int LeftGreenZone { get => _leftGreenZone; set { _leftGreenZone = value; InvalidateVisual(); } }
    public int RightGap { get => _rightGap; set { _rightGap = value; InvalidateVisual(); } }
    public int RightRedZone { get => _rightRedZone; set { _rightRedZone = value; InvalidateVisual(); } }
    public int RightGreenZone { get => _rightGreenZone; set { _rightGreenZone = value; InvalidateVisual(); } }

    public CuesPreviewControl()
    {
        MinHeight = 70;
        Height = 70;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        ClipToBounds = true;
    }

    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0 || _frameRate <= 0)
        {
            return;
        }

        var rect = new Rect(0, 0, w, h);
        using var _ = context.PushClip(rect);

        // Background (gap area) — lighter than the dark-mode window background so paragraph blocks stand out
        context.FillRectangle(new SolidColorBrush(Color.FromRgb(168, 168, 168)), rect);

        var pxPerFrame = w / (_frameRate * WindowSeconds);
        var center = w / 2.0;

        // Green zones
        var greenBrush = new SolidColorBrush(Color.FromRgb(0, 110, 0));
        if (_leftGreenZone > 0)
        {
            var width = _leftGreenZone * pxPerFrame;
            context.FillRectangle(greenBrush, new Rect(center - width, 0, width, h));
        }
        if (_rightGreenZone > 0)
        {
            context.FillRectangle(greenBrush, new Rect(center, 0, _rightGreenZone * pxPerFrame, h));
        }

        // Red zones (drawn on top of green)
        var redBrush = new SolidColorBrush(Color.FromRgb(178, 34, 34)); // firebrick
        if (_leftRedZone > 0)
        {
            var width = _leftRedZone * pxPerFrame;
            context.FillRectangle(redBrush, new Rect(center - width, 0, width, h));
        }
        if (_rightRedZone > 0)
        {
            context.FillRectangle(redBrush, new Rect(center, 0, _rightRedZone * pxPerFrame, h));
        }

        // Paragraph blocks (semi-transparent black) — left ends before gap, right starts after gap
        var paragraphBrush = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0));
        var leftEnd = center - _leftGap * pxPerFrame;
        var rightStart = center + _rightGap * pxPerFrame;
        if (leftEnd > 0)
        {
            context.FillRectangle(paragraphBrush, new Rect(0, 0, leftEnd, h));
        }
        if (rightStart < w)
        {
            context.FillRectangle(paragraphBrush, new Rect(rightStart, 0, w - rightStart, h));
        }

        // Subtitle labels (only when the paragraph has enough room)
        var textBrush = new SolidColorBrush(Colors.White);
        var typeface = new Typeface(FontFamily.Default);
        if (_leftGap <= 12 && leftEnd > 40)
        {
            DrawLabel(context, typeface, textBrush, 4, 4, leftEnd - 8, h - 8, 1000, GetLeftOutCueMs());
        }
        if (_rightGap <= 12 && (w - rightStart) > 40)
        {
            DrawLabel(context, typeface, textBrush, rightStart + 4, 4, w - rightStart - 8, h - 8, GetRightInCueMs(), 5000);
        }

        // Shot change line — pale green, 3px
        var shotBrush = new SolidColorBrush(Color.FromRgb(152, 251, 152)); // PaleGreen
        context.FillRectangle(shotBrush, new Rect(center - 1.5, 0, 3, h));
    }

    private double GetLeftOutCueMs() => 3000 - _leftGap * (1000.0 / _frameRate);
    private double GetRightInCueMs() => 3000 + _rightGap * (1000.0 / _frameRate);

    private void DrawLabel(DrawingContext context, Typeface typeface, IBrush brush,
        double x, double y, double maxW, double maxH, double startMs, double endMs)
    {
        if (maxW <= 0 || maxH <= 0)
        {
            return;
        }

        var label = $"{FormatTimeCode(startMs)} --> {FormatTimeCode(endMs)}\n{_previewText}";
        var ft = new FormattedText(
            label,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            11,
            brush)
        {
            MaxTextWidth = maxW,
            MaxTextHeight = maxH,
            TextAlignment = TextAlignment.Left,
        };

        using var _ = context.PushClip(new Rect(x, y, maxW, maxH));
        context.DrawText(ft, new Point(x, y));
    }

    private static string FormatTimeCode(double totalMs)
    {
        var ts = TimeSpan.FromMilliseconds(totalMs);
        return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2},{ts.Milliseconds:D3}";
    }
}
