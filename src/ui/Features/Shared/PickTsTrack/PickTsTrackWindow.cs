using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.PickTsTrack;

public class PickTsTrackWindow : Window
{
    public PickTsTrackWindow(PickTsTrackViewModel vm)
    {
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

        Loaded += (_, _) => vm.SelectAndScrollToRow(0);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private Border MakeTracksView(PickTsTrackViewModel vm)
    {
        var dataGridTracks = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridTracks.Width = double.NaN;
        dataGridTracks.Height = double.NaN;
        dataGridTracks.DataContext = vm;
        dataGridTracks.ItemsSource = vm.Tracks;

        // Content-sized (Auto) on the DataGrid; TableView treats Auto as star, so the
        // narrow columns get fixed widths and Name becomes the star column.
        var numberColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(TsTrackInfoDisplay.TrackNumber)),
            Width = new GridLength(60),
        };
        var nameColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(TsTrackInfoDisplay.Name)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        var languageColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Language,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(TsTrackInfoDisplay.Language)),
            Width = new GridLength(100),
        };
        var codecColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Codec,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(TsTrackInfoDisplay.Codec)),
            Width = new GridLength(140),
        };
        var defaultColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Default,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(TsTrackInfoDisplay.IsDefault)),
            Width = new GridLength(80),
        };
        var forcedColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Forced,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(TsTrackInfoDisplay.IsForced)),
            Width = new GridLength(80),
        };

        dataGridTracks.Columns.Add(numberColumn);
        dataGridTracks.Columns.Add(nameColumn);
        dataGridTracks.Columns.Add(languageColumn);
        dataGridTracks.Columns.Add(codecColumn);
        dataGridTracks.Columns.Add(defaultColumn);
        dataGridTracks.Columns.Add(forcedColumn);

        dataGridTracks.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedTrack)));
        dataGridTracks.SelectionChanged += vm.TracksGridSelectionChanged;
        dataGridTracks.DoubleTapped += (s, e) => vm.OkCommand.Execute(null);
        vm.TracksGrid = dataGridTracks;

        // Track order is presentation-only (OK uses the selected item), so the
        // in-place header sorter is safe.
        new TableViewHeaderSorter(dataGridTracks)
            .AddSortable<TsTrackInfoDisplay, int>(numberColumn, x => x.TrackNumber)
            .AddSortable<TsTrackInfoDisplay, string>(nameColumn, x => x.Name)
            .AddSortable<TsTrackInfoDisplay, string>(languageColumn, x => x.Language)
            .AddSortable<TsTrackInfoDisplay, string>(codecColumn, x => x.Codec)
            .AddSortable<TsTrackInfoDisplay, bool>(defaultColumn, x => x.IsDefault)
            .AddSortable<TsTrackInfoDisplay, bool>(forcedColumn, x => x.IsForced);

        return UiUtil.MakeBorderForControlNoPadding(dataGridTracks);
    }

    private static Border MakeSubtitleView(PickTsTrackViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var dataGridSubtitle = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridSubtitle.Width = double.NaN;
        dataGridSubtitle.Height = double.NaN;
        dataGridSubtitle.DataContext = vm;
        dataGridSubtitle.ItemsSource = vm.Rows;

        // No sorter here: the preview shows subtitle cues in subtitle order.
        dataGridSubtitle.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(TsSubtitleCueDisplay.Number)),
                    Width = new GridLength(60),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(TsSubtitleCueDisplay.Show)) { Converter = fullTimeConverter },
                    Width = new GridLength(120),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Duration,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(TsSubtitleCueDisplay.Duration)) { Converter = shortTimeConverter },
                    Width = new GridLength(90),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.TextOrImage,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Width = new GridLength(1, GridUnitType.Star),
                    CellTemplate = new FuncDataTemplate<TsSubtitleCueDisplay>((item, _) =>
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
}