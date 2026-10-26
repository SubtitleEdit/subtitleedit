using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Ocr.Download;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Platform.Progress;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

public class VideoOcrWindow : Window
{
    private ComboBox? _comboEngine;
    private ComboBox? _comboLlamaCppModel;
    private ComboBox? _comboCrispEmbedModel;

    public VideoOcrWindow(VideoOcrViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        // The view model is initialized with the video before the window is built, so the
        // file being OCR'ed can go straight into the title.
        Title = string.IsNullOrEmpty(vm.VideoFileName) || UiUtil.HideFileNames
            ? Se.Language.Video.VideoOcr.Title
            : $"{Se.Language.Video.VideoOcr.Title} - {System.IO.Path.GetFileName(vm.VideoFileName)}";
        CanResize = true;
        Width = 1100;
        Height = 800;
        MinWidth = 900;
        MinHeight = 650;
        vm.Window = this;
        DataContext = vm;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.VideoTool);

        var previewView = MakePreviewView(vm);
        var settingsView = MakeSettingsView(vm);
        var linesView = MakeLinesView(vm);
        var progressView = MakeProgressView(vm);

        var buttonStart = UiUtil.MakeButton(Se.Language.Video.VideoOcr.StartOcr, vm.StartOcrCommand)
            .WithBindEnabled(nameof(vm.IsRunning), InverseBooleanConverter.Instance).AsPrimary();
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand)
            .WithBindEnabled(nameof(vm.IsOkEnabled));
        var buttonPanel = EditorChrome.MakeButtonBar(MakeStatusText(vm), UiUtil.MakeButtonCancel(vm.CancelCommand), buttonOk, buttonStart);

        // Preview on the left, the settings column on the right (it scrolls on its own), the
        // result lines under both; the lines give up height first on a short screen.
        var top = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // preview
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // settings
            },
        };
        top.Add(previewView, 0, 0);
        top.Add(settingsView, 0, 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(3, GridUnitType.Star) }, // preview + settings
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }, // result lines
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // progress
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(top, 0, 0);
        grid.Add(new Border
        {
            Child = linesView,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
        }, 1, 0);
        grid.Add(progressView, 2, 0);
        grid.Add(buttonPanel, 3, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, () => { _comboEngine?.Focus(); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
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
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(imageArea, Se.Language.Video.VideoOcr.ScanAreaInfo);
        }

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
            FontSize = EditorChrome.FieldFontSize,
            Margin = new Thickness(5, 0, 0, 0),
        };
        positionText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.PreviewPositionText)) { Source = vm });

        var sliderRow = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        var sliderLabel = EditorChrome.MakeDimLabel(Se.Language.Video.VideoOcr.PreviewPosition, new Thickness(0));
        sliderRow.Add(sliderLabel, 0, 0);
        sliderRow.Add(slider.WithLabeledBy(sliderLabel), 0, 1);
        sliderRow.Add(positionText, 0, 2);

        var scanAreaText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            Margin = new Thickness(8, 0, 0, 0),
        };
        scanAreaText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ScanAreaText)) { Source = vm });

        // The three preset buttons share the "Scan area" label to their left. UI Automation
        // has no notion of that pairing, so a screen reader hears "Bottom third, button"
        // with nothing saying what it is a third of (#12087) - put the heading in each name.
        var scanAreaRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.Video.VideoOcr.ScanArea, new Thickness(0, 0, 6, 0)),
                MakePresetButton(Se.Language.Video.VideoOcr.BottomThird, vm.SetScanAreaBottomThirdCommand)
                    .WithAccessibleName($"{Se.Language.Video.VideoOcr.ScanArea}: {Se.Language.Video.VideoOcr.BottomThird}"),
                MakePresetButton(Se.Language.Video.VideoOcr.BottomHalf, vm.SetScanAreaBottomHalfCommand)
                    .WithAccessibleName($"{Se.Language.Video.VideoOcr.ScanArea}: {Se.Language.Video.VideoOcr.BottomHalf}"),
                MakePresetButton(Se.Language.Video.VideoOcr.FullFrame, vm.SetScanAreaFullFrameCommand)
                    .WithAccessibleName($"{Se.Language.Video.VideoOcr.ScanArea}: {Se.Language.Video.VideoOcr.FullFrame}"),
                scanAreaText,
            },
        };

        // The result of a test only went to the status text below the progress bar, which a
        // screen reader user cannot reach or hear (#12087). The button is disabled while the test
        // runs, which drops keyboard focus to nothing - put it back on the button when the test
        // is done, with the result as its description, so NVDA reads the result right away.
        var testButton = EditorChrome.MakeActionButton("fa-solid fa-flask", Se.Language.Video.VideoOcr.TestOcr, vm.TestOcrCommand)
            .WithBindEnabled(nameof(vm.IsRunning), InverseBooleanConverter.Instance);
        testButton.Bind(AutomationProperties.HelpTextProperty, new Binding(nameof(vm.TestOcrResult)) { Source = vm });
        RefocusWhenReEnabled(testButton);

        // On its own row: sharing the scan-area row squeezed that row's buttons once the
        // preview column narrowed.
        var testRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                testButton,
            },
        };

        // A plain border around the frame (no rounded clipping), like the video players
        var frame = new Border
        {
            Child = imageArea,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1),
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            RowSpacing = 6,
            Margin = new Thickness(14, 12, 14, 10),
        };
        var header = EditorChrome.MakeSectionHeader(Se.Language.General.Preview);
        header.Margin = new Thickness(0, 0, 0, 4);
        grid.Add(header, 0, 0);
        grid.Add(frame, 1, 0);
        grid.Add(sliderRow, 2, 0);
        grid.Add(scanAreaRow, 3, 0);
        grid.Add(testRow, 4, 0);

        return new Border
        {
            Child = grid,
            Background = EditorChrome.PanelBrush,
        };
    }

    /// <summary>
    /// Flat text button for the scan area presets.
    /// </summary>
    private static Button MakePresetButton(string text, System.Windows.Input.ICommand command)
    {
        var button = new Button
        {
            Content = text,
            Command = command,
            Height = EditorChrome.ScaledSize(28),
            FontSize = EditorChrome.FieldFontSize,
            Padding = new Thickness(10, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
        };
        button.Classes.Add("tool");
        return button;
    }

    private const double SettingsWidth = 370;
    private const double FieldWidth = 330;

    private Border MakeSettingsView(VideoOcrViewModel vm)
    {
        var comboEngine = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine))
            .WithAccessibleName(Se.Language.Video.VideoOcr.Engine);
        comboEngine.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboEngine.ItemTemplate = BuildEngineItemTemplate();
        _comboEngine = comboEngine;

        var comboPaddleLanguage = UiUtil.MakeComboBox(vm.PaddleLanguages, vm, nameof(vm.SelectedPaddleLanguage)).WithWidth(220);

        // The engine picker, plus a settings/info button for the selected engine - like the
        // speech-to-text and text-to-speech windows.
        var enginePanel = new Grid
        {
            ColumnSpacing = 4,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        enginePanel.Add(comboEngine, 0, 0);
        enginePanel.Add(EditorChrome.CreateToolButton(IconNames.Settings,
            $"{Se.Language.Video.VideoOcr.Engine} - {Se.Language.General.Settings}", vm.ShowEngineSettingsCommand, 28), 0, 1);

        var engineRows = new StackPanel { Spacing = 8 };
        engineRows.Children.Add(enginePanel);

        // What the selected engine actually is - each engine carries a one-line description
        // (local vs cloud, what it needs installed) that had no home in the window until now.
        var engineDescription = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
        };
        engineDescription.Bind(TextBlock.TextProperty, new Binding(nameof(vm.SelectedEngineDescription)) { Source = vm });
        engineRows.Children.Add(engineDescription);

        // Paddle OCR settings
        var paddlePanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        AddLabeledSetting(paddlePanel, Se.Language.General.Language, comboPaddleLanguage);
        paddlePanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsPaddleEngine)) { Source = vm });
        engineRows.Children.Add(paddlePanel);

        // Ollama settings
        var ollamaPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        AddLabeledSetting(ollamaPanel, Se.Language.General.Url, TextField(vm, nameof(vm.OllamaUrl)));
        AddLabeledSetting(ollamaPanel, Se.Language.General.Model, TextField(vm, nameof(vm.OllamaModel)));
        AddLabeledSetting(ollamaPanel, Se.Language.General.Language, TextField(vm, nameof(vm.OllamaLanguage)));
        ollamaPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsOllamaEngine)) { Source = vm });
        engineRows.Children.Add(ollamaPanel);

        // GLM settings
        var glmPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        AddLabeledSetting(glmPanel, Se.Language.General.Url, TextField(vm, nameof(vm.GlmUrl)));
        AddLabeledSetting(glmPanel, Se.Language.General.Model, TextField(vm, nameof(vm.GlmModel)));
        glmPanel.Children.Add(EditorChrome.MakeDimLabel(Se.Language.General.ApiKey, new Thickness(0)));
        glmPanel.Children.Add(UiUtil.MakeApiKeyTextBox(290, vm, nameof(vm.GlmApiKey)));
        AddLabeledSetting(glmPanel, Se.Language.General.Language, TextField(vm, nameof(vm.GlmLanguage)));
        glmPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsGlmEngine)) { Source = vm });
        engineRows.Children.Add(glmPanel);

        // llama.cpp settings
        var comboLlamaCppModel = UiUtil.MakeComboBox(vm.LlamaCppModels, vm, nameof(vm.SelectedLlamaCppModel)).WithWidth(FieldWidth);
        comboLlamaCppModel.ItemTemplate = BuildLlamaCppModelItemTemplate();
        _comboLlamaCppModel = comboLlamaCppModel;

        var llamaCppPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        var llamaCppLocalPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        AddLabeledSetting(llamaCppLocalPanel, Se.Language.General.Model, comboLlamaCppModel);
        var llamaCppButtons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
        llamaCppButtons.Children.Add(UiUtil.MakeButton(vm.DownloadLlamaCppCommand, IconNames.Download, Se.Language.General.Download));
        llamaCppButtons.Children.Add(MakeLlamaCppServerButton(vm));
        llamaCppLocalPanel.Children.Add(llamaCppButtons);
        llamaCppLocalPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsLlamaCppLocalVisible)) { Source = vm });
        llamaCppPanel.Children.Add(llamaCppLocalPanel);
        var llamaCppRemotePanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        AddLabeledSetting(llamaCppRemotePanel, Se.Language.General.Url, TextField(vm, nameof(vm.LlamaCppUrl)));
        llamaCppRemotePanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsLlamaCppRemoteVisible)) { Source = vm });
        llamaCppPanel.Children.Add(llamaCppRemotePanel);
        AddLabeledSetting(llamaCppPanel, Se.Language.General.Language, TextField(vm, nameof(vm.LlamaCppLanguage)));
        llamaCppPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsLlamaCppEngine)) { Source = vm });
        engineRows.Children.Add(llamaCppPanel);

        // CrispEmbed settings
        var comboCrispEmbedModel = UiUtil.MakeComboBox(vm.CrispEmbedModels, vm, nameof(vm.SelectedCrispEmbedModel)).WithWidth(FieldWidth);
        comboCrispEmbedModel.ItemTemplate = BuildCrispEmbedModelItemTemplate();
        _comboCrispEmbedModel = comboCrispEmbedModel;

        var crispEmbedPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        AddLabeledSetting(crispEmbedPanel, Se.Language.General.Backend, UiUtil.MakeComboBox(vm.CrispEmbedBackends, vm, nameof(vm.SelectedCrispEmbedBackend)).WithWidth(FieldWidth));
        AddLabeledSetting(crispEmbedPanel, Se.Language.General.Model, comboCrispEmbedModel);
        // No download buttons here - engine build and model downloads live in the CrispEmbed
        // settings dialog opened from the gear button next to the engine combo.
        crispEmbedPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsCrispEmbedEngine)) { Source = vm });
        engineRows.Children.Add(crispEmbedPanel);

        // Apple Vision settings - language only: the engine is part of macOS, so there is no
        // model to pick, nothing to download and no server to start.
        var appleVisionPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        AddLabeledSetting(appleVisionPanel, Se.Language.General.Language, UiUtil.MakeComboBox(vm.AppleVisionLanguages, vm, nameof(vm.SelectedAppleVisionLanguage)).WithWidth(FieldWidth));
        appleVisionPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsAppleVisionEngine)) { Source = vm });
        engineRows.Children.Add(appleVisionPanel);

        // Scan settings
        var scanRows = new StackPanel { Spacing = 8 };
        scanRows.Children.Add(MakeSettingRow(
            Se.Language.Video.VideoOcr.FramesPerSecond,
            NumberField(vm, nameof(vm.FramesPerSecond), 1, 30, 5),
            Se.Language.Video.VideoOcr.FramesPerSecondHint));
        scanRows.Children.Add(MakeSettingRow(
            Se.Language.Video.VideoOcr.TextBrightnessMinimum,
            NumberField(vm, nameof(vm.BrightnessMinimum), 0, 255, 190),
            Se.Language.Video.VideoOcr.TextBrightnessMinimumHint));

        // Time ranges (#16334): only these parts of the video are scanned. The button adds
        // the preview position as the next range start or end.
        var timeRangesTextBox = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.TimeRanges));
        timeRangesTextBox.Classes.Add("text");
        timeRangesTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        timeRangesTextBox.PlaceholderText = Se.Language.Video.VideoOcr.TimeRangesWatermark;
        timeRangesTextBox.WithBindEnabled(nameof(vm.IsRunning), InverseBooleanConverter.Instance);
        var timeRangesRow = new Grid
        {
            ColumnSpacing = 4,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        timeRangesRow.Add(timeRangesTextBox, 0, 0);
        timeRangesRow.Add(EditorChrome.CreateToolButton("fa-solid fa-location-crosshairs",
                Se.Language.Video.VideoOcr.InsertPreviewTime, vm.InsertPreviewTimeCommand, 28)
            .WithBindEnabled(nameof(vm.IsRunning), InverseBooleanConverter.Instance), 0, 1);
        var timeRangesLabel = EditorChrome.MakeDimLabel(Se.Language.Video.VideoOcr.TimeRanges, new Thickness(0));
        timeRangesTextBox.WithLabeledBy(timeRangesLabel);
        var keepLinesCheckBox = new CheckBox
        {
            Content = Se.Language.Video.VideoOcr.KeepExistingLines,
            DataContext = vm,
        };
        keepLinesCheckBox.Bind(CheckBox.IsCheckedProperty, new Avalonia.Data.Binding(nameof(vm.KeepExistingLines)) { Mode = Avalonia.Data.BindingMode.TwoWay });
        keepLinesCheckBox.WithBindEnabled(nameof(vm.IsRunning), InverseBooleanConverter.Instance);
        var timeRangesPanel = new StackPanel { Spacing = 4, Children = { timeRangesLabel, timeRangesRow, keepLinesCheckBox } };
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(timeRangesPanel, Se.Language.Video.VideoOcr.TimeRangesHint);
        }

        scanRows.Children.Add(timeRangesPanel);

        // Post-processing settings
        var postRows = new StackPanel { Spacing = 8 };
        postRows.Children.Add(MakeSettingRow(
            Se.Language.Video.VideoOcr.TextSimilarityPercent,
            NumberField(vm, nameof(vm.TextSimilarityPercent), 0, 100, 80),
            Se.Language.Video.VideoOcr.TextSimilarityPercentHint));
        postRows.Children.Add(MakeSettingRow(
            Se.Language.Video.VideoOcr.MaxGapMs,
            NumberField(vm, nameof(vm.MaxGapMs), 0, 10_000, 250),
            Se.Language.Video.VideoOcr.MaxGapHint));
        postRows.Children.Add(MakeSettingRow(
            Se.Language.Video.VideoOcr.MinDurationMs,
            NumberField(vm, nameof(vm.MinDurationMs), 0, 10_000, 250),
            Se.Language.Video.VideoOcr.MinDurationHint));
        postRows.Children.Add(EditorChrome.MakeSwitchRow(nameof(vm.AddAssaPositionTag), Se.Language.Video.VideoOcr.AddAssaPositionTag));
        postRows.Children.Add(EditorChrome.MakeSwitchRow(nameof(vm.DoFixOcrErrors), Se.Language.General.FixOcrErrors));
        var buttonDownloadDictionary = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.Video.VideoOcr.DownloadDictionaryHint, vm.DownloadDictionaryCommand, 28)
            .WithBindEnabled(nameof(vm.DoFixOcrErrors));

        var comboDictionary = UiUtil.MakeComboBox(vm.Dictionaries, vm, nameof(vm.SelectedDictionary)).WithWidth(170)
            .WithBindEnabled(nameof(vm.DoFixOcrErrors));
        postRows.Children.Add(MakeSettingRow(
            Se.Language.General.Dictionary,
            new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 3,
                Children =
                {
                    comboDictionary,
                    buttonDownloadDictionary,
                },
            },
            Se.Language.Video.VideoOcr.DictionaryHint));

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = SettingsWidth,
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.Video.VideoOcr.Engine, engineRows, first: true),
                EditorChrome.MakeEditorSection(Se.Language.Video.VideoOcr.Scan, scanRows),
                EditorChrome.MakeEditorSection(Se.Language.Video.VideoOcr.PostProcessing, postRows),
            },
        };

        var scrollViewer = new ScrollViewer
        {
            Content = panel,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,

            // Give the scrollbar its own layout lane: as an auto-hiding overlay it is drawn
            // on top of the content and covered the rightmost controls.
            AllowAutoHide = false,
        };

        return new Border
        {
            Child = scrollViewer,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        };
    }

    private static TextBox TextField(VideoOcrViewModel vm, string property)
    {
        var textBox = UiUtil.MakeTextBox(FieldWidth, vm, property);
        textBox.Classes.Add("text");
        return textBox;
    }

    private static NumericUpDown NumberField(VideoOcrViewModel vm, string property, int min, int max, int defaultValue)
    {
        var field = UiUtil.MakeNumericUpDownInt(min, max, defaultValue, 90, vm, property);
        field.Classes.Add("field");
        return field;
    }

    private static Button MakeLlamaCppServerButton(VideoOcrViewModel vm)
    {
        var button = UiUtil.MakeButton(string.Empty, vm.ToggleLlamaCppServerCommand);
        button.Bind(Button.ContentProperty, new Binding(nameof(vm.LlamaCppServerButtonText)));
        return button;
    }

    // Install-status dot for the engine combo: green = ready to use, amber = a newer build is
    // available, grey = downloadable but not on disk. Engines with nothing for us to download
    // (Paddle OCR Python via pip, Ollama server, the cloud GLM API) get no dot.
    private static DownloadDotStatus GetEngineDotStatus(VideoOcrEngineItem engine)
    {
        switch (engine.EngineType)
        {
            case OcrEngineType.PaddleOcrStandalone:
                return PaddleOcrInstallHelper.IsStandaloneInstalled()
                    ? DownloadDotStatus.UpToDate
                    : DownloadDotStatus.NotInstalled;
            case OcrEngineType.LlamaCpp:
                return LlamaCppDownloadHelper.GetEngineDotStatus();
            case OcrEngineType.CrispEmbed:
                return CrispEmbedEngine.IsEngineInstalled()
                    // Installed: the cheap .installed.sha256 sidecar turns an outdated build amber.
                    ? StatusDots.From(true, DownloadHashManager.GetSidecarStatus(CrispEmbedEngine.GetAndCreateFolder()))
                    : DownloadDotStatus.NotInstalled;
            default:
                return DownloadDotStatus.None;
        }
    }

    private static Avalonia.Controls.Templates.FuncDataTemplate<VideoOcrEngineItem> BuildEngineItemTemplate()
    {
        return StatusDots.ComboItemTemplate<VideoOcrEngineItem>(
            engine => engine.Name,
            // CrispEmbed is the one engine here with a tracked local install, so show what it
            // costs to download until it is on disk - same as the OCR window's engine combo.
            engine => engine.EngineType == OcrEngineType.CrispEmbed && !CrispEmbedEngine.IsEngineInstalled()
                ? CrispEmbedEngine.DownloadSizeText
                : null,
            GetEngineDotStatus);
    }

    private static Avalonia.Controls.Templates.FuncDataTemplate<LlamaCppModelDisplay> BuildLlamaCppModelItemTemplate()
    {
        return LlamaCppDownloadHelper.ModelItemTemplate();
    }

    private static Avalonia.Controls.Templates.FuncDataTemplate<CrispEmbedModelDisplay> BuildCrispEmbedModelItemTemplate()
    {
        return StatusDots.ComboItemTemplate<CrispEmbedModelDisplay>(
            model => model.Model.Name,
            model => model.Model.Size,
            model => model.Backend.IsModelInstalled(model.Model)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled);
    }

    // Rebuilds the combo item templates so the install-status dots are re-evaluated - the dots are
    // one-off snapshots, so a fresh template is the refresh. Called after a download or server start.
    public void RefreshDownloadDots()
    {
        if (_comboEngine != null)
        {
            _comboEngine.ItemTemplate = BuildEngineItemTemplate();
        }
        if (_comboLlamaCppModel != null)
        {
            _comboLlamaCppModel.ItemTemplate = BuildLlamaCppModelItemTemplate();
        }
        if (_comboCrispEmbedModel != null)
        {
            _comboCrispEmbedModel.ItemTemplate = BuildCrispEmbedModelItemTemplate();
        }
    }

    private static Grid MakeSettingRow(string label, Control control, string? hint = null)
    {
        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        var labelControl = EditorChrome.MakeDimLabel(label, new Thickness(0));
        labelControl.TextWrapping = TextWrapping.Wrap;
        grid.Add(labelControl, 0, 0);

        // Link the control to its visible label so a screen reader announces what the
        // setting is, not just its value and control type (#12087). A composite control
        // (combo box + button in a panel) gets the label on its first child, which is the
        // element that actually takes focus.
        var labeled = control is Panel panel && panel.Children.Count > 0 ? panel.Children[0] : control;
        AutomationProperties.SetLabeledBy(labeled, labelControl);
        grid.Add(control, 0, 1);

        if (hint != null && Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(grid, hint);
        }

        return grid;
    }

    /// <summary>
    /// Adds a label above a control and links them (UIA LabeledBy), so the control is
    /// announced by its purpose and not only as "combo box"/"edit" (#12087).
    /// </summary>
    private static void AddLabeledSetting(Panel panel, string label, Control control)
    {
        var labelControl = EditorChrome.MakeDimLabel(label, new Thickness(0));
        panel.Children.Add(labelControl);
        panel.Children.Add(control.WithLabeledBy(labelControl));
    }

    private static Grid MakeLinesView(VideoOcrViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var tableView = TableViewExtras.MakeTableView();
        tableView.DataContext = vm;
        tableView.ItemsSource = vm.Lines;

        // Rows announced the item's class name (VideoOcrLineItem) to screen readers; give
        // them the same "number: text, start - end, duration" name as the main grid (#12087).
        TableViewExtras.BindRowProperty(tableView, AutomationProperties.NameProperty,
            new MultiBinding
            {
                StringFormat = "{0}: {1}, {2} - {3}, {4}",
                Bindings =
                {
                    new Binding(nameof(VideoOcrLineItem.Number)),
                    new Binding(nameof(VideoOcrLineItem.Text)),
                    new Binding(nameof(VideoOcrLineItem.StartTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                    new Binding(nameof(VideoOcrLineItem.EndTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                    new Binding(nameof(VideoOcrLineItem.Duration)) { Converter = shortTimeConverter, Mode = BindingMode.OneWay },
                },
            });

        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(VideoOcrLineItem.Number)),
            Width = new GridLength(50),
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(VideoOcrLineItem.StartTime)) { Converter = fullTimeConverter },
            Width = new GridLength(110),
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Hide,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(VideoOcrLineItem.EndTime)) { Converter = fullTimeConverter },
            Width = new GridLength(110),
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(VideoOcrLineItem.Duration)) { Converter = shortTimeConverter },
            Width = new GridLength(90),
        });

        // Text is edited in a dialog (context menu "Edit..." - multi-line texts do not edit
        // comfortably inside a table row). The cell renders the item's FormattedText: the
        // fix engine's per-word coloring (green = word known, red = unknown, same palette as
        // the subtitle-bitmap OCR window) as lines arrive during OCR, plain themed text
        // before that. Everything is bound (never captured), so container recycling stays
        // safe - see the FuncDataTemplate recycling notes in NOcrTrainFontListTests.
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
            CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<VideoOcrLineItem>((_, _) =>
            {
                var contentControl = new ContentControl
                {
                    VerticalContentAlignment = VerticalAlignment.Center,
                    [!ContentControl.ContentProperty] = new Binding(nameof(VideoOcrLineItem.FormattedText)),
                };

                ScreenPrivacyBlurConverter.BlurUnlessSelected(contentControl, tableView);
                return contentControl;
            }, supportsRecycling: true),
        });

        var flyout = new MenuFlyout();
        var menuItemEdit = new MenuItem
        {
            Header = Se.Language.General.EditDotDotDot,
            InputGesture = new Avalonia.Input.KeyGesture(Avalonia.Input.Key.Enter),
        };
        menuItemEdit.Click += async (_, _) =>
        {
            if (tableView.SelectedItem is VideoOcrLineItem item)
            {
                await vm.EditLine(item);
            }
        };
        flyout.Items.Add(menuItemEdit);

        var menuItemItalic = new MenuItem
        {
            Header = Se.Language.General.Italic,
        };
        menuItemItalic.Click += (_, _) =>
        {
            var selected = tableView.SelectedItems?.OfType<VideoOcrLineItem>().ToList();
            if (selected != null)
            {
                VideoOcrViewModel.ToggleItalic(selected);
            }
        };
        flyout.Items.Add(menuItemItalic);

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
        };
        menuItemDelete.Click += (_, _) =>
        {
            var selected = tableView.SelectedItems?.OfType<VideoOcrLineItem>().ToList();
            if (selected != null)
            {
                vm.DeleteLines(selected);
            }
        };
        flyout.Items.Add(menuItemDelete);

        tableView.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(tableView);

        // Double-click a line to see the frame it came from in the preview.
        tableView.DoubleTapped += (s, e) =>
        {
            if (tableView.SelectedItem is VideoOcrLineItem item)
            {
                vm.SeekPreview(item);
            }
        };

        // Delete removes the selected lines (unless a cell edit is in progress).
        tableView.KeyDown += (s, e) =>
        {
            if (e.Key == Avalonia.Input.Key.Delete && e.Source is not TextBox)
            {
                var selectedItems = tableView.SelectedItems;
                if (selectedItems != null)
                {
                    vm.DeleteLines(selectedItems.OfType<VideoOcrLineItem>().ToList());
                }

                e.Handled = true;
            }
        };

        // Tunnel phase: the TableView handles Enter internally (see #12734), so a bubbling
        // KeyDown never sees it - the edit shortcut must run before the control's own handling.
        tableView.AddHandler(KeyDownEvent, (s, e) =>
        {
            if (e.Key is Avalonia.Input.Key.Enter or Avalonia.Input.Key.F2 &&
                e.Source is not TextBox &&
                tableView.SelectedItem is VideoOcrLineItem editItem)
            {
                e.Handled = true;
                _ = vm.EditLine(editItem);
            }
        }, RoutingStrategies.Tunnel);

        return EditorChrome.MakeListPane(Se.Language.General.Lines, tableView);
    }

    /// <summary>
    /// Disabling the focused button leaves keyboard focus on nothing; when the button is enabled
    /// again and focus is still nowhere, give it back to the button.
    /// </summary>
    internal static void RefocusWhenReEnabled(Button button)
    {
        var lostFocusByDisable = false;
        var focusVisible = false;

        // The focus rectangle is already off on LostFocus, so note how the button got focus.
        button.GotFocus += (_, e) =>
            focusVisible = e.NavigationMethod is NavigationMethod.Tab or NavigationMethod.Directional;

        // Checked on LostFocus: by the time IsEnabled reports the change, focus is already gone.
        button.LostFocus += (_, e) =>
            lostFocusByDisable = !button.IsEffectivelyEnabled && e.NewFocusedElement == null;

        button.PropertyChanged += (_, e) =>
        {
            if (e.Property != IsEffectivelyEnabledProperty || e.NewValue is not true || !lostFocusByDisable)
            {
                return;
            }

            lostFocusByDisable = false;
            var focused = TopLevel.GetTopLevel(button)?.FocusManager?.GetFocusedElement();
            if (focused == null || focused is TopLevel)
            {
                button.Focus(focusVisible ? NavigationMethod.Tab : NavigationMethod.Unspecified);
            }
        };
    }

    /// <summary>
    /// Progress bar and status in a strip above the buttons while the OCR runs.
    /// </summary>
    private Border MakeProgressView(VideoOcrViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.VerticalAlignment = VerticalAlignment.Center;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.DataContext = vm;
        progressBar.WithPlatformProgress(this, nameof(vm.IsRunning));

        var statusText = EditorChrome.MakeBoundHint(nameof(vm.ProgressText), 12);
        statusText.DataContext = vm;
        statusText.MaxWidth = 420;

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // progress bar
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // status text
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(progressBar, 0, 0);
        grid.Add(statusText, 0, 1);

        var strip = EditorChrome.MakeProgressStrip(grid);
        strip.Bind(IsVisibleProperty, new Binding(nameof(vm.IsRunning)));
        return strip;
    }

    /// <summary>
    /// The status (test result, lines found) in the button bar while the OCR is not running -
    /// while it runs the text is in the progress strip.
    /// </summary>
    private static TextBlock MakeStatusText(VideoOcrViewModel vm)
    {
        var statusText = EditorChrome.MakeBoundHint(nameof(vm.ProgressText), 12);
        statusText.Bind(IsVisibleProperty, new Binding(nameof(vm.IsRunning)) { Converter = InverseBooleanConverter.Instance });
        return statusText;
    }
}
