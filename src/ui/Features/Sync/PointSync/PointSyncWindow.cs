using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Sync.PointSync;

public class PointSyncWindow : Window
{
    public PointSyncWindow(PointSyncViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Sync.PointSync);
        Width = 900;
        Height = 600;
        MinWidth = 700;
        MinHeight = 480;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        DataContext = vm;

        var subtitleView = PointSyncChrome.MakeListPane(
            PointSyncChrome.MakeHeaderRow(
                PointSyncChrome.MakeHeaderWithCount(Se.Language.General.Lines, $"{nameof(vm.Subtitles)}.Count"),
                null),
            MakeSubtitleView(vm));
        var controlView = MakeControlView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindIsEnabled(nameof(vm.IsOkEnabled));
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = EditorChrome.MakeButtonBar(MakeFileInfo(vm), buttonCancel, buttonOk);

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        content.Add(subtitleView, 0, 0);
        content.Add(controlView, 0, 1);

        // The lists give up height first, so the button bar stays reachable on a short screen.
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Add(content, 0, 0);
        grid.Add(panelButtons, 1, 0);

        Content = grid;

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// The subtitle being synced, on the left of the button bar.
    /// </summary>
    private static Control MakeFileInfo(PointSyncViewModel vm)
    {
        var label = PointSyncChrome.MakeFileNameLabel(vm, nameof(vm.FileName));
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        grid.Add(PointSyncChrome.MakeFileIcon(), 0, 0);
        grid.Add(label, 0, 1);
        return grid;
    }

    private static Control MakeControlView(PointSyncViewModel vm)
    {
        var syncPointList = PointSyncChrome.MakeSyncPointList(vm, vm.SyncPoints, nameof(vm.SelectedSyncPoint),
            vm.DeleteSelectedPointSyncCommand, () => vm.SelectedSyncPoint != null);

        var buttonSetSyncPoint = PointSyncChrome.MakeWideAction(IconNames.ArrowLeftRightBold, Se.Language.Sync.SetSyncPoint, vm.SetSyncPointCommand);

        return PointSyncChrome.MakeSidePanel(syncPointList, $"{nameof(vm.SyncPoints)}.Count", new Thickness(1, 0, 0, 0), buttonSetSyncPoint);
    }

    private static Border MakeSubtitleView(PointSyncViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        // No header-click sorting (the DataGrid's CanUserSortColumns is not carried
        // over): the lines are shown in timeline order, which the sync-point logic
        // relies on.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Subtitles;
        dataGrid.Columns.AddRange(new[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.NumberSymbol,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                Width = new GridLength(60), // content-sized (Auto) on the DataGrid; TableView treats Auto as star
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Show,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
                Width = new GridLength(115),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Duration,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
                Width = new GridLength(90),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Text,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.Text)),
                NameBinding = new Binding(nameof(SubtitleLineViewModel.Text)),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle)));
        dataGrid.WithAccessibleName(Se.Language.General.Lines);
        TableViewExtras.AttachListNavigation(dataGrid);

        // Bring the initially selected line (the main window's selection) into view.
        dataGrid.Loaded += (_, _) =>
        {
            if (vm.SelectedSubtitle is { } selected)
            {
                dataGrid.ScrollIntoView(selected);
                TableViewExtras.CenterRow(dataGrid, selected);
            }
        };

        return EditorChrome.MakeListFrame(dataGrid);
    }
}