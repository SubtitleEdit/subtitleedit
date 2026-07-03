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
using Avalonia.Styling;
using System.Collections.ObjectModel;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Optris.Icons.Avalonia;
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

        // Context line: what is about to be spoken + whether a video is loaded. Before this the
        // window only revealed the line count via the progress text after Generate was clicked.
        var contextIcon = new ContentControl { VerticalAlignment = VerticalAlignment.Center };
        Attached.SetIcon(contextIcon, IconNames.ClosedCaption);
        var contextText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.75,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.LinesInfo)) { Mode = BindingMode.OneWay },
        };
        var videoChip = new Border
        {
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(9, 1, 9, 2),
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.FromArgb(28, 128, 128, 128)),
            Child = new TextBlock
            {
                FontSize = 11.5,
                Opacity = 0.8,
                [!TextBlock.TextProperty] = new Binding(nameof(vm.VideoInfo)) { Mode = BindingMode.OneWay },
            },
            [!Border.IsVisibleProperty] = new Binding(nameof(vm.HasVideoFile)) { Mode = BindingMode.OneWay },
        };
        var panelContext = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(2, 0, 0, 2),
            Children = { contextIcon, contextText, videoChip },
        };

        var labelEngine = MakeSectionHeader(IconNames.AccountVoice, Se.Language.Video.TextToSpeech.EngineAndVoice);
        var labelSettings = MakeSectionHeader(IconNames.Settings, Se.Language.Video.TextToSpeech.Output);

        // Frozen while generating: the pipeline reads SelectedLanguage/Region/Model live per
        // segment, so changing engine/model/language mid-run made the remaining segments use
        // the new values (or silently ended the run when the engine switch left SelectedVoice
        // transiently null). Only the engine panel - the settings panel's review/video
        // checkboxes are read once after generation, and toggling them mid-run is useful
        // (e.g. remembering to enable review during a long run).
        var engineLayout = MakeEngineControls(vm);
        engineLayout.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsNotGenerating)));

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
        // Generate is the window's primary action: accent-filled, icon, far right - previously
        // all buttons had identical weight and Generate sat first in the row.
        var buttonGenerate = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.GenerateSpeechFromText, vm.GenerateTtsCommand)
            .WithIconLeft(IconNames.Waveform)
            .WithBindIsEnabled(nameof(vm.IsNotGenerating));
        buttonGenerate.Classes.Add("accent");

        var buttonPanel = UiUtil.MakeButtonBar(
            buttonCast,
            UiUtil.MakeButton(Se.Language.General.ImportDotDotDot, vm.ImportCommand).WithBindIsEnabled(nameof(vm.IsNotGenerating)),
            buttonCancel,
            buttonDone,
            buttonGenerate
        ).WithMarginTop(0);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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

        grid.Add(panelContext, 0, 0, 1, 2);

        grid.Add(labelEngine, 1, 0);
        grid.Add(labelSettings, 1, 1);

        grid.Add(engineLayout, 2, 0);
        grid.Add(settingsLayout, 2, 1);

        grid.Add(progressBarLayout, 3, 0, 1, 2);

        grid.Add(buttonPanel, 4, 0, 1, 2);

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

    // The API key is a secret: mask it by default so it never shows in screenshots/screen
    // shares, with an eye button that toggles visibility while editing (#12093).
    private static StackPanel MakeApiKeyTextBox(TextToSpeechViewModel vm)
    {
        var textBox = UiUtil.MakeTextBox(325, vm, nameof(vm.ApiKey));
        textBox.PasswordChar = '●';

        var buttonReveal = UiUtil.MakeButton(null, IconNames.Eye);
        AutomationProperties.SetName(buttonReveal, Se.Language.General.ApiKey);
        buttonReveal.Click += (_, _) =>
        {
            textBox.RevealPassword = !textBox.RevealPassword;
            Attached.SetIcon(buttonReveal, textBox.RevealPassword ? IconNames.EyeOff : IconNames.Eye);
        };

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children = { textBox, buttonReveal },
        };
    }

    // Checkbox (or checkbox row) with a muted one-line explanation below, indented to align
    // with the checkbox text.
    private static StackPanel WithCheckBoxHint(Control checkBoxOrPanel, string hint)
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                checkBoxOrPanel,
                new TextBlock
                {
                    Text = hint,
                    FontSize = 11.5,
                    Opacity = 0.65,
                    Margin = new Thickness(28, 0, 0, 10),
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 260,
                    HorizontalAlignment = HorizontalAlignment.Left,
                },
            },
        };
    }

    private static StackPanel MakeSectionHeader(string iconName, string text)
    {
        var icon = new ContentControl { VerticalAlignment = VerticalAlignment.Center, Opacity = 0.7 };
        Attached.SetIcon(icon, iconName);
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(2, 4, 0, 4),
            Children =
            {
                icon,
                new TextBlock { Text = text, FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center },
            },
        };
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

        // One line about the selected engine (cost/speed/quality or the engine's own blurb) -
        // every engine carries a description that was previously shown nowhere.
        var labelEngineDescription = new TextBlock
        {
            FontSize = 11.5,
            Opacity = 0.65,
            Margin = new Thickness(labelMinWidth + 5, 2, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.EngineDescription)) { Mode = BindingMode.OneWay },
            [!TextBlock.IsVisibleProperty] = new Binding(nameof(vm.HasEngineDescription)) { Mode = BindingMode.OneWay },
        };

        var buttonTestVoice = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.TestVoice, vm.TestVoiceCommand)
            .WithIconLeft(IconNames.Play)
            .WithBindIsEnabled(nameof(vm.IsVoiceTestEnabled));

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
                new Border
                {
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(9, 1, 9, 2),
                    Margin = new Thickness(6, 0, 2, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = new SolidColorBrush(Color.FromArgb(28, 128, 128, 128)),
                    Child = new TextBlock
                    {
                        FontSize = 11.5,
                        Opacity = 0.8,
                        [!TextBlock.TextProperty] = new Binding(nameof(vm.VoiceCountInfo)) { Mode = BindingMode.OneWay },
                    },
                    [!Border.IsVisibleProperty] = new Binding(nameof(vm.IsVoiceCountVisible)) { Mode = BindingMode.OneWay },
                },
                buttonTestVoice,
                UiUtil.MakeButton(vm.ShowTestVoiceSettingsCommand, IconNames.Settings),
            }
        };

        var comboBoxModels = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel)).WithWidth(controlMinWidth);
        comboBoxModels.ItemTemplate = BuildModelItemTemplate(vm);
        _comboBoxModels = comboBoxModels;

        var buttonDownloadModel = UiUtil.MakeButton(vm.DownloadModelCommand, IconNames.Download)
            .WithMarginLeft(5)
            .WithBindIsVisible(nameof(vm.IsModelDownloadVisible));
        ToolTip.SetTip(buttonDownloadModel, Se.Language.General.Download);

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
                buttonDownloadModel,
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
                MakeApiKeyTextBox(vm),
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
        grid.Add(labelEngineDescription, 1, 0);
        grid.Add(panelModel, 2, 0);
        grid.Add(panelVoice, 3, 0);
        grid.Add(panelRegion, 4, 0);
        grid.Add(panelLanguage, 5, 0);
        grid.Add(panelApiKey, 6, 0);
        grid.Add(panelKeyFile, 7, 0);
        grid.Add(panelInstruction, 8, 0);

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
            Margin = new Thickness(0, 10, 0, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.DoReviewAudioClips)) { Mode = BindingMode.TwoWay }
        };
        var panelReviewAudioClips = WithCheckBoxHint(checkBoxReviewAudioClips, Se.Language.Video.TextToSpeech.ReviewAudioSegmentsHint);

        var checkBoxAddAudioToVideoFile = new CheckBox
        {
            Content = Se.Language.Video.TextToSpeech.AddAudioToVideoFile,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 0, 0),
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
        var panelAddAudioWithHint = WithCheckBoxHint(panelAddAudioToVideoFile, Se.Language.Video.TextToSpeech.AddAudioToVideoFileHint);

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

        grid.Add(panelReviewAudioClips, 0, 0);
        grid.Add(panelAddAudioWithHint, 1, 0);
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
        var labelPercent = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 6, 8, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ProgressPercentText)) { Mode = BindingMode.OneWay },
        };

        var labelEta = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Opacity = 0.7,
            FontSize = 12,
            Margin = new Thickness(8, 6, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ProgressEtaText)) { Mode = BindingMode.OneWay },
        };

        var labelStatus = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Left,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 6, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ProgressText)) { Mode = BindingMode.OneWay },
        };

        // percent | status | elapsed/remaining on one line
        var statusRow = new Grid
        {
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
        var label = statusRow;

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
