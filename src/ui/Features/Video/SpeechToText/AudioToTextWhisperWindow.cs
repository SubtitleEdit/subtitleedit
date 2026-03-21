using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public class AudioToTextWhisperWindow : Window
{
    public AudioToTextWhisperWindow(AudioToTextWhisperViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.AudioToText.Title;
        Width = 1100;
        Height = 660;
        MinWidth = 800;
        MinHeight = 500;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var labelConsoleLog = new TextBlock
        {
            Text = Se.Language.General.ConsoleLog,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(10, 12, 10, 10),
        };

        var consoleLogAndBatchView = MakeConsoleLogAndBatchView(vm);
        var consoleLogOnlyView = MakeConsoleLogOnlyView(vm);

        var labelEngine = UiUtil.MakeTextBlock(Se.Language.General.Engine).WithMarginTop(10);
        var comboEngine = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine))
            .WithMinWidth(220)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithMarginTop(10);

        comboEngine.SelectionChanged += vm.OnEngineChanged;

        var labelLanguage = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.InputLanguage).WithMarginTop(10);
        var comboLanguage = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage))
            .WithMinWidth(220)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .WithMarginTop(10);

        var labelModel = UiUtil.MakeTextBlock(Se.Language.General.Model).WithMarginBottom(20).WithMarginTop(10);
        var comboModel = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel))
            .WithMinWidth(220)
            .WithMarginBottom(20)
            .WithMarginTop(10)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));

        var buttonModelDownload = UiUtil.MakeButtonBrowse(vm.DownloadModelCommand)
            .WithMarginBottom(20)
            .WithMarginTop(10)
            .WithMarginLeft(5)
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));

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
            .BindIsVisible(vm, nameof(vm.IsTranslateVisible));
        var checkTranslateToEnglish = UiUtil.MakeCheckBox(vm, nameof(vm.DoTranslateToEnglish))
            .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
            .BindIsVisible(vm, nameof(vm.IsTranslateVisible));

        var labelPostProcessing = UiUtil.MakeTextBlock(Se.Language.General.PostProcessing).WithMarginTop(15);
        var checkPostProcessing = UiUtil.MakeCheckBox(vm, nameof(vm.DoPostProcessing)).BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));
        var buttonPostProcessing = UiUtil.MakeButton(vm.ShowPostProcessingSettingsCommand, IconNames.Settings)
                    .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled));

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

        var labelAdvancedSettings = UiUtil.MakeTextBlock(Se.Language.General.AdvancedSettings).WithMarginTop(15);

        var buttonAdvancedSettings = UiUtil.MakeButton(vm.ShowAdvancedSettingsCommand, IconNames.Settings)
                    .BindIsEnabled(vm, nameof(vm.IsTranscribeEnabled))
                    .WithMarginTop(15);

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
        };
        textBoxAdvancedSettings.Bind(TextBox.TextProperty, new Binding
        {
            Path = nameof(vm.Parameters),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        var progressSlider = new Slider()
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            Margin = new Thickness(10, 0, 0, 0),
            Width = double.NaN,
            Height = 10,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(Thumb.IsVisibleProperty, false)
                    },
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 8.0)
                    },
                },
            },
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding
        {
            Path = nameof(vm.ProgressValue),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });
        progressSlider.Bind(Slider.OpacityProperty, new Binding
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
            Children =
            {
                progressSlider,
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

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Engine
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Language
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Model
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Translate to English
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Post processing
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Advanced settings
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Console log
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // OK/Cancel
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
        ContextFlyout = flyout;

        var menuItemViewWhisperLog = new MenuItem
        {
            Header = Se.Language.Video.AudioToText.ViewWhisperLogFile,
            DataContext = vm,
            Command = vm.ViewWhisperLogFileCommand,
        };
        flyout.Items.Add(menuItemViewWhisperLog);

        var menuItemReDownloadWhisperEngine = new MenuItem
        {
            DataContext = vm,
            Command = vm.ReDownloadWhisperEngineCommand,
        };
        menuItemReDownloadWhisperEngine.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsReDownloadVisible)) { Source = vm });
        menuItemReDownloadWhisperEngine.Bind(MenuItem.HeaderProperty, new Binding(nameof(vm.ReDownloadText)) { Source = vm });
        flyout.Items.Add(menuItemReDownloadWhisperEngine);


        var row = 0;

        grid.Children.Add(labelConsoleLog);
        Grid.SetRow(labelConsoleLog, row);
        Grid.SetColumn(labelConsoleLog, 2);
        Grid.SetRowSpan(labelConsoleLog, 2);
        row++;

        grid.Children.Add(consoleLogAndBatchView);
        Grid.SetRow(consoleLogAndBatchView, row);
        Grid.SetColumn(consoleLogAndBatchView, 2);
        Grid.SetRowSpan(consoleLogAndBatchView, 8);
        grid.Children.Add(consoleLogOnlyView);
        Grid.SetRow(consoleLogOnlyView, row);
        Grid.SetColumn(consoleLogOnlyView, 2);
        Grid.SetRowSpan(consoleLogOnlyView, 8);
        row++;

        grid.Children.Add(labelEngine);
        Grid.SetRow(labelEngine, row);
        Grid.SetColumn(labelEngine, 0);

        grid.Children.Add(comboEngine);
        Grid.SetRow(comboEngine, row);
        Grid.SetColumn(comboEngine, 1);
        row++;

        grid.Children.Add(labelLanguage);
        Grid.SetRow(labelLanguage, row);
        Grid.SetColumn(labelLanguage, 0);

        grid.Children.Add(comboLanguage);
        Grid.SetRow(comboLanguage, row);
        Grid.SetColumn(comboLanguage, 1);
        row++;

        grid.Children.Add(labelModel);
        Grid.SetRow(labelModel, row);
        Grid.SetColumn(labelModel, 0);

        grid.Children.Add(panelModelControls);
        Grid.SetRow(panelModelControls, row);
        Grid.SetColumn(panelModelControls, 1);
        row++;

        grid.Children.Add(labelTranslateToEnglish);
        Grid.SetRow(labelTranslateToEnglish, row);
        Grid.SetColumn(labelTranslateToEnglish, 0);

        grid.Children.Add(checkTranslateToEnglish);
        Grid.SetRow(checkTranslateToEnglish, row);
        Grid.SetColumn(checkTranslateToEnglish, 1);
        row++;

        grid.Children.Add(labelPostProcessing);
        Grid.SetRow(labelPostProcessing, row);
        Grid.SetColumn(labelPostProcessing, 0);

        grid.Children.Add(panelPostProcessingControls);
        Grid.SetRow(panelPostProcessingControls, row);
        Grid.SetColumn(panelPostProcessingControls, 1);
        row++;

        grid.Children.Add(labelAdvancedSettings);
        Grid.SetRow(labelAdvancedSettings, row);
        Grid.SetColumn(labelAdvancedSettings, 0);

        grid.Children.Add(buttonAdvancedSettings);
        Grid.SetRow(buttonAdvancedSettings, row);
        Grid.SetColumn(buttonAdvancedSettings, 1);
        row++;

        grid.Children.Add(textBoxAdvancedSettings);
        Grid.SetRow(textBoxAdvancedSettings, row);
        Grid.SetColumn(textBoxAdvancedSettings, 0);
        Grid.SetColumnSpan(textBoxAdvancedSettings, 2);
        row++;

        grid.Children.Add(panelProgress);
        Grid.SetRow(panelProgress, row);
        Grid.SetColumn(panelProgress, 0);
        Grid.SetColumnSpan(panelProgress, 3);
        Grid.SetRowSpan(panelProgress, 2);

        row++;
        grid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, row);
        Grid.SetColumn(buttonPanel, 0);
        Grid.SetColumnSpan(buttonPanel, 3);

        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
        Loaded += (s, e) => vm.OnWindowLoaded();
        Closing += (s, e) => vm.OnWindowClosing(e);
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static Grid MakeConsoleLogAndBatchView(AudioToTextWhisperViewModel vm)
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
        textBoxConsoleLog.Bind(TextBox.TextProperty, new Binding
        {
            Path = nameof(vm.ConsoleLog),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        vm.TextBoxConsoleLog = textBoxConsoleLog;


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
                    Binding = new Binding(nameof(WhisperJobItem.InputVideoFileNameShort)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Size,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(WhisperJobItem.SizeDisplay)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Status,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(WhisperJobItem.Status)),
                    IsReadOnly = true,
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedBatchItem)) { Source = vm });
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

    private static TextBox MakeConsoleLogOnlyView(AudioToTextWhisperViewModel vm)
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
        textBoxConsoleLog.Bind(TextBox.TextProperty, new Binding
        {
            Path = nameof(vm.ConsoleLog),
            Mode = BindingMode.OneWay,
            Source = vm,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        textBoxConsoleLog.WithBindIsVisible(nameof(vm.IsBatchMode), new InverseBooleanConverter());
        vm.TextBoxConsoleLog = textBoxConsoleLog;

        return textBoxConsoleLog;
    }
}
