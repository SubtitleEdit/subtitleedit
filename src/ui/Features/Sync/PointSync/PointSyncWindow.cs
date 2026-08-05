using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Collections;

namespace Nikse.SubtitleEdit.Features.Sync.PointSync;

public class PointSyncWindow : Window
{
    public PointSyncWindow(PointSyncViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Sync.PointSync;
        Width = 800;
        Height = 600;
        MinWidth = 600;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var subtitleViewView = MakeSubtitleView(vm);
        var controlView = MakeControlView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindIsEnabled(nameof(vm.IsOkEnabled));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(subtitleViewView, 0);
        grid.Add(controlView, 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Control MakeControlView(PointSyncViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(50, GridUnitType.Pixel) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 60, 0, 0),
        };

        // The DataGrid this replaces hid its header (HeadersVisibility.None); TableView
        // has no such switch, so the single column's header now doubles as the panel title.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.CanUserResizeColumns = false; // single star column, nothing to resize
        // Fixed width: this panel sits in an Auto-sized outer column, and a TableView
        // with a star column measured without a width constraint demands more than the
        // whole window (star columns have no content-based size), squeezing the
        // subtitle grids to slivers and overflowing the right edge.
        dataGrid.Width = 280;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.SyncPoints;
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Sync.SyncPoints,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SyncPoint.Text)),
            Width = new GridLength(1, GridUnitType.Star),
        });

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSyncPoint)));
        TableViewExtras.AttachListNavigation(dataGrid);

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.DeleteSelectedPointSyncCommand,
        };
        var flyout = new MenuFlyout { Items = { menuItemDelete } };
        flyout.Opening += (_, _) => menuItemDelete.IsEnabled = vm.SelectedSyncPoint != null;
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);
        dataGrid.KeyDown += (_, e) =>
        {
            if (e.Key is Key.Delete or Key.Back)
            {
                e.Handled = true;
                vm.DeleteSelectedPointSyncCommand.Execute(null);
            }
        };

        var buttonSetSyncPoint = UiUtil.MakeButton(Se.Language.Sync.SetSyncPoint, vm.SetSyncPointCommand)
            .WithIconLeft(IconNames.ArrowLeftRightBold);

        grid.Add(buttonSetSyncPoint, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 1);

        return grid;
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
                Width = new GridLength(1, GridUnitType.Star),
            },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle)));
        TableViewExtras.AttachListNavigation(dataGrid);

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }

}