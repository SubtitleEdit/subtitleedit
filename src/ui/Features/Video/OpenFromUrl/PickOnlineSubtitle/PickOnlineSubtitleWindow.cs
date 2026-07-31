using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl.PickOnlineSubtitle;

public class PickOnlineSubtitleWindow : Window
{
    public PickOnlineSubtitleWindow(PickOnlineSubtitleViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.PickOnlineSubtitleTitle;
        Width = 1024;
        Height = 600;
        MinWidth = 800;
        MinHeight = 500;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var tracksView = MakeTracksView(vm);
        var previewView = MakePreviewView(vm);

        var statusBar = MakeStatusBar(vm);

        var buttonSave = UiUtil.MakeButton(Se.Language.General.SaveDotDotDot, vm.SaveCommand);
        buttonSave.Bind(IsEnabledProperty, new Binding(nameof(vm.IsOkEnabled)));
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Bind(IsEnabledProperty, new Binding(nameof(vm.IsOkEnabled)));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonSave, buttonOk, buttonCancel);

        var splitGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
        };
        splitGrid.Add(tracksView, 0, 0);
        splitGrid.Add(previewView, 0, 1);

        var outer = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 8,
        };
        outer.Add(splitGrid, 0);
        outer.Add(statusBar, 1);
        outer.Add(panelButtons, 2);

        Content = outer;

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
    }

    private static Border MakeTracksView(PickOnlineSubtitleViewModel vm)
    {
        var dataGridTracks = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridTracks.Width = double.NaN;
        dataGridTracks.Height = double.NaN;
        dataGridTracks.DataContext = vm;
        dataGridTracks.ItemsSource = vm.Tracks;

        var columnLanguage = new SeTableViewColumn
        {
            Header = Se.Language.General.Language,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(OnlineSubtitleTrackDisplay.Language)),
            Width = new GridLength(1.6, GridUnitType.Star),
        };
        var columnName = new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(OnlineSubtitleTrackDisplay.Name)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        var columnFormat = new SeTableViewColumn
        {
            Header = Se.Language.General.Format,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(OnlineSubtitleTrackDisplay.Format)),
            Width = new GridLength(0.6, GridUnitType.Star),
        };
        dataGridTracks.Columns.Add(columnLanguage);
        dataGridTracks.Columns.Add(columnName);
        dataGridTracks.Columns.Add(columnFormat);

        // Header sorting is safe here: the list is a pick list of search results whose
        // order is presentation-only - the chosen subtitle is consumed via SelectedTrack,
        // never via the collection's order or indexes.
        var sorter = new TableViewHeaderSorter(dataGridTracks);
        sorter.AddSortable<OnlineSubtitleTrackDisplay, string>(columnLanguage, x => x.Language)
              .AddSortable<OnlineSubtitleTrackDisplay, string>(columnName, x => x.Name)
              .AddSortable<OnlineSubtitleTrackDisplay, string>(columnFormat, x => x.Format);

        dataGridTracks.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedTrack)) { Mode = BindingMode.TwoWay });
        dataGridTracks.DoubleTapped += (_, e) =>
        {
            // A double click on a column header (sorting) must not count as "pick".
            if (vm.IsOkEnabled && !TableViewExtras.IsInColumnHeader(e.Source as Avalonia.Visual))
            {
                vm.OkCommand.Execute(null);
            }
        };
        vm.TracksGrid = dataGridTracks;

        return UiUtil.MakeBorderForControlNoPadding(dataGridTracks);
    }

    private static Border MakePreviewView(PickOnlineSubtitleViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        // No header sorting: this is a read-only preview of the subtitle's cues in
        // subtitle order.
        var dataGridPreview = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridPreview.Width = double.NaN;
        dataGridPreview.Height = double.NaN;
        dataGridPreview.DataContext = vm;
        dataGridPreview.ItemsSource = vm.PreviewRows;
        dataGridPreview.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(OnlineSubtitleCueDisplay.Number)),
            Width = new GridLength(60), // was content-sized (Auto) on the DataGrid
        });
        dataGridPreview.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(OnlineSubtitleCueDisplay.Show)) { Converter = fullTimeConverter },
            Width = new GridLength(120), // was content-sized (Auto) on the DataGrid
        });
        dataGridPreview.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(OnlineSubtitleCueDisplay.Duration)) { Converter = shortTimeConverter },
            Width = new GridLength(90), // was content-sized (Auto) on the DataGrid
        });
        dataGridPreview.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(OnlineSubtitleCueDisplay.Text)),
            Width = new GridLength(1, GridUnitType.Star),
        });

        return UiUtil.MakeBorderForControlNoPadding(dataGridPreview);
    }

    private static Border MakeStatusBar(PickOnlineSubtitleViewModel vm)
    {
        var statusText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Opacity = 0.85,
        };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusText)));

        return new Border
        {
            Child = statusText,
            Padding = new Thickness(4, 2),
        };
    }
}
