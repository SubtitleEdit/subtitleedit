using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.CutVideo;

public class CutVideoWindow : Window
{
    public CutVideoWindow(CutVideoViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.CutVideoTitle;
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 800;
        MinHeight = 600;
        vm.Window = this;
        DataContext = vm;

        var segmentsView = MakeSegmentsView(vm);
        var videoPlayerView = MakeVideoPlayerView(vm);
        var audioVisualizerView = MakeAudioVisualizerView(vm);
        var progressView = MakeProgressView(vm);

        var comboBoxCutType = UiUtil.MakeComboBox<CutTypeDisplay>(
            vm.CutTypes,
            vm,
            nameof(vm.SelectedCutType)
        ).WithMarginRight(10);


        var labelVideoExtension = UiUtil.MakeLabel(Se.Language.General.VideoExtension);

        var comboBoxVideoExtension = UiUtil.MakeComboBox(
            vm.VideoExtensions,
            vm,
            nameof(vm.SelectedVideoExtension)
        ).WithMarginRight(10);

        var checkBoxCutSubtitle = UiUtil.MakeCheckBox(Se.Language.Video.CutVideoAlsoCutSubtitle, vm, nameof(vm.CutSubtitleToo))
            .WithMarginRight(10);
        checkBoxCutSubtitle[!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsCutSubtitleVisible));

        var buttonGenerate = UiUtil.MakeButton(Se.Language.General.Generate, vm.GenerateCommand)
            .WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var buttonConfig = UiUtil.MakeButton(vm.OkCommand, IconNames.Settings, Se.Language.General.Settings)
            .WithMarginRight(5)
            .WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var buttonPanel = UiUtil.MakeButtonBar(
            checkBoxCutSubtitle,
            comboBoxCutType,
            labelVideoExtension,
            comboBoxVideoExtension,
            buttonGenerate,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // segments and video player
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // audio visualizer
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // progress bar
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // segments
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // video player
            },
            Margin = UiUtil.MakeWindowMargin(),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 5,
            RowSpacing = 5,
        };

        grid.Add(segmentsView, 0, 0);
        grid.Add(videoPlayerView, 0, 1);
        grid.Add(audioVisualizerView, 1, 0, 1, 2);
        grid.Add(progressView, 2, 0, 1, 2);
        grid.Add(buttonPanel, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate
        {
            // initial focus on an input, not an action button - a focused button clicks on bare Space
            if (vm.SegmentGrid != null)
            {
                TableViewExtras.FocusRow(vm.SegmentGrid);
            }
        };
        Loaded += (s, e) => vm.OnLoaded();
        Closing += (s, e) => vm.OnClosing();
        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        AddHandler(KeyUpEvent, vm.OnKeyUpHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private static Border MakeSegmentsView(CutVideoViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        // No header sorting: the segment list is kept in time order (segments are
        // inserted in position and feed the cut output), so reordering it would be wrong.
        var dataGridSubtitle = TableViewExtras.MakeTableView();
        dataGridSubtitle.Width = double.NaN;
        dataGridSubtitle.Height = double.NaN;
        dataGridSubtitle.DataContext = vm;
        dataGridSubtitle.ItemsSource = vm.Segments;
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
            Width = new GridLength(60), // was content-sized (Auto) on the DataGrid
        });
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
            Width = new GridLength(120), // was content-sized (Auto) on the DataGrid
        });
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Hide,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter },
            Width = new GridLength(120), // was content-sized (Auto) on the DataGrid
        });
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGridSubtitle.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSegment)) { Source = vm });
        dataGridSubtitle.SelectionChanged += vm.SegmentsGridChanged;
        dataGridSubtitle.DoubleTapped += vm.SegmentsGridDoubleTapped;
        vm.SegmentGrid = dataGridSubtitle;

        var buttonAdd = UiUtil.MakeButton(Se.Language.General.Add, vm.AddCommand);
        var buttonSetStart = UiUtil.MakeButton(Se.Language.General.SetStart, vm.SetStartCommand).WithBindIsEnabled(nameof(vm.IsSetStartEnabled));
        var buttonSetEnd = UiUtil.MakeButton(Se.Language.General.SetEnd, vm.SetEndCommand).WithBindIsEnabled(nameof(vm.IsSetEndEnabled));
        var buttonDelete = UiUtil.MakeButton(Se.Language.General.Delete, vm.DeleteCommand).WithBindIsEnabled(nameof(vm.IsDeleteEnabled));
        var buttonImport = new SplitButton
        {
            Content = Se.Language.General.ImportDotDotDot,
            Command = vm.ImportCommand,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.Video.ImportCurrentSubtitle,
                        Command = vm.ImportCurrentCommand,
                    },
                }
            }
        };

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                buttonAdd,
                buttonSetStart,
                buttonSetEnd,
                buttonDelete,
                buttonImport,
            },
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // segments
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 5,
            RowSpacing = 5,
        };

        grid.Add(dataGridSubtitle, 0, 0);
        grid.Add(panelButtons, 1, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeVideoPlayerView(CutVideoViewModel vm)
    {
        vm.VideoPlayer = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayer.FullScreenIsVisible = false;
        return UiUtil.MakeBorderForControl(vm.VideoPlayer);
    }

    private static Border MakeAudioVisualizerView(CutVideoViewModel vm)
    {
        vm.AudioVisualizer = new AudioVisualizer { Height = 80, Width = double.NaN, IsReadOnly = false };
        vm.AudioVisualizer.OnVideoPositionChanged += vm.AudioVisualizerPositionChanged;
        vm.AudioVisualizer.OnNewSelectionInsert += vm.AudioVisualizerOnNewSelectionInsert;
        vm.AudioVisualizer.DrawGridLines = Se.Settings.Waveform.DrawGridLines;
        vm.AudioVisualizer.WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor();
        vm.AudioVisualizer.WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor();
        vm.AudioVisualizer.OnSelectRequested += vm.AudioVisualizerSelectRequested;
        vm.AudioVisualizer.OnPrimarySingleClicked += vm.AudioVisualizerOnPrimarySingleClicked;
        vm.AudioVisualizer.OnPrimaryDoubleClicked += vm.AudioVisualizerOnPrimaryDoubleClicked;

        return UiUtil.MakeBorderForControl(vm.AudioVisualizer);
    }

    private static Grid MakeProgressView(CutVideoViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

        var statusText = new TextBlock
        {
            Margin = new Thickness(5, 20, 0, 0),
        };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));
        statusText.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(progressBar, 0, 0);
        grid.Add(statusText, 0, 0);

        return grid;
    }
}
