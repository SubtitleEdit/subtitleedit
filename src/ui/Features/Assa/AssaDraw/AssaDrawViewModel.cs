using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaSetPosition;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PickLayer;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Features.Video.GoToVideoPosition;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

public partial class AssaDrawViewModel : ObservableObject
{
    public Window? Window { get; set; }
    public AssaDrawCanvas? Canvas { get; set; }
    public Button? CopyToClipboardButton { get; set; }

    public bool OkPressed { get; private set; }

    /// <summary>
    /// The generated ASSA drawing code result.
    /// </summary>
    public string AssaDrawingCode { get; private set; } = string.Empty;

    [ObservableProperty] private List<DrawShape> _shapes = [];
    [ObservableProperty] private DrawShape? _activeShape;
    [ObservableProperty] private DrawCoordinate? _activePoint;
    [ObservableProperty] private DrawingTool _currentTool = DrawingTool.Line;
    [ObservableProperty] private int _canvasWidth = 1920;
    [ObservableProperty] private int _canvasHeight = 1080;
    [ObservableProperty] private string _positionText = "Position: 0, 0";
    [ObservableProperty] private string _zoomText = "Zoom: 100%";
    [ObservableProperty] private float _pointX;
    [ObservableProperty] private float _pointY;
    [ObservableProperty] private bool _isPointSelected;
    [ObservableProperty] private bool _isLayerSelected;
    [ObservableProperty] private bool _isShapeSelected;
    [ObservableProperty] private bool _shapeIsEraser;
    [ObservableProperty] private Color _layerColor = Colors.White;
    [ObservableProperty] private bool _showGrid = DrawSettings.ShowGrid;
    [ObservableProperty] private bool _showPreview;
    [ObservableProperty] private string _previewStatusText = string.Empty;
    [ObservableProperty] private ObservableCollection<ShapeTreeItem> _shapeTreeItems = [];
    [ObservableProperty] private ShapeTreeItem? _selectedTreeItem;
    [ObservableProperty] private List<DrawShape> _selectedShapes = [];
    [ObservableProperty] private ShapeLibraryItem? _currentLibraryShape = ShapeLibrary.BuiltIn[0];
    [ObservableProperty] private float _shapeX;
    [ObservableProperty] private float _shapeY;
    [ObservableProperty] private float _shapeWidth;
    [ObservableProperty] private float _shapeHeight;
    [ObservableProperty] private int _shapeLayer;
    [ObservableProperty] private string _selectionText = string.Empty;
    [ObservableProperty] private string _codePreview = string.Empty;
    [ObservableProperty] private double _zoomPercent = 100;
    [ObservableProperty] private double _backgroundOpacity = Math.Clamp(Se.Settings.Assa.DrawBackgroundOpacity, 0.1, 1);
    [ObservableProperty] private bool _backgroundStretch = Se.Settings.Assa.DrawBackgroundStretch;
    [ObservableProperty] private bool _hasBackground;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UndoCommand))]
    private bool _canUndo;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RedoCommand))]
    private bool _canRedo;

    public Subtitle ResultSubtitle { get; set; } = new Subtitle();

    private float _currentX = float.MinValue;
    private float _currentY = float.MinValue;
    private readonly Regex _regexStart = new(@"\{[^{]*\\p1[^}]*\}");
    private readonly Regex _regexEnd = new(@"\{[^{]*\\p0[^}]*\}");
    private readonly Regex _regexIclip = new(@"\\iclip\(([^)]+)\)");
    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private string _fileName = string.Empty;
    private Subtitle? _subtitle;
    private string? _videoFileName;
    private double _videoSeconds;
    private bool _backgroundRequested;
    private int _backgroundVersion;
    private DispatcherTimer? _previewTimer;
    private string _previewSource = string.Empty;
    private bool _previewBusy;
    private bool _refreshingTree;
    private bool _updatingShapeFields;
    private bool _updatingLayerColor;

    private const int MaxUndoSteps = 100;
    private readonly List<UndoState> _undoStack = [];
    private readonly List<UndoState> _redoStack = [];
    private string? _lastUndoKey;
    private DateTime _lastUndoTime;

    public AssaDrawViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
    }

    public void Initialize()
    {
        UiUtil.RestoreWindowPosition(Window);
        ZoomToFitCurrentVideoResolution();
        RefreshTreeView();
        Canvas?.InvalidateVisual();

        if (DrawSettings.ShowPreview)
        {
            _ = TogglePreview();
        }
    }

    private void ZoomToFitCurrentVideoResolution()
    {
        if (Canvas == null)
        {
            return;
        }

        // Wait a bit to ensure the canvas bounds are updated
        Dispatcher.UIThread.Post(() =>
        {
            Canvas?.FitToView();
            UpdateZoomText();
        }, DispatcherPriority.Background);
    }

    public void Initialize(Subtitle subtitle, List<SubtitleLineViewModel> selectedLines, int? width, int? height,
        string? videoFileName = null, double? videoPositionSeconds = null)
    {
        _subtitle = subtitle;
        _videoFileName = videoFileName;

        // Same frame Set position uses: the paused player position when it is inside the line
        _videoSeconds = selectedLines.Count > 0
            ? AssaSetPositionViewModel.GetScreenshotSeconds(selectedLines[0], videoPositionSeconds)
            : videoPositionSeconds ?? 0;

        // Drawing coordinates live in script (PlayRes) space, so the script resolution wins. The main
        // window writes the video size into the header before opening, so without a video this is the
        // only place the real resolution comes from - the 1920x1080 default put shapes in the wrong spot.
        if (TryGetPlayRes(subtitle.Header, out var playResX, out var playResY))
        {
            CanvasWidth = playResX;
            CanvasHeight = playResY;
        }
        else if (width is > 0 && height is > 0)
        {
            CanvasWidth = width.Value;
            CanvasHeight = height.Value;
        }

        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(subtitle.Header);

        foreach (var line in selectedLines)
        {
            var style = styles.FirstOrDefault(s => s.Name.Equals(line.Style, StringComparison.OrdinalIgnoreCase)) ?? new SsaStyle();
            var color = style.Primary.ToAvaloniaColor();

            // 1. Process iclip (Vector Mask)
            var iclipMatch = _regexIclip.Match(line.Text);
            if (iclipMatch.Success)
            {
                // Extract only the drawing commands inside the parentheses
                var clipCommands = iclipMatch.Groups[1].Value;
                ImportAssaDrawingFromText(clipCommands, line.Layer, color, true);
            }

            // 2. Process \p1 Drawing (the \p1 can share its tag block with \pos, \an etc.)
            if (_regexStart.IsMatch(line.Text))
            {
                // Remove all tags (anything inside curly braces) to get raw vector data
                string drawingOnly = Regex.Replace(line.Text, @"\{[^}]+\}", string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(drawingOnly))
                {
                    ImportAssaDrawingFromText(drawingOnly, line.Layer, color, false);
                }
            }
        }

        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    public void SetCanvas(AssaDrawCanvas canvas)
    {
        Canvas = canvas;
        Canvas.Shapes = Shapes;
        Canvas.CanvasWidth = CanvasWidth;
        Canvas.CanvasHeight = CanvasHeight;
        Canvas.CurrentTool = CurrentTool;
        Canvas.BackgroundOpacity = BackgroundOpacity;
        Canvas.BackgroundStretch = BackgroundStretch;

        Canvas.CanvasClicked += OnCanvasClicked;
        Canvas.CanvasMouseMoved += OnCanvasMouseMoved;
        Canvas.PointSelected += OnPointSelected;
        Canvas.PointDragged += OnPointDragged;
        Canvas.ZoomChanged += OnZoomChanged;
        Canvas.ShapeClicked += OnShapeClicked;
        Canvas.ShapeMoved += OnShapeMoved;
        Canvas.EditStarting += (_, _) => SaveUndo();
        Canvas.InsertRequested += OnInsertRequested;
        Canvas.ColorPicked += (_, color) => PickColor(color);
        Canvas.InsertShape = CurrentLibraryShape;
    }

    partial void OnCurrentLibraryShapeChanged(ShapeLibraryItem? value)
    {
        if (Canvas != null)
        {
            Canvas.InsertShape = value;
        }
    }

    [RelayCommand]
    private void ShapeTool() => SetTool(DrawingTool.Shape);

    [RelayCommand]
    private void ColorPickerTool() => SetTool(DrawingTool.ColorPicker);

    /// <summary>
    /// Eyedropper result: becomes the current color - new shapes get it, and a selected shape's
    /// (or layer's) color changes to it.
    /// </summary>
    public void PickColor(Color color)
    {
        LayerColor = color;
        UpdateSelectionInfo();
    }

    /// <summary>
    /// New shapes join the layer that already has the current color, or start a new layer - one
    /// color per layer is what the ASSA output can hold (a white shape put on a yellow layer 0
    /// came out yellow).
    /// </summary>
    private int GetLayerForColor(Color color)
    {
        var existing = Shapes.FirstOrDefault(s => s.ForeColor == color && !s.IsEraser);
        if (existing != null)
        {
            return existing.Layer;
        }

        return Shapes.Count == 0 ? 0 : Shapes.Max(s => s.Layer) + 1;
    }

    /// <summary>
    /// Picks the shape the shape tool places (and switches to the shape tool).
    /// </summary>
    [RelayCommand]
    private void PickLibraryShape(ShapeLibraryItem? item)
    {
        if (item == null)
        {
            return;
        }

        CurrentLibraryShape = item;
        SetTool(DrawingTool.Shape);
    }

    private void OnInsertRequested(object? sender, CanvasInsertEventArgs e) => InsertLibraryShape(e);

    /// <summary>
    /// Places the current library shape on a new layer above the drawing, in the current color,
    /// then selects it with the select tool so it can be moved and resized right away.
    /// </summary>
    public void InsertLibraryShape(CanvasInsertEventArgs e)
    {
        var item = CurrentLibraryShape;
        if (item == null)
        {
            return;
        }

        var x = e.X;
        var y = e.Y;
        var width = e.Width;
        var height = e.Height;
        if (e.IsClick)
        {
            // Default size: a fifth of the frame width (or a third of its height for tall shapes)
            width = CanvasWidth / 5f;
            height = width * item.AspectRatio;
            if (height > CanvasHeight / 3f)
            {
                height = CanvasHeight / 3f;
                width = height / item.AspectRatio;
            }

            x -= width / 2f;
            y -= height / 2f;
        }

        CancelDrawing();
        SaveUndo();
        var firstLayer = Shapes.Count == 0 ? 0 : Shapes.Max(s => s.Layer) + 1;
        var shapes = item.CreateShapes(x, y, width, height, LayerColor, firstLayer);
        Shapes.AddRange(shapes);
        SetTool(DrawingTool.Select);
        RefreshTreeView();
        if (shapes.Count == 1)
        {
            SelectShape(shapes[0]);
        }
        else
        {
            SelectedShapes = shapes;
            UpdateSelectionInfo();
        }

        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private async Task AddToLibrary()
    {
        var shapes = SelectedShapes.Count > 1 ? SelectedShapes.ToList() : TargetShape is { } target ? [target] : [];
        if (shapes.Count == 0 || Window == null)
        {
            return;
        }

        var vm = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window, vm =>
            vm.Initialize(Se.Language.Assa.DrawShapeName, string.Empty, 300, 30, returnSubmits: true));
        if (!vm.OkPressed || string.IsNullOrWhiteSpace(vm.Text))
        {
            return;
        }

        AddToLibrary(vm.Text.Trim(), shapes);
    }

    /// <summary>
    /// Saves shapes to "My shapes" and makes the new item the shape tool's current shape.
    /// </summary>
    public ShapeLibraryItem AddToLibrary(string name, List<DrawShape> shapes)
    {
        var saved = ShapeLibrary.Save(name, shapes);
        var item = ShapeLibrary.GetUserShapes().First(i => i.UserShape == saved);
        CurrentLibraryShape = item;
        return item;
    }

    [RelayCommand]
    private void RemoveFromLibrary(ShapeLibraryItem? item)
    {
        if (item?.UserShape == null)
        {
            return;
        }

        ShapeLibrary.Remove(item.UserShape);
        if (CurrentLibraryShape == item || CurrentLibraryShape?.UserShape == item.UserShape)
        {
            CurrentLibraryShape = ShapeLibrary.BuiltIn[0];
        }
    }

    private void OnShapeClicked(object? sender, DrawShape? shape)
    {
        if (shape == null)
        {
            ClearSelection();
            return;
        }

        SelectShape(shape);
    }

    private void OnShapeMoved(object? sender, DrawShape shape)
    {
        // Point labels in the tree changed - rebuild once at the end of the drag, not on every move
        RefreshTreeView();
    }

    /// <summary>
    /// Right-click on the canvas: select what is under the pointer so the context menu acts on it.
    /// </summary>
    public void PrepareContextMenu(CanvasContextEventArgs e)
    {
        if (e.Point != null)
        {
            SelectPoint(e.Point);
        }
        else if (e.Shape != null)
        {
            SelectShape(e.Shape);
        }
    }

    public bool IsDrawing => ActiveShape != null && !Shapes.Contains(ActiveShape);

    /// <summary>
    /// The finished shape the shape commands act on: the one picked in the tree or on the canvas,
    /// or the shape of the selected point.
    /// </summary>
    public DrawShape? TargetShape
    {
        get
        {
            if (SelectedTreeItem?.Shape != null)
            {
                return SelectedTreeItem.Shape;
            }

            if (SelectedTreeItem?.Point != null)
            {
                var pointShape = SelectedTreeItem.Point.DrawShape ?? Shapes.FirstOrDefault(s => s.Points.Contains(SelectedTreeItem.Point));
                if (pointShape != null && Shapes.Contains(pointShape))
                {
                    return pointShape;
                }
            }

            return ActiveShape != null && Shapes.Contains(ActiveShape) ? ActiveShape : null;
        }
    }

    /// <summary>
    /// Layers in use, for the "Move to layer" menu.
    /// </summary>
    public List<int> UsedLayers => Shapes.Select(s => s.Layer).Distinct().OrderBy(l => l).ToList();

    public void SelectShape(DrawShape shape)
    {
        var item = FindTreeItem(null, shape, null);
        if (item != null)
        {
            SelectedTreeItem = item;
        }
    }

    public void SelectPoint(DrawCoordinate point)
    {
        var item = FindTreeItem(point, null, null);
        if (item != null)
        {
            SelectedTreeItem = item;
        }
    }

    private void ClearSelection()
    {
        if (SelectedTreeItem != null)
        {
            SelectedTreeItem = null;
        }

        if (ActiveShape != null && Shapes.Contains(ActiveShape))
        {
            ActiveShape = null;
        }

        ActivePoint = null;
        IsPointSelected = false;
        SelectedShapes = [];
        if (Canvas != null)
        {
            Canvas.SelectedShape = null;
            Canvas.InvalidateVisual();
        }
    }

    private void OnZoomChanged(object? sender, float zoomFactor)
    {
        ZoomText = $"Zoom: {zoomFactor * 100:0}%";
        if (Math.Abs(ZoomPercent - zoomFactor * 100) > 0.5)
        {
            ZoomPercent = Math.Round(zoomFactor * 100);
        }
    }

    partial void OnZoomPercentChanged(double value)
    {
        // Zoom slider: zoom around the middle of the view
        if (Canvas != null && Math.Abs(Canvas.ZoomFactor * 100 - value) > 0.5)
        {
            Canvas.ZoomAt((float)(value / 100), Canvas.Bounds.Center);
        }
    }

    /// <summary>
    /// Fills the shape fields (position, size, layer), the status text and the code preview from the selection.
    /// </summary>
    private void UpdateSelectionInfo()
    {
        var shape = TargetShape;
        _updatingShapeFields = true;
        try
        {
            if (shape != null && shape.Points.Count > 0)
            {
                var (left, top, right, bottom) = shape.GetBounds();
                ShapeX = MathF.Round(left, 1);
                ShapeY = MathF.Round(top, 1);
                ShapeWidth = MathF.Round(right - left, 1);
                ShapeHeight = MathF.Round(bottom - top, 1);
                ShapeLayer = shape.Layer;
            }
        }
        finally
        {
            _updatingShapeFields = false;
        }

        var item = SelectedTreeItem;
        SelectionText = SelectedShapes.Count > 1
            ? $"{SelectedShapes.Count} shapes"
            : item?.Point != null
                ? item.Name.Trim()
                : shape != null
                    ? $"{string.Format(Se.Language.Assa.DrawLayerX, shape.Layer)} · {shape.Points.Count} points"
                    : item?.IsLayer == true
                        ? item.Name
                        : string.Empty;

        var code = GenerateAssaCode();
        CodePreview = code.Length > 400 ? code[..400] + "..." : code;
    }

    partial void OnShapeXChanged(float value) => MoveTargetShape(value - (TargetShape?.GetBounds().Left ?? value), 0, "shape-x");

    partial void OnShapeYChanged(float value) => MoveTargetShape(0, value - (TargetShape?.GetBounds().Top ?? value), "shape-y");

    private void MoveTargetShape(float dx, float dy, string undoKey)
    {
        var shape = TargetShape;
        if (_updatingShapeFields || shape == null || (Math.Abs(dx) < 0.001f && Math.Abs(dy) < 0.001f))
        {
            return;
        }

        SaveUndo($"{undoKey}-{RuntimeHelpers.GetHashCode(shape)}");
        shape.Offset(dx, dy);
        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    partial void OnShapeWidthChanged(float value) => ResizeTargetShape(value, null);

    partial void OnShapeHeightChanged(float value) => ResizeTargetShape(null, value);

    /// <summary>
    /// Size fields: scale the shape from its top-left corner.
    /// </summary>
    private void ResizeTargetShape(float? width, float? height)
    {
        var shape = TargetShape;
        if (_updatingShapeFields || shape == null)
        {
            return;
        }

        var (left, top, right, bottom) = shape.GetBounds();
        var scaleX = width.HasValue && right - left > 0.01f && width.Value > 0.01f ? width.Value / (right - left) : 1f;
        var scaleY = height.HasValue && bottom - top > 0.01f && height.Value > 0.01f ? height.Value / (bottom - top) : 1f;
        if (Math.Abs(scaleX - 1) < 0.0001f && Math.Abs(scaleY - 1) < 0.0001f)
        {
            return;
        }

        SaveUndo($"shape-size-{RuntimeHelpers.GetHashCode(shape)}");
        foreach (var point in shape.Points)
        {
            point.X = left + (point.X - left) * scaleX;
            point.Y = top + (point.Y - top) * scaleY;
        }

        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    partial void OnShapeLayerChanged(int value)
    {
        if (!_updatingShapeFields && TargetShape != null && TargetShape.Layer != value && value >= 0)
        {
            MoveShapeToLayer(value);
        }
    }

    private void OnCanvasClicked(object? sender, CanvasClickEventArgs e)
    {
        if (!e.IsLeftButton)
        {
            return;
        }

        var x = e.X;
        var y = e.Y;

        if (ShowGrid)
        {
            x = MathF.Round(x / DrawSettings.GridSize) * DrawSettings.GridSize;
            y = MathF.Round(y / DrawSettings.GridSize) * DrawSettings.GridSize;
        }

        ActivePoint = null;
        IsPointSelected = false;
        SelectedShapes = [];

        // Continue drawing on existing shape
        if (ActiveShape != null && ActiveShape.Points.Count > 0 && !Shapes.Contains(ActiveShape))
        {
            AddPointToActiveShape(x, y);
            Canvas?.InvalidateVisual();
            return;
        }

        // Start new shape
        StartNewShape(x, y);
        Canvas?.InvalidateVisual();
    }

    private void AddPointToActiveShape(float x, float y)
    {
        if (ActiveShape == null)
        {
            return;
        }

        switch (CurrentTool)
        {
            case DrawingTool.Line:
                ActiveShape.AddPoint(DrawCoordinateType.Line, x, y, DrawSettings.PointColor);
                break;

            case DrawingTool.Bezier:
                // Add two control points and endpoint
                var lastPoint = ActiveShape.Points[^1];
                var oneThirdX = (x - lastPoint.X) / 3f;
                var oneThirdY = (y - lastPoint.Y) / 3f;

                ActiveShape.AddPoint(DrawCoordinateType.BezierCurveSupport1,
                    lastPoint.X + oneThirdX, lastPoint.Y + oneThirdY, DrawSettings.PointHelperColor);
                ActiveShape.AddPoint(DrawCoordinateType.BezierCurveSupport2,
                    lastPoint.X + oneThirdX * 2, lastPoint.Y + oneThirdY * 2, DrawSettings.PointHelperColor);
                ActiveShape.AddPoint(DrawCoordinateType.BezierCurve, x, y, DrawSettings.PointColor);
                break;

            case DrawingTool.Circle:
                // Complete the circle with second click
                if (ActiveShape.Points.Count == 1)
                {
                    var start = ActiveShape.Points[0];
                    var radius = Math.Max(Math.Abs(x - start.X), Math.Abs(y - start.Y));
                    if (radius > 1)
                    {
                        SaveUndo();
                        ActiveShape = CircleBezier.MakeCircle(start.X, start.Y, radius, ActiveShape.Layer, ActiveShape.ForeColor);
                        Shapes.Add(ActiveShape);
                        RefreshTreeView();
                        ActiveShape = null;
                        _currentX = float.MinValue;
                        _currentY = float.MinValue;
                        if (Canvas != null)
                        {
                            Canvas.Shapes = Shapes;
                            Canvas.ActiveShape = null;
                            Canvas.CurrentX = float.MinValue;
                            Canvas.CurrentY = float.MinValue;
                        }
                    }
                }
                break;

            case DrawingTool.Rectangle:
                // Complete the rectangle with second click
                if (ActiveShape.Points.Count == 1)
                {
                    var start = ActiveShape.Points[0];
                    SaveUndo();
                    ActiveShape = MakeRectangle(start.X, start.Y, x - start.X, y - start.Y, ActiveShape.Layer, ActiveShape.ForeColor);
                    Shapes.Add(ActiveShape);
                    RefreshTreeView();
                    ActiveShape = null;
                    _currentX = float.MinValue;
                    _currentY = float.MinValue;
                    if (Canvas != null)
                    {
                        Canvas.Shapes = Shapes;
                        Canvas.ActiveShape = null;
                        Canvas.CurrentX = float.MinValue;
                        Canvas.CurrentY = float.MinValue;
                    }
                }
                break;
        }
    }

    private void StartNewShape(float x, float y)
    {
        ActiveShape = new DrawShape { ForeColor = LayerColor, Layer = GetLayerForColor(LayerColor) };

        switch (CurrentTool)
        {
            case DrawingTool.Line:
                ActiveShape.AddPoint(DrawCoordinateType.Line, x, y, DrawSettings.PointColor);
                break;

            case DrawingTool.Bezier:
                ActiveShape.AddPoint(DrawCoordinateType.BezierCurve, x, y, DrawSettings.PointColor);
                break;

            case DrawingTool.Circle:
            case DrawingTool.Rectangle:
                ActiveShape.AddPoint(DrawCoordinateType.Line, x, y, DrawSettings.PointColor);
                break;
        }

        if (Canvas != null)
        {
            Canvas.ActiveShape = ActiveShape;
        }
    }

    private void OnCanvasMouseMoved(object? sender, CanvasMouseEventArgs e)
    {
        _currentX = e.X;
        _currentY = e.Y;
        PositionText = $"Position: {e.X:0}, {e.Y:0}";

        if (Canvas != null)
        {
            Canvas.CurrentX = _currentX;
            Canvas.CurrentY = _currentY;
            Canvas.InvalidateVisual();
        }
    }

    private void OnPointSelected(object? sender, DrawCoordinate point)
    {
        // Select the point in the tree too, so the shape commands act on the clicked point's shape
        if (FindTreeItem(point, null, null) is { } item && !ReferenceEquals(SelectedTreeItem, item))
        {
            SelectedTreeItem = item;
        }

        ActivePoint = point;
        PointX = point.X;
        PointY = point.Y;
        IsPointSelected = true;
    }

    private void OnPointDragged(object? sender, DrawCoordinate point)
    {
        PointX = point.X;
        PointY = point.Y;

        // Only the dragged point's label changes - rebuilding the whole tree on every mouse move
        // was slow on big drawings and dropped the tree selection.
        UpdateSelectedPointName();
    }

    [RelayCommand]
    private void SelectTool() => SetTool(DrawingTool.Select);

    [RelayCommand]
    private void LineTool() => SetTool(DrawingTool.Line);

    [RelayCommand]
    private void BezierTool() => SetTool(DrawingTool.Bezier);

    [RelayCommand]
    private void RectangleTool() => SetTool(DrawingTool.Rectangle);

    [RelayCommand]
    private void CircleTool() => SetTool(DrawingTool.Circle);

    private void SetTool(DrawingTool tool)
    {
        CurrentTool = tool;
        if (Canvas != null)
        {
            Canvas.CurrentTool = tool;
        }
    }

    [RelayCommand]
    private void CloseShape()
    {
        // A shape picked in the tree is already finished - closing it again appended a closing
        // bezier to it (Bezier tool) or added a stray circle/rectangle copy (Circle/Rectangle tool).
        if (ActiveShape == null || Shapes.Contains(ActiveShape))
        {
            return;
        }

        // For circle/rectangle tools, allow closing with 1 point if cursor is active
        var isCircleOrRect = CurrentTool == DrawingTool.Circle || CurrentTool == DrawingTool.Rectangle;
        var hasValidCursor = _currentX > float.MinValue && _currentY > float.MinValue;

        if (ActiveShape.Points.Count < 1 ||
            (ActiveShape.Points.Count < 2 && !isCircleOrRect) ||
            (ActiveShape.Points.Count < 3 && !isCircleOrRect && !hasValidCursor))
        {
            return;
        }

        // Handle circle/rectangle special cases
        if (CurrentTool == DrawingTool.Circle && _currentX > float.MinValue)
        {
            var start = ActiveShape.Points[0];
            var radius = Math.Max(Math.Abs(_currentX - start.X), Math.Abs(_currentY - start.Y));
            if (radius > 1)
            {
                ActiveShape = CircleBezier.MakeCircle(start.X, start.Y, radius, ActiveShape.Layer, ActiveShape.ForeColor);
            }
        }
        else if (CurrentTool == DrawingTool.Rectangle && _currentX > float.MinValue)
        {
            var start = ActiveShape.Points[0];
            ActiveShape = MakeRectangle(start.X, start.Y, _currentX - start.X, _currentY - start.Y,
                ActiveShape.Layer, ActiveShape.ForeColor);
        }
        else if (CurrentTool == DrawingTool.Bezier && ActiveShape.Points.Count >= 1)
        {
            // Close with a bezier curve from last point back to first point
            var lastPoint = ActiveShape.Points[^1];
            var firstPoint = ActiveShape.Points[0];

            // Calculate control points for a smooth closing bezier curve
            var oneThirdX = (firstPoint.X - lastPoint.X) / 3f;
            var oneThirdY = (firstPoint.Y - lastPoint.Y) / 3f;

            ActiveShape.AddPoint(DrawCoordinateType.BezierCurveSupport1,
                lastPoint.X + oneThirdX, lastPoint.Y + oneThirdY, DrawSettings.PointHelperColor);
            ActiveShape.AddPoint(DrawCoordinateType.BezierCurveSupport2,
                lastPoint.X + oneThirdX * 2, lastPoint.Y + oneThirdY * 2, DrawSettings.PointHelperColor);
            ActiveShape.AddPoint(DrawCoordinateType.BezierCurve,
                firstPoint.X, firstPoint.Y, DrawSettings.PointColor);
        }

        if (!Shapes.Contains(ActiveShape))
        {
            SaveUndo();
            Shapes.Add(ActiveShape);
        }

        RefreshTreeView();
        ActiveShape = null;
        _currentX = float.MinValue;
        _currentY = float.MinValue;

        if (Canvas != null)
        {
            Canvas.ActiveShape = null;
            Canvas.CurrentX = float.MinValue;
            Canvas.CurrentY = float.MinValue;
            Canvas.Shapes = Shapes;
            Canvas.InvalidateVisual();
        }
    }

    private static DrawShape MakeRectangle(float x, float y, float width, float height, int layer, Color color)
    {
        var shape = new DrawShape { ForeColor = color, Layer = layer };
        shape.AddPoint(DrawCoordinateType.Line, x, y, DrawSettings.PointColor);
        shape.AddPoint(DrawCoordinateType.Line, x + width, y, DrawSettings.PointColor);
        shape.AddPoint(DrawCoordinateType.Line, x + width, y + height, DrawSettings.PointColor);
        shape.AddPoint(DrawCoordinateType.Line, x, y + height, DrawSettings.PointColor);
        return shape;
    }

    [RelayCommand]
    private void DeleteShape()
    {
        var shape = IsDrawing ? ActiveShape : TargetShape;
        if (shape != null)
        {
            if (ActivePoint != null && shape.Points.Contains(ActivePoint))
            {
                ActivePoint = null;
                IsPointSelected = false;
            }

            if (Shapes.Contains(shape))
            {
                SaveUndo();
            }

            Shapes.Remove(shape);
            SelectedShapes = SelectedShapes.Where(s => s != shape).ToList();
            ActiveShape = null;
            _currentX = float.MinValue;
            _currentY = float.MinValue;
            if (Canvas != null)
            {
                Canvas.SelectedShape = null;
                Canvas.CurrentX = float.MinValue;
                Canvas.CurrentY = float.MinValue;
            }

            RefreshTreeView();
            Canvas?.InvalidateVisual();
        }
    }

    [RelayCommand]
    private async Task ChangeLayer()
    {
        if (Window == null || SelectedTreeItem == null)
        {
            return;
        }

        // Determine the current layer based on selection
        int currentLayer;
        if (SelectedTreeItem.IsLayer)
        {
            currentLayer = SelectedTreeItem.Layer;
        }
        else if (SelectedTreeItem.Shape != null)
        {
            currentLayer = SelectedTreeItem.Shape.Layer;
        }
        else
        {
            return;
        }

        var vm = new PickLayerViewModel { Layer = currentLayer };
        var pickLayerWindow = new PickLayerWindow(vm);
        await WindowService.ShowModalAsync(Window, pickLayerWindow);

        if (!vm.OkPressed || vm.Layer == currentLayer)
        {
            return;
        }

        var newLayer = vm.Layer;
        SaveUndo();

        // Get color from existing shapes in the new layer (if any)
        var existingShapeInNewLayer = Shapes.FirstOrDefault(s => s.Layer == newLayer);
        var newColor = existingShapeInNewLayer?.ForeColor;

        if (SelectedTreeItem.IsLayer)
        {
            // Change layer for all shapes in the selected layer
            foreach (var shape in Shapes.Where(s => s.Layer == currentLayer).ToList())
            {
                shape.Layer = newLayer;
                if (newColor.HasValue)
                {
                    shape.ForeColor = newColor.Value;
                }
            }
        }
        else if (SelectedTreeItem.Shape != null)
        {
            // Change layer for just the selected shape
            SelectedTreeItem.Shape.Layer = newLayer;
            if (newColor.HasValue)
            {
                SelectedTreeItem.Shape.ForeColor = newColor.Value;
            }
        }

        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void DuplicateShape()
    {
        var shape = TargetShape;
        if (shape == null)
        {
            return;
        }

        SaveUndo();
        var copy = shape.Clone();
        copy.Hidden = false;
        copy.Offset(DrawSettings.GridSize, DrawSettings.GridSize);
        Shapes.Insert(Shapes.IndexOf(shape) + 1, copy);
        RefreshTreeView();
        SelectShape(copy);
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void FlipShapeHorizontal()
    {
        var shape = TargetShape;
        if (shape == null)
        {
            return;
        }

        SaveUndo();
        shape.FlipHorizontal();
        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void RotateShapeClockwise() => RotateShape(90);

    [RelayCommand]
    private void RotateShapeCounterClockwise() => RotateShape(-90);

    private void RotateShape(float degrees)
    {
        var shape = TargetShape;
        if (shape == null)
        {
            return;
        }

        SaveUndo();
        shape.Rotate(degrees);
        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void FlipShapeVertical()
    {
        var shape = TargetShape;
        if (shape == null)
        {
            return;
        }

        SaveUndo();
        shape.FlipVertical();
        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void ToggleShapeEraser()
    {
        var shape = TargetShape;
        if (shape == null)
        {
            return;
        }

        SaveUndo();
        shape.IsEraser = !shape.IsEraser;
        if (shape == ActiveShape)
        {
            ShapeIsEraser = shape.IsEraser;
        }

        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void ToggleShapeVisibility()
    {
        var shape = TargetShape;
        if (shape == null)
        {
            return;
        }

        SaveUndo();
        shape.Hidden = !shape.Hidden;
        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void ToggleLayerVisibility()
    {
        if (SelectedTreeItem?.IsLayer != true)
        {
            return;
        }

        var layerShapes = Shapes.Where(s => s.Layer == SelectedTreeItem.Layer).ToList();
        var hide = layerShapes.Any(s => !s.Hidden);
        SaveUndo();
        foreach (var shape in layerShapes)
        {
            shape.Hidden = hide;
        }

        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void DeleteLayer()
    {
        if (SelectedTreeItem?.IsLayer != true)
        {
            return;
        }

        var layer = SelectedTreeItem.Layer;
        SaveUndo();
        Shapes.RemoveAll(s => s.Layer == layer);
        if (ActiveShape != null && ActiveShape.Layer == layer && !IsDrawing)
        {
            ActiveShape = null;
        }

        ActivePoint = null;
        IsPointSelected = false;
        SelectedShapes = SelectedShapes.Where(s => s.Layer != layer).ToList();
        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void MoveShapeToLayer(int layer)
    {
        var shape = TargetShape;
        if (shape == null || shape.Layer == layer)
        {
            return;
        }

        SaveUndo();

        // Same rule as Change layer: a shape joining a layer takes that layer's color
        var existing = Shapes.FirstOrDefault(s => s.Layer == layer);
        shape.Layer = layer;
        if (existing != null)
        {
            shape.ForeColor = existing.ForeColor;
        }

        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void DeletePoint()
    {
        var point = ActivePoint;
        var shape = point == null ? null : point.DrawShape ?? Shapes.FirstOrDefault(s => s.Points.Contains(point));
        if (point == null || shape == null || point.DrawType is DrawCoordinateType.BezierCurveSupport1 or DrawCoordinateType.BezierCurveSupport2)
        {
            return;
        }

        SaveUndo();
        if (!shape.RemovePoint(point))
        {
            return;
        }

        ActivePoint = null;
        IsPointSelected = false;
        if (shape.Points.Count == 0)
        {
            Shapes.Remove(shape);
            if (ActiveShape == shape)
            {
                ActiveShape = null;
            }
        }

        RefreshTreeView();
        if (Shapes.Contains(shape))
        {
            SelectShape(shape);
        }

        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void ConvertPointToCurve()
    {
        var point = ActivePoint;
        var shape = point == null ? null : point.DrawShape ?? Shapes.FirstOrDefault(s => s.Points.Contains(point));
        if (point == null || shape == null || !shape.IsLineSegment(point))
        {
            return;
        }

        SaveUndo();
        shape.ConvertSegmentToCurve(point);
        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void ConvertPointToLine()
    {
        var point = ActivePoint;
        var shape = point == null ? null : point.DrawShape ?? Shapes.FirstOrDefault(s => s.Points.Contains(point));
        if (point == null || shape == null || !shape.IsCurveSegment(point))
        {
            return;
        }

        SaveUndo();
        var end = shape.GetSegmentEnd(point)!;
        shape.ConvertSegmentToLine(point);

        // A removed control point can't stay selected - select the segment's end point instead
        if (point != end)
        {
            ActivePoint = null;
            IsPointSelected = false;
        }

        RefreshTreeView();
        if (point != end)
        {
            SelectPoint(end);
        }

        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void ConvertShapeToCurves()
    {
        var shape = TargetShape;
        if (shape == null || !shape.Points.Any(shape.IsLineSegment))
        {
            return;
        }

        SaveUndo();
        shape.ConvertAllLinesToCurves();
        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void ConvertShapeToLines()
    {
        var shape = TargetShape;
        if (shape == null || !shape.Points.Any(shape.IsCurveSegment))
        {
            return;
        }

        SaveUndo();
        if (ActivePoint != null && ActivePoint.DrawType is DrawCoordinateType.BezierCurveSupport1 or DrawCoordinateType.BezierCurveSupport2)
        {
            ActivePoint = null;
            IsPointSelected = false;
        }

        shape.ConvertAllCurvesToLines();
        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void CancelDrawing()
    {
        if (!IsDrawing)
        {
            return;
        }

        ActiveShape = null;
        _currentX = float.MinValue;
        _currentY = float.MinValue;
        if (Canvas != null)
        {
            Canvas.ActiveShape = null;
            Canvas.CurrentX = float.MinValue;
            Canvas.CurrentY = float.MinValue;
            Canvas.InvalidateVisual();
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        if (Shapes.Count > 0)
        {
            SaveUndo();
        }

        ClearAllShapes();
    }

    private void ClearAllShapes()
    {
        Shapes.Clear();
        ActiveShape = null;
        ActivePoint = null;
        IsPointSelected = false;
        SelectedShapes = [];
        _currentX = float.MinValue;
        _currentY = float.MinValue;
        RefreshTreeView();

        if (Canvas != null)
        {
            Canvas.Shapes = Shapes;
            Canvas.ActiveShape = null;
            Canvas.ActivePoint = null;
            Canvas.InvalidateVisual();
        }
    }

    [RelayCommand]
    private void ZoomIn()
    {
        Canvas?.ZoomIn();
        UpdateZoomText();
    }

    [RelayCommand]
    private void ZoomOut()
    {
        Canvas?.ZoomOut();
        UpdateZoomText();
    }

    [RelayCommand]
    private void ResetView()
    {
        // Back to the opening view: whole frame fitted and centered (was 100% at the top-left
        // corner, which for a 1080p frame is mostly off screen).
        Canvas?.FitToView();
        UpdateZoomText();
    }

    private void UpdateZoomText()
    {
        if (Canvas != null)
        {
            ZoomText = $"Zoom: {Canvas.ZoomFactor * 100:0}%";
        }
    }

    [RelayCommand]
    private void ToggleGrid()
    {
        ShowGrid = !ShowGrid;
        DrawSettings.ShowGrid = ShowGrid;
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private async Task TogglePreview()
    {
        if (!ShowPreview && Window != null)
        {
            var ffmpegOk = await FfmpegRequirement.EnsureAsync(
                Window,
                async () => (await _windowService.ShowDialogAsync<DownloadFfmpegWindow, DownloadFfmpegViewModel>(Window)).FfmpegFileName);
            if (!ffmpegOk)
            {
                return;
            }
        }

        ShowPreview = !ShowPreview;
        DrawSettings.ShowPreview = ShowPreview;
    }

    partial void OnShowPreviewChanged(bool value)
    {
        if (Canvas != null)
        {
            Canvas.ShowPreview = value;
        }

        if (!value)
        {
            _previewTimer?.Stop();
            PreviewStatusText = string.Empty;
            return;
        }

        LoadPreviewBackground();
        _previewSource = string.Empty;
        if (_previewTimer == null)
        {
            // Shapes change from many places (clicks, drags, tree edits, nudges, colors), so poll the
            // generated script instead of hooking each one - ffmpeg only runs when it differs.
            _previewTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _previewTimer.Tick += (_, _) => UpdatePreview();
        }

        _previewTimer.Start();
        UpdatePreview();
    }

    /// <summary>
    /// The libass preview shows over the video frame, so the first preview loads it unless a
    /// background was already picked (or removed) by hand.
    /// </summary>
    private void LoadPreviewBackground()
    {
        if (_backgroundRequested || !HasVideo)
        {
            return;
        }

        _ = LoadVideoFrameBackgroundAsync(_videoSeconds);
    }

    public bool HasVideo => !string.IsNullOrEmpty(_videoFileName) && File.Exists(_videoFileName);

    /// <summary>
    /// Shows a video frame behind the drawing. Returns false, keeping the current background,
    /// when no frame could be extracted (position past the end, undecodable video, no ffmpeg).
    /// </summary>
    internal async Task<bool> LoadVideoFrameBackgroundAsync(double seconds)
    {
        if (!HasVideo)
        {
            return false;
        }

        var wasRequested = _backgroundRequested;
        _backgroundRequested = true;
        var version = ++_backgroundVersion;
        var videoFileName = _videoFileName!;
        var position = seconds.ToString("0.###", CultureInfo.InvariantCulture);
        Bitmap? bitmap = null;
        try
        {
            bitmap = await Task.Run(() => LoadBitmapAndDelete(FfmpegGenerator.GetScreenShot(videoFileName, position)));
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "ASSA draw: could not get video frame at " + position + " from " + videoFileName);
        }

        if (bitmap == null)
        {
            // A newer background request replaced this one - nothing to report
            if (version != _backgroundVersion)
            {
                return true;
            }

            _backgroundRequested = wasRequested;
            return false;
        }

        SetBackground(bitmap, version);
        return true;
    }

    private async Task LoadVideoFrameBackgroundOrShowError(double seconds)
    {
        if (Window == null)
        {
            return;
        }

        var ffmpegOk = await FfmpegRequirement.EnsureAsync(
            Window,
            async () => (await _windowService.ShowDialogAsync<DownloadFfmpegWindow, DownloadFfmpegViewModel>(Window)).FfmpegFileName);
        if (!ffmpegOk)
        {
            return;
        }

        if (!await LoadVideoFrameBackgroundAsync(seconds))
        {
            var timeCode = new TimeCode(TimeSpan.FromSeconds(seconds)).ToDisplayString();
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.Format(Se.Language.Assa.DrawBackgroundVideoFrameFailed, timeCode), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Shows an image file behind the drawing. Returns false when it can't be read as an image.
    /// </summary>
    public bool LoadImageBackground(string fileName)
    {
        Bitmap bitmap;
        try
        {
            using var stream = File.OpenRead(fileName);
            bitmap = new Bitmap(stream);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "ASSA draw: could not load background image " + fileName);
            return false;
        }

        _backgroundRequested = true;
        SetBackground(bitmap, ++_backgroundVersion);
        return true;
    }

    private void SetBackground(Bitmap? bitmap, int version)
    {
        // A slower ffmpeg grab must not replace a background picked after it was started
        if (Canvas == null || version != _backgroundVersion)
        {
            bitmap?.Dispose();
            return;
        }

        var old = Canvas.BackgroundImage as IDisposable;
        Canvas.BackgroundImage = bitmap;
        old?.Dispose();
        HasBackground = bitmap != null;
    }

    [RelayCommand(CanExecute = nameof(HasVideo))]
    private Task BackgroundFromVideo() => LoadVideoFrameBackgroundOrShowError(_videoSeconds);

    [RelayCommand(CanExecute = nameof(HasVideo))]
    private async Task BackgroundFromVideoAt()
    {
        if (Window == null)
        {
            return;
        }

        var vm = await _windowService.ShowDialogAsync<GoToVideoPositionWindow, GoToVideoPositionViewModel>(
            Window, vm => vm.Time = TimeSpan.FromSeconds(_videoSeconds));
        if (!vm.OkPressed)
        {
            return;
        }

        _videoSeconds = Math.Max(0, vm.Time.TotalSeconds);
        await LoadVideoFrameBackgroundOrShowError(_videoSeconds);
    }

    [RelayCommand]
    private async Task BackgroundFromImage()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.Assa.DrawBackgroundImage.TrimEnd('.'),
            Se.Language.Assa.DrawImages, "*.png;*.jpg;*.jpeg;*.bmp;*.webp");
        if (!string.IsNullOrEmpty(fileName) && !LoadImageBackground(fileName))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Path.GetFileName(fileName), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    [RelayCommand]
    private void RemoveBackground()
    {
        // Also keeps the preview from bringing the video frame back
        _backgroundRequested = true;
        SetBackground(null, ++_backgroundVersion);
    }

    [RelayCommand]
    private void ToggleBackgroundStretch() => BackgroundStretch = !BackgroundStretch;

    partial void OnBackgroundOpacityChanged(double value)
    {
        Se.Settings.Assa.DrawBackgroundOpacity = value;
        if (Canvas != null)
        {
            Canvas.BackgroundOpacity = value;
        }
    }

    partial void OnBackgroundStretchChanged(bool value)
    {
        Se.Settings.Assa.DrawBackgroundStretch = value;
        if (Canvas != null)
        {
            Canvas.BackgroundStretch = value;
        }
    }

    internal void UpdatePreview()
    {
        if (_previewBusy || Canvas == null || !ShowPreview)
        {
            return;
        }

        var subtitle = GenerateSubtitle();
        var source = new AdvancedSubStationAlpha().ToText(subtitle, string.Empty) + CanvasWidth + "x" + CanvasHeight;
        if (source == _previewSource)
        {
            return;
        }

        _previewSource = source;
        _previewBusy = true;
        var width = CanvasWidth;
        var height = CanvasHeight;
        _ = Task.Run(() =>
        {
            Bitmap? bitmap = null;
            try
            {
                bitmap = subtitle.Paragraphs.Count == 0
                    ? null
                    : LoadBitmapAndDelete(FfmpegGenerator.GetScreenShotWithSubtitle(subtitle, width, height));
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "ASSA draw preview failed");
            }

            var failed = bitmap == null && subtitle.Paragraphs.Count > 0;
            Dispatcher.UIThread.Post(() =>
            {
                _previewBusy = false;
                PreviewStatusText = failed && ShowPreview ? Se.Language.Assa.DrawPreviewFailed : string.Empty;
                if (Canvas == null)
                {
                    bitmap?.Dispose();
                    return;
                }

                var old = Canvas.PreviewImage as IDisposable;
                Canvas.PreviewImage = bitmap;
                old?.Dispose();
            });
        });
    }

    private static Bitmap? LoadBitmapAndDelete(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return null;
        }

        try
        {
            using var stream = File.OpenRead(fileName);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
        finally
        {
            try
            {
                File.Delete(fileName);
            }
            catch
            {
                // ignore cleanup errors
            }
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (Shapes.Count == 0)
        {
            return;
        }

        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(
            Window,
            ".assadraw",
            string.IsNullOrEmpty(_fileName) ? "untitled.assadraw" : Path.GetFileName(_fileName),
            "Save ASSA drawing");

        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        _fileName = fileName;

        try
        {
            var subtitle = GenerateSubtitle();
            var format = new AdvancedSubStationAlpha();
            var text = format.ToText(subtitle, string.Empty);
            await File.WriteAllTextAsync(fileName, text);
        }
        catch (Exception ex)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    [RelayCommand]
    private async Task Load()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(
            Window,
            "Open ASSA drawing",
            "ASSA drawing files",
            "*.assadraw",
            "ASS files",
            "*.ass");

        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            var text = await File.ReadAllTextAsync(fileName);
            _fileName = fileName;
            LoadFromText(text);
        }
        catch (Exception ex)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    [RelayCommand]
    private async Task ImportSvg()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.Assa.DrawImportSvg.TrimEnd('.'), Se.Language.Assa.DrawSvgImages, "*.svg");
        if (!string.IsNullOrEmpty(fileName))
        {
            await ImportSvgFile(fileName);
        }
    }

    public async Task ImportSvgFile(string fileName)
    {
        try
        {
            var text = await File.ReadAllTextAsync(fileName);
            if (!ImportSvgText(text) && Window != null)
            {
                await MessageBox.Show(Window, Se.Language.Assa.DrawImportSvg.TrimEnd('.'), Se.Language.Assa.DrawSvgNoShapes, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex) when (ex is System.Xml.XmlException or IOException or UnauthorizedAccessException)
        {
            if (Window != null)
            {
                await MessageBox.Show(Window, Se.Language.General.Error, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// Adds the SVG's shapes on new layers above the existing drawing. Returns false when it had none.
    /// </summary>
    public bool ImportSvgText(string svgText)
    {
        var firstLayer = Shapes.Count == 0 ? 0 : Shapes.Max(s => s.Layer) + 1;
        var result = SvgImporter.Import(svgText, CanvasWidth, CanvasHeight, firstLayer);
        if (result.Shapes.Count == 0)
        {
            return false;
        }

        CancelDrawing();
        SaveUndo();
        Shapes.AddRange(result.Shapes);
        SelectedShapes = result.Shapes.ToList();
        RefreshTreeView();
        Canvas?.InvalidateVisual();
        return true;
    }

    private static readonly string[] BackgroundImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".webp"];

    internal void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = GetDroppedFile(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    /// <summary>
    /// An .svg is imported as shapes, an image becomes the background.
    /// </summary>
    internal void OnDrop(object? sender, DragEventArgs e)
    {
        var fileName = GetDroppedFile(e);
        if (fileName == null)
        {
            return;
        }

        e.Handled = true;
        if (fileName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            Dispatcher.UIThread.Post(() => _ = ImportSvgFile(fileName));
        }
        else
        {
            Dispatcher.UIThread.Post(() => LoadImageBackground(fileName));
        }
    }

    private static string? GetDroppedFile(DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            return null;
        }

        return e.DataTransfer.TryGetFiles()?
            .Select(f => f.Path.LocalPath)
            .FirstOrDefault(f => f.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) ||
                                 BackgroundImageExtensions.Any(x => f.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
    }

    [RelayCommand]
    private async Task CopyToClipboard()
    {
        var code = GenerateAssaCode();
        if (string.IsNullOrEmpty(code) || Window?.Clipboard == null)
        {
            return;
        }

        await ClipboardHelper.SetTextAsync(Window, code);

        if (CopyToClipboardButton?.Content is Optris.Icons.Avalonia.Icon icon)
        {
            icon.Value = "fa-solid fa-check";
            await Task.Delay(1500);
            icon.Value = "fa-solid fa-copy";
        }
    }

    private Subtitle GenerateSubtitle(bool includeAll = false)
    {
        var subtitle = new Subtitle
        {
            Header = AdvancedSubStationAlpha.DefaultHeader
        };
        
        if (_subtitle != null)
        {
            subtitle.Header = _subtitle.Header;
            subtitle.Footer = _subtitle.Footer;
        }

        // Shapes are drawn in canvas coordinates, so a header without a resolution needs the canvas
        // size - libass falls back to 384x288 and scaled the drawing far off screen. The default header
        // has no PlayRes lines at all, so the old "PlayResX: 384" replace never matched. An existing
        // resolution is kept: it positions every other line of the script too.
        if (!TryGetPlayRes(subtitle.Header, out _, out _))
        {
            subtitle.Header = AdvancedSubStationAlpha.SetResolution(subtitle.Header, CanvasWidth, CanvasHeight);
        }

        // Collect unique colors from all layers
        var colorToStyleName = new Dictionary<Color, string>();
        var layers = Shapes.Where(s => !s.Hidden || includeAll).GroupBy(s => s.Layer).OrderBy(g => g.Key).ToList();
        
        foreach (var layer in layers)
        {
            var firstShape = GetLayerStyleShape(layer);
            if (firstShape != null && !colorToStyleName.ContainsKey(firstShape.ForeColor))
            {
                var color = firstShape.ForeColor;
                var colorName = GetColorName(color);
                var styleName = $"AssaDraw{colorName}";
                colorToStyleName[color] = styleName;

                // Create and add style to header
                var style = new SsaStyle
                {
                    Name = styleName,
                    Alignment = "7", // top/left
                    MarginVertical = 0,
                    MarginLeft = 0,
                    MarginRight = 0,
                    ShadowWidth = 0,
                    OutlineWidth = 0,
                    Primary = color.ToSkColor(),
                };
                subtitle.Header = AdvancedSubStationAlpha.AddSsaStyle(style, subtitle.Header);
            }
        }

        // Generate paragraphs with style names
        var sbDraw = new StringBuilder();
        var sbErase = new StringBuilder();
        var finalText = new StringBuilder();
        foreach (var layer in layers)
        {
            sbDraw.Clear();
            sbErase.Clear();
            var firstShape = GetLayerStyleShape(layer);

            // Collect draw shapes (normal shapes)
            foreach (var shape in layer.Where(p => !p.IsEraser))
            {
                sbDraw.Append(shape.ToAssa());
                sbDraw.Append("  ");
            }

            // Collect erase shapes (iclip shapes)
            foreach (var shape in layer.Where(p => p.IsEraser))
            {
                sbErase.Append(shape.ToAssa());
                sbErase.Append("  ");
            }

            var drawText = sbDraw.ToString().Trim();
            var eraseText = sbErase.ToString().Trim();

            // Build the final text with draw and optionally iclip
            if (!string.IsNullOrEmpty(drawText) || !string.IsNullOrEmpty(eraseText))
            {
                finalText.Clear();

                // Add iclip if we have erase shapes
                if (!string.IsNullOrEmpty(eraseText))
                {
                    finalText.Append($"{{\\iclip({eraseText})}}");
                }

                // Add draw shapes
                if (!string.IsNullOrEmpty(drawText))
                {
                    finalText.Append($"{{\\p1}}{drawText}{{\\p0}}");
                }

                if (finalText.Length > 0)
                {
                    string styleName = string.Empty;
                    if (firstShape != null)
                    {
                        styleName = colorToStyleName.GetValueOrDefault(firstShape.ForeColor, "Default");
                    }
                  
                    var p = new Paragraph(finalText.ToString(), 0, 10000)
                    {
                        Layer = layer.Key,
                        Extra = styleName,
                    };
                    subtitle.Paragraphs.Add(p);
                }
            }
        }

        return subtitle;
    }

    /// <summary>
    /// The shape whose color names the layer's style: the first draw shape, or for a layer of only
    /// erase shapes (written as an \iclip-only line) the first erase shape. The style loop and the
    /// line loop picked differently, so an erase-only layer got a line with an empty style name.
    /// </summary>
    private static DrawShape? GetLayerStyleShape(IEnumerable<DrawShape> layer)
    {
        DrawShape? firstEraser = null;
        foreach (var shape in layer)
        {
            if (!shape.IsEraser)
            {
                return shape;
            }

            firstEraser ??= shape;
        }

        return firstEraser;
    }

    private static string GetColorName(Color color)
    {
        // Translucent colors get the alpha in the name - otherwise e.g. a 60% white and an opaque
        // white shared one style name and the second style replaced the first.
        var name = GetRgbName(color);
        return color.A < 255 ? $"{name}A{color.A:X2}" : name;
    }

    private static string GetRgbName(Color color)
    {
        // Create a readable color name based on RGB values
        if (color is { R: 255, G: 255, B: 255 })
        {
            return "White";
        }
        if (color.R == 0 && color is { G: 0, B: 0 })
        {
            return "Black";
        }
        if (color is { R: 255, G: 0, B: 0 })
        {
            return "Red";
        }
        if (color is { R: 0, G: 255, B: 0 })
        {
            return "Green";
        }
        if (color is { R: 0, G: 0, B: 255 })
        {
            return "Blue";
        }
        if (color is { R: 255, G: 255, B: 0 })
        {
            return "Yellow";
        }
        if (color is { R: 255, G: 0, B: 255 })
        {
            return "Magenta";
        }
        if (color is { R: 0, G: 255, B: 255 })
        {
            return "Cyan";
        }
        if (color is { R: 255, G: 165, B: 0 })
        {
            return "Orange";
        }
        
        // For other colors, use hex representation
        return $"{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private string GenerateAssaCode()
    {
        if (Shapes.Count == 0)
        {
            return string.Empty;
        }

        var sbDraw = new StringBuilder();
        foreach (var shape in Shapes.Where(s => !s.IsEraser && !s.Hidden))
        {
            sbDraw.Append(shape.ToAssa());
            sbDraw.Append(' ');
        }

        var sbErase = new StringBuilder();
        foreach (var shape in Shapes.Where(s => s.IsEraser && !s.Hidden))
        {
            sbErase.Append(shape.ToAssa());
            sbErase.Append(' ');
        }

        var drawCode = sbDraw.ToString().Trim();
        var eraseCode = sbErase.ToString().Trim();

        if (string.IsNullOrEmpty(drawCode) && string.IsNullOrEmpty(eraseCode))
        {
            return string.Empty;
        }

        var result = new StringBuilder();
        if (!string.IsNullOrEmpty(eraseCode))
        {
            result.Append($"{{\\iclip({eraseCode})}}");
        }
        if (!string.IsNullOrEmpty(drawCode))
        {
            result.Append($"{{\\p1}}{drawCode}{{\\p0}}");
        }

        return result.ToString();
    }

    [RelayCommand]
    private void Ok()
    {
        AssaDrawingCode = GenerateAssaCode();
        ResultSubtitle = GenerateSubtitle(true);
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private void RefreshTreeView()
    {
        var previous = SelectedTreeItem;

        _refreshingTree = true;
        try
        {
            ShapeTreeItems.Clear();

            var layers = Shapes.GroupBy(s => s.Layer).OrderBy(g => g.Key);
            foreach (var layer in layers)
            {
                var layerShapes = layer.ToList();
                var layerItem = new ShapeTreeItem
                {
                    Name = string.Format(Se.Language.Assa.DrawLayerX, layer.Key),
                    IsLayer = true,
                    Layer = layer.Key,
                    IsExpanded = true,
                    IconName = "fa-solid fa-layer-group",
                    Swatch = new SolidColorBrush(layerShapes[0].ForeColor),
                    IsHidden = layerShapes.All(s => s.Hidden),
                    Meta = layerShapes.Count.ToString(CultureInfo.InvariantCulture),
                };

                var shapeNumber = 0;
                foreach (var shape in layerShapes)
                {
                    shapeNumber++;
                    var shapeItem = new ShapeTreeItem
                    {
                        Name = $"Shape {shapeNumber} ({(shape.IsEraser ? "erase" : "draw")})",
                        Shape = shape,
                        IsExpanded = shape.Expanded,
                        IconName = shape.IsEraser ? "fa-solid fa-eraser" : "fa-solid fa-draw-polygon",
                        IsHidden = shape.Hidden,
                        Meta = shape.Points.Count.ToString(CultureInfo.InvariantCulture),
                    };

                    foreach (var point in shape.Points)
                    {
                        var isControlPoint = point.DrawType is DrawCoordinateType.BezierCurveSupport1 or DrawCoordinateType.BezierCurveSupport2;
                        shapeItem.Children.Add(new ShapeTreeItem
                        {
                            Name = point.GetText(point.X, point.Y),
                            Point = point,
                            IconName = isControlPoint ? "fa-regular fa-circle" : "fa-regular fa-square",
                            IsHidden = shape.Hidden,
                        });
                    }

                    layerItem.Children.Add(shapeItem);
                }

                ShapeTreeItems.Add(layerItem);
            }
        }
        finally
        {
            _refreshingTree = false;
        }

        // Keep the selection across the rebuild - it used to vanish after every edit
        var restored = previous == null
            ? null
            : FindTreeItem(previous.Point, previous.Shape, previous.IsLayer ? previous.Layer : null);
        if (restored != null)
        {
            SelectedTreeItem = restored;
        }
        else if (SelectedTreeItem != null)
        {
            SelectedTreeItem = null;
        }
        else
        {
            ApplyTreeSelection(null);
        }
    }

    /// <summary>
    /// The tree item for a point, shape or layer, with its parents expanded so it can be shown selected.
    /// </summary>
    private ShapeTreeItem? FindTreeItem(DrawCoordinate? point, DrawShape? shape, int? layer)
    {
        foreach (var layerItem in ShapeTreeItems)
        {
            if (layer.HasValue && point == null && shape == null && layerItem.Layer == layer.Value)
            {
                return layerItem;
            }

            foreach (var shapeItem in layerItem.Children)
            {
                if (shape != null && point == null && shapeItem.Shape == shape)
                {
                    layerItem.IsExpanded = true;
                    return shapeItem;
                }

                if (point == null)
                {
                    continue;
                }

                foreach (var pointItem in shapeItem.Children)
                {
                    if (pointItem.Point == point)
                    {
                        layerItem.IsExpanded = true;
                        shapeItem.IsExpanded = true;
                        return pointItem;
                    }
                }
            }
        }

        return null;
    }

    private void ImportAssaDrawingFromText(string text, int layer, Color color, bool isEraser)
    {
        Shapes.AddRange(DrawShape.ParseAssa(text, layer, color, isEraser));
    }

    private static bool TryGetPlayRes(string? header, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (string.IsNullOrEmpty(header))
        {
            return false;
        }

        var playResX = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", header);
        var playResY = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", header);
        return int.TryParse(playResX, NumberStyles.Integer, CultureInfo.InvariantCulture, out width) && width >= 125 && width <= 4096 &&
               int.TryParse(playResY, NumberStyles.Integer, CultureInfo.InvariantCulture, out height) && height >= 125 && height <= 4096;
    }

    private void LoadFromText(string text)
    {
        if (Shapes.Count > 0)
        {
            SaveUndo();
        }

        ClearAllShapes();

        var subtitle = new Subtitle();
        var format = new AdvancedSubStationAlpha();
        format.LoadSubtitle(subtitle, text.SplitToLines(), _fileName);

        // Read resolution from header
        if (TryGetPlayRes(subtitle.Header, out var width, out var height))
        {
            CanvasWidth = width;
            CanvasHeight = height;
        }

        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(subtitle.Header);

        foreach (var paragraph in subtitle.Paragraphs)
        {
            var color = Colors.White;
            if (!string.IsNullOrEmpty(paragraph.Extra))
            {
                var style = styles.FirstOrDefault(s => s.Name.Equals(paragraph.Extra, StringComparison.OrdinalIgnoreCase));
                if (style != null)
                {
                    color = style.Primary.ToAvaloniaColor();
                }
            }

            // Handle iclip
            var iclipMatch = _regexIclip.Match(paragraph.Text);
            if (iclipMatch.Success)
            {
                ImportAssaDrawingFromText(iclipMatch.Groups[1].Value, paragraph.Layer, color, true);
            }

            // Handle \p1 Drawing (the \p1 can share its tag block with \pos, \an etc.)
            if (_regexStart.IsMatch(paragraph.Text))
            {
                // Strip tags to avoid parsing non-coordinate text
                string drawingOnly = Regex.Replace(paragraph.Text, @"\{[^}]+\}", string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(drawingOnly))
                {
                    ImportAssaDrawingFromText(drawingOnly, paragraph.Layer, color, false);
                }
            }
        }

        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    internal async void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (IsDrawing)
            {
                CancelDrawing();
                e.Handled = true;
                return;
            }

            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Enter)
        {
            CloseShape();
            e.Handled = true;
        }
        else if (e.Key == Key.Delete && (IsDrawing || TargetShape != null))
        {
            DeleteShape();
            e.Handled = true;
        }
        else if ((e.KeyModifiers & ~KeyModifiers.Shift) is KeyModifiers.Control or KeyModifiers.Meta && e.Key is Key.Z or Key.Y)
        {
            // Ctrl/Cmd+Z undo, Ctrl/Cmd+Shift+Z or Ctrl+Y redo
            if (e.Key == Key.Y || e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                Redo();
            }
            else
            {
                Undo();
            }

            e.Handled = true;
        }
        else if (e.KeyModifiers.HasFlag(KeyModifiers.Alt) ||
                 (e.KeyModifiers.HasFlag(KeyModifiers.Control) && !e.KeyModifiers.HasFlag(KeyModifiers.Shift)))
        {
            var offset = e.KeyModifiers.HasFlag(KeyModifiers.Alt) ? 1 : 10;

            switch (e.Key)
            {
                case Key.Up:
                    AdjustPosition(0, -offset);
                    e.Handled = true;
                    break;
                case Key.Down:
                    AdjustPosition(0, offset);
                    e.Handled = true;
                    break;
                case Key.Left:
                    AdjustPosition(-offset, 0);
                    e.Handled = true;
                    break;
                case Key.Right:
                    AdjustPosition(offset, 0);
                    e.Handled = true;
                    break;
                case Key.D0:
                case Key.NumPad0:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                    {
                        ResetView();
                        e.Handled = true;
                    }
                    break;
                case Key.OemPlus:
                case Key.Add:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                    {
                        ZoomIn();
                        e.Handled = true;
                    }
                    break;
                case Key.OemMinus:
                case Key.Subtract:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                    {
                        ZoomOut();
                        e.Handled = true;
                    }
                    break;
                case Key.C:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                    {
                        await CopyToClipboard();
                        e.Handled = true;
                    }
                    break;
                case Key.N:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                    {
                        ClearAll();
                        e.Handled = true;
                    }
                    break;
                case Key.G:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                    {
                        ToggleGrid();
                        e.Handled = true;
                    }
                    break;
                case Key.A:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                    {
                        SelectAllShapes();
                        e.Handled = true;
                    }
                    break;
                case Key.D:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                    {
                        DuplicateShape();
                        e.Handled = true;
                    }
                    break;
            }
        }
        else if (e.KeyModifiers == KeyModifiers.None && !IsTextInput(e) && e.Key is Key.V or Key.L or Key.B or Key.R or Key.C or Key.S or Key.I)
        {
            // Single-key tools like other drawing programs (not while typing in a number box)
            SetTool(e.Key switch
            {
                Key.V => DrawingTool.Select,
                Key.L => DrawingTool.Line,
                Key.B => DrawingTool.Bezier,
                Key.R => DrawingTool.Rectangle,
                Key.S => DrawingTool.Shape,
                Key.I => DrawingTool.ColorPicker,
                _ => DrawingTool.Circle,
            });
            e.Handled = true;
        }
        else if (e.Key == Key.F4)
        {
            LineTool();
            e.Handled = true;
        }
        else if (e.Key == Key.F5)
        {
            BezierTool();
            e.Handled = true;
        }
        else if (e.Key == Key.F6)
        {
            RectangleTool();
            e.Handled = true;
        }
        else if (e.Key == Key.F7)
        {
            CircleTool();
            e.Handled = true;
        }
        else if (e.Key == Key.F8)
        {
            CloseShape();
            e.Handled = true;
        }
        else if (e.Key == Key.F9)
        {
            e.Handled = true;
            await TogglePreview();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/assa-draw");
        }
    }

    private static bool IsTextInput(KeyEventArgs e)
    {
        return e.Source is Avalonia.Visual visual &&
               (visual is TextBox || Avalonia.VisualTree.VisualExtensions.FindAncestorOfType<TextBox>(visual) != null);
    }

    private void AdjustPosition(float xAdjust, float yAdjust)
    {
        if (SelectedShapes.Count == 0 && SelectedTreeItem?.Shape == null && ActiveShape == null)
        {
            return;
        }

        // Holding an arrow key is one undo step, not one per repeat
        SaveUndo("nudge");

        // Check if multiple shapes are selected (Ctrl+A scenario)
        if (SelectedShapes.Count > 0)
        {
            foreach (var shape in SelectedShapes)
            {
                foreach (var point in shape.Points)
                {
                    point.X += xAdjust;
                    point.Y += yAdjust;
                }
            }
            RefreshTreeView();
            Canvas?.InvalidateVisual();
            return;
        }

        // Check if a shape is selected in the tree view
        if (SelectedTreeItem?.Shape != null)
        {
            foreach (var point in SelectedTreeItem.Shape.Points)
            {
                point.X += xAdjust;
                point.Y += yAdjust;
            }
            RefreshTreeView();
            Canvas?.InvalidateVisual();
            return;
        }

        // Fall back to active shape (currently being drawn)
        if (ActiveShape != null)
        {
            foreach (var point in ActiveShape.Points)
            {
                point.X += xAdjust;
                point.Y += yAdjust;
            }
            RefreshTreeView();
            Canvas?.InvalidateVisual();
        }
    }

    partial void OnCanvasWidthChanged(int value)
    {
        if (Canvas != null)
        {
            Canvas.CanvasWidth = value;
            ZoomToFitCurrentVideoResolution();
        }
    }

    partial void OnCanvasHeightChanged(int value)
    {
        if (Canvas != null)
        {
            Canvas.CanvasHeight = value;
            ZoomToFitCurrentVideoResolution();
        }
    }

    partial void OnActiveShapeChanged(DrawShape? value)
    {
        if (Canvas != null)
        {
            Canvas.ActiveShape = value;
        }

        UndoCommand.NotifyCanExecuteChanged();
    }

    partial void OnActivePointChanged(DrawCoordinate? value)
    {
        if (Canvas != null)
        {
            Canvas.ActivePoint = value;
        }
    }

    partial void OnSelectedShapesChanged(List<DrawShape> value)
    {
        if (Canvas != null)
        {
            Canvas.SelectedShapes = value;
        }
    }

    partial void OnPointXChanged(float value)
    {
        if (ActivePoint != null && Math.Abs(ActivePoint.X - value) > 0.001f)
        {
            SaveUndo($"point-x-{RuntimeHelpers.GetHashCode(ActivePoint)}");
            ActivePoint.X = value;
            UpdateSelectedPointName();
            Canvas?.InvalidateVisual();
        }
    }

    partial void OnPointYChanged(float value)
    {
        if (ActivePoint != null && Math.Abs(ActivePoint.Y - value) > 0.001f)
        {
            SaveUndo($"point-y-{RuntimeHelpers.GetHashCode(ActivePoint)}");
            ActivePoint.Y = value;
            UpdateSelectedPointName();
            Canvas?.InvalidateVisual();
        }
    }

    partial void OnShapeIsEraserChanged(bool value)
    {
        if (ActiveShape != null && ActiveShape.IsEraser != value)
        {
            if (Shapes.Contains(ActiveShape))
            {
                SaveUndo();
            }

            ActiveShape.IsEraser = value;
            RefreshTreeView();
            Canvas?.InvalidateVisual();
        }
    }

    private void UpdateSelectedPointName()
    {
        if (ActivePoint == null)
        {
            return;
        }

        // Find and update the tree item that contains this point
        foreach (var layerItem in ShapeTreeItems)
        {
            foreach (var shapeItem in layerItem.Children)
            {
                foreach (var pointItem in shapeItem.Children)
                {
                    if (pointItem.Point == ActivePoint)
                    {
                        pointItem.Name = ActivePoint.GetText(ActivePoint.X, ActivePoint.Y);
                        return;
                    }
                }
            }
        }
    }

    partial void OnSelectedTreeItemChanged(ShapeTreeItem? value)
    {
        if (_refreshingTree)
        {
            return;
        }

        ApplyTreeSelection(value);
    }

    private void ApplyTreeSelection(ShapeTreeItem? value)
    {
        if (value != null)
        {
            if (value.Point != null)
            {
                ActivePoint = value.Point;
                PointX = value.Point.X;
                PointY = value.Point.Y;
                IsPointSelected = true;
            }
            else
            {
                ActivePoint = null;
                IsPointSelected = false;
            }

            // Active shape for canvas rendering and shape properties
            if (value.Shape != null)
            {
                ActiveShape = value.Shape;
                ShapeIsEraser = value.Shape.IsEraser;
                SetLayerColorFromSelection(value.Shape.ForeColor);
            }
        }

        // Update layer selection state
        IsLayerSelected = value?.IsLayer == true;
        IsShapeSelected = value?.Shape != null;

        // A tree pick ends a Ctrl+A selection - otherwise the arrow keys kept nudging every shape.
        if (value != null)
        {
            SelectedShapes = [];
        }
        
        if (value?.IsLayer == true)
        {
            // Get color from first shape in this layer
            var firstShape = Shapes.FirstOrDefault(s => s.Layer == value.Layer);
            if (firstShape != null)
            {
                SetLayerColorFromSelection(firstShape.ForeColor);
            }
        }

        if (Canvas != null)
        {
            Canvas.SelectedShape = value?.Shape ?? value?.Point?.DrawShape;
            Canvas.InvalidateVisual();
        }

        UpdateSelectionInfo();
    }

    /// <summary>
    /// Shows the selection's color without recoloring its layer - a layer can hold shapes of different colors.
    /// </summary>
    private void SetLayerColorFromSelection(Color color)
    {
        _updatingLayerColor = true;
        try
        {
            LayerColor = color;
        }
        finally
        {
            _updatingLayerColor = false;
        }
    }

    partial void OnLayerColorChanged(Color value)
    {
        if (_updatingLayerColor)
        {
            return;
        }

        // The color belongs to the layer: picked on a layer, or on a shape of it
        int? layer = SelectedTreeItem?.IsLayer == true ? SelectedTreeItem.Layer : TargetShape?.Layer;
        if (layer == null || !Shapes.Any(s => s.Layer == layer && s.ForeColor != value))
        {
            return;
        }

        SaveUndo("layer-color");
        foreach (var shape in Shapes.Where(s => s.Layer == layer))
        {
            shape.ForeColor = value;
        }

        var layerItem = FindTreeItem(null, null, layer);
        if (layerItem != null)
        {
            layerItem.Swatch = new SolidColorBrush(value);
        }

        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void ToggleItemVisibility(ShapeTreeItem? item)
    {
        if (item == null)
        {
            return;
        }

        var shapes = item.IsLayer
            ? Shapes.Where(s => s.Layer == item.Layer).ToList()
            : item.Shape != null ? [item.Shape] : [];
        if (shapes.Count == 0)
        {
            return;
        }

        SaveUndo();
        var hide = shapes.Any(s => !s.Hidden);
        foreach (var shape in shapes)
        {
            shape.Hidden = hide;
        }

        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    /// <summary>
    /// Remembers the drawing before a change. Changes with the same <paramref name="coalesceKey"/>
    /// less than a second apart (held arrow key, spinning a number box) share one undo step.
    /// </summary>
    private void SaveUndo(string? coalesceKey = null)
    {
        var now = DateTime.UtcNow;
        if (coalesceKey != null && coalesceKey == _lastUndoKey && now - _lastUndoTime < TimeSpan.FromSeconds(1) && _undoStack.Count > 0)
        {
            _lastUndoTime = now;
            return;
        }

        _lastUndoKey = coalesceKey;
        _lastUndoTime = now;
        _undoStack.Add(CaptureState());
        if (_undoStack.Count > MaxUndoSteps)
        {
            _undoStack.RemoveAt(0);
        }

        _redoStack.Clear();
        UpdateUndoState();
    }

    [RelayCommand(CanExecute = nameof(CanUndoOrRemovePoint))]
    private void Undo()
    {
        // While drawing, undo takes back the last click
        if (IsDrawing)
        {
            RemoveLastDrawnPoint();
            return;
        }

        if (_undoStack.Count == 0)
        {
            return;
        }

        _redoStack.Add(CaptureState());
        var state = _undoStack[^1];
        _undoStack.RemoveAt(_undoStack.Count - 1);
        RestoreState(state);
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        if (_redoStack.Count == 0)
        {
            return;
        }

        CancelDrawing();
        _undoStack.Add(CaptureState());
        var state = _redoStack[^1];
        _redoStack.RemoveAt(_redoStack.Count - 1);
        RestoreState(state);
    }

    private bool CanUndoOrRemovePoint() => CanUndo || IsDrawing;

    private void RemoveLastDrawnPoint()
    {
        var points = ActiveShape!.Points;
        var count = points.Count >= 4 && points[^1].DrawType == DrawCoordinateType.BezierCurve &&
                    points[^2].DrawType == DrawCoordinateType.BezierCurveSupport2
            ? 3
            : 1;
        points.RemoveRange(points.Count - count, count);
        if (points.Count == 0)
        {
            CancelDrawing();
        }

        Canvas?.InvalidateVisual();
        UpdateUndoState();
    }

    private UndoState CaptureState()
    {
        var state = new UndoState();
        foreach (var shape in Shapes)
        {
            state.Shapes.Add(shape.Clone());
        }

        // Selection by position, so it can be restored on the cloned shapes
        var item = SelectedTreeItem;
        var selectedShape = item?.Shape ?? item?.Point?.DrawShape;
        state.SelectedShapeIndex = selectedShape != null ? Shapes.IndexOf(selectedShape) : -1;
        state.SelectedPointIndex = item?.Point != null && selectedShape != null ? selectedShape.Points.IndexOf(item.Point) : -1;
        state.SelectedLayer = item?.IsLayer == true ? item.Layer : null;
        return state;
    }

    private void RestoreState(UndoState state)
    {
        ActivePoint = null;
        IsPointSelected = false;
        SelectedShapes = [];
        if (ActiveShape != null && Shapes.Contains(ActiveShape))
        {
            ActiveShape = null;
        }

        // Same list instance - the canvas holds on to it
        Shapes.Clear();
        foreach (var shape in state.Shapes)
        {
            Shapes.Add(shape.Clone());
        }

        if (SelectedTreeItem != null)
        {
            SelectedTreeItem = null;
        }

        RefreshTreeView();

        if (state.SelectedShapeIndex >= 0 && state.SelectedShapeIndex < Shapes.Count)
        {
            var shape = Shapes[state.SelectedShapeIndex];
            if (state.SelectedPointIndex >= 0 && state.SelectedPointIndex < shape.Points.Count)
            {
                SelectPoint(shape.Points[state.SelectedPointIndex]);
            }
            else
            {
                SelectShape(shape);
            }
        }
        else if (state.SelectedLayer.HasValue)
        {
            var layerItem = FindTreeItem(null, null, state.SelectedLayer.Value);
            if (layerItem != null)
            {
                SelectedTreeItem = layerItem;
            }
        }

        _lastUndoKey = null;
        Canvas?.InvalidateVisual();
        UpdateUndoState();
    }

    private void UpdateUndoState()
    {
        CanUndo = _undoStack.Count > 0;
        CanRedo = _redoStack.Count > 0;
        UndoCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void SelectAllShapes()
    {
        SelectedShapes = Shapes.ToList();
        Canvas?.InvalidateVisual();
    }

    public void OnClosing()
    {
        UiUtil.SaveWindowPosition(Window);

        _previewTimer?.Stop();
        if (Canvas != null)
        {
            (Canvas.PreviewImage as IDisposable)?.Dispose();
            (Canvas.BackgroundImage as IDisposable)?.Dispose();
            Canvas.PreviewImage = null;
            Canvas.BackgroundImage = null;
            Canvas = null;
        }
    }
}

internal sealed class UndoState
{
    public List<DrawShape> Shapes { get; } = [];
    public int SelectedShapeIndex { get; set; } = -1;
    public int SelectedPointIndex { get; set; } = -1;
    public int? SelectedLayer { get; set; }
}

/// <summary>
/// Represents an item in the shape tree view.
/// </summary>
public partial class ShapeTreeItem : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private ObservableCollection<ShapeTreeItem> _children = [];
    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private string _iconName = string.Empty;
    [ObservableProperty] private IBrush? _swatch;
    [ObservableProperty] private bool _isHidden;
    [ObservableProperty] private string _meta = string.Empty;

    public bool IsLayer { get; set; }
    public int Layer { get; set; }
    public DrawShape? Shape { get; set; }
    public DrawCoordinate? Point { get; set; }

    public bool HasSwatch => IsLayer;
    public bool CanToggleVisibility => IsLayer || Shape != null;

    partial void OnIsExpandedChanged(bool value)
    {
        // Remembered on the shape, so a tree rebuild keeps its points open or closed
        if (Shape != null)
        {
            Shape.Expanded = value;
        }
    }

    public override string ToString() => Name;
}