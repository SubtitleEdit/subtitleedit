using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using System.Collections.ObjectModel;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public class TextToSpeechWindow : Window
{
    private readonly TextToSpeechViewModel _vm;
    private ComboBox? _comboBoxEngines;
    private ComboBox? _comboBoxModels;

    public TextToSpeechWindow(TextToSpeechViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        vm.RefreshDownloadDots = RefreshDownloadDots;
        DataContext = vm;

        var labelEngine = new Label
        {
            Content = Se.Language.Video.TextToSpeech.TextToSpeechEngine,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        var labelSettings = new Label
        {
            Content = Se.Language.General.Settings,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        var engineLayout = MakeEngineControls(vm);

        var settingsLayout = MakeSettingsControls(vm);

        // Wrap the progress bar layout in WidthIgnoringPanel so its measured width never feeds
        // back up into the outer grid's column sizing. See the panel's class comment for why.
        var progressBarLayout = new WidthIgnoringPanel { Children = { MakeProgressBarControls(vm) } };

        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand).WithBindIsVisible(nameof(vm.IsNotGenerating));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand).WithBindIsVisible(nameof(vm.IsGenerating));
        var buttonCast = UiUtil.MakeButton(string.Empty, vm.ShowCastCommand)
            .WithIconLeftBindText(IconNames.PoliceBadge, nameof(vm.CastButtonText))
            .WithBindIsVisible(nameof(vm.HasCast))
            .WithBindIsEnabled(nameof(vm.IsNotGenerating));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonCast, Se.Language.Video.TextToSpeech.SetupCastHint);
        }
        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.Video.TextToSpeech.GenerateSpeechFromText, vm.GenerateTtsCommand).WithBindIsEnabled(nameof(vm.IsNotGenerating)),
            buttonCast,
            UiUtil.MakeButton(Se.Language.General.ImportDotDotDot, vm.ImportCommand).WithBindIsEnabled(nameof(vm.IsNotGenerating)),
            buttonCancel,
            buttonDone
        ).WithMarginTop(0);

        var grid = new Grid
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelEngine, 0, 0);
        grid.Add(labelSettings, 0, 1);

        grid.Add(engineLayout, 1, 0);
        grid.Add(settingsLayout, 1, 1);

        grid.Add(progressBarLayout, 2, 0, 1, 2);

        grid.Add(buttonPanel, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonDone.Focus(); }; // hack to make OnKeyDown work
    }

    // Install-status dot for the engine combo: green = ready, amber = a newer build is available,
    // grey = downloadable but not on disk. Cloud/online engines (Azure, ElevenLabs, Google,
    // Edge TTS, ...) have nothing to install and get no dot.
    private static DownloadDotStatus GetTtsEngineDotStatus(ITtsEngine engine)
    {
        // IsInstalled is a Task only by interface contract; the local engines below complete it
        // synchronously (Task.FromResult of a File.Exists check), so .Result does not block.
        switch (engine)
        {
            case KokoroTtsCpp:
                return StatusDots.From(engine.IsInstalled(null).Result, KokoroTtsCpp.GetEngineUpdateStatus());
            case Qwen3TtsCpp:
                return StatusDots.From(engine.IsInstalled(null).Result, Qwen3TtsCpp.GetEngineUpdateStatus());
            case Qwen3TtsCrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, Qwen3TtsCrispAsr.GetEngineUpdateStatus());
            case VibeVoiceCrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, VibeVoiceCrispAsr.GetEngineUpdateStatus());
            case IndexTtsCrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, IndexTtsCrispAsr.GetEngineUpdateStatus());
            case CosyVoice3CrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, CosyVoice3CrispAsr.GetEngineUpdateStatus());
            case F5TtsCrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, F5TtsCrispAsr.GetEngineUpdateStatus());
            case VoxCPM2CrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, VoxCPM2CrispAsr.GetEngineUpdateStatus());
            case OmniVoiceTtsCpp:
                return StatusDots.From(engine.IsInstalled(null).Result, OmniVoiceTtsCpp.GetEngineUpdateStatus());
            case ChatterboxTtsCpp:
                return StatusDots.From(engine.IsInstalled(null).Result, ChatterboxTtsCpp.GetEngineUpdateStatus());
            case Piper:
                return StatusDots.From(engine.IsInstalled(null).Result, Piper.GetEngineUpdateStatus());
            default:
                return DownloadDotStatus.None;
        }
    }

    private static FuncDataTemplate<ITtsEngine> BuildEngineItemTemplate()
    {
        return StatusDots.ComboItemTemplate<ITtsEngine>(
            engine => engine.Name,
            _ => null,
            GetTtsEngineDotStatus);
    }

    // Install-status dot for a model in the model combo. Local engines (Qwen3, Chatterbox) get
    // a green/grey dot per model; cloud engines (ElevenLabs, Mistral) have nothing to install.
    private static DownloadDotStatus GetTtsModelDotStatus(ITtsEngine? engine, string modelKey)
    {
        return engine switch
        {
            Qwen3TtsCpp => Qwen3TtsCpp.IsModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            Qwen3TtsCrispAsr => Qwen3TtsCrispAsr.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            VibeVoiceCrispAsr => VibeVoiceCrispAsr.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            IndexTtsCrispAsr => IndexTtsCrispAsr.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            CosyVoice3CrispAsr => CosyVoice3CrispAsr.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            F5TtsCrispAsr => F5TtsCrispAsr.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            VoxCPM2CrispAsr => VoxCPM2CrispAsr.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            ChatterboxTtsCpp => ChatterboxTtsCpp.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            _ => DownloadDotStatus.None,
        };
    }

    private static FuncDataTemplate<string> BuildModelItemTemplate(TextToSpeechViewModel vm)
    {
        return StatusDots.ComboItemTemplate<string>(
            modelKey => modelKey,
            _ => null,
            modelKey => GetTtsModelDotStatus(vm.SelectedEngine, modelKey));
    }

    // Rebuilds the engine and model combo item templates so the install-status dots are
    // re-evaluated. Called after an engine/model may have been (re)downloaded - the dots are
    // one-off snapshots, so a fresh template is the refresh.
    public void RefreshDownloadDots()
    {
        if (_comboBoxEngines != null)
        {
            _comboBoxEngines.ItemTemplate = BuildEngineItemTemplate();
        }
        if (_comboBoxModels != null)
        {
            _comboBoxModels.ItemTemplate = BuildModelItemTemplate(_vm);
        }
    }

    private Border MakeEngineControls(TextToSpeechViewModel vm)
    {
        var labelMinWidth = 100;
        var controlMinWidth = 200;

        var comboBoxEngines = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine)).WithWidth(controlMinWidth);
        comboBoxEngines.ItemTemplate = BuildEngineItemTemplate();
        comboBoxEngines.SelectionChanged += vm.SelectedEngineChanged;
        _comboBoxEngines = comboBoxEngines;

        var panelEngine = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.Engine,
                    MinWidth = labelMinWidth,
                },
                comboBoxEngines,
                UiUtil.MakeButton(vm.ShowEngineSettingsCommand, IconNames.Settings)
                    .WithMarginLeft(5)
                    .WithBindIsVisible(nameof(vm.IsEngineSettingsVisible)),
            }
        };

        var buttonTestVoice = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.TestVoice, vm.TestVoiceCommand).WithBindIsEnabled(nameof(vm.IsVoiceTestEnabled));

        // The Qwen3 VoiceDesign model has no speaker encoder - the combo is locked to "Default".
        var comboBoxVoices = UiUtil.MakeComboBox(vm.Voices, vm, nameof(vm.SelectedVoice)).WithWidth(controlMinWidth);
        comboBoxVoices.Bind(ComboBox.IsEnabledProperty, new Binding(nameof(vm.IsVoiceComboEnabled)) { Mode = BindingMode.OneWay });

        var panelVoice = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.Voice,
                    MinWidth = labelMinWidth,
                },
                comboBoxVoices,
                new Label
                {
                    [!Label.ContentProperty] = new Binding(nameof(vm.VoiceCountInfo)) { Mode = BindingMode.TwoWay }
                },
                buttonTestVoice,
                UiUtil.MakeButton(vm.ShowTestVoiceSettingsCommand, IconNames.Settings),
            }
        };

        var comboBoxModels = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel)).WithWidth(controlMinWidth);
        comboBoxModels.ItemTemplate = BuildModelItemTemplate(vm);
        _comboBoxModels = comboBoxModels;
        var panelModel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.Model,
                    MinWidth = labelMinWidth,
                },
                comboBoxModels,
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.HasModel)) { Mode = BindingMode.OneWay },
        };
        comboBoxModels.SelectionChanged += vm.SelectedModelChanged;

        var panelRegion = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.Region,
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeComboBox(vm.Regions, vm, nameof(vm.SelectedRegion)).WithWidth(controlMinWidth),
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.HasRegion)) { Mode = BindingMode.OneWay },
        };

        var comboBoxLanguages = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage)).WithWidth(controlMinWidth);
        var panelLanguage = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.Language,
                    MinWidth = labelMinWidth,
                },
                comboBoxLanguages,
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.HasLanguageParameter)) { Mode = BindingMode.OneWay },
        };
        comboBoxLanguages.SelectionChanged += vm.SelectedLanguageChanged;

        var panelApiKey = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.ApiKey,
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeTextBox(325, vm, nameof(vm.ApiKey)),
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.HasApiKey)) { Mode = BindingMode.OneWay },
        };

        var panelKeyFile = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.KeyFile,
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeTextBox(325, vm, nameof(vm.KeyFile)).WithMarginRight(4),
                UiUtil.MakeButtonBrowse(vm.BrowseKeyFileCommand),
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.HasKeyFile)) { Mode = BindingMode.OneWay },
        };

        // Voice instruction applied to every segment. Qwen3 TTS takes free text; OmniVoice TTS
        // only accepts fixed voice-design keywords, so it gets a multi-select picker instead.
        // The panel is shown for both (HasInstruction); the two controls toggle within it.
        var textBoxInstruction = new TextBox
        {
            Width = 325,
            Height = 60,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalContentAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
            PlaceholderText = Se.Language.Video.TextToSpeech.VoiceInstructionHint,
            DataContext = vm,
            [!TextBox.IsVisibleProperty] = new Binding(nameof(vm.IsInstructionTextVisible)) { Mode = BindingMode.OneWay },
        };
        textBoxInstruction.Bind(TextBox.TextProperty, new Binding(nameof(vm.Instruction))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });

        var panelInstruction = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.Video.TextToSpeech.VoiceInstruction,
                    MinWidth = labelMinWidth,
                    VerticalAlignment = VerticalAlignment.Top,
                },
                textBoxInstruction,
                MakeInstructionKeywordPicker(vm),
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.HasInstruction)) { Mode = BindingMode.OneWay },
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 15),
        };

        grid.Add(panelEngine, 0, 0);
        grid.Add(panelModel, 1, 0);
        grid.Add(panelVoice, 2, 0);
        grid.Add(panelRegion, 3, 0);
        grid.Add(panelLanguage, 4, 0);
        grid.Add(panelApiKey, 5, 0);
        grid.Add(panelKeyFile, 6, 0);
        grid.Add(panelInstruction, 7, 0);

        // Give the left (Engine/Voice/Model/...) panel a sensible minimum so the window doesn't
        // collapse into a narrow column when the right-side panel happens to be wider than the
        // engine controls' intrinsic size.
        var border = UiUtil.MakeBorderForControl(grid);
        border.MinWidth = 500;
        return border;
    }

    // Single-select picker of OmniVoice TTS voice-design keywords: one combo box per mutually
    // exclusive group (gender, age, pitch, accent) plus a stand-alone "whisper" checkbox. Shown
    // only for OmniVoice; disabled with a note for cloned voices, since omnivoice-tts ignores
    // --instruct when a reference WAV is used.
    private static Control MakeInstructionKeywordPicker(TextToSpeechViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 8,
            RowSpacing = 6,
            [!Grid.IsEnabledProperty] = new Binding(nameof(vm.IsInstructionPickerEnabled)) { Mode = BindingMode.OneWay },
        };

        AddInstructionPickerRow(grid, 0, Se.Language.Video.TextToSpeech.VoiceGender, vm, vm.OmniVoiceGenders, nameof(vm.SelectedOmniVoiceGender));
        AddInstructionPickerRow(grid, 1, Se.Language.Video.TextToSpeech.VoiceAge, vm, vm.OmniVoiceAges, nameof(vm.SelectedOmniVoiceAge));
        AddInstructionPickerRow(grid, 2, Se.Language.Video.TextToSpeech.VoicePitch, vm, vm.OmniVoicePitches, nameof(vm.SelectedOmniVoicePitch));
        AddInstructionPickerRow(grid, 3, Se.Language.Video.TextToSpeech.VoiceAccent, vm, vm.OmniVoiceAccents, nameof(vm.SelectedOmniVoiceAccent));

        var whisper = new CheckBox
        {
            Content = OmniVoiceTtsCpp.InstructionWhisper,
            DataContext = vm,
            Margin = new Thickness(0, 4, 0, 0),
        };
        whisper.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(vm.OmniVoiceWhisper)) { Mode = BindingMode.TwoWay });
        grid.Add(whisper, 4, 0, 1, 2);

        var clonedVoiceNote = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.VoiceInstructionClonedVoiceNote,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.7,
            MaxWidth = 280,
            Margin = new Thickness(0, 6, 0, 0),
            [!TextBlock.IsVisibleProperty] = new Binding(nameof(vm.IsInstructionVoiceHintVisible)) { Mode = BindingMode.OneWay },
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { grid, clonedVoiceNote },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.IsInstructionPickerVisible)) { Mode = BindingMode.OneWay },
        };
    }

    private static void AddInstructionPickerRow(Grid grid, int row, string label, TextToSpeechViewModel vm,
        ObservableCollection<string> items, string selectedPropertyPath)
    {
        grid.Add(new Label { Content = label, MinWidth = 60, VerticalAlignment = VerticalAlignment.Center }, row, 0);
        grid.Add(UiUtil.MakeComboBox(items, vm, selectedPropertyPath).WithWidth(200), row, 1);
    }

    private static Border MakeSettingsControls(TextToSpeechViewModel vm)
    {
        var checkBoxReviewAudioClips = new CheckBox
        {
            Content = Se.Language.Video.TextToSpeech.ReviewAudioSegments,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 10, 0, 10),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.DoReviewAudioClips)) { Mode = BindingMode.TwoWay }
        };

        var checkBoxAddAudioToVideoFile = new CheckBox
        {
            Content = Se.Language.Video.TextToSpeech.AddAudioToVideoFile,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 0, 10),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.DoGenerateVideoFile)) { Mode = BindingMode.TwoWay }
        };

        var panelAddAudioToVideoFile = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                checkBoxAddAudioToVideoFile,
                UiUtil.MakeButton(vm.ShowEncodingSettingsCommand, IconNames.Settings)
                      .WithMarginLeft(5).WithMarginTop(0).WithTopAlignment(),
            }
        };

        var buttonAdvanced = UiUtil.MakeButton(Se.Language.General.AdvancedDotDotDot, vm.ShowAdvancedSettingsCommand)
            .WithMarginTop(5);
        // MakeButton defaults to Center; the right-side settings panel reads more cleanly with
        // the Advanced button left-aligned under the checkboxes above it.
        buttonAdvanced.HorizontalAlignment = HorizontalAlignment.Left;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(checkBoxReviewAudioClips, 0, 0);
        grid.Add(panelAddAudioToVideoFile, 1, 0);
        grid.Add(buttonAdvanced, 2, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Grid MakeProgressBarControls(TextToSpeechViewModel vm)
    {
        // Two rows so the status label and the progress bar stack vertically instead of
        // overlapping in the same cell. Two columns so the progress text can explicitly span
        // both — same shape as the outer window grid, so the text gets the full available
        // width when long messages render (e.g. "Generating speech: segment 999 of 1000").
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 5, 0, 7),
            [!Grid.OpacityProperty] = new Binding(nameof(vm.ProgressOpacity)) { Mode = BindingMode.OneWay },
        };

        // TextBlock (not Label) so we can use TextTrimming for overflow rendering. The actual
        // "don't grow the window" guarantee comes from the WidthIgnoringPanel wrapper around
        // this whole grid below — that panel reports 0 desired width up the layout chain so the
        // outer grid's Star columns never see the progress text's natural width.
        var label = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Left,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 6, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ProgressText)) { Mode = BindingMode.OneWay },
        };

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.HorizontalAlignment = HorizontalAlignment.Stretch;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        // Both rows explicitly span the two columns so the bar and status text use the full
        // window width (matching the engine + settings panels above), and so the text doesn't
        // need to grow horizontally when messages get longer.
        grid.Add(progressBar, 0, 0, 1, 2);
        grid.Add(label, 1, 0, 1, 2);

        return grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded(e);
    }
}

// Wraps a child whose width should not influence the parent's measure. We use this for the
// progress text + bar at the bottom of the TTS window.
//
// Without it, the progress TextBlock's natural text width feeds up through the inner grid into
// the outer 2-column grid, where colSpan=2 distributes that desired width across both Star
// columns. Each column then grows to max(natural, distributedShare), so when the progress text
// gets longer mid-generation (e.g. "Adding audio to video file..."), the column with the
// smaller natural content (the settings panel) is pushed wider — making the whole window jump.
// TextTrimming on the TextBlock doesn't help because trimming runs at render time, not measure.
//
// MeasureOverride still measures the child (so it can lay itself out correctly in arrange and
// know its height), but reports Size(0, childHeight) as our own desired. ArrangeOverride gives
// the child the final size we were allocated — which the outer grid computed purely from the
// engine/settings panels and the button bar, not from our child's text content. Net effect: the
// outer grid's column widths are stable across progress-text changes, and the bar/text simply
// trim within whatever width the engine/settings rows decided.
internal sealed class WidthIgnoringPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        var height = 0d;
        foreach (var child in Children)
        {
            child.Measure(availableSize);
            if (child.DesiredSize.Height > height)
            {
                height = child.DesiredSize.Height;
            }
        }
        return new Size(0, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var child in Children)
        {
            child.Arrange(new Rect(finalSize));
        }
        return finalSize;
    }
}
