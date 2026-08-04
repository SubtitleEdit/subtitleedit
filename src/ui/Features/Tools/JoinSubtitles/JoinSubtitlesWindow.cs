using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections;
using System.Windows.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.JoinSubtitles;

public class JoinSubtitlesWindow : Window
{
    private TableView _tableViewFiles = null!;

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

        Activated += delegate { TableViewExtras.FocusRow(_tableViewFiles); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private Border MakeFilesView(JoinSubtitlesViewModel vm)
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

        // Sorting dropped in the DataGrid -> TableView conversion: the join is produced
        // by iterating this list in order (the VM sorts it by start time itself), so the
        // list must not be reordered by clicking a header. Reordering is done explicitly
        // instead, through the move items in the context menu.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        _tableViewFiles = dataGrid;
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.JoinItems;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NoSymbolLines,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(JoinDisplayItem.Lines)),
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(80),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.StartTime,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(JoinDisplayItem.StartTime)) { Converter = fullTimeConverter },
                    Width = new GridLength(120),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.EndTime,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(JoinDisplayItem.EndTime)) { Converter = fullTimeConverter },
                    Width = new GridLength(120),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.FileName,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(JoinDisplayItem.FileName)),
                    Width = new GridLength(1, GridUnitType.Star),
                },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedJoinItem)) { Source = vm });
        dataGrid.KeyDown += vm.GridKeyDown;
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var index = e.Key == Key.Home ? 0 : items.Count - 1;
                dataGrid.SelectedIndex = index;
                dataGrid.ScrollIntoView(index);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);
        dataGrid.AddHandler(InputElement.KeyDownEvent, vm.GridMoveKeyDown, RoutingStrategies.Tunnel);
        vm.JoinItemsGrid = dataGrid;

        var flyout = new MenuFlyout();
        flyout.Opening += vm.ItemsContextMenuOpening;
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.RemoveCommand,
        };
        menuItemDelete.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsDeleteVisible)) { Source = vm });
        flyout.Items.Add(menuItemDelete);

        AddMoveMenuItems(flyout, vm);

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

    /// <summary>
    /// The "move up/down/to top/to bottom" block of the file list context menu (#13092).
    /// The files are joined in list order, so this is real reordering, not a view sort.
    /// Hidden in "Keep time codes" mode, where the order does not matter - the paragraphs
    /// are sorted by start time regardless - exactly as in SE 4.
    /// </summary>
    private static void AddMoveMenuItems(MenuFlyout flyout, JoinSubtitlesViewModel vm)
    {
        var separator = new Separator();
        separator.Bind(Separator.IsVisibleProperty, new Binding(nameof(vm.IsMoveVisible)) { Source = vm });
        flyout.Items.Add(separator);

        var items = new (string Header, ICommand Command, KeyGesture? Gesture)[]
        {
            (Se.Language.General.MoveUp, vm.MoveUpCommand, new KeyGesture(Key.Up, KeyModifiers.Control)),
            (Se.Language.General.MoveDown, vm.MoveDownCommand, new KeyGesture(Key.Down, KeyModifiers.Control)),
            (Se.Language.General.MoveToTop, vm.MoveToTopCommand, null),
            (Se.Language.General.MoveToBottom, vm.MoveToBottomCommand, null),
        };

        foreach (var (header, command, gesture) in items)
        {
            var menuItem = new MenuItem
            {
                Header = header,
                DataContext = vm,
                Command = command,
                InputGesture = gesture,
            };
            menuItem.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsMoveVisible)) { Source = vm });
            flyout.Items.Add(menuItem);
        }
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
