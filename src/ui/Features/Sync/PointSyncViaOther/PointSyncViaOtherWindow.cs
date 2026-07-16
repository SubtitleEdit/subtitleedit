using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;

public class PointSyncViaOtherWindow : Window
{
    public PointSyncViaOtherWindow(PointSyncViaOtherViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Sync.PointSyncViaOther;
        Width = 1100;
        Height = 600;
        MinWidth = 800;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var subtitleViewView = MakeSubtitleView(vm);
        var controlView = MakeControlView(vm);
        var subtitleOtherView = MakeSubtitleOtherView(vm);

        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand).WithBindIsEnabled(nameof(vm.IsOkEnabled));
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindIsEnabled(nameof(vm.IsOkEnabled));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonApply, buttonOk, buttonCancel);

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(subtitleViewView, 0);
        grid.Add(controlView, 0, 1);
        grid.Add(subtitleOtherView, 0, 2);
        grid.Add(panelButtons, 1, 0, 1, 3);

        Content = grid;

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Control MakeControlView(PointSyncViaOtherViewModel vm)
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

        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            DataContext = vm,
            ItemsSource = vm.SyncPoints,
            HeadersVisibility = DataGridHeadersVisibility.None,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.Sync.SyncPoints,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SyncPoint.Text)),
                    IsReadOnly = true,
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedSyncPoint)));

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

    private static Grid MakeSubtitleView(PointSyncViaOtherViewModel vm)
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
        };

        var labelFileName = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.FileName));
        labelFileName.VerticalAlignment = VerticalAlignment.Center;
        var buttonFindText = UiUtil.MakeButton(Se.Language.Sync.FindText, vm.FindTextLeftCommand);
        buttonFindText.HorizontalAlignment = HorizontalAlignment.Right;
        var panelHeader = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        panelHeader.Add(labelFileName, 0, 0);
        panelHeader.Add(buttonFindText, 0, 1);

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
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
            ItemsSource = vm.Subtitles,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Text)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle)));

        grid.Add(panelHeader, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 1);

        return grid;
    }

    private static Grid MakeSubtitleOtherView(PointSyncViaOtherViewModel vm)
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
        };

        var buttonBrowseOther = UiUtil.MakeButtonBrowse(vm.BrowseOtherCommand);
        var labelOtherFileName = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.FileNameOther));
        labelOtherFileName.VerticalAlignment = VerticalAlignment.Center;
        var panelOtherBrowse = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonBrowseOther, labelOtherFileName },
        };
        var buttonFindTextOther = UiUtil.MakeButton(Se.Language.Sync.FindText, vm.FindTextOtherCommand);
        buttonFindTextOther.HorizontalAlignment = HorizontalAlignment.Right;
        var panelOtherHeader = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        panelOtherHeader.Add(panelOtherBrowse, 0, 0);
        panelOtherHeader.Add(buttonFindTextOther, 0, 1);

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var dataGridSubtitle = new DataGrid
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
            ItemsSource = vm.Othersubtitles,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Text)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGridSubtitle.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedOtherSubtitle)));

        // Clicking a line in the left grid scrolls this grid to the matching time (#12529)
        // without touching its selection.
        vm.ScrollOtherToLine = line => dataGridSubtitle.ScrollIntoView(line, null);

        grid.Add(panelOtherHeader, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGridSubtitle), 1);

        return grid;
    }
}