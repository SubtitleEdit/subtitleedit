using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

public class AssaDrawWindow : Window
{
    private readonly AssaDrawViewModel _vm;
    private readonly AssaDrawCanvas _canvas;

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

        // Toolbar
        var toolbar = CreateToolbar(vm);
        Grid.SetRow(toolbar, 0);
        mainGrid.Children.Add(toolbar);

        // Main content area with canvas and side panel
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

    private static Border CreateToolbar(AssaDrawViewModel vm)
    {
        var toolbarPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
        };

        // File actions
        var saveButton = CreateToolButton("fa-solid fa-floppy-disk", "Save", vm.SaveCommand);
        var loadButton = CreateToolButton("fa-solid fa-folder-open", "Load", vm.LoadCommand);
        var copyButton = CreateToolButton("fa-solid fa-copy", Se.Language.Assa.DrawCopyToClipboard, vm.CopyToClipboardCommand);

        toolbarPanel.Children.Add(saveButton);
        toolbarPanel.Children.Add(loadButton);
        toolbarPanel.Children.Add(copyButton);

        toolbarPanel.Children.Add(new Separator { Width = 2, Margin = new Thickness(5, 2) });

        // Drawing tools
        var selectButton = CreateToolButton("fa-solid fa-arrow-pointer", Se.Language.Assa.DrawSelectTool, vm.SelectToolCommand);
        var lineButton = CreateToolButton("fa-solid fa-pen", Se.Language.Assa.DrawLineTool, vm.LineToolCommand);
        var bezierButton = CreateToolButton("fa-solid fa-bezier-curve", Se.Language.Assa.DrawBezierTool, vm.BezierToolCommand);
        var rectButton = CreateToolButton("fa-regular fa-square", Se.Language.Assa.DrawRectangleTool, vm.RectangleToolCommand);
        var circleButton = CreateToolButton("fa-regular fa-circle", Se.Language.Assa.DrawCircleTool, vm.CircleToolCommand);

        toolbarPanel.Children.Add(selectButton);
        toolbarPanel.Children.Add(lineButton);
        toolbarPanel.Children.Add(bezierButton);
        toolbarPanel.Children.Add(rectButton);
        toolbarPanel.Children.Add(circleButton);

        toolbarPanel.Children.Add(new Separator { Width = 2, Margin = new Thickness(5, 2) });

        // Shape actions
        var closeShapeButton = CreateToolButton("fa-solid fa-check", Se.Language.Assa.DrawCloseShape, vm.CloseShapeCommand);
        var deleteShapeButton = CreateToolButton("fa-solid fa-trash", Se.Language.Assa.DrawDeleteShape, vm.DeleteShapeCommand);
        var clearAllButton = CreateToolButton("fa-solid fa-eraser", Se.Language.Assa.DrawClearAll, vm.ClearAllCommand);

        toolbarPanel.Children.Add(closeShapeButton);
        toolbarPanel.Children.Add(deleteShapeButton);
        toolbarPanel.Children.Add(clearAllButton);

        toolbarPanel.Children.Add(new Separator { Width = 2, Margin = new Thickness(5, 2) });

        // View actions
        var zoomInButton = CreateToolButton("fa-solid fa-magnifying-glass-plus", Se.Language.Assa.DrawZoomIn, vm.ZoomInCommand);
        var zoomOutButton = CreateToolButton("fa-solid fa-magnifying-glass-minus", Se.Language.Assa.DrawZoomOut, vm.ZoomOutCommand);
        var resetViewButton = CreateToolButton("fa-solid fa-expand", Se.Language.Assa.DrawResetView, vm.ResetViewCommand);
        var toggleGridButton = CreateToolButton("fa-solid fa-border-all", Se.Language.Assa.DrawToggleGrid, vm.ToggleGridCommand);

        toolbarPanel.Children.Add(zoomInButton);
        toolbarPanel.Children.Add(zoomOutButton);
        toolbarPanel.Children.Add(resetViewButton);
        toolbarPanel.Children.Add(toggleGridButton);

        toolbarPanel.Children.Add(new Separator { Width = 2, Margin = new Thickness(5, 2) });

        var widthLabel = new TextBlock
        {
            Text = Se.Language.General.Width,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0),
        };
        var widthBox = new NumericUpDown
        {
            Minimum = 125,
            Maximum = 4096,
            Width = 125,
            Increment = 10,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.CanvasWidth)) { Mode = BindingMode.TwoWay },
        };

        var heightLabel = new TextBlock
        {
            Text = Se.Language.General.Height,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0),
        };
        var heightBox = new NumericUpDown
        {
            Minimum = 125,
            Maximum = 4096,
            Width = 125,
            Increment = 10,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.CanvasHeight)) { Mode = BindingMode.TwoWay },
        };

        toolbarPanel.Children.Add(widthLabel);
        toolbarPanel.Children.Add(widthBox);
        toolbarPanel.Children.Add(heightLabel);
        toolbarPanel.Children.Add(heightBox);

        return new Border
        {
            Child = toolbarPanel,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(5),
            Margin = new Thickness(0, 0, 0, 10),
        };
    }

    private static Button CreateToolButton(string icon, string tooltip, System.Windows.Input.ICommand command)
    {
        var button = new Button
        {
            Content = new Projektanker.Icons.Avalonia.Icon { Value = icon },
            Width = 32,
            Height = 32,
            Command = command,
            Padding = new Thickness(4),
        };
        ToolTip.SetTip(button, tooltip);
        return button;
    }

    private static Grid CreateContentArea(AssaDrawViewModel vm, out AssaDrawCanvas canvas)
    {
        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(250, GridUnitType.Pixel) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(0, 0, 0, 10),
        };

        // Drawing canvas
        var canvasBorder = new Border
        {
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            ClipToBounds = true,
            Margin = new Thickness(0, 0, 10, 0),
        };

        canvas = new AssaDrawCanvas();
        canvasBorder.Child = canvas;

        Grid.SetColumn(canvasBorder, 0);
        contentGrid.Children.Add(canvasBorder);

        // Side panel
        var sidePanel = CreateSidePanel(vm);
        Grid.SetColumn(sidePanel, 1);
        contentGrid.Children.Add(sidePanel);

        return contentGrid;
    }

    private static Border CreateSidePanel(AssaDrawViewModel vm)
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
        var headerLabel = new TextBlock
        {
            Text = Se.Language.Assa.DrawShapes,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(5),
        };
        Grid.SetRow(headerLabel, 0);
        panelGrid.Children.Add(headerLabel);

        // Tree view for shapes
        var treeView = new TreeView
        {
            [!TreeView.ItemsSourceProperty] = new Binding(nameof(vm.ShapeTreeItems)),
            Margin = new Thickness(5),
        };

        // Configure the tree view to display hierarchical data
        treeView.ItemTemplate = new FuncTreeDataTemplate<ShapeTreeItem>(
            (item, _) => new TextBlock { [!TextBlock.TextProperty] = new Binding(nameof(ShapeTreeItem.Name)) },
            item => item.Children
        );

        // Wire up selection changed event
        treeView.SelectionChanged += (sender, e) =>
        {
            if (treeView.SelectedItem is ShapeTreeItem selectedItem)
            {
                vm.SelectedTreeItem = selectedItem;

                // Update point editor if a point is selected
                if (selectedItem.Point != null)
                {
                    vm.ActivePoint = selectedItem.Point;
                    vm.PointX = selectedItem.Point.X;
                    vm.PointY = selectedItem.Point.Y;
                    vm.IsPointSelected = true;
                }
                else
                {
                    vm.ActivePoint = null;
                    vm.IsPointSelected = false;
                }

                // Update active shape for canvas rendering and shape properties
                if (selectedItem.Shape != null)
                {
                    vm.ActiveShape = selectedItem.Shape;
                    vm.ShapeIsEraser = selectedItem.Shape.IsEraser;
                }
            }
        };

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
            CornerRadius = new CornerRadius(4),
        };
    }

    private static StackPanel CreatePointEditorPanel(AssaDrawViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(5),
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.IsPointSelected)),
        };

        var headerLabel = new TextBlock
        {
            Text = Se.Language.Assa.DrawSelectedPoint,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        panel.Children.Add(headerLabel);

        var xPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2) };
        xPanel.Children.Add(new TextBlock { Text = "X:", Width = 30, VerticalAlignment = VerticalAlignment.Center });
        var xBox = new NumericUpDown
        {
            Minimum = -10000,
            Maximum = 10000,
            Width = 120,
            Increment = 1,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.PointX)) { Mode = BindingMode.TwoWay },
        };
        xPanel.Children.Add(xBox);
        panel.Children.Add(xPanel);

        var yPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2) };
        yPanel.Children.Add(new TextBlock { Text = "Y:", Width = 30, VerticalAlignment = VerticalAlignment.Center });
        var yBox = new NumericUpDown
        {
            Minimum = -10000,
            Maximum = 10000,
            Width = 120,
            Increment = 1,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.PointY)) { Mode = BindingMode.TwoWay },
        };
        yPanel.Children.Add(yBox);
        panel.Children.Add(yPanel);

        return panel;
    }

    private static StackPanel CreateShapeActionsPanel(AssaDrawViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(5),
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.IsShapeSelected)),
        };

        var headerLabel = new TextBlock
        {
            Text = Se.Language.Assa.DrawSelectedShape,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        panel.Children.Add(headerLabel);

        var changeLayerButton = new Button
        {
            Content = Se.Language.Assa.DrawChangeLayer,
            Command = vm.ChangeLayerCommand,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        panel.Children.Add(changeLayerButton);

        var isEraserCheckBox = new CheckBox
        {
            Content = "Use shape for erase (iclip)",
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ShapeIsEraser)) { Mode = BindingMode.TwoWay },
            Margin = new Thickness(0, 5, 0, 0),
        };
        panel.Children.Add(isEraserCheckBox);

        return panel;
    }

    private static StackPanel CreateLayerEditorPanel(AssaDrawViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(5),
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.IsLayerSelected)),
        };

        var headerLabel = new TextBlock
        {
            Text = Se.Language.Assa.DrawSelectedLayer,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        panel.Children.Add(headerLabel);

        var changeLayerButton = new Button
        {
            Content = Se.Language.Assa.DrawChangeLayer,
            Command = vm.ChangeLayerCommand,
            Margin = new Thickness(0, 0, 0, 10),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        panel.Children.Add(changeLayerButton);

        var colorPicker = new ColorPicker
        {
            Width = 200,
            IsAlphaEnabled = true,
            IsAlphaVisible = true,
            IsColorSpectrumSliderVisible = false,
            IsColorComponentsVisible = true,
            IsColorModelVisible = false,
            IsColorPaletteVisible = false,
            IsAccentColorsVisible = false,
            IsColorSpectrumVisible = true,
            IsComponentTextInputVisible = true,
            [!ColorPicker.ColorProperty] = new Binding(nameof(vm.LayerColor))
            {
                Source = vm,
                Mode = BindingMode.TwoWay
            },
        };
        panel.Children.Add(colorPicker);

        return panel;
    }

    private static Border CreateStatusBar(AssaDrawViewModel vm)
    {
        var statusPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 20,
        };

        var positionLabel = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PositionText)),
            VerticalAlignment = VerticalAlignment.Center,
        };
        statusPanel.Children.Add(positionLabel);

        var zoomLabel = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ZoomText)),
            VerticalAlignment = VerticalAlignment.Center,
        };
        statusPanel.Children.Add(zoomLabel);

        // Current tool indicator
        var toolLabel = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
        };
        toolLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.CurrentTool))
        {
            Converter = new Avalonia.Data.Converters.FuncValueConverter<DrawingTool, string>(
                tool => string.Format(Se.Language.Assa.DrawToolX, tool))
        });
        statusPanel.Children.Add(toolLabel);

        // Help text
        var helpLabel = new TextBlock
        {
            Text = Se.Language.Assa.DrawHelpText,
            Foreground = UiUtil.GetTextColor(0.6),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20, 0, 0, 0),
        };
        statusPanel.Children.Add(helpLabel);

        return new Border
        {
            Child = statusPanel,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(10, 5),
            Margin = new Thickness(0, 0, 0, 10),
        };
    }
}
