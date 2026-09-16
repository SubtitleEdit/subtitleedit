using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Icon = Optris.Icons.Avalonia.Icon;
using MenuItem = Avalonia.Controls.MenuItem;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;

public class ReviewSpeechWindow : Window
{
    private readonly ReviewSpeechViewModel _vm;
    private TableViewColumnManager? _columnManager;
    private SeTableViewColumn? _colNumber;
    private SeTableViewColumn? _colActor;
    private SeTableViewColumn? _colCps;
    private SeTableViewColumn? _colSpeed;
    private SeTableViewColumn? _colText;
    private SeTableViewColumn? _colEngine;
    private SeTableViewColumn? _colVoice;
    private SeTableViewColumn? _colLanguage;

    public ReviewSpeechWindow(ReviewSpeechViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.ReviewAudioSegments;
        Width = 1100;
        Height = 700;
        MinWidth = 700;
        MinHeight = 690;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var controls = MakeControls(vm);
        var lineGridView = MakeLineGrid(vm);
        var waveform = MakeWaveform(vm);

        // Disabled while a regenerate runs: its progress popup is non-modal, and publishing
        // (OK/Export) or closing mid-run would commit the row's half-updated step result.
        var buttonExport = UiUtil.MakeButton(Se.Language.General.ExportDotDotDot, vm.ExportCommand).WithBindEnabled(nameof(vm.IsRegenerateEnabled));
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindEnabled(nameof(vm.IsRegenerateEnabled));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand).WithBindEnabled(nameof(vm.IsRegenerateEnabled));
        var panelButtons = UiUtil.MakeButtonBar(buttonExport, buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var checkBoxAutoContinue = new CheckBox
        {
            Content = Se.Language.Video.TextToSpeech.AutoContinuePlaying,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.AutoContinue)) { Mode = BindingMode.TwoWay },
        };

        grid.Add(controls, 0, 0);
        grid.Add(lineGridView, 0, 1);
        grid.Add(waveform, 1, 0, 1, 2);
        grid.Add(panelButtons, 2, 0, 1, 2);
        grid.Add(checkBoxAutoContinue, 2, 0);

        Content = grid;

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
            UpdateDefaultColumnVisibility();
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

    private void UpdateDefaultColumnVisibility()
    {
        var hasMultiple = _vm.HasMultipleActors;
        if (_colActor != null) _colActor.IsVisible = hasMultiple;
        if (_colEngine != null) _colEngine.IsVisible = hasMultiple;
        if (_colVoice != null) _colVoice.IsVisible = hasMultiple;
        if (_colLanguage != null) _colLanguage.IsVisible = hasMultiple;
    }

    private Border MakeLineGrid(ReviewSpeechViewModel vm)
    {
        var lineGrid = TableViewExtras.MakeTableView(multiSelect: true);
        lineGrid.Margin = new Thickness(0, 10, 0, 0);
        lineGrid.Width = double.NaN;
        lineGrid.Height = double.NaN;
        lineGrid[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.Lines));
        lineGrid[!TableView.SelectedItemProperty] = new Binding(nameof(vm.SelectedLine)) { Mode = BindingMode.TwoWay };

        var gridFlyout = new MenuFlyout();
        gridFlyout.Opening += (_, _) => PopulateGridFlyout(gridFlyout, lineGrid, vm);
        lineGrid.ContextFlyout = gridFlyout;
        AttachGridRightClickFlyout(lineGrid, vm, () =>
        {
            PopulateGridFlyout(gridFlyout, lineGrid, vm);
            return gridFlyout;
        });
        UiUtil.AttachMacContextFlyoutHandler(lineGrid);

        var includeHeaderTheme = new ControlTheme(typeof(TableViewColumnHeader))
        {
            BasedOn = UiUtil.TableViewColumnHeaderTheme,
            Setters =
            {
                new Setter(ContentControl.ContentTemplateProperty, new FuncDataTemplate<object>((_, _) =>
                {
                    var allChecked = vm.Lines.Count > 0 && vm.Lines.All(l => l.Include);
                    var anyChecked = vm.Lines.Any(l => l.Include);
                    var headerCheckBox = new CheckBox
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        IsChecked = allChecked ? true : (anyChecked ? null : false),
                    };
                    var headerBorder = new Border
                    {
                        Background = Brushes.Transparent,
                        Padding = new Thickness(4),
                        Child = headerCheckBox,
                    };
                    var headerFlyout = new MenuFlyout();
                    headerFlyout.Opening += (_, _) => PopulateCheckboxHeaderFlyout(headerFlyout, vm);
                    headerBorder.ContextFlyout = headerFlyout;
                    UiUtil.AttachMacContextFlyoutHandler(headerBorder);

                    headerCheckBox.Click += (_, _) =>
                    {
                        vm.PushUndoSnapshot();
                        var target = headerCheckBox.IsChecked ?? false;
                        foreach (var line in vm.Lines)
                        {
                            line.Include = target;
                        }
                        if (vm.LineGrid != null)
                        {
                            SyncGridSelectionFromIncluded(vm.LineGrid, vm);
                        }
                    };

                    return headerBorder;
                })),
            }
        };

        _columnManager = new TableViewColumnManager(lineGrid);

        var colInclude = new SeTableViewColumn
        {
            Header = "Include",
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = includeHeaderTheme,
            CellTemplate = new FuncDataTemplate<ReviewRow>((item, _) =>
            {
                var checkBox = new CheckBox
                {
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(ReviewRow.Include)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                checkBox.AddHandler(InputElement.PointerPressedEvent, (_, _) =>
                {
                    vm.PushUndoSnapshot();
                }, Avalonia.Interactivity.RoutingStrategies.Tunnel);
                checkBox.AddHandler(InputElement.KeyDownEvent, (_, ke) =>
                {
                    if (ke.Key == Key.Space)
                    {
                        vm.PushUndoSnapshot();
                    }
                }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

                var border = new Border
                {
                    Padding = new Thickness(4),
                    Child = checkBox,
                };

                var flyout = new MenuFlyout();
                flyout.Opening += (_, _) => PopulateGridFlyout(flyout, lineGrid, vm);
                border.ContextFlyout = flyout;
                AttachCellRightClickFlyout(border, lineGrid, item, vm, () =>
                {
                    PopulateGridFlyout(flyout, lineGrid, vm);
                    return flyout;
                });
                AttachCellRightClickFlyout(checkBox, lineGrid, item, vm, () =>
                {
                    PopulateGridFlyout(flyout, lineGrid, vm);
                    return flyout;
                });

                return border;
            }),
            Width = new GridLength(50),
            Tag = "Include",
        };
        _columnManager.Add(colInclude);

        var colButtons = new SeTableViewColumn
        {
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<ReviewRow>((item, _) =>
            {
                var buttonRegenerate = UiUtil.MakeButton(vm.RegenerateAudioCommand, IconNames.Recycle, Se.Language.Video.TextToSpeech.RegenerateAudio)
                .WithBindEnabled(nameof(item.IsPlayingEnabled));
                buttonRegenerate.CommandParameter = item;

                var buttonHistory = UiUtil.MakeButton(vm.ShowHistoryCommand, IconNames.DotsVertical, Se.Language.General.ShowHistory).WithBindEnabled(nameof(ReviewRow.HasHistory));
                buttonHistory.CommandParameter = item;
                buttonHistory.Bind(Button.OpacityProperty, new Binding(nameof(ReviewRow.HistoryButtonOpacity)));

                var buttonPlay = UiUtil.MakeButton(vm.PlayRowCommand,"fa-solid fa-play")
                .WithBindIsVisible(nameof(item.IsPlaying), InverseBooleanConverter.Instance)
                .WithBindEnabled(nameof(item.IsPlayingEnabled));
                buttonPlay.CommandParameter = item;

                var buttonStop = UiUtil.MakeButton(vm.StopCommand, "fa-solid fa-stop")
                .WithBindIsVisible(nameof(item.IsPlaying));
                buttonStop.CommandParameter = item;

                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Spacing = 5,
                    Children =
                    {
                        buttonRegenerate,
                        buttonHistory,
                        buttonPlay,
                        buttonStop,
                    }
                };
            }),
            Width = new GridLength(150),
            Tag = "Buttons",
        };
        _columnManager.Add(colButtons);

        _colNumber = new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(ReviewRow.Number)),
            Width = new GridLength(50),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Tag = "Number",
        };
        _columnManager.Add(_colNumber);

        _colActor = new SeTableViewColumn
        {
            Header = Se.Language.General.Actor,
            Binding = new Binding(nameof(ReviewRow.Actor)),
            Width = new GridLength(100),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Tag = "Actor",
            IsVisible = vm.HasMultipleActors,
        };
        _columnManager.Add(_colActor);

        _colCps = new SeTableViewColumn
        {
            Header = Se.Language.General.CharsPerSec,
            Binding = new Binding(nameof(ReviewRow.Cps)),
            Width = new GridLength(80),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Tag = "Cps",
        };
        _columnManager.Add(_colCps);

        _colSpeed = new SeTableViewColumn
        {
            Header = Se.Language.General.Speed,
            Binding = new Binding(nameof(ReviewRow.Speed)),
            Width = new GridLength(70),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Tag = "Speed",
        };
        _columnManager.Add(_colSpeed);

        _colText = new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(ReviewRow.Text)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Tag = "Text",
        };
        _columnManager.Add(_colText);

        _colEngine = new SeTableViewColumn
        {
            Header = Se.Language.General.Engine,
            Binding = new Binding(nameof(ReviewRow.Engine)),
            Width = new GridLength(130),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Tag = "Engine",
            IsVisible = vm.HasMultipleActors,
        };
        _columnManager.Add(_colEngine);

        _colVoice = new SeTableViewColumn
        {
            Header = Se.Language.General.Voice,
            Binding = new Binding(nameof(ReviewRow.Voice)),
            Width = new GridLength(150),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Tag = "Voice",
            IsVisible = vm.HasMultipleActors,
        };
        _columnManager.Add(_colVoice);

        _colLanguage = new SeTableViewColumn
        {
            Header = Se.Language.General.Language,
            Binding = new Binding(nameof(ReviewRow.Language)),
            Width = new GridLength(110),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Tag = "Language",
            IsVisible = vm.HasMultipleActors,
        };
        _columnManager.Add(_colLanguage);

        UpdateDefaultColumnVisibility();

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
            Margin = new Thickness(0, 0, 0, 3),
        };
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            textBox.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }
        textBox.WithAccessibleName(Se.Language.General.Text); // edits the selected row's text; no visible label (#12087)

        var grid = new Grid
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
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(lineGrid, 0, 0);
        grid.Add(textBox, 1, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeControls(ReviewSpeechViewModel vm)
    {
        var labelMinWidth = 100;
        var controlMinWidth = 200;

        var comboBoxEngines = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine)).WithMinWidth(controlMinWidth);
        comboBoxEngines.SelectionChanged += vm.SelectedEngineChanged;
        var buttonEngineSettings = UiUtil.MakeButton(string.Empty, vm.ShowEngineSettingsCommand)
            .WithIconLeft(IconNames.Settings)
            .WithBindIsVisible(nameof(vm.IsEngineSettingsVisible));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonEngineSettings, Se.Language.General.Settings);
        }

        var buttonElevenLabsRest = UiUtil.MakeButton(Se.Language.General.Reset, vm.ElevenLabsResetCommand)
            .WithIconLeft(IconNames.Repeat)
            .WithBindIsVisible(nameof(vm.IsElevenLabsControlsVisible));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonElevenLabsRest, Se.Language.Video.TextToSpeech.ElevenLabsSettingsResetHint);
        }

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
                buttonEngineSettings,
                buttonElevenLabsRest,
            }
        };

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
                UiUtil.MakeComboBox(vm.Voices, vm, nameof(vm.SelectedVoice)).WithWidth(controlMinWidth),
            }
        };

        var comboBoxModels = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel)).WithWidth(controlMinWidth);
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
                UiUtil.MakeButton(vm.ShowElevenLabsEngineV3HelpCommand, IconNames.Help, $"{Se.Language.General.Model} - {Se.Language.General.Help}")
                    .WithBindIsVisible(nameof(vm.IsElevenLabsEngineV3Selected))
                    .WithMarginLeft(5),
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.SelectedEngine) + "." + nameof(ITtsEngine.HasModel)) { Mode = BindingMode.OneWay },
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
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.SelectedEngine) + "." + nameof(ITtsEngine.HasRegion)) { Mode = BindingMode.OneWay },
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
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.SelectedEngine) + "." + nameof(ITtsEngine.HasLanguageParameter)) { Mode = BindingMode.OneWay },
        };
        comboBoxLanguages.SelectionChanged += vm.SelectedLanguageChanged;


        var elevenLabsControls = MakeElevenLabsControls(vm);
        var panelInstruction = MakeInstructionPanel(vm, labelMinWidth);

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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // filler
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
        // Model comes before Voice — same ordering as the main TTS window and Cast dialog so the
        // user picks a model first (which sometimes filters the voice list) and the dropdowns
        // line up across windows.
        grid.Add(panelModel, 1, 0);
        grid.Add(panelVoice, 2, 0);
        grid.Add(panelRegion, 3, 0);
        grid.Add(panelLanguage, 4, 0);
        grid.Add(elevenLabsControls, 5, 0);
        grid.Add(panelInstruction, 6, 0);
        // 7 is filler

        return UiUtil.MakeBorderForControl(grid);
    }

    // Voice-design controls shared with the main TTS window: free-text instruction (Qwen3
    // VoiceDesign model) and OmniVoice keyword picker. Visibility flags on the VM mirror those
    // in TextToSpeechViewModel — see ReviewSpeechViewModel.UpdateInstructionVisibility.
    private static StackPanel MakeInstructionPanel(ReviewSpeechViewModel vm, int labelMinWidth)
    {
        var textBoxInstruction = new TextBox
        {
            Width = 260,
            Height = 90,
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

        return new StackPanel
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

        AddPickerRow(grid, 0, Se.Language.Video.TextToSpeech.VoiceGender, vm, vm.OmniVoiceGenders, nameof(vm.SelectedOmniVoiceGender));
        AddPickerRow(grid, 1, Se.Language.Video.TextToSpeech.VoiceAge, vm, vm.OmniVoiceAges, nameof(vm.SelectedOmniVoiceAge));
        AddPickerRow(grid, 2, Se.Language.Video.TextToSpeech.VoicePitch, vm, vm.OmniVoicePitches, nameof(vm.SelectedOmniVoicePitch));
        AddPickerRow(grid, 3, Se.Language.Video.TextToSpeech.VoiceAccent, vm, vm.OmniVoiceAccents, nameof(vm.SelectedOmniVoiceAccent));

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

    private static void AddPickerRow(Grid grid, int row, string label, ReviewSpeechViewModel vm,
        System.Collections.ObjectModel.ObservableCollection<string> items, string selectedPropertyPath)
    {
        grid.Add(new Label { Content = label, MinWidth = 60, VerticalAlignment = VerticalAlignment.Center }, row, 0);
        grid.Add(UiUtil.MakeComboBox(items, vm, selectedPropertyPath).WithWidth(200), row, 1);
    }

    private static Grid MakeElevenLabsControls(ReviewSpeechViewModel vm)
    {
        var sliderWidth = 150;

        var labelStability = UiUtil.MakeLabel(Se.Language.Video.TextToSpeech.Stability);
        var sliderStability = new Slider
        {
            Minimum = 0,
            Maximum = 1,
            Value = vm.Stability,
            Width = sliderWidth,
            [!Slider.ValueProperty] = new Binding(nameof(vm.Stability)),
        };

        var labelStabilityValue = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.Stability), new DoubleToTwoDecimalConverter());
        var buttonStability = UiUtil.MakeButton(vm.ShowStabilityHelpCommand, IconNames.Help, $"{Se.Language.Video.TextToSpeech.Stability} - {Se.Language.General.Help}");

        var labelSimilarity = UiUtil.MakeLabel(Se.Language.Video.TextToSpeech.Similarity);
        var sliderSimilarity = new Slider
        {
            Minimum = 0,
            Maximum = 1,
            Value = vm.Similarity,
            Width = sliderWidth,
            [!Slider.ValueProperty] = new Binding(nameof(vm.Similarity)),
        };
        var labelSimilarityValue = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.Similarity), new DoubleToTwoDecimalConverter());
        var buttonSimilarity = UiUtil.MakeButton(vm.ShowSimilarityHelpCommand, IconNames.Help, $"{Se.Language.Video.TextToSpeech.Similarity} - {Se.Language.General.Help}");

        var labelSpeakerBoost = UiUtil.MakeLabel(Se.Language.Video.TextToSpeech.SpeakerBoost);
        var sliderSpeakerBoost = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Value = vm.SpeakerBoost,
            Width = sliderWidth,
            [!Slider.ValueProperty] = new Binding(nameof(vm.SpeakerBoost)),
        };
        var labelSpeakerBoostValue = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.SpeakerBoost), new DoubleToTwoDecimalConverter());
        var buttonSpeakerBoost = UiUtil.MakeButton(vm.ShowSpeakerBoostHelpCommand, IconNames.Help, $"{Se.Language.Video.TextToSpeech.SpeakerBoost} - {Se.Language.General.Help}");

        var labelSpeed = UiUtil.MakeLabel(Se.Language.General.Speed);
        var sliderSpeed = new Slider
        {
            Minimum = 0.7,
            Maximum = 1.2,
            Value = vm.Speed,
            Width = sliderWidth,
            [!Slider.ValueProperty] = new Binding(nameof(vm.Speed)),
        };
        var labelSpeedValue = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.Speed), new DoubleToTwoDecimalConverter());
        var buttonSpeed = UiUtil.MakeButton(vm.ShowSpeedHelpCommand, IconNames.Help, $"{Se.Language.General.Speed} - {Se.Language.General.Help}");

        var labelStyleExaggeration = UiUtil.MakeLabel(Se.Language.General.StyleExaggeration);
        var sliderStyleExaggeration = new Slider
        {
            Minimum = 0.0,
            Maximum = 1.0,
            Value = vm.StyleExaggeration,
            Width = sliderWidth,
            Margin = new Thickness(5, 0, 0, 0),
            [!Slider.ValueProperty] = new Binding(nameof(ElevenLabsSettingsViewModel.StyleExaggeration)),
        };
        var labelStyleExaggerationValue = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.StyleExaggeration), new DoubleToTwoDecimalConverter());
        var buttonStyleExaggeration = UiUtil.MakeButton(vm.ShowStyleExaggerationHelpCommand, IconNames.Help, $"{Se.Language.General.StyleExaggeration} - {Se.Language.General.Help}");

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!Grid.IsVisibleProperty] = new Binding(nameof(vm.IsElevenLabsControlsVisible)) { Mode = BindingMode.OneWay },
        };

        grid.Add(labelStability, 0, 0);
        grid.Add(sliderStability, 0, 1);
        grid.Add(labelStabilityValue, 0, 2);
        grid.Add(buttonStability, 0, 3);

        grid.Add(labelSimilarity, 1, 0);
        grid.Add(sliderSimilarity, 1, 1);
        grid.Add(labelSimilarityValue, 1, 2);
        grid.Add(buttonSimilarity, 1, 3);

        grid.Add(labelSpeakerBoost, 2, 0);
        grid.Add(sliderSpeakerBoost, 2, 1);
        grid.Add(labelSpeakerBoostValue, 2, 2);
        grid.Add(buttonSpeakerBoost, 2, 3);

        grid.Add(labelSpeed, 3, 0);
        grid.Add(sliderSpeed, 3, 1);
        grid.Add(labelSpeedValue, 3, 2);
        grid.Add(buttonSpeed, 3, 3);

        grid.Add(labelStyleExaggeration, 4, 0);
        grid.Add(sliderStyleExaggeration, 4, 1);
        grid.Add(labelStyleExaggerationValue, 4, 2);
        grid.Add(buttonStyleExaggeration, 4, 3);

        return grid;
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
        var menuPlay = new MenuItem { Header = Se.Language.Video.TextToSpeech.PlayLine, Command = vm.PlayRowCommand };
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

        return new Border
        {
            Margin = new Thickness(2),
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

    private void AttachGridRightClickFlyout(TableView lineGrid, ReviewSpeechViewModel vm, Func<MenuFlyout> getFlyout)
    {
        lineGrid.AddHandler(InputElement.PointerPressedEvent, (s, e) =>
        {
            var point = e.GetCurrentPoint(lineGrid);
            if (point.Properties.IsRightButtonPressed)
            {
                var pos = e.GetPosition(lineGrid);
                var hitVisual = lineGrid.InputHitTest(pos) as Visual;
                if (TableViewExtras.IsInColumnHeader(hitVisual))
                {
                    var headerFlyout = MakeHeaderFlyout();
                    lineGrid.ContextFlyout = headerFlyout;
                    headerFlyout.ShowAt(lineGrid, showAtPointer: true);
                    e.Handled = true;
                    return;
                }

                if (TableViewExtras.IsInScrollBar(hitVisual))
                {
                    return;
                }

                var rowIndex = TableViewExtras.GetRowIndexFromPoint(lineGrid, pos);
                if (rowIndex >= 0 && rowIndex < vm.Lines.Count)
                {
                    var clickedRow = vm.Lines[rowIndex];
                    var selectedRows = lineGrid.SelectedItems?.OfType<ReviewRow>().ToList() ?? new List<ReviewRow>();

                    if (!selectedRows.Contains(clickedRow))
                    {
                        lineGrid.SelectedItem = clickedRow;
                        vm.SelectedLine = clickedRow;
                    }
                }

                var flyout = getFlyout();
                lineGrid.ContextFlyout = flyout;
                flyout.ShowAt(lineGrid, showAtPointer: true);
                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private MenuFlyout MakeHeaderFlyout()
    {
        var flyout = new MenuFlyout();
        PopulateHeaderFlyout(flyout);
        return flyout;
    }

    private void PopulateHeaderFlyout(MenuFlyout flyout)
    {
        flyout.Items.Clear();

        AddColumnToggleMenuItem(flyout, Se.Language.General.NumberSymbol, _colNumber);
        AddColumnToggleMenuItem(flyout, Se.Language.General.Actor, _colActor);
        AddColumnToggleMenuItem(flyout, Se.Language.General.CharsPerSec, _colCps);
        AddColumnToggleMenuItem(flyout, Se.Language.General.Speed, _colSpeed);
        AddColumnToggleMenuItem(flyout, Se.Language.General.Text, _colText);
        AddColumnToggleMenuItem(flyout, Se.Language.General.Engine, _colEngine);
        AddColumnToggleMenuItem(flyout, Se.Language.General.Voice, _colVoice);
        AddColumnToggleMenuItem(flyout, Se.Language.General.Language, _colLanguage);
    }

    private static void AddColumnToggleMenuItem(MenuFlyout flyout, string header, SeTableViewColumn? column)
    {
        if (column == null)
        {
            return;
        }

        var item = new MenuItem
        {
            Header = header,
        };
        if (column.IsVisible)
        {
            item.Icon = new Icon { Value = IconNames.Check, FontSize = 12 };
        }

        item.AddHandler(InputElement.PointerReleasedEvent, (s, e) =>
        {
            if (e.InitialPressMouseButton == MouseButton.Left ||
                e.GetCurrentPoint(item).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
            {
                e.Pointer.Capture(null);
                column.IsVisible = !column.IsVisible;
                item.Icon = column.IsVisible ? new Icon { Value = IconNames.Check, FontSize = 12 } : null;
                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        item.Click += (_, _) =>
        {
            column.IsVisible = !column.IsVisible;
            item.Icon = column.IsVisible ? new Icon { Value = IconNames.Check, FontSize = 12 } : null;
        };

        flyout.Items.Add(item);
    }

    private static void AddColumnToggleToSubMenu(MenuItem parent, string header, SeTableViewColumn? column)
    {
        if (column == null)
        {
            return;
        }

        var item = new MenuItem
        {
            Header = header,
        };
        if (column.IsVisible)
        {
            item.Icon = new Icon { Value = IconNames.Check, FontSize = 12 };
        }

        item.AddHandler(InputElement.PointerReleasedEvent, (s, e) =>
        {
            if (e.InitialPressMouseButton == MouseButton.Left ||
                e.GetCurrentPoint(item).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
            {
                e.Pointer.Capture(null);
                column.IsVisible = !column.IsVisible;
                item.Icon = column.IsVisible ? new Icon { Value = IconNames.Check, FontSize = 12 } : null;
                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        item.Click += (_, _) =>
        {
            column.IsVisible = !column.IsVisible;
            item.Icon = column.IsVisible ? new Icon { Value = IconNames.Check, FontSize = 12 } : null;
        };

        parent.Items.Add(item);
    }

    private static void AttachCellRightClickFlyout(Control control, TableView lineGrid, ReviewRow item, ReviewSpeechViewModel vm, Func<MenuFlyout> getFlyout)
    {
        control.AddHandler(InputElement.PointerPressedEvent, (s, e) =>
        {
            var point = e.GetCurrentPoint(control);
            if (point.Properties.IsRightButtonPressed)
            {
                var selectedRows = lineGrid.SelectedItems?.OfType<ReviewRow>().ToList() ?? new List<ReviewRow>();
                if (!selectedRows.Contains(item))
                {
                    lineGrid.SelectedItem = item;
                    vm.SelectedLine = item;
                }

                var flyout = getFlyout();
                control.ContextFlyout = flyout;
                flyout.ShowAt(control, showAtPointer: true);
                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private static List<ReviewRow> GetTargetRows(TableView lineGrid, ReviewSpeechViewModel vm)
    {
        var selected = lineGrid.SelectedItems?.OfType<ReviewRow>().ToList();
        if (selected != null && selected.Count > 0)
        {
            return selected;
        }
        if (vm.SelectedLine != null)
        {
            return new List<ReviewRow> { vm.SelectedLine };
        }
        return new List<ReviewRow>();
    }

    internal static void SyncGridSelectionFromIncluded(TableView lineGrid, ReviewSpeechViewModel vm)
    {
        if (lineGrid?.Selection == null)
        {
            return;
        }

        var firstIncluded = -1;
        lineGrid.Selection.BeginBatchUpdate();
        try
        {
            lineGrid.Selection.Clear();
            var runStart = -1;
            for (var i = 0; i < vm.Lines.Count; i++)
            {
                if (vm.Lines[i].Include)
                {
                    if (firstIncluded < 0)
                    {
                        firstIncluded = i;
                    }
                    if (runStart < 0)
                    {
                        runStart = i;
                    }
                }
                else
                {
                    if (runStart >= 0)
                    {
                        lineGrid.Selection.SelectRange(runStart, i - 1);
                        runStart = -1;
                    }
                }
            }
            if (runStart >= 0)
            {
                lineGrid.Selection.SelectRange(runStart, vm.Lines.Count - 1);
            }
        }
        finally
        {
            lineGrid.Selection.EndBatchUpdate();
        }

        if (firstIncluded >= 0)
        {
            vm.SelectedLine = vm.Lines[firstIncluded];
        }

        TableViewExtras.SyncSelectedItemsWithSelection(lineGrid);
    }

    private static MenuItem MakeSearchableSubMenu<T>(
        string header,
        IReadOnlyList<T> items,
        Func<T, string> getName,
        Func<T, bool> isSelected,
        Action<T> onSelect,
        MenuFlyout rootFlyout,
        string watermark,
        bool isEnabled)
    {
        var parentMenu = new MenuItem
        {
            Header = header,
            IsEnabled = isEnabled,
        };

        if (!isEnabled || items.Count == 0)
        {
            return parentMenu;
        }

        var searchBox = new TextBox
        {
            PlaceholderText = watermark,
            Margin = new Thickness(4, 2, 4, 4),
        };

        searchBox.AddHandler(InputElement.PointerPressedEvent, (s, e) =>
        {
            e.Handled = true;
        }, Avalonia.Interactivity.RoutingStrategies.Bubble, handledEventsToo: true);

        searchBox.AddHandler(InputElement.PointerReleasedEvent, (s, e) =>
        {
            e.Handled = true;
        }, Avalonia.Interactivity.RoutingStrategies.Bubble, handledEventsToo: true);

        searchBox.AddHandler(InputElement.TappedEvent, (s, e) =>
        {
            e.Handled = true;
        }, Avalonia.Interactivity.RoutingStrategies.Bubble, handledEventsToo: true);

        var itemsPanel = new StackPanel();
        var menuItems = new List<(T Item, MenuItem MenuItem, string Name)>(items.Count);

        foreach (var item in items)
        {
            var targetItem = item;
            var name = getName(targetItem);
            var menuItem = new MenuItem
            {
                Header = name,
            };
            if (isSelected(targetItem))
            {
                menuItem.Icon = new Icon { Value = IconNames.Check, FontSize = 12 };
            }
            menuItem.Click += (_, _) =>
            {
                onSelect(targetItem);
                rootFlyout.Hide();
            };
            itemsPanel.Children.Add(menuItem);
            menuItems.Add((targetItem, menuItem, name));
        }

        searchBox.TextChanged += (_, _) =>
        {
            var filter = searchBox.Text?.Trim() ?? string.Empty;
            foreach (var (_, mi, name) in menuItems)
            {
                mi.IsVisible = string.IsNullOrEmpty(filter) || name.Contains(filter, StringComparison.OrdinalIgnoreCase);
            }
        };

        searchBox.KeyDown += (_, ke) =>
        {
            if (ke.Key == Key.Enter)
            {
                ke.Handled = true;
                var first = menuItems.FirstOrDefault(m => m.MenuItem.IsVisible);
                if (first.MenuItem != null)
                {
                    onSelect(first.Item);
                    rootFlyout.Hide();
                }
            }
            else if (ke.Key == Key.Escape)
            {
                ke.Handled = true;
                rootFlyout.Hide();
            }
        };

        var scrollViewer = new ScrollViewer
        {
            MaxHeight = 350,
            Width = 270,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = itemsPanel,
        };

        var contentGrid = new Grid
        {
            Width = 270,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            Children =
            {
                searchBox,
                new Separator { Margin = new Thickness(0, 2) },
                scrollViewer,
            }
        };
        Grid.SetRow(searchBox, 0);
        Grid.SetRow((Control)contentGrid.Children[1], 1);
        Grid.SetRow(scrollViewer, 2);

        contentGrid.AddHandler(InputElement.PointerReleasedEvent, (s, e) =>
        {
            var isChildMenuItem = false;
            var current = e.Source as Visual;
            while (current != null && current != contentGrid)
            {
                if (current is MenuItem)
                {
                    isChildMenuItem = true;
                    break;
                }
                current = current.GetVisualParent();
            }

            if (!isChildMenuItem)
            {
                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Bubble, handledEventsToo: true);

        var containerItem = new MenuItem
        {
            Template = new FuncControlTemplate<MenuItem>((_, _) => contentGrid),
            Focusable = false,
        };

        containerItem.AddHandler(MenuItem.ClickEvent, (s, e) =>
        {
            if (e.Source == containerItem)
            {
                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Bubble, handledEventsToo: true);

        parentMenu.Items.Add(containerItem);

        parentMenu.SubmenuOpened += (_, _) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                searchBox.Focus();
                searchBox.SelectAll();
            }, Avalonia.Threading.DispatcherPriority.Input);
        };

        return parentMenu;
    }

    private static void PopulateGridFlyout(MenuFlyout flyout, TableView lineGrid, ReviewSpeechViewModel vm)
    {
        flyout.Items.Clear();

        var selectedRows = GetTargetRows(lineGrid, vm);

        var engineMenu = MakeSearchableSubMenu(
            Se.Language.General.Engine,
            vm.Engines,
            e => e.Name,
            e => selectedRows.Count > 0 && selectedRows.All(r => string.Equals(r.Engine, e.Name, StringComparison.OrdinalIgnoreCase)),
            targetEngine =>
            {
                var rows = GetTargetRows(lineGrid, vm);
                vm.ChangeEngineForRows(rows, targetEngine);
            },
            flyout,
            Se.Language.General.Search,
            vm.Engines.Count > 0 && selectedRows.Count > 0);
        flyout.Items.Add(engineMenu);

        var voiceMenu = MakeSearchableSubMenu(
            Se.Language.General.Voice,
            vm.Voices,
            v => v.Name,
            v => selectedRows.Count > 0 && selectedRows.All(r => r.SelectedVoice == v || string.Equals(r.Voice, v.Name, StringComparison.OrdinalIgnoreCase)),
            targetVoice =>
            {
                var rows = GetTargetRows(lineGrid, vm);
                vm.ChangeVoiceForRows(rows, targetVoice);
            },
            flyout,
            Se.Language.Video.TextToSpeech.SearchVoices,
            vm.Voices.Count > 0 && selectedRows.Count > 0);
        flyout.Items.Add(voiceMenu);

        var langMenu = MakeSearchableSubMenu(
            Se.Language.General.Language,
            vm.Languages,
            l => l.Name,
            l => selectedRows.Count > 0 && selectedRows.All(r => r.SelectedLanguage == l || string.Equals(r.Language, l.Name, StringComparison.OrdinalIgnoreCase) || string.Equals(r.Language, l.Code, StringComparison.OrdinalIgnoreCase)),
            targetLang =>
            {
                var rows = GetTargetRows(lineGrid, vm);
                vm.ChangeLanguageForRows(rows, targetLang);
            },
            flyout,
            Se.Language.General.Search,
            vm.Languages.Count > 0 && selectedRows.Count > 0);
        flyout.Items.Add(langMenu);

        flyout.Items.Add(new Separator());

        if (selectedRows.Count > 1)
        {
            var itemCheckSelected = new MenuItem
            {
                Header = "Check selected lines",
            };
            itemCheckSelected.Click += (_, _) =>
            {
                vm.PushUndoSnapshot();
                foreach (var r in selectedRows)
                {
                    r.Include = true;
                }
                SyncGridSelectionFromIncluded(lineGrid, vm);
            };
            flyout.Items.Add(itemCheckSelected);

            var itemUncheckSelected = new MenuItem
            {
                Header = "Uncheck selected lines",
            };
            itemUncheckSelected.Click += (_, _) =>
            {
                vm.PushUndoSnapshot();
                foreach (var r in selectedRows)
                {
                    r.Include = false;
                }
                SyncGridSelectionFromIncluded(lineGrid, vm);
            };
            flyout.Items.Add(itemUncheckSelected);

            flyout.Items.Add(new Separator());
        }

        var itemSelectAll = new MenuItem
        {
            Header = Se.Language.General.SelectAll,
        };
        itemSelectAll.Click += (_, _) =>
        {
            vm.PushUndoSnapshot();
            foreach (var line in vm.Lines)
            {
                line.Include = true;
            }
            SyncGridSelectionFromIncluded(lineGrid, vm);
        };
        flyout.Items.Add(itemSelectAll);

        var itemSelectNone = new MenuItem
        {
            Header = Se.Language.General.SelectNone,
        };
        itemSelectNone.Click += (_, _) =>
        {
            vm.PushUndoSnapshot();
            foreach (var line in vm.Lines)
            {
                line.Include = false;
            }
            SyncGridSelectionFromIncluded(lineGrid, vm);
        };
        flyout.Items.Add(itemSelectNone);

        var itemInvert = new MenuItem
        {
            Header = Se.Language.General.InvertSelection,
        };
        itemInvert.Click += (_, _) =>
        {
            vm.PushUndoSnapshot();
            foreach (var line in vm.Lines)
            {
                line.Include = !line.Include;
            }
            SyncGridSelectionFromIncluded(lineGrid, vm);
        };
        flyout.Items.Add(itemInvert);

        var actors = vm.Lines
            .Select(l => !string.IsNullOrWhiteSpace(l.Actor) ? l.Actor : (l.StepResult?.Paragraph?.Actor ?? l.WaveformParagraph?.Actor ?? string.Empty))
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();

        if (actors.Count > 0)
        {
            flyout.Items.Add(new Separator());
            var actorSubMenu = new MenuItem
            {
                Header = Se.Language.General.Actor,
            };
            foreach (var actor in actors)
            {
                var targetActor = actor;
                var actorItem = new MenuItem
                {
                    Header = targetActor,
                };
                actorItem.Click += (_, _) =>
                {
                    vm.PushUndoSnapshot();
                    foreach (var line in vm.Lines)
                    {
                        var a = !string.IsNullOrWhiteSpace(line.Actor) ? line.Actor : (line.StepResult?.Paragraph?.Actor ?? line.WaveformParagraph?.Actor);
                        line.Include = string.Equals(a, targetActor, StringComparison.OrdinalIgnoreCase);
                    }
                    SyncGridSelectionFromIncluded(lineGrid, vm);
                };
                actorSubMenu.Items.Add(actorItem);
            }
            flyout.Items.Add(actorSubMenu);
        }

        flyout.Items.Add(new Separator());
        var targetRow = selectedRows.FirstOrDefault() ?? vm.SelectedLine;
        var hasIncluded = vm.Lines.Any(l => l.Include);
        var itemRegenerate = new MenuItem
        {
            Header = Se.Language.Video.TextToSpeech.RegenerateAudio,
            IsEnabled = vm.IsRegenerateEnabled && (hasIncluded || (targetRow != null && targetRow.IsPlayingEnabled)),
        };
        itemRegenerate.Click += (_, _) =>
        {
            if (vm.Lines.Any(l => l.Include))
            {
                vm.RegenerateSelectedLinesCommand.Execute(null);
            }
            else if (targetRow != null)
            {
                vm.RegenerateAudioCommand.Execute(targetRow);
            }
        };
        flyout.Items.Add(itemRegenerate);
    }

    private void PopulateCheckboxHeaderFlyout(MenuFlyout flyout, ReviewSpeechViewModel vm)
    {
        flyout.Items.Clear();

        var itemSelectAll = new MenuItem
        {
            Header = Se.Language.General.SelectAll,
        };
        itemSelectAll.Click += (_, _) =>
        {
            vm.PushUndoSnapshot();
            foreach (var line in vm.Lines)
            {
                line.Include = true;
            }
            if (vm.LineGrid != null)
            {
                SyncGridSelectionFromIncluded(vm.LineGrid, vm);
            }
        };
        flyout.Items.Add(itemSelectAll);

        var itemSelectNone = new MenuItem
        {
            Header = Se.Language.General.SelectNone,
        };
        itemSelectNone.Click += (_, _) =>
        {
            vm.PushUndoSnapshot();
            foreach (var line in vm.Lines)
            {
                line.Include = false;
            }
            if (vm.LineGrid != null)
            {
                SyncGridSelectionFromIncluded(vm.LineGrid, vm);
            }
        };
        flyout.Items.Add(itemSelectNone);

        var itemInvert = new MenuItem
        {
            Header = Se.Language.General.InvertSelection,
        };
        itemInvert.Click += (_, _) =>
        {
            vm.PushUndoSnapshot();
            foreach (var line in vm.Lines)
            {
                line.Include = !line.Include;
            }
            if (vm.LineGrid != null)
            {
                SyncGridSelectionFromIncluded(vm.LineGrid, vm);
            }
        };
        flyout.Items.Add(itemInvert);

        var actors = vm.Lines
            .Select(l => !string.IsNullOrWhiteSpace(l.Actor) ? l.Actor : (l.StepResult?.Paragraph?.Actor ?? l.WaveformParagraph?.Actor ?? string.Empty))
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();

        if (actors.Count > 0)
        {
            flyout.Items.Add(new Separator());
            var actorSubMenu = new MenuItem
            {
                Header = Se.Language.General.Actor,
            };
            foreach (var actor in actors)
            {
                var targetActor = actor;
                var actorItem = new MenuItem
                {
                    Header = targetActor,
                };
                actorItem.Click += (_, _) =>
                {
                    vm.PushUndoSnapshot();
                    foreach (var line in vm.Lines)
                    {
                        var a = !string.IsNullOrWhiteSpace(line.Actor) ? line.Actor : (line.StepResult?.Paragraph?.Actor ?? line.WaveformParagraph?.Actor);
                        line.Include = string.Equals(a, targetActor, StringComparison.OrdinalIgnoreCase);
                    }
                    if (vm.LineGrid != null)
                    {
                        SyncGridSelectionFromIncluded(vm.LineGrid, vm);
                    }
                };
                actorSubMenu.Items.Add(actorItem);
            }
            flyout.Items.Add(actorSubMenu);
        }

        // Submenu Columns
        flyout.Items.Add(new Separator());
        var columnsSubMenu = new MenuItem
        {
            Header = Se.Language.General.Columns,
        };
        AddColumnToggleToSubMenu(columnsSubMenu, Se.Language.General.NumberSymbol, _colNumber);
        AddColumnToggleToSubMenu(columnsSubMenu, Se.Language.General.Actor, _colActor);
        AddColumnToggleToSubMenu(columnsSubMenu, Se.Language.General.CharsPerSec, _colCps);
        AddColumnToggleToSubMenu(columnsSubMenu, Se.Language.General.Speed, _colSpeed);
        AddColumnToggleToSubMenu(columnsSubMenu, Se.Language.General.Text, _colText);
        AddColumnToggleToSubMenu(columnsSubMenu, Se.Language.General.Engine, _colEngine);
        AddColumnToggleToSubMenu(columnsSubMenu, Se.Language.General.Voice, _colVoice);
        AddColumnToggleToSubMenu(columnsSubMenu, Se.Language.General.Language, _colLanguage);
        flyout.Items.Add(columnsSubMenu);

        flyout.Items.Add(new Separator());
        var itemRegenerateSelectedHeader = new MenuItem
        {
            Header = Se.Language.Video.TextToSpeech.RegenerateAudio,
            Command = vm.RegenerateSelectedLinesCommand,
            IsEnabled = vm.IsRegenerateEnabled && vm.Lines.Any(l => l.Include),
        };
        flyout.Items.Add(itemRegenerateSelectedHeader);
    }
}
