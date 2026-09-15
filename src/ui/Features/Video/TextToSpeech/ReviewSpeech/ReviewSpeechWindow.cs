using Avalonia;
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
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using Optris.Icons.Avalonia;
using MenuItem = Avalonia.Controls.MenuItem;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;

public class ReviewSpeechWindow : Window
{
    private readonly ReviewSpeechViewModel _vm;

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

    private static Border MakeLineGrid(ReviewSpeechViewModel vm)
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

        var headerFlyout = new MenuFlyout();
        headerFlyout.Opening += (_, _) => PopulateCheckboxHeaderFlyout(headerFlyout, vm);

        var headerText = new TextBlock
        {
            Text = Se.Language.General.Enabled,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var headerBorder = new Border
        {
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Child = headerText,
            ContextFlyout = headerFlyout,
        };
        AttachExplicitRightClickFlyout(headerBorder, () =>
        {
            PopulateCheckboxHeaderFlyout(headerFlyout, vm);
            return headerFlyout;
        });
        UiUtil.AttachMacContextFlyoutHandler(headerBorder);

        // Re-enabled: OK publishes only rows with Include ticked and Export/Import
        // round-trip the flag, so without this column an imported session's excluded
        // rows were invisible and could never be re-included.
        lineGrid.Columns.Add(new SeTableViewColumn
        {
            Header = headerBorder,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<ReviewRow>((item, _) =>
            {
                var checkBox = new CheckBox
                {
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(ReviewRow.Include)),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                var border = new Border
                {
                    Background = Brushes.Transparent, // Prevents highlighting
                    Padding = new Thickness(4),
                    Child = checkBox
                };

                var flyout = new MenuFlyout();
                flyout.Opening += (_, _) => PopulateGridFlyout(flyout, lineGrid, vm);
                border.ContextFlyout = flyout;
                checkBox.ContextFlyout = flyout;
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
                UiUtil.AttachMacContextFlyoutHandler(border);
                UiUtil.AttachMacContextFlyoutHandler(checkBox);

                return border;
            }),
            Width = new GridLength(80),
        });
        lineGrid.Columns.Add(new SeTableViewColumn
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
        });
        lineGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(ReviewRow.Number)),
            Width = new GridLength(50),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        var hasActors = vm.Lines.Any(l => !string.IsNullOrWhiteSpace(l.StepResult?.Paragraph?.Actor ?? l.WaveformParagraph?.Actor));
        if (!hasActors)
        {
            lineGrid.Columns.Add(new SeTableViewColumn
            {
                Header = Se.Language.General.Voice,
                Binding = new Binding(nameof(ReviewRow.Voice)),
                Width = new GridLength(140),
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            });
            lineGrid.Columns.Add(new SeTableViewColumn
            {
                Header = Se.Language.General.Language,
                Binding = new Binding(nameof(ReviewRow.Language)),
                Width = new GridLength(100),
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            });
        }
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
        if (hasActors)
        {
            lineGrid.Columns.Add(new SeTableViewColumn
            {
                Header = Se.Language.General.Voice,
                Binding = new Binding(nameof(ReviewRow.Voice)),
                Width = new GridLength(140),
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            });

            lineGrid.Columns.Add(new SeTableViewColumn
            {
                Header = Se.Language.General.Language,
                Binding = new Binding(nameof(ReviewRow.Language)),
                Width = new GridLength(100),
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            });
        }
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

    private static void AttachExplicitRightClickFlyout(Control control, Func<MenuFlyout> getFlyout)
    {
        control.AddHandler(InputElement.PointerPressedEvent, (s, e) =>
        {
            var point = e.GetCurrentPoint(control);
            if (point.Properties.IsRightButtonPressed)
            {
                var flyout = getFlyout();
                control.ContextFlyout = flyout;
                flyout.ShowAt(control, showAtPointer: true);
                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private static void AttachGridRightClickFlyout(TableView lineGrid, ReviewSpeechViewModel vm, Func<MenuFlyout> getFlyout)
    {
        lineGrid.AddHandler(InputElement.PointerPressedEvent, (s, e) =>
        {
            var point = e.GetCurrentPoint(lineGrid);
            if (point.Properties.IsRightButtonPressed)
            {
                var pos = e.GetPosition(lineGrid);
                var hitVisual = lineGrid.InputHitTest(pos) as Visual;
                if (TableViewExtras.IsInColumnHeader(hitVisual) || TableViewExtras.IsInScrollBar(hitVisual))
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

    private static void PopulateGridFlyout(MenuFlyout flyout, TableView lineGrid, ReviewSpeechViewModel vm)
    {
        flyout.Items.Clear();

        var selectedRows = GetTargetRows(lineGrid, vm);

        var voiceMenu = new MenuItem
        {
            Header = Se.Language.General.Voice,
            IsEnabled = vm.Voices.Count > 0 && selectedRows.Count > 0,
        };
        foreach (var voice in vm.Voices)
        {
            var targetVoice = voice;
            var voiceItem = new MenuItem
            {
                Header = targetVoice.Name,
            };
            if (selectedRows.Count > 0 && selectedRows.All(r => r.SelectedVoice == targetVoice || string.Equals(r.Voice, targetVoice.Name, StringComparison.OrdinalIgnoreCase)))
            {
                voiceItem.Icon = new Icon { Value = IconNames.Check, FontSize = 12 };
            }
            voiceItem.Click += (_, _) =>
            {
                var rows = GetTargetRows(lineGrid, vm);
                vm.ChangeVoiceForRows(rows, targetVoice);
            };
            voiceMenu.Items.Add(voiceItem);
        }
        flyout.Items.Add(voiceMenu);

        var langMenu = new MenuItem
        {
            Header = Se.Language.General.Language,
            IsEnabled = vm.Languages.Count > 0 && selectedRows.Count > 0,
        };
        foreach (var lang in vm.Languages)
        {
            var targetLang = lang;
            var langItem = new MenuItem
            {
                Header = targetLang.Name,
            };
            if (selectedRows.Count > 0 && selectedRows.All(r => r.SelectedLanguage == targetLang || string.Equals(r.Language, targetLang.Name, StringComparison.OrdinalIgnoreCase) || string.Equals(r.Language, targetLang.Code, StringComparison.OrdinalIgnoreCase)))
            {
                langItem.Icon = new Icon { Value = IconNames.Check, FontSize = 12 };
            }
            langItem.Click += (_, _) =>
            {
                var rows = GetTargetRows(lineGrid, vm);
                vm.ChangeLanguageForRows(rows, targetLang);
            };
            langMenu.Items.Add(langItem);
        }
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
                foreach (var r in selectedRows)
                {
                    r.Include = true;
                }
            };
            flyout.Items.Add(itemCheckSelected);

            var itemUncheckSelected = new MenuItem
            {
                Header = "Uncheck selected lines",
            };
            itemUncheckSelected.Click += (_, _) =>
            {
                foreach (var r in selectedRows)
                {
                    r.Include = false;
                }
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
            foreach (var line in vm.Lines)
            {
                line.Include = true;
            }
        };
        flyout.Items.Add(itemSelectAll);

        var itemSelectNone = new MenuItem
        {
            Header = Se.Language.General.SelectNone,
        };
        itemSelectNone.Click += (_, _) =>
        {
            foreach (var line in vm.Lines)
            {
                line.Include = false;
            }
        };
        flyout.Items.Add(itemSelectNone);

        var itemInvert = new MenuItem
        {
            Header = Se.Language.General.InvertSelection,
        };
        itemInvert.Click += (_, _) =>
        {
            foreach (var line in vm.Lines)
            {
                line.Include = !line.Include;
            }
        };
        flyout.Items.Add(itemInvert);

        var actors = vm.Lines
            .Select(l => l.StepResult?.Paragraph?.Actor ?? l.WaveformParagraph?.Actor ?? string.Empty)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();

        if (actors.Count > 0)
        {
            flyout.Items.Add(new Separator());
            foreach (var actor in actors)
            {
                var targetActor = actor;
                var actorItem = new MenuItem
                {
                    Header = $"Select actor: {targetActor}",
                };
                actorItem.Click += (_, _) =>
                {
                    foreach (var line in vm.Lines)
                    {
                        var a = line.StepResult?.Paragraph?.Actor ?? line.WaveformParagraph?.Actor;
                        line.Include = string.Equals(a, targetActor, StringComparison.OrdinalIgnoreCase);
                    }
                };
                flyout.Items.Add(actorItem);
            }
        }

        flyout.Items.Add(new Separator());
        var itemRegenerate = new MenuItem
        {
            Header = Se.Language.Video.TextToSpeech.RegenerateAudio + " (" + Se.Language.General.SelectedLines + ")",
            Command = vm.RegenerateSelectedLinesCommand,
            IsEnabled = vm.IsRegenerateEnabled && vm.Lines.Any(l => l.Include),
        };
        flyout.Items.Add(itemRegenerate);
    }

    private static void PopulateCheckboxHeaderFlyout(MenuFlyout flyout, ReviewSpeechViewModel vm)
    {
        flyout.Items.Clear();

        var itemSelectAll = new MenuItem
        {
            Header = Se.Language.General.SelectAll,
        };
        itemSelectAll.Click += (_, _) =>
        {
            foreach (var line in vm.Lines)
            {
                line.Include = true;
            }
        };
        flyout.Items.Add(itemSelectAll);

        var itemSelectNone = new MenuItem
        {
            Header = Se.Language.General.SelectNone,
        };
        itemSelectNone.Click += (_, _) =>
        {
            foreach (var line in vm.Lines)
            {
                line.Include = false;
            }
        };
        flyout.Items.Add(itemSelectNone);

        var itemInvert = new MenuItem
        {
            Header = Se.Language.General.InvertSelection,
        };
        itemInvert.Click += (_, _) =>
        {
            foreach (var line in vm.Lines)
            {
                line.Include = !line.Include;
            }
        };
        flyout.Items.Add(itemInvert);

        var actors = vm.Lines
            .Select(l => l.StepResult?.Paragraph?.Actor ?? l.WaveformParagraph?.Actor ?? string.Empty)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();

        if (actors.Count > 0)
        {
            flyout.Items.Add(new Separator());
            foreach (var actor in actors)
            {
                var targetActor = actor;
                var actorItem = new MenuItem
                {
                    Header = $"Select actor: {targetActor}",
                };
                actorItem.Click += (_, _) =>
                {
                    foreach (var line in vm.Lines)
                    {
                        var a = line.StepResult?.Paragraph?.Actor ?? line.WaveformParagraph?.Actor;
                        line.Include = string.Equals(a, targetActor, StringComparison.OrdinalIgnoreCase);
                    }
                };
                flyout.Items.Add(actorItem);
            }
        }
    }
}
