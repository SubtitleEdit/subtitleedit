using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaProgressBar;

public class AssaProgressBarWindow : Window
{
    public AssaProgressBarWindow(AssaProgressBarViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.ProgressBarTitle;
        CanResize = true;
        Width = 1100;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;

        vm.Window = this;
        DataContext = vm;

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
        };

        // Main content area (settings and preview)
        var contentArea = CreateContentArea(vm);
        Grid.SetRow(contentArea, 0);
        mainGrid.Children.Add(contentArea);

        // Buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonReset = new Button
        {
            Content = Se.Language.General.Reset,
            Command = vm.ResetCommand,
            Margin = new Thickness(0, 0, 10, 0),
        };
        var panelButtons = UiUtil.MakeButtonBar(buttonReset, buttonOk, buttonCancel);
        Grid.SetRow(panelButtons, 1);
        mainGrid.Children.Add(panelButtons);

        Content = mainGrid;

        Activated += delegate { buttonOk.Focus(); };
        Loaded += (_, __) => vm.LoadVideoAndSubtitle();
        KeyDown += vm.KeyDown;
    }

    private static Grid CreateContentArea(AssaProgressBarViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(450, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 15,
        };

        // Left panel with settings
        var settingsPanel = CreateSettingsPanel(vm);
        Grid.SetColumn(settingsPanel, 0);
        grid.Children.Add(settingsPanel);

        // Right panel with preview
        var previewPanel = CreatePreviewPanel(vm);
        Grid.SetColumn(previewPanel, 1);
        grid.Children.Add(previewPanel);

        return grid;
    }

    private static ScrollViewer CreateSettingsPanel(AssaProgressBarViewModel vm)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
        };

        // Progress bar settings
        stackPanel.Children.Add(CreateProgressBarSettings(vm));

        // Chapters settings
        stackPanel.Children.Add(CreateChaptersSettings(vm));

        // Chapters formatting
        stackPanel.Children.Add(CreateChaptersFormattingSettings(vm));

        return new ScrollViewer
        {
            Content = stackPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
    }

    private static Border CreateProgressBarSettings(AssaProgressBarViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
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
            Text = Se.Language.Assa.ProgressBarSettings,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        Grid.SetRow(titleLabel, 0);
        Grid.SetColumnSpan(titleLabel, 2);
        grid.Children.Add(titleLabel);

        // Position radio buttons
        var posLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarPosition + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(posLabel, 1);
        grid.Children.Add(posLabel);

        var posPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 15 };
        var radioTop = new RadioButton
        {
            Content = Se.Language.Assa.ProgressBarTop,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.PositionTop)) { Mode = BindingMode.TwoWay },
            GroupName = "Position",
        };
        var radioBottom = new RadioButton
        {
            Content = Se.Language.Assa.ProgressBarBottom,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.PositionBottom)) { Mode = BindingMode.TwoWay },
            GroupName = "Position",
        };
        posPanel.Children.Add(radioTop);
        posPanel.Children.Add(radioBottom);
        Grid.SetRow(posPanel, 1);
        Grid.SetColumn(posPanel, 1);
        grid.Children.Add(posPanel);

        // Height
        var heightLabel = new TextBlock { Text = Se.Language.General.Height + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(heightLabel, 2);
        grid.Children.Add(heightLabel);

        var heightBox = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 200,
            Width = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.BarHeight)) { Mode = BindingMode.TwoWay },
        };
        Grid.SetRow(heightBox, 2);
        Grid.SetColumn(heightBox, 1);
        grid.Children.Add(heightBox);

        // Colors
        var fgLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarForeColor + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(fgLabel, 3);
        grid.Children.Add(fgLabel);

        var fgPicker = UiUtil.MakeColorPicker(vm, nameof(vm.ForegroundColor));
        fgPicker.HorizontalAlignment = HorizontalAlignment.Left;
        Grid.SetRow(fgPicker, 3);
        Grid.SetColumn(fgPicker, 1);
        grid.Children.Add(fgPicker);

        var bgLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarBackColor + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(bgLabel, 4);
        grid.Children.Add(bgLabel);

        var bgPicker = UiUtil.MakeColorPicker(vm, nameof(vm.BackgroundColor));
        bgPicker.HorizontalAlignment = HorizontalAlignment.Left;
        Grid.SetRow(bgPicker, 4);
        Grid.SetColumn(bgPicker, 1);
        grid.Children.Add(bgPicker);

        // Style
        var styleLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarStyle + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(styleLabel, 5);
        grid.Children.Add(styleLabel);

        var styleCombo = new ComboBox
        {
            Width = 150,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.CornerStyles)),
            [!ComboBox.SelectedIndexProperty] = new Binding(nameof(vm.CornerStyleIndex)) { Mode = BindingMode.TwoWay },
        };
        Grid.SetRow(styleCombo, 5);
        Grid.SetColumn(styleCombo, 1);
        grid.Children.Add(styleCombo);

        return new Border
        {
            Child = grid,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
        };
    }

    private static Border CreateChaptersSettings(AssaProgressBarViewModel vm)
    {
        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(180, GridUnitType.Pixel) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            RowSpacing = 8,
        };

        var titleLabel = new TextBlock
        {
            Text = Se.Language.Assa.ProgressBarChapters,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        Grid.SetRow(titleLabel, 0);
        mainGrid.Children.Add(titleLabel);

        // Chapter list
        var listBox = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.Chapters)),
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedChapter)) { Mode = BindingMode.TwoWay },
        };
        listBox.ItemTemplate = new FuncDataTemplate<ProgressBarChapter>((chapter, _) =>
            new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children =
                {
                    new TextBlock { [!TextBlock.TextProperty] = new Binding(nameof(ProgressBarChapter.Text)), MinWidth = 150 },
                    new TextBlock { [!TextBlock.TextProperty] = new Binding(nameof(ProgressBarChapter.StartTimeDisplay)), Foreground = Brushes.Gray },
                }
            });

        var listBorder = new Border
        {
            Child = listBox,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
        };
        Grid.SetRow(listBorder, 1);
        mainGrid.Children.Add(listBorder);

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
        };

        var btnAdd = new Button { Content = Se.Language.General.Add, Command = vm.AddChapterCommand };
        var btnRemove = new Button { Content = Se.Language.General.Remove, Command = vm.RemoveChapterCommand };
        var btnRemoveAll = new Button { Content = Se.Language.General.Remove + " " + Se.Language.General.All, Command = vm.RemoveAllChaptersCommand };

        buttonPanel.Children.Add(btnAdd);
        buttonPanel.Children.Add(btnRemove);
        buttonPanel.Children.Add(btnRemoveAll);

        Grid.SetRow(buttonPanel, 2);
        mainGrid.Children.Add(buttonPanel);

        // Chapter editor
        var editorGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 5,
            ColumnSpacing = 8,
            Margin = new Thickness(0, 5, 0, 0),
            [!Grid.IsVisibleProperty] = new Binding(nameof(vm.SelectedChapter)) { Converter = new Avalonia.Data.Converters.FuncValueConverter<object?, bool>(o => o != null) },
        };

        var textLabel = new TextBlock { Text = Se.Language.General.Text + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(textLabel, 0);
        editorGrid.Children.Add(textLabel);

        var textBox = new TextBox
        {
            [!TextBox.TextProperty] = new Binding(nameof(vm.ChapterText)) { Mode = BindingMode.TwoWay },
        };
        Grid.SetRow(textBox, 0);
        Grid.SetColumn(textBox, 1);
        editorGrid.Children.Add(textBox);

        var timeLabel = new TextBlock { Text = Se.Language.General.StartTime + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(timeLabel, 1);
        editorGrid.Children.Add(timeLabel);

        var timeBox = new TextBox
        {
            Width = 150,
            HorizontalAlignment = HorizontalAlignment.Left,
            Watermark = "hh:mm:ss.fff",
        };
        timeBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.ChapterStartTime))
        {
            Mode = BindingMode.TwoWay,
            Converter = new Avalonia.Data.Converters.FuncValueConverter<TimeSpan, string>(
                ts => ts.ToString(@"hh\:mm\:ss\.fff"),
                s => TimeSpan.TryParse(s, out var result) ? result : TimeSpan.Zero)
        });
        Grid.SetRow(timeBox, 1);
        Grid.SetColumn(timeBox, 1);
        editorGrid.Children.Add(timeBox);

        Grid.SetRow(editorGrid, 3);
        mainGrid.Children.Add(editorGrid);

        return new Border
        {
            Child = mainGrid,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
        };
    }

    private static Border CreateChaptersFormattingSettings(AssaProgressBarViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
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
            Text = Se.Language.Assa.ProgressBarChapters + " - Format",
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        grid.Add(titleLabel, 0, 0, 1, 2);

        // Splitter width
        var splitterWidthLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarSplitterWidth + ":", VerticalAlignment = VerticalAlignment.Center };
        grid.Add(splitterWidthLabel, 1);

        var splitterWidthBox = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 50,
            Width = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.SplitterWidth)) { Mode = BindingMode.TwoWay },
        };
        grid.Add(splitterWidthBox, 1, 1);

        // Splitter height
        var splitterHeightLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarSplitterHeight + ":", VerticalAlignment = VerticalAlignment.Center };
        grid.Add(splitterHeightLabel, 2);

        var splitterHeightBox = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 200,
            Width = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.SplitterHeight)) { Mode = BindingMode.TwoWay },
        };
        grid.Add(splitterHeightBox, 2, 1);

        // Font
        var fontLabel = new TextBlock { Text = Se.Language.General.Font + ":", VerticalAlignment = VerticalAlignment.Center };
        grid.Add(fontLabel, 3);

        var fontCombo = new ComboBox
        {
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Fonts)),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.FontName)) { Mode = BindingMode.TwoWay },
            MinWidth = 150,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        grid.Add(fontCombo, 3, 1);

        // Font size
        var sizeLabel = new TextBlock { Text = Se.Language.General.Size + ":", VerticalAlignment = VerticalAlignment.Center };
        grid.Add(sizeLabel, 4);

        var sizeBox = new NumericUpDown
        {
            Minimum = 6,
            Maximum = 200,
            Width = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.FontSize)) { Mode = BindingMode.TwoWay },
        };
        grid.Add(sizeBox, 4, 1);

        // Text color
        var colorLabel = new TextBlock { Text = Se.Language.General.Color + ":", VerticalAlignment = VerticalAlignment.Center };
        grid.Add(colorLabel, 5);

        var colorPicker = UiUtil.MakeColorPicker(vm, nameof(vm.TextColor));
        colorPicker.HorizontalAlignment = HorizontalAlignment.Left;
        grid.Add(colorPicker, 5, 1);

        // X adjustment
        var xLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarXAdjustment + ":", VerticalAlignment = VerticalAlignment.Center };
        grid.Add(xLabel, 6);

        var xBox = new NumericUpDown
        {
            Minimum = -500,
            Maximum = 500,
            Width = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.XAdjustment)) { Mode = BindingMode.TwoWay },
        };
        grid.Add(xBox, 6, 1);

        // Y adjustment
        var yLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarYAdjustment + ":", VerticalAlignment = VerticalAlignment.Center };
        grid.Add(yLabel, 7);

        var yBox = new NumericUpDown
        {
            Minimum = -500,
            Maximum = 500,
            Width = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.YAdjustment)) { Mode = BindingMode.TwoWay },
        };
        grid.Add(yBox, 7, 1);

        // Text alignment
        var alignLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarTextAlignment + ":", VerticalAlignment = VerticalAlignment.Center };
        grid.Add(alignLabel, 8);

        var alignCombo = new ComboBox
        {
            Width = 150,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.TextAlignments)),
            [!ComboBox.SelectedIndexProperty] = new Binding(nameof(vm.TextAlignmentIndex)) { Mode = BindingMode.TwoWay },
        };
        grid.Add(alignCombo, 8, 1);

        return new Border
        {
            Child = grid,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
        };
    }

    private static Border CreatePreviewPanel(AssaProgressBarViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 8,
        };

        var titleLabel = new TextBlock
        {
            Text = Se.Language.Assa.ProgressBarPreview,
            FontWeight = FontWeight.Bold,
        };
        Grid.SetRow(titleLabel, 0);
        grid.Children.Add(titleLabel);

        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = false;
        Grid.SetRow(vm.VideoPlayerControl, 1);
        grid.Children.Add(vm.VideoPlayerControl);

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
