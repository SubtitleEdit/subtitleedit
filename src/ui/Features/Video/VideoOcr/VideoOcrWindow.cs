using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

public class VideoOcrWindow : Window
{
    public VideoOcrWindow(VideoOcrViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.VideoOcr.Title;
        CanResize = true;
        Width = 1100;
        Height = 800;
        MinWidth = 900;
        MinHeight = 650;
        vm.Window = this;
        DataContext = vm;

        var previewView = MakePreviewView(vm);
        var settingsView = MakeSettingsView(vm);
        var linesView = MakeLinesView(vm);
        var progressView = MakeProgressView(vm);

        var buttonStart = UiUtil.MakeButton(Se.Language.Video.VideoOcr.StartOcr, vm.StartOcrCommand)
            .WithBindEnabled(nameof(vm.IsRunning), new InverseBooleanConverter());
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand)
            .WithBindEnabled(nameof(vm.IsOkEnabled));
        var buttonPanel = UiUtil.MakeButtonBar(
            buttonStart,
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(3, GridUnitType.Star) }, // preview + settings
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }, // result lines
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // progress
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // preview
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // settings
            },
            Margin = UiUtil.MakeWindowMargin(),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 5,
            RowSpacing = 5,
        };

        grid.Add(previewView, 0, 0);
        grid.Add(settingsView, 0, 1);
        grid.Add(linesView, 1, 0, 1, 2);
        grid.Add(progressView, 2, 0, 1, 2);
        grid.Add(buttonPanel, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonStart.Focus(); }; // hack to make OnKeyDown work
        Loaded += (s, e) => vm.OnLoaded();
        Closing += (s, e) => vm.OnClosing();
        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
    }

    private static Border MakePreviewView(VideoOcrViewModel vm)
    {
        var image = new Image
        {
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.PreviewBitmap)) { Source = vm });

        var cropSelector = new CropAreaSelector
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        cropSelector.Bind(CropAreaSelector.VideoWidthProperty, new Binding(nameof(vm.VideoWidth)) { Source = vm });
        cropSelector.Bind(CropAreaSelector.VideoHeightProperty, new Binding(nameof(vm.VideoHeight)) { Source = vm });
        cropSelector.Bind(CropAreaSelector.SelectionXProperty, new Binding(nameof(vm.SelectionX)) { Source = vm, Mode = BindingMode.TwoWay });
        cropSelector.Bind(CropAreaSelector.SelectionYProperty, new Binding(nameof(vm.SelectionY)) { Source = vm, Mode = BindingMode.TwoWay });
        cropSelector.Bind(CropAreaSelector.SelectionWidthProperty, new Binding(nameof(vm.SelectionWidth)) { Source = vm, Mode = BindingMode.TwoWay });
        cropSelector.Bind(CropAreaSelector.SelectionHeightProperty, new Binding(nameof(vm.SelectionHeight)) { Source = vm, Mode = BindingMode.TwoWay });
        vm.CropSelector = cropSelector;

        var imageArea = new Panel
        {
            Background = Brushes.Black,
            Children = { image, cropSelector },
        };
        ToolTip.SetTip(imageArea, Se.Language.Video.VideoOcr.ScanAreaInfo);

        var slider = new Slider
        {
            Minimum = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };
        slider.Bind(Slider.MaximumProperty, new Binding(nameof(vm.DurationSeconds)) { Source = vm });
        slider.Bind(Slider.ValueProperty, new Binding(nameof(vm.PreviewPositionSeconds)) { Source = vm, Mode = BindingMode.TwoWay });

        var positionText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 60,
            Margin = new Thickness(5, 0, 0, 0),
        };
        positionText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.PreviewPositionText)) { Source = vm });

        var sliderRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        sliderRow.Add(UiUtil.MakeLabel(Se.Language.Video.VideoOcr.PreviewPosition), 0, 0);
        sliderRow.Add(slider, 0, 1);
        sliderRow.Add(positionText, 0, 2);

        var scanAreaText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.7,
            Margin = new Thickness(10, 0, 0, 0),
        };
        scanAreaText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ScanAreaText)) { Source = vm });

        var scanAreaRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Video.VideoOcr.ScanArea),
                UiUtil.MakeButton(Se.Language.Video.VideoOcr.BottomThird, vm.SetScanAreaBottomThirdCommand),
                UiUtil.MakeButton(Se.Language.Video.VideoOcr.BottomHalf, vm.SetScanAreaBottomHalfCommand),
                UiUtil.MakeButton(Se.Language.Video.VideoOcr.FullFrame, vm.SetScanAreaFullFrameCommand),
                scanAreaText,
            },
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            RowSpacing = 5,
        };
        grid.Add(imageArea, 0, 0);
        grid.Add(sliderRow, 1, 0);
        grid.Add(scanAreaRow, 2, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeSettingsView(VideoOcrViewModel vm)
    {
        var comboEngine = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine)).WithWidth(220);

        var comboPaddleLanguage = UiUtil.MakeComboBox(vm.PaddleLanguages, vm, nameof(vm.SelectedPaddleLanguage)).WithWidth(220);

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            Width = 350,
        };

        panel.Children.Add(MakeHeader(Se.Language.Video.VideoOcr.Engine, isFirst: true));
        panel.Children.Add(comboEngine);

        // Paddle OCR settings
        var paddlePanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        paddlePanel.Children.Add(UiUtil.MakeLabel(Se.Language.Video.VideoOcr.Language));
        paddlePanel.Children.Add(comboPaddleLanguage);
        paddlePanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsPaddleEngine)) { Source = vm });
        panel.Children.Add(paddlePanel);

        // Ollama settings
        var ollamaPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        ollamaPanel.Children.Add(UiUtil.MakeLabel(Se.Language.Video.VideoOcr.Url));
        ollamaPanel.Children.Add(UiUtil.MakeTextBox(330, vm, nameof(vm.OllamaUrl)));
        ollamaPanel.Children.Add(UiUtil.MakeLabel(Se.Language.Video.VideoOcr.Model));
        ollamaPanel.Children.Add(UiUtil.MakeTextBox(330, vm, nameof(vm.OllamaModel)));
        ollamaPanel.Children.Add(UiUtil.MakeLabel(Se.Language.Video.VideoOcr.Language));
        ollamaPanel.Children.Add(UiUtil.MakeTextBox(330, vm, nameof(vm.OllamaLanguage)));
        ollamaPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsOllamaEngine)) { Source = vm });
        panel.Children.Add(ollamaPanel);

        // GLM settings
        var glmPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        glmPanel.Children.Add(UiUtil.MakeLabel(Se.Language.Video.VideoOcr.Url));
        glmPanel.Children.Add(UiUtil.MakeTextBox(330, vm, nameof(vm.GlmUrl)));
        glmPanel.Children.Add(UiUtil.MakeLabel(Se.Language.Video.VideoOcr.Model));
        glmPanel.Children.Add(UiUtil.MakeTextBox(330, vm, nameof(vm.GlmModel)));
        glmPanel.Children.Add(UiUtil.MakeLabel(Se.Language.Video.VideoOcr.ApiKey));
        glmPanel.Children.Add(UiUtil.MakeApiKeyTextBox(290, vm, nameof(vm.GlmApiKey)));
        glmPanel.Children.Add(UiUtil.MakeLabel(Se.Language.Video.VideoOcr.Language));
        glmPanel.Children.Add(UiUtil.MakeTextBox(330, vm, nameof(vm.GlmLanguage)));
        glmPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsGlmEngine)) { Source = vm });
        panel.Children.Add(glmPanel);

        // llama.cpp settings
        var llamaCppPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        llamaCppPanel.Children.Add(UiUtil.MakeLabel(Se.Language.Video.VideoOcr.Model));
        llamaCppPanel.Children.Add(UiUtil.MakeComboBox(vm.LlamaCppModels, vm, nameof(vm.SelectedLlamaCppModel)).WithWidth(330));
        var llamaCppButtons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
        llamaCppButtons.Children.Add(UiUtil.MakeButton(vm.DownloadLlamaCppCommand, IconNames.Download, Se.Language.General.Download));
        llamaCppButtons.Children.Add(MakeLlamaCppServerButton(vm));
        llamaCppPanel.Children.Add(llamaCppButtons);
        llamaCppPanel.Children.Add(UiUtil.MakeLabel(Se.Language.Video.VideoOcr.Language));
        llamaCppPanel.Children.Add(UiUtil.MakeTextBox(330, vm, nameof(vm.LlamaCppLanguage)));
        llamaCppPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsLlamaCppEngine)) { Source = vm });
        panel.Children.Add(llamaCppPanel);

        // Scan settings
        panel.Children.Add(MakeHeader(Se.Language.Video.VideoOcr.Scan));
        panel.Children.Add(MakeSettingRow(
            Se.Language.Video.VideoOcr.FramesPerSecond,
            UiUtil.MakeNumericUpDownInt(1, 30, 5, 120, vm, nameof(vm.FramesPerSecond))));
        panel.Children.Add(MakeSettingRow(
            Se.Language.Video.VideoOcr.TextBrightnessMinimum,
            UiUtil.MakeNumericUpDownInt(0, 255, 190, 120, vm, nameof(vm.BrightnessMinimum))));

        // Post-processing settings
        panel.Children.Add(MakeHeader(Se.Language.Video.VideoOcr.PostProcessing));
        panel.Children.Add(MakeSettingRow(
            Se.Language.Video.VideoOcr.TextSimilarityPercent,
            UiUtil.MakeNumericUpDownInt(0, 100, 80, 120, vm, nameof(vm.TextSimilarityPercent))));
        panel.Children.Add(MakeSettingRow(
            Se.Language.Video.VideoOcr.MaxGapMs,
            UiUtil.MakeNumericUpDownInt(0, 10_000, 250, 120, vm, nameof(vm.MaxGapMs))));
        panel.Children.Add(MakeSettingRow(
            Se.Language.Video.VideoOcr.MinDurationMs,
            UiUtil.MakeNumericUpDownInt(0, 10_000, 250, 120, vm, nameof(vm.MinDurationMs))));
        panel.Children.Add(UiUtil.MakeCheckBox(Se.Language.Video.VideoOcr.AddAssaPositionTag, vm, nameof(vm.AddAssaPositionTag)));

        var scrollViewer = new ScrollViewer
        {
            Content = panel,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
        };

        return UiUtil.MakeBorderForControl(scrollViewer);
    }

    private static Button MakeLlamaCppServerButton(VideoOcrViewModel vm)
    {
        var button = UiUtil.MakeButton(string.Empty, vm.ToggleLlamaCppServerCommand);
        button.Bind(Button.ContentProperty, new Binding(nameof(vm.LlamaCppServerButtonText)));
        return button;
    }

    private static TextBlock MakeHeader(string text, bool isFirst = false)
    {
        return new TextBlock
        {
            Text = text,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, isFirst ? 0 : 12, 0, 2),
        };
    }

    private static Grid MakeSettingRow(string label, Control control)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Add(UiUtil.MakeLabel(label), 0, 0);
        grid.Add(control, 0, 1);
        return grid;
    }

    private static Border MakeLinesView(VideoOcrViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Extended,
            CanUserResizeColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            IsReadOnly = true,
            DataContext = vm,
            ItemsSource = vm.Lines,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(VideoOcrLineItem.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(VideoOcrLineItem.StartTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Hide,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(VideoOcrLineItem.EndTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Duration,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(VideoOcrLineItem.Duration)) { Converter = shortTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(VideoOcrLineItem.Text)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };

        return UiUtil.MakeBorderForControl(dataGrid);
    }

    private static Grid MakeProgressView(VideoOcrViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.IsVisibleProperty, new Binding(nameof(vm.IsRunning)));
        progressBar.DataContext = vm;

        var statusText = new TextBlock
        {
            Margin = new Thickness(5, 20, 0, 0),
            DataContext = vm,
        };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(progressBar, 0, 0);
        grid.Add(statusText, 0, 0);

        return grid;
    }
}
