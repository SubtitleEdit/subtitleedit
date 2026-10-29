using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using static Nikse.SubtitleEdit.Logic.EditorChrome;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// ASSA draw - laid out like a vector editor: options bar on top, tool strip on the left, canvas,
/// shapes/properties panel on the right. The chrome follows the app theme (the editor palette
/// below, dark or light), and so does the canvas workspace.
/// </summary>
public class AssaDrawWindow : Window
{
    private readonly AssaDrawViewModel _vm;
    private readonly AssaDrawCanvas _canvas;
    private readonly ContextMenu _canvasMenu;

    public AssaDrawWindow(AssaDrawViewModel vm)
    {
        _vm = vm;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.AssaDraw;
        Width = 1280;
        Height = 820;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        Background = WindowBrush;
        vm.Window = this;
        DataContext = vm;
        AddEditorStyles(Styles);

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        // Options bar
        var toolbar = CreateToolbar(vm);
        Grid.SetRow(toolbar, 0);
        mainGrid.Children.Add(toolbar);

        // Tool strip, canvas and side panel
        var contentGrid = CreateContentArea(vm, out _canvas);
        Grid.SetRow(contentGrid, 1);
        mainGrid.Children.Add(contentGrid);

        // Status bar
        var statusBar = CreateStatusBar(vm);
        Grid.SetRow(statusBar, 2);
        mainGrid.Children.Add(statusBar);

        // Code preview + OK/Cancel
        var buttonBar = CreateButtonBar(vm);
        Grid.SetRow(buttonBar, 3);
        mainGrid.Children.Add(buttonBar);

        Content = mainGrid;

        // Right-click on the canvas: the canvas reports what is under the pointer, the menu is built for it
        _canvasMenu = new ContextMenu
        {
            Placement = PlacementMode.AnchorAndGravity,
            PlacementAnchor = PopupAnchor.TopLeft,
            PlacementGravity = PopupGravity.BottomRight,
        };
        _canvas.ContextMenuRequested += OnCanvasContextMenuRequested;

        // Drop an .svg (import) or an image (background) anywhere on the window
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, vm.OnDragOver, RoutingStrategies.Bubble);
        AddHandler(DragDrop.DropEvent, vm.OnDrop, RoutingStrategies.Bubble);

        Loaded += OnLoaded;
        Closing += (_, e) =>
        {
            // The X button asks before discarding changes too (OK and a confirmed Cancel/Esc don't)
            if (vm.ShouldAskBeforeClosing)
            {
                e.Cancel = true;
                _ = vm.CloseAfterConfirm();
                return;
            }

            vm.OnClosing();
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private void OnLoaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Setup the canvas after the window is loaded
        _vm.SetCanvas(_canvas);
        _vm.Initialize();
    }

    private void OnCanvasContextMenuRequested(object? sender, CanvasContextEventArgs e)
    {
        _vm.PrepareContextMenu(e);

        var items = new List<Control>();
        if (e.Point != null)
        {
            AddPointItems(items, e.Point);
        }

        if (e.Shape != null)
        {
            AddSeparator(items);
            AddShapeItems(items, e.Shape);
        }
        else if (e.Point == null)
        {
            AddCanvasItems(items);
        }

        if (items.Count == 0)
        {
            return;
        }

        _canvasMenu.ItemsSource = items;
        _canvasMenu.PlacementRect = new Rect(e.Position, new Size(1, 1));
        _canvasMenu.Open(_canvas);
    }

    private void OnTreeContextMenuOpening(ContextMenu menu, CancelEventArgs e)
    {
        var item = _vm.SelectedTreeItem;
        var items = new List<Control>();
        if (item?.IsLayer == true)
        {
            AddLayerItems(items, item);
        }
        else if (item?.IsGroup == true)
        {
            AddGroupNodeItems(items);
        }
        else if (item?.Point != null)
        {
            AddPointItems(items, item.Point);
            var shape = _vm.TargetShape;
            if (shape != null)
            {
                AddSeparator(items);
                AddShapeItems(items, shape);
            }
        }
        else if (item?.Shape != null)
        {
            AddShapeItems(items, item.Shape);
        }

        if (items.Count == 0)
        {
            e.Cancel = true;
            return;
        }

        menu.ItemsSource = items;
    }

    private void AddPointItems(List<Control> items, DrawCoordinate point)
    {
        var shape = point.DrawShape;
        if (shape != null && shape.IsLineSegment(point))
        {
            items.Add(MakeMenuItem(Se.Language.Assa.DrawConvertToCurve, "fa-solid fa-bezier-curve", _vm.ConvertPointToCurveCommand));
        }
        else if (shape != null && shape.IsCurveSegment(point))
        {
            items.Add(MakeMenuItem(Se.Language.Assa.DrawConvertToLine, "fa-solid fa-slash", _vm.ConvertPointToLineCommand));
        }

        var isControlPoint = point.DrawType is DrawCoordinateType.BezierCurveSupport1 or DrawCoordinateType.BezierCurveSupport2;
        var deletePoint = MakeMenuItem(Se.Language.Assa.DrawDeletePoint, "fa-solid fa-xmark", _vm.DeletePointCommand);
        deletePoint.IsEnabled = !isControlPoint;
        items.Add(deletePoint);
    }

    private void AddShapeItems(List<Control> items, DrawShape shape)
    {
        items.Add(MakeMenuItem(Se.Language.General.Duplicate, "fa-regular fa-clone", _vm.DuplicateShapeCommand, new KeyGesture(Key.D, KeyModifiers.Control)));
        AddGroupItems(items);

        var moveToLayer = MakeMenuItem(Se.Language.Assa.DrawMoveToLayer, "fa-solid fa-layer-group", null);
        var layerItems = new List<Control>();
        foreach (var layer in _vm.UsedLayers)
        {
            var layerItem = MakeMenuItem(string.Format(Se.Language.Assa.DrawLayerX, layer), null, _vm.MoveShapeToLayerCommand);
            layerItem.CommandParameter = layer;
            layerItem.ToggleType = MenuItemToggleType.Radio;
            layerItem.IsChecked = layer == shape.Layer;
            layerItems.Add(layerItem);
        }

        layerItems.Add(new Separator());
        layerItems.Add(MakeMenuItem(Se.Language.Assa.DrawChangeLayer + "...", null, _vm.ChangeLayerCommand));
        moveToLayer.ItemsSource = layerItems;
        items.Add(moveToLayer);

        AddSeparator(items);
        items.Add(MakeMenuItem(Se.Language.Assa.DrawRotateClockwise, "fa-solid fa-rotate-right", _vm.RotateShapeClockwiseCommand));
        items.Add(MakeMenuItem(Se.Language.Assa.DrawRotateCounterClockwise, "fa-solid fa-rotate-left", _vm.RotateShapeCounterClockwiseCommand));
        items.Add(MakeMenuItem(Se.Language.Assa.DrawFlipHorizontal, "fa-solid fa-left-right", _vm.FlipShapeHorizontalCommand));
        items.Add(MakeMenuItem(Se.Language.Assa.DrawFlipVertical, "fa-solid fa-up-down", _vm.FlipShapeVerticalCommand));

        var toCurves = MakeMenuItem(Se.Language.Assa.DrawConvertShapeToCurves, "fa-solid fa-bezier-curve", _vm.ConvertShapeToCurvesCommand);
        toCurves.IsEnabled = shape.Points.Any(shape.IsLineSegment);
        items.Add(toCurves);
        var toLines = MakeMenuItem(Se.Language.Assa.DrawConvertShapeToLines, "fa-solid fa-draw-polygon", _vm.ConvertShapeToLinesCommand);
        toLines.IsEnabled = shape.Points.Any(shape.IsCurveSegment);
        items.Add(toLines);

        var eraser = MakeMenuItem(Se.Language.Assa.DrawUseShapeForErase, null, _vm.ToggleShapeEraserCommand);
        eraser.ToggleType = MenuItemToggleType.CheckBox;
        eraser.IsChecked = shape.IsEraser;
        items.Add(eraser);

        items.Add(MakeMenuItem(
            shape.Hidden ? Se.Language.Assa.DrawShowShape : Se.Language.Assa.DrawHideShape,
            shape.Hidden ? "fa-solid fa-eye" : "fa-solid fa-eye-slash",
            _vm.ToggleShapeVisibilityCommand));

        AddSeparator(items);
        items.Add(MakeMenuItem(Se.Language.Assa.DrawAddToLibrary, "fa-solid fa-shapes", _vm.AddToLibraryCommand));

        AddSeparator(items);
        items.Add(MakeMenuItem(WithoutShortcut(Se.Language.Assa.DrawDeleteShape), "fa-solid fa-trash", _vm.DeleteShapeCommand, new KeyGesture(Key.Delete)));
    }

    /// <summary>
    /// Group / Ungroup, each shown only when it can do something.
    /// </summary>
    private void AddGroupItems(List<Control> items)
    {
        if (_vm.GroupShapesCommand.CanExecute(null))
        {
            items.Add(MakeMenuItem(WithoutShortcut(Se.Language.Assa.DrawGroup), "fa-regular fa-object-group", _vm.GroupShapesCommand, new KeyGesture(Key.G, KeyModifiers.Control)));
        }

        if (_vm.UngroupShapesCommand.CanExecute(null))
        {
            items.Add(MakeMenuItem(WithoutShortcut(Se.Language.Assa.DrawUngroup), "fa-regular fa-object-ungroup", _vm.UngroupShapesCommand, new KeyGesture(Key.G, KeyModifiers.Control | KeyModifiers.Shift)));
        }
    }

    /// <summary>
    /// A group node in the tree: what acts on the whole group.
    /// </summary>
    private void AddGroupNodeItems(List<Control> items)
    {
        items.Add(MakeMenuItem(Se.Language.General.Duplicate, "fa-regular fa-clone", _vm.DuplicateShapeCommand, new KeyGesture(Key.D, KeyModifiers.Control)));
        AddGroupItems(items);
        AddSeparator(items);
        items.Add(MakeMenuItem(Se.Language.Assa.DrawRotateClockwise, "fa-solid fa-rotate-right", _vm.RotateShapeClockwiseCommand));
        items.Add(MakeMenuItem(Se.Language.Assa.DrawRotateCounterClockwise, "fa-solid fa-rotate-left", _vm.RotateShapeCounterClockwiseCommand));
        items.Add(MakeMenuItem(Se.Language.Assa.DrawFlipHorizontal, "fa-solid fa-left-right", _vm.FlipShapeHorizontalCommand));
        items.Add(MakeMenuItem(Se.Language.Assa.DrawFlipVertical, "fa-solid fa-up-down", _vm.FlipShapeVerticalCommand));
        AddSeparator(items);
        items.Add(MakeMenuItem(Se.Language.Assa.DrawAddToLibrary, "fa-solid fa-shapes", _vm.AddToLibraryCommand));
        AddSeparator(items);
        items.Add(MakeMenuItem(WithoutShortcut(Se.Language.Assa.DrawDeleteShape), "fa-solid fa-trash", _vm.DeleteShapeCommand, new KeyGesture(Key.Delete)));
    }

    private void AddLayerItems(List<Control> items, ShapeTreeItem layerItem)
    {
        items.Add(MakeMenuItem(
            layerItem.IsHidden ? Se.Language.Assa.DrawShowLayer : Se.Language.Assa.DrawHideLayer,
            layerItem.IsHidden ? "fa-solid fa-eye" : "fa-solid fa-eye-slash",
            _vm.ToggleLayerVisibilityCommand));
        items.Add(MakeMenuItem(Se.Language.Assa.DrawChangeLayer + "...", "fa-solid fa-layer-group", _vm.ChangeLayerCommand));
        AddSeparator(items);
        items.Add(MakeMenuItem(Se.Language.Assa.DrawDeleteLayer, "fa-solid fa-trash", _vm.DeleteLayerCommand));
    }

    private void AddCanvasItems(List<Control> items)
    {
        items.Add(MakeMenuItem(Se.Language.General.Undo, "fa-solid fa-rotate-left", _vm.UndoCommand, new KeyGesture(Key.Z, KeyModifiers.Control)));
        items.Add(MakeMenuItem(Se.Language.General.Redo, "fa-solid fa-rotate-right", _vm.RedoCommand, new KeyGesture(Key.Y, KeyModifiers.Control)));
        AddSeparator(items);

        if (_vm.IsDrawing)
        {
            items.Add(MakeMenuItem(WithoutShortcut(Se.Language.Assa.DrawCloseShape), "fa-solid fa-check", _vm.CloseShapeCommand, new KeyGesture(Key.Enter)));
            items.Add(MakeMenuItem(Se.Language.Assa.DrawCancelDrawing, "fa-solid fa-xmark", _vm.CancelDrawingCommand, new KeyGesture(Key.Escape)));
            AddSeparator(items);
        }

        if (_vm.Shapes.Count > 0)
        {
            items.Add(MakeMenuItem(Se.Language.General.SelectAll, "fa-solid fa-object-group", _vm.SelectAllShapesCommand, new KeyGesture(Key.A, KeyModifiers.Control)));
            AddSeparator(items);
        }

        items.Add(MakeMenuItem(WithoutShortcut(Se.Language.Assa.DrawZoomIn), "fa-solid fa-magnifying-glass-plus", _vm.ZoomInCommand, new KeyGesture(Key.OemPlus, KeyModifiers.Control)));
        items.Add(MakeMenuItem(WithoutShortcut(Se.Language.Assa.DrawZoomOut), "fa-solid fa-magnifying-glass-minus", _vm.ZoomOutCommand, new KeyGesture(Key.OemMinus, KeyModifiers.Control)));
        items.Add(MakeMenuItem(WithoutShortcut(Se.Language.Assa.DrawResetView), "fa-solid fa-expand", _vm.ResetViewCommand, new KeyGesture(Key.D0, KeyModifiers.Control)));
        AddSeparator(items);

        var grid = MakeMenuItem(WithoutShortcut(Se.Language.Assa.DrawToggleGrid), null, _vm.ToggleGridCommand, new KeyGesture(Key.OemQuotes, KeyModifiers.Control));
        grid.ToggleType = MenuItemToggleType.CheckBox;
        grid.IsChecked = _vm.ShowGrid;
        items.Add(grid);

        var preview = MakeMenuItem(WithoutShortcut(Se.Language.Assa.DrawTogglePreview), null, _vm.TogglePreviewCommand, new KeyGesture(Key.F9));
        preview.ToggleType = MenuItemToggleType.CheckBox;
        preview.IsChecked = _vm.ShowPreview;
        items.Add(preview);

        AddSeparator(items);
        items.Add(MakeMenuItem(Se.Language.Assa.DrawImportSvg, "fa-solid fa-file-import", _vm.ImportSvgCommand));
        var background = MakeMenuItem(Se.Language.General.Background, "fa-regular fa-image", null);
        background.ItemsSource = MakeBackgroundItems(_vm);
        items.Add(background);

        if (_vm.Shapes.Count > 0)
        {
            AddSeparator(items);
            items.Add(MakeMenuItem(WithoutShortcut(Se.Language.Assa.DrawClearAll), "fa-solid fa-eraser", _vm.ClearAllCommand, new KeyGesture(Key.N, KeyModifiers.Control)));
        }
    }

    private static void AddSeparator(List<Control> items)
    {
        if (items.Count > 0 && items[^1] is not Separator)
        {
            items.Add(new Separator());
        }
    }

    /// <summary>
    /// The tool tip texts end with their shortcut, e.g. "Delete Shape (Del)" - menus show the shortcut in their own column.
    /// </summary>
    private static string WithoutShortcut(string text)
    {
        return Regex.Replace(text, @"\s*\([^()]*\)\s*$", string.Empty);
    }

    /// <summary>
    /// The tool's translated name, for the status bar (older translations still end in "(F4)" etc.).
    /// </summary>
    internal static string GetToolName(DrawingTool tool)
    {
        var l = Se.Language.Assa;
        return WithoutShortcut(tool switch
        {
            DrawingTool.Select => l.DrawSelectTool,
            DrawingTool.Line => l.DrawLineTool,
            DrawingTool.Bezier => l.DrawBezierTool,
            DrawingTool.Rectangle => l.DrawRectangleTool,
            DrawingTool.Circle => l.DrawCircleTool,
            DrawingTool.Shape => l.DrawShapeTool,
            DrawingTool.Text => l.DrawTextTool,
            DrawingTool.ColorPicker => l.DrawColorPickerToolName,
            _ => tool.ToString(),
        });
    }

    private static List<Control> MakeBackgroundItems(AssaDrawViewModel vm)
    {
        var stretch = MakeMenuItem(Se.Language.Assa.DrawBackgroundStretch, null, vm.ToggleBackgroundStretchCommand);
        stretch.ToggleType = MenuItemToggleType.CheckBox;
        stretch.IsChecked = vm.BackgroundStretch;

        var none = MakeMenuItem(Se.Language.Assa.DrawBackgroundNone, "fa-solid fa-xmark", vm.RemoveBackgroundCommand);
        none.IsEnabled = vm.HasBackground;

        return
        [
            MakeMenuItem(Se.Language.Assa.DrawBackgroundVideoFrame, "fa-solid fa-film", vm.BackgroundFromVideoCommand),
            MakeMenuItem(Se.Language.Assa.DrawBackgroundVideoFrameAt, "fa-solid fa-clock", vm.BackgroundFromVideoAtCommand),
            MakeMenuItem(Se.Language.Assa.DrawBackgroundImage, "fa-regular fa-image", vm.BackgroundFromImageCommand),
            new Separator(),
            stretch,
            none,
        ];
    }

    private static Border CreateToolbar(AssaDrawViewModel vm)
    {
        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
        };

        // Undo/redo
        leftPanel.Children.Add(CreateToolButton("fa-solid fa-rotate-left", Se.Language.General.Undo + " (Ctrl+Z)", vm.UndoCommand));
        leftPanel.Children.Add(CreateToolButton("fa-solid fa-rotate-right", Se.Language.General.Redo + " (Ctrl+Y)", vm.RedoCommand));
        leftPanel.Children.Add(MakeToolbarSeparator());

        // Shape actions
        leftPanel.Children.Add(CreateToolButton("fa-solid fa-check", Se.Language.Assa.DrawCloseShape, vm.CloseShapeCommand));
        leftPanel.Children.Add(CreateToolButton("fa-regular fa-clone", Se.Language.General.Duplicate + " (Ctrl+D)", vm.DuplicateShapeCommand));
        leftPanel.Children.Add(CreateToolButton("fa-regular fa-object-group", Se.Language.Assa.DrawGroup, vm.GroupShapesCommand));
        leftPanel.Children.Add(CreateToolButton("fa-regular fa-object-ungroup", Se.Language.Assa.DrawUngroup, vm.UngroupShapesCommand));
        leftPanel.Children.Add(CreateToolButton("fa-solid fa-trash", Se.Language.Assa.DrawDeleteShape, vm.DeleteShapeCommand));
        leftPanel.Children.Add(CreateToolButton("fa-solid fa-eraser", Se.Language.Assa.DrawClearAll, vm.ClearAllCommand));
        leftPanel.Children.Add(MakeToolbarSeparator());

        // View toggles, highlighted while on
        var gridButton = CreateToolButton("fa-solid fa-border-all", Se.Language.Assa.DrawToggleGrid, vm.ToggleGridCommand);
        BindHighlight(gridButton, vm, () => vm.ShowGrid, nameof(vm.ShowGrid));
        var previewButton = CreateToolButton("fa-solid fa-eye", Se.Language.Assa.DrawTogglePreview, vm.TogglePreviewCommand);
        BindHighlight(previewButton, vm, () => vm.ShowPreview, nameof(vm.ShowPreview));
        leftPanel.Children.Add(gridButton);
        leftPanel.Children.Add(previewButton);
        leftPanel.Children.Add(MakeToolbarSeparator());

        // Background: video frame or image to trace over, with its opacity
        var backgroundButton = CreateToolButton("fa-regular fa-image", Se.Language.General.Background, null);
        BindHighlight(backgroundButton, vm, () => vm.HasBackground, nameof(vm.HasBackground));
        // Items depend on the current state (video loaded, background set), so the menu is built on
        // click - a MenuFlyout filled in its Opening event showed up empty.
        var backgroundMenu = new ContextMenu { Placement = PlacementMode.BottomEdgeAlignedLeft };
        backgroundButton.Click += (_, _) =>
        {
            backgroundMenu.ItemsSource = MakeBackgroundItems(vm);
            backgroundMenu.Open(backgroundButton);
        };
        leftPanel.Children.Add(backgroundButton);
        var opacitySlider = new Slider
        {
            Minimum = 0.1,
            Maximum = 1,
            Width = 80,
            Margin = new Thickness(6, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding(nameof(vm.BackgroundOpacity)) { Mode = BindingMode.TwoWay },
            [!Slider.IsEnabledProperty] = new Binding(nameof(vm.HasBackground)),
        };
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(opacitySlider, Se.Language.Assa.DrawBackgroundOpacity);
        }

        AutomationProperties.SetName(opacitySlider, Se.Language.Assa.DrawBackgroundOpacity);
        leftPanel.Children.Add(opacitySlider);
        leftPanel.Children.Add(MakeToolbarSeparator());

        // Canvas size
        leftPanel.Children.Add(MakeDimLabel(Se.Language.General.Width, new Thickness(4, 0, 6, 0)));
        leftPanel.Children.Add(MakeField(nameof(vm.CanvasWidth), 125, 4096, 64));
        leftPanel.Children.Add(MakeDimLabel("×", new Thickness(6, 0)));
        leftPanel.Children.Add(MakeField(nameof(vm.CanvasHeight), 125, 4096, 64));

        // File actions on the right
        var rightPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var copyButton = CreateToolButton("fa-solid fa-copy", Se.Language.Assa.DrawCopyToClipboard, vm.CopyToClipboardCommand);
        vm.CopyToClipboardButton = copyButton;
        rightPanel.Children.Add(CreateToolButton("fa-solid fa-file-import", Se.Language.Assa.DrawImportSvg, vm.ImportSvgCommand));
        rightPanel.Children.Add(copyButton);
        rightPanel.Children.Add(CreateToolButton("fa-solid fa-folder-open", Se.Language.Assa.DrawOpenDrawing, vm.LoadCommand));
        rightPanel.Children.Add(CreateToolButton("fa-solid fa-floppy-disk", Se.Language.Assa.DrawSaveDrawing, vm.SaveCommand));

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Children.Add(leftPanel);
        Grid.SetColumn(rightPanel, 1);
        grid.Children.Add(rightPanel);

        return new Border
        {
            Child = grid,
            Background = PanelBrush,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 6),
        };
    }

    /// <summary>
    /// Shows a tool or toggle as pressed (accent colored) while <paramref name="isOn"/> is true.
    /// </summary>
    private static void BindHighlight(Button button, AssaDrawViewModel vm, Func<bool> isOn, string propertyName)
    {
        button.Classes.Set("active", isOn());
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == propertyName)
            {
                button.Classes.Set("active", isOn());
            }
        };
    }

    private static Border CreateToolStrip(AssaDrawViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        // Each tool's shortcut shown once: the letter, plus the function key where there is one
        void AddTool(string icon, string tooltip, System.Windows.Input.ICommand command, DrawingTool tool, string shortcut, string? functionKey = null)
        {
            var keys = functionKey == null ? shortcut : shortcut + ", " + functionKey;
            var button = CreateToolButton(icon, WithoutShortcut(tooltip) + " (" + keys + ")", command, 38, shortcut);
            BindHighlight(button, vm, () => vm.CurrentTool == tool, nameof(vm.CurrentTool));
            panel.Children.Add(button);
        }

        AddTool("fa-solid fa-arrow-pointer", Se.Language.Assa.DrawSelectTool, vm.SelectToolCommand, DrawingTool.Select, "V");
        panel.Children.Add(MakeStripSeparator());
        AddTool("fa-solid fa-pen-nib", Se.Language.Assa.DrawLineTool, vm.LineToolCommand, DrawingTool.Line, "L", "F4");
        AddTool("fa-solid fa-bezier-curve", Se.Language.Assa.DrawBezierTool, vm.BezierToolCommand, DrawingTool.Bezier, "B", "F5");
        AddTool("fa-regular fa-square", Se.Language.Assa.DrawRectangleTool, vm.RectangleToolCommand, DrawingTool.Rectangle, "R", "F6");
        AddTool("fa-regular fa-circle", Se.Language.Assa.DrawCircleTool, vm.CircleToolCommand, DrawingTool.Circle, "C", "F7");

        // Shape tool: picks a shape from the library palette that opens next to the strip
        var shapeButton = CreateToolButton("fa-solid fa-shapes", WithoutShortcut(Se.Language.Assa.DrawShapeTool) + " (S)", null, 38, "S");
        BindHighlight(shapeButton, vm, () => vm.CurrentTool == DrawingTool.Shape, nameof(vm.CurrentTool));
        var palette = new Popup
        {
            PlacementTarget = shapeButton,
            Placement = PlacementMode.RightEdgeAlignedTop,
            HorizontalOffset = 8,
            IsLightDismissEnabled = true,
        };
        shapeButton.Click += (_, _) =>
        {
            vm.ShapeToolCommand.Execute(null);
            palette.Child = MakeShapePalette(vm, () => palette.IsOpen = false);
            palette.IsOpen = true;
        };
        panel.Children.Add(shapeButton);
        panel.Children.Add(palette);
        AddTool("fa-solid fa-font", Se.Language.Assa.DrawTextTool, vm.TextToolCommand, DrawingTool.Text, "T");
        AddTool("fa-solid fa-eye-dropper", Se.Language.Assa.DrawColorPickerTool, vm.ColorPickerToolCommand, DrawingTool.ColorPicker, "I");
        panel.Children.Add(MakeStripSeparator());
        panel.Children.Add(CreateToolButton("fa-solid fa-magnifying-glass-plus", Se.Language.Assa.DrawZoomIn, vm.ZoomInCommand, 38));
        panel.Children.Add(CreateToolButton("fa-solid fa-magnifying-glass-minus", Se.Language.Assa.DrawZoomOut, vm.ZoomOutCommand, 38));
        panel.Children.Add(CreateToolButton("fa-solid fa-expand", Se.Language.Assa.DrawResetView, vm.ResetViewCommand, 38));

        // Current layer color, like the fill swatch of a tool palette
        var swatch = new Border
        {
            Width = 24,
            Height = 24,
            CornerRadius = new CornerRadius(5),
            BorderBrush = StrongTextBrush,
            BorderThickness = new Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Center,
            [!Border.BackgroundProperty] = new Binding(nameof(vm.LayerColor)) { Converter = new FuncValueConverter<Color, IBrush>(c => new SolidColorBrush(c)) },
        };

        var dock = new DockPanel { LastChildFill = false };
        DockPanel.SetDock(panel, Dock.Top);
        dock.Children.Add(panel);
        swatch.Margin = new Thickness(0, 0, 0, 10);
        DockPanel.SetDock(swatch, Dock.Bottom);
        dock.Children.Add(swatch);

        return new Border
        {
            Child = dock,
            Width = 54,
            Background = PanelBrush,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Padding = new Thickness(0, 10, 0, 0),
        };
    }

    /// <summary>
    /// The shape library: built-in categories, then "My shapes". Clicking a shape picks it for the
    /// shape tool; saved shapes have a menu to remove them.
    /// </summary>
    private static Border MakeShapePalette(AssaDrawViewModel vm, Action close)
    {
        const int columns = 6;
        const double cell = 46;
        var content = new StackPanel { Spacing = 6 };
        content.Children.Add(MakeSectionHeader(Se.Language.Assa.DrawShapeLibrary));

        void AddCategory(string title, IEnumerable<ShapeLibraryItem> items)
        {
            var list = items.ToList();
            if (list.Count == 0)
            {
                return;
            }

            content.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = EditorChrome.FieldFontSize,
                Foreground = DimTextBrush,
                Margin = new Thickness(2, 6, 0, 0),
            });

            var wrap = new WrapPanel { Width = columns * (cell + 4) };
            foreach (var item in list)
            {
                Control thumbnail;
                if (item.HasOwnColors)
                {
                    // Saved shapes and clip art show all their colors: one path per layer, in one
                    // coordinate space, scaled together
                    var (left, top, right, bottom) = item.Bounds;
                    var parts = new Canvas { Width = Math.Max(1, right - left), Height = Math.Max(1, bottom - top) };
                    foreach (var (color, geometry) in item.ToColoredGeometries())
                    {
                        parts.Children.Add(new Avalonia.Controls.Shapes.Path { Data = geometry, Fill = new SolidColorBrush(color) });
                    }

                    thumbnail = new Viewbox { Child = parts, Stretch = Stretch.Uniform, Margin = new Thickness(7) };
                }
                else
                {
                    thumbnail = new Avalonia.Controls.Shapes.Path
                    {
                        Data = item.ToGeometry(),
                        Fill = ThumbnailBrush,
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(9),
                    };
                }

                var button = new Button
                {
                    Content = thumbnail,
                    Width = cell,
                    Height = cell,
                    Margin = new Thickness(0, 0, 4, 4),
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                };
                button.Classes.Add("tool");
                button.Classes.Set("active", vm.CurrentLibraryShape?.Name == item.Name && vm.CurrentLibraryShape?.Category == item.Category);
                if (Se.Settings.Appearance.ShowHints)
                {
                    ToolTip.SetTip(button, item.Name);
                }

                AutomationProperties.SetName(button, item.Name);
                button.Click += (_, _) =>
                {
                    vm.PickLibraryShapeCommand.Execute(item);
                    close();
                };
                AddPaletteDrag(button, item, close);

                if (item.UserShape != null)
                {
                    var remove = MakeMenuItem(Se.Language.Assa.DrawRemoveFromLibrary, "fa-solid fa-trash", null);
                    remove.Click += (_, _) =>
                    {
                        vm.RemoveFromLibraryCommand.Execute(item);
                        wrap.Children.Remove(button);
                    };
                    button.ContextMenu = new ContextMenu { ItemsSource = new List<Control> { remove } };
                }

                wrap.Children.Add(button);
            }

            content.Children.Add(wrap);
        }

        foreach (var category in ShapeLibrary.BuiltInCategories)
        {
            AddCategory(category, ShapeLibrary.BuiltIn.Where(i => i.Category == category));
        }

        AddCategory(Se.Language.Assa.DrawCategoryMyShapes, ShapeLibrary.GetUserShapes());

        content.Children.Add(new TextBlock
        {
            Text = Se.Language.Assa.DrawShapeLibraryHint,
            FontSize = EditorChrome.ScaledFont(11.5),
            Foreground = FaintTextBrush,
            TextWrapping = TextWrapping.Wrap,
            Width = columns * (cell + 4),
            Margin = new Thickness(2, 4, 0, 0),
        });

        return new Border
        {
            Child = new ScrollViewer { Content = content, MaxHeight = 560 },
            Background = MenuBrush,
            BorderBrush = MenuBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12, 10, 8, 10),
            BoxShadow = new BoxShadows(new BoxShadow { Blur = 24, OffsetY = 8, Color = Color.FromArgb(150, 0, 0, 0) }),
        };
    }

    /// <summary>
    /// Dragging a palette shape (instead of clicking it) drops it on the canvas where it is released.
    /// </summary>
    private static void AddPaletteDrag(Button button, ShapeLibraryItem item, Action close)
    {
        PointerPressedEventArgs? press = null;
        Point start = default;
        button.AddHandler(PointerPressedEvent, (_, e) =>
        {
            if (e.GetCurrentPoint(button).Properties.IsLeftButtonPressed)
            {
                press = e;
                start = e.GetPosition(button);
            }
        }, RoutingStrategies.Tunnel);
        button.AddHandler(PointerReleasedEvent, (_, _) => press = null, RoutingStrategies.Tunnel);
        button.AddHandler(PointerMovedEvent, async (_, e) =>
        {
            if (press == null || !e.GetCurrentPoint(button).Properties.IsLeftButtonPressed)
            {
                return;
            }

            var position = e.GetPosition(button);
            if (Math.Abs(position.X - start.X) < 6 && Math.Abs(position.Y - start.Y) < 6)
            {
                return;
            }

            var pressed = press;
            press = null;
            try
            {
                // macOS aborts the app on a drag with nothing it can put on its pasteboard (an
                // in-process format only) - the shape's name goes along as text
                var dataItem = new DataTransferItem();
                dataItem.Set(AssaDrawViewModel.LibraryShapeFormat, item);
                dataItem.SetText(item.Name);
                var transfer = new DataTransfer();
                transfer.Add(dataItem);
                await DragDrop.DoDragDropAsync(pressed, transfer, DragDropEffects.Copy);
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "ASSA draw: could not drag shape from palette");
            }
            finally
            {
                close();
            }
        }, RoutingStrategies.Tunnel);
    }

    private static Border MakeStripSeparator()
    {
        return new Border
        {
            Height = 1,
            Width = 26,
            Background = SeparatorBrush,
            Margin = new Thickness(0, 4),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
    }

    private Grid CreateContentArea(AssaDrawViewModel vm, out AssaDrawCanvas canvas)
    {
        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(296, GridUnitType.Pixel) },
            },
        };

        contentGrid.Children.Add(CreateToolStrip(vm));

        canvas = new AssaDrawCanvas();
        Grid.SetColumn(canvas, 1);
        contentGrid.Children.Add(canvas);

        var sidePanel = CreateSidePanel(vm);
        Grid.SetColumn(sidePanel, 2);
        contentGrid.Children.Add(sidePanel);

        return contentGrid;
    }

    private Border CreateSidePanel(AssaDrawViewModel vm)
    {
        var panelGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        // Header with quick actions
        var header = new DockPanel { Margin = new Thickness(12, 10, 8, 4) };
        var headerButtons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        headerButtons.Children.Add(CreateToolButton("fa-regular fa-clone", Se.Language.General.Duplicate, vm.DuplicateShapeCommand, 24));
        headerButtons.Children.Add(CreateToolButton("fa-solid fa-trash", WithoutShortcut(Se.Language.Assa.DrawDeleteShape), vm.DeleteShapeCommand, 24));
        DockPanel.SetDock(headerButtons, Dock.Right);
        header.Children.Add(headerButtons);
        header.Children.Add(MakeSectionHeader(Se.Language.Assa.DrawShapes));
        Grid.SetRow(header, 0);
        panelGrid.Children.Add(header);

        // Layers > shapes > points; selection is two-way so canvas picks show up here
        var treeView = new TreeView
        {
            [!TreeView.ItemsSourceProperty] = new Binding(nameof(vm.ShapeTreeItems)),
            [!TreeView.SelectedItemProperty] = new Binding(nameof(vm.SelectedTreeItem)) { Mode = BindingMode.TwoWay },
            Margin = new Thickness(6, 0, 6, 6),

            // Ctrl/Shift+click picks several shapes or groups; a plain click still picks one
            SelectionMode = SelectionMode.Multiple,
        };
        treeView.SelectionChanged += (_, _) =>
        {
            if (treeView.SelectedItems is { Count: > 1 } selectedItems)
            {
                vm.SetTreeMultiSelection(selectedItems.OfType<ShapeTreeItem>().ToList());
            }
            else if (treeView.SelectedItems is { Count: 1 } single && single[0] is ShapeTreeItem item)
            {
                // A plain click on the row that was already the tree's current item ends the multi-selection
                vm.OnTreeSingleSelection(item);
            }
        };
        treeView.Styles.Add(new Style(x => x.OfType<TreeViewItem>())
        {
            Setters =
            {
                new Setter(TreeViewItem.IsExpandedProperty, new Binding(nameof(ShapeTreeItem.IsExpanded)) { Mode = BindingMode.TwoWay }),
            },
        });

        var hiddenToOpacity = new FuncValueConverter<bool, double>(hidden => hidden ? 0.45 : 1.0);
        var hiddenToIcon = new FuncValueConverter<bool, string>(hidden => hidden ? "fa-solid fa-eye-slash" : "fa-solid fa-eye");
        treeView.ItemTemplate = new FuncTreeDataTemplate<ShapeTreeItem>(
            (_, _) =>
            {
                var row = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                    },
                    ColumnSpacing = 6,
                };

                var eye = CreateToolButton("fa-solid fa-eye", Se.Language.Assa.DrawHideShape, vm.ToggleItemVisibilityCommand, 22);
                eye.Bind(Button.CommandParameterProperty, new Binding("."));
                eye.Bind(Button.IsVisibleProperty, new Binding(nameof(ShapeTreeItem.CanToggleVisibility)));
                if (eye.Content is Optris.Icons.Avalonia.Icon eyeIcon)
                {
                    eyeIcon.FontSize = 10;
                    eyeIcon.Bind(Optris.Icons.Avalonia.Icon.ValueProperty, new Binding(nameof(ShapeTreeItem.IsHidden)) { Converter = hiddenToIcon });
                }

                row.Children.Add(eye);

                var content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 7,
                    [!StackPanel.OpacityProperty] = new Binding(nameof(ShapeTreeItem.IsHidden)) { Converter = hiddenToOpacity },
                };
                var icon = new Optris.Icons.Avalonia.Icon
                {
                    FontSize = 11,
                    Width = 14,
                    Foreground = DimTextBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!Optris.Icons.Avalonia.Icon.ValueProperty] = new Binding(nameof(ShapeTreeItem.IconName)),
                };
                Grid.SetColumn(icon, 1);
                row.Children.Add(icon);

                var swatch = new Border
                {
                    Width = 13,
                    Height = 13,
                    CornerRadius = new CornerRadius(4),
                    BorderThickness = new Thickness(1),
                    BorderBrush = SeparatorBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!Border.BackgroundProperty] = new Binding(nameof(ShapeTreeItem.Swatch)),
                    [!Border.IsVisibleProperty] = new Binding(nameof(ShapeTreeItem.HasSwatch)),
                };
                Grid.SetColumn(swatch, 2);
                row.Children.Add(swatch);

                content.Children.Add(new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    [!TextBlock.TextProperty] = new Binding(nameof(ShapeTreeItem.Name)),
                });
                Grid.SetColumn(content, 3);
                row.Children.Add(content);

                var meta = new TextBlock
                {
                    FontSize = EditorChrome.ScaledFont(11),
                    Foreground = FaintTextBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 6, 0),
                    [!TextBlock.TextProperty] = new Binding(nameof(ShapeTreeItem.Meta)),
                };
                Grid.SetColumn(meta, 4);
                row.Children.Add(meta);
                return row;
            },
            item => item.Children);

        var treeMenu = new ContextMenu();
        treeMenu.Opening += (_, e) => OnTreeContextMenuOpening(treeMenu, e);
        treeView.ContextMenu = treeMenu;

        Grid.SetRow(treeView, 1);
        panelGrid.Children.Add(treeView);

        // Properties scroll when shape, text and point sections don't fit under the tree
        var properties = new StackPanel();
        properties.Children.Add(CreateShapePropertiesPanel(vm));
        properties.Children.Add(CreateTextPropertiesPanel(vm));
        properties.Children.Add(CreateLayerPropertiesPanel(vm));
        properties.Children.Add(AssaDrawAnimationPanel.Create(vm));
        properties.Children.Add(CreatePointPropertiesPanel(vm));
        var propertiesScroll = new ScrollViewer
        {
            Content = properties,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        Grid.SetRow(propertiesScroll, 2);
        panelGrid.Children.Add(propertiesScroll);

        // An Auto row never scrolls - cap it so the tree keeps some room
        panelGrid.SizeChanged += (_, e) => propertiesScroll.MaxHeight = Math.Max(120, e.NewSize.Height - header.Bounds.Height - 140);

        return new Border
        {
            Child = panelGrid,
            Background = PanelBrush,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        };
    }

    private static Border MakePropertySection(string title, Control content, string isVisibleProperty)
    {
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(MakeSectionHeader(title));
        panel.Children.Add(content);
        return new Border
        {
            Child = panel,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(12, 10, 12, 12),
            [!Border.IsVisibleProperty] = new Binding(isVisibleProperty),
        };
    }

    private static Border CreateShapePropertiesPanel(AssaDrawViewModel vm)
    {
        var rows = new StackPanel { Spacing = 6 };
        rows.Children.Add(MakePropertyRow(Se.Language.General.Position,
            MakeField(nameof(vm.ShapeX), -10000, 10000, prefix: "X", format: "0.#"),
            MakeField(nameof(vm.ShapeY), -10000, 10000, prefix: "Y", format: "0.#")));
        rows.Children.Add(MakePropertyRow(Se.Language.General.Size,
            MakeField(nameof(vm.ShapeWidth), 0, 10000, prefix: "W", format: "0.#"),
            MakeField(nameof(vm.ShapeHeight), 0, 10000, prefix: "H", format: "0.#")));

        var colorButton = UiUtil.MakeColorPickerButton(vm, nameof(vm.LayerColor));
        colorButton.HorizontalAlignment = HorizontalAlignment.Left;
        colorButton.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsDrawShapeSelected)));
        rows.Children.Add(MakePropertyRow(Se.Language.General.Layer,
            MakeField(nameof(vm.ShapeLayer), 0, 1000),
            colorButton));
        rows.Children.Add(MakeOutlineRow(vm));

        var eraser = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(nameof(vm.ShapeIsEraser)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(eraser, Se.Language.Assa.DrawUseShapeForErase);
        var eraserRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.IsDrawShapeSelected)),
        };
        eraserRow.Children.Add(eraser);
        eraserRow.Children.Add(new TextBlock { Text = Se.Language.Assa.DrawUseShapeForErase, VerticalAlignment = VerticalAlignment.Center, FontSize = EditorChrome.FieldFontSize });
        rows.Children.Add(eraserRow);

        return MakePropertySection(Se.Language.Assa.DrawSelectedShape, rows, nameof(vm.IsShapeSelected));
    }

    /// <summary>
    /// Outline width and color: of the layer for drawing shapes, of the text for a text box.
    /// </summary>
    private static Grid MakeOutlineRow(AssaDrawViewModel vm)
    {
        var outlineColor = UiUtil.MakeColorPickerButton(vm, nameof(vm.ShapeOutlineColor));
        outlineColor.HorizontalAlignment = HorizontalAlignment.Left;
        var width = MakeField(nameof(vm.ShapeOutlineWidth), 0, 100, format: "0.#");
        width.Increment = 0.5m;
        AutomationProperties.SetName(width, Se.Language.General.Outline);
        var row = MakePropertyRow(Se.Language.General.Outline, width, outlineColor);
        row.Bind(Grid.IsVisibleProperty, new Binding(nameof(vm.CanEditOutline)));
        return row;
    }

    private Border CreateTextPropertiesPanel(AssaDrawViewModel vm)
    {
        var rows = new StackPanel { Spacing = 6 };

        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 56,
            MaxHeight = 120,
            FontSize = EditorChrome.FieldFontSize,
            Background = FieldBrush,
            BorderBrush = FieldBorderBrush,
            CornerRadius = new CornerRadius(6),
            [!TextBox.TextProperty] = new Binding(nameof(vm.ShapeText)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(textBox, Se.Language.General.Text);
        rows.Children.Add(textBox);
        vm.TextEditRequested += (_, _) => Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            textBox.Focus();
            textBox.SelectAll();
        });

        var fontCombo = new ComboBox
        {
            ItemsSource = vm.FontNames,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinHeight = 28,
            FontSize = EditorChrome.FieldFontSize,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.TextFontName)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(fontCombo, Se.Language.General.FontName);
        rows.Children.Add(MakePropertyRow(Se.Language.General.FontName, fontCombo));

        var bold = new ToggleButton
        {
            Content = new Optris.Icons.Avalonia.Icon { Value = "fa-solid fa-bold", FontSize = 12 },
            Width = 28,
            Height = 28,
            Padding = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.TextBold)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(bold, Se.Language.General.Bold);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(bold, Se.Language.General.Bold);
        }

        var size = MakeField(nameof(vm.TextFontSize), AssaTextLayout.MinFontSize, 1000, format: "0.#");
        AutomationProperties.SetName(size, Se.Language.General.FontSize);
        var sizeAndBold = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        size.Width = 70;
        sizeAndBold.Children.Add(size);
        sizeAndBold.Children.Add(bold);
        var textColor = UiUtil.MakeColorPickerButton(vm, nameof(vm.TextColor));
        textColor.HorizontalAlignment = HorizontalAlignment.Left;
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(textColor, Se.Language.General.TextColor);
        }

        AutomationProperties.SetName(textColor, Se.Language.General.TextColor);
        rows.Children.Add(MakePropertyRow(Se.Language.General.FontSize, sizeAndBold, textColor));

        var shrink = new CheckBox
        {
            Content = Se.Language.Assa.DrawShrinkToFit,
            FontSize = EditorChrome.FieldFontSize,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.TextShrinkToFit)) { Mode = BindingMode.TwoWay },
        };
        rows.Children.Add(shrink);

        return MakePropertySection(Se.Language.General.Text, rows, nameof(vm.CanEditText));
    }

    private static Border CreateLayerPropertiesPanel(AssaDrawViewModel vm)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        row.Children.Add(UiUtil.MakeColorPickerButton(vm, nameof(vm.LayerColor)));
        var changeLayer = new Button
        {
            Content = Se.Language.Assa.DrawChangeLayer,
            Command = vm.ChangeLayerCommand,
            VerticalAlignment = VerticalAlignment.Center,
        };
        row.Children.Add(changeLayer);
        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(row);
        rows.Children.Add(MakeOutlineRow(vm));
        return MakePropertySection(Se.Language.Assa.DrawSelectedLayer, rows, nameof(vm.IsLayerSelected));
    }

    private static Border CreatePointPropertiesPanel(AssaDrawViewModel vm)
    {
        var row = MakePropertyRow(Se.Language.General.Position,
            MakeField(nameof(vm.PointX), -10000, 10000, prefix: "X", format: "0.#"),
            MakeField(nameof(vm.PointY), -10000, 10000, prefix: "Y", format: "0.#"));
        return MakePropertySection(Se.Language.Assa.DrawSelectedPoint, row, nameof(vm.IsPointSelected));
    }

    private static Border CreateStatusBar(AssaDrawViewModel vm)
    {
        var left = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18,
            VerticalAlignment = VerticalAlignment.Center,
        };

        static StackPanel MakeStatusItem(string icon, Control text)
        {
            var item = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
            item.Children.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 11, Foreground = FaintTextBrush, VerticalAlignment = VerticalAlignment.Center });
            item.Children.Add(text);
            return item;
        }

        TextBlock StatusText(string property) => new()
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(property),
        };

        left.Children.Add(MakeStatusItem("fa-solid fa-crosshairs", StatusText(nameof(vm.PositionText))));

        var toolLabel = StatusText(nameof(vm.CurrentTool));
        toolLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.CurrentTool))
        {
            Converter = new FuncValueConverter<DrawingTool, string>(tool => string.Format(Se.Language.Assa.DrawToolX, GetToolName(tool))),
        });
        left.Children.Add(MakeStatusItem("fa-solid fa-pen-ruler", toolLabel));

        var selection = MakeStatusItem("fa-solid fa-draw-polygon", StatusText(nameof(vm.SelectionText)));
        selection.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.SelectionText)) { Converter = StringConverters.IsNotNullOrEmpty });
        left.Children.Add(selection);

        left.Children.Add(new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PreviewStatusText)),
            Foreground = Brushes.OrangeRed,
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
        });

        var help = new TextBlock
        {
            Text = Se.Language.Assa.DrawHelpText,
            Foreground = FaintTextBrush,
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(18, 0),
        };
        ToolTip.SetTip(help, Se.Language.Assa.DrawHelpText);

        // Zoom: - slider + percent Fit
        var zoom = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
        zoom.Children.Add(CreateToolButton("fa-solid fa-minus", WithoutShortcut(Se.Language.Assa.DrawZoomOut), vm.ZoomOutCommand, 22));
        var zoomSlider = new Slider
        {
            Minimum = 10,
            Maximum = 400,
            Width = 110,
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding(nameof(vm.ZoomPercent)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(zoomSlider, Se.Language.Assa.DrawZoom);
        zoom.Children.Add(zoomSlider);
        zoom.Children.Add(CreateToolButton("fa-solid fa-plus", WithoutShortcut(Se.Language.Assa.DrawZoomIn), vm.ZoomInCommand, 22));
        zoom.Children.Add(new TextBlock
        {
            Width = 44,
            FontSize = EditorChrome.FieldFontSize,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ZoomPercent)) { StringFormat = "{0:0}%" },
        });
        var fit = new Button
        {
            Content = WithoutShortcut(Se.Language.Assa.DrawResetView),
            Command = vm.ResetViewCommand,
            FontSize = EditorChrome.FieldFontSize,
            Padding = new Thickness(10, 2),
            MinHeight = 0,
            Margin = new Thickness(6, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        zoom.Children.Add(fit);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Children.Add(left);
        Grid.SetColumn(help, 1);
        grid.Children.Add(help);
        Grid.SetColumn(zoom, 2);
        grid.Children.Add(zoom);

        return new Border
        {
            Child = grid,
            Background = StatusBrush,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(12, 4),
            MinHeight = 32,
        };
    }

    private static Border CreateButtonBar(AssaDrawViewModel vm)
    {
        var code = new TextBlock
        {
            FontFamily = new FontFamily("Menlo, Consolas, Cascadia Mono, DejaVu Sans Mono, monospace"),
            FontSize = EditorChrome.FieldFontSize,
            Foreground = FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 0, 16, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.CodePreview)),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");

        // Not the default button: Enter means "close shape" on the canvas, and closing the
        // editor from wherever focus happens to be (layer tree, check box...) is a surprise.
        buttonOk.IsDefault = false;
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        buttons.Children.Add(buttonCancel);
        buttons.Children.Add(buttonOk);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Children.Add(code);
        Grid.SetColumn(buttons, 1);
        grid.Children.Add(buttons);

        return new Border
        {
            Child = grid,
            Background = ButtonBarBrush,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
        };
    }
}
