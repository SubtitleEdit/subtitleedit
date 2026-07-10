using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

/// <summary>
/// Full-frame position map for image-based subtitles (discussion #12318): draws the
/// video canvas with letterbox bars and every subtitle's real rectangle, color coded
/// by zone (green = active picture, amber = bottom bar, red = top bar). The selected
/// subtitle renders its actual decoded bitmap. Clicking a rectangle selects that line.
/// </summary>
public class PositionMonitorControl : Control
{
    public ObservableCollection<BinarySubtitleItem>? Items { get; set; }
    public BinarySubtitleItem? SelectedItem { get; set; }
    public int ScreenWidth { get; set; } = 1920;
    public int ScreenHeight { get; set; } = 1080;
    public int BarHeight { get; set; }
    public bool ShowTitleSafe { get; set; }
    public double TitleSafePercent { get; set; } = 5;

    public event EventHandler<BinarySubtitleItem>? ItemClicked;

    private const double FramePadding = 12;

    private static readonly Color ZoneGreen = Color.FromRgb(0x22, 0xC5, 0x5E);
    private static readonly Color ZoneAmber = Color.FromRgb(0xF5, 0x9E, 0x0B);
    private static readonly Color ZoneRed = Color.FromRgb(0xEF, 0x44, 0x44);

    private static readonly IBrush FrameBrush = new SolidColorBrush(Color.FromRgb(0x0D, 0x0D, 0x0D));
    private static readonly IBrush BarBrush = new SolidColorBrush(Color.FromArgb(0x14, 0xFF, 0xFF, 0xFF));
    private static readonly Pen FrameBorderPen = new(new SolidColorBrush(Color.FromArgb(0x50, 0xFF, 0xFF, 0xFF)), 1);
    private static readonly Pen BarSeparatorPen = new(new SolidColorBrush(Color.FromArgb(0x60, 0xFF, 0xFF, 0xFF)), 1)
    {
        DashStyle = new DashStyle(new AvaloniaList<double> { 4, 4 }, 0),
    };
    private static readonly Pen TitleSafePen = new(new SolidColorBrush(Color.FromArgb(0x48, 0xFF, 0xFF, 0xFF)), 1)
    {
        DashStyle = new DashStyle(new AvaloniaList<double> { 2, 4 }, 0),
    };
    private static readonly Pen SelectedPen = new(Brushes.White, 2);

    // Cached per-zone brushes/pens - Render runs over every subtitle in the file,
    // so avoid per-item allocations.
    private static readonly IBrush GreenFill = new SolidColorBrush(ZoneGreen, 0.26);
    private static readonly IBrush AmberFill = new SolidColorBrush(ZoneAmber, 0.26);
    private static readonly IBrush RedFill = new SolidColorBrush(ZoneRed, 0.26);
    private static readonly Pen GreenPen = new(new SolidColorBrush(ZoneGreen, 0.9), 1);
    private static readonly Pen AmberPen = new(new SolidColorBrush(ZoneAmber, 0.9), 1);
    private static readonly Pen RedPen = new(new SolidColorBrush(ZoneRed, 0.9), 1);
    private static readonly Pen GreenSelectedPen = new(new SolidColorBrush(ZoneGreen), 1);
    private static readonly Pen AmberSelectedPen = new(new SolidColorBrush(ZoneAmber), 1);
    private static readonly Pen RedSelectedPen = new(new SolidColorBrush(ZoneRed), 1);

    public PositionMonitorControl()
    {
        ClipToBounds = true;
        Cursor = new Cursor(StandardCursorType.Arrow);
        PointerPressed += OnPointerPressedHandler;
        PointerMoved += OnPointerMovedHandler;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0 || ScreenWidth <= 0 || ScreenHeight <= 0)
        {
            return;
        }

        var frame = GetFrameRect();
        var scale = frame.Width / ScreenWidth;

        // Video frame
        context.DrawRectangle(FrameBrush, null, frame);

        // Letterbox bars
        if (BarHeight > 0)
        {
            var barPixels = BarHeight * scale;
            if (barPixels > 0.5 && barPixels * 2 < frame.Height)
            {
                var topBar = new Rect(frame.X, frame.Y, frame.Width, barPixels);
                var bottomBar = new Rect(frame.X, frame.Bottom - barPixels, frame.Width, barPixels);
                context.DrawRectangle(BarBrush, null, topBar);
                context.DrawRectangle(BarBrush, null, bottomBar);
                context.DrawLine(BarSeparatorPen, new Point(frame.X, topBar.Bottom), new Point(frame.Right, topBar.Bottom));
                context.DrawLine(BarSeparatorPen, new Point(frame.X, bottomBar.Top), new Point(frame.Right, bottomBar.Top));
            }
        }

        // Title-safe margin
        if (ShowTitleSafe && TitleSafePercent > 0 && TitleSafePercent < 25)
        {
            var marginX = frame.Width * TitleSafePercent / 100.0;
            var marginY = frame.Height * TitleSafePercent / 100.0;
            var safeRect = new Rect(frame.X + marginX, frame.Y + marginY, frame.Width - 2 * marginX, frame.Height - 2 * marginY);
            context.DrawRectangle(null, TitleSafePen, safeRect);
        }

        // Subtitle rectangles (selected drawn last, on top, with its real bitmap)
        var items = Items;
        if (items == null || items.Count == 0)
        {
            DrawCenteredHint(context, frame, Se.Language.Tools.ImageBasedEdit.NoImageSubtitlesLoaded);
            context.DrawRectangle(null, FrameBorderPen, frame);
            return;
        }

        foreach (var item in items)
        {
            if (!ReferenceEquals(item, SelectedItem))
            {
                DrawItem(context, frame, scale, item, false);
            }
        }

        if (SelectedItem != null)
        {
            DrawItem(context, frame, scale, SelectedItem, true);
        }

        context.DrawRectangle(null, FrameBorderPen, frame);
    }

    private void DrawItem(DrawingContext context, Rect frame, double scale, BinarySubtitleItem item, bool isSelected)
    {
        var rect = GetItemRect(item, frame, scale);
        if (rect == null)
        {
            return;
        }

        var zone = PositionMonitorLogic.Classify(item.Y, GetBitmapHeight(item), ScreenHeight, BarHeight);
        var (fill, border, selectedBorder) = zone switch
        {
            PositionMonitorZone.TopBar => (RedFill, RedPen, RedSelectedPen),
            PositionMonitorZone.BottomBar => (AmberFill, AmberPen, AmberSelectedPen),
            _ => (GreenFill, GreenPen, GreenSelectedPen),
        };

        if (isSelected && item.Bitmap != null)
        {
            var src = new Rect(0, 0, item.Bitmap.PixelSize.Width, item.Bitmap.PixelSize.Height);
            context.DrawImage(item.Bitmap, src, rect.Value);
            context.DrawRectangle(null, SelectedPen, rect.Value.Inflate(1));
            context.DrawRectangle(null, selectedBorder, rect.Value.Inflate(2.5));
        }
        else
        {
            context.DrawRectangle(fill, border, rect.Value);
        }
    }

    private static void DrawCenteredHint(DrawingContext context, Rect frame, string text)
    {
        var ft = new FormattedText(
            text,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            13,
            new SolidColorBrush(Color.FromArgb(0x90, 0xFF, 0xFF, 0xFF)));
        context.DrawText(ft, new Point(
            frame.X + (frame.Width - ft.Width) / 2.0,
            frame.Y + (frame.Height - ft.Height) / 2.0));
    }

    private Rect GetFrameRect()
    {
        var availableWidth = Math.Max(0, Bounds.Width - 2 * FramePadding);
        var availableHeight = Math.Max(0, Bounds.Height - 2 * FramePadding);
        if (availableWidth <= 0 || availableHeight <= 0)
        {
            return new Rect(FramePadding, FramePadding, 0, 0);
        }

        var scale = Math.Min(availableWidth / ScreenWidth, availableHeight / ScreenHeight);
        var width = ScreenWidth * scale;
        var height = ScreenHeight * scale;
        return new Rect(
            (Bounds.Width - width) / 2.0,
            (Bounds.Height - height) / 2.0,
            width,
            height);
    }

    private Rect? GetItemRect(BinarySubtitleItem item, Rect frame, double scale)
    {
        var w = GetBitmapWidth(item);
        var h = GetBitmapHeight(item);
        if (w <= 0 || h <= 0)
        {
            return null;
        }

        return new Rect(
            frame.X + item.X * scale,
            frame.Y + item.Y * scale,
            Math.Max(1.5, w * scale),
            Math.Max(1.5, h * scale));
    }

    private static int GetBitmapWidth(BinarySubtitleItem item)
    {
        return item.Bitmap?.PixelSize.Width ?? 0;
    }

    private static int GetBitmapHeight(BinarySubtitleItem item)
    {
        return item.Bitmap?.PixelSize.Height ?? 0;
    }

    private BinarySubtitleItem? HitTest(Point point)
    {
        var items = Items;
        if (items == null || items.Count == 0 || ScreenWidth <= 0 || ScreenHeight <= 0)
        {
            return null;
        }

        var frame = GetFrameRect();
        if (frame.Width <= 0)
        {
            return null;
        }

        var scale = frame.Width / ScreenWidth;

        // Prefer the smallest rectangle containing the point so overlapping
        // subtitles remain individually selectable.
        BinarySubtitleItem? best = null;
        var bestArea = double.MaxValue;
        foreach (var item in items)
        {
            var rect = GetItemRect(item, frame, scale);
            if (rect == null || !rect.Value.Inflate(1).Contains(point))
            {
                continue;
            }

            var area = rect.Value.Width * rect.Value.Height;
            if (area < bestArea)
            {
                bestArea = area;
                best = item;
            }
        }

        return best;
    }

    private void OnPointerPressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var hit = HitTest(e.GetPosition(this));
        if (hit != null)
        {
            ItemClicked?.Invoke(this, hit);
            e.Handled = true;
        }
    }

    private void OnPointerMovedHandler(object? sender, PointerEventArgs e)
    {
        var hit = HitTest(e.GetPosition(this));
        Cursor = hit != null
            ? new Cursor(StandardCursorType.Hand)
            : new Cursor(StandardCursorType.Arrow);
    }
}
