using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System.Linq;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public class SpeechToTextWindow : Window
{
    public SpeechToTextWindow(SpeechToTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.AudioToText.Title;
        Width = 1100;
        Height = 680;
        MinWidth = 800;
        MinHeight = 680;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var labelConsoleLog = new TextBlock
        {
            Text = Se.Language.General.ConsoleLog,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var buttonCopyConsoleLog = UiUtil.MakeButton(
            vm.CopyConsoleLogCommand,
            IconNames.Copy,
            Se.Language.General.CopyTextToClipboard);
        vm.CopyConsoleLogButton = buttonCopyConsoleLog;
        var panelConsoleLogHeader = UiUtil.MakeHorizontalPanel(labelConsoleLog, buttonCopyConsoleLog);
        panelConsoleLogHeader.Margin = new Thickness(10, 12, 10, 10);

        var consoleLogAndBatchView = MakeConsoleLogAndBatchView(vm);
        var consoleLogOnlyView = MakeConsoleLogOnlyView(vm);

        var labelEngine = UiUtil.MakeTextBlock(Se.Language.General.Engine).WithMarginTop(10);
        var comboEngine = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine))
            .WithMinWidth(260)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithMarginTop(10)
            .WithLabeledBy(labelEngine);
        comboEngine.ItemTemplate = MakeEngineItemTemplate();
        comboEngine.SelectionChanged += vm.OnEngineChanged;

        // Force the combo to rebuild its row visuals after a re-download finishes, so the
        // install-status dot reflects the freshly-written .installed.sha256 sidecar instead
        // of the snapshot taken when each row was first realised.
        vm.RefreshEngineCombo = () => comboEngine.ItemTemplate = MakeEngineItemTemplate();
        var buttonEngineWebsite = UiUtil.MakeButton(vm.ShowWebLinkCommand, IconNames.Web)
            .WithMarginLeft(5)
            .WithMarginTop(10);
        var buttonEngineDownload = UiUtil.MakeButton(vm.DownloadSelectedEngineCommand, IconNames.Download)
            .WithMarginLeft(5)
            .WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsEngineDownloadButtonVisible))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithAccessibleName(Se.Language.General.Download);
        buttonEngineDownload.Bind(ToolTip.TipProperty, new Binding(nameof(vm.EngineDownloadHint))
        {
            Source = vm,
            Mode = BindingMode.OneWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });

        var buttonEngineSettings = UiUtil.MakeButton(vm.ShowEngineSettingsCommand, IconNames.Settings)
            .WithMarginLeft(5)
            .WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsEngineSettingsButtonVisible))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithAccessibleName(Se.Language.Video.AudioToText.BackendAndUpdateStatus);
        ToolTip.SetTip(buttonEngineSettings, Se.Language.Video.AudioToText.BackendAndUpdateStatus);

        var panelEngineControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                comboEngine,
                buttonEngineWebsite,
                buttonEngineDownload,
                buttonEngineSettings,
            }
        };

        var labelBackend = UiUtil.MakeTextBlock(Se.Language.General.Backend).WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsBackendSelectionVisible));
        var comboWhisperCppBackend = UiUtil.MakeComboBox(vm.WhisperCppBackends, vm, nameof(vm.SelectedWhisperCppBackend))
            .WithMinWidth(220)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsWhisperCppSelected))
            .WithLabeledBy(labelBackend);
        var comboCrispAsrBackend = UiUtil.MakeComboBox(vm.CrispAsrBackends, vm, nameof(vm.SelectedCrispAsrBackend))
            .WithMinWidth(220)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsCrispAsrSelected))
            .WithLabeledBy(labelBackend);

        var labelForcedAligner = UiUtil.MakeTextBlock("Forced aligner").WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsForcedAlignerVisible));
        var comboForcedAligner = UiUtil.MakeComboBox(vm.ForcedAligners, vm, nameof(vm.SelectedForcedAligner))
            .WithMinWidth(220)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithMarginTop(10)
            .WithLabeledBy(labelForcedAligner);
        comboForcedAligner.ItemTemplate = MakeForcedAlignerItemTemplate();
        var buttonForcedAlignerDownload = UiUtil.MakeButtonBrowse(vm.DownloadForcedAlignerCommand, accessibleName: Se.Language.General.Download)
            .WithMarginTop(10)
            .WithMarginLeft(5)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));
        var panelForcedAlignerControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                comboForcedAligner,
                buttonForcedAlignerDownload,
            },
        }.BindIsVisible(vm, nameof(vm.IsForcedAlignerVisible));

        var labelLanguage = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.InputLanguage).WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsLanguageSelectionVisible));
        var comboLanguage = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage))
            .WithMinWidth(220)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsLanguageSelectionVisible))
            .WithLabeledBy(labelLanguage);

        var labelModel = UiUtil.MakeTextBlock(Se.Language.General.Model).WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsModelSelectionVisible));
        var comboModel = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel))
            .WithMinWidth(220)
            .WithMarginTop(10)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .BindIsVisible(vm, nameof(vm.IsModelSelectionVisible))
            .WithLabeledBy(labelModel);
        comboModel.ItemTemplate = MakeModelItemTemplate();

        var buttonModelDownload = UiUtil.MakeButtonBrowse(vm.DownloadModelCommand, accessibleName: Se.Language.General.Download)
            .WithMarginTop(10)
            .WithMarginLeft(5)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .BindIsVisible(vm, nameof(vm.IsModelSelectionVisible));

        var panelModelControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                comboModel,
                buttonModelDownload
            }
        };

        var labelTranslateToEnglish = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.TranslateToEnglish)
            .WithMarginTop(20)
            .BindIsVisible(vm, nameof(vm.IsTranslateVisible));
        var checkTranslateToEnglish = UiUtil.MakeCheckBox(vm, nameof(vm.DoTranslateToEnglish))
            .WithMarginTop(20)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .BindIsVisible(vm, nameof(vm.IsTranslateVisible))
            .WithLabeledBy(labelTranslateToEnglish);

        var labelPostProcessing = UiUtil.MakeTextBlock(Se.Language.General.PostProcessing).WithMarginTop(15);
        var checkPostProcessing = UiUtil.MakeCheckBox(vm, nameof(vm.DoPostProcessing)).BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithLabeledBy(labelPostProcessing);
        var buttonPostProcessing = UiUtil.MakeButton(vm.ShowPostProcessingSettingsCommand, IconNames.Settings)
                    .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
                    .WithAccessibleName(Se.Language.General.PostProcessing);

        var panelPostProcessingControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 15, 0, 0),
            Children =
            {
                checkPostProcessing,
                buttonPostProcessing
            }
        };

        var openAiRows = MakeOpenAiCompatibleSttRows(vm);
        var openRouterRows = MakeOpenRouterSttRows(vm);
        var dashScopeRows = MakeDashScopeSttRows(vm);

        var labelAdvancedSettings = UiUtil.MakeTextBlock(Se.Language.General.AdvancedSettings).WithMarginTop(15)
            .BindIsVisible(vm, nameof(vm.IsAdvancedSettingsVisible));

        var buttonAdvancedSettings = UiUtil.MakeButton(vm.ShowAdvancedSettingsCommand, IconNames.Settings)
                    .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
                    .WithMarginTop(15)
                    .BindIsVisible(vm, nameof(vm.IsAdvancedSettingsVisible))
                    .WithAccessibleName(Se.Language.General.AdvancedSettings);

        var textBoxAdvancedSettings = new TextBox()
        {
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
            IsReadOnly = true,
            FontSize = 12,
            Margin = new Thickness(0),
            Opacity = 0.6,
            BorderThickness = new Thickness(0),
            MaxWidth = 320,
        }.BindIsVisible(vm, nameof(vm.IsAdvancedSettingsVisible))
            .WithLabeledBy(labelAdvancedSettings);
        textBoxAdvancedSettings.Bind(TextBox.TextProperty, new Binding
        {
            Path = nameof(vm.Parameters),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        var progressBar = UiUtil.MakeProgressBar();
        AutomationProperties.SetName(progressBar, Se.Language.Video.AudioToText.Transcribe);
        progressBar.Margin = new Thickness(10, 0, 10, 8);
        progressBar.Width = double.NaN;
        progressBar.VerticalAlignment = VerticalAlignment.Center;
        progressBar.HorizontalAlignment = HorizontalAlignment.Stretch;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding
        {
            Path = nameof(vm.ProgressValue),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });
        progressBar.Bind(ProgressBar.OpacityProperty, new Binding
        {
            Path = nameof(vm.ProgressOpacity),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });

        var progressText = new TextBlock()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 10, 0),
        };
        progressText.Bind(TextBlock.TextProperty, new Binding
        {
            Path = nameof(vm.ProgressText),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        progressText.Bind(TextBlock.OpacityProperty, new Binding
        {
            Path = nameof(vm.ProgressOpacity),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        var estimatedTimeText = new TextBlock()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 10, 0),
        };
        estimatedTimeText.Bind(TextBlock.TextProperty, new Binding
        {
            Path = nameof(vm.EstimatedText),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        estimatedTimeText.Bind(TextBlock.OpacityProperty, new Binding
        {
            Path = nameof(vm.ProgressOpacity),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        var elapsedTimeText = new TextBlock()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 10, 0),
        };
        elapsedTimeText.Bind(TextBlock.TextProperty, new Binding
        {
            Path = nameof(vm.ElapsedText),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        elapsedTimeText.Bind(TextBlock.OpacityProperty, new Binding
        {
            Path = nameof(vm.ProgressOpacity),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });


        var panelProgress = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 5, 0, 0),
            Children =
            {
                progressBar,
                progressText,
                elapsedTimeText,
                estimatedTimeText,
            },
        };

        var buttonBatchMode = UiUtil.MakeButton(Se.Language.General.BatchMode, vm.BatchModeCommand)
            .WithBindIsVisible(nameof(vm.IsBatchModeVisible))
            .WithBindEnabled(nameof(vm.IsTranscribeEnabled));
        var buttonSingleMode = UiUtil.MakeButton(Se.Language.General.SingleMode, vm.SingleModeCommand)
            .WithBindIsVisible(nameof(vm.IsSingleModeVisible))
            .WithBindEnabled(nameof(vm.IsTranscribeEnabled));
        var transcribeButton = UiUtil.MakeButton(Se.Language.Video.AudioToText.Transcribe, vm.TranscribeCommand).BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));
        var buttonPanel = UiUtil.MakeButtonBar(
            transcribeButton,
            buttonBatchMode,
            buttonSingleMode,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );
        buttonPanel.Margin = new Thickness(10, 0, 10, 10);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 0: Console log label
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // 1: Console log (right) — Star fills remaining
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 2: Engine
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 3: Backend
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 4: Language
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 5: Model
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 6: Forced aligner
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 7: OpenAI STT - Endpoint URL
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 8: OpenAI STT - API Key
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 9: OpenAI STT - Model
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 10: OpenAI STT - Language
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 11: OpenAI STT - Timeout
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 12: OpenAI STT - Temperature
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 13: OpenAI STT - Prompt
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 14: OpenAI STT - Extra Headers
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 15: OpenAI STT - Audio Format
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 16: OpenAI STT - Stream
                // OpenRouter STT rows (6) — only visible when OpenRouter is selected, else collapse to 0.
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 17: OpenRouter - API Key
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 18: OpenRouter - Model
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 19: OpenRouter - Language
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 20: OpenRouter - Timeout
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 21: OpenRouter - Temperature
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 22: OpenRouter - Prompt
                // Alibaba Qwen3-ASR (DashScope) STT rows (6).
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 23: DashScope - API Key
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 24: DashScope - Model
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 25: DashScope - Region
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 26: DashScope - Language
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 27: DashScope - Word timestamps
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 28: DashScope - Timeout
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 29: Translate to English
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 30: Post processing
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 31: Advanced settings label + button
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 32: textBoxAdvancedSettings (parameters)
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // 33: panelProgress + OK/Cancel (same row)
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var flyout = new MenuFlyout();
        flyout.Opening += vm.WindowContextMenuOpening;
        // Attach the flyout to the root Grid rather than the Window itself — on macOS
        // right-clicks on bare window chrome don't reach `Window.ContextFlyout`.
        grid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(this, grid);

        var menuItemViewToolsLog = new MenuItem
        {
            Header = Se.Language.Video.AudioToText.ViewToolsLogFile,
            DataContext = vm,
            Command = vm.ViewToolsLogFileCommand,
        };
        flyout.Items.Add(menuItemViewToolsLog);

        var menuItemReDownloadWhisperEngine = new MenuItem
        {
            DataContext = vm,
            Command = vm.ReDownloadWhisperEngineCommand,
        };
        menuItemReDownloadWhisperEngine.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsReDownloadVisible)) { Source = vm });
        menuItemReDownloadWhisperEngine.Bind(MenuItem.HeaderProperty, new Binding(nameof(vm.ReDownloadText)) { Source = vm });
        flyout.Items.Add(menuItemReDownloadWhisperEngine);


        var row = 0;

        grid.Add(panelConsoleLogHeader, row, 2);
        row++;

        // Row span must reach the row just above the progress/buttons row so the
        // console log fills the full height of the settings column. It covers every
        // settings row including the OpenAI/OpenRouter/DashScope online-STT rows.
        grid.Add(consoleLogAndBatchView, row, 2, 32);
        grid.Add(consoleLogOnlyView, row, 2, 32);
        row++;

        grid.Add(labelEngine, row, 0);
        grid.Add(panelEngineControls, row, 1);
        row++;

        grid.Add(labelBackend, row, 0);
        grid.Add(comboWhisperCppBackend, row, 1);
        grid.Add(comboCrispAsrBackend, row, 1);
        row++;

        grid.Add(labelLanguage, row, 0);
        grid.Add(comboLanguage, row, 1);
        row++;

        grid.Add(labelModel, row, 0);
        grid.Add(panelModelControls, row, 1);
        row++;

        grid.Add(labelForcedAligner, row, 0);
        grid.Add(panelForcedAlignerControls, row, 1);
        row++;

        foreach (var (label, control) in openAiRows.Concat(openRouterRows).Concat(dashScopeRows))
        {
            // Link each online-STT input to its visible label so a screen reader announces the
            // label as the control's name instead of a bare "edit"/"combo box" (#11745). Only the
            // selected engine's rows are visible; the rest collapse to zero-height grid rows.
            AutomationProperties.SetLabeledBy(control, label);
            grid.Add(label, row, 0);
            grid.Add(control, row, 1);
            row++;
        }

        grid.Add(labelTranslateToEnglish, row, 0);
        grid.Add(checkTranslateToEnglish, row, 1);
        row++;

        grid.Add(labelPostProcessing, row, 0);
        grid.Add(panelPostProcessingControls, row, 1);
        row++;

        grid.Add(labelAdvancedSettings, row, 0);
        grid.Add(buttonAdvancedSettings, row, 1);
        row++;

        grid.Add(textBoxAdvancedSettings, row, 0, 1, 2);
        row++;

        grid.Add(panelProgress, row, 0, 1, 3);
        grid.Add(buttonPanel, row, 0, 1, 3);

        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
        Loaded += (s, e) => vm.OnWindowLoaded();
        Closing += (s, e) => vm.OnWindowClosing(e);
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static Grid MakeConsoleLogAndBatchView(SpeechToTextViewModel vm)
    {
        var textBoxConsoleLog = new TextBox()
        {
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsReadOnly = true,
            Margin = new Thickness(0, 0, 0, 10),
        };
        AutomationProperties.SetName(textBoxConsoleLog, Se.Language.General.ConsoleLog);
        textBoxConsoleLog.Bind(TextBox.TextProperty, new Binding
        {
            Path = nameof(vm.ConsoleLog),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        vm.TextBoxConsoleLogBatch = textBoxConsoleLog;


        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.BatchItems,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FileName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SpeechToTextJobItem.InputVideoFileNameShort)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Size,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SpeechToTextJobItem.SizeDisplay)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Status,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SpeechToTextJobItem.Status)),
                    IsReadOnly = true,
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedBatchItem)) { Source = vm });
        AutomationProperties.SetName(dataGrid, Se.Language.General.BatchMode);
        vm.BatchGrid = dataGrid;

        var buttonAdd = UiUtil.MakeButton(Se.Language.General.AddDotDotDot, vm.AddCommand).BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));
        var buttonRemove = UiUtil.MakeButton(Se.Language.General.Remove, vm.RemoveCommand).BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));
        var buttonClear = UiUtil.MakeButton(Se.Language.General.Clear, vm.ClearCommand).BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));

        var panelFileControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                buttonAdd,
                buttonRemove,
                buttonClear,
            }
        };

        var gridBatch = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Batch view
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Batch view buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        // hack to make drag and drop work on the DataGrid - also on empty rows
        var dropHost = new Border
        {
            Background = Brushes.Transparent,
            Child = dataGrid,
        };
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, vm.FileGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, vm.FileGridOnDrop, RoutingStrategies.Bubble);

        gridBatch.Add(dropHost, 0, 0);
        gridBatch.Add(panelFileControls, 1, 0);

        var borderBatch = UiUtil.MakeBorderForControl(gridBatch);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Console log
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Batch view
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(textBoxConsoleLog, 0, 0);
        grid.Add(borderBatch, 1, 0);

        grid.WithBindVisible(vm, nameof(vm.IsBatchMode));

        return grid;
    }

    private static TextBox MakeConsoleLogOnlyView(SpeechToTextViewModel vm)
    {
        var textBoxConsoleLog = new TextBox()
        {
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsReadOnly = true,
            Margin = new Thickness(10),
        };
        AutomationProperties.SetName(textBoxConsoleLog, Se.Language.General.ConsoleLog);
        textBoxConsoleLog.Bind(TextBox.TextProperty, new Binding
        {
            Path = nameof(vm.ConsoleLog),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        textBoxConsoleLog.WithBindIsVisible(nameof(vm.IsBatchMode), new InverseBooleanConverter());
        vm.TextBoxConsoleLogSingle = textBoxConsoleLog;

        return textBoxConsoleLog;
    }

    private static (Control Label, Control Control)[] MakeOpenAiCompatibleSttRows(SpeechToTextViewModel vm)
    {
        Control MakeLabel(string text) => UiUtil.MakeTextBlock(text).WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsOpenAiCompatibleSttVisible));

        TextBox MakeText(string property, double width, bool isPassword = false)
        {
            var tb = new TextBox
            {
                DataContext = vm,
                Width = width,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0),
                [!TextBox.TextProperty] = new Binding(property) { Mode = BindingMode.TwoWay }
            };
            if (isPassword)
            {
                tb.PasswordChar = '*';
            }
            return tb.BindIsVisible(vm, nameof(vm.IsOpenAiCompatibleSttVisible));
        }

        var numericTimeout = new NumericUpDown
        {
            DataContext = vm,
            Width = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0),
            Minimum = 10,
            Maximum = 3600,
            FormatString = "F0",
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.OpenAiCompatibleSttTimeoutSeconds)) { Mode = BindingMode.TwoWay }
        }.BindIsVisible(vm, nameof(vm.IsOpenAiCompatibleSttVisible));

        var numericTemperature = new NumericUpDown
        {
            DataContext = vm,
            Width = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0),
            Minimum = 0,
            Maximum = 1,
            Increment = 0.1m,
            FormatString = "F2",
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.OpenAiCompatibleSttTemperature)) { Mode = BindingMode.TwoWay }
        }.BindIsVisible(vm, nameof(vm.IsOpenAiCompatibleSttVisible));

        var textExtraHeaders = new TextBox
        {
            DataContext = vm,
            Width = 400,
            Height = 60,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0),
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            [!TextBox.TextProperty] = new Binding(nameof(vm.OpenAiCompatibleSttExtraHeaders)) { Mode = BindingMode.TwoWay }
        }.BindIsVisible(vm, nameof(vm.IsOpenAiCompatibleSttVisible));

        var checkStream = UiUtil.MakeCheckBox(vm, nameof(vm.OpenAiCompatibleSttStream))
            .BindIsVisible(vm, nameof(vm.IsOpenAiCompatibleSttVisible));
        ToolTip.SetTip(checkStream, Se.Language.General.OpenAiCompatibleSttStreamHint);

        var comboAudioFormat = UiUtil.MakeComboBox(vm.OpenAiCompatibleSttAudioFormats, vm, nameof(vm.OpenAiCompatibleSttAudioFormat))
            .WithMinWidth(120)
            .WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsOpenAiCompatibleSttVisible));
        ToolTip.SetTip(comboAudioFormat, Se.Language.General.OpenAiCompatibleSttAudioFormatHint);

        return new (Control, Control)[]
        {
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttEndpoint), MakeText(nameof(vm.OpenAiCompatibleSttUrl), 400)),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttApiKey), MakeText(nameof(vm.OpenAiCompatibleSttApiKey), 400, isPassword: true)),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttModel), MakeText(nameof(vm.OpenAiCompatibleSttModel), 250)),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttLanguage), MakeText(nameof(vm.OpenAiCompatibleSttLanguage), 150)),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttTimeout), numericTimeout),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttTemperature), numericTemperature),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttPrompt), MakeText(nameof(vm.OpenAiCompatibleSttPrompt), 400)),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttExtraHeaders), textExtraHeaders),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttAudioFormat), comboAudioFormat),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttStream), checkStream),
        };
    }

    private static (Control Label, Control Control)[] MakeOpenRouterSttRows(SpeechToTextViewModel vm)
    {
        Control MakeLabel(string text) => UiUtil.MakeTextBlock(text).WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsOpenRouterSttVisible));

        TextBox MakeText(string property, double width, bool isPassword = false)
        {
            var tb = new TextBox
            {
                DataContext = vm,
                Width = width,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0),
                [!TextBox.TextProperty] = new Binding(property) { Mode = BindingMode.TwoWay }
            };
            if (isPassword)
            {
                tb.PasswordChar = '*';
            }
            return tb.BindIsVisible(vm, nameof(vm.IsOpenRouterSttVisible));
        }

        var numericTimeout = new NumericUpDown
        {
            DataContext = vm,
            Width = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0),
            Minimum = 10,
            Maximum = 3600,
            FormatString = "F0",
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.OpenRouterSttTimeoutSeconds)) { Mode = BindingMode.TwoWay }
        }.BindIsVisible(vm, nameof(vm.IsOpenRouterSttVisible));

        var numericTemperature = new NumericUpDown
        {
            DataContext = vm,
            Width = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0),
            Minimum = 0,
            Maximum = 1,
            Increment = 0.1m,
            FormatString = "F2",
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.OpenRouterSttTemperature)) { Mode = BindingMode.TwoWay }
        }.BindIsVisible(vm, nameof(vm.IsOpenRouterSttVisible));

        return new (Control, Control)[]
        {
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttApiKey), MakeText(nameof(vm.OpenRouterSttApiKey), 400, isPassword: true)),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttModel), MakeText(nameof(vm.OpenRouterSttModel), 250)),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttLanguage), MakeText(nameof(vm.OpenRouterSttLanguage), 150)),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttTimeout), numericTimeout),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttTemperature), numericTemperature),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttPrompt), MakeText(nameof(vm.OpenRouterSttPrompt), 400)),
        };
    }

    private static (Control Label, Control Control)[] MakeDashScopeSttRows(SpeechToTextViewModel vm)
    {
        Control MakeLabel(string text) => UiUtil.MakeTextBlock(text).WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsDashScopeSttVisible));

        TextBox MakeText(string property, double width, bool isPassword = false)
        {
            var tb = new TextBox
            {
                DataContext = vm,
                Width = width,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0),
                [!TextBox.TextProperty] = new Binding(property) { Mode = BindingMode.TwoWay }
            };
            if (isPassword)
            {
                tb.PasswordChar = '*';
            }
            return tb.BindIsVisible(vm, nameof(vm.IsDashScopeSttVisible));
        }

        var comboRegion = UiUtil.MakeComboBox(vm.DashScopeSttRegions, vm, nameof(vm.DashScopeSttRegion))
            .WithMinWidth(150)
            .WithMarginTop(10)
            .BindIsVisible(vm, nameof(vm.IsDashScopeSttVisible));

        var checkEnableWords = UiUtil.MakeCheckBox(vm, nameof(vm.DashScopeSttEnableWords))
            .BindIsVisible(vm, nameof(vm.IsDashScopeSttVisible));

        var numericTimeout = new NumericUpDown
        {
            DataContext = vm,
            Width = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0),
            Minimum = 30,
            Maximum = 21600,
            FormatString = "F0",
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.DashScopeSttTimeoutSeconds)) { Mode = BindingMode.TwoWay }
        }.BindIsVisible(vm, nameof(vm.IsDashScopeSttVisible));

        return new (Control, Control)[]
        {
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttApiKey), MakeText(nameof(vm.DashScopeSttApiKey), 400, isPassword: true)),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttModel), MakeText(nameof(vm.DashScopeSttModel), 250)),
            (MakeLabel(Se.Language.General.DashScopeSttRegion), comboRegion),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttLanguage), MakeText(nameof(vm.DashScopeSttLanguage), 150)),
            (MakeLabel(Se.Language.General.DashScopeSttEnableWords), checkEnableWords),
            (MakeLabel(Se.Language.General.OpenAiCompatibleSttTimeout), numericTimeout),
        };
    }

    // Engine combo item template: each row gets a small install-status dot (green = ready,
    // amber = update available, grey = not downloaded yet, none for cloud/API engines) plus the
    // approximate download size for engines that still need downloading.
    private static FuncDataTemplate<ISpeechToTextEngine> MakeEngineItemTemplate()
    {
        return StatusDots.ComboItemTemplate<ISpeechToTextEngine>(
            engine => engine.Name,
            engine => engine.CanBeDownloaded() && !engine.IsEngineInstalled() && !string.IsNullOrEmpty(engine.DownloadSizeText)
                ? engine.DownloadSizeText
                : null,
            GetEngineDotStatus);
    }

    private static DownloadDotStatus GetEngineDotStatus(ISpeechToTextEngine engine)
    {
        if (!engine.CanBeDownloaded())
        {
            // MLX Whisper is installed via pip rather than downloaded by Subtitle Edit,
            // but it still has a local install state worth surfacing.
            if (engine is MlxWhisperMac)
            {
                return engine.IsEngineInstalled() ? DownloadDotStatus.UpToDate : DownloadDotStatus.NotInstalled;
            }

            return DownloadDotStatus.None; // cloud / API engine - nothing to install
        }

        if (!engine.IsEngineInstalled())
        {
            return DownloadDotStatus.NotInstalled;
        }

        // Installed: read the cheap .installed.sha256 sidecar so an outdated build shows amber.
        return StatusDots.From(true, DownloadHashManager.GetSidecarStatus(engine.GetAndCreateWhisperFolder()));
    }

    // Model combo item template: a dot (green = downloaded, grey = not downloaded yet) plus the
    // model's download size, replacing the old "..., not installed" text suffix.
    private static FuncDataTemplate<SpeechToTextModelDisplay> MakeModelItemTemplate()
    {
        return StatusDots.ComboItemTemplate<SpeechToTextModelDisplay>(
            model => model.Model.Name,
            model => string.IsNullOrEmpty(model.Model.Size) ? null : model.Model.Size,
            model => model.Engine.IsModelInstalled(model.Model)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled);
    }

    // Forced-aligner combo item template: same dot + download-size treatment as the model combo.
    // The built-in aligner needs no download, so it gets no dot or size; downloadable aligners show
    // green (on disk) or grey (not downloaded yet). Install state is snapshotted on each option by
    // the view model when it rebuilds the list.
    private static FuncDataTemplate<ForcedAlignerOption> MakeForcedAlignerItemTemplate()
    {
        return StatusDots.ComboItemTemplate<ForcedAlignerOption>(
            aligner => aligner.BaseDisplay,
            aligner => aligner.IsBuiltIn ? null : aligner.Size,
            aligner => aligner.IsBuiltIn
                ? DownloadDotStatus.None
                : aligner.IsInstalled
                    ? DownloadDotStatus.UpToDate
                    : DownloadDotStatus.NotInstalled);
    }
}
