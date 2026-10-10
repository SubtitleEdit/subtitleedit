using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;

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
    private List<DrawShape>? _dragShapes;
    private DrawShape? _pressedShape;
    private bool _dragShapeMoved;
    private bool _pointDragStarted;

    // Scale/rotate handles: 0-7 scale (TL, T, TR, R, BR, B, BL, L), 8 rotate
    private const int RotateHandle = 8;
    private const double HandlePadding = 8;
    private const double RotateHandleDistance = 26;
    private int _transformHandle = -1;
    private List<DrawShape>? _transformShapes;
    private List<(float X, float Y)[]>? _transformOriginal;
    private (float Left, float Top, float Right, float Bottom) _transformBounds;
    private Point _transformStart;
    private bool _transformStarted;
    private string? _transformLabel;

    private static readonly Cursor[] HandleCursors =
    [
        new(StandardCursorType.TopLeftCorner), new(StandardCursorType.TopSide), new(StandardCursorType.TopRightCorner),
        new(StandardCursorType.RightSide), new(StandardCursorType.BottomRightCorner), new(StandardCursorType.BottomSide),
        new(StandardCursorType.BottomLeftCorner), new(StandardCursorType.LeftSide), new(StandardCursorType.Hand),
    ];
    private static readonly Cursor MoveCursor = new(StandardCursorType.SizeAll);

    private static readonly ImmutableSolidColorBrush WorkspaceBrush = new(Color.FromRgb(22, 23, 27));
    private static readonly ImmutableSolidColorBrush CheckerBrush = new(Color.FromArgb(10, 255, 255, 255));
    private static readonly Color AccentColor = Color.FromRgb(76, 110, 245);
    private static readonly Color HandleFillColor = Colors.White;
    private static readonly Color GuideColor = Color.FromArgb(150, 160, 170, 190);

    private readonly Dictionary<Color, IBrush> _brushCache = new();
    private readonly Dictionary<(Color Color, double Thickness, PenLineCap LineCap, PenLineJoin LineJoin, bool Dashed), IPen> _penCache = new();

    private static readonly ImmutableDashStyle DashedStyle = new ImmutableDashStyle(DashStyle.Dash.Dashes, DashStyle.Dash.Offset);

    private IBrush GetBrush(Color color)
    {
        if (!_brushCache.TryGetValue(color, out var brush))
        {
            brush = new ImmutableSolidColorBrush(color);
            _brushCache[color] = brush;
        }

        return brush;
    }

    private IPen GetPen(Color color, double thickness, PenLineCap lineCap = PenLineCap.Flat, PenLineJoin lineJoin = PenLineJoin.Miter, bool dashed = false)
    {
        var key = (color, thickness, lineCap, lineJoin, dashed);
        if (!_penCache.TryGetValue(key, out var pen))
        {
            pen = new ImmutablePen((ImmutableSolidColorBrush)GetBrush(color), thickness, dashed ? DashedStyle : null, lineCap, lineJoin);
            _penCache[key] = pen;
        }

        return pen;
    }

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

    public static readonly StyledProperty<bool> ShowPreviewProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, bool>(nameof(ShowPreview));

    /// <summary>
    /// The drawing as libass renders it, at canvas (PlayRes) size with a transparent background.
    /// </summary>
    public static readonly StyledProperty<IImage?> PreviewImageProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, IImage?>(nameof(PreviewImage));

    /// <summary>
    /// Video frame shown behind the preview.
    /// </summary>
    public static readonly StyledProperty<IImage?> BackgroundImageProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, IImage?>(nameof(BackgroundImage));

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

    public bool ShowPreview
    {
        get => GetValue(ShowPreviewProperty);
        set => SetValue(ShowPreviewProperty, value);
    }

    public IImage? PreviewImage
    {
        get => GetValue(PreviewImageProperty);
        set => SetValue(PreviewImageProperty, value);
    }

    public IImage? BackgroundImage
    {
        get => GetValue(BackgroundImageProperty);
        set => SetValue(BackgroundImageProperty, value);
    }

    public static readonly StyledProperty<double> BackgroundOpacityProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, double>(nameof(BackgroundOpacity), 1.0);

    public static readonly StyledProperty<bool> BackgroundStretchProperty =
        AvaloniaProperty.Register<AssaDrawCanvas, bool>(nameof(BackgroundStretch));

    public double BackgroundOpacity
    {
        get => GetValue(BackgroundOpacityProperty);
        set => SetValue(BackgroundOpacityProperty, value);
    }

    /// <summary>
    /// Stretch the background to the canvas; otherwise it keeps its aspect ratio, centered.
    /// </summary>
    public bool BackgroundStretch
    {
        get => GetValue(BackgroundStretchProperty);
        set => SetValue(BackgroundStretchProperty, value);
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
            InvalidateVisual();
            ZoomChanged?.Invoke(this, _zoomFactor);
        }
    }

    public event EventHandler<CanvasClickEventArgs>? CanvasClicked;
    public event EventHandler<CanvasMouseEventArgs>? CanvasMouseMoved;
    public event EventHandler<DrawCoordinate>? PointSelected;
    public event EventHandler<DrawCoordinate>? PointDragged;
    public event EventHandler<float>? ZoomChanged;

    /// <summary>
    /// Select tool click: the shape under the pointer, or null for empty canvas.
    /// </summary>
    public event EventHandler<DrawShape?>? ShapeClicked;

    /// <summary>
    /// A shape was dragged to a new position with the select tool.
    /// </summary>
    public event EventHandler<DrawShape>? ShapeMoved;

    /// <summary>
    /// Right-click: what is under the pointer, so a context menu can be shown for it.
    /// </summary>
    public event EventHandler<CanvasContextEventArgs>? ContextMenuRequested;

    /// <summary>
    /// A point or shape drag is about to change the drawing (raised once per drag, for undo).
    /// </summary>
    public event EventHandler? EditStarting;

    static AssaDrawCanvas()
    {
        AffectsRender<AssaDrawCanvas>(
            ShapesProperty,
            ActiveShapeProperty,
            SelectedShapeProperty,
            SelectedShapesProperty,
            ActivePointProperty,
            CanvasWidthProperty,
            CanvasHeightProperty,
            ShowPreviewProperty,
            PreviewImageProperty,
            BackgroundImageProperty,
            BackgroundOpacityProperty,
            BackgroundStretchProperty);
    }

    public AssaDrawCanvas()
    {
        ClipToBounds = true;
        Focusable = true;
    }

    /// <summary>
    /// Zooms so the whole frame fits the visible area and centers it.
    /// </summary>
    public void FitToView(double padding = 20)
    {
        _isPanning = false;
        var availableWidth = Bounds.Width - padding * 2;
        var availableHeight = Bounds.Height - padding * 2;
        if (availableWidth < 1 || availableHeight < 1 || CanvasWidth < 1 || CanvasHeight < 1)
        {
            return;
        }

        var zoom = Math.Clamp((float)Math.Min(availableWidth / CanvasWidth, availableHeight / CanvasHeight), 0.1f, 10f);
        _panX = (float)(Bounds.Width - CanvasWidth * zoom) / 2f;
        _panY = (float)(Bounds.Height - CanvasHeight * zoom) / 2f;
        ZoomFactor = zoom;
    }

    // Same step as Ctrl+mouse wheel - the buttons used 2%, so a click barely changed anything.
    public void ZoomIn() => ZoomAt(_zoomFactor * 1.2f, Bounds.Center);
    public void ZoomOut() => ZoomAt(_zoomFactor / 1.2f, Bounds.Center);

    /// <summary>
    /// Zooms keeping the canvas point under <paramref name="anchor"/> (control coordinates) in place.
    /// </summary>
    public void ZoomAt(float zoom, Point anchor)
    {
        var x = FromZoomFactorX((float)anchor.X);
        var y = FromZoomFactorY((float)anchor.Y);
        var newZoom = Math.Clamp(zoom, 0.1f, 10f);
        _panX = (float)anchor.X - x * newZoom;
        _panY = (float)anchor.Y - y * newZoom;
        ZoomFactor = newZoom;
    }

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
        context.FillRectangle(WorkspaceBrush, new Rect(bounds.Size));

        var canvasRect = new Rect(_panX, _panY, CanvasWidth * _zoomFactor, CanvasHeight * _zoomFactor);
        DrawFrameShadow(context, canvasRect);

        // The frame itself: checkered so it reads as transparent, with the video frame or image on top
        DrawCanvasArea(context);
        DrawCheckerBackground(context, canvasRect);
        if (BackgroundImage is { } background && background.Size.Width > 0 && background.Size.Height > 0)
        {
            var target = canvasRect;
            if (!BackgroundStretch)
            {
                var scale = Math.Min(canvasRect.Width / background.Size.Width, canvasRect.Height / background.Size.Height);
                var size = new Size(background.Size.Width * scale, background.Size.Height * scale);
                target = new Rect(canvasRect.Center.X - size.Width / 2, canvasRect.Center.Y - size.Height / 2, size.Width, size.Height);
            }

            using (context.PushOpacity(Math.Clamp(BackgroundOpacity, 0.05, 1)))
            {
                context.DrawImage(background, target);
            }
        }

        // Draw grid if enabled
        if (DrawSettings.ShowGrid)
        {
            DrawGrid(context);
        }

        // Rendered result under the outlines, so points stay editable while previewing
        if (ShowPreview && PreviewImage != null)
        {
            context.DrawImage(PreviewImage, canvasRect);
        }

        // Draw resolution border
        DrawResolutionBorder(context);

        // Shapes are filled with their layer color, unless the libass render already shows them
        var fillShapes = !ShowPreview || PreviewImage == null;

        // Draw all shapes
        var shapes = Shapes;
        var selectedShapes = SelectedShapes;
        var selectedSet = selectedShapes.Count > 4 ? new HashSet<DrawShape>(selectedShapes) : null;
        var activeShape = ActiveShape;
        var selectedShape = SelectedShape;
        var activeShapeInList = activeShape != null && shapes.Contains(activeShape);
        if (fillShapes)
        {
            DrawLayerFills(context, shapes);
        }

        for (var idx = 0; idx < shapes.Count; idx++)
        {
            var shape = shapes[idx];
            if (shape.Hidden)
            {
                continue;
            }

            var isSelected = selectedSet != null ? selectedSet.Contains(shape) : selectedShapes.Contains(shape);
            var isActive = shape == activeShape || shape == selectedShape || isSelected;
            DrawShape(context, shape, isActive, isSelected, isInShapes: true, fill: false);
        }

        // Handles on top of every shape, so a later shape can't cover the selected one's points.
        // A multi-selection (Ctrl+A, SVG import) gets one box around all of it and no point handles.
        var isMultiSelection = selectedShapes.Count > 1;
        for (var idx = 0; idx < shapes.Count; idx++)
        {
            var shape = shapes[idx];
            if (shape.Hidden)
            {
                continue;
            }

            var isActive = shape == activeShape || shape == selectedShape ||
                           (!isMultiSelection && (selectedSet != null ? selectedSet.Contains(shape) : selectedShapes.Contains(shape)));
            if (isActive)
            {
                DrawSelectionBox(context, shape.GetBounds(), withHandles: CurrentTool == DrawingTool.Select && !isMultiSelection);
            }

            DrawShapePoints(context, shape, isActive);
        }

        if (isMultiSelection)
        {
            var visible = selectedShapes.Where(s => !s.Hidden && s.Points.Count > 0).ToList();
            if (visible.Count > 0)
            {
                var selectionBounds = visible[0].GetBounds();
                foreach (var shape in visible.Skip(1))
                {
                    var b = shape.GetBounds();
                    selectionBounds = (Math.Min(selectionBounds.Left, b.Left), Math.Min(selectionBounds.Top, b.Top),
                        Math.Max(selectionBounds.Right, b.Right), Math.Max(selectionBounds.Bottom, b.Bottom));
                }

                DrawSelectionBox(context, selectionBounds, withHandles: CurrentTool == DrawingTool.Select);
            }
        }

        // Draw active shape being created (not yet in Shapes list)
        if (ActiveShape != null && !activeShapeInList)
        {
            DrawShape(context, ActiveShape, true, false, isInShapes: false, fill: false);
            DrawShapePoints(context, ActiveShape, true);

            // Draw preview to current mouse position
            if (CurrentX > float.MinValue && CurrentY > float.MinValue && ActiveShape.Points.Count > 0)
            {
                var lastPoint = ActiveShape.Points[^1];
                var pen = GetPen(AccentColor, 1.5, lineCap: PenLineCap.Round, dashed: true);

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
            var center = ToZoomFactorPoint(ActivePoint);
            context.DrawEllipse(GetBrush(AccentColor), GetPen(HandleFillColor, 2), center, 6, 6);
            context.DrawEllipse(null, GetPen(Color.FromArgb(110, AccentColor.R, AccentColor.G, AccentColor.B), 2), center, 10, 10);
        }
    }

    private static void DrawFrameShadow(DrawingContext context, Rect canvasRect)
    {
        for (var i = 1; i <= 6; i++)
        {
            var shadow = new ImmutableSolidColorBrush(Color.FromArgb((byte)(28 - i * 4), 0, 0, 0));
            context.FillRectangle(shadow, canvasRect.Inflate(i * 2).Translate(new Vector(0, i)));
        }
    }

    private void DrawCheckerBackground(DrawingContext context, Rect canvasRect)
    {
        const double size = 16;
        var visible = canvasRect.Intersect(new Rect(Bounds.Size));
        if (visible.Width <= 0 || visible.Height <= 0)
        {
            return;
        }

        using (context.PushClip(visible))
        {
            var startColumn = (int)Math.Floor((visible.Left - canvasRect.Left) / size);
            var startRow = (int)Math.Floor((visible.Top - canvasRect.Top) / size);
            for (var row = startRow; canvasRect.Top + row * size < visible.Bottom; row++)
            {
                for (var column = startColumn; canvasRect.Left + column * size < visible.Right; column++)
                {
                    if ((row + column) % 2 == 0)
                    {
                        context.FillRectangle(CheckerBrush, new Rect(canvasRect.Left + column * size, canvasRect.Top + row * size, size, size));
                    }
                }
            }
        }
    }

    private void DrawCanvasArea(DrawingContext context)
    {
        var brush = GetBrush(DrawSettings.BackgroundColor);
        var rect = new Rect(_panX, _panY, CanvasWidth * _zoomFactor, CanvasHeight * _zoomFactor);
        context.FillRectangle(brush, rect);
    }

    private void DrawGrid(DrawingContext context)
    {
        var pen = GetPen(DrawSettings.GridColor, 1);
        var gridSize = DrawSettings.GridSize * _zoomFactor;
        while (gridSize < 8)
        {
            // Zoomed far out the lines would merge into a solid sheet - draw every other one
            gridSize *= 2;
        }

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
        var pen = GetPen(DrawSettings.ScreenSizeColor, 1);
        var rect = new Rect(_panX - 0.5, _panY - 0.5, CanvasWidth * _zoomFactor + 1, CanvasHeight * _zoomFactor + 1);
        context.DrawRectangle(null, pen, rect);
    }

    private void DrawShape(DrawingContext context, DrawShape shape, bool isActive, bool isSelected, bool isInShapes, bool fill)
    {
        if (shape.Points.Count == 0)
        {
            return;
        }

        var isClosed = shape.Points.Count > 2 && isInShapes;
        var geometry = BuildGeometry(shape, isClosed);

        IBrush? fillBrush = null;
        if (fill && isClosed && !shape.IsEraser)
        {
            fillBrush = GetBrush(Color.FromArgb(215, shape.ForeColor.R, shape.ForeColor.G, shape.ForeColor.B));
        }

        // Eraser shapes (iclip masks) are dashed and orange so they read as "cut out", not drawn
        Color color;
        if (shape.IsEraser && !isSelected)
        {
            color = isActive ? Colors.OrangeRed : Colors.DarkOrange;
        }
        else
        {
            color = isActive || isSelected ? AccentColor : DrawSettings.ShapeLineColor;
        }

        var pen = GetPen(color, isActive || isSelected ? 2 : 1.5, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round, dashed: shape.IsEraser);
        context.DrawGeometry(fillBrush, pen, geometry);

        // Bezier control point guides
        if (isActive)
        {
            var guidePen = GetPen(GuideColor, 1);
            for (var i = 1; i + 2 < shape.Points.Count; i++)
            {
                if (shape.Points[i].DrawType == DrawCoordinateType.BezierCurveSupport1)
                {
                    context.DrawLine(guidePen, ToZoomFactorPoint(shape.Points[i - 1]), ToZoomFactorPoint(shape.Points[i]));
                    context.DrawLine(guidePen, ToZoomFactorPoint(shape.Points[i + 2]), ToZoomFactorPoint(shape.Points[i + 1]));
                    i += 2;
                }
            }
        }
    }

    /// <summary>
    /// Fills each layer as one geometry, like libass renders a layer's line: the shapes' contours
    /// combine with non-zero winding (so holes of imported SVGs stay open) and the layer's eraser
    /// shapes (\iclip) are cut out of it.
    /// </summary>
    private void DrawLayerFills(DrawingContext context, List<DrawShape> shapes)
    {
        foreach (var layer in shapes.Where(s => !s.Hidden && s.Points.Count > 2).GroupBy(s => s.Layer).OrderBy(g => g.Key))
        {
            var drawShapes = layer.Where(s => !s.IsEraser).ToList();
            if (drawShapes.Count == 0)
            {
                continue;
            }

            Geometry geometry = BuildLayerGeometry(drawShapes);
            var eraserShapes = layer.Where(s => s.IsEraser).ToList();
            if (eraserShapes.Count > 0)
            {
                geometry = new CombinedGeometry(GeometryCombineMode.Exclude, geometry, BuildLayerGeometry(eraserShapes));
            }

            var color = drawShapes[0].ForeColor;
            var alpha = (byte)Math.Round(color.A * 215 / 255.0);
            context.DrawGeometry(GetBrush(Color.FromArgb(alpha, color.R, color.G, color.B)), null, geometry);
        }
    }

    private StreamGeometry BuildLayerGeometry(List<DrawShape> shapes)
    {
        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        ctx.SetFillRule(FillRule.NonZero);
        foreach (var shape in shapes)
        {
            AddFigure(ctx, shape, true);
        }

        return geometry;
    }

    private StreamGeometry BuildGeometry(DrawShape shape, bool isClosed)
    {
        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        ctx.SetFillRule(FillRule.NonZero);
        AddFigure(ctx, shape, isClosed);
        return geometry;
    }

    private void AddFigure(StreamGeometryContext ctx, DrawShape shape, bool isClosed)
    {
        ctx.BeginFigure(ToZoomFactorPoint(shape.Points[0]), isClosed);
        var i = 1;
        while (i < shape.Points.Count)
        {
            var point = shape.Points[i];
            if (point.DrawType == DrawCoordinateType.BezierCurveSupport1 && i + 2 < shape.Points.Count)
            {
                ctx.CubicBezierTo(
                    ToZoomFactorPoint(point),
                    ToZoomFactorPoint(shape.Points[i + 1]),
                    ToZoomFactorPoint(shape.Points[i + 2]));
                i += 3;
            }
            else
            {
                ctx.LineTo(ToZoomFactorPoint(point));
                i++;
            }
        }

        ctx.EndFigure(isClosed);
    }

    private void DrawSelectionBox(DrawingContext context, (float Left, float Top, float Right, float Bottom) bounds, bool withHandles)
    {
        var (left, top, right, bottom) = bounds;
        if (right - left < 0.01f && bottom - top < 0.01f)
        {
            return;
        }

        var rect = GetSelectionRect(bounds);
        context.DrawRectangle(null, GetPen(AccentColor, 1, dashed: true), rect);
        if (!withHandles)
        {
            return;
        }

        // Scale/rotate handles only where they work: the select tool
        var handles = GetHandlePositions(rect);
        var handlePen = GetPen(AccentColor, 1.5);
        var handleBrush = GetBrush(HandleFillColor);
        context.DrawLine(GetPen(AccentColor, 1), new Point(rect.Center.X, rect.Top), handles[RotateHandle]);
        for (var i = 0; i < RotateHandle; i++)
        {
            context.DrawRectangle(handleBrush, handlePen, new Rect(handles[i].X - 3.5, handles[i].Y - 3.5, 7, 7));
        }

        context.DrawEllipse(handleBrush, handlePen, handles[RotateHandle], 5, 5);

        if (_transformLabel != null && _transformStarted)
        {
            var text = new FormattedText(_transformLabel, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface.Default, 12, GetBrush(Colors.White));
            var origin = new Point(rect.Center.X - text.Width / 2, rect.Bottom + 10);
            context.DrawRectangle(GetBrush(AccentColor), null, new Rect(origin.X - 6, origin.Y - 3, text.Width + 12, text.Height + 6), 4, 4);
            context.DrawText(text, origin);
        }
    }

    private Rect GetSelectionRect((float Left, float Top, float Right, float Bottom) bounds)
    {
        return new Rect(
            new Point(ToZoomFactorX(bounds.Left), ToZoomFactorY(bounds.Top)),
            new Point(ToZoomFactorX(bounds.Right), ToZoomFactorY(bounds.Bottom))).Inflate(HandlePadding);
    }

    private static Point[] GetHandlePositions(Rect rect)
    {
        var center = rect.Center;
        return
        [
            rect.TopLeft, new Point(center.X, rect.Top), rect.TopRight, new Point(rect.Right, center.Y),
            rect.BottomRight, new Point(center.X, rect.Bottom), rect.BottomLeft, new Point(rect.Left, center.Y),
            new Point(center.X, rect.Top - RotateHandleDistance),
        ];
    }

    /// <summary>
    /// What the handles act on: a multi-selection, or the single selected shape.
    /// </summary>
    private List<DrawShape> GetTransformTargets()
    {
        var selected = SelectedShapes;
        if (selected.Count > 1)
        {
            return selected.Where(s => !s.Hidden && s.Points.Count > 0).ToList();
        }

        var shape = SelectedShape ?? ActiveShape;
        return shape != null && !shape.Hidden && shape.Points.Count > 0 && Shapes.Contains(shape) ? [shape] : [];
    }

    private static (float Left, float Top, float Right, float Bottom) GetUnionBounds(List<DrawShape> shapes)
    {
        var bounds = shapes[0].GetBounds();
        foreach (var shape in shapes.Skip(1))
        {
            var b = shape.GetBounds();
            bounds = (Math.Min(bounds.Left, b.Left), Math.Min(bounds.Top, b.Top), Math.Max(bounds.Right, b.Right), Math.Max(bounds.Bottom, b.Bottom));
        }

        return bounds;
    }

    private int HitTestHandle(Point screenPoint, out List<DrawShape> targets)
    {
        targets = CurrentTool == DrawingTool.Select ? GetTransformTargets() : [];
        if (targets.Count == 0)
        {
            return -1;
        }

        var bounds = GetUnionBounds(targets);
        if (bounds.Right - bounds.Left < 0.01f && bounds.Bottom - bounds.Top < 0.01f)
        {
            return -1;
        }

        var handles = GetHandlePositions(GetSelectionRect(bounds));
        for (var i = handles.Length - 1; i >= 0; i--)
        {
            if (Math.Abs(handles[i].X - screenPoint.X) <= 7 && Math.Abs(handles[i].Y - screenPoint.Y) <= 7)
            {
                return i;
            }
        }

        return -1;
    }

    private void ApplyTransform(Point screenPoint, KeyModifiers modifiers)
    {
        var (left, top, right, bottom) = _transformBounds;
        var centerX = (left + right) / 2f;
        var centerY = (top + bottom) / 2f;

        Func<float, float, (float X, float Y)> map;
        if (_transformHandle == RotateHandle)
        {
            var startAngle = Math.Atan2(FromZoomFactorY((float)_transformStart.Y) - centerY, FromZoomFactorX((float)_transformStart.X) - centerX);
            var angle = Math.Atan2(FromZoomFactorY((float)screenPoint.Y) - centerY, FromZoomFactorX((float)screenPoint.X) - centerX) - startAngle;
            var degrees = angle * 180 / Math.PI;
            degrees = (degrees + 540) % 360 - 180;
            if (modifiers.HasFlag(KeyModifiers.Shift))
            {
                degrees = Math.Round(degrees / 15) * 15;
            }

            var radians = degrees * Math.PI / 180;
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);
            map = (x, y) => (centerX + (x - centerX) * cos - (y - centerY) * sin, centerY + (x - centerX) * sin + (y - centerY) * cos);
            _transformLabel = $"{degrees:0}°";
        }
        else
        {
            // The dragged edge follows the pointer, the opposite edge stays put
            var dx = (float)(screenPoint.X - _transformStart.X) / _zoomFactor;
            var dy = (float)(screenPoint.Y - _transformStart.Y) / _zoomFactor;
            var h = _transformHandle;
            var movesLeft = h is 0 or 6 or 7;
            var movesRight = h is 2 or 3 or 4;
            var movesTop = h is 0 or 1 or 2;
            var movesBottom = h is 4 or 5 or 6;
            var width = right - left;
            var height = bottom - top;

            var anchorX = movesLeft ? right : left;
            var anchorY = movesTop ? bottom : top;
            var scaleX = 1f;
            var scaleY = 1f;
            if (width > 0.01f && (movesLeft || movesRight))
            {
                scaleX = movesLeft ? (width - dx) / width : (width + dx) / width;
            }

            if (height > 0.01f && (movesTop || movesBottom))
            {
                scaleY = movesTop ? (height - dy) / height : (height + dy) / height;
            }

            var isCorner = h is 0 or 2 or 4 or 6;
            if (isCorner && modifiers.HasFlag(KeyModifiers.Shift))
            {
                var uniform = Math.Abs(scaleX) > Math.Abs(scaleY) ? scaleX : scaleY;
                scaleX = Math.Sign(scaleX) * Math.Abs(uniform);
                scaleY = Math.Sign(scaleY) * Math.Abs(uniform);
            }

            var sx = scaleX;
            var sy = scaleY;
            map = (x, y) => (anchorX + (x - anchorX) * sx, anchorY + (y - anchorY) * sy);
            _transformLabel = $"{Math.Abs(width * sx):0} × {Math.Abs(height * sy):0}";
        }

        for (var i = 0; i < _transformShapes!.Count; i++)
        {
            var points = _transformShapes[i].Points;
            var original = _transformOriginal![i];
            for (var j = 0; j < points.Count && j < original.Length; j++)
            {
                var (x, y) = map(original[j].X, original[j].Y);
                points[j].X = x;
                points[j].Y = y;
            }
        }
    }

    private void DrawShapePoints(DrawingContext context, DrawShape shape, bool isActive)
    {
        foreach (var point in shape.Points)
        {
            var center = ToZoomFactorPoint(point);
            var isControlPoint = point.DrawType is DrawCoordinateType.BezierCurveSupport1 or DrawCoordinateType.BezierCurveSupport2;

            if (!isActive)
            {
                // Small dots keep every point grabbable without cluttering unselected shapes
                if (!isControlPoint)
                {
                    context.DrawEllipse(GetBrush(point.PointColor), null, center, 2.5, 2.5);
                }

                continue;
            }

            if (isControlPoint)
            {
                context.DrawEllipse(GetBrush(HandleFillColor), GetPen(point.PointColor, 1.5), center, 3.5, 3.5);
            }
            else
            {
                context.DrawRectangle(GetBrush(HandleFillColor), GetPen(AccentColor, 1.5), new Rect(center.X - 4, center.Y - 4, 8, 8));
            }
        }
    }

    /// <summary>
    /// The topmost visible shape under (x, y) in canvas coordinates.
    /// </summary>
    public DrawShape? HitTestShape(float x, float y)
    {
        var shapes = Shapes;
        for (var i = shapes.Count - 1; i >= 0; i--)
        {
            var shape = shapes[i];
            if (!shape.Hidden && shape.HitTest(x, y, 5f / _zoomFactor))
            {
                return shape;
            }
        }

        return null;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();

        var point = e.GetPosition(this);
        var x = FromZoomFactorX((float)point.X);
        var y = FromZoomFactorY((float)point.Y);
        var properties = e.GetCurrentPoint(this).Properties;

        if (properties.IsRightButtonPressed)
        {
            var targetPoint = GetClosePoint(x, y);
            var targetShape = targetPoint == null ? HitTestShape(x, y) : FindShape(targetPoint);
            ContextMenuRequested?.Invoke(this, new CanvasContextEventArgs(point, x, y, targetPoint, targetShape));
            e.Handled = true;
            return;
        }

        // Scale/rotate handles of the selection (select tool)
        if (properties.IsLeftButtonPressed)
        {
            var handle = HitTestHandle(point, out var targets);
            if (handle >= 0)
            {
                _transformHandle = handle;
                _transformShapes = targets;
                _transformOriginal = targets.Select(s => s.Points.Select(p => (p.X, p.Y)).ToArray()).ToList();
                _transformBounds = GetUnionBounds(targets);
                _transformStart = point;
                _transformStarted = false;
                _transformLabel = null;
                e.Handled = true;
                return;
            }
        }

        // Shift+drag or middle mouse button pans
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift) || properties.IsMiddleButtonPressed)
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
            PointSelected?.Invoke(this, closePoint);
            InvalidateVisual();
            e.Handled = true;
            return;
        }

        if (CurrentTool == DrawingTool.Select)
        {
            // Select tool: pick the shape under the pointer, and drag it to move it
            var shape = HitTestShape(x, y);
            if (shape != null)
            {
                // Dragging a shape of a multi-selection moves the whole selection
                _dragShapes = SelectedShapes.Count > 1 && SelectedShapes.Contains(shape) ? SelectedShapes.ToList() : [shape];
                _pressedShape = shape;
                _dragShapeMoved = false;
                _lastMousePosition = point;
                if (_dragShapes.Count > 1)
                {
                    InvalidateVisual();
                    e.Handled = true;
                    return;
                }
            }

            ShapeClicked?.Invoke(this, shape);
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

        if (_transformShapes != null && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (!_transformStarted)
            {
                if (Math.Abs(point.X - _transformStart.X) < 1 && Math.Abs(point.Y - _transformStart.Y) < 1)
                {
                    return;
                }

                _transformStarted = true;
                EditStarting?.Invoke(this, EventArgs.Empty);
            }

            ApplyTransform(point, e.KeyModifiers);
            InvalidateVisual();
            CanvasMouseMoved?.Invoke(this, new CanvasMouseEventArgs(x, y));
            return;
        }

        // Dragging a whole shape
        if (_dragShapes != null && _lastMousePosition.HasValue && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var dx = (float)(point.X - _lastMousePosition.Value.X) / _zoomFactor;
            var dy = (float)(point.Y - _lastMousePosition.Value.Y) / _zoomFactor;
            if (dx != 0 || dy != 0)
            {
                if (!_dragShapeMoved)
                {
                    EditStarting?.Invoke(this, EventArgs.Empty);
                }

                foreach (var shape in _dragShapes)
                {
                    shape.Offset(dx, dy);
                }

                _dragShapeMoved = true;
                _lastMousePosition = point;
                InvalidateVisual();
            }

            CanvasMouseMoved?.Invoke(this, new CanvasMouseEventArgs(x, y));
            return;
        }

        // Dragging a point
        if (ActivePoint != null && _lastMousePosition.HasValue && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (!_pointDragStarted)
            {
                _pointDragStarted = true;
                EditStarting?.Invoke(this, EventArgs.Empty);
            }

            ActivePoint.X = x;
            ActivePoint.Y = y;
            PointDragged?.Invoke(this, ActivePoint);
            InvalidateVisual();
            return;
        }

        UpdateHoverCursor(point, x, y);
        CanvasMouseMoved?.Invoke(this, new CanvasMouseEventArgs(x, y));
    }

    private void UpdateHoverCursor(Point point, float x, float y)
    {
        Cursor? cursor = null;
        if (CurrentTool == DrawingTool.Select)
        {
            var handle = HitTestHandle(point, out _);
            if (handle >= 0)
            {
                cursor = HandleCursors[handle];
            }
            else if (GetClosePoint(x, y) == null && HitTestShape(x, y) != null)
            {
                cursor = MoveCursor;
            }
        }

        if (Cursor != cursor)
        {
            Cursor = cursor;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isPanning = false;
        _lastMousePosition = null;
        _pointDragStarted = false;

        var dragShapes = _dragShapes;
        var moved = _dragShapeMoved;
        _dragShapes = null;
        _dragShapeMoved = false;
        if (dragShapes != null && moved)
        {
            ShapeMoved?.Invoke(this, dragShapes[0]);
        }
        else if (dragShapes is { Count: > 1 } && _pressedShape != null)
        {
            // A click (no drag) on a shape of a multi-selection selects just that shape
            ShapeClicked?.Invoke(this, _pressedShape);
        }

        _pressedShape = null;

        var transformShapes = _transformShapes;
        var transformed = _transformStarted;
        _transformShapes = null;
        _transformOriginal = null;
        _transformHandle = -1;
        _transformStarted = false;
        _transformLabel = null;
        if (transformShapes != null && transformed && transformShapes.Count > 0)
        {
            ShapeMoved?.Invoke(this, transformShapes[0]);
        }

        InvalidateVisual();
    }

    private DrawShape? FindShape(DrawCoordinate point)
    {
        return point.DrawShape ?? Shapes.FirstOrDefault(s => s.Points.Contains(point));
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            // Zoom with Ctrl+Scroll
            // Zoom around the mouse pointer
            ZoomAt(e.Delta.Y > 0 ? _zoomFactor * 1.1f : _zoomFactor / 1.1f, e.GetPosition(this));
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

public class CanvasContextEventArgs : EventArgs
{
    /// <summary>
    /// Pointer position in control coordinates (where the menu opens).
    /// </summary>
    public Point Position { get; }

    public float X { get; }
    public float Y { get; }
    public DrawCoordinate? Point { get; }
    public DrawShape? Shape { get; }

    public CanvasContextEventArgs(Point position, float x, float y, DrawCoordinate? point, DrawShape? shape)
    {
        Position = position;
        X = x;
        Y = y;
        Point = point;
        Shape = shape;
    }
}
