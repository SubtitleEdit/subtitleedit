using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// ASSA draw - laid out like a vector editor: options bar on top, tool strip on the left, canvas,
/// shapes/properties panel on the right. Always dark, like most drawing programs, so the artwork
/// stands out; the colors below are the editor palette.
/// </summary>
public class AssaDrawWindow : Window
{
    private static readonly IBrush WindowBrush = new ImmutableSolidColorBrush(Color.FromRgb(30, 31, 36));
    private static readonly IBrush PanelBrush = new ImmutableSolidColorBrush(Color.FromRgb(36, 38, 44));
    private static readonly IBrush StatusBrush = new ImmutableSolidColorBrush(Color.FromRgb(27, 28, 33));
    private static readonly IBrush ButtonBarBrush = new ImmutableSolidColorBrush(Color.FromRgb(32, 34, 39));
    private static readonly IBrush LineBrush = new ImmutableSolidColorBrush(Color.FromRgb(47, 50, 58));
    private static readonly IBrush SeparatorBrush = new ImmutableSolidColorBrush(Color.FromRgb(55, 58, 67));
    private static readonly IBrush DimTextBrush = new ImmutableSolidColorBrush(Color.FromRgb(140, 146, 160));
    private static readonly IBrush FaintTextBrush = new ImmutableSolidColorBrush(Color.FromRgb(111, 117, 131));
    private static readonly IBrush IconBrush = new ImmutableSolidColorBrush(Color.FromRgb(174, 180, 194));
    private static readonly IBrush HoverBrush = new ImmutableSolidColorBrush(Color.FromRgb(51, 54, 63));
    private static readonly IBrush PressedBrush = new ImmutableSolidColorBrush(Color.FromRgb(59, 62, 71));
    private static readonly IBrush AccentBrush = new ImmutableSolidColorBrush(Color.FromRgb(76, 110, 245));
    private static readonly IBrush AccentHoverBrush = new ImmutableSolidColorBrush(Color.FromRgb(92, 124, 250));
    private static readonly IBrush AccentSoftBrush = new ImmutableSolidColorBrush(Color.FromArgb(70, 76, 110, 245));
    private static readonly IBrush FieldBrush = new ImmutableSolidColorBrush(Color.FromRgb(27, 28, 33));
    private static readonly IBrush FieldBorderBrush = new ImmutableSolidColorBrush(Color.FromRgb(54, 57, 68));
    private static readonly IBrush MenuBrush = new ImmutableSolidColorBrush(Color.FromRgb(42, 44, 51));
    private static readonly IBrush MenuBorderBrush = new ImmutableSolidColorBrush(Color.FromRgb(66, 69, 79));

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
        RequestedThemeVariant = ThemeVariant.Dark;
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
        Closing += (_, e) => vm.OnClosing();
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private void OnLoaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Setup the canvas after the window is loaded
        _vm.SetCanvas(_canvas);
        _vm.Initialize();
    }

    /// <summary>
    /// Flat icon buttons with an accent "active" state, compact number fields and accent menus.
    /// </summary>
    private static void AddEditorStyles(Styles styles)
    {
        static Style ToolPresenter(Func<Selector?, Selector> button, params (AvaloniaProperty Property, object Value)[] setters)
        {
            var style = new Style(x => button(x).Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            foreach (var (property, value) in setters)
            {
                style.Setters.Add(new Setter(property, value));
            }

            return style;
        }

        styles.Add(new Style(x => x.OfType<Button>().Class("tool"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, Brushes.Transparent),
                new Setter(Button.BorderThicknessProperty, new Thickness(0)),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(7)),
                new Setter(Button.ForegroundProperty, IconBrush),
                new Setter(Button.PaddingProperty, new Thickness(0)),
            },
        });
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, HoverBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class(":pressed"),
            (ContentPresenter.BackgroundProperty, PressedBrush)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class(":disabled"),
            (ContentPresenter.BackgroundProperty, Brushes.Transparent), (ContentPresenter.ForegroundProperty, SeparatorBrush)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class("active"),
            (ContentPresenter.BackgroundProperty, AccentBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class("active").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, AccentHoverBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));

        // Primary button (OK) in the editor accent
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("primary"),
            (ContentPresenter.BackgroundProperty, AccentBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("primary").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, AccentHoverBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));

        // Compact number fields
        styles.Add(new Style(x => x.OfType<NumericUpDown>().Class("field"))
        {
            Setters =
            {
                new Setter(NumericUpDown.ShowButtonSpinnerProperty, false),
                new Setter(NumericUpDown.MinHeightProperty, 28.0),
                new Setter(NumericUpDown.HeightProperty, 28.0),
                new Setter(NumericUpDown.FontSizeProperty, 12.5),
                new Setter(NumericUpDown.BackgroundProperty, FieldBrush),
                new Setter(NumericUpDown.BorderBrushProperty, FieldBorderBrush),
                new Setter(NumericUpDown.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        styles.Add(new Style(x => x.OfType<NumericUpDown>().Class("field").Template().OfType<TextBox>())
        {
            Setters =
            {
                new Setter(TextBox.MinHeightProperty, 0.0),
                new Setter(TextBox.HeightProperty, 28.0),
                new Setter(TextBox.PaddingProperty, new Thickness(6, 0)),
                new Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center),
                new Setter(TextBox.BackgroundProperty, FieldBrush),
            },
        });

        // Menus: rounded, darker, accent highlight
        foreach (var menuType in new[] { typeof(ContextMenu), typeof(MenuFlyoutPresenter) })
        {
            styles.Add(new Style(x => x.OfType(menuType))
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, MenuBrush),
                    new Setter(TemplatedControl.BorderBrushProperty, MenuBorderBrush),
                    new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(9)),
                    new Setter(TemplatedControl.PaddingProperty, new Thickness(4)),
                },
            });
        }

        styles.Add(new Style(x => x.OfType<MenuItem>())
        {
            Setters = { new Setter(MenuItem.CornerRadiusProperty, new CornerRadius(5)) },
        });
        styles.Add(new Style(x => x.OfType<MenuItem>().Class(":pointerover").Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters = { new Setter(Border.BackgroundProperty, AccentBrush) },
        });
        styles.Add(new Style(x => x.OfType<MenuItem>().Class(":open").Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters = { new Setter(Border.BackgroundProperty, AccentBrush) },
        });

        // Tree: rounded rows, soft accent selection
        styles.Add(new Style(x => x.OfType<TreeViewItem>().Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters =
            {
                new Setter(Border.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(Border.MinHeightProperty, 28.0),
            },
        });
        styles.Add(new Style(x => x.OfType<TreeViewItem>().Class(":selected").Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters = { new Setter(Border.BackgroundProperty, AccentSoftBrush) },
        });
        styles.Add(new Style(x => x.OfType<TreeViewItem>().Class(":selected").Class(":pointerover").Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters = { new Setter(Border.BackgroundProperty, AccentSoftBrush) },
        });
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
        var background = MakeMenuItem(Se.Language.Assa.DrawBackground, "fa-regular fa-image", null);
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
        var backgroundButton = CreateToolButton("fa-regular fa-image", Se.Language.Assa.DrawBackground, null);
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
            Background = PanelBrush,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 6),
        };
    }

    private static TextBlock MakeDimLabel(string text, Thickness margin)
    {
        return new TextBlock
        {
            Text = text,
            Foreground = DimTextBrush,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = margin,
        };
    }

    /// <summary>
    /// Compact number field without spinner buttons, optionally with a dim prefix like "X".
    /// </summary>
    private static NumericUpDown MakeField(string property, double minimum, double maximum, double width = double.NaN, string? prefix = null, string format = "0")
    {
        var field = new NumericUpDown
        {
            Minimum = (decimal)minimum,
            Maximum = (decimal)maximum,
            Increment = 1,
            Width = width,
            FormatString = format,
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        field.Classes.Add("field");
        if (prefix != null)
        {
            field.InnerLeftContent = new TextBlock
            {
                Text = prefix,
                Foreground = FaintTextBrush,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
            };
        }

        return field;
    }

    private static Border MakeToolbarSeparator()
    {
        return new Border
        {
            Width = 1,
            Height = 22,
            Background = SeparatorBrush,
            Margin = new Thickness(8, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    private static Button CreateToolButton(string icon, string tooltip, System.Windows.Input.ICommand? command, double size = 30, string? shortcut = null)
    {
        Control content = new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = size > 34 ? 15 : 13 };
        if (shortcut != null)
        {
            // Shortcut letter in the corner, like the tool palettes of drawing programs
            var grid = new Grid { Width = size, Height = size };
            content.HorizontalAlignment = HorizontalAlignment.Center;
            content.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(content);
            grid.Children.Add(new TextBlock
            {
                Text = shortcut,
                FontSize = 8,
                Opacity = 0.6,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 4, 2),
            });
            content = grid;
        }

        var button = new Button
        {
            Content = content,
            Width = size,
            Height = size,
            Command = command,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");
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

        void AddTool(string icon, string tooltip, System.Windows.Input.ICommand command, DrawingTool tool, string shortcut)
        {
            var button = CreateToolButton(icon, tooltip + " (" + shortcut + ")", command, 38, shortcut);
            BindHighlight(button, vm, () => vm.CurrentTool == tool, nameof(vm.CurrentTool));
            panel.Children.Add(button);
        }

        AddTool("fa-solid fa-arrow-pointer", Se.Language.Assa.DrawSelectTool, vm.SelectToolCommand, DrawingTool.Select, "V");
        panel.Children.Add(MakeStripSeparator());
        AddTool("fa-solid fa-pen-nib", Se.Language.Assa.DrawLineTool, vm.LineToolCommand, DrawingTool.Line, "L");
        AddTool("fa-solid fa-bezier-curve", Se.Language.Assa.DrawBezierTool, vm.BezierToolCommand, DrawingTool.Bezier, "B");
        AddTool("fa-regular fa-square", Se.Language.Assa.DrawRectangleTool, vm.RectangleToolCommand, DrawingTool.Rectangle, "R");
        AddTool("fa-regular fa-circle", Se.Language.Assa.DrawCircleTool, vm.CircleToolCommand, DrawingTool.Circle, "C");

        // Shape tool: picks a shape from the library palette that opens next to the strip
        var shapeButton = CreateToolButton("fa-solid fa-shapes", Se.Language.Assa.DrawShapeTool + " (S)", null, 38, "S");
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
            BorderBrush = Brushes.White,
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
                FontSize = 12,
                Foreground = DimTextBrush,
                Margin = new Thickness(2, 6, 0, 0),
            });

            var wrap = new WrapPanel { Width = columns * (cell + 4) };
            foreach (var item in list)
            {
                var path = new Avalonia.Controls.Shapes.Path
                {
                    Data = item.ToGeometry(),
                    Fill = item.UserShape != null ? null : new SolidColorBrush(Color.FromRgb(214, 218, 226)),
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(9),
                };
                if (item.UserShape != null)
                {
                    // Saved shapes show their own (first) color
                    var color = item.Template.FirstOrDefault(s => !s.IsEraser)?.ForeColor ?? Colors.White;
                    path.Fill = new SolidColorBrush(color);
                }

                var button = new Button
                {
                    Content = path,
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
            FontSize = 11.5,
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

    private static TextBlock MakeSectionHeader(string text)
    {
        return new TextBlock
        {
            Text = text.ToUpperInvariant(),
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            LetterSpacing = 0.6,
            Foreground = DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
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
                    FontSize = 11,
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

        var shapePanel = CreateShapePropertiesPanel(vm);
        Grid.SetRow(shapePanel, 2);
        panelGrid.Children.Add(shapePanel);

        var layerPanel = CreateLayerPropertiesPanel(vm);
        Grid.SetRow(layerPanel, 3);
        panelGrid.Children.Add(layerPanel);

        var pointPanel = CreatePointPropertiesPanel(vm);
        Grid.SetRow(pointPanel, 4);
        panelGrid.Children.Add(pointPanel);

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

    /// <summary>
    /// A "Label  [field] [field]" row of the properties panel.
    /// </summary>
    private static Grid MakePropertyRow(string label, params Control[] fields)
    {
        var grid = new Grid { ColumnSpacing = 6 };
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(70)));
        grid.Children.Add(MakeDimLabel(label, new Thickness(0)));
        for (var i = 0; i < fields.Length; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            Grid.SetColumn(fields[i], i + 1);
            grid.Children.Add(fields[i]);
        }

        return grid;
    }

    private static Border CreateShapePropertiesPanel(AssaDrawViewModel vm)
    {
        var rows = new StackPanel { Spacing = 6 };
        rows.Children.Add(MakePropertyRow(Se.Language.Assa.DrawPosition,
            MakeField(nameof(vm.ShapeX), -10000, 10000, prefix: "X", format: "0.#"),
            MakeField(nameof(vm.ShapeY), -10000, 10000, prefix: "Y", format: "0.#")));
        rows.Children.Add(MakePropertyRow(Se.Language.Assa.DrawSize,
            MakeField(nameof(vm.ShapeWidth), 0, 10000, prefix: "W", format: "0.#"),
            MakeField(nameof(vm.ShapeHeight), 0, 10000, prefix: "H", format: "0.#")));

        var colorButton = UiUtil.MakeColorPickerButton(vm, nameof(vm.LayerColor));
        colorButton.HorizontalAlignment = HorizontalAlignment.Left;
        rows.Children.Add(MakePropertyRow(Se.Language.Assa.DrawLayer,
            MakeField(nameof(vm.ShapeLayer), 0, 1000),
            colorButton));

        var eraser = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(nameof(vm.ShapeIsEraser)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(eraser, Se.Language.Assa.DrawUseShapeForErase);
        var eraserRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        eraserRow.Children.Add(eraser);
        eraserRow.Children.Add(new TextBlock { Text = Se.Language.Assa.DrawUseShapeForErase, VerticalAlignment = VerticalAlignment.Center, FontSize = 12.5 });
        rows.Children.Add(eraserRow);

        return MakePropertySection(Se.Language.Assa.DrawSelectedShape, rows, nameof(vm.IsShapeSelected));
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
        return MakePropertySection(Se.Language.Assa.DrawSelectedLayer, row, nameof(vm.IsLayerSelected));
    }

    private static Border CreatePointPropertiesPanel(AssaDrawViewModel vm)
    {
        var row = MakePropertyRow(Se.Language.Assa.DrawPosition,
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
            FontSize = 12,
            Foreground = DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(property),
        };

        left.Children.Add(MakeStatusItem("fa-solid fa-crosshairs", StatusText(nameof(vm.PositionText))));

        var toolLabel = StatusText(nameof(vm.CurrentTool));
        toolLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.CurrentTool))
        {
            Converter = new FuncValueConverter<DrawingTool, string>(tool => string.Format(Se.Language.Assa.DrawToolX, tool)),
        });
        left.Children.Add(MakeStatusItem("fa-solid fa-pen-ruler", toolLabel));

        var selection = MakeStatusItem("fa-solid fa-draw-polygon", StatusText(nameof(vm.SelectionText)));
        selection.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.SelectionText)) { Converter = StringConverters.IsNotNullOrEmpty });
        left.Children.Add(selection);

        left.Children.Add(new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PreviewStatusText)),
            Foreground = Brushes.OrangeRed,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
        });

        var help = new TextBlock
        {
            Text = Se.Language.Assa.DrawHelpText,
            Foreground = FaintTextBrush,
            FontSize = 12,
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
            FontSize = 12,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ZoomPercent)) { StringFormat = "{0:0}%" },
        });
        var fit = new Button
        {
            Content = WithoutShortcut(Se.Language.Assa.DrawResetView),
            Command = vm.ResetViewCommand,
            FontSize = 12,
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
            FontSize = 12,
            Foreground = FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 0, 16, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.CodePreview)),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
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
