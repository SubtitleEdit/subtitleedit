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

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

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
        Width = 1200;
        Height = 800;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(10),
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

        // Button bar
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        Grid.SetRow(panelButtons, 3);
        mainGrid.Children.Add(panelButtons);

        Content = mainGrid;

        // Right-click on the canvas: the canvas reports what is under the pointer, the menu is built for it
        _canvasMenu = new ContextMenu
        {
            Placement = PlacementMode.AnchorAndGravity,
            PlacementAnchor = PopupAnchor.TopLeft,
            PlacementGravity = PopupGravity.BottomRight,
        };
        _canvas.ContextMenuRequested += OnCanvasContextMenuRequested;

        // Drop an .svg file anywhere on the window to import it
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, vm.OnDragOver, RoutingStrategies.Bubble);
        AddHandler(DragDrop.DropEvent, vm.OnDrop, RoutingStrategies.Bubble);

        Loaded += OnLoaded;
        Closing += (_, e) => vm.OnClosing();
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

        var grid = MakeMenuItem(WithoutShortcut(Se.Language.Assa.DrawToggleGrid), null, _vm.ToggleGridCommand, new KeyGesture(Key.G, KeyModifiers.Control));
        grid.ToggleType = MenuItemToggleType.CheckBox;
        grid.IsChecked = _vm.ShowGrid;
        items.Add(grid);

        var preview = MakeMenuItem(WithoutShortcut(Se.Language.Assa.DrawTogglePreview), null, _vm.TogglePreviewCommand, new KeyGesture(Key.F9));
        preview.ToggleType = MenuItemToggleType.CheckBox;
        preview.IsChecked = _vm.ShowPreview;
        items.Add(preview);

        AddSeparator(items);
        items.Add(MakeMenuItem(Se.Language.Assa.DrawImportSvg, "fa-solid fa-file-import", _vm.ImportSvgCommand));

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

    private static MenuItem MakeMenuItem(string header, string? icon, System.Windows.Input.ICommand? command, KeyGesture? gesture = null)
    {
        var item = new MenuItem
        {
            Header = header,
            Command = command,
            InputGesture = gesture,
        };

        if (icon != null)
        {
            item.Icon = new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 13 };
        }

        return item;
    }

    /// <summary>
    /// The tool tip texts end with their shortcut, e.g. "Delete Shape (Del)" - menus show the shortcut in their own column.
    /// </summary>
    private static string WithoutShortcut(string text)
    {
        return Regex.Replace(text, @"\s*\([^()]*\)\s*$", string.Empty);
    }

    private static Border CreateToolbar(AssaDrawViewModel vm)
    {
        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
        };

        // Undo/redo
        leftPanel.Children.Add(CreateToolButton("fa-solid fa-rotate-left", Se.Language.General.Undo + " (Ctrl+Z)", vm.UndoCommand));
        leftPanel.Children.Add(CreateToolButton("fa-solid fa-rotate-right", Se.Language.General.Redo + " (Ctrl+Y)", vm.RedoCommand));
        leftPanel.Children.Add(MakeToolbarSeparator());

        // Shape actions
        leftPanel.Children.Add(CreateToolButton("fa-solid fa-check", Se.Language.Assa.DrawCloseShape, vm.CloseShapeCommand));
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

        // Canvas size
        leftPanel.Children.Add(new TextBlock
        {
            Text = Se.Language.General.Width,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0),
            Opacity = 0.75,
        });
        leftPanel.Children.Add(new NumericUpDown
        {
            Minimum = 125,
            Maximum = 4096,
            Width = 125,
            Increment = 10,
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.CanvasWidth)) { Mode = BindingMode.TwoWay },
        });
        leftPanel.Children.Add(new TextBlock
        {
            Text = Se.Language.General.Height,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0),
            Opacity = 0.75,
        });
        leftPanel.Children.Add(new NumericUpDown
        {
            Minimum = 125,
            Maximum = 4096,
            Width = 125,
            Increment = 10,
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.CanvasHeight)) { Mode = BindingMode.TwoWay },
        });

        // File actions on the right
        var rightPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
        };
        var copyButton = CreateToolButton("fa-solid fa-copy", Se.Language.Assa.DrawCopyToClipboard, vm.CopyToClipboardCommand);
        vm.CopyToClipboardButton = copyButton;
        rightPanel.Children.Add(CreateToolButton("fa-solid fa-file-import", Se.Language.Assa.DrawImportSvg, vm.ImportSvgCommand));
        rightPanel.Children.Add(copyButton);
        rightPanel.Children.Add(CreateToolButton("fa-solid fa-folder-open", "Load", vm.LoadCommand));
        rightPanel.Children.Add(CreateToolButton("fa-solid fa-floppy-disk", "Save", vm.SaveCommand));

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
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(6, 4),
            Margin = new Thickness(0, 0, 0, 8),
        };
    }

    private static Border MakeToolbarSeparator()
    {
        return new Border
        {
            Width = 1,
            Height = 22,
            Background = UiUtil.GetBorderBrush(),
            Margin = new Thickness(6, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    private static Button CreateToolButton(string icon, string tooltip, System.Windows.Input.ICommand command, double size = 32)
    {
        var button = new Button
        {
            Content = new Optris.Icons.Avalonia.Icon { Value = icon },
            Width = size,
            Height = size,
            Command = command,
            Padding = new Thickness(4),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(button, tooltip);
        }

        // Icon-only, so the tool tip text is the only text there is for a screen reader (#12087).
        AutomationProperties.SetName(button, tooltip);
        return button;
    }

    /// <summary>
    /// Shows a tool or toggle as pressed (accent colored) while <paramref name="isOn"/> is true.
    /// </summary>
    private static void BindHighlight(Button button, AssaDrawViewModel vm, Func<bool> isOn, string propertyName)
    {
        button.Classes.Set("accent", isOn());
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == propertyName)
            {
                button.Classes.Set("accent", isOn());
            }
        };
    }

    private static Border CreateToolStrip(AssaDrawViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
        };

        void AddTool(string icon, string tooltip, System.Windows.Input.ICommand command, DrawingTool tool)
        {
            var button = CreateToolButton(icon, tooltip, command, 38);
            BindHighlight(button, vm, () => vm.CurrentTool == tool, nameof(vm.CurrentTool));
            panel.Children.Add(button);
        }

        AddTool("fa-solid fa-arrow-pointer", Se.Language.Assa.DrawSelectTool, vm.SelectToolCommand, DrawingTool.Select);
        AddTool("fa-solid fa-pen", Se.Language.Assa.DrawLineTool, vm.LineToolCommand, DrawingTool.Line);
        AddTool("fa-solid fa-bezier-curve", Se.Language.Assa.DrawBezierTool, vm.BezierToolCommand, DrawingTool.Bezier);
        AddTool("fa-regular fa-square", Se.Language.Assa.DrawRectangleTool, vm.RectangleToolCommand, DrawingTool.Rectangle);
        AddTool("fa-regular fa-circle", Se.Language.Assa.DrawCircleTool, vm.CircleToolCommand, DrawingTool.Circle);

        panel.Children.Add(new Border
        {
            Height = 1,
            Width = 26,
            Background = UiUtil.GetBorderBrush(),
            Margin = new Thickness(0, 6),
            HorizontalAlignment = HorizontalAlignment.Center,
        });

        panel.Children.Add(CreateToolButton("fa-solid fa-magnifying-glass-plus", Se.Language.Assa.DrawZoomIn, vm.ZoomInCommand, 38));
        panel.Children.Add(CreateToolButton("fa-solid fa-magnifying-glass-minus", Se.Language.Assa.DrawZoomOut, vm.ZoomOutCommand, 38));
        panel.Children.Add(CreateToolButton("fa-solid fa-expand", Se.Language.Assa.DrawResetView, vm.ResetViewCommand, 38));

        return new Border
        {
            Child = panel,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(5),
            Margin = new Thickness(0, 0, 8, 0),
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
                new ColumnDefinition { Width = new GridLength(270, GridUnitType.Pixel) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(0, 0, 0, 8),
        };

        var toolStrip = CreateToolStrip(vm);
        contentGrid.Children.Add(toolStrip);

        // Drawing canvas
        var canvasBorder = new Border
        {
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            ClipToBounds = true,
            Margin = new Thickness(0, 0, 8, 0),
        };

        canvas = new AssaDrawCanvas();
        canvasBorder.Child = canvas;

        Grid.SetColumn(canvasBorder, 1);
        contentGrid.Children.Add(canvasBorder);

        // Side panel
        var sidePanel = CreateSidePanel(vm);
        Grid.SetColumn(sidePanel, 2);
        contentGrid.Children.Add(sidePanel);

        return contentGrid;
    }

    private static TextBlock MakeSectionHeader(string text)
    {
        return new TextBlock
        {
            Text = text.ToUpperInvariant(),
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            Opacity = 0.65,
            Margin = new Thickness(0, 0, 0, 6),
        };
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        // Header
        var headerLabel = MakeSectionHeader(Se.Language.Assa.DrawShapes);
        headerLabel.Margin = new Thickness(10, 10, 10, 2);
        Grid.SetRow(headerLabel, 0);
        panelGrid.Children.Add(headerLabel);

        // Layers > shapes > points; selection is two-way so canvas picks show up here
        var treeView = new TreeView
        {
            [!TreeView.ItemsSourceProperty] = new Binding(nameof(vm.ShapeTreeItems)),
            [!TreeView.SelectedItemProperty] = new Binding(nameof(vm.SelectedTreeItem)) { Mode = BindingMode.TwoWay },
            Margin = new Thickness(4),
        };
        treeView.Styles.Add(new Style(x => x.OfType<TreeViewItem>())
        {
            Setters =
            {
                new Setter(TreeViewItem.IsExpandedProperty, new Binding(nameof(ShapeTreeItem.IsExpanded)) { Mode = BindingMode.TwoWay }),
            },
        });

        var hiddenToOpacity = new FuncValueConverter<bool, double>(hidden => hidden ? 0.45 : 1.0);
        treeView.ItemTemplate = new FuncTreeDataTemplate<ShapeTreeItem>(
            (_, _) =>
            {
                var row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    [!StackPanel.OpacityProperty] = new Binding(nameof(ShapeTreeItem.IsHidden)) { Converter = hiddenToOpacity },
                };
                row.Children.Add(new Optris.Icons.Avalonia.Icon
                {
                    FontSize = 11,
                    Width = 14,
                    Opacity = 0.7,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!Optris.Icons.Avalonia.Icon.ValueProperty] = new Binding(nameof(ShapeTreeItem.IconName)),
                });
                row.Children.Add(new Border
                {
                    Width = 12,
                    Height = 12,
                    CornerRadius = new CornerRadius(3),
                    BorderBrush = UiUtil.GetBorderBrush(),
                    BorderThickness = new Thickness(1),
                    VerticalAlignment = VerticalAlignment.Center,
                    [!Border.BackgroundProperty] = new Binding(nameof(ShapeTreeItem.Swatch)),
                    [!Border.IsVisibleProperty] = new Binding(nameof(ShapeTreeItem.HasSwatch)),
                });
                row.Children.Add(new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(ShapeTreeItem.Name)),
                });
                return row;
            },
            item => item.Children);

        var treeMenu = new ContextMenu();
        treeMenu.Opening += (_, e) => OnTreeContextMenuOpening(treeMenu, e);
        treeView.ContextMenu = treeMenu;

        Grid.SetRow(treeView, 1);
        panelGrid.Children.Add(treeView);

        // Point editor panel
        var pointEditorPanel = CreatePointEditorPanel(vm);
        Grid.SetRow(pointEditorPanel, 2);
        panelGrid.Children.Add(pointEditorPanel);

        // Shape editor panel (for shape-specific actions)
        var shapeActionsPanel = CreateShapeActionsPanel(vm);
        Grid.SetRow(shapeActionsPanel, 3);
        panelGrid.Children.Add(shapeActionsPanel);

        // Layer color editor panel
        var layerEditorPanel = CreateLayerEditorPanel(vm);
        Grid.SetRow(layerEditorPanel, 4);
        panelGrid.Children.Add(layerEditorPanel);

        return new Border
        {
            Child = panelGrid,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
        };
    }

    private static Border MakePropertySection(Control content, string isVisibleProperty)
    {
        return new Border
        {
            Child = content,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(10, 8, 10, 10),
            [!Border.IsVisibleProperty] = new Binding(isVisibleProperty),
        };
    }

    private static Border CreatePointEditorPanel(AssaDrawViewModel vm)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical };
        panel.Children.Add(MakeSectionHeader(Se.Language.Assa.DrawSelectedPoint));

        // X and Y on their own rows - side by side the boxes were too narrow for 4 digits
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            RowSpacing = 6,
        };

        var xLabel = new TextBlock { Text = "X", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0), Opacity = 0.75 };
        var xBox = new NumericUpDown
        {
            Minimum = -10000,
            Maximum = 10000,
            Increment = 1,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.PointX)) { Mode = BindingMode.TwoWay },
        };
        var yLabel = new TextBlock { Text = "Y", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0), Opacity = 0.75 };
        var yBox = new NumericUpDown
        {
            Minimum = -10000,
            Maximum = 10000,
            Increment = 1,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.PointY)) { Mode = BindingMode.TwoWay },
        };
        grid.Children.Add(xLabel);
        Grid.SetColumn(xBox, 1);
        grid.Children.Add(xBox);
        Grid.SetRow(yLabel, 1);
        grid.Children.Add(yLabel);
        Grid.SetRow(yBox, 1);
        Grid.SetColumn(yBox, 1);
        grid.Children.Add(yBox);
        panel.Children.Add(grid);

        return MakePropertySection(panel, nameof(vm.IsPointSelected));
    }

    private static Border CreateShapeActionsPanel(AssaDrawViewModel vm)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 6 };
        panel.Children.Add(MakeSectionHeader(Se.Language.Assa.DrawSelectedShape));

        panel.Children.Add(new Button
        {
            Content = Se.Language.Assa.DrawChangeLayer,
            Command = vm.ChangeLayerCommand,
            HorizontalAlignment = HorizontalAlignment.Left,
        });

        panel.Children.Add(new CheckBox
        {
            Content = Se.Language.Assa.DrawUseShapeForErase,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ShapeIsEraser)) { Mode = BindingMode.TwoWay },
        });

        return MakePropertySection(panel, nameof(vm.IsShapeSelected));
    }

    private static Border CreateLayerEditorPanel(AssaDrawViewModel vm)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 6 };
        panel.Children.Add(MakeSectionHeader(Se.Language.Assa.DrawSelectedLayer));

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        row.Children.Add(UiUtil.MakeColorPickerButton(vm, nameof(vm.LayerColor)));
        row.Children.Add(new Button
        {
            Content = Se.Language.Assa.DrawChangeLayer,
            Command = vm.ChangeLayerCommand,
            VerticalAlignment = VerticalAlignment.Center,
        });
        panel.Children.Add(row);

        return MakePropertySection(panel, nameof(vm.IsLayerSelected));
    }

    private static Border CreateStatusBar(AssaDrawViewModel vm)
    {
        var statusPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18,
        };

        static StackPanel MakeStatusItem(string icon, Control text)
        {
            var item = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
            item.Children.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 11, Opacity = 0.6, VerticalAlignment = VerticalAlignment.Center });
            item.Children.Add(text);
            return item;
        }

        statusPanel.Children.Add(MakeStatusItem("fa-solid fa-crosshairs", new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PositionText)),
            VerticalAlignment = VerticalAlignment.Center,
        }));

        statusPanel.Children.Add(MakeStatusItem("fa-solid fa-magnifying-glass", new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ZoomText)),
            VerticalAlignment = VerticalAlignment.Center,
        }));

        // Current tool indicator
        var toolLabel = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
        };
        toolLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.CurrentTool))
        {
            Converter = new FuncValueConverter<DrawingTool, string>(
                tool => string.Format(Se.Language.Assa.DrawToolX, tool))
        });
        statusPanel.Children.Add(MakeStatusItem("fa-solid fa-pen-ruler", toolLabel));

        var previewStatusLabel = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PreviewStatusText)),
            Foreground = Brushes.OrangeRed,
            VerticalAlignment = VerticalAlignment.Center,
        };
        statusPanel.Children.Add(previewStatusLabel);

        // Help text
        var helpLabel = new TextBlock
        {
            Text = Se.Language.Assa.DrawHelpText,
            Foreground = UiUtil.GetTextColor(0.6),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        statusPanel.Children.Add(helpLabel);

        return new Border
        {
            Child = statusPanel,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 5),
            Margin = new Thickness(0, 0, 0, 10),
            ClipToBounds = true,
        };
    }
}
