using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Platform.Progress;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

/// <summary>
/// Speech to text, in the editor look (see <see cref="EditorChrome"/>): engine, model and options
/// in a scrolling column on the left, the console log (and in batch mode the file list) on the
/// right, progress and the Single/Batch switch, Cancel and Transcribe at the bottom.
/// </summary>
public class SpeechToTextWindow : Window
{
    private const double LabelWidth = 96;
    private const double SettingsWidth = 410;

    /// <summary>
    /// The settings column (engine, model, options, online engine settings, advanced).
    /// </summary>
    internal const string SettingsPanelName = "SpeechToTextSettings";

    /// <summary>
    /// The progress strip above the button bar.
    /// </summary>
    internal const string ProgressPanelName = "SpeechToTextProgress";

    /// <summary>
    /// The bottom button bar.
    /// </summary>
    internal const string ButtonBarName = "SpeechToTextButtonBar";

    public SpeechToTextWindow(SpeechToTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.AudioToText.Title;
        Width = 1100;
        Height = 680;
        MinWidth = 820;
        MinHeight = 480;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.SpeechToText);
        vm.Window = this;
        DataContext = vm;

        // The settings column scrolls on its own and the console log gives up height first, so
        // the progress strip and the buttons stay reachable on a short screen - and no settings
        // row can ever end up on top of the progress text again.
        var settingsScroller = new ScrollViewer
        {
            Content = MakeSettingsPanel(vm),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        var settings = new Border
        {
            Name = SettingsPanelName,
            Child = settingsScroller,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(SettingsWidth)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        content.Add(settings, 0, 0);
        content.Add(MakeLogPanel(vm), 0, 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)), // settings + console log / batch list
                new RowDefinition(GridLength.Auto), // progress, while transcribing
                new RowDefinition(GridLength.Auto), // mode, buttons
            },
        };
        grid.Add(content, 0, 0);
        grid.Add(MakeProgressView(vm), 1, 0);
        grid.Add(MakeButtonBar(vm), 2, 0);

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

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, () => { Focus(); }); // hack to make OnKeyDown work
        Loaded += (s, e) => vm.OnWindowLoaded();
        Closing += (s, e) => vm.OnWindowClosing(e);
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static TextBlock Label(string text) => UiUtil.MakeTextBlock(text);

    private static StackPanel MakeSettingsPanel(SpeechToTextViewModel vm)
    {
        var panel = new StackPanel();
        panel.Children.Add(MakeEngineSection(vm));
        panel.Children.Add(MakeOptionsSection(vm));
        panel.Children.Add(MakeOnlineSttSection(vm));
        panel.Children.Add(MakeAdvancedSection(vm));
        return panel;
    }

    /// <summary>
    /// Engine (with web site, download and backend-status buttons), backend, model and language.
    /// </summary>
    private static Border MakeEngineSection(SpeechToTextViewModel vm)
    {
        var comboEngine = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithAccessibleName(Se.Language.General.Engine);
        comboEngine.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboEngine.ItemTemplate = MakeEngineItemTemplate();
        comboEngine.SelectionChanged += vm.OnEngineChanged;

        // Force the combo to rebuild its row visuals after a re-download finishes, so the
        // install-status dot reflects the freshly-written .installed.sha256 sidecar instead
        // of the snapshot taken when each row was first realised.
        vm.RefreshEngineCombo = () => comboEngine.ItemTemplate = MakeEngineItemTemplate();
        var buttonEngineWebsite = EditorChrome.AsToolButton(UiUtil.MakeButton(vm.ShowWebLinkCommand, IconNames.Web))
            .WithAccessibleName($"{Se.Language.General.Engine} - {Se.Language.General.MoreInfo}");
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonEngineWebsite, Se.Language.General.MoreInfo);
        }

        var buttonEngineDownload = EditorChrome.AsToolButton(UiUtil.MakeButton(vm.DownloadSelectedEngineCommand, IconNames.Download))
            .BindIsVisible(vm, nameof(vm.IsEngineDownloadButtonVisible))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithAccessibleName(Se.Language.General.Download);
        buttonEngineDownload.Bind(ToolTip.TipProperty, new Binding(nameof(vm.EngineDownloadHint))
        {
            Source = vm,
            Mode = BindingMode.OneWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });

        var buttonEngineSettings = EditorChrome.AsToolButton(UiUtil.MakeButton(vm.ShowEngineSettingsCommand, IconNames.Settings))
            .BindIsVisible(vm, nameof(vm.IsEngineSettingsButtonVisible))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithAccessibleName(Se.Language.Video.AudioToText.BackendAndUpdateStatus);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonEngineSettings, Se.Language.Video.AudioToText.BackendAndUpdateStatus);
        }

        var engineRow = SpeechToTextChrome.WithTrailing(comboEngine, buttonEngineWebsite, buttonEngineDownload, buttonEngineSettings);

        var labelBackend = Label(Se.Language.General.Backend);
        var comboWhisperCppBackend = UiUtil.MakeComboBox(vm.WhisperCppBackends, vm, nameof(vm.SelectedWhisperCppBackend))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .BindIsVisible(vm, nameof(vm.IsWhisperCppSelected))
            .WithLabeledBy(labelBackend);
        comboWhisperCppBackend.HorizontalAlignment = HorizontalAlignment.Stretch;
        var comboCrispAsrBackend = UiUtil.MakeComboBox(vm.CrispAsrBackends, vm, nameof(vm.SelectedCrispAsrBackend))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .BindIsVisible(vm, nameof(vm.IsCrispAsrSelected))
            .WithLabeledBy(labelBackend);
        comboCrispAsrBackend.HorizontalAlignment = HorizontalAlignment.Stretch;
        var backends = new Panel { Children = { comboWhisperCppBackend, comboCrispAsrBackend } };
        var rowBackend = EditorChrome.MakeLabeledRow(labelBackend, backends, LabelWidth)
            .BindIsVisible(vm, nameof(vm.IsBackendSelectionVisible));

        var labelModel = Label(Se.Language.General.Model);
        var comboModel = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithLabeledBy(labelModel);
        comboModel.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboModel.ItemTemplate = MakeModelItemTemplate();

        var buttonModelDownload = EditorChrome.AsToolButton(UiUtil.MakeButtonBrowse(vm.DownloadModelCommand, accessibleName: Se.Language.General.Download))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .BindIsVisible(vm, nameof(vm.IsModelDownloadVisible));
        var rowModel = EditorChrome.MakeLabeledRow(labelModel, SpeechToTextChrome.WithTrailing(comboModel, buttonModelDownload), LabelWidth)
            .BindIsVisible(vm, nameof(vm.IsModelSelectionVisible));

        var labelLanguage = Label(Se.Language.Video.AudioToText.InputLanguage);
        var comboLanguage = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithLabeledBy(labelLanguage);
        comboLanguage.HorizontalAlignment = HorizontalAlignment.Stretch;
        var rowLanguage = EditorChrome.MakeLabeledRow(labelLanguage, comboLanguage, LabelWidth)
            .BindIsVisible(vm, nameof(vm.IsLanguageSelectionVisible));

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(engineRow);
        rows.Children.Add(rowBackend);
        rows.Children.Add(rowModel);
        rows.Children.Add(rowLanguage);
        return EditorChrome.MakeEditorSection(Se.Language.General.Engine, rows, first: true);
    }

    /// <summary>
    /// Forced aligner, VAD and the on/off options (isolate speech, detect speakers, translate,
    /// post-processing).
    /// </summary>
    private static Border MakeOptionsSection(SpeechToTextViewModel vm)
    {
        var labelForcedAligner = Label(Se.Language.Video.ForcedAligner);
        var comboForcedAligner = UiUtil.MakeComboBox(vm.ForcedAligners, vm, nameof(vm.SelectedForcedAligner))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithLabeledBy(labelForcedAligner);
        comboForcedAligner.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboForcedAligner.ItemTemplate = MakeForcedAlignerItemTemplate();
        var buttonForcedAlignerDownload = EditorChrome.AsToolButton(UiUtil.MakeButtonBrowse(vm.DownloadForcedAlignerCommand, accessibleName: Se.Language.General.Download))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));
        var rowForcedAligner = EditorChrome.MakeLabeledRow(labelForcedAligner, SpeechToTextChrome.WithTrailing(comboForcedAligner, buttonForcedAlignerDownload), LabelWidth)
            .BindIsVisible(vm, nameof(vm.IsForcedAlignerVisible));

        var labelVad = Label(Se.Language.Video.AudioToText.Vad);
        var comboVad = UiUtil.MakeComboBox(vm.VadOptions, vm, nameof(vm.SelectedVadOption))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithLabeledBy(labelVad);
        comboVad.HorizontalAlignment = HorizontalAlignment.Stretch;
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(labelVad, Se.Language.Video.AudioToText.VadHint);
            ToolTip.SetTip(comboVad, Se.Language.Video.AudioToText.VadHint);
        }

        var rowVad = EditorChrome.MakeLabeledRow(labelVad, comboVad, LabelWidth)
            .BindIsVisible(vm, nameof(vm.IsVadVisible));

        Grid MakeSwitchRow(string property, string text, string? visibleProperty, string? hint, params Control[] trailing)
        {
            var toggle = EditorChrome.EnabledBy(EditorChrome.MakeSwitch(property, text, vm), vm, nameof(vm.IsTranscribeEnabled));
            var label = Label(text);
            if (hint != null && Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(label, hint);
                ToolTip.SetTip(toggle, hint);
            }

            var row = EditorChrome.MakeSwitchRow(toggle, label, trailing);
            if (visibleProperty != null)
            {
                row.BindIsVisible(vm, visibleProperty);
            }

            return row;
        }

        var buttonPostProcessing = EditorChrome.AsToolButton(UiUtil.MakeButton(vm.ShowPostProcessingSettingsCommand, IconNames.Settings), 26)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithAccessibleName(Se.Language.General.PostProcessing);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonPostProcessing, Se.Language.General.PostProcessing);
        }

        var switches = new StackPanel { Spacing = 6, Margin = new Thickness(0, 2, 0, 0) };
        switches.Children.Add(MakeSwitchRow(nameof(vm.DoIsolateSpeech), Se.Language.Video.AudioToText.IsolateSpeech,
            nameof(vm.IsCrispAsrSelected), Se.Language.Video.AudioToText.IsolateSpeechHint));
        switches.Children.Add(MakeSwitchRow(nameof(vm.DoDetectSpeakers), Se.Language.Video.AudioToText.DetectSpeakers,
            nameof(vm.IsDetectSpeakersVisible), Se.Language.Video.AudioToText.DetectSpeakersHint));
        switches.Children.Add(MakeSwitchRow(nameof(vm.DoTranslateToEnglish), Se.Language.Video.AudioToText.TranslateToEnglish,
            nameof(vm.IsTranslateVisible), null));
        switches.Children.Add(MakeSwitchRow(nameof(vm.DoPostProcessing), Se.Language.General.PostProcessing,
            null, null, buttonPostProcessing));

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(rowForcedAligner);
        rows.Children.Add(rowVad);
        rows.Children.Add(switches);
        return EditorChrome.MakeEditorSection(Se.Language.General.Options, rows);
    }

    /// <summary>
    /// The online engines' settings (OpenAI-compatible, OpenRouter, DashScope, Google Cloud) -
    /// only the selected engine's group is visible, and the section only for an online engine.
    /// </summary>
    private static Border MakeOnlineSttSection(SpeechToTextViewModel vm)
    {
        var groups = new StackPanel();
        groups.Children.Add(MakeOnlineGroup(vm, nameof(vm.IsOpenAiCompatibleSttVisible), MakeOpenAiCompatibleSttRows(vm)));
        groups.Children.Add(MakeOnlineGroup(vm, nameof(vm.IsOpenRouterSttVisible), MakeOpenRouterSttRows(vm)));
        groups.Children.Add(MakeOnlineGroup(vm, nameof(vm.IsDashScopeSttVisible), MakeDashScopeSttRows(vm)));
        groups.Children.Add(MakeOnlineGroup(vm, nameof(vm.IsGoogleCloudSttVisible), MakeGoogleCloudSttRows(vm)));

        var section = EditorChrome.MakeEditorSection(Se.Language.General.Settings, groups);
        section.Bind(Visual.IsVisibleProperty, new MultiBinding
        {
            Converter = BoolConverters.Or,
            Bindings =
            {
                new Binding(nameof(vm.IsOpenAiCompatibleSttVisible)) { Source = vm },
                new Binding(nameof(vm.IsOpenRouterSttVisible)) { Source = vm },
                new Binding(nameof(vm.IsDashScopeSttVisible)) { Source = vm },
                new Binding(nameof(vm.IsGoogleCloudSttVisible)) { Source = vm },
            },
        });
        return section;
    }

    private static StackPanel MakeOnlineGroup(SpeechToTextViewModel vm, string visibleProperty, Control[] rows)
    {
        var group = new StackPanel { Spacing = 10 };
        foreach (var row in rows)
        {
            group.Children.Add(row);
        }

        return group.BindIsVisible(vm, visibleProperty);
    }

    /// <summary>
    /// An online-engine setting: label above a stretching field. The input is linked to its
    /// visible label so a screen reader announces the label as the control's name instead of a
    /// bare "edit"/"combo box" (#11745).
    /// </summary>
    private static StackPanel OnlineRow(string text, Control control)
    {
        var label = Label(text);
        AutomationProperties.SetLabeledBy(control, label);
        return SpeechToTextChrome.Stacked(label, control);
    }

    private static Grid OnlineSwitch(SpeechToTextViewModel vm, string property, string text, string? hint = null)
    {
        var toggle = EditorChrome.MakeSwitch(property, text, vm);
        var label = Label(text);
        if (hint != null && Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(toggle, hint);
            ToolTip.SetTip(label, hint);
        }

        return EditorChrome.MakeSwitchRow(toggle, label);
    }

    private static TextBox OnlineText(SpeechToTextViewModel vm, string property)
    {
        return EditorChrome.AsTextField(new TextBox
        {
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(property) { Mode = BindingMode.TwoWay }
        });
    }

    private static NumericUpDown OnlineNumber(SpeechToTextViewModel vm, string property, double minimum, double maximum, string format, decimal increment = 1)
    {
        var field = EditorChrome.MakeField(property, minimum, maximum, 120, null, format);
        field.DataContext = vm;
        field.Increment = increment;
        field.HorizontalAlignment = HorizontalAlignment.Left;
        return field;
    }

    private static StackPanel OnlineApiKey(SpeechToTextViewModel vm, string property)
    {
        return SpeechToTextChrome.CompactApiKey(UiUtil.MakeApiKeyTextBox(SettingsWidth - 60, vm, property), SettingsWidth - 60);
    }

    private static Control[] MakeOpenAiCompatibleSttRows(SpeechToTextViewModel vm)
    {
        var textExtraHeaders = EditorChrome.AsTextField(new TextBox
        {
            DataContext = vm,
            Height = 60,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalContentAlignment = VerticalAlignment.Top,
            [!TextBox.TextProperty] = new Binding(nameof(vm.OpenAiCompatibleSttExtraHeaders)) { Mode = BindingMode.TwoWay }
        });

        var comboAudioFormat = UiUtil.MakeComboBox(vm.OpenAiCompatibleSttAudioFormats, vm, nameof(vm.OpenAiCompatibleSttAudioFormat))
            .WithMinWidth(160);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(comboAudioFormat, Se.Language.General.OpenAiCompatibleSttAudioFormatHint);
        }

        var numbers = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
        };
        numbers.Add(OnlineRow(Se.Language.General.OpenAiCompatibleSttTimeout, OnlineNumber(vm, nameof(vm.OpenAiCompatibleSttTimeoutSeconds), 10, 3600, "F0")), 0, 0);
        numbers.Add(OnlineRow(Se.Language.General.Temperature, OnlineNumber(vm, nameof(vm.OpenAiCompatibleSttTemperature), 0, 1, "F2", 0.1m)), 0, 1);

        return new Control[]
        {
            OnlineRow(Se.Language.General.OpenAiCompatibleSttEndpoint, OnlineText(vm, nameof(vm.OpenAiCompatibleSttUrl))),
            OnlineRow(Se.Language.General.OpenAiCompatibleSttApiKey, OnlineApiKey(vm, nameof(vm.OpenAiCompatibleSttApiKey))),
            OnlineRow(Se.Language.General.Model, OnlineText(vm, nameof(vm.OpenAiCompatibleSttModel))),
            OnlineRow(Se.Language.General.OpenAiCompatibleSttLanguage, OnlineText(vm, nameof(vm.OpenAiCompatibleSttLanguage))),
            numbers,
            OnlineRow(Se.Language.General.Prompt, OnlineText(vm, nameof(vm.OpenAiCompatibleSttPrompt))),
            OnlineRow(Se.Language.General.OpenAiCompatibleSttExtraHeaders, textExtraHeaders),
            OnlineRow(Se.Language.General.OpenAiCompatibleSttAudioFormat, comboAudioFormat),
            OnlineSwitch(vm, nameof(vm.OpenAiCompatibleSttStream), Se.Language.General.OpenAiCompatibleSttStream, Se.Language.General.OpenAiCompatibleSttStreamHint),
        };
    }

    private static Control[] MakeOpenRouterSttRows(SpeechToTextViewModel vm)
    {
        var numbers = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
        };
        numbers.Add(OnlineRow(Se.Language.General.OpenAiCompatibleSttTimeout, OnlineNumber(vm, nameof(vm.OpenRouterSttTimeoutSeconds), 10, 3600, "F0")), 0, 0);
        numbers.Add(OnlineRow(Se.Language.General.Temperature, OnlineNumber(vm, nameof(vm.OpenRouterSttTemperature), 0, 1, "F2", 0.1m)), 0, 1);

        return new Control[]
        {
            OnlineRow(Se.Language.General.OpenAiCompatibleSttApiKey, OnlineApiKey(vm, nameof(vm.OpenRouterSttApiKey))),
            OnlineRow(Se.Language.General.Model, OnlineText(vm, nameof(vm.OpenRouterSttModel))),
            OnlineRow(Se.Language.General.OpenAiCompatibleSttLanguage, OnlineText(vm, nameof(vm.OpenRouterSttLanguage))),
            numbers,
            OnlineRow(Se.Language.General.Prompt, OnlineText(vm, nameof(vm.OpenRouterSttPrompt))),
        };
    }

    private static Control[] MakeDashScopeSttRows(SpeechToTextViewModel vm)
    {
        var comboRegion = UiUtil.MakeComboBox(vm.DashScopeSttRegions, vm, nameof(vm.DashScopeSttRegion))
            .WithMinWidth(160);

        return new Control[]
        {
            OnlineRow(Se.Language.General.OpenAiCompatibleSttApiKey, OnlineApiKey(vm, nameof(vm.DashScopeSttApiKey))),
            OnlineRow(Se.Language.General.Model, OnlineText(vm, nameof(vm.DashScopeSttModel))),
            OnlineRow(Se.Language.General.DashScopeSttRegion, comboRegion),
            OnlineRow(Se.Language.General.OpenAiCompatibleSttLanguage, OnlineText(vm, nameof(vm.DashScopeSttLanguage))),
            OnlineRow(Se.Language.General.OpenAiCompatibleSttTimeout, OnlineNumber(vm, nameof(vm.DashScopeSttTimeoutSeconds), 30, 21600, "F0")),
            OnlineSwitch(vm, nameof(vm.DashScopeSttEnableWords), Se.Language.General.DashScopeSttEnableWords),
        };
    }

    private static Control[] MakeGoogleCloudSttRows(SpeechToTextViewModel vm)
    {
        var buttonBrowse = EditorChrome.AsToolButton(UiUtil.MakeButtonBrowse(vm.BrowseGoogleCloudSttKeyFileCommand, accessibleName: Se.Language.General.KeyFile));
        var textKeyFile = OnlineText(vm, nameof(vm.GoogleCloudSttKeyFile));
        var keyFile = SpeechToTextChrome.WithTrailing(textKeyFile, buttonBrowse);
        var keyFileRow = SpeechToTextChrome.Stacked(Label(Se.Language.General.KeyFile), keyFile);
        AutomationProperties.SetLabeledBy(textKeyFile, keyFileRow.Children[0]);

        return new Control[]
        {
            keyFileRow,
            OnlineRow(Se.Language.General.GoogleCloudSttProjectId, OnlineText(vm, nameof(vm.GoogleCloudSttProjectId))),
            OnlineRow(Se.Language.General.Region, OnlineText(vm, nameof(vm.GoogleCloudSttRegion))),
            OnlineRow(Se.Language.General.Model, OnlineText(vm, nameof(vm.GoogleCloudSttModel))),
            OnlineRow(Se.Language.General.OpenAiCompatibleSttLanguage, OnlineText(vm, nameof(vm.GoogleCloudSttLanguage))),
            OnlineRow(Se.Language.General.GoogleCloudSttBucketName, OnlineText(vm, nameof(vm.GoogleCloudSttBucketName))),
            OnlineSwitch(vm, nameof(vm.GoogleCloudSttDynamicBatching), Se.Language.General.GoogleCloudSttDynamicBatching),
            OnlineRow(Se.Language.General.OpenAiCompatibleSttTimeout, OnlineNumber(vm, nameof(vm.GoogleCloudSttTimeoutSeconds), 30, 21600, "F0")),
        };
    }

    /// <summary>
    /// The engine's command line parameters, with the button that opens the advanced dialog.
    /// </summary>
    private static Border MakeAdvancedSection(SpeechToTextViewModel vm)
    {
        var buttonAdvancedSettings = EditorChrome.AsToolButton(UiUtil.MakeButton(vm.ShowAdvancedSettingsCommand, IconNames.Settings), 26)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithAccessibleName(Se.Language.General.AdvancedSettings);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonAdvancedSettings, Se.Language.General.AdvancedSettings);
        }

        var textBoxAdvancedSettings = new TextBox
        {
            IsReadOnly = true,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            MinHeight = 0,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        }.WithAccessibleName(Se.Language.General.AdvancedSettings);
        textBoxAdvancedSettings.Bind(TextBox.TextProperty, new Binding
        {
            Path = nameof(vm.Parameters),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        return EditorChrome.MakeEditorSection(Se.Language.General.AdvancedSettings, textBoxAdvancedSettings, buttonAdvancedSettings)
            .BindIsVisible(vm, nameof(vm.IsAdvancedSettingsVisible));
    }

    /// <summary>
    /// The console log - and in batch mode the file list under it.
    /// </summary>
    private static Grid MakeLogPanel(SpeechToTextViewModel vm)
    {
        var buttonCopyConsoleLog = EditorChrome.AsToolButton(UiUtil.MakeButton(
            vm.CopyConsoleLogCommand,
            IconNames.Copy,
            Se.Language.General.CopyTextToClipboard), 26);
        vm.CopyConsoleLogButton = buttonCopyConsoleLog;

        var body = new Panel();
        body.Children.Add(MakeConsoleLogOnlyView(vm));
        body.Children.Add(MakeConsoleLogAndBatchView(vm));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
            Margin = new Thickness(14, 12, 14, 14),
        };
        grid.Add(SpeechToTextChrome.ListHeader(Se.Language.General.ConsoleLog, buttonCopyConsoleLog), 0, 0);
        grid.Add(body, 1, 0);
        return grid;
    }

    private static TextBox MakeConsoleLogTextBox(SpeechToTextViewModel vm)
    {
        var textBoxConsoleLog = new TextBox()
        {
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsReadOnly = true,
            FontSize = EditorChrome.FieldFontSize,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(10, 8),
        };
        AutomationProperties.SetName(textBoxConsoleLog, Se.Language.General.ConsoleLog);
        textBoxConsoleLog.Bind(TextBox.TextProperty, new Binding
        {
            Path = nameof(vm.ConsoleLog),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        return textBoxConsoleLog;
    }

    private static Grid MakeConsoleLogAndBatchView(SpeechToTextViewModel vm)
    {
        var textBoxConsoleLog = MakeConsoleLogTextBox(vm);
        vm.TextBoxConsoleLogBatch = textBoxConsoleLog;

        // No header sorting: the batch queue is processed top-to-bottom (the run aliases
        // BatchItems and walks it in list order), so the list order is meaningful.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.BatchItems;
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.FileName,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SpeechToTextJobItem.InputVideoFileNameShort)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Size,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SpeechToTextJobItem.SizeDisplay)),
            Width = new GridLength(90), // was content-sized (Auto) on the DataGrid
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Status,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SpeechToTextJobItem.Status)),
            Width = new GridLength(110), // was content-sized (Auto) on the DataGrid
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedBatchItem)) { Source = vm });
        AutomationProperties.SetName(dataGrid, Se.Language.General.BatchMode);
        vm.BatchGrid = dataGrid;

        Button Action(string icon, string name, System.Windows.Input.ICommand command)
        {
            return EditorChrome.CreateToolButton(icon, name, command, 26)
                .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));
        }

        var buttonAdd = Action("fa-solid fa-plus", Se.Language.General.AddDotDotDot, vm.AddCommand);
        var buttonRemove = Action("fa-solid fa-minus", Se.Language.General.Remove, vm.RemoveCommand);
        var buttonClear = Action("fa-solid fa-trash", Se.Language.General.Clear, vm.ClearCommand);

        var labelAddLanguageCode = Label(Se.Language.Video.AudioToText.AddLanguageCodeToFileName);
        var switchAddLanguageCode = EditorChrome.EnabledBy(EditorChrome.MakeSwitch(nameof(vm.AddLanguageCodeToFileName), Se.Language.Video.AudioToText.AddLanguageCodeToFileName, vm), vm, nameof(vm.IsTranscribeEnabled));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(switchAddLanguageCode, Se.Language.Video.AudioToText.AddLanguageCodeToFileNameHint);
            ToolTip.SetTip(labelAddLanguageCode, Se.Language.Video.AudioToText.AddLanguageCodeToFileNameHint);
        }

        var rowAddLanguageCode = EditorChrome.MakeSwitchRow(switchAddLanguageCode, labelAddLanguageCode);
        rowAddLanguageCode.Margin = new Thickness(0, 8, 0, 0);

        // hack to make drag and drop work on the grid - also on empty rows
        var dropHost = new Border
        {
            Background = Brushes.Transparent,
            Child = dataGrid,
        };
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, vm.FileGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, vm.FileGridOnDrop, RoutingStrategies.Bubble);

        var batchHeader = SpeechToTextChrome.ListHeader(Se.Language.General.BatchMode, buttonAdd, buttonRemove, buttonClear);
        batchHeader.Margin = new Thickness(0, 14, 0, 8);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Console log
                new RowDefinition { Height = GridLength.Auto }, // Batch header + actions
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Batch view
                new RowDefinition { Height = GridLength.Auto }, // Add language code
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(EditorChrome.MakeListFrame(textBoxConsoleLog), 0, 0);
        grid.Add(batchHeader, 1, 0);
        grid.Add(EditorChrome.MakeListFrame(dropHost), 2, 0);
        grid.Add(rowAddLanguageCode, 3, 0);

        grid.WithBindVisible(vm, nameof(vm.IsBatchMode));

        return grid;
    }

    private static Border MakeConsoleLogOnlyView(SpeechToTextViewModel vm)
    {
        var textBoxConsoleLog = MakeConsoleLogTextBox(vm);
        vm.TextBoxConsoleLogSingle = textBoxConsoleLog;

        var frame = EditorChrome.MakeListFrame(textBoxConsoleLog);
        frame.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsBatchMode)) { Source = vm, Converter = InverseBooleanConverter.Instance });
        return frame;
    }

    private Border MakeProgressView(SpeechToTextViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.WithPlatformProgress(this, nameof(vm.IsTranscribeEnabled), InverseBooleanConverter.Instance);
        AutomationProperties.SetName(progressBar, Se.Language.Video.AudioToText.Transcribe);
        progressBar.Margin = new Thickness(0);
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

        var progressText = EditorChrome.MakeBoundHint(nameof(vm.ProgressText), 12);
        var elapsedTimeText = EditorChrome.MakeBoundHint(nameof(vm.ElapsedText), 12);
        var estimatedTimeText = EditorChrome.MakeBoundHint(nameof(vm.EstimatedText), 12);

        var times = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16, VerticalAlignment = VerticalAlignment.Center };
        times.Children.Add(elapsedTimeText);
        times.Children.Add(estimatedTimeText);

        var grid = new Grid
        {
            ColumnSpacing = 12,
            RowSpacing = 4,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
        };
        grid.Add(progressBar, 0, 0);
        grid.Add(times, 0, 1);
        grid.Add(progressText, 1, 0, 1, 2);

        // Shown while the view model shows its progress (ProgressOpacity), like the old fading
        // progress bar and texts.
        return new Border
        {
            Name = ProgressPanelName,
            Child = grid,
            Background = EditorChrome.StatusBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 8),
            [!Border.IsVisibleProperty] = new Binding(nameof(vm.ProgressOpacity))
            {
                Source = vm,
                Converter = new FuncValueConverter<double, bool>(opacity => opacity > 0),
            },
        };
    }

    private static Border MakeButtonBar(SpeechToTextViewModel vm)
    {
        // Single / batch switch. Without a video only batch mode is possible, and the view model
        // hides both (IsBatchModeVisible and IsSingleModeVisible false).
        Button Mode(string text, System.Windows.Input.ICommand command, bool batch)
        {
            var button = new Button
            {
                Content = new TextBlock { Text = text, FontSize = EditorChrome.FieldFontSize, Margin = new Thickness(12, 0) },
                Height = 28,
                Command = command,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            button.Classes.Add("tool");
            AutomationProperties.SetName(button, text);

            void Update()
            {
                button.Classes.Set("active", vm.IsBatchMode == batch);
                button.IsEnabled = vm.IsTranscribeEnabled;
            }

            Update();
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(vm.IsBatchMode) or nameof(vm.IsTranscribeEnabled))
                {
                    Update();
                }
            };
            return button;
        }

        var modeButtons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        modeButtons.Children.Add(Mode(Se.Language.General.SingleMode, vm.SingleModeCommand, false));
        modeButtons.Children.Add(Mode(Se.Language.General.BatchMode, vm.BatchModeCommand, true));
        var modes = new Border
        {
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Child = modeButtons,
        };
        modes.Bind(Visual.IsVisibleProperty, new MultiBinding
        {
            Converter = BoolConverters.Or,
            Bindings =
            {
                new Binding(nameof(vm.IsBatchModeVisible)) { Source = vm },
                new Binding(nameof(vm.IsSingleModeVisible)) { Source = vm },
            },
        });

        var transcribeButton = EditorChrome.AsPrimary(UiUtil.MakeButton(Se.Language.Video.AudioToText.Transcribe, vm.TranscribeCommand))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));
        var bar = EditorChrome.MakeFlushButtonBar(modes, UiUtil.MakeButtonCancel(vm.CancelCommand), transcribeButton);
        bar.Name = ButtonBarName;
        return bar;
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
