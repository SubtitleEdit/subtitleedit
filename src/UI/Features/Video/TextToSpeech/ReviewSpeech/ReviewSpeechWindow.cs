using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

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
        var dataGrid = MakeDataGrid(vm);
        var waveform = MakeWaveform(vm);

        var buttonExport = UiUtil.MakeButton(Se.Language.General.ExportDotDotDot, vm.ExportCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
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
        grid.Add(dataGrid, 0, 1);
        grid.Add(waveform, 1, 0, 1, 2);
        grid.Add(panelButtons, 2, 0, 1, 2);
        grid.Add(checkBoxAutoContinue, 2, 0);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        Loaded += delegate 
        {
            vm.Loaded();
            vm.SelectedEngineChanged(); 
        };
    }

    private static Border MakeDataGrid(ReviewSpeechViewModel vm)
    {
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            Margin = new Thickness(0, 10, 0, 0),
            [!DataGrid.ItemsSourceProperty] = new Binding(nameof(vm.Lines)),
            [!DataGrid.SelectedItemProperty] = new Binding(nameof(vm.SelectedLine)) { Mode = BindingMode.TwoWay },
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Columns =
            {
                //new DataGridTemplateColumn
                //{
                //    Header = Se.Language.General.Enabled,
                //    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                //    CellTemplate = new FuncDataTemplate<ReviewRow>((item, _) =>
                //        new Border
                //        {
                //            Background = Brushes.Transparent, // Prevents highlighting
                //            Padding = new Thickness(4),
                //            Child = new CheckBox
                //            {
                //                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(ReviewRow.Include)),
                //                HorizontalAlignment = HorizontalAlignment.Center
                //            }
                //        }),
                //    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                //},
                new DataGridTemplateColumn
                {
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<ReviewRow>((item, _) =>
                    {
                        var buttonRegenerate = UiUtil.MakeButton(vm.RegenerateAudioCommand, IconNames.Recycle)
                        .WithBindEnabled(nameof(item.IsPlayingEnabled));
                        buttonRegenerate.CommandParameter = item;
                        if (Se.Settings.Appearance.ShowHints)
                        {
                            ToolTip.SetTip(buttonRegenerate, Se.Language.Video.TextToSpeech.RegenerateAudio);
                        }

                        var buttonHistory = UiUtil.MakeButton(vm.ShowHistoryCommand, IconNames.DotsVertical).WithBindEnabled(nameof(ReviewRow.HasHistory));
                        buttonHistory.CommandParameter = item;
                        if (Se.Settings.Appearance.ShowHints)
                        {
                            ToolTip.SetTip(buttonHistory, Se.Language.General.ShowHistory);
                        }

                        var buttonPlay = UiUtil.MakeButton(vm.PlayRowCommand,"fa-solid fa-play")
                        .WithBindIsVisible(nameof(item.IsPlaying), new InverseBooleanConverter())
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
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Binding = new Binding(nameof(ReviewRow.Number)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Voice,
                    Binding = new Binding(nameof(ReviewRow.Voice)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.CharsPerSec,
                    Binding = new Binding(nameof(ReviewRow.Cps)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Speed,
                    Binding = new Binding(nameof(ReviewRow.Speed)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    Binding = new Binding(nameof(ReviewRow.Text)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
            },
        };
        dataGrid.DoubleTapped += (s, e) => vm.DataGridDoubleClicked();
        vm.LineGrid = dataGrid;

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
            textBox.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

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

        grid.Add(dataGrid, 0, 0);
        grid.Add(textBox, 1, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeControls(ReviewSpeechViewModel vm)
    {
        var labelMinWidth = 100;
        var controlMinWidth = 200;

        var comboBoxEngines = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine)).WithMinWidth(controlMinWidth);
        comboBoxEngines.SelectionChanged += vm.SelectedEngineChanged;
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
                UiUtil.MakeButton(vm.ShowElevenLabsEngineV3HelpCommand, IconNames.Help)
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

        var buttonRegenerateAudio = new Button
        {
            Content = Se.Language.Video.TextToSpeech.RegenerateAudioSelectedLine,
            Command = vm.RegenerateAudioCommand,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5),
        }.WithIconLeft(IconNames.Recycle);

        //var buttonPlay = new Button
        //{
        //    Content = Se.Language.General.PlaySelectedLine,
        //    Command = vm.PlayCommand,
        //    HorizontalAlignment = HorizontalAlignment.Stretch,
        //    Width = double.NaN,
        //    VerticalAlignment = VerticalAlignment.Center,
        //}.WithIconLeft(IconNames.PlayCircle).WithBindIsVisible(nameof(vm.IsPlayVisible));

        var buttonStop = new Button
        {
            Content = Se.Language.General.Stop,
            Command = vm.StopCommand,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            VerticalAlignment = VerticalAlignment.Center,
        }.WithIconLeft(IconNames.StopCircle).WithBindIsVisible(nameof(vm.IsStopVisible));

        //var checkBoxAutoContinue = new CheckBox
        //{
        //    Content = Se.Language.General.AutoContinue,
        //    [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.AutoContinue)) { Mode = BindingMode.TwoWay },
        //};

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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // filler
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
        grid.Add(panelVoice, 1, 0);
        grid.Add(panelModel, 2, 0);
        grid.Add(panelRegion, 3, 0);
        grid.Add(panelLanguage, 4, 0);
        grid.Add(elevenLabsControls, 5, 0);
        // 6 is filler
        //grid.Add(buttonRegenerateAudio, 7, 0);
        //grid.Add(buttonPlay, 8, 0);
        // grid.Add(buttonStop, 8, 0);
        //  grid.Add(checkBoxAutoContinue, 9, 0);

        return UiUtil.MakeBorderForControl(grid);
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
        var buttonStability = UiUtil.MakeButton(vm.ShowStabilityHelpCommand, IconNames.Help);

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
        var buttonSimilarity = UiUtil.MakeButton(vm.ShowSimilarityHelpCommand, IconNames.Help);

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
        var buttonSpeakerBoost = UiUtil.MakeButton(vm.ShowSpeakerBoostHelpCommand, IconNames.Help);

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
        var buttonSpeed = UiUtil.MakeButton(vm.ShowSpeedHelpCommand, IconNames.Help);

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
        var buttonStyleExaggeration = UiUtil.MakeButton(vm.ShowStyleExaggerationHelpCommand, IconNames.Help);

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
        return new Border
        {
            Margin = new Thickness(2),
            //          Child = new TextBlock { Text = "Waveform placeholder" } // Placeholder for waveform control
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
}
