using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Optris.Icons.Avalonia;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using MenuItem = Avalonia.Controls.MenuItem;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class OcrWindow : Window
{
    public OcrWindow(OcrViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        AutomationProperties.SetAutomationId(this, "OcrWindow");
        UpdateTitle(vm);
        Width = 1200;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        EditorChrome.ApplyWindow(this, EditorWindowStyles.VideoTool);
        AddOcrStyles(Styles);

        var topControlsView = MakeTopControlsView(vm);
        var subtitleView = MakeSubtitleView(vm);
        var editView = MakeEditView(vm);
        var progressView = MakeProgressView(vm);
        var buttonView = MakeBottomView(vm);

        var editViewHeight = 240;
        var editViewRow = new RowDefinition
        {
            Height = new GridLength(editViewHeight, GridUnitType.Pixel),
            MinHeight = 150
        };

        // Engine toolbar, image subtitle list, splitter, edit area (text, options, unknown
        // words), progress strip and button bar.
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 100 },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                editViewRow,
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        // Track the user's preferred height (saved when OCR starts)
        var savedHeight = (double)editViewHeight;

        // Collapse row when OCR is running, restore when stopped
        PropertyChangedEventHandler ocrRunningHandler = (s, e) =>
        {
            if (e.PropertyName == nameof(vm.Title))
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => UpdateTitle(vm));
            }

            if (e.PropertyName == nameof(vm.IsOcrRunning))
            {
                // Dispatch to UI thread since PropertyChanged may be raised from background thread
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    UpdateTitle(vm);

                    try
                    {
                        if (vm.IsOcrRunning)
                        {
                            // Save current height before collapsing (use actual bounds if available)
                            if (editView.Bounds.Height > 0)
                            {
                                savedHeight = editView.Bounds.Height;
                            }
                            else if (editViewRow.Height.GridUnitType == GridUnitType.Pixel && editViewRow.Height.Value > 0)
                            {
                                savedHeight = editViewRow.Height.Value;
                            }

                            // Use Auto to collapse (content is hidden via binding)
                            editViewRow.Height = new GridLength(1, GridUnitType.Auto);
                            editViewRow.MinHeight = 0;
                        }
                        else
                        {
                            // Restore to saved height (ensure it's valid)
                            var heightToRestore = savedHeight > 0 ? savedHeight : editViewHeight;
                            editViewRow.MinHeight = 150;
                            editViewRow.Height = new GridLength(heightToRestore, GridUnitType.Pixel);
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore exceptions during height restoration
                    }
                });
            }
        };
        vm.PropertyChanged += ocrRunningHandler;
        Closed += (_, _) => vm.PropertyChanged -= ocrRunningHandler;

        // Set initial state if OCR is already running when window opens
        if (vm.IsOcrRunning)
        {
            editViewRow.Height = new GridLength(1, GridUnitType.Auto);
            editViewRow.MinHeight = 0;
        }

        var splitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            ResizeDirection = GridResizeDirection.Rows,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = EditorChrome.LineBrush,
        };
        splitter.Bind(GridSplitter.IsVisibleProperty, new Binding(nameof(vm.IsOcrRunning)) { Source = vm, Converter = InverseBooleanConverter.Instance });

        grid.Add(topControlsView, 0, 0);
        grid.Add(subtitleView, 1, 0);
        grid.Add(splitter, 2, 0);
        grid.Add(editView, 3, 0);
        grid.Add(progressView, 4, 0);
        grid.Add(buttonView, 5, 0);

        Content = grid;

        Activated += (_, _) => Focus();
        Loaded += (s, e) => vm.OnLoaded();
        Closing += (s, e) => vm.OnClosing(e);
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static Border MakeTopControlsView(OcrViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 8,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var toggleButtonCaptureAlignment = MakeToolToggle(IconNames.DockTop, Se.Language.Ocr.CaptureTopAlign);
        toggleButtonCaptureAlignment.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(vm.HasCaptureAlignment)));

        var toggleButtonPreProcessing = MakeToolToggle(IconNames.Image, Se.Language.Ocr.ImagePreProcessing);
        toggleButtonPreProcessing.Command = vm.ShowPreProcessingCommand;
        toggleButtonPreProcessing.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(vm.HasPreProcessingSettings)));

        var toggleButtonVobSubColors = MakeToolToggle(IconNames.Palette, Se.Language.Ocr.VobSubColors);
        toggleButtonVobSubColors.Command = vm.PickVobSubColorsCommand;
        toggleButtonVobSubColors.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(vm.HasCustomVobSubColors)));
        toggleButtonVobSubColors.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsVobSubVisible)));

        var toggleButtonFallbackDatabase = MakeToolToggle(IconNames.DatabaseArrowRight, Se.Language.Ocr.FallbackOcrDatabase);
        toggleButtonFallbackDatabase.Command = vm.PickFallbackDatabaseCommand;
        toggleButtonFallbackDatabase.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(vm.HasFallbackDatabase)));
        toggleButtonFallbackDatabase.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsFallbackDatabaseVisible)));

        var toggleButtonShowOnlyForced = MakeToolToggle(IconNames.Filter, Se.Language.Ocr.ShowOnlyForcedSubtitles);
        toggleButtonShowOnlyForced.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(vm.ShowOnlyForced)));
        toggleButtonShowOnlyForced.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.HasForcedSubtitles)));
        toggleButtonShowOnlyForced.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsOcrRunning)) { Converter = InverseBooleanConverter.Instance });

        var panelRight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 2,
            Children =
            {
                EditorChrome.MakeToolbarSeparator(),
                toggleButtonShowOnlyForced,
                toggleButtonCaptureAlignment,
                toggleButtonPreProcessing,
                toggleButtonVobSubColors,
                toggleButtonFallbackDatabase,
            }
        };

        var comboBoxEngines = UiUtil.MakeComboBox(vm.OcrEngines, vm, nameof(vm.SelectedOcrEngine))
            .WithMarginRight(10)
            .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance);
        comboBoxEngines.SelectionChanged += vm.EngineSelectionChanged;
        comboBoxEngines.ItemTemplate = MakeOcrEngineItemTemplate();
        vm.RefreshEngineCombo = () => comboBoxEngines.ItemTemplate = MakeOcrEngineItemTemplate();

        var comboBoxCrispEmbedModels = UiUtil.MakeComboBox(vm.CrispEmbedModels, vm, nameof(vm.SelectedCrispEmbedModel),
                nameof(vm.IsCrispEmbedVisible))
            .WithMinWidth(220)
            .WithMarginRight(5)
            .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance);
        comboBoxCrispEmbedModels.ItemTemplate = MakeCrispEmbedModelItemTemplate();
        vm.RefreshCrispEmbedModelCombo = () => comboBoxCrispEmbedModels.ItemTemplate = MakeCrispEmbedModelItemTemplate();

        var comboBoxLlamaCppModels = UiUtil.MakeComboBox(vm.LlamaCppOcrModels, vm, nameof(vm.SelectedLlamaCppOcrModel),
                nameof(vm.IsLlamaCppLocalVisible))
            .WithWidth(220)
            .WithMarginRight(5)
            .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance);
        comboBoxLlamaCppModels.ItemTemplate = LlamaCppDownloadHelper.ModelItemTemplate();
        vm.RefreshLlamaCppOcrModelCombo = () => comboBoxLlamaCppModels.ItemTemplate = LlamaCppDownloadHelper.ModelItemTemplate();

        // The engine picker followed by the selected engine's settings - each engine's
        // controls are bound visible only while that engine is picked.
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.Ocr.OcrEngine, new Thickness(0, 0, 3, 0)),
                comboBoxEngines,

                // NOcr settings
                MakeEngineLabel(vm, Se.Language.Ocr.Database, nameof(vm.IsNOcrVisible)),
                UiUtil.MakeComboBox(vm.NOcrDatabases, vm, nameof(vm.SelectedNOcrDatabase), nameof(vm.IsNOcrVisible))
                    .WithMarginRight(0)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),
                MakeEngineToolButton(vm, IconNames.Settings, Se.Language.General.Settings, vm.ShowNOcrSettingsCommand, nameof(vm.IsNOcrVisible))
                    .WithMarginRight(10),
                MakeEngineLabel(vm, Se.Language.Ocr.MaxWrongPixels, nameof(vm.IsNOcrVisible)),
                UiUtil.MakeComboBox(vm.NOcrMaxWrongPixelsList, vm, nameof(vm.SelectedNOcrMaxWrongPixels),
                        nameof(vm.IsNOcrVisible))
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),
                MakeEngineLabel(vm, Se.Language.Ocr.NumberOfPixelsIsSpace, nameof(vm.IsNOcrVisible)),
                WithPixelsAreSpaceItemTemplate(UiUtil.MakeComboBox(vm.NOcrPixelsAreSpaceList, vm, nameof(vm.SelectedNOcrPixelsAreSpace),
                        nameof(vm.IsNOcrVisible)))
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),

                // Image Compare settings
                MakeEngineLabel(vm, Se.Language.Ocr.Database, nameof(vm.IsBinaryImageCompareVisible)),
                UiUtil.MakeComboBox(vm.ImageCompareDatabases, vm, nameof(vm.SelectedImageCompareDatabase), nameof(vm.IsBinaryImageCompareVisible))
                    .WithMarginRight(0)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),
                MakeEngineToolButton(vm, IconNames.Settings, Se.Language.General.Settings, vm.ShowBinaryOcrSettingsCommand, nameof(vm.IsBinaryImageCompareVisible))
                    .WithMarginRight(10),
                MakeEngineLabel(vm, Se.Language.Ocr.NumberOfPixelsIsSpace, nameof(vm.IsBinaryImageCompareVisible)),
                UiUtil.MakeComboBox(vm.BinaryOcrPixelsAreSpaceList, vm, nameof(vm.SelectedBinaryOcrPixelsAreSpace),
                        nameof(vm.IsBinaryImageCompareVisible))
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),
                MakeEngineLabel(vm, Se.Language.Ocr.MaxErrorPct, nameof(vm.IsBinaryImageCompareVisible)),
                MakeEngineField(UiUtil.MakeNumericUpDownOneDecimal(0, 50, 90, vm, nameof(vm.BinaryOcrMaxErrorPercent), nameof(vm.IsBinaryImageCompareVisible))
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance)),

                // Tesseract settings
                MakeEngineLabel(vm, Se.Language.General.Language, nameof(vm.IsTesseractVisible)),
                UiUtil.MakeComboBox(vm.TesseractDictionaryItems, vm, nameof(vm.SelectedTesseractDictionaryItem),
                        nameof(vm.IsTesseractVisible))
                    .WithWidth(100)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),
                MakeEngineToolButton(vm, IconNames.Download, Se.Language.General.Download, vm.PickTesseractModelCommand, nameof(vm.IsTesseractVisible))
                    .WithMarginRight(10),
                MakeEngineLabel(vm, Se.Language.Ocr.TesseractEngineMode, nameof(vm.IsTesseractVisible)),
                UiUtil.MakeComboBox(vm.TesseractEngineModes, vm, nameof(vm.SelectedTesseractEngineMode),
                        nameof(vm.IsTesseractVisible))
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),

                // Ollama settings
                MakeEngineLabel(vm, Se.Language.General.Language, nameof(vm.IsOllamaVisible)),
                UiUtil.MakeComboBox(vm.OllamaLanguages, vm, nameof(vm.SelectedOllamaLanguage),
                        nameof(vm.IsOllamaVisible))
                    .WithWidth(100)
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),
                MakeEngineLabel(vm, Se.Language.General.Model, nameof(vm.IsOllamaVisible)),
                MakeEngineTextBox(160, vm, nameof(vm.OllamaModel), nameof(vm.IsOllamaVisible)),
                MakeEngineToolButton(vm, "fa-solid fa-ellipsis", Se.Language.General.PickOllamaModel, vm.PickOllamaModelCommand, nameof(vm.IsOllamaVisible))
                    .WithMarginRight(10),
                MakeEngineLabel(vm, Se.Language.General.Url, nameof(vm.IsOllamaVisible)),
                MakeEngineTextBox(220, vm, nameof(vm.OllamaUrl), nameof(vm.IsOllamaVisible)),

                // llama.cpp settings
                MakeEngineLabel(vm, Se.Language.General.Language, nameof(vm.IsLlamaCppVisible)),
                UiUtil.MakeComboBox(vm.OllamaLanguages, vm, nameof(vm.SelectedOllamaLanguage),
                        nameof(vm.IsLlamaCppVisible))
                    .WithWidth(100)
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),
                MakeEngineLabel(vm, Se.Language.General.Model, nameof(vm.IsLlamaCppLocalVisible)),
                comboBoxLlamaCppModels,
                MakeEngineToolButton(vm, IconNames.Download, Se.Language.General.Download, vm.DownloadLlamaCppOcrCommand, nameof(vm.IsLlamaCppLocalVisible)),
                MakeLlamaCppOcrToggleServerButton(vm)
                    .WithMarginRight(5)
                    .BindIsVisible(vm, nameof(vm.IsLlamaCppLocalVisible))
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),
                MakeEngineLabel(vm, Se.Language.General.Url, nameof(vm.IsLlamaCppRemoteVisible)),
                MakeEngineTextBox(220, vm, nameof(vm.LlamaCppUrl), nameof(vm.IsLlamaCppRemoteVisible))
                    .WithMarginRight(5),
                MakeEngineToolButton(vm, IconNames.Settings, Se.Language.General.Settings, vm.ShowLlamaCppOcrSettingsCommand, nameof(vm.IsLlamaCppVisible))
                    .WithMarginRight(10),

                // CrispEmbed settings
                MakeEngineLabel(vm, Se.Language.General.Backend, nameof(vm.IsCrispEmbedVisible)),
                UiUtil.MakeComboBox(vm.CrispEmbedBackends, vm, nameof(vm.SelectedCrispEmbedBackend),
                        nameof(vm.IsCrispEmbedVisible))
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),
                MakeEngineLabel(vm, Se.Language.General.Model, nameof(vm.IsCrispEmbedVisible)),
                comboBoxCrispEmbedModels,
                MakeEngineToolButton(vm, IconNames.Download, Se.Language.General.Download, vm.DownloadCrispEmbedCommand, nameof(vm.IsCrispEmbedVisible)),
                // Separate from the download button above, which fetches the selected *model*:
                // this one re-fetches the engine binaries and re-asks CPU/Vulkan/CUDA, the only
                // way to change hardware build after the first install (issue #13400).
                MakeEngineToolButton(vm, IconNames.CloudDownload,
                        string.Format(Se.Language.General.ReDownloadX, CrispEmbedEngine.StaticName),
                        vm.ReDownloadCrispEmbedEngineCommand, nameof(vm.IsCrispEmbedVisible))
                    .WithMarginRight(10),

                // Apple Vision settings - language only: the engine ships with macOS, so there is
                // nothing to download, no key to enter and no model to pick.
                MakeEngineLabel(vm, Se.Language.General.Language, nameof(vm.IsAppleVisionVisible)),
                UiUtil.MakeComboBox(vm.AppleVisionLanguages, vm, nameof(vm.SelectedAppleVisionLanguage),
                        nameof(vm.IsAppleVisionVisible))
                    .WithWidth(180)
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),

                // Google vision settings
                MakeEngineLabel(vm, Se.Language.General.Language, nameof(vm.IsGoogleVisionVisible)),
                UiUtil.MakeComboBox(vm.GoogleVisionLanguages, vm, nameof(vm.SelectedGoogleVisionLanguage),
                        nameof(vm.IsGoogleVisionVisible))
                    .WithWidth(100)
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),
                MakeEngineLabel(vm, Se.Language.General.ApiKey, nameof(vm.IsGoogleVisionVisible)),
                MakeApiKeyBox(vm, nameof(vm.GoogleVisionApiKey), nameof(vm.IsGoogleVisionVisible)),

                // Google Lens settings
                MakeEngineLabel(vm, Se.Language.General.Language, nameof(vm.IsGoogleLensVisible)),
                UiUtil.MakeComboBox(vm.GoogleLensLanguages, vm, nameof(vm.SelectedGoogleLensLanguage),
                        nameof(vm.IsGoogleLensVisible))
                    .WithWidth(100)
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),

                // Paddle OCR settings
                MakeEngineLabel(vm, Se.Language.General.Language, nameof(vm.IsPaddleOcrVisible)),
                UiUtil.MakeComboBox(vm.PaddleOcrLanguages, vm, nameof(vm.SelectedPaddleOcrLanguage),
                        nameof(vm.IsPaddleOcrVisible))
                    .WithWidth(100)
                    .WithMarginRight(10)
                    .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance),

                // Mistral OCR settings
                MakeEngineLabel(vm, Se.Language.General.ApiKey, nameof(vm.IsMistralOcrVisible)),
                MakeApiKeyBox(vm, nameof(vm.MistralApiKey), nameof(vm.IsMistralOcrVisible)),
            }
        };

        // On a narrow window the engine settings are clipped rather than pushed under the
        // toggles on the right.
        var panelHost = new Panel { ClipToBounds = true, Children = { panel } };
        grid.Add(panelHost, 0, 0);
        grid.Add(panelRight, 0, 1);

        return new Border
        {
            Child = grid,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(14, 10),
        };
    }

    /// <summary>
    /// Dim label of an engine setting, shown only while that engine is picked.
    /// </summary>
    private static TextBlock MakeEngineLabel(OcrViewModel vm, string text, string isVisiblePath)
    {
        var label = EditorChrome.MakeDimLabel(text, new Thickness(0));
        label.Bind(Visual.IsVisibleProperty, new Binding(isVisiblePath) { Source = vm, Mode = BindingMode.OneWay });
        return label;
    }

    /// <summary>
    /// Flat icon button of an engine setting (settings, download, pick), shown only while that
    /// engine is picked and disabled while OCR runs.
    /// </summary>
    private static Button MakeEngineToolButton(OcrViewModel vm, string icon, string tooltip, System.Windows.Input.ICommand command, string isVisiblePath)
    {
        return EditorChrome.CreateToolButton(icon, tooltip, command, 28)
            .BindIsVisible(vm, isVisiblePath)
            .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance);
    }

    private static TextBox MakeEngineTextBox(double width, OcrViewModel vm, string textPath, string isVisiblePath)
    {
        var textBox = UiUtil.MakeTextBox(width, vm, textPath)
            .BindIsVisible(vm, isVisiblePath)
            .BindIsEnabled(vm, nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance);
        textBox.Classes.Add("text");
        return textBox;
    }

    private static NumericUpDown MakeEngineField(NumericUpDown field)
    {
        field.Classes.Add("field");
        return field;
    }

    /// <summary>
    /// Flat icon toggle for the toolbar; lit in the accent while on.
    /// </summary>
    private static ToggleButton MakeToolToggle(string icon, string name)
    {
        var toggle = new ToggleButton
        {
            Width = 30,
            Height = 30,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        toggle.Classes.Add("tool");
        Attached.SetIcon(toggle, icon);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(toggle, name);
        }

        AutomationProperties.SetName(toggle, name);
        return toggle;
    }

    /// <summary>
    /// Masked API key box (eye button to reveal) like Translate/TTS/Video OCR, disabled while OCR runs.
    /// </summary>
    private static StackPanel MakeApiKeyBox(OcrViewModel vm, string apiKeyPath, string isVisiblePath)
    {
        var panel = UiUtil.MakeApiKeyTextBox(200, vm, apiKeyPath, isVisiblePath);
        if (panel.Children.Count > 0 && panel.Children[0] is TextBox textBox)
        {
            textBox.Classes.Add("text");
        }

        panel.Bind(StackPanel.IsEnabledProperty, new Binding
        {
            Path = nameof(OcrViewModel.IsOcrRunning),
            Mode = BindingMode.OneWay,
            Source = vm,
            Converter = InverseBooleanConverter.Instance,
        });
        return panel;
    }

    private static Border MakeSubtitleView(OcrViewModel vm)
    {
        // One brush for every preview cell in this window, so recolouring repaints them all.
        var previewBackground = ImagePreviewBackground.CreateBrush();

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        // TableView (Avalonia 12.1) pilot #2, after Show history (#12704): this is SE's
        // heaviest grid - virtualized template columns with per-row bitmaps - chosen to
        // judge TableView's scrolling against DataGrid's. Differences vs the DataGrid it
        // replaces: no column sorting (TableView has none), and extended selection
        // (shift/ctrl-click, shift+arrows) is native ListBox behavior instead of
        // DataGridCheckboxMultiSelect.
        //
        // TableView has no content-based column sizing either - GridLength.Auto is
        // treated as 1* by its layout helper - so the narrow columns get pixel widths
        // measured from their widest content (the VM is initialized before the window
        // ctor, so the items are available here).
        //
        // Measure with the UI font the user picked - the FontManager default (Helvetica Neue
        // on macOS) may not load at all, and a measuring failure must never stop the window
        // from opening: fall back to a rough per-character estimate (#15562).
        const double cellChrome = 16; // cell padding/margins + slack
        var measureTypeface = new Typeface(FontFamilyHelper.Make(Se.Settings.Appearance.FontName));
        double MeasureWidth(string text)
        {
            try
            {
                return new FormattedText(text ?? string.Empty, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                    measureTypeface, 14, null).Width;
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "OcrWindow: could not measure text width");
                return (text?.Length ?? 0) * 14 * 0.6;
            }
        }
        double ColumnWidth(string header, string widestCellText) =>
            Math.Max(MeasureWidth(header), MeasureWidth(widestCellText)) + cellChrome;

        var lastItem = vm.OcrSubtitleItems.LastOrDefault();
        var maxDuration = vm.OcrSubtitleItems.Count > 0 ? vm.OcrSubtitleItems.Max(p => p.Duration) : TimeSpan.Zero;
        var numberWidth = ColumnWidth(Se.Language.General.NumberSymbol, vm.OcrSubtitleItems.Count.ToString());
        var showWidth = ColumnWidth(Se.Language.General.Show,
            fullTimeConverter.Convert(lastItem?.StartTime ?? TimeSpan.Zero, typeof(string), null, CultureInfo.CurrentUICulture) as string ?? string.Empty);
        var durationWidth = ColumnWidth(Se.Language.General.Duration,
            shortTimeConverter.Convert(maxDuration, typeof(string), null, CultureInfo.CurrentUICulture) as string ?? string.Empty);
        var dataGridSubtitle = new TableView
        {
            SelectionMode = SelectionMode.Multiple,
            CanUserResizeColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.OcrSubtitleItems,
        };

        if (vm.HasForcedSubtitles)
        {
            dataGridSubtitle.Columns.Add(new TableViewColumn
            {
                Header = Se.Language.General.Forced,
                Width = new GridLength(MeasureWidth(Se.Language.General.Forced) + cellChrome),
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = new FuncDataTemplate<OcrSubtitleItem>((_, _) =>
                    new CheckBox
                    {
                        [!ToggleButton.IsCheckedProperty] = new Binding(nameof(OcrSubtitleItem.IsForced)) { Mode = BindingMode.OneWay },
                        HorizontalAlignment = HorizontalAlignment.Center,
                        IsHitTestVisible = false,
                        Focusable = false,
                    }),
            });
        }

        dataGridSubtitle.Columns.AddRange(new[]
        {
                new TableViewColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Width = new GridLength(numberWidth),
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(OcrSubtitleItem.Number)),
                },
                new TableViewColumn
                {
                    Header = Se.Language.General.Show,
                    Width = new GridLength(showWidth),
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(OcrSubtitleItem.StartTime)) { Converter = fullTimeConverter },
                },
                new TableViewColumn
                {
                    Header = Se.Language.General.Duration,
                    Width = new GridLength(durationWidth),
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(OcrSubtitleItem.Duration)) { Converter = shortTimeConverter },
                },
                new TableViewColumn
                {
                    Header = Se.Language.General.Image,
                    Width = new GridLength(vm.ImageMaxWidth + cellChrome),
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = new FuncDataTemplate<OcrSubtitleItem>((item, _) =>
                    {
                        var stackPanel = new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            Spacing = 5,
                            Margin = new Thickness(5),
                        };
                        var bitmap = item.GetBitmapCropped();
                        if (bitmap != null)
                        {
                            var image = new Image
                            {
                                Source = bitmap,
                                Stretch = Avalonia.Media.Stretch.Uniform
                            };
                            image.Bind(Image.MaxHeightProperty, new Binding(nameof(vm.ImageMaxHeight)) { Source = vm });
                            image.Bind(Image.MaxWidthProperty, new Binding(nameof(vm.ImageMaxWidth)) { Source = vm });

                            // Subtitle bitmaps are usually light text on a transparent background, which
                            // is invisible on a light grid - give them a backdrop so they show. A
                            // checkerboard was tried here (issue #12692) but the tiling competes with the
                            // glyphs at thumbnail size; it is kept only in the pre-processing preview,
                            // where the image is large enough for it. The colour is shared with the
                            // binary-edit grid and configurable from either (#14328).
                            var imageContainer = new Border
                            {
                                Background = previewBackground,
                                CornerRadius = new CornerRadius(3),
                                Padding = new Thickness(3),
                                Child = image,
                            };
                            ScreenPrivacyBlurConverter.BlurUnlessSelected(imageContainer, dataGridSubtitle);
                            stackPanel.Children.Add(imageContainer);
                        }

                        return stackPanel;
                    })
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Text,
                    NameBinding = new Binding(nameof(OcrSubtitleItem.Text)),
                    Width = new GridLength(1, GridUnitType.Star),
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = new FuncDataTemplate<OcrSubtitleItem>((item, _) =>
                    {
                        var contentPresenter = new ContentPresenter
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(3, 1, 3, 1)
                        };

                        // Bind to HasFormattedText to trigger updates when FixResult changes
                        var binding = new Binding(nameof(OcrSubtitleItem.HasFormattedText))
                        {
                            Source = item,
                            Converter = new FuncValueConverter<bool, TextBlock>(hasFormatted =>
                                item.CreateFormattedText())
                        };

                        contentPresenter.Bind(ContentPresenter.ContentProperty, binding);
                        ScreenPrivacyBlurConverter.BlurUnlessSelected(contentPresenter, dataGridSubtitle);
                        return contentPresenter;
                    })
                },
        });

        // Index-mapped scrollbar (#13579), like the main subtitle grid: the native bar is
        // pixel-mapped and the virtualizing panel estimates its extent from the average
        // realized row height, so the thumb jumps around while scrolling. Rows here vary
        // even more than in the main grid - each holds a subtitle bitmap - so hide the
        // native vertical bar and dock a row-index one beside the grid instead.
        dataGridSubtitle.WithAccessibleName(Se.Language.General.ImageBasedSubtitles);
        TableViewExtras.ApplyDefaultRowNames(dataGridSubtitle); // #12087: rows are named from the columns

        var scrollBarHost = new TableViewIndexScrollBar(dataGridSubtitle);

        // Keep the view on the row being edited when a row changes height (#13619, #15275):
        // adding or removing a line in the edit box grows or shrinks the row, the virtualizing
        // panel re-estimates its pixel extent, and the grid scrolled away from the line.
        TableViewScrollAnchor.Attach(dataGridSubtitle);

        // The image thumbnails scale with Ctrl+plus/minus/0 and Ctrl+wheel (Image.MaxWidth/MaxHeight are
        // bound to the VM) - keep the pixel-sized image column in step with the zoom.
        var imageColumn = dataGridSubtitle.Columns[dataGridSubtitle.Columns.Count - 2];
        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(vm.ImageMaxWidth))
            {
                imageColumn.Width = new GridLength(vm.ImageMaxWidth + cellChrome);

                // Zooming re-measures every row, and the ScrollViewer keeps its pixel
                // offset - which lands on a different row (zooming out far enough pins the
                // list to its end). Stay on the row the user was looking at.
                scrollBarHost.PreserveTopRow();
            }
        };

        UiUtil.ApplyTableViewRowStyle(dataGridSubtitle);
        TableViewExtras.AttachListNavigation(dataGridSubtitle);
        dataGridSubtitle.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedOcrSubtitleItem)) { Source = vm });
        dataGridSubtitle.KeyDown += vm.SubtitleGridKeyDown;
        dataGridSubtitle.DoubleTapped += (s, e) => vm.SubtitleGridDoubleTapped();
        dataGridSubtitle.AddHandler(InputElement.PointerPressedEvent, vm.DataGridSubtitleMacPointerPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        dataGridSubtitle.AddHandler(InputElement.PointerReleasedEvent, vm.DataGridSubtitleMacPointerReleased, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        dataGridSubtitle.AddHandler(InputElement.PointerWheelChangedEvent, vm.SubtitleGridPointerWheelChanged, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        dataGridSubtitle.SelectionChanged += vm.SubtitleGridSelectionChanged;
        vm.SubtitleGrid = dataGridSubtitle;

        // Create a Flyout for the DataGrid
        var flyout = new MenuFlyout();

        flyout.Opening += vm.SubtitleGridContextOpening;

        var menuItemOcrSelectedLines = new MenuItem
        {
            Header = Se.Language.Ocr.OcrSelectedLines,
            DataContext = vm,
            Command = vm.StartOcrSelectedLinesCommand,
        };
        menuItemOcrSelectedLines.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemOcrSelectedLines);

        var menuItemInspectMatchesForLine = new MenuItem
        {
            Header = Se.Language.Ocr.InspectLine,
            DataContext = vm,
            Command = vm.InspectLineCommand,
        };
        menuItemInspectMatchesForLine.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsInspectLineVisible)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemInspectMatchesForLine);

        var menuItemShowImage = new MenuItem
        {
            Header = Se.Language.Ocr.ShowImage,
            DataContext = vm,
            Command = vm.ViewSelectedImageCommand,
            InputGesture = new KeyGesture(Key.P, KeyModifiers.Control), // handled in SubtitleGridKeyDown
        };
        menuItemShowImage.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemShowImage);

        var menuItemSaveImage = new MenuItem
        {
            Header = Se.Language.General.SaveImageAsDotDotDot,
            DataContext = vm,
            Command = vm.SaveImageAsCommand,
        };
        menuItemSaveImage.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemSaveImage);

        var menuItemCopyImageToClipboard = new MenuItem
        {
            Header = Se.Language.General.CopyImageToClipboard,
            DataContext = vm,
            Command = vm.CopyImageToClipboardCommand,
        };
        menuItemCopyImageToClipboard.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemCopyImageToClipboard);

        flyout.Items.Add(new Separator());

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.DeleteSelectedLinesCommand,
        };
        menuItemDelete.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemDelete);

        var menuItemFind = new MenuItem
        {
            Header = Se.Language.Main.Menu.Find,
            DataContext = vm,
            Command = vm.ShowFindCommand,
        };
        menuItemFind.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemFind);

        var menuItemReplace = new MenuItem
        {
            Header = Se.Language.Main.Menu.Replace,
            DataContext = vm,
            Command = vm.ShowReplaceCommand,
        };
        menuItemReplace.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemReplace);

        var menuItemFillSelectedLinesWithClipboard = new MenuItem
        {
            Header = Se.Language.Ocr.FillSelectedLinesWithClipboard,
            DataContext = vm,
            Command = vm.FillSelectedLinesWithClipboardCommand,
        };
        menuItemFillSelectedLinesWithClipboard.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.HasMultipleLinesSelected)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemFillSelectedLinesWithClipboard);

        flyout.Items.Add(new Separator());

        var menuItemItalic = new MenuItem
        {
            Header = Se.Language.General.Italic,
            DataContext = vm,
            Command = vm.ToggleItalicCommand,
        };
        menuItemItalic.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemItalic);

        var menuItemBold = new MenuItem
        {
            Header = Se.Language.General.Bold,
            DataContext = vm,
            Command = vm.ToggleBoldCommand,
        };
        menuItemBold.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemBold);

        flyout.Items.Add(new Separator());

        var menuItemExport = new MenuItem
        {
            Header = Se.Language.Ocr.EditExportDotDotDot,
            DataContext = vm,
            Command = vm.ExportCurrentOcrItemsCommand,
        };
        menuItemExport.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemExport);

        var menuItemImportTextFromSubtitle = new MenuItem
        {
            Header = Se.Language.Ocr.ImportTextFromSubtitleDotDotDot,
            DataContext = vm,
            Command = vm.ImportTextFromSubtitleCommand,
        };
        menuItemImportTextFromSubtitle.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemImportTextFromSubtitle);

        var menuItemExportTextAsSubtitle = new MenuItem
        {
            Header = Se.Language.Ocr.ExportTextAsSubtitleDotDotDot,
            DataContext = vm,
            Command = vm.ExportTextAsSubtitleCommand,
        };
        menuItemExportTextAsSubtitle.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemExportTextAsSubtitle);

        var menuItemSaveAllImagesWithHtmlIndex = new MenuItem
        {
            Header = Se.Language.Ocr.SaveAllImagesWithHtmlIndexDotDotDot,
            DataContext = vm,
            Command = vm.SaveAllImagesWithHtmlIndexCommand,
        };
        menuItemSaveAllImagesWithHtmlIndex.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowContextMenu)) { Mode = BindingMode.OneWay });
        flyout.Items.Add(menuItemSaveAllImagesWithHtmlIndex);

        flyout.Items.Add(new Separator());
        flyout.Items.Add(ImagePreviewBackground.MakeMenuItem(vm.WindowService, () => vm.Window, previewBackground));

        vm.SubtitleGrid.ContextFlyout = flyout;

        return new Border
        {
            Child = EditorChrome.MakeListPane(Se.Language.General.ImageBasedSubtitles, scrollBarHost, margin: new Thickness(14, 10, 14, 12)),
            Background = EditorChrome.PanelBrush,
        };
    }

    private static Border MakeEditView(OcrViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ClipToBounds = true, // Ensure content doesn't affect parent measure
        };

        var textBoxText = new TextBox
        {
            Height = double.NaN,
            MinHeight = 80,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            AcceptsReturn = true,
            TextWrapping = Avalonia.Media.TextWrapping.NoWrap,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            CornerRadius = new CornerRadius(6),
            [!TextBox.TextProperty] = new Binding($"{nameof(vm.SelectedOcrSubtitleItem)}.{nameof(OcrSubtitleItem.Text)}")
            {
                Source = vm,
                Mode = BindingMode.TwoWay
            }
        };

        textBoxText.Bind(FontFamilyProperty, new Binding(nameof(vm.TextBoxFontFamily)) { Mode = BindingMode.TwoWay });
        textBoxText.Bind(FontSizeProperty, new Binding(nameof(vm.TextBoxFontSize)) { Mode = BindingMode.TwoWay });
        textBoxText.Bind(TextBox.FontWeightProperty, new Binding(nameof(vm.TextBoxFontWeight)) { Mode = BindingMode.TwoWay });
        UiUtil.FixMacDiacriticClipping(textBoxText);

        // Setting ContextFlyout replaces the TextBox's built-in Cut/Copy/Paste menu, so add
        // those back above the font item (#15275)
        var flyout = new MenuFlyout();
        var menuItemCut = new MenuItem { Header = Se.Language.General.Cut };
        menuItemCut.Click += (_, _) => textBoxText.Cut();
        flyout.Items.Add(menuItemCut);
        var menuItemCopy = new MenuItem { Header = Se.Language.General.Copy };
        menuItemCopy.Click += (_, _) => textBoxText.Copy();
        flyout.Items.Add(menuItemCopy);
        var menuItemPaste = new MenuItem { Header = Se.Language.General.Paste };
        menuItemPaste.Click += (_, _) => textBoxText.Paste();
        flyout.Items.Add(menuItemPaste);
        var menuItemSelectAll = new MenuItem { Header = Se.Language.General.SelectAll };
        menuItemSelectAll.Click += (_, _) => textBoxText.SelectAll();
        flyout.Items.Add(menuItemSelectAll);
        flyout.Items.Add(new Separator());
        flyout.Items.Add(new MenuItem { Header = Se.Language.Main.Menu.Find, DataContext = vm, Command = vm.ShowFindCommand });
        flyout.Items.Add(new MenuItem { Header = Se.Language.Main.Menu.Replace, DataContext = vm, Command = vm.ShowReplaceCommand });
        flyout.Items.Add(new Separator());
        var menuItemSetFont = new MenuItem
        {
            Header = Se.Language.General.SetFontDotDotDot,
            DataContext = vm,
            Command = vm.PickFontCommand,
        };
        flyout.Items.Add(menuItemSetFont);
        textBoxText.ContextFlyout = flyout;
        vm.EditTextBox = textBoxText;
        textBoxText.PointerReleased += vm.TextBoxPointerReleased;
        textBoxText.KeyDown += vm.TextBoxKeyDown;

        textBoxText.TextChanged += vm.TextBoxTextChanged;
        var labelText = EditorChrome.MakeSectionHeader(Se.Language.General.Text);
        labelText.Margin = new Thickness(0, 0, 0, 8);
        textBoxText.WithLabeledBy(labelText);
        var panelText = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(14, 12, 14, 14),
        };
        panelText.Add(labelText, 0, 0);
        panelText.Add(textBoxText, 1, 0);

        var comboBoxDictionary = UiUtil.MakeComboBox(vm.Dictionaries, vm, nameof(vm.SelectedDictionary))
            .WithWidth(190)
            .WithAccessibleName(Se.Language.General.Dictionary);
        comboBoxDictionary.SelectionChanged += (sender, args) => vm.DictionaryChanged();
        var buttonDictionaryBrowse = EditorChrome.CreateToolButton("fa-solid fa-ellipsis",
            Se.Language.Video.VideoOcr.DownloadDictionaryHint, vm.PickDictionaryCommand, 28);
        var panelDictionary = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 3,
            Margin = new Thickness(0, 0, 0, 4),
            Children =
            {
                comboBoxDictionary,
                buttonDictionaryBrowse,
            }
        };

        var switchFixOcrErrors = EditorChrome.MakeSwitchRow(nameof(vm.DoFixOcrErrors), Se.Language.General.FixOcrErrors);
        switchFixOcrErrors.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsDictionaryLoaded)));
        var switchPromptForUnknownWords = EditorChrome.MakeSwitchRow(nameof(vm.DoPromptForUnknownWords), Se.Language.Ocr.PromptForUknownWords);
        switchPromptForUnknownWords.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsDictionaryLoaded)));
        var switchTryToGuessUnknownWords = EditorChrome.MakeSwitchRow(nameof(vm.DoTryToGuessUnknownWords), Se.Language.Ocr.TryToGuessUnknownWords);
        switchTryToGuessUnknownWords.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsDictionaryLoaded)));
        var switchAutoBreak = EditorChrome.MakeSwitchRow(nameof(vm.DoAutoBreak), string.Format(Se.Language.Ocr.AutoBreakIfMoreThanXLines, Se.Settings.General.MaxNumberOfLines));

        var panelOptions = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Spacing = 2,
            Width = 300,
            Children =
            {
                panelDictionary,
                switchFixOcrErrors,
                switchPromptForUnknownWords,
                switchTryToGuessUnknownWords,
                switchAutoBreak,
            }
        };

        // Scrolls on its own when the edit area is dragged short with the splitter.
        var optionsView = new ScrollViewer
        {
            Content = EditorChrome.MakeEditorSection(Se.Language.General.Dictionary, panelOptions, first: true),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var tabControl = new TabControl
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Padding = new Thickness(0, 6, 0, 0),
            Margin = new Thickness(10, 4, 14, 12),
            Items =
            {
                new TabItem
                {
                    Header = Se.Language.Ocr.UnknownWords,
                    Content = MakeUnknownWordsView(vm),
                },
                new TabItem
                {
                    Header = Se.Language.Ocr.AllFixes,
                    Content = MakeAllFixesView(vm)
                },
                new TabItem
                {
                    Header = Se.Language.Ocr.GuessesUsed,
                    Content = MakeGuessesUsedView(vm)
                },
            }
        };
        tabControl.Bind(TabControl.IsVisibleProperty, new Binding(nameof(vm.IsDictionaryLoaded)));

        var tabsView = new Border
        {
            Child = tabControl,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        };
        tabsView.Bind(Border.IsVisibleProperty, new Binding(nameof(vm.IsDictionaryLoaded)));

        grid.Add(panelText, 0, 0);
        grid.Add(new Border
        {
            Child = optionsView,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);
        grid.Add(tabsView, 0, 2);

        var border = new Border
        {
            Child = grid,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 0),
        };
        border.ClipToBounds = true; // Prevent content from pushing the parent row larger
        border.Bind(Border.IsVisibleProperty, new Binding(nameof(vm.IsOcrRunning)) { Source = vm, Converter = InverseBooleanConverter.Instance });

        return border;
    }

    private static Grid MakeUnknownWordsView(OcrViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var listBox = new ListBox
        {
            [Avalonia.Automation.AutomationProperties.NameProperty] = Se.Language.Ocr.UnknownWords,
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.UnknownWords)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedUnknownWord)) { Mode = BindingMode.TwoWay },
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        listBox.Styles.Add(new Style(x => x.OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(ListBoxItem.PaddingProperty, new Thickness(2)),
                new Setter(ListBoxItem.MarginProperty, new Thickness(0)),
            }
        });

        listBox.ItemTemplate = new FuncDataTemplate<UnknownWordItem>((item, _) =>
        {
            var tb = new TextBlock { Text = item?.ToString() };
            tb.Bind(TextBlock.FontFamilyProperty, new Binding(nameof(vm.TextBoxFontFamily)) { Source = vm, Mode = BindingMode.OneWay });
            return tb;
        });
        listBox.SelectionChanged += (s, e) => vm.UnknownWordSelectionChanged();
        listBox.Tapped += (s, e) => vm.UnknownWordSelectionTapped();
        listBox.KeyDown += (s, e) => vm.UnknownWordListKeyDown(e);


        var flyout = new MenuFlyout();
        var menuItemClear = new MenuItem
        {
            Header = Se.Language.General.Clear,
            DataContext = vm,
            Command = vm.UnknownWordsClearCommand,
        };
        flyout.Items.Add(menuItemClear);

        var menuItemRemoveCurrent = new MenuItem
        {
            DataContext = vm,
            Command = vm.UnknownWordsRemoveCurrentCommand,
        };
        menuItemRemoveCurrent.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsUnknownWordSelected)) { Mode = BindingMode.OneWay });
        menuItemRemoveCurrent.Bind(MenuItem.HeaderProperty, new Binding(nameof(vm.UnknownWordsRemoveCurrentText)) { Mode = BindingMode.OneWay });

        flyout.Items.Add(menuItemRemoveCurrent);

        listBox.ContextFlyout = flyout;


        var buttonAddToNamesList = MakeFlatTextButton(Se.Language.General.AddToNamesListCaseSensitive, vm.AddUnknownWordToNamesCommand)
            .WithBindEnabled(nameof(vm.IsUnknownWordSelected))
            .WithHorizontalAlignmentStretch();
        var buttonAddToUserDictionary = MakeFlatTextButton(Se.Language.General.AddToUserDictionary, vm.AddUnknownWordToUserDictionaryCommand)
            .WithBindEnabled(nameof(vm.IsUnknownWordSelected))
            .WithHorizontalAlignmentStretch();
        var buttonAddToOcrPair = MakeFlatTextButton(Se.Language.Ocr.AddToOcrPair, vm.AddUnknownWordToOcrPairCommand)
            .WithBindEnabled(nameof(vm.IsUnknownWordSelected))
            .WithHorizontalAlignmentStretch();
        var buttonGoogleIt = MakeFlatTextButton(Se.Language.General.GoogleIt, vm.GoogleUnknowWordCommand)
            .WithBindEnabled(nameof(vm.IsUnknownWordSelected))
            .WithHorizontalAlignmentStretch();
        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            Children =
            {
                buttonAddToNamesList,
                buttonAddToUserDictionary,
                buttonAddToOcrPair,
                buttonGoogleIt,
            },
        };

        grid.ColumnSpacing = 8;
        grid.Add(EditorChrome.MakeListFrame(listBox), 0, 0);
        grid.Add(panelButtons, 0, 1);

        return grid;
    }

    private static Grid MakeAllFixesView(OcrViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var listBox = new ListBox
        {
            [Avalonia.Automation.AutomationProperties.NameProperty] = Se.Language.Ocr.AllFixes,
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.AllFixes)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedAllFix)) { Mode = BindingMode.TwoWay },
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        listBox.Styles.Add(new Style(x => x.OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(ListBoxItem.PaddingProperty, new Thickness(2)),
                new Setter(ListBoxItem.MarginProperty, new Thickness(0)),
            }
        });

        listBox.ItemTemplate = new FuncDataTemplate<ReplacementUsedItem>((item, _) =>
        {
            var tb = new TextBlock { Text = item?.ToString() };
            tb.Bind(TextBlock.FontFamilyProperty, new Binding(nameof(vm.TextBoxFontFamily)) { Source = vm, Mode = BindingMode.OneWay });
            return tb;
        });
        ScrollViewer.SetHorizontalScrollBarVisibility(listBox, ScrollBarVisibility.Auto);
        listBox.SelectionChanged += (s, e) => vm.AllFixesTapped();
        listBox.Tapped += (s, e) => vm.AllFixesTapped();

        grid.Add(EditorChrome.MakeListFrame(listBox), 0, 0);

        return grid;
    }

    private static Grid MakeGuessesUsedView(OcrViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var listBox = new ListBox
        {
            [Avalonia.Automation.AutomationProperties.NameProperty] = Se.Language.Ocr.GuessesUsed,
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.AllGuesses)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedAllGuess)) { Mode = BindingMode.TwoWay },
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        listBox.Styles.Add(new Style(x => x.OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(ListBoxItem.PaddingProperty, new Thickness(2)),
                new Setter(ListBoxItem.MarginProperty, new Thickness(0)),
            }
        });
        listBox.ItemTemplate = new FuncDataTemplate<GuessUsedItem>((item, _) =>
        {
            var tb = new TextBlock { Text = item?.ToString() };
            tb.Bind(TextBlock.FontFamilyProperty, new Binding(nameof(vm.TextBoxFontFamily)) { Source = vm, Mode = BindingMode.OneWay });
            return tb;
        });
        listBox.SelectionChanged += (s, e) => vm.GuessUsedTapped();
        listBox.Tapped += (s, e) => vm.GuessUsedTapped();

        grid.Add(EditorChrome.MakeListFrame(listBox), 0, 0);

        return grid;
    }

    // The " - Running OCR..." suffix doubles as a cheap completion signal for
    // automation tools (e.g. AutoHotkey WinWaitClose on the title text), see #15769.
    private void UpdateTitle(OcrViewModel vm)
    {
        Title = vm.IsOcrRunning
            ? $"{vm.Title} - {Se.Language.Ocr.RunningOcrDotDotDot}"
            : vm.Title;
    }

    /// <summary>
    /// Progress bar and status in a strip above the buttons while the OCR runs.
    /// </summary>
    private static Border MakeProgressView(OcrViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Width = double.NaN;
        progressBar.HorizontalAlignment = HorizontalAlignment.Stretch;
        progressBar.VerticalAlignment = VerticalAlignment.Center;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        var statusText = EditorChrome.MakeBoundHint(nameof(vm.ProgressText), 12);
        statusText.HorizontalAlignment = HorizontalAlignment.Left;

        // Stable UI Automation ids for external automation (#15769)
        AutomationProperties.SetAutomationId(progressBar, "OcrProgressBar");
        AutomationProperties.SetAutomationId(statusText, "OcrProgressText");

        var grid = new Grid
        {
            RowSpacing = 4,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // progress bar
                new RowDefinition { Height = GridLength.Auto }, // status text
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        grid.Add(progressBar, 0, 0);
        grid.Add(statusText, 1, 0);

        var strip = EditorChrome.MakeProgressStrip(grid);
        strip.Bind(Border.IsVisibleProperty, new Binding(nameof(vm.IsOcrRunning)) { Source = vm });
        return strip;
    }

    private static Border MakeBottomView(OcrViewModel vm)
    {
        var buttonStart = UiUtil.MakeButton(Se.Language.Ocr.StartOcr, vm.StartOcrCommand)
            .WithBindIsVisible(nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance).AsPrimary();
        var buttonPause = UiUtil.MakeButton(Se.Language.Ocr.PauseOcr, vm.PauseOcrCommand)
            .WithBindIsVisible(nameof(OcrViewModel.IsOcrRunning)).AsPrimary();
        var buttonInspect = UiUtil.MakeButton(Se.Language.Ocr.InspectLine, vm.InspectLineCommand)
            .WithBindIsVisible(nameof(OcrViewModel.IsInspectLineVisible))
            .WithBindIsEnabled(nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance);
        var buttonInspectAdditions = UiUtil.MakeButton(Se.Language.General.InspectAdditions, vm.InspectAdditionsCommand)
            .WithBindIsVisible(nameof(vm.IsInspectAdditionsVisible))
            .WithBindIsEnabled(nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance);
        var buttonExport = UiUtil.MakeButton(Se.Language.Ocr.EditExportDotDotDot, vm.EditExportCommand)
            .WithBindIsEnabled(nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand)
            .WithBindIsEnabled(nameof(OcrViewModel.IsOcrRunning), InverseBooleanConverter.Instance);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var subtitleCountText = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        subtitleCountText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.SelectionStatus)) { Source = vm });
        subtitleCountText.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.IsOcrRunning)) { Source = vm, Converter = InverseBooleanConverter.Instance });

        // Stable UI Automation ids for external automation (#15769)
        AutomationProperties.SetAutomationId(subtitleCountText, "OcrSelectionStatus");
        AutomationProperties.SetAutomationId(buttonStart, "OcrStartButton");
        AutomationProperties.SetAutomationId(buttonPause, "OcrPauseButton");
        AutomationProperties.SetAutomationId(buttonInspect, "OcrInspectLineButton");
        AutomationProperties.SetAutomationId(buttonInspectAdditions, "OcrInspectAdditionsButton");
        AutomationProperties.SetAutomationId(buttonExport, "OcrExportButton");
        AutomationProperties.SetAutomationId(buttonOk, "OcrOkButton");
        AutomationProperties.SetAutomationId(buttonCancel, "OcrCancelButton");

        return EditorChrome.MakeButtonBar(subtitleCountText, buttonInspect, buttonInspectAdditions, buttonExport, buttonStart, buttonPause, buttonOk, buttonCancel);
    }

    /// <summary>
    /// Flat bordered text button (llama.cpp server toggle, unknown word actions).
    /// </summary>
    private static Button MakeFlatTextButton(string text, System.Windows.Input.ICommand? command)
    {
        var button = new Button
        {
            Content = text,
            Command = command,
            Height = 28,
            FontSize = EditorChrome.FieldFontSize,
            Padding = new Thickness(10, 0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
        };
        return button;
    }

    /// <summary>
    /// Flat icon toggles: the editor "tool" look for toggle buttons, lit in the accent while on.
    /// </summary>
    private static void AddOcrStyles(Styles styles)
    {
        static Style Presenter(Func<Selector?, Selector> toggle, params (AvaloniaProperty Property, object Value)[] setters)
        {
            var style = new Style(x => toggle(x).Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            foreach (var (property, value) in setters)
            {
                style.Setters.Add(new Setter(property, value));
            }

            return style;
        }

        styles.Add(new Style(x => x.OfType<ToggleButton>().Class("tool"))
        {
            Setters =
            {
                new Setter(ToggleButton.BackgroundProperty, Brushes.Transparent),
                new Setter(ToggleButton.BorderThicknessProperty, new Thickness(0)),
                new Setter(ToggleButton.CornerRadiusProperty, new CornerRadius(7)),
                new Setter(ToggleButton.ForegroundProperty, EditorChrome.IconBrush),
                new Setter(ToggleButton.PaddingProperty, new Thickness(0)),
                new Setter(ToggleButton.FontSizeProperty, 15.0),
            },
        });
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("tool").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, EditorChrome.HoverBrush), (ContentPresenter.ForegroundProperty, EditorChrome.StrongTextBrush)));
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("tool").Class(":pressed"),
            (ContentPresenter.BackgroundProperty, EditorChrome.PressedBrush)));
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("tool").Class(":checked"),
            (ContentPresenter.BackgroundProperty, EditorChrome.AccentBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("tool").Class(":checked").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, EditorChrome.AccentHoverBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("tool").Class(":disabled"),
            (ContentPresenter.BackgroundProperty, Brushes.Transparent), (ContentPresenter.ForegroundProperty, EditorChrome.SeparatorBrush)));

        // The lists in the unknown words tabs sit in a field-colored frame
        styles.Add(new Style(x => x.OfType<ListBox>())
        {
            Setters = { new Setter(ListBox.BackgroundProperty, Brushes.Transparent) },
        });
    }

    private static Button MakeLlamaCppOcrToggleServerButton(OcrViewModel vm)
    {
        var button = MakeFlatTextButton(string.Empty, vm.ToggleLlamaCppOcrServerCommand);
        button.Bind(Button.ContentProperty, new Binding(nameof(vm.LlamaCppOcrServerButtonText)));
        return button;
    }

    // Engine combo item template: CrispEmbed and llama.cpp are the OCR engines with a locally
    // tracked install, so they get the install-status dot (green = ready, amber = update
    // available, grey = not downloaded yet); CrispEmbed also shows its download size until it is
    // on disk. Engines with nothing for us to download show no dot.
    private static FuncDataTemplate<OcrEngineItem> MakeOcrEngineItemTemplate()
    {
        return StatusDots.ComboItemTemplate<OcrEngineItem>(
            engine => engine.Name,
            engine => engine.EngineType == OcrEngineType.CrispEmbed && !CrispEmbedEngine.IsEngineInstalled()
                ? CrispEmbedEngine.DownloadSizeText
                : null,
            GetOcrEngineDotStatus);
    }

    private static DownloadDotStatus GetOcrEngineDotStatus(OcrEngineItem engine)
    {
        switch (engine.EngineType)
        {
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

    // Model combo item template: a dot (green = downloaded, grey = not downloaded yet) plus the
    // model's download size - same treatment as the speech-to-text model combo.
    // 0 = auto: word spaces relative to the text size (NOcrSpaceDetector).
    // Text is bound (not set once) - the template recycles, so the closed combo's presenter
    // keeps its TextBlock and only gets a new DataContext when the selection changes (#16187).
    private static ComboBox WithPixelsAreSpaceItemTemplate(ComboBox comboBox)
    {
        comboBox.ItemTemplate = new FuncDataTemplate<int>((_, _) => new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(".")
            {
                Converter = new FuncValueConverter<int, string>(value =>
                    value <= 0 ? Se.Language.General.Auto : value.ToString(CultureInfo.CurrentCulture)),
            },
        }, true);
        return comboBox;
    }

    private static FuncDataTemplate<CrispEmbedModelDisplay> MakeCrispEmbedModelItemTemplate()
    {
        return StatusDots.ComboItemTemplate<CrispEmbedModelDisplay>(
            model => model.Model.Name,
            model => string.IsNullOrEmpty(model.Model.Size) ? null : model.Model.Size,
            model => model.Backend.IsModelInstalled(model.Model)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled);
    }
}
