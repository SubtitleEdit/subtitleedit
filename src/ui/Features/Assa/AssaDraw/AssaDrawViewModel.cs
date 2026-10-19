using Avalonia;
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
    [ObservableProperty] private string _positionText = string.Format(Se.Language.Assa.DrawPositionXY, 0, 0);
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
    [ObservableProperty] private float _shapeOutlineWidth;
    [ObservableProperty] private Color _shapeOutlineColor = Colors.Black;
    [ObservableProperty] private bool _isTextBoxSelected;
    [ObservableProperty] private bool _isDrawShapeSelected;
    [ObservableProperty] private bool _canEditText;
    [ObservableProperty] private bool _canEditOutline;
    [ObservableProperty] private string _shapeText = string.Empty;
    [ObservableProperty] private string _textFontName = "Arial";
    [ObservableProperty] private float _textFontSize = 48;
    [ObservableProperty] private bool _textBold;
    [ObservableProperty] private Color _textColor = Colors.Black;
    [ObservableProperty] private bool _textShrinkToFit = true;
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

    /// <summary>
    /// A text box or bubble was just placed: the window puts the cursor in the text field.
    /// </summary>
    public event EventHandler? TextEditRequested;

    private List<string>? _fontNames;

    /// <summary>
    /// Fonts by the names libass knows them by.
    /// </summary>
    public List<string> FontNames
    {
        get
        {
            if (_fontNames == null)
            {
                try
                {
                    _fontNames = FontHelper.GetLibAssaFonts().Distinct().OrderBy(f => f).ToList();
                }
                catch (Exception exception)
                {
                    Se.LogError(exception, "ASSA draw: could not list fonts");
                    _fontNames = [];
                }

                if (!_fontNames.Contains("Arial"))
                {
                    _fontNames.Insert(0, "Arial");
                }
            }

            return _fontNames;
        }
    }

    private float _currentX = float.MinValue;
    private float _currentY = float.MinValue;
    private readonly Regex _regexStart = new(@"\{[^{]*\\p1[^}]*\}");
    private readonly Regex _regexEnd = new(@"\{[^{]*\\p0[^}]*\}");
    private readonly Regex _regexIclip = new(@"\\iclip\(([^)]+)\)");
    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private string _fileName = string.Empty;
    private string? _openedState;
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
        _openedState = GetChangeState();
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
        Dispatcher.UIThread.Post(() => Canvas?.FitToView(), DispatcherPriority.Background);
    }

    public void Initialize(Subtitle subtitle, List<SubtitleLineViewModel> selectedLines, int? width, int? height,
        string? videoFileName = null, double? videoPositionSeconds = null)
    {
        _subtitle = subtitle;
        _videoFileName = videoFileName;
        // The main window writes the result into the selected drawing rows, which keep their own
        // times, so the animation must be timed by the first drawing row - a 2 s dialogue line
        // selected above a 5 s drawing made the out-animation fire after 2 s. Without a drawing
        // row the new rows get the times of the last selected line.
        var timingLine = selectedLines.FirstOrDefault(p => MainViewModel.IsAssaDrawingText(p.Text)) ??
                         selectedLines.LastOrDefault();
        if (timingLine != null && timingLine.Duration.TotalMilliseconds >= 1)
        {
            LineDurationMs = (int)Math.Round(timingLine.Duration.TotalMilliseconds);
        }

        // Same frame Set position uses: the paused player position when it is inside the line
        _videoSeconds = timingLine != null
            ? AssaSetPositionViewModel.GetScreenshotSeconds(timingLine, videoPositionSeconds)
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
            ImportLine(line.Text, line.Layer, style.Primary.ToAvaloniaColor());
        }

        AttachTextLines();
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
        Canvas.ShapeToggled += OnShapeToggled;
        Canvas.MarqueeSelected += OnMarqueeSelected;
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

    [RelayCommand]
    private void TextTool() => SetTool(DrawingTool.Text);

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
        var existing = Shapes.FirstOrDefault(s => s.ForeColor == color && !s.IsEraser && !s.IsTextBox);
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

    private void OnInsertRequested(object? sender, CanvasInsertEventArgs e)
    {
        if (CurrentTool == DrawingTool.Text)
        {
            InsertTextBox(e);
        }
        else
        {
            InsertLibraryShape(e);
        }
    }

    /// <summary>
    /// Text tool: a text box on a new layer above the drawing, in the current color with a black
    /// outline like a subtitle, selected with its text ready to be typed over.
    /// </summary>
    public DrawShape InsertTextBox(CanvasInsertEventArgs e)
    {
        var x = e.X;
        var y = e.Y;
        var width = e.Width;
        var height = e.Height;
        if (e.IsClick)
        {
            width = CanvasWidth * 0.4f;
            height = CanvasHeight * 0.12f;
            x -= width / 2f;
            y -= height / 2f;
        }

        CancelDrawing();
        SaveUndo();
        var layer = Shapes.Count == 0 ? 0 : Shapes.Max(s => s.Layer) + 1;
        var shape = MakeRectangle(x, y, width, height, layer, LayerColor);
        shape.IsTextBox = true;
        shape.SetTextBounds(x, y, x + width, y + height);
        shape.Text = Se.Language.Assa.DrawDefaultText;
        shape.TextColor = LayerColor;
        shape.FontSize = DefaultFontSize(16f);
        shape.OutlineWidth = 2;
        shape.OutlineColor = Colors.Black;
        Shapes.Add(shape);
        SetTool(DrawingTool.Select);
        RefreshTreeView();
        SelectShape(shape);
        Canvas?.InvalidateVisual();
        TextEditRequested?.Invoke(this, EventArgs.Empty);
        return shape;
    }

    /// <summary>
    /// A font size that suits the frame: a fraction of its height (54 for 1080p with 20).
    /// </summary>
    private float DefaultFontSize(float linesPerFrame) => MathF.Max(AssaTextLayout.MinFontSize, MathF.Round(CanvasHeight / linesPerFrame));

    /// <summary>
    /// Black text on light shapes, white on dark ones.
    /// </summary>
    private static Color ContrastColor(Color color)
    {
        var luminance = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
        return luminance > 140 ? Colors.Black : Colors.White;
    }

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

        // A multi-part shape (clip art, a ring) is one group: a click picks all of it
        var groupId = shapes.Count > 1 ? NextGroupId() : 0;
        foreach (var shape in shapes)
        {
            shape.TextColor = ContrastColor(shape.ForeColor);
            shape.FontSize = DefaultFontSize(20f);
            shape.GroupId = groupId;
            shape.Animation = item.AnimatedColor is not { } animated || shape.ForeColor == animated ? item.Animation?.Clone() : null;
        }

        Shapes.AddRange(shapes);
        SetTool(DrawingTool.Select);
        RefreshTreeView();
        if (shapes.Count == 1)
        {
            SelectShape(shapes[0]);
            if (item.TextArea != null)
            {
                TextEditRequested?.Invoke(this, EventArgs.Empty);
            }
        }
        else
        {
            SelectGroup(groupId);
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

    private void OnShapeClicked(object? sender, ShapeClickEventArgs e)
    {
        var shape = e.Shape;
        if (shape == null)
        {
            ClearSelection();
            return;
        }

        if (e.SelectPart)
        {
            // Alt+click / double-click: the single part inside its group, for point editing
            SelectShape(shape);
            return;
        }

        // A click on the group that is already selected keeps it
        var members = GetGroupMembers(shape);
        if (members.Count > 1 && SelectedShapes.Count == members.Count && members.All(SelectedShapes.Contains))
        {
            return;
        }

        SelectShapeOrGroup(shape);
    }

    private void OnShapeToggled(object? sender, ShapeClickEventArgs e)
    {
        if (e.Shape != null)
        {
            ToggleShapeSelection(e.Shape, e.SelectPart);
        }
    }

    private void OnMarqueeSelected(object? sender, CanvasMarqueeEventArgs e)
    {
        SelectInRectangle(e.Left, e.Top, e.Right, e.Bottom, e.Additive, e.SelectParts);
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
        else if (e.Shape != null && !(SelectedShapes.Count > 1 && SelectedShapes.Contains(e.Shape)))
        {
            // A right-click inside the selection keeps it, so the menu acts on all of it
            SelectShapeOrGroup(e.Shape);
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

    /// <summary>
    /// The shapes of the shape's group (in drawing order), or just the shape when it has none.
    /// </summary>
    public List<DrawShape> GetGroupMembers(DrawShape shape)
    {
        return shape.GroupId == 0 ? [shape] : Shapes.Where(s => s.GroupId == shape.GroupId).ToList();
    }

    private int NextGroupId() => Shapes.Count == 0 ? 1 : Shapes.Max(s => s.GroupId) + 1;

    /// <summary>
    /// The shapes plus every other shape of their groups, in drawing order.
    /// </summary>
    private List<DrawShape> ExpandToGroups(IEnumerable<DrawShape> shapes)
    {
        var set = new HashSet<DrawShape>(shapes);
        var groupIds = set.Where(s => s.GroupId != 0).Select(s => s.GroupId).ToHashSet();
        return Shapes.Where(s => set.Contains(s) || (s.GroupId != 0 && groupIds.Contains(s.GroupId))).ToList();
    }

    /// <summary>
    /// A canvas click: the whole group of a grouped shape, else the shape.
    /// </summary>
    public void SelectShapeOrGroup(DrawShape shape)
    {
        if (shape.GroupId != 0 && Shapes.Count(s => s.GroupId == shape.GroupId) > 1)
        {
            SelectGroup(shape.GroupId);
        }
        else
        {
            SelectShape(shape);
        }
    }

    /// <summary>
    /// Selects the group's tree node, which selects all of its shapes.
    /// </summary>
    public void SelectGroup(int groupId)
    {
        var item = FindGroupTreeItem(groupId);
        if (item != null)
        {
            SelectedTreeItem = item;
        }
    }

    /// <summary>
    /// The shapes in the selection: the multi-selection, else the single target shape.
    /// </summary>
    private List<DrawShape> GetSelectedShapeList()
    {
        if (SelectedShapes.Count > 0)
        {
            return SelectedShapes.Where(Shapes.Contains).ToList();
        }

        return TargetShape is { } target && SelectedTreeItem?.IsLayer != true ? [target] : [];
    }

    /// <summary>
    /// Makes these shapes the selection: none clears it, one selects it (as a part), exactly one
    /// group's shapes select that group, anything else is a multi-selection.
    /// </summary>
    public void SetSelection(IEnumerable<DrawShape> shapes)
    {
        var set = new HashSet<DrawShape>(shapes);
        var list = Shapes.Where(set.Contains).ToList();
        if (list.Count == 0)
        {
            ClearSelection();
            UpdateSelectionInfo();
            NotifyCommandStates();
            return;
        }

        if (list.Count == 1)
        {
            if (SelectedTreeItem?.Shape == list[0])
            {
                ApplyTreeSelection(SelectedTreeItem);
            }
            else
            {
                SelectShape(list[0]);
            }

            return;
        }

        var groupId = list[0].GroupId;
        if (groupId != 0 && list.All(s => s.GroupId == groupId) && Shapes.Count(s => s.GroupId == groupId) == list.Count)
        {
            SelectGroup(groupId);
            return;
        }

        ClearSelection();
        SelectedShapes = list;
        UpdateSelectionInfo();
    }

    /// <summary>
    /// Ctrl+click: adds the shape (or its whole group) to the selection, or takes it out.
    /// </summary>
    public void ToggleShapeSelection(DrawShape shape, bool partOnly = false)
    {
        var current = GetSelectedShapeList();
        var toggled = partOnly ? [shape] : GetGroupMembers(shape);
        if (toggled.All(current.Contains))
        {
            SetSelection(current.Where(s => !toggled.Contains(s)));
        }
        else
        {
            SetSelection(current.Concat(toggled));
        }
    }

    /// <summary>
    /// Rubber-band selection: every visible shape whose bounds touch the rectangle (with the rest
    /// of their groups unless <paramref name="partsOnly"/>), added to the selection when <paramref name="additive"/>.
    /// </summary>
    public void SelectInRectangle(float left, float top, float right, float bottom, bool additive, bool partsOnly = false)
    {
        var hits = Shapes.Where(s =>
        {
            if (s.Hidden || s.Points.Count == 0)
            {
                return false;
            }

            var b = s.GetBounds();
            return b.Left <= right && b.Right >= left && b.Top <= bottom && b.Bottom >= top;
        }).ToList();

        if (!partsOnly)
        {
            hits = ExpandToGroups(hits);
        }

        SetSelection(additive ? GetSelectedShapeList().Concat(hits) : hits);
    }

    /// <summary>
    /// Ctrl/Shift+click in the shapes tree: the shapes of the picked shape, group (and layer)
    /// nodes become the canvas selection. The tree keeps showing its own selection.
    /// </summary>
    public void SetTreeMultiSelection(IEnumerable<ShapeTreeItem> items)
    {
        var shapes = new HashSet<DrawShape>();
        foreach (var item in items)
        {
            if (item.IsGroup)
            {
                shapes.UnionWith(Shapes.Where(s => s.GroupId == item.GroupId));
            }
            else if (item.Shape != null)
            {
                shapes.Add(item.Shape);
            }
            else if (item.Point?.DrawShape is { } pointShape)
            {
                shapes.Add(pointShape);
            }
            else if (item.IsLayer)
            {
                shapes.UnionWith(Shapes.Where(s => s.Layer == item.Layer));
            }
        }

        var list = Shapes.Where(shapes.Contains).ToList();
        if (list.Count < 2)
        {
            return;
        }

        ActivePoint = null;
        IsPointSelected = false;
        if (ActiveShape != null && Shapes.Contains(ActiveShape))
        {
            ActiveShape = null;
        }

        IsShapeSelected = false;
        IsLayerSelected = false;
        SelectedShapes = list;
        if (Canvas != null)
        {
            Canvas.SelectedShape = null;
            Canvas.InvalidateVisual();
        }

        UpdateSelectionInfo();
    }

    /// <summary>
    /// The tree is down to one picked row again. When that row was already the current item (so
    /// no selection change came through the binding) a tree multi-selection still has to end.
    /// </summary>
    public void OnTreeSingleSelection(ShapeTreeItem item)
    {
        if (ReferenceEquals(SelectedTreeItem, item) && !item.IsGroup && SelectedShapes.Count > 0)
        {
            ApplyTreeSelection(item);
        }
    }

    private bool CanGroupShapes()
    {
        var targets = GetCommandTargets();
        if (targets.Count < 2)
        {
            return false;
        }

        // Already exactly one group: nothing to do
        var groupId = targets[0].GroupId;
        return groupId == 0 || targets.Any(s => s.GroupId != groupId) || Shapes.Count(s => s.GroupId == groupId) != targets.Count;
    }

    /// <summary>
    /// Ctrl+G: the selected shapes become one group (shapes of other groups leave those - groups don't nest).
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGroupShapes))]
    private void GroupShapes()
    {
        if (!CanGroupShapes())
        {
            return;
        }

        var targets = GetCommandTargets();
        SaveUndo();
        var groupId = NextGroupId();
        foreach (var shape in targets)
        {
            shape.GroupId = groupId;
        }

        RefreshTreeView();
        SelectGroup(groupId);
        Canvas?.InvalidateVisual();
    }

    private bool CanUngroupShapes() => GetCommandTargets().Any(s => s.GroupId != 0);

    /// <summary>
    /// Ctrl+Shift+G: the selected shapes leave their groups and stay selected.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUngroupShapes))]
    private void UngroupShapes()
    {
        var targets = GetCommandTargets();
        if (!targets.Any(s => s.GroupId != 0))
        {
            return;
        }

        SaveUndo();
        foreach (var shape in targets)
        {
            shape.GroupId = 0;
        }

        RefreshTreeView();
        SetSelection(targets);
        Canvas?.InvalidateVisual();
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
                ShapeText = shape.Text;
                TextFontName = shape.FontName;
                TextFontSize = shape.FontSize;
                TextBold = shape.FontBold;
                TextColor = shape.TextColor;
                TextShrinkToFit = shape.ShrinkToFit;
                if (!FontNames.Contains(shape.FontName))
                {
                    FontNames.Add(shape.FontName);
                }
            }

            var outlineShape = GetOutlineShapes().FirstOrDefault();
            if (outlineShape != null)
            {
                ShapeOutlineWidth = outlineShape.OutlineWidth;
                ShapeOutlineColor = outlineShape.OutlineColor;
            }
        }
        finally
        {
            _updatingShapeFields = false;
        }

        IsTextBoxSelected = shape?.IsTextBox == true;
        IsDrawShapeSelected = shape != null && !shape.IsTextBox;
        CanEditText = shape != null && !shape.IsEraser;
        CanEditOutline = GetOutlineShapes().Count > 0;

        var item = SelectedTreeItem;
        SelectionText = item?.IsGroup == true
            ? $"{item.Name} · {string.Format(Se.Language.Assa.DrawXShapes, SelectedShapes.Count)}"
            : SelectedShapes.Count > 1
            ? string.Format(Se.Language.Assa.DrawXShapes, SelectedShapes.Count)
            : item?.Point != null
                ? item.Name.Trim()
                : shape != null
                    ? $"{string.Format(Se.Language.Assa.DrawLayerX, shape.Layer)} · {string.Format(Se.Language.Assa.DrawXPoints, shape.Points.Count)}"
                    : item?.IsLayer == true
                        ? item.Name
                        : string.Empty;

        RefreshAnimationFields();
        UpdateCodePreview();
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

        shape.TransformText((x, y) => (left + (x - left) * scaleX, top + (y - top) * scaleY), mirrored: false);

        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    /// <summary>
    /// What the outline fields change: a selected text box, or else the drawing shapes of the
    /// selected layer (or of the selected shape's layer) - the layer is one ASSA line with one outline.
    /// </summary>
    private List<DrawShape> GetOutlineShapes()
    {
        var shape = TargetShape;
        if (shape?.IsTextBox == true)
        {
            return [shape];
        }

        int? layer = SelectedTreeItem?.IsLayer == true ? SelectedTreeItem.Layer : shape?.Layer;
        return layer == null ? [] : Shapes.Where(s => s.Layer == layer && !s.IsEraser && !s.IsTextBox).ToList();
    }

    partial void OnShapeOutlineWidthChanged(float value) => ApplyOutline();

    partial void OnShapeOutlineColorChanged(Color value) => ApplyOutline();

    private void ApplyOutline()
    {
        if (_updatingShapeFields)
        {
            return;
        }

        var width = Math.Max(0, ShapeOutlineWidth);
        var color = ShapeOutlineColor;
        var shapes = GetOutlineShapes();
        if (!shapes.Any(s => Math.Abs(s.OutlineWidth - width) > 0.001f || s.OutlineColor != color))
        {
            return;
        }

        SaveUndo($"outline-{RuntimeHelpers.GetHashCode(shapes[0])}");
        foreach (var shape in shapes)
        {
            shape.OutlineWidth = width;
            shape.OutlineColor = color;
        }

        UpdateCodePreview();
        Canvas?.InvalidateVisual();
    }

    partial void OnShapeTextChanged(string value) => EditText("text", shape =>
    {
        // First text in a plain shape: pick a color that shows on it
        if (string.IsNullOrEmpty(shape.Text) && !shape.IsTextBox && shape.TextColor == Colors.Black)
        {
            shape.TextColor = ContrastColor(shape.ForeColor);
            SetTextColorField(shape.TextColor);
        }

        if (string.IsNullOrEmpty(shape.Text) && shape.FontSize == 48)
        {
            shape.FontSize = DefaultFontSize(20f);
            SetTextFontSizeField(shape.FontSize);
        }

        shape.Text = value ?? string.Empty;
    });

    partial void OnTextFontNameChanged(string value) => EditText("font", shape => shape.FontName = string.IsNullOrWhiteSpace(value) ? "Arial" : value);

    partial void OnTextFontSizeChanged(float value) => EditText("font-size", shape => shape.FontSize = Math.Clamp(value, AssaTextLayout.MinFontSize, 1000));

    partial void OnTextBoldChanged(bool value) => EditText("bold", shape => shape.FontBold = value);

    partial void OnTextColorChanged(Color value) => EditText("text-color", shape => shape.TextColor = value);

    partial void OnTextShrinkToFitChanged(bool value) => EditText("shrink", shape => shape.ShrinkToFit = value);

    private void EditText(string undoKey, Action<DrawShape> apply)
    {
        var shape = TargetShape;
        if (_updatingShapeFields || shape == null || shape.IsEraser)
        {
            return;
        }

        SaveUndo($"{undoKey}-{RuntimeHelpers.GetHashCode(shape)}");
        apply(shape);
        if (undoKey == "text")
        {
            UpdateShapeTreeName(shape);
        }

        Canvas?.InvalidateVisual();
    }

    private void SetTextColorField(Color color)
    {
        _updatingShapeFields = true;
        try
        {
            TextColor = color;
        }
        finally
        {
            _updatingShapeFields = false;
        }
    }

    private void SetTextFontSizeField(float size)
    {
        _updatingShapeFields = true;
        try
        {
            TextFontSize = size;
        }
        finally
        {
            _updatingShapeFields = false;
        }
    }

    private void UpdateCodePreview()
    {
        var code = GenerateAssaCode();
        CodePreview = code.Length > 400 ? code[..400] + "..." : code;
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
            NotifyCommandStates();
            Canvas?.InvalidateVisual();
            return;
        }

        // Start new shape
        StartNewShape(x, y);
        NotifyCommandStates();
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
        PositionText = string.Format(Se.Language.Assa.DrawPositionXY, e.X.ToString("0", CultureInfo.CurrentCulture), e.Y.ToString("0", CultureInfo.CurrentCulture));

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

        CloseShapeCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanCloseShape))]
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

    /// <summary>
    /// A shape is being drawn with enough points to finish it (a circle/rectangle needs its first
    /// corner only, the cursor gives the rest).
    /// </summary>
    private bool CanCloseShape()
    {
        if (!IsDrawing)
        {
            return false;
        }

        var isCircleOrRect = CurrentTool == DrawingTool.Circle || CurrentTool == DrawingTool.Rectangle;
        return ActiveShape!.Points.Count >= (isCircleOrRect ? 1 : 2);
    }

    /// <summary>
    /// The shapes the shape commands act on: a multi-selection (Ctrl+A, SVG import, inserted
    /// library shapes), else the single target shape.
    /// </summary>
    private List<DrawShape> GetCommandTargets()
    {
        if (SelectedShapes.Count > 1)
        {
            return SelectedShapes.Where(Shapes.Contains).ToList();
        }

        return TargetShape is { } target ? [target] : [];
    }

    private bool CanDeleteShape() => IsDrawing || GetCommandTargets().Count > 0;

    private bool CanDuplicateShape() => GetCommandTargets().Count > 0;

    private bool CanClearAll() => Shapes.Count > 0 || IsDrawing;

    /// <summary>
    /// The selection, the shape list or the drawing state changed - the toolbar buttons and menu
    /// items of these commands grey out when they can't run.
    /// </summary>
    private void NotifyCommandStates()
    {
        CloseShapeCommand.NotifyCanExecuteChanged();
        DeleteShapeCommand.NotifyCanExecuteChanged();
        DuplicateShapeCommand.NotifyCanExecuteChanged();
        ClearAllCommand.NotifyCanExecuteChanged();
        GroupShapesCommand.NotifyCanExecuteChanged();
        UngroupShapesCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Rebuilds the tree but keeps a multi-selection - restoring the tree selection would end it.
    /// </summary>
    private void RefreshTreeViewKeepingMultiSelection()
    {
        var multiSelection = SelectedShapes;
        RefreshTreeView();
        if (multiSelection.Count > 1)
        {
            SelectedShapes = multiSelection.Where(Shapes.Contains).ToList();
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

    [RelayCommand(CanExecute = nameof(CanDeleteShape))]
    private void DeleteShape()
    {
        var shapes = IsDrawing ? [ActiveShape!] : GetCommandTargets();
        if (shapes.Count > 0)
        {
            if (ActivePoint != null && shapes.Any(s => s.Points.Contains(ActivePoint)))
            {
                ActivePoint = null;
                IsPointSelected = false;
            }

            // One undo step for the whole selection
            if (shapes.Any(Shapes.Contains))
            {
                SaveUndo();
            }

            foreach (var shape in shapes)
            {
                Shapes.Remove(shape);
            }

            SelectedShapes = SelectedShapes.Where(s => !shapes.Contains(s)).ToList();
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

        // Get color and animation from existing shapes in the new layer (if any)
        var existingShapeInNewLayer = Shapes.FirstOrDefault(s => s.Layer == newLayer);
        var newColor = existingShapeInNewLayer?.ForeColor;
        var newAnimation = existingShapeInNewLayer != null ? GetLayerAnimation(newLayer) : null;

        if (SelectedTreeItem.IsLayer)
        {
            // Change layer for all shapes in the selected layer
            foreach (var shape in Shapes.Where(s => s.Layer == currentLayer).ToList())
            {
                shape.Layer = newLayer;
                if (newColor.HasValue)
                {
                    shape.ForeColor = newColor.Value;
                    shape.Animation = newAnimation?.Clone();
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
                SelectedTreeItem.Shape.Animation = newAnimation?.Clone();
            }
        }

        RefreshTreeView();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand(CanExecute = nameof(CanDuplicateShape))]
    private void DuplicateShape()
    {
        var shapes = GetCommandTargets();
        if (shapes.Count > 1)
        {
            DuplicateShapes(shapes);
            return;
        }

        var shape = shapes.FirstOrDefault();
        if (shape == null)
        {
            return;
        }

        SaveUndo();
        var copy = shape.Clone();
        copy.Hidden = false;

        // A part copied out of its group is a shape of its own
        copy.GroupId = 0;
        copy.Offset(DrawSettings.GridSize, DrawSettings.GridSize);
        Shapes.Insert(Shapes.IndexOf(shape) + 1, copy);
        RefreshTreeView();
        SelectShape(copy);
        Canvas?.InvalidateVisual();
    }

    /// <summary>
    /// Duplicates a multi-selection in one undo step; the copies become the selection.
    /// </summary>
    private void DuplicateShapes(List<DrawShape> shapes)
    {
        SaveUndo();

        // Copies of a group form a new group of their own - never one sharing the original's id
        var nextGroupId = NextGroupId();
        var newGroupIds = new Dictionary<int, int>();
        foreach (var groupId in shapes.Where(s => s.GroupId != 0).GroupBy(s => s.GroupId).Where(g => g.Count() > 1).Select(g => g.Key))
        {
            newGroupIds[groupId] = nextGroupId++;
        }

        var copies = new List<DrawShape>();
        foreach (var shape in shapes)
        {
            var copy = shape.Clone();
            copy.Hidden = false;
            copy.GroupId = newGroupIds.GetValueOrDefault(shape.GroupId);
            copy.Offset(DrawSettings.GridSize, DrawSettings.GridSize);
            Shapes.Insert(Shapes.IndexOf(shape) + 1, copy);
            copies.Add(copy);
        }

        RefreshTreeView();
        SetSelection(copies);
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void FlipShapeHorizontal()
    {
        var shapes = GetCommandTargets();
        if (shapes.Count == 0)
        {
            return;
        }

        SaveUndo();
        if (shapes.Count == 1)
        {
            shapes[0].FlipHorizontal();
        }
        else
        {
            // The selection is mirrored as a whole, like the transform handles do
            var (left, _, right, _) = DrawShape.GetBounds(shapes);
            foreach (var shape in shapes)
            {
                shape.FlipHorizontal(left, right);
            }
        }

        RefreshTreeViewKeepingMultiSelection();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void RotateShapeClockwise() => RotateShape(90);

    [RelayCommand]
    private void RotateShapeCounterClockwise() => RotateShape(-90);

    private void RotateShape(float degrees)
    {
        var shapes = GetCommandTargets();
        if (shapes.Count == 0)
        {
            return;
        }

        SaveUndo();
        if (shapes.Count == 1)
        {
            shapes[0].Rotate(degrees);
        }
        else
        {
            // The selection turns as a whole around its middle, like with the rotate handle
            var (left, top, right, bottom) = DrawShape.GetBounds(shapes);
            foreach (var shape in shapes)
            {
                shape.Rotate(degrees, (left + right) / 2f, (top + bottom) / 2f);
            }
        }

        RefreshTreeViewKeepingMultiSelection();
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private void FlipShapeVertical()
    {
        var shapes = GetCommandTargets();
        if (shapes.Count == 0)
        {
            return;
        }

        SaveUndo();
        if (shapes.Count == 1)
        {
            shapes[0].FlipVertical();
        }
        else
        {
            var (_, top, _, bottom) = DrawShape.GetBounds(shapes);
            foreach (var shape in shapes)
            {
                shape.FlipVertical(top, bottom);
            }
        }

        RefreshTreeViewKeepingMultiSelection();
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
        var shapes = GetCommandTargets().Where(s => s.Layer != layer).ToList();
        if (shapes.Count == 0)
        {
            return;
        }

        SaveUndo();

        // Same rule as Change layer: a shape joining a layer takes that layer's color and animation
        var existing = Shapes.FirstOrDefault(s => s.Layer == layer);
        var layerAnimation = existing != null ? GetLayerAnimation(layer) : null;
        foreach (var shape in shapes)
        {
            shape.Layer = layer;
            if (existing != null)
            {
                shape.ForeColor = existing.ForeColor;
                shape.Animation = layerAnimation?.Clone();
            }
        }

        RefreshTreeViewKeepingMultiSelection();
        Canvas?.InvalidateVisual();
    }

    /// <summary>
    /// An anchor point is selected - bezier control points can't be deleted on their own.
    /// </summary>
    private bool CanDeleteActivePoint()
    {
        var point = ActivePoint;
        var shape = point == null ? null : point.DrawShape ?? Shapes.FirstOrDefault(s => s.Points.Contains(point));
        return point != null && shape != null && point.DrawType is not (DrawCoordinateType.BezierCurveSupport1 or DrawCoordinateType.BezierCurveSupport2);
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

        // RemovePoint refuses a point that isn't in the shape - check first, so no empty undo step
        // is saved (it also cleared the redo stack)
        if (!shape.Points.Contains(point))
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

    [RelayCommand(CanExecute = nameof(CanClearAll))]
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
    }

    [RelayCommand]
    private void ZoomOut()
    {
        Canvas?.ZoomOut();
    }

    [RelayCommand]
    private void ResetView()
    {
        // Back to the opening view: whole frame fitted and centered (was 100% at the top-left
        // corner, which for a 1080p frame is mostly off screen).
        Canvas?.FitToView();
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
            Se.Language.General.Images, "*.png;*.jpg;*.jpeg;*.bmp;*.webp");
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

        // The still preview shows the drawing at rest - the canvas plays the animation
        var subtitle = GenerateSubtitle(withAnimation: false);
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
            Se.Language.Assa.DrawSaveDrawing);

        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        _fileName = fileName;

        try
        {
            await File.WriteAllTextAsync(fileName, GetAssaDrawFileText());
        }
        catch (Exception ex)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// The .assadraw file: hidden shapes are kept, as Comment lines, so a reload still has them (hidden),
    /// and groups are kept in the Effect field (libass ignores an Effect it doesn't know).
    /// </summary>
    internal string GetAssaDrawFileText()
    {
        var subtitle = GenerateSubtitle(hiddenAsComments: true, withGroups: true);
        return new AdvancedSubStationAlpha().ToText(subtitle, string.Empty);
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
            Se.Language.Assa.DrawOpenDrawing,
            Se.Language.Assa.DrawDrawingFiles,
            "*.assadraw",
            Se.Language.Assa.DrawAssFiles,
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

        // The imported picture is one group
        var groupId = result.Shapes.Count > 1 ? NextGroupId() : 0;
        foreach (var shape in result.Shapes)
        {
            shape.GroupId = groupId;
        }

        Shapes.AddRange(result.Shapes);
        RefreshTreeView();
        if (groupId != 0)
        {
            SelectGroup(groupId);
        }
        else
        {
            SelectedShapes = result.Shapes.ToList();
            UpdateSelectionInfo();
        }

        Canvas?.InvalidateVisual();
        return true;
    }

    private static readonly string[] BackgroundImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".webp"];

    /// <summary>
    /// A shape dragged from the shape palette (same process only).
    /// </summary>
    public static readonly DataFormat<ShapeLibraryItem> LibraryShapeFormat = DataFormat.CreateInProcessFormat<ShapeLibraryItem>("se-assa-draw-library-shape");

    internal void OnDragOver(object? sender, DragEventArgs e)
    {
        if (GetDraggedLibraryShape(e) != null)
        {
            e.DragEffects = GetCanvasDropPoint(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
            return;
        }

        e.DragEffects = GetDroppedFile(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    /// <summary>
    /// The palette shape being dragged: the in-process item, or (if the platform only hands back
    /// the text that went along) the library shape with that name.
    /// </summary>
    private static ShapeLibraryItem? GetDraggedLibraryShape(DragEventArgs e)
    {
        if (e.DataTransfer.Contains(LibraryShapeFormat) && e.DataTransfer.TryGetValue(LibraryShapeFormat) is { } item)
        {
            return item;
        }

        if (!e.DataTransfer.Contains(DataFormat.Text) || e.DataTransfer.TryGetText() is not { } name)
        {
            return null;
        }

        return ShapeLibrary.BuiltIn.Concat(ShapeLibrary.GetUserShapes()).FirstOrDefault(i => i.Name == name);
    }

    /// <summary>
    /// Where on the drawing a drag is, or null when it is not over the canvas.
    /// </summary>
    private (float X, float Y)? GetCanvasDropPoint(DragEventArgs e)
    {
        if (Canvas == null)
        {
            return null;
        }

        var position = e.GetPosition(Canvas);
        return new Rect(Canvas.Bounds.Size).Contains(position) ? Canvas.ToCanvasPoint(position) : null;
    }

    /// <summary>
    /// A shape dropped from the palette: default size, centered where it was dropped, selected.
    /// </summary>
    public void DropLibraryShape(ShapeLibraryItem item, float x, float y)
    {
        CurrentLibraryShape = item;
        InsertLibraryShape(new CanvasInsertEventArgs(x, y, 0, 0, isClick: true));
    }

    /// <summary>
    /// An .svg is imported as shapes, an image becomes the background.
    /// </summary>
    internal void OnDrop(object? sender, DragEventArgs e)
    {
        if (GetDraggedLibraryShape(e) is { } item)
        {
            e.Handled = true;
            if (GetCanvasDropPoint(e) is { } point)
            {
                DropLibraryShape(item, point.X, point.Y);
            }

            return;
        }

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

    /// <summary>
    /// The drawing as ASSA lines. <paramref name="hiddenAsComments"/> (.assadraw save) writes hidden
    /// shapes as Comment lines, which <see cref="LoadFromText"/> turns back into hidden shapes.
    /// <paramref name="withGroups"/> (.assadraw save too) writes the shapes' groups in the Effect field.
    /// <paramref name="withAnimation"/> false (the still preview) leaves the in/out effects out.
    /// </summary>
    private Subtitle GenerateSubtitle(bool includeAll = false, bool hiddenAsComments = false, bool withGroups = false, bool withAnimation = true)
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
        var layers = new List<(int Key, List<DrawShape> Shapes, bool IsComment)>();
        foreach (var group in Shapes.Where(s => !s.Hidden || includeAll || hiddenAsComments).GroupBy(s => s.Layer).OrderBy(g => g.Key))
        {
            if (!hiddenAsComments)
            {
                layers.Add((group.Key, group.ToList(), false));
                continue;
            }

            // Runs of visible and hidden shapes, so the shape order survives a save and load
            List<DrawShape>? run = null;
            foreach (var shape in group)
            {
                if (run == null || shape.Hidden != layers[^1].IsComment)
                {
                    run = [];
                    layers.Add((group.Key, run, shape.Hidden));
                }

                run.Add(shape);
            }
        }

        foreach (var (_, layer, _) in layers)
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

        var hasText = layers.Any(l => l.Shapes.Any(s => s.HasText));
        if (hasText)
        {
            // Own style for the text lines: centered, no margins, outline or shadow - so a Default
            // style with an opaque box or big margins doesn't change them
            subtitle.Header = AdvancedSubStationAlpha.AddSsaStyle(new SsaStyle
            {
                Name = AssaDrawTextLine.StyleName,
                Alignment = "5",
                MarginVertical = 0,
                MarginLeft = 0,
                MarginRight = 0,
                ShadowWidth = 0,
                OutlineWidth = 0,
                BorderStyle = "1",
            }, subtitle.Header);
        }

        // Generate paragraphs with style names
        var sbDraw = new StringBuilder();
        var sbErase = new StringBuilder();
        var finalText = new StringBuilder();
        foreach (var (layerKey, layer, isComment) in layers)
        {
            sbDraw.Clear();
            sbErase.Clear();
            var firstShape = GetLayerStyleShape(layer);

            var animation = withAnimation && firstShape != null ? GetLayerAnimation(layerKey) : null;
            var anchor = animation != null ? GetAnimationAnchor(firstShape!) : default;

            // Collect draw shapes (normal shapes) - relative to the anchor when animated
            foreach (var shape in layer.Where(p => !p.IsEraser && !p.IsTextBox))
            {
                sbDraw.Append(animation != null ? shape.ToAssa(anchor.X, anchor.Y) : shape.ToAssa());
                sbDraw.Append("  ");
            }

            // Collect erase shapes (iclip shapes). These stay in absolute coordinates even when the
            // layer is animated: libass never moves, scales or turns a vector \iclip with \pos/\move,
            // \fscx or \frz, and \t can't animate one. They erase the right spot when the animation
            // is at rest (and always for a fade or blink), so the clip is kept rather than dropped; the
            // canvas animation preview shows them standing still the same way.
            foreach (var shape in layer.Where(p => p.IsEraser && !p.IsTextBox))
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

                // Add draw shapes, with the layer's outline
                if (!string.IsNullOrEmpty(drawText))
                {
                    finalText.Append('{');
                    if (firstShape is { IsEraser: false, OutlineWidth: > 0 })
                    {
                        AssaDrawTextLine.AppendOutline(finalText, firstShape.OutlineWidth, firstShape.OutlineColor);
                    }

                    finalText.Append($"\\p1}}{drawText}{{\\p0}}");
                }

                if (finalText.Length > 0 && animation != null)
                {
                    // A drawing turns around its \\pos (the anchor) unless the animation needs another \\org
                    var origin = string.Empty;
                    if (animation.OriginKind != DrawAnimationOrigin.Anchor &&
                        animation.GetOrigin(anchor.X, anchor.Y, anchor.Bounds, CanvasWidth, CanvasHeight) is { } org)
                    {
                        origin = $"\\org({FormatCoordinate(org.X)},{FormatCoordinate(org.Y)})";
                    }

                    finalText.Insert(0, "{\\an7" +
                        animation.GetPositionTag(anchor.X, anchor.Y, LineDurationMs, anchor.Bounds, CanvasWidth, CanvasHeight) +
                        origin + animation.GetEffectTags(LineDurationMs, 0, anchor.Bounds, CanvasWidth, CanvasHeight) + "}" +
                        animation.FormatMarker(anchor.X, anchor.Y));
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
                        Layer = layerKey,
                        IsComment = isComment,
                        Extra = styleName,
                        Effect = withGroups ? GetGroupEffect(layer) : null,
                    };
                    subtitle.Paragraphs.Add(p);
                }
            }

            // Text after the layer's drawing, on the same layer: libass draws later lines of a
            // layer on top, so the text shows over its bubble and under the layers above
            foreach (var shape in layer.Where(s => s.HasText && s.Points.Count > 0))
            {
                var textAnimation = withAnimation ? GetLayerAnimation(layerKey) : null;
                var textLine = AssaDrawTextLine.Format(shape);
                if (textAnimation != null)
                {
                    textLine = AddTextAnimation(textLine, shape, textAnimation);
                }

                subtitle.Paragraphs.Add(new Paragraph(textLine, 0, 10000)
                {
                    Layer = layerKey,
                    IsComment = isComment,
                    Extra = AssaDrawTextLine.StyleName,

                    // A text box is only this line; a bubble's group is on its drawing line
                    Effect = withGroups && shape.IsTextBox && shape.GroupId != 0 ? GroupEffectPrefix + shape.GroupId.ToString(CultureInfo.InvariantCulture) : null,
                });
            }
        }

        return subtitle;
    }

    /// <summary>
    /// Duration of the line the drawing is written to (the first selected line), for the out effect.
    /// </summary>
    public int LineDurationMs { get; set; } = 2000;

    /// <summary>
    /// A layer is one line, so it has one animation: the first one set on any of its shapes. The
    /// output, the canvas preview and the animation panel all read it this way - the output used
    /// the first draw shape's, so a shape without one at the top of the layer dropped the effect.
    /// </summary>
    internal DrawAnimation? GetLayerAnimation(int layer) =>
        Shapes.Where(s => s.Layer == layer).Select(s => s.Animation).FirstOrDefault(a => a is { IsNone: false });

    /// <summary>
    /// What an animation scales and turns around, and what it slides out of the frame: the shape's
    /// group, or else its layer.
    /// </summary>
    internal (float X, float Y, (float Left, float Top, float Right, float Bottom) Bounds) GetAnimationAnchor(DrawShape shape)
    {
        var members = shape.GroupId != 0
            ? Shapes.Where(s => s.GroupId == shape.GroupId && !s.IsEraser)
            : Shapes.Where(s => s.Layer == shape.Layer && !s.IsEraser);
        var bounds = DrawShape.GetBounds(members);
        return ((bounds.Left + bounds.Right) / 2, (bounds.Top + bounds.Bottom) / 2, bounds);
    }

    /// <summary>
    /// A text line follows its shape's animation: it turns around the same \org as its drawing (the
    /// anchor, its top edge, or a far one), so its position is written to allow for its own \frz
    /// turning around that point; \move instead of \pos for a slide. The marker and the effects go
    /// after its tag block.
    /// </summary>
    private string AddTextAnimation(string textLine, DrawShape shape, DrawAnimation animation)
    {
        var anchor = GetAnimationAnchor(shape);
        var (left, top, right, bottom) = shape.GetTextBounds();

        // The \frz as the text line writes it (two decimals) - a far \org magnifies any difference
        var baseFrz = (float)Math.Round(-shape.GetTextFrame().Angle, 2);
        var (originX, originY) = animation.GetOrigin(anchor.X, anchor.Y, anchor.Bounds, CanvasWidth, CanvasHeight) ?? (anchor.X, anchor.Y);

        var position = animation.GetPositionTag((left + right) / 2, (top + bottom) / 2, LineDurationMs, anchor.Bounds, CanvasWidth, CanvasHeight,
            (x, y) => DrawAnimation.MapAroundOrigin(x, y, originX, originY, baseFrz));
        var posStart = textLine.IndexOf("\\pos(", StringComparison.Ordinal);
        if (posStart >= 0)
        {
            var posEnd = textLine.IndexOf(')', posStart);
            textLine = textLine[..posStart] + position + textLine[(posEnd + 1)..];
        }

        var blockEnd = textLine.IndexOf('}');
        var effects = $"{{\\org({FormatCoordinate(originX)},{FormatCoordinate(originY)})" + animation.GetEffectTags(LineDurationMs, baseFrz, anchor.Bounds, CanvasWidth, CanvasHeight) + "}";
        return textLine[..(blockEnd + 1)] + animation.FormatMarker(anchor.X, anchor.Y) + effects + textLine[(blockEnd + 1)..];
    }

    private static string FormatCoordinate(double value) =>
        Math.Abs(value) < 0.005 ? "0" : value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>
    /// The shape whose color names the layer's style: the first draw shape, or for a layer of only
    /// erase shapes (written as an \iclip-only line) the first erase shape. The style loop and the
    /// line loop picked differently, so an erase-only layer got a line with an empty style name.
    /// </summary>
    /// <summary>
    /// Effect field of an .assadraw line: "SeGroups:" and the group id of each drawing shape in
    /// the line, then "|" and those of its erase shapes, e.g. "SeGroups:1 1 0|2" - no commas, the
    /// Effect field ends at one.
    /// </summary>
    private const string GroupEffectPrefix = "SeGroups:";

    private static string? GetGroupEffect(List<DrawShape> layer)
    {
        var drawShapes = layer.Where(s => !s.IsEraser && !s.IsTextBox).ToList();
        var eraseShapes = layer.Where(s => s.IsEraser && !s.IsTextBox).ToList();
        if (drawShapes.Concat(eraseShapes).All(s => s.GroupId == 0))
        {
            return null;
        }

        var effect = GroupEffectPrefix + string.Join(' ', drawShapes.Select(s => s.GroupId.ToString(CultureInfo.InvariantCulture)));
        if (eraseShapes.Count > 0)
        {
            effect += "|" + string.Join(' ', eraseShapes.Select(s => s.GroupId.ToString(CultureInfo.InvariantCulture)));
        }

        return effect;
    }

    /// <summary>
    /// The group ids of a line's drawing and erase shapes (see <see cref="GroupEffectPrefix"/>); empty without them.
    /// </summary>
    private static (List<int> Draw, List<int> Erase) ParseGroupEffect(string? effect)
    {
        if (string.IsNullOrEmpty(effect) || !effect.StartsWith(GroupEffectPrefix, StringComparison.Ordinal))
        {
            return ([], []);
        }

        static List<int> ParseIds(string text) => text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0 ? id : 0)
            .ToList();

        var parts = effect[GroupEffectPrefix.Length..].Split('|');
        return (ParseIds(parts[0]), parts.Length > 1 ? ParseIds(parts[1]) : []);
    }

    private static DrawShape? GetLayerStyleShape(IEnumerable<DrawShape> layer)
    {
        DrawShape? firstEraser = null;
        foreach (var shape in layer)
        {
            if (shape.IsTextBox)
            {
                continue;
            }

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

    internal string GenerateAssaCode()
    {
        if (Shapes.Count == 0)
        {
            return string.Empty;
        }

        var sbDraw = new StringBuilder();
        foreach (var shape in Shapes.Where(s => !s.IsEraser && !s.Hidden && !s.IsTextBox))
        {
            sbDraw.Append(shape.ToAssa());
            sbDraw.Append(' ');
        }

        var sbErase = new StringBuilder();
        foreach (var shape in Shapes.Where(s => s.IsEraser && !s.Hidden && !s.IsTextBox))
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
            result.Append('{');
            var outlineShape = Shapes.FirstOrDefault(s => !s.IsEraser && !s.Hidden && !s.IsTextBox);
            if (outlineShape is { OutlineWidth: > 0 })
            {
                AssaDrawTextLine.AppendOutline(result, outlineShape.OutlineWidth, outlineShape.OutlineColor);
            }

            result.Append($"\\p1}}{drawCode}{{\\p0}}");
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
    private async Task Cancel()
    {
        await CloseAfterConfirm();
    }

    private bool _closeConfirmed;
    private bool _askingToClose;

    /// <summary>
    /// Closing without OK (X button) while there are changes - the window cancels the close and asks first.
    /// </summary>
    public bool ShouldAskBeforeClosing => !OkPressed && !_closeConfirmed && HasChanges;

    internal async Task CloseAfterConfirm()
    {
        if (_askingToClose)
        {
            return;
        }

        _askingToClose = true;
        try
        {
            if (await ConfirmDiscardChanges())
            {
                _closeConfirmed = true;
                Window?.Close();
            }
        }
        finally
        {
            _askingToClose = false;
        }
    }

    /// <summary>
    /// The drawing as it would be written (text, outlines, hidden shapes and canvas size included),
    /// to tell whether anything changed since the dialog opened.
    /// </summary>
    private string GetChangeState()
    {
        var subtitle = GenerateSubtitle(includeAll: true);
        var sb = new StringBuilder();
        sb.Append(CanvasWidth).Append('x').Append(CanvasHeight).Append('\n');
        foreach (var p in subtitle.Paragraphs)
        {
            sb.Append(p.Layer).Append(' ').Append(p.Extra).Append(' ').Append(p.Text).Append('\n');
        }

        foreach (var shape in Shapes.Where(s => s.Hidden))
        {
            sb.Append("hidden ").Append(Shapes.IndexOf(shape)).Append('\n');
        }

        return sb.ToString();
    }

    public bool HasChanges => _openedState != null && _openedState != GetChangeState();

    /// <summary>
    /// True when the dialog may close without OK: nothing changed, or the user agreed to discard.
    /// </summary>
    internal async Task<bool> ConfirmDiscardChanges()
    {
        if (Window == null || !HasChanges)
        {
            return true;
        }

        var result = await MessageBox.Show(Window, Se.Language.General.DiscardChangesTitle, Se.Language.Assa.DrawDiscardChanges,
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        return result == MessageBoxResult.Yes;
    }

    private void RefreshTreeView()
    {
        var previous = SelectedTreeItem;

        _refreshingTree = true;
        try
        {
            ShapeTreeItems.Clear();
            NormalizeGroups();

            // A group on one layer is a node inside that layer. A group spread over several layers
            // (clip art puts each color on its own layer) is one node next to its lowest layer, its
            // shapes named with their layer - split over layer nodes it would show up as several
            // one-shape groups, and selecting it would no longer mean "the whole group".
            var groupLayers = Shapes.Where(s => s.GroupId != 0).GroupBy(s => s.GroupId)
                .ToDictionary(g => g.Key, g => (Min: g.Min(s => s.Layer), Multi: g.Select(s => s.Layer).Distinct().Count() > 1));
            var shapeNumbers = new Dictionary<DrawShape, int>();
            foreach (var layer in Shapes.GroupBy(s => s.Layer))
            {
                var number = 0;
                foreach (var shape in layer)
                {
                    shapeNumbers[shape] = ++number;
                }
            }

            var crossLayerGroupItems = new Dictionary<int, ShapeTreeItem>();
            foreach (var shape in Shapes.Where(s => s.GroupId != 0 && groupLayers[s.GroupId].Multi))
            {
                if (!crossLayerGroupItems.TryGetValue(shape.GroupId, out var groupItem))
                {
                    groupItem = MakeGroupTreeItem(shape.GroupId);
                    crossLayerGroupItems[shape.GroupId] = groupItem;
                }

                groupItem.Children.Add(MakeShapeTreeItem(shape, shapeNumbers[shape], withLayer: true));
            }

            var layers = Shapes.GroupBy(s => s.Layer).OrderBy(g => g.Key);
            foreach (var layer in layers)
            {
                var layerShapes = layer.Where(s => s.GroupId == 0 || !groupLayers[s.GroupId].Multi).ToList();
                if (layerShapes.Count > 0)
                {
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

                    var layerGroupItems = new Dictionary<int, ShapeTreeItem>();
                    foreach (var shape in layerShapes)
                    {
                        var shapeItem = MakeShapeTreeItem(shape, shapeNumbers[shape], withLayer: false);
                        if (shape.GroupId == 0)
                        {
                            layerItem.Children.Add(shapeItem);
                            continue;
                        }

                        if (!layerGroupItems.TryGetValue(shape.GroupId, out var groupItem))
                        {
                            groupItem = MakeGroupTreeItem(shape.GroupId);
                            layerGroupItems[shape.GroupId] = groupItem;
                            layerItem.Children.Add(groupItem);
                        }

                        groupItem.Children.Add(shapeItem);
                    }

                    foreach (var groupItem in layerGroupItems.Values)
                    {
                        FinishGroupTreeItem(groupItem);
                    }

                    ShapeTreeItems.Add(layerItem);
                }

                foreach (var groupItem in crossLayerGroupItems.Where(g => groupLayers[g.Key].Min == layer.Key).Select(g => g.Value))
                {
                    FinishGroupTreeItem(groupItem);
                    ShapeTreeItems.Add(groupItem);
                }
            }
        }
        finally
        {
            _refreshingTree = false;
        }

        // Keep the selection across the rebuild - it used to vanish after every edit
        var restored = previous == null
            ? null
            : previous.IsGroup
                ? FindGroupTreeItem(previous.GroupId)
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

        NotifyCommandStates();
    }

    private static string GetShapeTreeName(DrawShape shape, int shapeNumber)
    {
        var text = shape.Text.Replace("\r", string.Empty).Replace('\n', ' ').Trim();
        if (text.Length > 24)
        {
            text = text[..24] + "…";
        }

        if (shape.IsTextBox)
        {
            return text.Length > 0 ? $"{Se.Language.Assa.DrawTextBox}: {text}" : Se.Language.Assa.DrawTextBox;
        }

        var name = string.Format(shape.IsEraser ? Se.Language.Assa.DrawShapeXErase : Se.Language.Assa.DrawShapeXDraw, shapeNumber);
        return text.Length > 0 && !shape.IsEraser ? $"{name}: {text}" : name;
    }

    /// <summary>
    /// Typing renames the shape's tree row - rebuilding the tree per key press would drop the text field's focus.
    /// </summary>
    private void UpdateShapeTreeName(DrawShape shape)
    {
        var shapeItem = EnumerateShapeTreeItems().FirstOrDefault(i => i.Shape == shape);
        if (shapeItem != null)
        {
            shapeItem.Name = GetShapeTreeName(shape, shapeItem.ShapeNumber, shapeItem.NameWithLayer);
        }
    }

    private static string GetShapeTreeName(DrawShape shape, int shapeNumber, bool withLayer)
    {
        var name = GetShapeTreeName(shape, shapeNumber);
        return withLayer ? $"{string.Format(Se.Language.Assa.DrawLayerX, shape.Layer)} · {name}" : name;
    }

    private static ShapeTreeItem MakeShapeTreeItem(DrawShape shape, int shapeNumber, bool withLayer)
    {
        var shapeItem = new ShapeTreeItem
        {
            Name = GetShapeTreeName(shape, shapeNumber, withLayer),
            Shape = shape,
            ShapeNumber = shapeNumber,
            NameWithLayer = withLayer,
            IsExpanded = shape.Expanded,
            IconName = shape.IsTextBox ? "fa-solid fa-font" : shape.IsEraser ? "fa-solid fa-eraser" : "fa-solid fa-draw-polygon",
            IsHidden = shape.Hidden,
            Meta = shape.IsTextBox ? string.Empty : shape.Points.Count.ToString(CultureInfo.InvariantCulture),
        };

        // A text box is sized with the handles, its corners aren't points to edit
        foreach (var point in shape.IsTextBox ? [] : shape.Points)
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

        return shapeItem;
    }

    /// <summary>
    /// Groups collapsed in the tree (open by default), by group id.
    /// </summary>
    private readonly HashSet<int> _collapsedGroups = [];

    private ShapeTreeItem MakeGroupTreeItem(int groupId)
    {
        var item = new ShapeTreeItem
        {
            Name = string.Format(Se.Language.Assa.DrawGroupX, groupId),
            IsGroup = true,
            GroupId = groupId,
            IsExpanded = !_collapsedGroups.Contains(groupId),
            IconName = "fa-regular fa-object-group",
        };
        item.ExpandedChanged = expanded =>
        {
            if (expanded)
            {
                _collapsedGroups.Remove(groupId);
            }
            else
            {
                _collapsedGroups.Add(groupId);
            }
        };
        return item;
    }

    private static void FinishGroupTreeItem(ShapeTreeItem groupItem)
    {
        groupItem.IsHidden = groupItem.Children.All(c => c.IsHidden);
        groupItem.Meta = groupItem.Children.Count.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// A group left with one shape (the others deleted, moved out or ungrouped) is no group.
    /// </summary>
    private void NormalizeGroups()
    {
        foreach (var group in Shapes.Where(s => s.GroupId != 0).GroupBy(s => s.GroupId).Where(g => g.Count() == 1).ToList())
        {
            group.First().GroupId = 0;
        }
    }

    /// <summary>
    /// Every shape row of the tree, in layers and in groups.
    /// </summary>
    private IEnumerable<ShapeTreeItem> EnumerateShapeTreeItems()
    {
        foreach (var topItem in ShapeTreeItems)
        {
            foreach (var child in topItem.Children)
            {
                if (child.Shape != null)
                {
                    yield return child;
                    continue;
                }

                foreach (var groupChild in child.Children.Where(c => c.Shape != null))
                {
                    yield return groupChild;
                }
            }
        }
    }

    private ShapeTreeItem? FindGroupTreeItem(int groupId)
    {
        return groupId == 0 ? null : FindAndExpand(ShapeTreeItems, i => i.IsGroup && i.GroupId == groupId, i => i.IsLayer);
    }

    /// <summary>
    /// The tree item for a point, shape or layer, with its parents expanded so it can be shown selected.
    /// </summary>
    private ShapeTreeItem? FindTreeItem(DrawCoordinate? point, DrawShape? shape, int? layer)
    {
        if (point != null)
        {
            return FindAndExpand(ShapeTreeItems, i => i.Point == point, _ => true);
        }

        if (shape != null)
        {
            // A shape row's children are its points - no need to look through those
            return FindAndExpand(ShapeTreeItems, i => i.Shape == shape, i => i.Shape == null);
        }

        return layer.HasValue ? ShapeTreeItems.FirstOrDefault(i => i.IsLayer && i.Layer == layer.Value) : null;
    }

    private static ShapeTreeItem? FindAndExpand(IEnumerable<ShapeTreeItem> items, Func<ShapeTreeItem, bool> match, Func<ShapeTreeItem, bool> descend)
    {
        foreach (var item in items)
        {
            if (match(item))
            {
                return item;
            }

            if (item.Children.Count == 0 || !descend(item))
            {
                continue;
            }

            var found = FindAndExpand(item.Children, match, descend);
            if (found != null)
            {
                item.IsExpanded = true;
                return found;
            }
        }

        return null;
    }

    private void ImportAssaDrawingFromText(string text, int layer, Color color, bool isEraser)
    {
        Shapes.AddRange(DrawShape.ParseAssa(text, layer, color, isEraser));
    }

    private readonly List<(int Layer, AssaDrawTextLine.Parsed Text, bool Hidden, int GroupId, DrawAnimation? Animation)> _importedTexts = [];

    /// <summary>
    /// One line of a script: its \iclip erase shapes, its \p1 drawing (with the line's outline),
    /// or an ASSA draw text line - those are attached to their shapes by <see cref="AttachTextLines"/>.
    /// A <paramref name="hidden"/> line (Comment line of a saved .assadraw) gives hidden shapes, and
    /// the line's <paramref name="effect"/> (saved .assadraw) their groups.
    /// </summary>
    private void ImportLine(string text, int layer, Color color, bool hidden = false, string? effect = null)
    {
        var (drawGroups, eraseGroups) = ParseGroupEffect(effect);

        // An animated line: the effects come off, and its drawing (written relative to the anchor) goes back in place
        DrawAnimation? animation = null;
        float anchorX = 0, anchorY = 0;
        var anchored = false;
        if (DrawAnimation.TryParseMarker(text, out var parsedAnimation, out anchorX, out anchorY))
        {
            animation = parsedAnimation.IsNone ? null : parsedAnimation;
            anchored = DrawAnimation.IsAnchoredDrawingLine(text);
            text = DrawAnimation.RemoveFromLine(text);
        }

        if (AssaDrawTextLine.TryParse(text, out var parsedText))
        {
            _importedTexts.Add((layer, parsedText, hidden, drawGroups.FirstOrDefault(), animation));
            return;
        }

        var firstNewShape = Shapes.Count;
        ImportDrawingLine(text, layer, color);
        int drawIndex = 0, eraseIndex = 0;
        for (var i = firstNewShape; i < Shapes.Count; i++)
        {
            var shape = Shapes[i];
            shape.Hidden = hidden;
            var groups = shape.IsEraser ? eraseGroups : drawGroups;
            var index = shape.IsEraser ? eraseIndex++ : drawIndex++;
            shape.GroupId = index < groups.Count ? groups[index] : 0;
            shape.Animation = animation?.Clone();
            if (anchored && !shape.IsEraser)
            {
                shape.Offset(anchorX, anchorY);
            }
        }
    }

    private void ImportDrawingLine(string text, int layer, Color color)
    {
        // 1. Process iclip (Vector Mask)
        var iclipMatch = _regexIclip.Match(text);
        if (iclipMatch.Success)
        {
            // Extract only the drawing commands inside the parentheses
            ImportAssaDrawingFromText(iclipMatch.Groups[1].Value, layer, color, true);
        }

        // 2. Process \p1 Drawing (the \p1 can share its tag block with \pos, \an etc.)
        if (_regexStart.IsMatch(text))
        {
            // Remove all tags (anything inside curly braces) to get raw vector data
            var drawingOnly = Regex.Replace(text, @"\{[^}]+\}", string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(drawingOnly))
            {
                var shapes = DrawShape.ParseAssa(drawingOnly, layer, color, false);
                AssaDrawTextLine.ParseOutline(text, out var outlineWidth, out var outlineColor);
                foreach (var shape in shapes)
                {
                    shape.OutlineWidth = outlineWidth;
                    shape.OutlineColor = outlineColor;
                }

                Shapes.AddRange(shapes);
            }
        }
    }

    /// <summary>
    /// Imported text goes back into the shape it was written for: the smallest drawing shape of
    /// its layer that holds the center of the text area. Text without one becomes a text box.
    /// </summary>
    private void AttachTextLines()
    {
        foreach (var (layer, parsed, hidden, groupId, animation) in _importedTexts)
        {
            var (left, top, right, bottom) = parsed.Box;
            var centerX = (left + right) / 2;
            var centerY = (top + bottom) / 2;
            var host = Shapes
                .Where(s => s.Layer == layer && s.Hidden == hidden && !s.IsEraser && !s.IsTextBox && !s.HasText && s.Points.Count > 2 && s.HitTest(centerX, centerY, 0))
                .OrderBy(s => { var b = s.GetBounds(); return (b.Right - b.Left) * (b.Bottom - b.Top); })
                .FirstOrDefault();
            if (host == null)
            {
                host = MakeRectangle(left, top, right - left, bottom - top, layer, parsed.Color ?? Colors.White);
                host.IsTextBox = true;
                host.Rotate(parsed.Angle);
                host.OutlineWidth = parsed.OutlineWidth;
                host.OutlineColor = parsed.OutlineColor;
                host.Hidden = hidden;
                host.GroupId = groupId;
                Shapes.Add(host);
            }

            host.Animation ??= animation?.Clone();

            host.Text = parsed.Text;
            host.FontName = parsed.FontName;
            host.FontSize = parsed.FontSize;
            host.FontBold = parsed.Bold;
            host.ShrinkToFit = parsed.ShrinkToFit;
            host.TextColor = parsed.Color ?? Colors.White;
            host.SetTextBounds(left, top, right, bottom, parsed.Angle);
        }

        _importedTexts.Clear();
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

    internal void LoadFromText(string text)
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

            ImportLine(paragraph.Text, paragraph.Layer, color, hidden: paragraph.IsComment, effect: paragraph.Effect);
        }

        AttachTextLines();
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

            // Esc steps back: selected point -> its shape -> nothing selected -> close
            if (ClearSelectionStep())
            {
                return;
            }

            await Cancel();
        }
        else if (IsTextInput(e) && e.Key is Key.Enter or Key.Delete or Key.Back)
        {
            // Typing in the text field (or a number box) - not a canvas command. Enter there only
            // commits the value: handled here, it no longer reaches the default OK button.
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Enter)
        {
            CloseShape();
            e.Handled = true;
        }
        else if (e.Key == Key.Delete && !IsDrawing && ActivePoint != null)
        {
            // A selected point deletes just that point (like the context menu), never the whole shape.
            // A bezier control point can't be deleted on its own, so Delete does nothing then.
            if (CanDeleteActivePoint())
            {
                DeletePoint();
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Delete && CanDeleteShape())
        {
            DeleteShape();
            e.Handled = true;
        }
        else if (e.Key is Key.Up or Key.Down or Key.Left or Key.Right && !IsTextInput(e) && GetNudgeStep(e.KeyModifiers) is { } step)
        {
            // Arrow 1 px, Shift+arrow 10 px (Alt+arrow 1 px and Ctrl+arrow 10 px still work)
            (float dx, float dy) = e.Key switch
            {
                Key.Up => (0f, -step),
                Key.Down => (0f, step),
                Key.Left => (-step, 0f),
                _ => (step, 0f),
            };

            if (AdjustPosition(dx, dy))
            {
                e.Handled = true;
            }
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
        else if ((e.KeyModifiers & ~KeyModifiers.Shift) is KeyModifiers.Control or KeyModifiers.Meta && e.Key == Key.G)
        {
            // Ctrl/Cmd+G groups the selection, Ctrl/Cmd+Shift+G ungroups it
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                UngroupShapes();
            }
            else
            {
                GroupShapes();
            }

            e.Handled = true;
        }
        else if (e.KeyModifiers.HasFlag(KeyModifiers.Alt) ||
                 (e.KeyModifiers.HasFlag(KeyModifiers.Control) && !e.KeyModifiers.HasFlag(KeyModifiers.Shift)))
        {
            switch (e.Key)
            {
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
                case Key.OemQuotes:
                    // Ctrl+' like other drawing programs - Ctrl+G is Group
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
        else if (e.KeyModifiers == KeyModifiers.None && !IsTextInput(e) && e.Key is Key.V or Key.L or Key.B or Key.R or Key.C or Key.S or Key.I or Key.T)
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
                Key.T => DrawingTool.Text,
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

    /// <summary>
    /// Arrow-key step in pixels: none or Alt 1, Shift or Ctrl 10; null for other combinations.
    /// </summary>
    private static float? GetNudgeStep(KeyModifiers modifiers)
    {
        if (modifiers.HasFlag(KeyModifiers.Alt))
        {
            return 1;
        }

        return modifiers switch
        {
            KeyModifiers.None => 1,
            KeyModifiers.Shift => 10,
            KeyModifiers.Control => 10,
            _ => null,
        };
    }

    /// <summary>
    /// Arrow-key nudge: the selected point, else the selected shape(s). False when nothing is selected.
    /// </summary>
    internal bool AdjustPosition(float xAdjust, float yAdjust)
    {
        // A selected point moves on its own (not while several shapes are selected)
        if (ActivePoint is { } point && SelectedShapes.Count <= 1 && !IsDrawing)
        {
            SaveUndo("nudge");
            point.X += xAdjust;
            point.Y += yAdjust;
            PointX = point.X;
            PointY = point.Y;
            UpdateSelectedPointName();
            UpdateSelectionInfo();
            Canvas?.InvalidateVisual();
            return true;
        }

        List<DrawShape> shapes;
        if (SelectedShapes.Count > 0)
        {
            // Multi-selection (Ctrl+A, SVG import, inserted library shapes)
            shapes = SelectedShapes;
        }
        else if (SelectedTreeItem?.Shape != null)
        {
            shapes = [SelectedTreeItem.Shape];
        }
        else if (ActiveShape != null)
        {
            // Fall back to active shape (e.g. currently being drawn)
            shapes = [ActiveShape];
        }
        else
        {
            return false;
        }

        // Holding an arrow key is one undo step, not one per repeat
        SaveUndo("nudge");
        foreach (var shape in shapes)
        {
            shape.Offset(xAdjust, yAdjust);
        }

        // Only the moved shapes' point labels change - no tree rebuild per key repeat
        UpdatePointNames(shapes);
        UpdateSelectionInfo();
        Canvas?.InvalidateVisual();
        return true;
    }

    private void UpdatePointNames(List<DrawShape> shapes)
    {
        var set = shapes.Count > 4 ? new HashSet<DrawShape>(shapes) : null;
        foreach (var shapeItem in EnumerateShapeTreeItems())
        {
            if (shapeItem.Shape == null || !(set?.Contains(shapeItem.Shape) ?? shapes.Contains(shapeItem.Shape)))
            {
                continue;
            }

            foreach (var pointItem in shapeItem.Children)
            {
                if (pointItem.Point is { } p)
                {
                    pointItem.Name = p.GetText(p.X, p.Y);
                }
            }
        }
    }

    /// <summary>
    /// One step back in the selection (Esc): a selected point goes back to its shape, a selection is
    /// cleared. False when nothing was selected.
    /// </summary>
    private bool ClearSelectionStep()
    {
        if (ActivePoint is { } point)
        {
            var shape = point.DrawShape ?? Shapes.FirstOrDefault(s => s.Points.Contains(point));
            if (shape != null && Shapes.Contains(shape) && SelectedShapes.Count <= 1)
            {
                SelectShape(shape);
            }

            if (ActivePoint != null)
            {
                ActivePoint = null;
                IsPointSelected = false;
                Canvas?.InvalidateVisual();
            }

            return true;
        }

        if (SelectedTreeItem != null || SelectedShapes.Count > 0 || (ActiveShape != null && Shapes.Contains(ActiveShape)))
        {
            ClearSelection();
            return true;
        }

        return false;
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
        NotifyCommandStates();
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

        NotifyCommandStates();
        RefreshAnimationFields();
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
        foreach (var shapeItem in EnumerateShapeTreeItems())
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
            else if (value.IsGroup && ActiveShape != null && Shapes.Contains(ActiveShape))
            {
                ActiveShape = null;
            }
        }

        // Update layer selection state
        IsLayerSelected = value?.IsLayer == true;
        IsShapeSelected = value?.Shape != null;

        // A tree pick ends a Ctrl+A selection - otherwise the arrow keys kept nudging every shape.
        // A group node is a selection of its own: all of its shapes.
        if (value?.IsGroup == true)
        {
            var members = Shapes.Where(s => s.GroupId == value.GroupId).ToList();
            SelectedShapes = members;
            if (members.Count > 0)
            {
                SetLayerColorFromSelection(members[0].ForeColor);
            }
        }
        else if (value != null)
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
        NotifyCommandStates();
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
            : item.IsGroup
                ? Shapes.Where(s => s.GroupId == item.GroupId).ToList()
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
        NotifyCommandStates();
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

        // A group or multi-selection comes back too (undo of group/ungroup, a moved group)
        if (SelectedShapes.Count > 1)
        {
            state.SelectedShapeIndexes = SelectedShapes.Select(s => Shapes.IndexOf(s)).Where(i => i >= 0).ToList();
        }

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

        if (state.SelectedShapeIndexes is { Count: > 1 } indexes && indexes.All(i => i < Shapes.Count))
        {
            SetSelection(indexes.Select(i => Shapes[i]));
        }
        else if (state.SelectedShapeIndex >= 0 && state.SelectedShapeIndex < Shapes.Count)
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
        // Like the rubber band: hidden shapes can't be seen, so they aren't moved or deleted with the rest
        SelectedShapes = Shapes.Where(s => !s.Hidden).ToList();
        Canvas?.InvalidateVisual();
    }

    public void OnClosing()
    {
        UiUtil.SaveWindowPosition(Window);

        _previewTimer?.Stop();
        StopAnimationPlayback();
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
    public List<int>? SelectedShapeIndexes { get; set; }
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

    /// <summary>
    /// A group node: its children are the group's shapes.
    /// </summary>
    public bool IsGroup { get; set; }
    public int GroupId { get; set; }

    /// <summary>
    /// A shape row's number within its layer, and whether its name starts with the layer (in a group spanning layers).
    /// </summary>
    public int ShapeNumber { get; set; }
    public bool NameWithLayer { get; set; }

    /// <summary>
    /// Called when a group node is opened or closed, so a tree rebuild keeps it that way.
    /// </summary>
    public Action<bool>? ExpandedChanged { get; set; }

    public bool HasSwatch => IsLayer;
    public bool CanToggleVisibility => IsLayer || IsGroup || Shape != null;

    partial void OnIsExpandedChanged(bool value)
    {
        // Remembered on the shape, so a tree rebuild keeps its points open or closed
        if (Shape != null)
        {
            Shape.Expanded = value;
        }

        ExpandedChanged?.Invoke(value);
    }

    public override string ToString() => Name;
}