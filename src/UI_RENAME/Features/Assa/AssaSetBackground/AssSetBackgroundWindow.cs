using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaSetBackground;

public class AssSetBackgroundWindow : Window
{
    public AssSetBackgroundWindow(AssSetBackgroundViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.BackgroundBoxGenerator;
        CanResize = true;
        Width = 1100;
        Height = 700;
        MinWidth = 900;
        MinHeight = 650;

        vm.Window = this;
        DataContext = vm;

        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = false;
        var videoPanel = UiUtil.MakeBorderForControl(vm.VideoPlayerControl);

        var leftPanel = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            RowSpacing = 12,
        };

        // Padding settings
        leftPanel.Add(CreatePaddingPanel(vm), 0);

        // Fill width settings
        leftPanel.Add(CreateFillWidthPanel(vm), 1);

        // Style settings
        leftPanel.Add(CreateStylePanel(vm), 2);

        // Colors
        leftPanel.Add(CreateColorsPanel(vm), 3);

        var mainGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 12,
        };

        // Progress text
        var labelProgress = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.ProgressText));

        // Buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        mainGrid.Add(leftPanel, 0, 0);
        mainGrid.Add(videoPanel, 0, 1);
        mainGrid.Add(labelProgress, 1, 0, 1, 2);
        mainGrid.Add(panelButtons, 1, 0, 1, 2);

        Content = mainGrid;

        Activated += delegate { buttonOk.Focus(); };
        AddHandler(KeyDownEvent, vm.KeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        Loaded += (_, _) => vm.OnLoaded();
        Closing += (_, _) => vm.OnClosing();
    }

    private static Border CreatePaddingPanel(AssSetBackgroundViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            RowSpacing = 8,
            ColumnSpacing = 10,
        };

        var titleLabel = new TextBlock
        {
            Text = Se.Language.Assa.BackgroundBoxPadding,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        grid.Add(titleLabel, 0, 0, 1, 4);

        // Left
        var leftLabel = new TextBlock { Text = Se.Language.General.Left, VerticalAlignment = VerticalAlignment.Center };
        grid.Add(leftLabel, 1, 0);

        var leftBox = UiUtil.MakeNumericUpDownInt(0, 500, 0, 120, vm, nameof(vm.PaddingLeft));
        grid.Add(leftBox, 1, 1);

        // Right
        var rightLabel = new TextBlock { Text = Se.Language.General.Right, VerticalAlignment = VerticalAlignment.Center };
        grid.Add(rightLabel, 1, 2);

        var rightBox = UiUtil.MakeNumericUpDownInt(0, 500, 0, 120, vm, nameof(vm.PaddingRight));
        grid.Add(rightBox, 1, 3);

        // Top
        var topLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarTop, VerticalAlignment = VerticalAlignment.Center };
        grid.Add(topLabel, 2, 0);

        var topBox = UiUtil.MakeNumericUpDownInt(0, 500, 0, 120, vm, nameof(vm.PaddingTop));
        grid.Add(topBox, 2, 1);

        // Bottom
        var bottomLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarBottom, VerticalAlignment = VerticalAlignment.Center };
        grid.Add(bottomLabel, 2, 2);

        var bottomBox = UiUtil.MakeNumericUpDownInt(0, 500, 0, 120, vm, nameof(vm.PaddingBottom));
        grid.Add(bottomBox, 2, 3);

        return new Border
        {
            Child = grid,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
        };
    }

    private static Border CreateFillWidthPanel(AssSetBackgroundViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            RowSpacing = 8,
            ColumnSpacing = 10,
        };

        var checkBox = new CheckBox
        {
            Content = Se.Language.Assa.BackgroundBoxFillWidth,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.FillWidth)) { Mode = BindingMode.TwoWay },
            FontWeight = FontWeight.Bold,
        };
        grid.Add(checkBox, 0, 0, 1, 4);

        // Margin Left
        var leftLabel = new TextBlock { Text = Se.Language.General.Left, VerticalAlignment = VerticalAlignment.Center };
        grid.Add(leftLabel, 1, 0);

        var leftBox = UiUtil.MakeNumericUpDownInt(0, 500, 0, 120, vm, nameof(vm.FillWidthMarginLeft));
        leftBox.Bind(NumericUpDown.IsEnabledProperty, new Binding(nameof(vm.FillWidth)));
        grid.Add(leftBox, 1, 1);

        // Margin Right
        var rightLabel = new TextBlock { Text = Se.Language.General.Right, VerticalAlignment = VerticalAlignment.Center };
        grid.Add(rightLabel, 1, 2);

        var rightBox = UiUtil.MakeNumericUpDownInt(0, 500, 0, 120, vm, nameof(vm.FillWidthMarginRight));
        rightBox.Bind(NumericUpDown.IsEnabledProperty, new Binding(nameof(vm.FillWidth)));
        grid.Add(rightBox, 1, 3);

        return new Border
        {
            Child = grid,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
        };
    }

    private static Border CreateStylePanel(AssSetBackgroundViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 8,
            ColumnSpacing = 10,
        };

        var titleLabel = new TextBlock
        {
            Text = Se.Language.General.Style,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        grid.Add(titleLabel, 0, 0, 1, 2);

        // Box style
        var styleLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarStyle, VerticalAlignment = VerticalAlignment.Center };
        grid.Add(styleLabel, 1, 0);

        var styleCombo = new ComboBox
        {
            MinWidth = 150,
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.BoxStyles)),
            [!ComboBox.SelectedIndexProperty] = new Binding(nameof(vm.BoxStyleIndex)) { Mode = BindingMode.TwoWay },
        };
        grid.Add(styleCombo, 1, 1);

        // Radius (for rounded corners)
        var radiusLabel = new TextBlock { Text = Se.Language.Assa.BackgroundBoxRadius, VerticalAlignment = VerticalAlignment.Center };
        grid.Add(radiusLabel, 2, 0);

        var radiusBox = UiUtil.MakeNumericUpDownInt(0, 200, 0, 120, vm, nameof(vm.Radius));
        grid.Add(radiusBox, 2, 1);

        // Outline width
        var outlineLabel = new TextBlock { Text = Se.Language.General.Outline, VerticalAlignment = VerticalAlignment.Center };
        grid.Add(outlineLabel, 3, 0);

        var outlineBox = UiUtil.MakeNumericUpDownInt(0, 20, 0, 120, vm, nameof(vm.OutlineWidth));
        grid.Add(outlineBox, 3, 1);

        return new Border
        {
            Child = grid,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
        };
    }

    private static Border CreateColorsPanel(AssSetBackgroundViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 8,
            ColumnSpacing = 10,
        };

        var titleLabel = new TextBlock
        {
            Text = Se.Language.General.Color,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        grid.Add(titleLabel, 0, 0, 1, 2);

        // Box color
        var boxLabel = new TextBlock { Text = Se.Language.Assa.BackgroundBoxBoxColor, VerticalAlignment = VerticalAlignment.Center };
        grid.Add(boxLabel, 1, 0);

        var boxPicker = UiUtil.MakeColorPicker(vm, nameof(vm.BoxColor));
        boxPicker.HorizontalAlignment = HorizontalAlignment.Left;
        grid.Add(boxPicker, 1, 1);

        // Outline color
        var outlineLabel = new TextBlock { Text = Se.Language.General.Outline, VerticalAlignment = VerticalAlignment.Center };
        grid.Add(outlineLabel, 2, 0);

        var outlinePicker = UiUtil.MakeColorPicker(vm, nameof(vm.OutlineColor));
        outlinePicker.HorizontalAlignment = HorizontalAlignment.Left;
        grid.Add(outlinePicker, 2, 1);

        // Shadow color
        var shadowLabel = new TextBlock { Text = Se.Language.General.Shadow, VerticalAlignment = VerticalAlignment.Center };
        grid.Add(shadowLabel, 3, 0);

        var shadowPicker = UiUtil.MakeColorPicker(vm, nameof(vm.ShadowColor));
        shadowPicker.HorizontalAlignment = HorizontalAlignment.Left;
        grid.Add(shadowPicker, 3, 1);

        return new Border
        {
            Child = grid,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
        };
    }
}
