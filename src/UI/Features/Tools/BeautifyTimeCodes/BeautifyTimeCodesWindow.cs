using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes;

public class BeautifyTimeCodesWindow : Window
{
    public BeautifyTimeCodesWindow(BeautifyTimeCodesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BeautifyTimeCodes.Title;
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 900;
        MinHeight = 500;
        vm.Window = this;
        DataContext = vm;

        // Settings panel - Row 1
        var checkBoxSnapToFrames = new CheckBox
        {
            Content = Se.Language.Tools.BeautifyTimeCodes.SnapToFrames,
            VerticalAlignment = VerticalAlignment.Center,
            [!CheckBox.IsCheckedProperty] = new Avalonia.Data.Binding($"{nameof(BeautifyTimeCodesViewModel.Settings)}.{nameof(BeautifySettings.SnapToFrames)}") { Source = vm, Mode = BindingMode.TwoWay },
        };

        var labelFrameGap = new TextBlock
        {
            Text = Se.Language.Tools.BeautifyTimeCodes.FrameGap,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var numericFrameGap = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 10,
            Increment = 1,
            FormatString = "0",
            Width = 120,
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Avalonia.Data.Binding($"{nameof(BeautifyTimeCodesViewModel.Settings)}.{nameof(BeautifySettings.FrameGap)}") { Source = vm, Mode = BindingMode.TwoWay },
        };
        numericFrameGap.ValueChanged += vm.ValueChanged;

        var labelMinDuration = new TextBlock
        {
            Text = Se.Language.Tools.BeautifyTimeCodes.MinDuration,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var numericMinDuration = new NumericUpDown
        {
            Minimum = 100,
            Maximum = 10000,
            Increment = 100,
            FormatString = "0",
            Width = 120,
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Avalonia.Data.Binding($"{nameof(BeautifyTimeCodesViewModel.Settings)}.{nameof(BeautifySettings.MinDurationMs)}") { Source = vm, Mode = BindingMode.TwoWay },
        };
        numericMinDuration.ValueChanged += vm.ValueChanged;

        // Settings panel - Row 2
        var labelShotChangeThreshold = new TextBlock
        {
            Text = Se.Language.Tools.BeautifyTimeCodes.ShotChangeThreshold,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var numericShotChangeThreshold = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1000,
            Increment = 10,
            FormatString = "0",
            Width = 120,
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Avalonia.Data.Binding($"{nameof(BeautifyTimeCodesViewModel.Settings)}.{nameof(BeautifySettings.ShotChangeThresholdMs)}") { Source = vm, Mode = BindingMode.TwoWay },
        };
        numericShotChangeThreshold.ValueChanged += vm.ValueChanged;

        var labelShotChangeOffset = new TextBlock
        {
            Text = Se.Language.Tools.BeautifyTimeCodes.ShotChangeOffset,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var numericShotChangeOffset = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 10,
            Increment = 1,
            FormatString = "0",
            Width = 120,
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Avalonia.Data.Binding($"{nameof(BeautifyTimeCodesViewModel.Settings)}.{nameof(BeautifySettings.ShotChangeOffsetFrames)}") { Source = vm, Mode = BindingMode.TwoWay },
        };
        numericShotChangeOffset.ValueChanged += vm.ValueChanged;

        var settingsGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,12,Auto,12,Auto"),
            ColumnDefinitions = new ColumnDefinitions("Auto,10,120,30,Auto,10,120"),
            RowSpacing = 0,
        };

        // Row 0: Checkbox (spans 3 columns)
        settingsGrid.Add(checkBoxSnapToFrames, 0, 0, 1, 3);

        // Row 2: Frame Gap and Min Duration
        settingsGrid.Add(labelFrameGap, 2, 0);
        settingsGrid.Add(numericFrameGap, 2, 2);
        settingsGrid.Add(labelMinDuration, 2, 4);
        settingsGrid.Add(numericMinDuration, 2, 6);

        // Row 4: Shot Change Threshold and Shot Change Offset
        settingsGrid.Add(labelShotChangeThreshold, 4, 0);
        settingsGrid.Add(numericShotChangeThreshold, 4, 2);
        settingsGrid.Add(labelShotChangeOffset, 4, 4);
        settingsGrid.Add(numericShotChangeOffset, 4, 6);

        var settingsTitle = new TextBlock
        {
            Text = Se.Language.Tools.BeautifyTimeCodes.BeautifySettings,
            FontWeight = FontWeight.Bold,
            FontSize = 14,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var settingsPanel = new StackPanel
        {
            Spacing = 10,
        };
        settingsPanel.Children.Add(settingsTitle);
        settingsPanel.Children.Add(settingsGrid);

        var settingsBorder = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Avalonia.Thickness(1),
            Padding = new Avalonia.Thickness(10),
            CornerRadius = new Avalonia.CornerRadius(4),
            Child = settingsPanel,
        };

        // Audio visualizers
        var labelOriginal = new TextBlock
        {
            Text = Se.Language.Tools.BeautifyTimeCodes.Original,
            FontWeight = FontWeight.Bold,
            Margin = new Avalonia.Thickness(5),
        };

        var audioVisualizerOriginal = new Controls.AudioVisualizerControl.AudioVisualizer
        {
            IsReadOnly = true,
            DrawGridLines = true,
        };

        var borderOriginal = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Avalonia.Thickness(1),
            Background = Brushes.Black,
            Child = audioVisualizerOriginal,
        };

        var labelBeautified = new TextBlock
        {
            Text = Se.Language.Tools.BeautifyTimeCodes.Beautified,
            FontWeight = FontWeight.Bold,
            Margin = new Avalonia.Thickness(5),
        };

        var audioVisualizerBeautified = new Controls.AudioVisualizerControl.AudioVisualizer
        {
            IsReadOnly = true,
            DrawGridLines = true,
        };

        var borderBeautified = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Avalonia.Thickness(1),
            Background = Brushes.Black,
            Child = audioVisualizerBeautified,
        };

        // Set up visualizers in ViewModel
        vm.AudioVisualizerOriginal = audioVisualizerOriginal;
        vm.AudioVisualizerBeautified = audioVisualizerBeautified;

        // Sync scroll and zoom changes from original to beautified
        audioVisualizerOriginal.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name == "StartPositionSeconds")
                audioVisualizerBeautified.StartPositionSeconds = audioVisualizerOriginal.StartPositionSeconds;
            else if (e.Property.Name == "ZoomFactor")
                audioVisualizerBeautified.ZoomFactor = audioVisualizerOriginal.ZoomFactor;
            else if (e.Property.Name == "VerticalZoomFactor")
                audioVisualizerBeautified.VerticalZoomFactor = audioVisualizerOriginal.VerticalZoomFactor;
        };

        var visualizerGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,5*,10,Auto,5*"),
            ColumnDefinitions = new ColumnDefinitions("*"),
        };
        visualizerGrid.Add(labelOriginal, 0, 0);
        visualizerGrid.Add(borderOriginal, 1, 0);
        visualizerGrid.Add(labelBeautified, 3, 0);
        visualizerGrid.Add(borderBeautified, 4, 0);

        // Buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        // Main grid
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
        };
        grid.Add(settingsBorder, 0, 0);
        grid.Add(visualizerGrid, 1, 0);
        grid.Add(panelButtons, 2, 0);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Closing += (_, __) => vm.Dispose();
    }
}
