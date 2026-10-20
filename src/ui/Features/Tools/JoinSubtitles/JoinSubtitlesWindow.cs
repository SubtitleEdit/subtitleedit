using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using System.Collections;
using System.Windows.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.JoinSubtitles;

/// <summary>
/// Join subtitles, in the editor look (see <see cref="EditorChrome"/>): the file list (drop files on
/// it) with add/remove/clear on its header, the time code mode below, the file count, Cancel and
/// Join at the bottom.
/// </summary>
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButton(Se.Language.Tools.JoinSubtitles.Join, vm.OkCommand).WithBindEnabled(nameof(vm.IsJoinEnabled));
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var labelFileCount = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0));
        labelFileCount.Bind(TextBlock.TextProperty, new Binding($"{nameof(vm.JoinItems)}.{nameof(vm.JoinItems.Count)}")
        {
            Source = vm,
            Mode = BindingMode.OneWay,
            StringFormat = Se.Language.General.XFiles,
        });

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        sections.Add(MakeFilesView(vm), 0);
        sections.Add(MakeControlsView(vm), 1);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(labelFileCount, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(_tableViewFiles); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private Border MakeFilesView(JoinSubtitlesViewModel vm)
    {
        // The list fills the rest of the window, so the section header and list are laid out
        // in a grid rather than in a section's stack panel.
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 8,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();

        // Sorting dropped in the DataGrid -> TableView conversion: the join is produced
        // by iterating this list in order (the VM sorts it by start time itself), so the
        // list must not be reordered by clicking a header. Reordering is done explicitly
        // instead, through the move items in the context menu.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false).WithAccessibleName(Se.Language.General.SubtitleFiles);
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

        var buttonAdd = EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.Add, vm.AddCommand, 28);
        var buttonRemove = EditorChrome.CreateToolButton("fa-solid fa-trash-can", Se.Language.General.Remove, vm.RemoveCommand, 28);
        var buttonClear = EditorChrome.CreateToolButton("fa-solid fa-xmark", Se.Language.General.Clear, vm.ClearCommand, 28);
        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Children = { buttonAdd, buttonRemove, buttonClear },
        };

        var header = new DockPanel();
        DockPanel.SetDock(panelButtons, Dock.Right);
        header.Children.Add(panelButtons);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.SubtitleFiles));

        // hack to make drag and drop work on the DataGrid - also on empty rows (the frame has
        // a background, so it is hit-tested everywhere)
        var dropHost = EditorChrome.MakeListFrame(dataGrid, clearListBackground: true);
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, vm.FileGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, vm.FileGridOnDrop, RoutingStrategies.Bubble);

        grid.Add(header, 0);
        grid.Add(dropHost, 1);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(14, 10, 14, 14),
        };
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

    private static Border MakeControlsView(JoinSubtitlesViewModel vm)
    {
        var radioKeepTimeCodes = UiUtil.MakeRadioButton(Se.Language.Tools.JoinSubtitles.KeepTimeCodes, vm, nameof(vm.KeepTimeCodes), "TimeCodes");
        radioKeepTimeCodes.FontSize = 12.5;

        var radioAppendTimeCodes = UiUtil.MakeRadioButton(Se.Language.Tools.JoinSubtitles.AppendTimeCodes, vm, nameof(vm.AppendTimeCodes), "TimeCodes")
            .WithMarginRight(12);
        radioAppendTimeCodes.FontSize = 12.5;
        var numericUpDownAppendMilliseconds = UiUtil.MakeNumericUpDownInt(0, 10000, 0, 90, vm, nameof(vm.AppendTimeCodesAddMilliseconds))
            .WithBindEnabled(nameof(vm.AppendTimeCodes))
            .WithAccessibleName(Se.Language.Tools.JoinSubtitles.AddMsAfterEachFile);
        numericUpDownAppendMilliseconds.Classes.Add("field");
        var labelAddMilliseconds = EditorChrome.MakeDimLabel(Se.Language.Tools.JoinSubtitles.AddMsAfterEachFile, new Thickness(8, 0, 0, 0));

        var stackPanelAppend = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children =
            {
                radioAppendTimeCodes,
                numericUpDownAppendMilliseconds,
                labelAddMilliseconds,
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

        return EditorChrome.MakeEditorSection(Se.Language.General.TimeCodes, stackPanel);
    }
}
