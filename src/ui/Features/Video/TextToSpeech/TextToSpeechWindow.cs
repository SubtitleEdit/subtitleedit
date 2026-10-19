using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections.ObjectModel;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Platform.Progress;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// Text to speech, in the editor look (see <see cref="EditorChrome"/>): import/cast actions on
/// top, engine and voice on the left, output options on the right, progress and Generate at
/// the bottom.
/// </summary>
public class TextToSpeechWindow : Window
{
    private readonly TextToSpeechViewModel _vm;
    private ComboBox? _comboBoxEngines;
    private ComboBox? _comboBoxModels;

    private const double LabelWidth = 92;
    private const double ComboWidth = 250;

    public TextToSpeechWindow(TextToSpeechViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Video.TextToSpeech.Title);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TextFieldsBare | EditorWindowStyles.MuteDisabledPrimary);

        _vm = vm;
        vm.Window = this;
        vm.RefreshDownloadDots = RefreshDownloadDots;
        DataContext = vm;

        // Frozen while generating: the pipeline reads SelectedLanguage/Region/Model live per
        // segment, so changing engine/model/language mid-run made the remaining segments use
        // the new values (or silently ended the run when the engine switch left SelectedVoice
        // transiently null). Only the engine panel - the settings panel's review/video
        // checkboxes are read once after generation, and toggling them mid-run is useful
        // (e.g. remembering to enable review during a long run).
        var engineLayout = MakeEngineControls(vm);
        engineLayout.Bind(InputElement.IsEnabledProperty, new Binding("!" + nameof(vm.IsGenerating)));

        var settingsLayout = MakeSettingsControls(vm);
        settingsLayout.BorderBrush = EditorChrome.LineBrush;
        settingsLayout.BorderThickness = new Thickness(1, 0, 0, 0);

        var settings = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), // engine and voice
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)), // output
            },
        };
        settings.Add(engineLayout, 0, 0);
        settings.Add(settingsLayout, 0, 1);

        // The settings scroll on their own, so progress and the buttons stay reachable when the
        // window is taller than the screen (e.g. an engine with instruction + API key rows).
        var settingsPanel = new Border
        {
            Background = EditorChrome.PanelBrush,
            Child = new ScrollViewer
            {
                Content = settings,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            },
        };

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto), // import / cast
                new RowDefinition(new GridLength(1, GridUnitType.Star)), // settings
                new RowDefinition(GridLength.Auto), // progress, while generating
                new RowDefinition(GridLength.Auto), // context + buttons
            },
        };
        root.Add(MakeToolbar(vm), 0, 0);
        root.Add(settingsPanel, 1, 0);
        root.Add(MakeProgressView(vm), 2, 0);
        root.Add(MakeButtonBar(vm), 3, 0);

        Content = root;

        UiUtil.FocusOnFirstActivation(this, () => { _comboBoxEngines?.Focus(); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
    }

    /// <summary>
    /// Import a session, and set up the cast when the subtitle has several actors.
    /// </summary>
    private static Border MakeToolbar(TextToSpeechViewModel vm)
    {
        var buttonImport = EditorChrome.MakeActionButton("fa-solid fa-file-import", Se.Language.General.ImportDotDotDot, vm.ImportCommand)
            .WithBindIsEnabled("!" + nameof(vm.IsGenerating));
        var buttonCast = EditorChrome.MakeActionButton("fa-solid fa-users", null, vm.ShowCastCommand, nameof(vm.CastButtonText))
            .WithBindIsVisible(nameof(vm.HasCast))
            .WithBindIsEnabled("!" + nameof(vm.IsGenerating));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonCast, Se.Language.Video.TextToSpeech.SetupCastHint);
        }

        var panel = TtsMainEditorLook.Horizontal(buttonImport, buttonCast);
        panel.Spacing = 2;
        return new Border
        {
            Child = panel,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 6),
        };
    }

    /// <summary>
    /// Context on the left - what is about to be spoken and whether a video is loaded - and
    /// Cancel, OK and Generate on the right.
    /// </summary>
    private static Border MakeButtonBar(TextToSpeechViewModel vm)
    {
        // Context line: what is about to be spoken + whether a video is loaded. Before this the
        // window only revealed the line count via the progress text after Generate was clicked.
        var contextIcon = new Optris.Icons.Avalonia.Icon
        {
            Value = "fa-solid fa-closed-captioning",
            FontSize = 12,
            Foreground = EditorChrome.FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var contextText = new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 320,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.LinesInfo)) { Mode = BindingMode.OneWay },
        };
        // A movie icon stands in for a "Video:" label inside the chip.
        var videoChip = TtsMainEditorLook.MakeChip(TtsMainEditorLook.Horizontal(
            new Optris.Icons.Avalonia.Icon
            {
                Value = "fa-solid fa-film",
                FontSize = 11,
                Foreground = EditorChrome.FaintTextBrush,
                VerticalAlignment = VerticalAlignment.Center,
            },
            new TextBlock
            {
                FontSize = 11.5,
                Foreground = EditorChrome.DimTextBrush,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 240,
                [!TextBlock.TextProperty] = new Binding(nameof(vm.VideoInfo)) { Mode = BindingMode.OneWay },
            }));
        ((StackPanel)videoChip.Child!).Spacing = 6;
        videoChip.Bind(IsVisibleProperty, new Binding(nameof(vm.HasVideoFile)) { Mode = BindingMode.OneWay });

        var context = TtsMainEditorLook.Horizontal(contextIcon, contextText, videoChip);
        context.Spacing = 8;

        // OK accepts the session (the main window applies merged lines and review text edits);
        // Cancel stops a running generation, or - when idle - closes discarding those changes.
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindIsVisible("!" + nameof(vm.IsGenerating));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        // Generate is the window's primary action: accent-filled, icon, far right - previously
        // all buttons had identical weight and Generate sat first in the row.
        var buttonGenerate = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.GenerateSpeechFromText, vm.GenerateTtsCommand)
            .WithIconLeft(IconNames.Waveform)
            .WithBindIsEnabled("!" + nameof(vm.IsGenerating));
        EditorChrome.AsPrimary(buttonGenerate);

        // The context is as wide as the buttons leave room for (trimmed), it never widens the
        // window - see WidthIgnoringPanel.
        return EditorChrome.MakeButtonBar(new WidthIgnoringPanel { Children = { context } }, buttonCancel, buttonOk, buttonGenerate);
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
            case PocketTtsCrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, PocketTtsCrispAsr.GetEngineUpdateStatus());
            case SupertonicCrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, SupertonicCrispAsr.GetEngineUpdateStatus());
            case DotsTtsCrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, DotsTtsCrispAsr.GetEngineUpdateStatus());
            case Confucius4TtsCrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, Confucius4TtsCrispAsr.GetEngineUpdateStatus());
            case IndexTts25AudioCpp:
                return StatusDots.From(engine.IsInstalled(null).Result, IndexTts25AudioCpp.GetEngineUpdateStatus());
            case HiggsTtsAudioCpp:
                return StatusDots.From(engine.IsInstalled(null).Result, HiggsTtsAudioCpp.GetEngineUpdateStatus());
            case FishTtsAudioCpp:
                return StatusDots.From(engine.IsInstalled(null).Result, FishTtsAudioCpp.GetEngineUpdateStatus());
            case FireRedTts3AudioCpp:
                return StatusDots.From(engine.IsInstalled(null).Result, FireRedTts3AudioCpp.GetEngineUpdateStatus());
            case BreezeTtsAudioCpp:
                return StatusDots.From(engine.IsInstalled(null).Result, BreezeTtsAudioCpp.GetEngineUpdateStatus());
            case KugelAudioAudioCpp:
                return StatusDots.From(engine.IsInstalled(null).Result, KugelAudioAudioCpp.GetEngineUpdateStatus());
            case VoxtralTtsAudioCpp:
                return StatusDots.From(engine.IsInstalled(null).Result, VoxtralTtsAudioCpp.GetEngineUpdateStatus());
            case CosyVoice3CrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, CosyVoice3CrispAsr.GetEngineUpdateStatus());
            case F5TtsCrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, F5TtsCrispAsr.GetEngineUpdateStatus());
            case VoxCPM2CrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, VoxCPM2CrispAsr.GetEngineUpdateStatus());
            case OmniVoiceCrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, OmniVoiceCrispAsr.GetEngineUpdateStatus());
            case MossTtsCrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, MossTtsCrispAsr.GetEngineUpdateStatus());
            case ZonosTtsCrispAsr:
                return StatusDots.From(engine.IsInstalled(null).Result, ZonosTtsCrispAsr.GetEngineUpdateStatus());
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

    private static FuncDataTemplate<string> BuildModelItemTemplate(TextToSpeechViewModel vm)
    {
        // Install-status dot + per-model download size both come from the view model, which owns
        // the single source of truth for model install state and sizes.
        return StatusDots.ComboItemTemplate<string>(
            modelKey => vm.GetModelDisplayText(modelKey),
            _ => null,
            modelKey => vm.GetModelDotStatus(vm.SelectedEngine, modelKey));
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

    private static Grid Row(string? label, Control content, string? labelBinding = null) => EditorChrome.MakeLabeledRow(label, content, LabelWidth, spacing: 6, labelBinding: labelBinding);

    private static ComboBox Combo<T>(ObservableCollection<T> items, TextToSpeechViewModel vm, string property, string name, double width = ComboWidth)
    {
        var comboBox = UiUtil.MakeComboBox(items, vm, property).WithWidth(width);
        comboBox.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(comboBox, name);
        return comboBox;
    }

    private Border MakeEngineControls(TextToSpeechViewModel vm)
    {
        var comboBoxEngines = Combo(vm.Engines, vm, nameof(vm.SelectedEngine), Se.Language.General.Engine);
        comboBoxEngines.ItemTemplate = BuildEngineItemTemplate();
        comboBoxEngines.SelectionChanged += vm.SelectedEngineChanged;
        _comboBoxEngines = comboBoxEngines;

        var buttonEngineSettings = EditorChrome.CreateToolButton("fa-solid fa-gear", TtsMainEditorLook.SettingsName(Se.Language.General.Engine), vm.ShowEngineSettingsCommand, 28)
            .WithBindIsVisible(nameof(vm.IsEngineSettingsVisible));
        var panelEngine = Row(Se.Language.General.Engine, TtsMainEditorLook.Horizontal(comboBoxEngines, buttonEngineSettings));

        // One line about the selected engine (cost/speed/quality or the engine's own blurb) -
        // every engine carries a description that was previously shown nowhere.
        var labelEngineDescription = TtsMainEditorLook.Hint(binding: nameof(vm.EngineDescription));
        labelEngineDescription.TextWrapping = TextWrapping.NoWrap;
        labelEngineDescription.TextTrimming = TextTrimming.CharacterEllipsis;
        labelEngineDescription.MaxWidth = ComboWidth + 40;
        labelEngineDescription.HorizontalAlignment = HorizontalAlignment.Left;
        labelEngineDescription.Margin = new Thickness(0, -3, 0, 0);
        var panelEngineDescription = Row(null, labelEngineDescription);
        panelEngineDescription.Bind(IsVisibleProperty, new Binding(nameof(vm.HasEngineDescription)) { Mode = BindingMode.OneWay });

        var buttonTestVoice = EditorChrome.MakeActionButton("fa-solid fa-play", Se.Language.Video.TextToSpeech.TestVoice, vm.TestVoiceCommand)
            .WithBindIsEnabled(nameof(vm.IsVoiceTestEnabled));
        buttonTestVoice.Height = 28;

        // The Qwen3 VoiceDesign model has no speaker encoder - the combo is locked to "Default".
        var comboBoxVoices = Combo(vm.Voices, vm, nameof(vm.SelectedVoice), Se.Language.General.Voice);
        comboBoxVoices.Bind(ComboBox.IsEnabledProperty, new Binding(nameof(vm.IsVoiceComboEnabled)) { Mode = BindingMode.OneWay });
        // Show Voice.DisplayName rather than the default ToString(): engines whose list mixes
        // several kinds of voice label them there (CosyVoice3's "Preset: ..." / "Clone: ...",
        // #13272). DisplayName falls back to the plain name, so every other engine is unchanged.
        comboBoxVoices.ItemTemplate = new FuncDataTemplate<Voice>((_, _) =>
        {
            // Bound rather than assigned: the template supports recycling, and a recycled
            // container only gets a new DataContext - a statically-set Text would go stale.
            var textBlock = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
            textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(Voice.DisplayName)));
            return textBlock;
        }, true);

        // Right-click: rename a user-imported clone voice (the file the engine lists it from).
        // Presets and built-in speakers have no file, so the item is disabled for those.
        var menuItemRenameVoice = new Avalonia.Controls.MenuItem
        {
            Header = Se.Language.Video.TextToSpeech.RenameVoiceDotDotDot,
            Command = vm.RenameVoiceCommand,
        };
        var menuItemDeleteVoice = new Avalonia.Controls.MenuItem
        {
            Header = Se.Language.Video.TextToSpeech.DeleteVoiceDotDotDot,
            Command = vm.DeleteVoiceCommand,
        };
        var menuItemVoiceManager = new Avalonia.Controls.MenuItem
        {
            Header = Se.Language.Video.TextToSpeech.VoiceManagerDotDotDot,
            Command = vm.ShowVoiceManagerCommand,
        };
        var voiceFlyout = new MenuFlyout { Items = { menuItemRenameVoice, menuItemDeleteVoice, new Separator(), menuItemVoiceManager } };
        voiceFlyout.Opening += (_, _) =>
        {
            var isFileVoice = vm.CanRenameSelectedVoice();
            menuItemRenameVoice.IsEnabled = isFileVoice;
            menuItemDeleteVoice.IsEnabled = isFileVoice;
        };
        comboBoxVoices.ContextFlyout = voiceFlyout;
        UiUtil.AttachMacContextFlyoutHandler(comboBoxVoices);

        var voiceCount = TtsMainEditorLook.MakeChip(new TextBlock
        {
            FontSize = 11.5,
            Foreground = EditorChrome.DimTextBrush,
            [!TextBlock.TextProperty] = new Binding($"{nameof(vm.Voices)}.{nameof(vm.Voices.Count)}")
            {
                Mode = BindingMode.OneWay,
                StringFormat = Se.Language.Video.TextToSpeech.XVoices,
            },
        });
        voiceCount.Margin = new Thickness(2, 0);
        voiceCount.Bind(IsVisibleProperty, new Binding(nameof(vm.IsVoiceCountVisible)) { Mode = BindingMode.OneWay });

        var buttonVoiceManager = EditorChrome.CreateToolButton("fa-solid fa-address-book", Se.Language.Video.TextToSpeech.VoiceManagerDotDotDot, vm.ShowVoiceManagerCommand, 28);
        var panelVoice = Row(Se.Language.General.Voice, TtsMainEditorLook.Horizontal(comboBoxVoices, voiceCount, buttonVoiceManager));

        // Test the voice on its own row: the voice row has the count and the voice manager.
        var buttonTestVoiceSettings = EditorChrome.CreateToolButton("fa-solid fa-gear", TtsMainEditorLook.SettingsName(Se.Language.Video.TextToSpeech.TestVoice), vm.ShowTestVoiceSettingsCommand, 28);
        var panelTestVoice = Row(null, TtsMainEditorLook.Horizontal(buttonTestVoice, buttonTestVoiceSettings));
        panelTestVoice.Margin = new Thickness(-10, -4, 0, 0); // line the play icon up with the combo text

        var comboBoxModels = Combo(vm.Models, vm, nameof(vm.SelectedModel), Se.Language.General.Model);
        comboBoxModels.ItemTemplate = BuildModelItemTemplate(vm);
        _comboBoxModels = comboBoxModels;

        var buttonDownloadModel = EditorChrome.CreateToolButton("fa-solid fa-download", Se.Language.General.Download, vm.DownloadModelCommand, 28)
            .WithBindIsVisible(nameof(vm.IsModelDownloadVisible));

        var panelModel = Row(Se.Language.General.Model, TtsMainEditorLook.Horizontal(comboBoxModels, buttonDownloadModel));
        panelModel.Bind(IsVisibleProperty, new Binding(nameof(vm.HasModel)) { Mode = BindingMode.OneWay });
        comboBoxModels.SelectionChanged += vm.SelectedModelChanged;

        var comboBoxRegions = Combo(vm.Regions, vm, nameof(vm.SelectedRegion), Se.Language.General.Region);
        comboBoxRegions.Bind(AutomationProperties.NameProperty, new Binding(nameof(vm.RegionLabel)) { Mode = BindingMode.OneWay });
        var panelRegion = Row(null, comboBoxRegions, nameof(vm.RegionLabel));
        panelRegion.Bind(IsVisibleProperty, new Binding(nameof(vm.HasRegion)) { Mode = BindingMode.OneWay });

        var comboBoxLanguages = Combo(vm.Languages, vm, nameof(vm.SelectedLanguage), Se.Language.General.Language);
        var panelLanguage = Row(Se.Language.General.Language, comboBoxLanguages);
        panelLanguage.Bind(IsVisibleProperty, new Binding(nameof(vm.HasLanguageParameter)) { Mode = BindingMode.OneWay });
        comboBoxLanguages.SelectionChanged += vm.SelectedLanguageChanged;

        var apiKey = UiUtil.MakeApiKeyTextBox(ComboWidth, vm, nameof(vm.ApiKey));
        foreach (var child in apiKey.Children)
        {
            if (child is TextBox textBox)
            {
                textBox.Classes.Add("text");
            }
        }

        var panelApiKey = Row(Se.Language.General.ApiKey, apiKey);
        panelApiKey.Bind(IsVisibleProperty, new Binding(nameof(vm.HasApiKey)) { Mode = BindingMode.OneWay });

        var textBoxKeyFile = UiUtil.MakeTextBox(ComboWidth, vm, nameof(vm.KeyFile));
        textBoxKeyFile.Classes.Add("text");
        AutomationProperties.SetName(textBoxKeyFile, Se.Language.General.KeyFile);
        var buttonBrowseKeyFile = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.KeyFile, vm.BrowseKeyFileCommand, 28);
        var panelKeyFile = Row(Se.Language.General.KeyFile, TtsMainEditorLook.Horizontal(textBoxKeyFile, buttonBrowseKeyFile));
        panelKeyFile.Bind(IsVisibleProperty, new Binding(nameof(vm.HasKeyFile)) { Mode = BindingMode.OneWay });

        // Voice instruction applied to every segment. Qwen3 TTS takes free text; OmniVoice TTS
        // only accepts fixed voice-design keywords, so it gets a multi-select picker instead.
        // The panel is shown for both (HasInstruction); the two controls toggle within it.
        var textBoxInstruction = new TextBox
        {
            Width = ComboWidth + 32,
            Height = 64,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalContentAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
            PlaceholderText = Se.Language.Video.TextToSpeech.VoiceInstructionHint,
            DataContext = vm,
            [!TextBox.IsVisibleProperty] = new Binding(nameof(vm.IsInstructionTextVisible)) { Mode = BindingMode.OneWay },
        };
        textBoxInstruction.Classes.Add("text");
        textBoxInstruction.Bind(TextBox.TextProperty, new Binding(nameof(vm.Instruction))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });
        AutomationProperties.SetName(textBoxInstruction, Se.Language.Video.TextToSpeech.VoiceInstruction);

        var instruction = new StackPanel { Children = { textBoxInstruction, MakeInstructionKeywordPicker(vm) } };
        var panelInstruction = Row(Se.Language.Video.TextToSpeech.VoiceInstruction, instruction);
        panelInstruction.Children[0].VerticalAlignment = VerticalAlignment.Top;
        panelInstruction.Children[0].Margin = new Thickness(0, 6, 0, 0);
        panelInstruction.Bind(IsVisibleProperty, new Binding(nameof(vm.HasInstruction)) { Mode = BindingMode.OneWay });

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(panelEngine);
        rows.Children.Add(panelEngineDescription);
        rows.Children.Add(panelModel);
        rows.Children.Add(panelVoice);
        rows.Children.Add(panelTestVoice);
        rows.Children.Add(panelRegion);
        rows.Children.Add(panelLanguage);
        rows.Children.Add(panelApiKey);
        rows.Children.Add(panelKeyFile);
        rows.Children.Add(panelInstruction);

        // Give the left (Engine/Voice/Model/...) panel a sensible minimum so the window doesn't
        // collapse into a narrow column when the right-side panel happens to be wider than the
        // engine controls' intrinsic size.
        var section = EditorChrome.MakeEditorSection(Se.Language.Video.TextToSpeech.EngineAndVoice, rows, first: true);
        section.MinWidth = 470;
        return section;
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
            FontSize = 12.5,
            Margin = new Thickness(0, 2, 0, 0),
        };
        whisper.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(vm.OmniVoiceWhisper)) { Mode = BindingMode.TwoWay });
        grid.Add(whisper, 4, 0, 1, 2);

        var clonedVoiceNote = TtsMainEditorLook.Hint(Se.Language.Video.TextToSpeech.VoiceInstructionClonedVoiceNote);
        clonedVoiceNote.MaxWidth = 280;
        clonedVoiceNote.HorizontalAlignment = HorizontalAlignment.Left;
        clonedVoiceNote.Margin = new Thickness(0, 6, 0, 0);
        clonedVoiceNote.Bind(IsVisibleProperty, new Binding(nameof(vm.IsInstructionVoiceHintVisible)) { Mode = BindingMode.OneWay });

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
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0));
        text.MinWidth = 60;
        grid.Add(text, row, 0);
        grid.Add(Combo(items, vm, selectedPropertyPath, label, 200), row, 1);
    }

    private static Border MakeSettingsControls(TextToSpeechViewModel vm)
    {
        var language = Se.Language.Video.TextToSpeech;

        var panelReviewAudioClips = TtsMainEditorLook.SwitchRow(nameof(vm.DoReviewAudioClips), language.ReviewAudioSegments, language.ReviewAudioSegmentsHint);

        var buttonEncodingSettings = EditorChrome.CreateToolButton("fa-solid fa-gear", TtsMainEditorLook.SettingsName(language.AddAudioToVideoFile), vm.ShowEncodingSettingsCommand, 26);
        var panelAddAudioWithHint = TtsMainEditorLook.SwitchRow(nameof(vm.DoGenerateVideoFile), language.AddAudioToVideoFile, language.AddAudioToVideoFileHint, buttonEncodingSettings);

        var backgroundMusic = Se.Language.Video.BackgroundMusic;
        var buttonBackgroundMusicSettings = EditorChrome.CreateToolButton("fa-solid fa-gear", TtsMainEditorLook.SettingsName(backgroundMusic.AddBackgroundMusic), vm.ShowBackgroundMusicSettingsCommand, 26);
        var panelAddBackgroundMusicWithHint = TtsMainEditorLook.SwitchRow(nameof(vm.DoAddBackgroundMusic), backgroundMusic.AddBackgroundMusic, backgroundMusic.AddBackgroundMusicHint, buttonBackgroundMusicSettings);

        // Left-aligned under the switches above it.
        var buttonAdvanced = EditorChrome.MakeActionButton("fa-solid fa-sliders", Se.Language.General.AdvancedDotDotDot, vm.ShowAdvancedSettingsCommand);
        buttonAdvanced.HorizontalAlignment = HorizontalAlignment.Left;
        buttonAdvanced.Margin = new Thickness(-10, 0, 0, 0);

        var rows = new StackPanel { Spacing = 12 };
        rows.Children.Add(panelReviewAudioClips);
        rows.Children.Add(panelAddAudioWithHint);
        rows.Children.Add(panelAddBackgroundMusicWithHint);
        rows.Children.Add(buttonAdvanced);

        var section = EditorChrome.MakeEditorSection(Se.Language.General.Output, rows, first: true);
        section.Width = 330;
        return section;
    }

    private Border MakeProgressView(TextToSpeechViewModel vm)
    {
        // TextBlock (not Label) so we can use TextTrimming for overflow rendering. The actual
        // "don't grow the window" guarantee comes from the WidthIgnoringPanel wrapper around
        // this whole grid below — that panel reports 0 desired width up the layout chain so the
        // outer grid's columns never see the progress text's natural width.
        var labelPercent = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            FontSize = 12,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ProgressPercentText)) { Mode = BindingMode.OneWay },
        };

        var labelEta = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = 12,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ProgressEtaText)) { Mode = BindingMode.OneWay },
        };

        var labelStatus = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Left,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = 12,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ProgressText)) { Mode = BindingMode.OneWay },
        };

        // percent | status | elapsed/remaining on one line, the bar under it
        var statusRow = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        statusRow.Add(labelPercent, 0, 0);
        statusRow.Add(labelStatus, 0, 1);
        statusRow.Add(labelEta, 0, 2);

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.HorizontalAlignment = HorizontalAlignment.Stretch;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.WithPlatformProgress(this, nameof(vm.IsGenerating));

        var grid = new Grid
        {
            RowSpacing = 6,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
        };
        grid.Add(statusRow, 0, 0);
        grid.Add(progressBar, 1, 0);

        // The strip keeps its height while idle (only faded out), so the window does not jump
        // when generating starts; idle it reads as part of the settings panel above.
        return new Border
        {
            Background = EditorChrome.PanelBrush,
            Child = new Border
            {
                Background = EditorChrome.StatusBrush,
                BorderBrush = EditorChrome.LineBrush,
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(16, 8, 16, 10),
                // Wrap the progress layout in WidthIgnoringPanel so its measured width never feeds
                // back up into the window's width. See the panel's class comment for why.
                Child = new WidthIgnoringPanel { Children = { grid } },
                [!Border.OpacityProperty] = new Binding(nameof(vm.ProgressOpacity)) { Mode = BindingMode.OneWay },
            },
        };
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
// progress text + bar at the bottom of the TTS window, and for the context (lines / video) in
// its button bar.
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
