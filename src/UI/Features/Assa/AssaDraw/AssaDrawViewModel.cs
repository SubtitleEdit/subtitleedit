using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PickLayer;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

public partial class AssaDrawViewModel : ObservableObject
{
    public Window? Window { get; set; }
    public AssaDrawCanvas? Canvas { get; set; }

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
    [ObservableProperty] private bool _showGrid = true;
    [ObservableProperty] private ObservableCollection<ShapeTreeItem> _shapeTreeItems = [];
    [ObservableProperty] private ShapeTreeItem? _selectedTreeItem;
    [ObservableProperty] private List<DrawShape> _selectedShapes = [];

    public Subtitle ResultSubtitle { get; set; } = new Subtitle();

    private float _currentX = float.MinValue;
    private float _currentY = float.MinValue;
    private readonly Regex _regexStart = new(@"\{[^{]*\\p1[^}]*\}");
    private readonly Regex _regexEnd = new(@"\{[^{]*\\p0[^}]*\}");
    private readonly IFileHelper _fileHelper;
    private string _fileName = string.Empty;
    private Subtitle? _subtitle;

    public AssaDrawViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
    }

    public void Initialize()
    {
        UiUtil.RestoreWindowPosition(Window);
        ZoomToFitCurrentVideoResolution();
        RefreshTreeView();
        Canvas?.InvalidateVisual();
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
            if (Canvas == null || Canvas.Bounds.Width < 1 || Canvas.Bounds.Height < 1)
            {
                return;
            }

            // Calculate zoom factor to fit the entire canvas (video resolution) in the visible area
            var availableWidth = Canvas.Bounds.Width;
            var availableHeight = Canvas.Bounds.Height;

            // Add some padding (e.g., 20 pixels on each side)
            var padding = 40.0;
            availableWidth -= padding;
            availableHeight -= padding;

            // Calculate zoom factors for width and height
            var zoomX = (float)(availableWidth / CanvasWidth);
            var zoomY = (float)(availableHeight / CanvasHeight);

            // Use the smaller zoom factor to ensure the entire canvas fits
            var newZoomFactor = Math.Min(zoomX, zoomY);

            // Clamp the zoom factor to reasonable bounds
            newZoomFactor = Math.Clamp(newZoomFactor, 0.1f, 10f);

            // Set the zoom factor
            Canvas.ZoomFactor = newZoomFactor;
            UpdateZoomText();
        }, DispatcherPriority.Background);
    }

    public void Initialize(Subtitle subtitle, List<SubtitleLineViewModel> selectedLines, int? width, int? height)
    {
        _subtitle = subtitle;
        if (width.HasValue &&  height.HasValue && width.Value >= 0 && height.Value >= 0)
        {
            CanvasWidth = width.Value;
            CanvasHeight = height.Value;
        }
        
        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(subtitle.Header);
        foreach (var line in selectedLines)
        {
            if (line.Text.Contains("{\\p1}") && line.Text.Contains("{\\p0}"))
            {
                var style = styles.FirstOrDefault(s => s.Name.Equals(line.Style, StringComparison.OrdinalIgnoreCase));
                if (style == null)
                {
                    style = new SsaStyle();
                }

                ImportAssaDrawingFromText(line.Text, line.Layer, style.Primary.ToAvaloniaColor(), false);
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

        Canvas.CanvasClicked += OnCanvasClicked;
        Canvas.CanvasMouseMoved += OnCanvasMouseMoved;
        Canvas.PointDragged += OnPointDragged;
        Canvas.ZoomChanged += OnZoomChanged;
    }

    private void OnZoomChanged(object? sender, float zoomFactor)
    {
        ZoomText = $"Zoom: {zoomFactor * 100:0}%";
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
        ActiveShape = new DrawShape();

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

    private void OnPointDragged(object? sender, DrawCoordinate point)
    {
        PointX = point.X;
        PointY = point.Y;
        RefreshTreeView();
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
        if (ActiveShape == null)
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
        if (ActiveShape != null)
        {
            Shapes.Remove(ActiveShape);
            ActiveShape = null;
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
        await pickLayerWindow.ShowDialog(Window);

        if (!vm.OkPressed || vm.Layer == currentLayer)
        {
            return;
        }

        var newLayer = vm.Layer;

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
    private void ClearAll()
    {
        Shapes.Clear();
        ActiveShape = null;
        ActivePoint = null;
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
        Canvas?.ResetView();
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
    private async Task CopyToClipboard()
    {
        var code = GenerateAssaCode();
        if (!string.IsNullOrEmpty(code) && Window?.Clipboard != null)
        {
            await ClipboardHelper.SetTextAsync(Window, code);
        }
    }

    private Subtitle GenerateSubtitle()
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

        // Update resolution in header
        subtitle.Header = subtitle.Header.Replace("PlayResX: 384", $"PlayResX: {CanvasWidth}");
        subtitle.Header = subtitle.Header.Replace("PlayResY: 288", $"PlayResY: {CanvasHeight}");

        // Collect unique colors from all layers
        var colorToStyleName = new Dictionary<Color, string>();
        var layers = Shapes.Where(s => !s.Hidden).GroupBy(s => s.Layer).OrderBy(g => g.Key).ToList();
        
        foreach (var layer in layers)
        {
            var firstShape = layer.FirstOrDefault(p => !p.IsEraser);
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
        foreach (var layer in layers)
        {
            var sbDraw = new StringBuilder();
            var sbErase = new StringBuilder();
            var firstShape = layer.FirstOrDefault(p => !p.IsEraser);

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
                var finalText = new StringBuilder();

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

                if (finalText.Length > 0 && firstShape != null)
                {
                    var styleName = colorToStyleName.GetValueOrDefault(firstShape.ForeColor, "Default");
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

    private static string GetColorName(Color color)
    {
        // Create a readable color name based on RGB values
        if (color is { R: 255, G: 255, B: 255 }) return "White";
        if (color.R == 0 && color is { G: 0, B: 0 }) return "Black";
        if (color is { R: 255, G: 0, B: 0 }) return "Red";
        if (color is { R: 0, G: 255, B: 0 }) return "Green";
        if (color is { R: 0, G: 0, B: 255 }) return "Blue";
        if (color is { R: 255, G: 255, B: 0 }) return "Yellow";
        if (color is { R: 255, G: 0, B: 255 }) return "Magenta";
        if (color is { R: 0, G: 255, B: 255 }) return "Cyan";
        if (color is { R: 255, G: 165, B: 0 }) return "Orange";
        
        // For other colors, use hex representation
        return $"{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private string GenerateAssaCode()
    {
        if (Shapes.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var shape in Shapes.Where(s => !s.IsEraser && !s.Hidden))
        {
            sb.Append(shape.ToAssa());
            sb.Append(' ');
        }

        var drawCode = sb.ToString().Trim();
        if (string.IsNullOrEmpty(drawCode))
        {
            return string.Empty;
        }

        return $"{{\\p1}}{drawCode}{{\\p0}}";
    }

    [RelayCommand]
    private void Ok()
    {
        AssaDrawingCode = GenerateAssaCode();
        ResultSubtitle = GenerateSubtitle();
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
        ShapeTreeItems.Clear();

        var layers = Shapes.GroupBy(s => s.Layer).OrderBy(g => g.Key);
        foreach (var layer in layers)
        {
            var layerItem = new ShapeTreeItem
            {
                Name = $"Layer {layer.Key}",
                IsLayer = true,
                Layer = layer.Key
            };

            foreach (var shape in layer)
            {
                var shapeItem = new ShapeTreeItem
                {
                    Name = $"Shape ({(shape.IsEraser ? "erase" : "draw")})",
                    Shape = shape
                };

                foreach (var point in shape.Points)
                {
                    shapeItem.Children.Add(new ShapeTreeItem
                    {
                        Name = point.GetText(point.X, point.Y),
                        Point = point
                    });
                }

                layerItem.Children.Add(shapeItem);
            }

            ShapeTreeItems.Add(layerItem);
        }
    }

    private void ImportAssaDrawingFromText(string text, int layer, Color color, bool isEraser)
    {
        text = _regexStart.Replace(text, string.Empty);
        text = _regexEnd.Replace(text, string.Empty);
        var arr = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        var i = 0;
        var bezierCount = 0;
        var state = DrawCoordinateType.None;
        DrawCoordinate? moveCoordinate = null;
        DrawShape? drawShape = null;

        while (i < arr.Length)
        {
            var v = arr[i];

            if (v == "m" && i < arr.Length - 2 &&
                float.TryParse(arr[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var mX) &&
                float.TryParse(arr[i + 2], NumberStyles.Float, CultureInfo.InvariantCulture, out var mY))
            {
                bezierCount = 0;
                moveCoordinate = new DrawCoordinate(null, DrawCoordinateType.Move, mX, mY, DrawSettings.PointColor);
                state = DrawCoordinateType.Move;
                i += 2;
            }
            else if (v == "l")
            {
                state = DrawCoordinateType.Line;
                bezierCount = 0;
                if (moveCoordinate != null)
                {
                    drawShape = new DrawShape { Layer = layer, ForeColor = color, IsEraser = isEraser };
                    drawShape.AddPoint(DrawCoordinateType.Line, moveCoordinate.X, moveCoordinate.Y, DrawSettings.PointColor);
                    moveCoordinate = null;
                    Shapes.Add(drawShape);
                }
            }
            else if (v == "b")
            {
                state = DrawCoordinateType.BezierCurve;
                if (moveCoordinate != null)
                {
                    drawShape = new DrawShape { Layer = layer, ForeColor = color, IsEraser = isEraser };
                    drawShape.AddPoint(DrawCoordinateType.BezierCurve, moveCoordinate.X, moveCoordinate.Y, DrawSettings.PointColor);
                    moveCoordinate = null;
                    Shapes.Add(drawShape);
                }
                bezierCount = 1;
            }
            else if (state == DrawCoordinateType.Line && drawShape != null && i < arr.Length - 1 &&
                float.TryParse(arr[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var lX) &&
                float.TryParse(arr[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lY))
            {
                drawShape.AddPoint(DrawCoordinateType.Line, lX, lY, DrawSettings.PointColor);
                i++;
            }
            else if (state == DrawCoordinateType.BezierCurve && drawShape != null && i < arr.Length - 1 &&
                float.TryParse(arr[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var bX) &&
                float.TryParse(arr[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var bY))
            {
                bezierCount++;
                if (bezierCount > 3)
                {
                    bezierCount = 1;
                }

                var pointType = bezierCount switch
                {
                    2 => DrawCoordinateType.BezierCurveSupport1,
                    3 => DrawCoordinateType.BezierCurveSupport2,
                    _ => DrawCoordinateType.BezierCurve
                };

                var pointColor = bezierCount is 2 or 3 ? DrawSettings.PointHelperColor : DrawSettings.PointColor;
                drawShape.AddPoint(pointType, bX, bY, pointColor);
                i++;
            }

            i++;
        }
    }

    private void LoadFromText(string text)
    {
        ClearAll();

        var subtitle = new Subtitle();
        var format = new AdvancedSubStationAlpha();
        format.LoadSubtitle(subtitle, text.SplitToLines(), _fileName);

        // Read resolution from header
        var playResX = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", subtitle.Header);
        if (int.TryParse(playResX, out var width) && width >= 125 && width <= 4096)
        {
            CanvasWidth = width;
        }

        var playResY = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", subtitle.Header);
        if (int.TryParse(playResY, out var height) && height >= 125 && height <= 4096)
        {
            CanvasHeight = height;
        }

        // Get styles from header
        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(subtitle.Header);

        // Import drawing codes from paragraphs
        foreach (var paragraph in subtitle.Paragraphs)
        {
            var color = Colors.White;
            
            // Try to get style from paragraph.Extra
            if (!string.IsNullOrEmpty(paragraph.Extra))
            {
                var style = styles.FirstOrDefault(s => s.Name.Equals(paragraph.Extra, StringComparison.OrdinalIgnoreCase));
                if (style != null)
                {
                    color = style.Primary.ToAvaloniaColor();
                }
            }
            
            ImportAssaDrawingFromText(paragraph.Text, paragraph.Layer, color, false);
        }

        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    internal async void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (ActiveShape != null && !Shapes.Contains(ActiveShape))
            {
                // Cancel current drawing
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
        else if (e.Key == Key.Delete && ActiveShape != null)
        {
            DeleteShape();
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
            }
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
    }

    private void AdjustPosition(float xAdjust, float yAdjust)
    {
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

    partial void OnPointXChanged(float value)
    {
        if (ActivePoint != null && Math.Abs(ActivePoint.X - value) > 0.001f)
        {
            ActivePoint.X = value;
            UpdateSelectedPointName();
            Canvas?.InvalidateVisual();
        }
    }

    partial void OnPointYChanged(float value)
    {
        if (ActivePoint != null && Math.Abs(ActivePoint.Y - value) > 0.001f)
        {
            ActivePoint.Y = value;
            UpdateSelectedPointName();
            Canvas?.InvalidateVisual();
        }
    }

    partial void OnShapeIsEraserChanged(bool value)
    {
        if (ActiveShape != null && ActiveShape.IsEraser != value)
        {
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
        // Update layer selection state
        IsLayerSelected = value?.IsLayer == true;
        IsShapeSelected = value?.Shape != null;
        
        if (value?.IsLayer == true)
        {
            // Get color from first shape in this layer
            var firstShape = Shapes.FirstOrDefault(s => s.Layer == value.Layer);
            if (firstShape != null)
            {
                LayerColor = firstShape.ForeColor;
            }
        }

        if (Canvas != null)
        {
            Canvas.SelectedShape = value?.Shape;
            Canvas.InvalidateVisual();
        }
    }

    partial void OnLayerColorChanged(Color value)
    {
        if (SelectedTreeItem?.IsLayer == true)
        {
            // Update all shapes in the selected layer
            foreach (var shape in Shapes.Where(s => s.Layer == SelectedTreeItem.Layer))
            {
                shape.ForeColor = value;
            }
            Canvas?.InvalidateVisual();
        }
    }

    [RelayCommand]
    private void SelectAllShapes()
    {
        SelectedShapes = Shapes.ToList();
        if (Canvas != null)
        {
            Canvas.SelectedShapes = SelectedShapes;
        }
        Canvas?.InvalidateVisual();
    }

    public void OnClosing()
    {
        UiUtil.SaveWindowPosition(Window);
    }
}

/// <summary>
/// Represents an item in the shape tree view.
/// </summary>
public partial class ShapeTreeItem : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private ObservableCollection<ShapeTreeItem> _children = [];

    public bool IsLayer { get; set; }
    public int Layer { get; set; }
    public DrawShape? Shape { get; set; }
    public DrawCoordinate? Point { get; set; }

    public override string ToString() => Name;
}