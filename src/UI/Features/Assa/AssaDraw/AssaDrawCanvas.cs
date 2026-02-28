using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// Custom canvas control for ASSA drawing with zoom, pan, and shape rendering.
/// </summary>
public class AssaDrawCanvas : Control
{
    private float _zoomFactor = 1.0f;
    private float _panX;
    private float _panY;
    private Point? _lastMousePosition;
    private bool _isPanning;

    public static readonly StyledProperty<List<DrawShape>> ShapesProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, List<DrawShape>>(nameof(Shapes), []);

    public static readonly StyledProperty<DrawShape?> ActiveShapeProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, DrawShape?>(nameof(ActiveShape));

    public static readonly StyledProperty<DrawShape?> SelectedShapeProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, DrawShape?>(nameof(SelectedShape));

    public static readonly StyledProperty<List<DrawShape>> SelectedShapesProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, List<DrawShape>>(nameof(SelectedShapes), []);

    public static readonly StyledProperty<DrawCoordinate?> ActivePointProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, DrawCoordinate?>(nameof(ActivePoint));

    public static readonly StyledProperty<int> CanvasWidthProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, int>(nameof(CanvasWidth), 1920);

    public static readonly StyledProperty<int> CanvasHeightProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, int>(nameof(CanvasHeight), 1080);

    public static readonly StyledProperty<float> CurrentXProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, float>(nameof(CurrentX), float.MinValue);

    public static readonly StyledProperty<float> CurrentYProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, float>(nameof(CurrentY), float.MinValue);

    public static readonly StyledProperty<DrawingTool> CurrentToolProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, DrawingTool>(nameof(CurrentTool), DrawingTool.Line);

    public List<DrawShape> Shapes
    {
        get => GetValue(ShapesProperty);
        set => SetValue(ShapesProperty, value);
    }

    public DrawShape? ActiveShape
    {
        get => GetValue(ActiveShapeProperty);
        set => SetValue(ActiveShapeProperty, value);
    }

    public DrawShape? SelectedShape
    {
        get => GetValue(SelectedShapeProperty);
        set => SetValue(SelectedShapeProperty, value);
    }

    public List<DrawShape> SelectedShapes
    {
        get => GetValue(SelectedShapesProperty);
        set => SetValue(SelectedShapesProperty, value);
    }

    public DrawCoordinate? ActivePoint
    {
        get => GetValue(ActivePointProperty);
        set => SetValue(ActivePointProperty, value);
    }

    public int CanvasWidth
    {
        get => GetValue(CanvasWidthProperty);
        set => SetValue(CanvasWidthProperty, value);
    }

    public int CanvasHeight
    {
        get => GetValue(CanvasHeightProperty);
        set => SetValue(CanvasHeightProperty, value);
    }

    public float CurrentX
    {
        get => GetValue(CurrentXProperty);
        set => SetValue(CurrentXProperty, value);
    }

    public float CurrentY
    {
        get => GetValue(CurrentYProperty);
        set => SetValue(CurrentYProperty, value);
    }

    public DrawingTool CurrentTool
    {
        get => GetValue(CurrentToolProperty);
        set => SetValue(CurrentToolProperty, value);
    }

    public float ZoomFactor
    {
        get => _zoomFactor;
        set
        {
            _zoomFactor = Math.Clamp(value, 0.1f, 10f);
            if (Math.Abs(_zoomFactor - 1.0f) < 0.05f)
            {
                _zoomFactor = 1.0f;
            }
            InvalidateVisual();
            ZoomChanged?.Invoke(this, _zoomFactor);
        }
    }

    public event EventHandler<CanvasClickEventArgs>? CanvasClicked;
    public event EventHandler<CanvasMouseEventArgs>? CanvasMouseMoved;
    public event EventHandler<DrawCoordinate>? PointDragged;
    public event EventHandler<float>? ZoomChanged;

    public AssaDrawCanvas()
    {
        ClipToBounds = true;
        Focusable = true;
    }

    public void ResetView()
    {
        _zoomFactor = 1.0f;
        _panX = 0;
        _panY = 0;
        InvalidateVisual();
        ZoomChanged?.Invoke(this, _zoomFactor);
    }

    public void ZoomIn() => ZoomFactor += 0.02f;
    public void ZoomOut() => ZoomFactor -= 0.02f;

    private float ToZoomFactorX(float v) => v * _zoomFactor + _panX;
    private float ToZoomFactorY(float v) => v * _zoomFactor + _panY;
    private float FromZoomFactorX(float v) => (v - _panX) / _zoomFactor;
    private float FromZoomFactorY(float v) => (v - _panY) / _zoomFactor;

    private Point ToZoomFactorPoint(DrawCoordinate coord) =>
        new(ToZoomFactorX(coord.X), ToZoomFactorY(coord.Y));

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;

        // Draw checkered background for transparency indication
        DrawCheckerBackground(context, bounds);

        // Draw the actual canvas area
        DrawCanvasArea(context);

        // Draw grid if enabled
        if (DrawSettings.ShowGrid)
        {
            DrawGrid(context);
        }

        // Draw resolution border
        DrawResolutionBorder(context);

        // Draw all shapes
        foreach (var shape in Shapes.Where(s => !s.Hidden))
        {
            var isSelected = SelectedShapes.Contains(shape);
            var isActive = shape == ActiveShape || shape == SelectedShape || isSelected;
            DrawShape(context, shape, isActive, isSelected);
            DrawShapePoints(context, shape);
        }

        // Draw active shape being created (not yet in Shapes list)
        if (ActiveShape != null && !Shapes.Contains(ActiveShape))
        {
            DrawShape(context, ActiveShape, true, false);
            DrawShapePoints(context, ActiveShape);

            // Draw preview to current mouse position
            if (CurrentX > float.MinValue && CurrentY > float.MinValue && ActiveShape.Points.Count > 0)
            {
                var lastPoint = ActiveShape.Points[^1];
                var pen = new Pen(new SolidColorBrush(DrawSettings.ActiveShapeLineColor), 2, lineCap: PenLineCap.Round);

                if (CurrentTool == DrawingTool.Circle && ActiveShape.Points.Count == 1)
                {
                    // Draw circle preview
                    var radius = Math.Max(Math.Abs(CurrentX - lastPoint.X), Math.Abs(CurrentY - lastPoint.Y));
                    var centerX = ToZoomFactorX(lastPoint.X);
                    var centerY = ToZoomFactorY(lastPoint.Y);
                    var rect = new Rect(
                        centerX - radius * _zoomFactor,
                        centerY - radius * _zoomFactor,
                        radius * 2 * _zoomFactor,
                        radius * 2 * _zoomFactor);
                    context.DrawEllipse(null, pen, rect);
                }
                else if (CurrentTool == DrawingTool.Rectangle && ActiveShape.Points.Count == 1)
                {
                    // Draw rectangle preview
                    var x1 = ToZoomFactorX(lastPoint.X);
                    var y1 = ToZoomFactorY(lastPoint.Y);
                    var x2 = ToZoomFactorX(CurrentX);
                    var y2 = ToZoomFactorY(CurrentY);
                    var rect = new Rect(
                        Math.Min(x1, x2),
                        Math.Min(y1, y2),
                        Math.Abs(x2 - x1),
                        Math.Abs(y2 - y1));
                    context.DrawRectangle(null, pen, rect);
                }
                else
                {
                    // Draw preview line for Line and Bezier tools
                    context.DrawLine(pen, ToZoomFactorPoint(lastPoint),
                        new Point(ToZoomFactorX(CurrentX), ToZoomFactorY(CurrentY)));
                }
            }
        }

        // Highlight active point
        if (ActivePoint != null)
        {
            var pointColor = new Color(255, ActivePoint.PointColor.R, ActivePoint.PointColor.G, ActivePoint.PointColor.B);
            var pen = new Pen(new SolidColorBrush(pointColor), 3);
            var x = ToZoomFactorX(ActivePoint.X);
            var y = ToZoomFactorY(ActivePoint.Y);
            context.DrawLine(pen, new Point(x - 8, y), new Point(x + 8, y));
            context.DrawLine(pen, new Point(x, y - 8), new Point(x, y + 8));
        }
    }

    private void DrawCheckerBackground(DrawingContext context, Rect bounds)
    {
        const int size = 10;
        var color1 = Color.FromRgb(60, 60, 60);
        var color2 = Color.FromRgb(80, 80, 80);

        for (var y = 0; y < bounds.Height; y += size)
        {
            for (var x = 0; x < bounds.Width; x += size)
            {
                var isEven = ((x / size) + (y / size)) % 2 == 0;
                var brush = new SolidColorBrush(isEven ? color1 : color2);
                context.FillRectangle(brush, new Rect(x, y, size, size));
            }
        }
    }

    private void DrawCanvasArea(DrawingContext context)
    {
        var brush = new SolidColorBrush(DrawSettings.BackgroundColor);
        var rect = new Rect(_panX, _panY, CanvasWidth * _zoomFactor, CanvasHeight * _zoomFactor);
        context.FillRectangle(brush, rect);
    }

    private void DrawGrid(DrawingContext context)
    {
        var pen = new Pen(new SolidColorBrush(DrawSettings.GridColor), 0.5);
        var gridSize = DrawSettings.GridSize * _zoomFactor;

        for (float x = _panX; x < _panX + CanvasWidth * _zoomFactor; x += gridSize)
        {
            context.DrawLine(pen, new Point(x, _panY), new Point(x, _panY + CanvasHeight * _zoomFactor));
        }

        for (float y = _panY; y < _panY + CanvasHeight * _zoomFactor; y += gridSize)
        {
            context.DrawLine(pen, new Point(_panX, y), new Point(_panX + CanvasWidth * _zoomFactor, y));
        }
    }

    private void DrawResolutionBorder(DrawingContext context)
    {
        var pen = new Pen(new SolidColorBrush(DrawSettings.ScreenSizeColor), 2);
        var rect = new Rect(_panX - 1, _panY - 1, CanvasWidth * _zoomFactor + 2, CanvasHeight * _zoomFactor + 2);
        context.DrawRectangle(null, pen, rect);
    }

    private void DrawShape(DrawingContext context, DrawShape shape, bool isActive, bool isSelected)
    {
        if (shape.Points.Count == 0)
        {
            return;
        }

        // Use different colors for eraser shapes to visually distinguish them
        var color = isSelected ? Colors.Yellow : (isActive ? DrawSettings.ActiveShapeLineColor : DrawSettings.ShapeLineColor);
        if (shape.IsEraser && !isSelected)
        {
            // Draw eraser shapes in red/orange to indicate they are iclip masks
            color = isActive ? Colors.OrangeRed : Colors.DarkOrange;
        }

        // Use dashed lines for eraser shapes
        var pen = new Pen(new SolidColorBrush(color), 2, lineCap: PenLineCap.Round);
        if (shape.IsEraser)
        {
            pen.DashStyle = DashStyle.Dash;
        }

        var i = 0;
        while (i < shape.Points.Count)
        {
            var point = shape.Points[i];

            if (point.DrawType == DrawCoordinateType.Line)
            {
                if (i > 0)
                {
                    var prev = shape.Points[i - 1];
                    context.DrawLine(pen, ToZoomFactorPoint(prev), ToZoomFactorPoint(point));
                }

                i++;
            }
            else if (point.IsBeizer)
            {
                if (i > 0 && shape.Points.Count - i >= 3)
                {
                    // Draw bezier curve
                    var startPoint = shape.Points[i - 1];
                    var control1 = shape.Points[i];
                    var control2 = shape.Points[i + 1];
                    var endPoint = shape.Points[i + 2];

                    var geometry = new StreamGeometry();
                    using (var ctx = geometry.Open())
                    {
                        ctx.BeginFigure(ToZoomFactorPoint(startPoint), false);
                        ctx.CubicBezierTo(
                            ToZoomFactorPoint(control1),
                            ToZoomFactorPoint(control2),
                            ToZoomFactorPoint(endPoint));
                    }

                    context.DrawGeometry(null, pen, geometry);

                    // Draw control point lines (guides)
                    if (isActive)
                    {
                        var guidePen = new Pen(new SolidColorBrush(Colors.Gray), 1, lineJoin: PenLineJoin.Round)
                        {
                            DashStyle = DashStyle.Dash
                        };
                        context.DrawLine(guidePen, ToZoomFactorPoint(startPoint), ToZoomFactorPoint(control1));
                        context.DrawLine(guidePen, ToZoomFactorPoint(endPoint), ToZoomFactorPoint(control2));
                    }

                    i += 3;
                }
                else
                {
                    i++;
                }
            }
            else
            {
                i++;
            }
        }

        // Close the shape by drawing line from last to first point
        if (shape.Points.Count > 2 && Shapes.Contains(shape))
        {
            var first = shape.Points[0];
            var last = shape.Points[^1];
            context.DrawLine(pen, ToZoomFactorPoint(last), ToZoomFactorPoint(first));
        }
    }

    private void DrawShapePoints(DrawingContext context, DrawShape shape)
    {
        foreach (var point in shape.Points)
        {
            var pen = new Pen(new SolidColorBrush(point.PointColor), 2);
            var x = ToZoomFactorX(point.X);
            var y = ToZoomFactorY(point.Y);

            // Draw cross marker
            context.DrawLine(pen, new Point(x - 5, y), new Point(x + 5, y));
            context.DrawLine(pen, new Point(x, y - 5), new Point(x, y + 5));
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();

        var point = e.GetPosition(this);
        var x = FromZoomFactorX((float)point.X);
        var y = FromZoomFactorY((float)point.Y);

        // Check for shift+click to pan
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            _isPanning = true;
            _lastMousePosition = point;
            e.Handled = true;
            return;
        }

        // Check if clicking near an existing point
        var closePoint = GetClosePoint(x, y);
        if (closePoint != null)
        {
            ActivePoint = closePoint;
            _lastMousePosition = point;
            InvalidateVisual();
            e.Handled = true;
            return;
        }

        // Regular click - trigger canvas click event
        CanvasClicked?.Invoke(this, new CanvasClickEventArgs(x, y, e.GetCurrentPoint(this).Properties.IsLeftButtonPressed));
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var point = e.GetPosition(this);
        var x = FromZoomFactorX((float)point.X);
        var y = FromZoomFactorY((float)point.Y);

        if (_isPanning && _lastMousePosition.HasValue)
        {
            _panX += (float)(point.X - _lastMousePosition.Value.X);
            _panY += (float)(point.Y - _lastMousePosition.Value.Y);
            _lastMousePosition = point;
            InvalidateVisual();
            return;
        }

        // Dragging a point
        if (ActivePoint != null && _lastMousePosition.HasValue && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            ActivePoint.X = x;
            ActivePoint.Y = y;
            PointDragged?.Invoke(this, ActivePoint);
            InvalidateVisual();
            return;
        }

        CanvasMouseMoved?.Invoke(this, new CanvasMouseEventArgs(x, y));
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isPanning = false;
        _lastMousePosition = null;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            // Zoom with Ctrl+Scroll
            var delta = e.Delta.Y > 0 ? 0.1f : -0.1f;
            ZoomFactor += delta;
            e.Handled = true;
        }
    }

    private DrawCoordinate? GetClosePoint(float x, float y)
    {
        const float maxDistance = 10f;
        DrawCoordinate? closest = null;
        var minDist = float.MaxValue;

        foreach (var shape in Shapes.Where(s => !s.Hidden))
        {
            foreach (var point in shape.Points)
            {
                var dist = Math.Abs(x - point.X) + Math.Abs(y - point.Y);
                if (dist < minDist && dist < maxDistance / _zoomFactor)
                {
                    minDist = dist;
                    closest = point;
                }
            }
        }

        return closest;
    }
}

public class CanvasClickEventArgs : EventArgs
{
    public float X { get; }
    public float Y { get; }
    public bool IsLeftButton { get; }

    public CanvasClickEventArgs(float x, float y, bool isLeftButton)
    {
        X = x;
        Y = y;
        IsLeftButton = isLeftButton;
    }
}

public class CanvasMouseEventArgs : EventArgs
{
    public float X { get; }
    public float Y { get; }

    public CanvasMouseEventArgs(float x, float y)
    {
        X = x;
        Y = y;
    }
}
