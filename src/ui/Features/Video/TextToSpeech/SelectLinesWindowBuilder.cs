using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// Shared layout for the select-lines dialogs (detect speakers, skip noise lines):
/// info label on top, a checkable rows table, a selection panel, and the Ok/Cancel bar.
/// The windows pass in only what differs - title, width, selection panel, extra columns.
/// </summary>
public static class SelectLinesWindowBuilder
{
    public static void Initialize<TRow>(Window window, SelectLinesViewModelBase<TRow> vm,
        string title, double width, Control selectionPanel, Control rowsView)
        where TRow : SelectLinesRowBase
    {
        UiUtil.InitializeWindow(window, window.GetType().Name);
        window.Title = UiUtil.MakeWindowTitle(title);
        window.CanResize = true;
        window.Width = width;
        window.Height = 600;
        window.MinWidth = 600;
        window.MinHeight = 400;
        vm.Window = window;
        window.DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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

        var labelInfo = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.RowsInfo));

        grid.Add(labelInfo, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(rowsView), 1);
        grid.Add(selectionPanel, 2);
        grid.Add(panelButtons, 3);

        window.Content = grid;

        // Focus the table, not OK. AddSpaceToggle puts a tunnelling handler on the TableView, so
        // Space only toggles a row while focus is inside it - with OK focused the very first
        // Space activated the button and accepted the dialog with every line still pre-checked.
        // ProfilesWindow spells out the same rule ("a focused button clicks on bare Space").
        window.Activated += delegate
        {
            if (rowsView is TableView tableView)
            {
                TableViewExtras.FocusRow(tableView);
            }
            else
            {
                buttonOk.Focus();
            }
        };
        window.KeyDown += vm.KeyDown;

        window.Closing += delegate { UiUtil.SaveWindowPosition(window); };
        window.Loaded += delegate { UiUtil.RestoreWindowPosition(window); };
    }

    public static Control MakeRowsView<TRow>(SelectLinesViewModelBase<TRow> vm,
        string checkBoxColumnHeader, params SeTableViewColumn[] extraColumnsBeforeText)
        where TRow : SelectLinesRowBase
    {
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Rows;

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = checkBoxColumnHeader,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<TRow>((_, _) => new Border
            {
                Background = Brushes.Transparent,
                Padding = new Avalonia.Thickness(4),
                Child = new CheckBox
                {
                    Focusable = false,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(SelectLinesRowBase.IsSelected))
                    {
                        Mode = BindingMode.TwoWay,
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
            }),
            Width = new GridLength(80),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(SelectLinesRowBase.Number)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(60),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            Binding = new Binding(nameof(SelectLinesRowBase.Show)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(120),
        });
        foreach (var column in extraColumnsBeforeText)
        {
            dataGrid.Columns.Add(column);
        }
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(SelectLinesRowBase.Text)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
        });

        TableViewExtras.AddSpaceToggle<TRow>(dataGrid,
            item => item.IsSelected,
            (item, value) => item.IsSelected = value);

        return dataGrid;
    }
}
