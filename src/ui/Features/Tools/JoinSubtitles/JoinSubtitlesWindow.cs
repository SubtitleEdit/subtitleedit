using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.JoinSubtitles;

public class JoinSubtitlesWindow : Window
{
    public JoinSubtitlesWindow(JoinSubtitlesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.JoinSubtitles.Title;
        CanResize = true;
        Width = 900;
        Height = 700;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButton(Se.Language.Tools.JoinSubtitles.Join, vm.OkCommand).WithBindEnabled(nameof(vm.IsJoinEnabled));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

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
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeFilesView(vm), 0);
        grid.Add(MakeControlsView(vm), 1);
        grid.Add(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Border MakeFilesView(JoinSubtitlesViewModel vm)
    {
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
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
        };

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();

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
            ItemsSource = vm.JoinItems,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NoSymbolLines,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(JoinDisplayItem.Lines)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.StartTime,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(JoinDisplayItem.StartTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                    Width = new DataGridLength(120, DataGridLengthUnitType.Pixel),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.EndTime,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(JoinDisplayItem.EndTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                    Width = new DataGridLength(120, DataGridLengthUnitType.Pixel),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FileName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(JoinDisplayItem.FileName)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedJoinItem)) { Source = vm });
        dataGrid.KeyDown += vm.DataGridKeyDown;

        var flyout = new MenuFlyout();
        flyout.Opening += vm.ItemsContextMenuOpening;
        dataGrid.ContextFlyout = flyout;

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.RemoveCommand,
        };
        menuItemDelete.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsDeleteVisible)) { Source = vm });
        flyout.Items.Add(menuItemDelete);

        var buttonAdd = UiUtil.MakeButton(vm.AddCommand, IconNames.Plus, Se.Language.General.New);
        var buttonRemove = UiUtil.MakeButton(vm.RemoveCommand, IconNames.Trash, Se.Language.General.Remove);
        var buttonClear = UiUtil.MakeButton(vm.ClearCommand, IconNames.Close, Se.Language.General.Clear);
        var panelButtons = UiUtil.MakeButtonBar(buttonAdd, buttonRemove, buttonClear).WithAlignmentLeft();

        // hack to make drag and drop work on the DataGrid - also on empty rows
        var dropHost = new Border
        {
            Background = Brushes.Transparent,
            Child = dataGrid,
        };
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, vm.FileGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, vm.FileGridOnDrop, RoutingStrategies.Bubble);

        grid.Add(dropHost, 0);
        grid.Add(panelButtons, 1);

        return UiUtil.MakeBorderForControlNoPadding(grid);
    }

    private static StackPanel MakeControlsView(JoinSubtitlesViewModel vm)
    {
        var radioKeepTimeCodes = UiUtil.MakeRadioButton(Se.Language.Tools.JoinSubtitles.KeepTimeCodes, vm, nameof(vm.KeepTimeCodes), "TimeCodes");

        var radioAppendTimeCodes = UiUtil.MakeRadioButton(Se.Language.Tools.JoinSubtitles.AppendTimeCodes, vm, nameof(vm.AppendTimeCodes), "TimeCodes")
            .WithMarginRight(5);
        var labelAddMilliseconds = UiUtil.MakeLabel(Se.Language.Tools.JoinSubtitles.AddMsAfterEachFile);
        var numericUpDownAppendMilliseconds = UiUtil.MakeNumericUpDownInt(0, 10000, 0, 140, vm, nameof(vm.AppendTimeCodesAddMilliseconds)).WithBindEnabled(nameof(vm.AppendTimeCodes));

        var stackPanelAppend = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children =
            {
                radioAppendTimeCodes,
                numericUpDownAppendMilliseconds
            }
        };

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children =
            {
                radioKeepTimeCodes,
                stackPanelAppend
            }
        };

        return stackPanel;
    }
}
