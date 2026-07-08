using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

/// <summary>
/// Overlay control for selecting the scan area on top of a video frame preview.
/// The selection is kept in video pixel coordinates; the control maps to/from the
/// letterboxed display rectangle of the underlying Image (Stretch=Uniform).
/// The area outside the selection is dimmed; the selection can be moved, resized
/// via edges/corners, or redrawn by dragging on the outside.
/// </summary>
public class CropAreaSelector : Control
{
    public static readonly StyledProperty<int> VideoWidthProperty =
        AvaloniaProperty.Register<CropAreaSelector, int>(nameof(VideoWidth), 1920);

    public static readonly StyledProperty<int> VideoHeightProperty =
        AvaloniaProperty.Register<CropAreaSelector, int>(nameof(VideoHeight), 1080);

    public static readonly StyledProperty<int> SelectionXProperty =
        AvaloniaProperty.Register<CropAreaSelector, int>(nameof(SelectionX));

    public static readonly StyledProperty<int> SelectionYProperty =
        AvaloniaProperty.Register<CropAreaSelector, int>(nameof(SelectionY), 720);

    public static readonly StyledProperty<int> SelectionWidthProperty =
        AvaloniaProperty.Register<CropAreaSelector, int>(nameof(SelectionWidth), 1920);

    public static readonly StyledProperty<int> SelectionHeightProperty =
        AvaloniaProperty.Register<CropAreaSelector, int>(nameof(SelectionHeight), 360);

    private const double HandleHitSize = 8;
    private const int MinSelectionSize = 16;

    private enum DragMode
    {
        None,
        Draw,
        Move,
        ResizeLeft,
        ResizeRight,
        ResizeTop,
        ResizeBottom,
        ResizeTopLeft,
        ResizeTopRight,
        ResizeBottomLeft,
        ResizeBottomRight,
    }

    private DragMode _dragMode = DragMode.None;
    private Point _dragStartDisplay;
    private Rect _dragStartSelectionVideo;

    private static readonly IImmutableSolidColorBrush DimBrush = new ImmutableSolidColorBrush(Colors.Black, 0.55);
    private static readonly IImmutableSolidColorBrush AccentBrush = new ImmutableSolidColorBrush(Color.FromRgb(0x2e, 0x9b, 0xe0));
    private static readonly ImmutablePen BorderPen = new(AccentBrush, 2);

    public int VideoWidth
    {
        get => GetValue(VideoWidthProperty);
        set => SetValue(VideoWidthProperty, value);
    }

    public int VideoHeight
    {
        get => GetValue(VideoHeightProperty);
        set => SetValue(VideoHeightProperty, value);
    }

    public int SelectionX
    {
        get => GetValue(SelectionXProperty);
        set => SetValue(SelectionXProperty, value);
    }

    public int SelectionY
    {
        get => GetValue(SelectionYProperty);
        set => SetValue(SelectionYProperty, value);
    }

    public int SelectionWidth
    {
        get => GetValue(SelectionWidthProperty);
        set => SetValue(SelectionWidthProperty, value);
    }

    public int SelectionHeight
    {
        get => GetValue(SelectionHeightProperty);
        set => SetValue(SelectionHeightProperty, value);
    }

    static CropAreaSelector()
    {
        AffectsRender<CropAreaSelector>(
            VideoWidthProperty,
            VideoHeightProperty,
            SelectionXProperty,
            SelectionYProperty,
            SelectionWidthProperty,
            SelectionHeightProperty);
    }

    public CropAreaSelector()
    {
        ClipToBounds = true;
    }

    /// <summary>The area (in control coordinates) where the video frame is drawn (uniform fit, centered).</summary>
    private Rect GetDisplayRect()
    {
        if (VideoWidth <= 0 || VideoHeight <= 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return new Rect();
        }

        var scale = Math.Min(Bounds.Width / VideoWidth, Bounds.Height / VideoHeight);
        var width = VideoWidth * scale;
        var height = VideoHeight * scale;
        return new Rect((Bounds.Width - width) / 2.0, (Bounds.Height - height) / 2.0, width, height);
    }

    private Rect VideoToDisplay(Rect videoRect)
    {
        var display = GetDisplayRect();
        if (display.Width <= 0)
        {
            return new Rect();
        }

        var scale = display.Width / VideoWidth;
        return new Rect(
            display.X + videoRect.X * scale,
            display.Y + videoRect.Y * scale,
            videoRect.Width * scale,
            videoRect.Height * scale);
    }

    private Point DisplayToVideo(Point displayPoint)
    {
        var display = GetDisplayRect();
        if (display.Width <= 0)
        {
            return new Point();
        }

        var scale = VideoWidth / display.Width;
        return new Point((displayPoint.X - display.X) * scale, (displayPoint.Y - display.Y) * scale);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var display = GetDisplayRect();
        if (display.Width <= 0 || display.Height <= 0)
        {
            return;
        }

        var selection = VideoToDisplay(new Rect(SelectionX, SelectionY, SelectionWidth, SelectionHeight));

        // Dim everything outside the selection.
        context.FillRectangle(DimBrush, new Rect(display.X, display.Y, display.Width, Math.Max(0, selection.Y - display.Y)));
        context.FillRectangle(DimBrush, new Rect(display.X, selection.Bottom, display.Width, Math.Max(0, display.Bottom - selection.Bottom)));
        context.FillRectangle(DimBrush, new Rect(display.X, selection.Y, Math.Max(0, selection.X - display.X), selection.Height));
        context.FillRectangle(DimBrush, new Rect(selection.Right, selection.Y, Math.Max(0, display.Right - selection.Right), selection.Height));

        // Selection border + corner handles.
        context.DrawRectangle(BorderPen, selection);

        foreach (var p in new[]
                 {
                     selection.TopLeft, selection.TopRight, selection.BottomLeft, selection.BottomRight,
                     new Point(selection.Center.X, selection.Y), new Point(selection.Center.X, selection.Bottom),
                     new Point(selection.X, selection.Center.Y), new Point(selection.Right, selection.Center.Y),
                 })
        {
            context.FillRectangle(AccentBrush, new Rect(p.X - 3.5, p.Y - 3.5, 7, 7));
        }
    }

    private DragMode HitTest(Point point)
    {
        var selection = VideoToDisplay(new Rect(SelectionX, SelectionY, SelectionWidth, SelectionHeight));

        bool Near(double a, double b) => Math.Abs(a - b) <= HandleHitSize;
        var nearLeft = Near(point.X, selection.X);
        var nearRight = Near(point.X, selection.Right);
        var nearTop = Near(point.Y, selection.Y);
        var nearBottom = Near(point.Y, selection.Bottom);
        var insideX = point.X > selection.X - HandleHitSize && point.X < selection.Right + HandleHitSize;
        var insideY = point.Y > selection.Y - HandleHitSize && point.Y < selection.Bottom + HandleHitSize;

        if (nearTop && nearLeft)
        {
            return DragMode.ResizeTopLeft;
        }

        if (nearTop && nearRight)
        {
            return DragMode.ResizeTopRight;
        }

        if (nearBottom && nearLeft)
        {
            return DragMode.ResizeBottomLeft;
        }

        if (nearBottom && nearRight)
        {
            return DragMode.ResizeBottomRight;
        }

        if (nearLeft && insideY)
        {
            return DragMode.ResizeLeft;
        }

        if (nearRight && insideY)
        {
            return DragMode.ResizeRight;
        }

        if (nearTop && insideX)
        {
            return DragMode.ResizeTop;
        }

        if (nearBottom && insideX)
        {
            return DragMode.ResizeBottom;
        }

        if (point.X > selection.X && point.X < selection.Right && point.Y > selection.Y && point.Y < selection.Bottom)
        {
            return DragMode.Move;
        }

        return DragMode.Draw;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var point = e.GetPosition(this);
        var display = GetDisplayRect();
        if (display.Width <= 0 || !display.Inflate(HandleHitSize).Contains(point))
        {
            return;
        }

        _dragMode = HitTest(point);
        _dragStartDisplay = point;
        _dragStartSelectionVideo = new Rect(SelectionX, SelectionY, SelectionWidth, SelectionHeight);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var point = e.GetPosition(this);

        if (_dragMode == DragMode.None)
        {
            Cursor = GetCursorFor(HitTest(point));
            return;
        }

        var startVideo = DisplayToVideo(_dragStartDisplay);
        var currentVideo = DisplayToVideo(point);
        var deltaX = currentVideo.X - startVideo.X;
        var deltaY = currentVideo.Y - startVideo.Y;
        var start = _dragStartSelectionVideo;

        double x = start.X, y = start.Y, width = start.Width, height = start.Height;

        switch (_dragMode)
        {
            case DragMode.Draw:
                x = Math.Min(startVideo.X, currentVideo.X);
                y = Math.Min(startVideo.Y, currentVideo.Y);
                width = Math.Abs(currentVideo.X - startVideo.X);
                height = Math.Abs(currentVideo.Y - startVideo.Y);
                break;
            case DragMode.Move:
                x = start.X + deltaX;
                y = start.Y + deltaY;
                break;
            case DragMode.ResizeLeft:
                x = start.X + deltaX;
                width = start.Width - deltaX;
                break;
            case DragMode.ResizeRight:
                width = start.Width + deltaX;
                break;
            case DragMode.ResizeTop:
                y = start.Y + deltaY;
                height = start.Height - deltaY;
                break;
            case DragMode.ResizeBottom:
                height = start.Height + deltaY;
                break;
            case DragMode.ResizeTopLeft:
                x = start.X + deltaX;
                width = start.Width - deltaX;
                y = start.Y + deltaY;
                height = start.Height - deltaY;
                break;
            case DragMode.ResizeTopRight:
                width = start.Width + deltaX;
                y = start.Y + deltaY;
                height = start.Height - deltaY;
                break;
            case DragMode.ResizeBottomLeft:
                x = start.X + deltaX;
                width = start.Width - deltaX;
                height = start.Height + deltaY;
                break;
            case DragMode.ResizeBottomRight:
                width = start.Width + deltaX;
                height = start.Height + deltaY;
                break;
        }

        SetSelection(x, y, width, height);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_dragMode != DragMode.None)
        {
            _dragMode = DragMode.None;
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    private Cursor GetCursorFor(DragMode mode)
    {
        return mode switch
        {
            DragMode.Move => new Cursor(StandardCursorType.SizeAll),
            DragMode.ResizeLeft or DragMode.ResizeRight => new Cursor(StandardCursorType.SizeWestEast),
            DragMode.ResizeTop or DragMode.ResizeBottom => new Cursor(StandardCursorType.SizeNorthSouth),
            DragMode.ResizeTopLeft or DragMode.ResizeBottomRight => new Cursor(StandardCursorType.TopLeftCorner),
            DragMode.ResizeTopRight or DragMode.ResizeBottomLeft => new Cursor(StandardCursorType.TopRightCorner),
            _ => Cursor.Default,
        };
    }

    private void SetSelection(double x, double y, double width, double height)
    {
        if (width < 0)
        {
            x += width;
            width = -width;
        }

        if (height < 0)
        {
            y += height;
            height = -height;
        }

        width = Math.Clamp(width, MinSelectionSize, VideoWidth);
        height = Math.Clamp(height, MinSelectionSize, VideoHeight);
        x = Math.Clamp(x, 0, VideoWidth - width);
        y = Math.Clamp(y, 0, VideoHeight - height);

        SelectionX = (int)Math.Round(x);
        SelectionY = (int)Math.Round(y);
        SelectionWidth = (int)Math.Round(width);
        SelectionHeight = (int)Math.Round(height);
    }

    /// <summary>Sets the selection from video coordinates (clamped), e.g. for presets.</summary>
    public void SetSelectionVideoRect(int x, int y, int width, int height)
    {
        SetSelection(x, y, width, height);
        InvalidateVisual();
    }
}
