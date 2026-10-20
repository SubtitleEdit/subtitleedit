using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;

/// <summary>
/// Review the generated speech, in the editor look (see <see cref="EditorChrome"/>): engine and
/// voice for regenerating on the left, the lines with the selected line's text on the right, the
/// waveform under them, auto-continue, position and the buttons at the bottom.
/// </summary>
public class ReviewSpeechWindow : Window
{
    private readonly ReviewSpeechViewModel _vm;

    private const double LabelWidth = 76;
    private const double SliderLabelWidth = 124;

    public ReviewSpeechWindow(ReviewSpeechViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.ReviewAudioSegments;
        Width = 1100;
        Height = 700;
        MinWidth = 760;
        MinHeight = 520;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TextFieldsBare | EditorWindowStyles.MuteDisabledPrimary);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        // The side panel scrolls on its own and the list gives up height first, so the waveform
        // and the button bar stay reachable on a short screen.
        var side = new Border
        {
            Child = new ScrollViewer
            {
                Content = MakeControls(vm),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            },
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(350)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        content.Add(side, 0, 0);
        content.Add(MakeLineGrid(vm), 0, 1);

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)), // settings + lines
                new RowDefinition(GridLength.Auto), // waveform
                new RowDefinition(GridLength.Auto), // buttons
            },
        };
        root.Add(content, 0, 0);
        root.Add(MakeWaveform(vm), 1, 0);
        root.Add(MakeButtonBar(vm), 2, 0);

        Content = root;

        // Focus the grid, not a button, so the window receives key events (OnKeyDown needs a
        // focused element) without arming any button: a focused button fires OnClick on bare
        // Space/Enter, and OK used to be focused here - so the first Space a user pressed
        // published the whole session instead of playing the selected line (#12093).
        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(vm.LineGrid); });

        // Tunnel-stage handlers: see Space/R before the focused control does. KeyDown alone is
        // not enough - Avalonia's Button fires OnClick from OnKeyUp on Space (unconditionally
        // when focused, no IsPressed check), so a focused button still clicked on Space release
        // even with the KeyDown handled (#12093).
        AddHandler(KeyDownEvent, (_, e) => vm.OnPreviewKeyDown(e), Avalonia.Interactivity.RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, (_, e) => vm.OnPreviewKeyUp(e), Avalonia.Interactivity.RoutingStrategies.Tunnel);
        Loaded += delegate
        {
            vm.Loaded();
            // When Initialize already selected the first row (Lines.Count > 0), that selection
            // has already kicked off ApplyLineToLeftPanelAsync which loads the right engine's
            // voices/models for the row. Firing SelectedEngineChanged here would post another
            // fire-and-forget refresh that races (and wins against) the row sync, replacing the
            // row's voice/model/instruction with the engine's defaults.
            if (vm.Lines.Count == 0)
            {
                vm.SelectedEngineChanged();
            }
        };
    }

    /// <summary>
    /// Auto-continue and the position on the left; Export, Cancel and OK on the right.
    /// </summary>
    private static Border MakeButtonBar(ReviewSpeechViewModel vm)
    {
        var autoContinue = TtsMainEditorLook.SwitchRow(nameof(vm.AutoContinue), Se.Language.Video.TextToSpeech.AutoContinuePlaying);
        autoContinue.HorizontalAlignment = HorizontalAlignment.Left;

        var left = TtsMainEditorLook.Horizontal(autoContinue, EditorChrome.MakeToolbarSeparator(), MakePositionLabel(vm));
        left.Spacing = 4;

        // Disabled while a regenerate runs: its progress popup is non-modal, and publishing
        // (OK/Export) or closing mid-run would commit the row's half-updated step result.
        var buttonExport = UiUtil.MakeButton(Se.Language.General.ExportDotDotDot, vm.ExportCommand).WithBindEnabled(nameof(vm.IsRegenerateEnabled));
        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand).WithBindEnabled(nameof(vm.IsRegenerateEnabled)));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand).WithBindEnabled(nameof(vm.IsRegenerateEnabled));
        return EditorChrome.MakeButtonBar(left, buttonExport, buttonCancel, buttonOk);
    }

    private static Grid MakeLineGrid(ReviewSpeechViewModel vm)
    {
        var lineGrid = TableViewExtras.MakeTableView(multiSelect: false);
        lineGrid.Width = double.NaN;
        lineGrid.Height = double.NaN;
        lineGrid[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.Lines));
        lineGrid[!TableView.SelectedItemProperty] = new Binding(nameof(vm.SelectedLine)) { Mode = BindingMode.TwoWay };
        AutomationProperties.SetName(lineGrid, Se.Language.General.Lines);

        // Re-enabled: OK publishes only rows with Include ticked and Export/Import
        // round-trip the flag, so without this column an imported session's excluded
        // rows were invisible and could never be re-included.
        lineGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<ReviewRow>(nameof(ReviewRow.Include), focusable: true),
            Width = new GridLength(80),
        });
        lineGrid.Columns.Add(new SeTableViewColumn
        {
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<ReviewRow>((item, _) =>
            {
                var buttonRegenerate = EditorChrome.CreateToolButton("fa-solid fa-rotate", Se.Language.Video.TextToSpeech.RegenerateAudio, vm.RegenerateAudioCommand, 26)
                    .WithBindEnabled(nameof(item.IsPlayingEnabled));
                buttonRegenerate.CommandParameter = item;

                var buttonHistory = EditorChrome.CreateToolButton("fa-solid fa-clock-rotate-left", Se.Language.General.ShowHistory, vm.ShowHistoryCommand, 26)
                    .WithBindEnabled(nameof(ReviewRow.HasHistory));
                buttonHistory.CommandParameter = item;
                buttonHistory.Bind(Button.OpacityProperty, new Binding(nameof(ReviewRow.HistoryButtonOpacity)));

                var buttonPlay = EditorChrome.CreateToolButton("fa-solid fa-play", Se.Language.General.PlayLine, vm.PlayRowCommand, 26)
                    .WithBindIsVisible(nameof(item.IsPlaying), InverseBooleanConverter.Instance)
                    .WithBindEnabled(nameof(item.IsPlayingEnabled));
                buttonPlay.CommandParameter = item;

                var buttonStop = EditorChrome.CreateToolButton("fa-solid fa-stop", Se.Language.General.Stop, vm.StopCommand, 26)
                    .WithBindIsVisible(nameof(item.IsPlaying));
                buttonStop.CommandParameter = item;

                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Spacing = 2,
                    Children =
                    {
                        buttonRegenerate,
                        buttonHistory,
                        buttonPlay,
                        buttonStop,
                    }
                };
            }),
            Width = new GridLength(110),
        });
        lineGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(ReviewRow.Number)),
            Width = new GridLength(50),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        lineGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Voice,
            Binding = new Binding(nameof(ReviewRow.Voice)),
            Width = new GridLength(150),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        lineGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.CharsPerSec,
            Binding = new Binding(nameof(ReviewRow.Cps)),
            Width = new GridLength(80),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        lineGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Speed,
            Binding = new Binding(nameof(ReviewRow.Speed)),
            Width = new GridLength(70),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        lineGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(ReviewRow.Text)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        lineGrid.DoubleTapped += (s, e) => vm.LineGridDoubleClicked();
        vm.LineGrid = lineGrid;

        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 80,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SelectedLine) + "." + nameof(ReviewRow.Text))
            {
                Mode = BindingMode.TwoWay
            },
            FontSize = Se.Settings.Appearance.SubtitleTextBoxFontSize,
            FontWeight = Se.Settings.Appearance.SubtitleTextBoxFontBold ? FontWeight.Bold : FontWeight.Normal,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(0, 10, 0, 0),
        };
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            textBox.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }
        textBox.WithAccessibleName(Se.Language.General.Text); // edits the selected row's text; no visible label (#12087)

        var header = EditorChrome.MakeSectionHeader(Se.Language.General.Lines);
        header.Margin = new Thickness(0, 0, 0, 10);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = new Thickness(14, 12, 14, 14),
        };

        grid.Add(header, 0, 0);
        grid.Add(EditorChrome.MakeListFrame(lineGrid), 1, 0);
        grid.Add(textBox, 2, 0);
        return grid;
    }

    private static Grid Row(string? label, Control content, string? labelBinding = null) => EditorChrome.MakeLabeledRow(label, content, LabelWidth, spacing: 6, labelBinding: labelBinding);

    /// <summary>
    /// A combo box filling the row, with optional tool buttons after it.
    /// </summary>
    private static Grid ComboRow(string? label, ComboBox comboBox, string name, string? labelBinding = null, params Control[] buttons)
    {
        comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboBox.Width = double.NaN;
        comboBox.MinWidth = 0;
        AutomationProperties.SetName(comboBox, name);
        // Docked rather than in grid columns: a hidden button then takes no room (no column
        // spacing left behind), so the combo boxes of all rows end at the same edge.
        var panel = new DockPanel();
        for (var i = buttons.Length - 1; i >= 0; i--)
        {
            buttons[i].Margin = new Thickness(4, 0, 0, 0);
            DockPanel.SetDock(buttons[i], Dock.Right);
            panel.Children.Add(buttons[i]);
        }

        panel.Children.Add(comboBox);
        return Row(label, panel, labelBinding);
    }

    private static StackPanel MakeControls(ReviewSpeechViewModel vm)
    {
        var comboBoxEngines = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine));
        comboBoxEngines.SelectionChanged += vm.SelectedEngineChanged;
        var buttonEngineSettings = EditorChrome.CreateToolButton("fa-solid fa-gear", Se.Language.General.Settings, vm.ShowEngineSettingsCommand, 28)
            .WithBindIsVisible(nameof(vm.IsEngineSettingsVisible));

        var buttonElevenLabsRest = EditorChrome.CreateToolButton("fa-solid fa-rotate-left", Se.Language.Video.TextToSpeech.ElevenLabsSettingsResetHint, vm.ElevenLabsResetCommand, 28)
            .WithBindIsVisible(nameof(vm.IsElevenLabsControlsVisible));
        AutomationProperties.SetName(buttonElevenLabsRest, Se.Language.General.Reset);

        var panelEngine = ComboRow(Se.Language.General.Engine, comboBoxEngines, Se.Language.General.Engine, null, buttonEngineSettings, buttonElevenLabsRest);

        var panelVoice = ComboRow(Se.Language.General.Voice, UiUtil.MakeComboBox(vm.Voices, vm, nameof(vm.SelectedVoice)), Se.Language.General.Voice);

        var comboBoxModels = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel));
        var buttonModelHelp = TtsMainEditorLook.MakeHelpButton(vm.ShowElevenLabsEngineV3HelpCommand, $"{Se.Language.General.Model} - {Se.Language.General.Help}")
            .WithBindIsVisible(nameof(vm.IsElevenLabsEngineV3Selected));
        var panelModel = ComboRow(Se.Language.General.Model, comboBoxModels, Se.Language.General.Model, null, buttonModelHelp);
        panelModel.Bind(IsVisibleProperty, new Binding(nameof(vm.SelectedEngine) + "." + nameof(ITtsEngine.HasModel)) { Mode = BindingMode.OneWay });
        comboBoxModels.SelectionChanged += vm.SelectedModelChanged;

        var comboBoxRegions = UiUtil.MakeComboBox(vm.Regions, vm, nameof(vm.SelectedRegion));
        var panelRegion = ComboRow(null, comboBoxRegions, Se.Language.General.Region, nameof(vm.RegionLabel));
        comboBoxRegions.Bind(AutomationProperties.NameProperty, new Binding(nameof(vm.RegionLabel)) { Mode = BindingMode.OneWay });
        panelRegion.Bind(IsVisibleProperty, new Binding(nameof(vm.SelectedEngine) + "." + nameof(ITtsEngine.HasRegion)) { Mode = BindingMode.OneWay });

        var comboBoxLanguages = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage));
        var panelLanguage = ComboRow(Se.Language.General.Language, comboBoxLanguages, Se.Language.General.Language);
        panelLanguage.Bind(IsVisibleProperty, new Binding(nameof(vm.SelectedEngine) + "." + nameof(ITtsEngine.HasLanguageParameter)) { Mode = BindingMode.OneWay });
        comboBoxLanguages.SelectionChanged += vm.SelectedLanguageChanged;

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(panelEngine);
        // Model comes before Voice — same ordering as the main TTS window and Cast dialog so the
        // user picks a model first (which sometimes filters the voice list) and the dropdowns
        // line up across windows.
        rows.Children.Add(panelModel);
        rows.Children.Add(panelVoice);
        rows.Children.Add(panelRegion);
        rows.Children.Add(panelLanguage);
        rows.Children.Add(MakeInstructionPanel(vm));

        var panel = new StackPanel();
        panel.Children.Add(EditorChrome.MakeEditorSection(Se.Language.Video.TextToSpeech.EngineAndVoice, rows, first: true));
        var elevenLabs = EditorChrome.MakeEditorSection(Se.Language.Video.TextToSpeech.ElevenLabsSettings, MakeElevenLabsControls(vm));
        elevenLabs.Bind(IsVisibleProperty, new Binding(nameof(vm.IsElevenLabsControlsVisible)) { Mode = BindingMode.OneWay });
        panel.Children.Add(elevenLabs);
        return panel;
    }

    // Voice-design controls shared with the main TTS window: free-text instruction (Qwen3
    // VoiceDesign model) and OmniVoice keyword picker. Visibility flags on the VM mirror those
    // in TextToSpeechViewModel — see ReviewSpeechViewModel.UpdateInstructionVisibility.
    private static Grid MakeInstructionPanel(ReviewSpeechViewModel vm)
    {
        var textBoxInstruction = new TextBox
        {
            Height = 90,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalContentAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Stretch,
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

        var row = Row(Se.Language.Video.TextToSpeech.VoiceInstruction, new StackPanel { Children = { textBoxInstruction, MakeInstructionKeywordPicker(vm) } });
        row.Children[0].VerticalAlignment = VerticalAlignment.Top;
        row.Children[0].Margin = new Thickness(0, 6, 0, 0);
        row.Bind(IsVisibleProperty, new Binding(nameof(vm.HasInstruction)) { Mode = BindingMode.OneWay });
        return row;
    }

    // OmniVoice keyword picker — gender/age/pitch/accent combos plus a whisper checkbox and a
    // "doesn't apply to cloned voices" hint. Mirrors the picker in the main TTS window so the
    // user sees the same control set regardless of which window they regenerate from.
    private static Control MakeInstructionKeywordPicker(ReviewSpeechViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
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

        AddPickerRow(grid, 0, Se.Language.Video.TextToSpeech.VoiceGender, vm, vm.OmniVoiceGenders, nameof(vm.SelectedOmniVoiceGender));
        AddPickerRow(grid, 1, Se.Language.Video.TextToSpeech.VoiceAge, vm, vm.OmniVoiceAges, nameof(vm.SelectedOmniVoiceAge));
        AddPickerRow(grid, 2, Se.Language.Video.TextToSpeech.VoicePitch, vm, vm.OmniVoicePitches, nameof(vm.SelectedOmniVoicePitch));
        AddPickerRow(grid, 3, Se.Language.Video.TextToSpeech.VoiceAccent, vm, vm.OmniVoiceAccents, nameof(vm.SelectedOmniVoiceAccent));

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
        clonedVoiceNote.Margin = new Thickness(0, 6, 0, 0);
        clonedVoiceNote.Bind(IsVisibleProperty, new Binding(nameof(vm.IsInstructionVoiceHintVisible)) { Mode = BindingMode.OneWay });

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { grid, clonedVoiceNote },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.IsInstructionPickerVisible)) { Mode = BindingMode.OneWay },
        };
    }

    private static void AddPickerRow(Grid grid, int row, string label, ReviewSpeechViewModel vm,
        System.Collections.ObjectModel.ObservableCollection<string> items, string selectedPropertyPath)
    {
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0));
        text.MinWidth = 50;
        grid.Add(text, row, 0);
        var comboBox = UiUtil.MakeComboBox(items, vm, selectedPropertyPath);
        comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(comboBox, label);
        grid.Add(comboBox, row, 1);
    }

    /// <summary>
    /// "Label  [slider] value (?)" - the label column is wider than the engine rows' so
    /// "Style exaggeration" fits.
    /// </summary>
    private static Grid SliderRow(string label, Slider slider, Control value, Control help)
    {
        AutomationProperties.SetName(slider, label);
        slider.HorizontalAlignment = HorizontalAlignment.Stretch;
        slider.Width = double.NaN;
        slider.Margin = new Thickness(0);
        slider.VerticalAlignment = VerticalAlignment.Center;
        value.VerticalAlignment = VerticalAlignment.Center;
        var grid = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(34)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        grid.Add(slider, 0, 0);
        grid.Add(value, 0, 1);
        grid.Add(help, 0, 2);
        return EditorChrome.MakeLabeledRow(label, grid, SliderLabelWidth, spacing: 6);
    }

    private static TextBlock SliderValue(ReviewSpeechViewModel vm, string property)
    {
        return new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            FontFeatures = new FontFeatureCollection { FontFeature.Parse("tnum") },
            [!TextBlock.TextProperty] = new Binding(property) { Converter = new DoubleToTwoDecimalConverter(), Source = vm },
        };
    }

    private static StackPanel MakeElevenLabsControls(ReviewSpeechViewModel vm)
    {
        var sliderStability = new Slider
        {
            Minimum = 0,
            Maximum = 1,
            Value = vm.Stability,
            [!Slider.ValueProperty] = new Binding(nameof(vm.Stability)),
        };
        var buttonStability = TtsMainEditorLook.MakeHelpButton(vm.ShowStabilityHelpCommand, $"{Se.Language.Video.TextToSpeech.Stability} - {Se.Language.General.Help}");

        var sliderSimilarity = new Slider
        {
            Minimum = 0,
            Maximum = 1,
            Value = vm.Similarity,
            [!Slider.ValueProperty] = new Binding(nameof(vm.Similarity)),
        };
        var buttonSimilarity = TtsMainEditorLook.MakeHelpButton(vm.ShowSimilarityHelpCommand, $"{Se.Language.Video.TextToSpeech.Similarity} - {Se.Language.General.Help}");

        var sliderSpeakerBoost = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Value = vm.SpeakerBoost,
            [!Slider.ValueProperty] = new Binding(nameof(vm.SpeakerBoost)),
        };
        var buttonSpeakerBoost = TtsMainEditorLook.MakeHelpButton(vm.ShowSpeakerBoostHelpCommand, $"{Se.Language.Video.TextToSpeech.SpeakerBoost} - {Se.Language.General.Help}");

        var sliderSpeed = new Slider
        {
            Minimum = 0.7,
            Maximum = 1.2,
            Value = vm.Speed,
            [!Slider.ValueProperty] = new Binding(nameof(vm.Speed)),
        };
        var buttonSpeed = TtsMainEditorLook.MakeHelpButton(vm.ShowSpeedHelpCommand, $"{Se.Language.General.Speed} - {Se.Language.General.Help}");

        var sliderStyleExaggeration = new Slider
        {
            Minimum = 0.0,
            Maximum = 1.0,
            Value = vm.StyleExaggeration,
            [!Slider.ValueProperty] = new Binding(nameof(ElevenLabsSettingsViewModel.StyleExaggeration)),
        };
        var buttonStyleExaggeration = TtsMainEditorLook.MakeHelpButton(vm.ShowStyleExaggerationHelpCommand, $"{Se.Language.General.StyleExaggeration} - {Se.Language.General.Help}");

        var rows = new StackPanel { Spacing = 2 };
        rows.Children.Add(SliderRow(Se.Language.Video.TextToSpeech.Stability, sliderStability, SliderValue(vm, nameof(vm.Stability)), buttonStability));
        rows.Children.Add(SliderRow(Se.Language.Video.TextToSpeech.Similarity, sliderSimilarity, SliderValue(vm, nameof(vm.Similarity)), buttonSimilarity));
        rows.Children.Add(SliderRow(Se.Language.Video.TextToSpeech.SpeakerBoost, sliderSpeakerBoost, SliderValue(vm, nameof(vm.SpeakerBoost)), buttonSpeakerBoost));
        rows.Children.Add(SliderRow(Se.Language.General.Speed, sliderSpeed, SliderValue(vm, nameof(vm.Speed)), buttonSpeed));
        rows.Children.Add(SliderRow(Se.Language.General.StyleExaggeration, sliderStyleExaggeration, SliderValue(vm, nameof(vm.StyleExaggeration)), buttonStyleExaggeration));
        return rows;
    }

    private static TextBlock MakePositionLabel(ReviewSpeechViewModel vm)
    {
        var label = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PositionText)),
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 15,
            FontFeatures = new FontFeatureCollection { FontFeature.Parse("tnum") },
            Background = Brushes.Transparent, // hit-testable between the digits
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(label, Se.Language.Video.GoToVideoPositionDotDotDot);
        }

        label.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(label).Properties.IsLeftButtonPressed)
            {
                e.Handled = true;
                vm.ShowGoToPositionCommand.Execute(null);
            }
        };

        return label;
    }

    private static Border MakeWaveform(ReviewSpeechViewModel vm)
    {
        // Mirror the main window's waveform theme so the review waveform looks the same as the
        // one users are already used to.
        var settings = Se.Settings.Waveform;
        var audioVisualizer = new AudioVisualizer
        {
            DrawGridLines = settings.DrawGridLines,
            WaveformColor = settings.WaveformColor.FromHexToColor(),
            WaveformBackgroundColor = settings.WaveformBackgroundColor.FromHexToColor(),
            WaveformSelectedColor = settings.WaveformSelectedColor.FromHexToColor(),
            WaveformCursorColor = settings.WaveformCursorColor.FromHexToColor(),
            WaveformShotChangeColor = settings.WaveformShotChangeColor.FromHexToColor(),
            WaveformGridColor = settings.WaveformGridColor.FromHexToColor(),
            WaveformParagraphLeftColor = settings.WaveformParagraphLeftColor.FromHexToColor(),
            WaveformParagraphRightColor = settings.WaveformParagraphRightColor.FromHexToColor(),
            WaveformFancyHighColor = settings.WaveformFancyHighColor.FromHexToColor(),
            ParagraphBackground = settings.ParagraphBackground.FromHexToColor(),
            ParagraphSelectedBackground = settings.ParagraphSelectedBackground.FromHexToColor(),
            InvertMouseWheel = settings.InvertMouseWheel,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = double.NaN,
            WaveformDrawStyle = InitWaveform.GetWaveformDrawStyle(settings.WaveformDrawStyle),
            MinGapSeconds = Se.Settings.General.MinimumBetweenLines.GetMilliseconds() / 1000.0,
            FocusOnMouseOver = settings.FocusOnMouseOver,
            IsReadOnly = Se.Settings.General.LockTimeCodes,
            WaveformHeightPercentage = settings.SpectrogramCombinedWaveformHeight,
        };

        vm.AudioVisualizer = audioVisualizer;
        audioVisualizer.Bind(AudioVisualizer.WavePeaksProperty, new Binding(nameof(vm.WavePeakData)));

        // Re-center on the selected paragraph whenever peaks arrive (e.g., async-loaded) or the
        // user picks a different row. SelectedLine changes also call RefreshWaveformPosition via
        // the VM's partial OnSelectedLineChanged.
        audioVisualizer.PropertyChanged += (_, e) =>
        {
            if (e.Property == AudioVisualizer.WavePeaksProperty)
            {
                vm.RefreshWaveformPosition();
            }
            else if (e.Property == AudioVisualizer.CurrentVideoPositionSecondsProperty)
            {
                vm.UpdatePositionText(audioVisualizer.CurrentVideoPositionSeconds);
            }
            else if (e.Property == AudioVisualizer.StartPositionSecondsProperty ||
                     e.Property == AudioVisualizer.ZoomFactorProperty ||
                     e.Property == BoundsProperty)
            {
                // The control only draws the blocks around the view it was last handed, and
                // nothing here feeds it on a timer like the main window does - so scrolling,
                // zooming out or widening the window ran past them into empty waveform (#15102).
                vm.ReloadWaveformParagraphs();
            }
        };

        // Clicking or grabbing a block selects its row (#14000). The control only raises
        // OnPrimarySingleClicked when something listens to OnVideoPositionChanged, hence the
        // empty playhead handler. OnDragStarted fires on press so the row is selected before a
        // move/resize mutates it; OnSelectRequested covers right-click-selects.
        audioVisualizer.OnVideoPositionChanged += (_, _) => { };
        audioVisualizer.OnPrimarySingleClicked += (_, e) =>
        {
            vm.SelectFromWaveform(e.Paragraph);
            vm.OnWaveformPositionClicked(e.Seconds);
        };
        audioVisualizer.OnDragStarted += (_, e) => vm.SelectFromWaveform(e.Paragraph);
        audioVisualizer.OnSelectRequested += (_, e) => vm.SelectFromWaveform(e.Paragraph);

        // Generated-clip length bar under each block (green fits / red overrun).
        audioVisualizer.ParagraphAudioLengthProvider = vm.GetWaveformParagraphAudioLength;

        // Context menu: the row actions from the grid plus the two timing fixes that only make
        // sense here. The target is the row under the pointer (selected on open), so the items
        // take it as CommandParameter rather than relying on SelectedLine.
        var menuPlay = new MenuItem { Header = Se.Language.General.PlayLine, Command = vm.PlayRowCommand };
        var menuRegenerate = new MenuItem { Header = Se.Language.Video.TextToSpeech.RegenerateAudio, Command = vm.RegenerateAudioCommand };
        var menuHistory = new MenuItem { Header = Se.Language.General.ShowHistory, Command = vm.ShowHistoryCommand };
        var menuFit = new MenuItem { Header = Se.Language.Video.TextToSpeech.FitDurationToGeneratedAudio, Command = vm.FitDurationToAudioCommand };
        var menuReset = new MenuItem { Header = Se.Language.Video.TextToSpeech.ResetTiming, Command = vm.ResetTimingCommand };
        var flyout = new MenuFlyout();
        flyout.Items.Add(menuPlay);
        flyout.Items.Add(menuRegenerate);
        flyout.Items.Add(menuHistory);
        flyout.Items.Add(new Separator());
        flyout.Items.Add(menuFit);
        flyout.Items.Add(menuReset);
        audioVisualizer.MenuFlyout = flyout;
        audioVisualizer.FlyoutMenuOpening += (_, e) =>
        {
            var row = vm.SelectRowAtWaveformPosition(e.PositionInSeconds);
            foreach (var item in new[] { menuPlay, menuRegenerate, menuHistory, menuFit, menuReset })
            {
                item.CommandParameter = row;
                item.IsEnabled = row != null;
            }

            if (row != null)
            {
                menuPlay.IsEnabled = row.IsPlayingEnabled && !row.IsPlaying;
                menuRegenerate.IsEnabled = vm.IsRegenerateEnabled && row.IsPlayingEnabled;
                menuHistory.IsEnabled = row.HasHistory;
                menuFit.IsEnabled = vm.GetGeneratedAudioLengthSeconds(row) > 0 && !audioVisualizer.IsReadOnly;
                menuReset.IsEnabled = !audioVisualizer.IsReadOnly;
            }
        };

        // A plain border with a line on top - the waveform keeps its own colors
        return new Border
        {
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Height = 120,
            Child = audioVisualizer,
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled && FocusManager?.GetFocusedElement() is AudioVisualizer && _vm.OnWaveformKeyDown(e))
        {
            e.Handled = true;
            return;
        }

        _vm.OnKeyDown(e);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing(e);
    }
}
