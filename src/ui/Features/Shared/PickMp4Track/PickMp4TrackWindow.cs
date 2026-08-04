using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.PickMp4Track;

public class PickMp4TrackWindow : Window
{
    private readonly PickMp4TrackViewModel _vm;

    public PickMp4TrackWindow(PickMp4TrackViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.WindowTitle;
        Width = 1024;
        Height = 600;
        MinWidth = 800;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var tracksView = MakeTracksView(vm);
        var subtitleView = MakeSubtitleView(vm);

        var buttonExport = UiUtil.MakeButton(Se.Language.General.ExportDotDotDot, vm.ExportCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonExport, buttonOk, buttonCancel);

        var labelSubtitleCount = UiUtil.MakeLabel(new Binding(nameof(vm.SubtitleCountText)));

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(tracksView, 0, 0);
        grid.Add(subtitleView, 0, 1);
        grid.Add(labelSubtitleCount, 1, 1);
        grid.Add(panelButtons, 2, 0, 1, 2);


        Content = grid;

        Activated += delegate
        {
            buttonOk.Focus(); // hack to make OnKeyDown work
        };
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.SelectAndScrollToRow(0);
    }

    private Border MakeTracksView(PickMp4TrackViewModel vm)
    {
        var dataGridTracks = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridTracks.Width = double.NaN;
        dataGridTracks.Height = double.NaN;
        dataGridTracks.DataContext = _vm;
        dataGridTracks.ItemsSource = _vm.Tracks;

        // Content-sized (Auto) on the DataGrid; TableView treats Auto as star, so the
        // narrow columns get fixed widths and Name becomes the star column.
        var handlerColumn = new SeTableViewColumn
        {
            Header = "HandlerName",
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(Mp4TrackInfoDisplay.HandlerType)),
            Width = new GridLength(120),
        };
        var nameColumn = new SeTableViewColumn
        {
            Header = "Name",
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(Mp4TrackInfoDisplay.Name)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        var durationColumn = new SeTableViewColumn
        {
            Header = "Duration",
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(Mp4TrackInfoDisplay.Duration)) { Converter = new TimeSpanToDisplayFullConverter() },
            Width = new GridLength(100),
        };
        var vobSubColumn = new SeTableViewColumn
        {
            Header = "IsVobSubSubtitle",
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(Mp4TrackInfoDisplay.IsVobSubSubtitle)),
            Width = new GridLength(130),
        };
        var startPositionColumn = new SeTableViewColumn
        {
            Header = "StartPosition",
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(Mp4TrackInfoDisplay.StartPosition)),
            Width = new GridLength(110),
        };

        dataGridTracks.Columns.Add(handlerColumn);
        dataGridTracks.Columns.Add(nameColumn);
        dataGridTracks.Columns.Add(durationColumn);
        dataGridTracks.Columns.Add(vobSubColumn);
        dataGridTracks.Columns.Add(startPositionColumn);

        dataGridTracks.Bind(TableView.SelectedItemProperty, new Binding(nameof(_vm.SelectedTrack)));
        dataGridTracks.SelectionChanged += vm.TracksGridSelectionChanged;
        vm.TracksGrid = dataGridTracks;

        // Track order is presentation-only (OK uses the selected item), so the
        // in-place header sorter is safe.
        new TableViewHeaderSorter(dataGridTracks)
            .AddSortable<Mp4TrackInfoDisplay, string>(handlerColumn, x => x.HandlerType)
            .AddSortable<Mp4TrackInfoDisplay, string>(nameColumn, x => x.Name)
            .AddSortable<Mp4TrackInfoDisplay, TimeSpan>(durationColumn, x => x.Duration)
            .AddSortable<Mp4TrackInfoDisplay, bool>(vobSubColumn, x => x.IsVobSubSubtitle)
            .AddSortable<Mp4TrackInfoDisplay, ulong>(startPositionColumn, x => x.StartPosition);

        return UiUtil.MakeBorderForControlNoPadding(dataGridTracks);
    }

    private Border MakeSubtitleView(PickMp4TrackViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var dataGridSubtitle = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridSubtitle.Width = double.NaN;
        dataGridSubtitle.Height = double.NaN;
        dataGridSubtitle.DataContext = _vm;
        dataGridSubtitle.ItemsSource = _vm.Rows;

        // No sorter here: the preview shows subtitle cues in subtitle order.
        dataGridSubtitle.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = "#",
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(Mp4SubtitleCueDisplay.Number)),
                    Width = new GridLength(60),
                },
                new SeTableViewColumn
                {
                    Header = "Show",
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(Mp4SubtitleCueDisplay.Show)) { Converter = fullTimeConverter },
                    Width = new GridLength(120),
                },
                new SeTableViewColumn
                {
                    Header = "Duration",
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(Mp4SubtitleCueDisplay.Duration)) { Converter = shortTimeConverter },
                    Width = new GridLength(90),
                },
                new SeTableViewColumn
                {
                    Header = "Text/Image",
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Width = new GridLength(1, GridUnitType.Star),
                    CellTemplate = new FuncDataTemplate<Mp4SubtitleCueDisplay>((item, _) =>
                    {
                        var stackPanel = new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            Spacing = 5
                        };

                        // Add text if available
                        if (!string.IsNullOrEmpty(item.Text))
                        {
                            var textBlock = new TextBlock
                            {
                                Text = item.Text,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                MaxWidth = 300 // Adjust as needed
                            };
                            stackPanel.Children.Add(textBlock);
                        }

                        // Add image if available
                        if (item.Image != null)
                        {
                            var image = new Image
                            {
                                Source = item.Image.Source,
                                MaxHeight = 100, // Adjust as needed
                                MaxWidth = 200,  // Adjust as needed
                                Stretch = Avalonia.Media.Stretch.Uniform
                            };
                            stackPanel.Children.Add(image);
                        }

                        return stackPanel;
                    })
                },
        });

        return UiUtil.MakeBorderForControlNoPadding(dataGridSubtitle);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}