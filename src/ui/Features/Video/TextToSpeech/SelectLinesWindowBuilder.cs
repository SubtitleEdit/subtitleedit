using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// Shared layout for the select-lines dialogs (detect speakers, skip noise lines), in the editor
/// look (see <see cref="EditorChrome"/>): a checkable rows table under a header with the selection
/// actions, the rows info and Cancel/OK in the button bar. The windows pass in only what differs -
/// title, width, extra options for the header, extra columns.
/// </summary>
public static class SelectLinesWindowBuilder
{
    public static void Initialize<TRow>(Window window, SelectLinesViewModelBase<TRow> vm,
        string title, double width, Control? options, Control rowsView)
        where TRow : SelectLinesRowBase
    {
        UiUtil.InitializeWindow(window, window.GetType().Name);
        window.Title = UiUtil.MakeWindowTitle(title);
        window.CanResize = true;
        window.Width = width;
        window.Height = 600;
        window.MinWidth = 600;
        window.MinHeight = 400;
        EditorChrome.ApplyWindow(window, EditorWindowStyles.TextFieldsBare | EditorWindowStyles.MuteDisabledPrimary);
        vm.Window = window;
        window.DataContext = vm;

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        // Selection actions on the right of the list header, after the window's own options
        var actions = TtsMainEditorLook.Horizontal();
        actions.Spacing = 2;
        if (options != null)
        {
            actions.Children.Add(options);
            actions.Children.Add(EditorChrome.MakeToolbarSeparator());
        }

        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-check-double", Se.Language.General.SelectAll, vm.SelectAllCommand, 26));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-right-left", Se.Language.General.InvertSelection, vm.InverseSelectionCommand, 26));

        var header = new DockPanel { Margin = new Avalonia.Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(actions, Dock.Right);
        header.Children.Add(actions);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Lines));

        var list = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Avalonia.Thickness(14, 10, 14, 14),
        };
        list.Add(header, 0);
        list.Add(EditorChrome.MakeListFrame(rowsView), 1);

        var labelInfo = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.RowsInfo)),
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Add(new Border { Background = EditorChrome.PanelBrush, Child = list }, 0);
        grid.Add(EditorChrome.MakeButtonBar(labelInfo, buttonCancel, buttonOk), 1);

        window.Content = grid;

        // Focus the table, not OK. AddSpaceToggle puts a tunnelling handler on the TableView, so
        // Space only toggles a row while focus is inside it - with OK focused the very first
        // Space activated the button and accepted the dialog with every line still pre-checked.
        // ProfilesWindow spells out the same rule ("a focused button clicks on bare Space").
        UiUtil.FocusOnFirstActivation(window, () =>
        {
            if (rowsView is TableView tableView)
            {
                TableViewExtras.FocusRow(tableView);
            }
            else
            {
                buttonOk.Focus();
            }
        });
        window.KeyDown += vm.KeyDown;

        window.Closing += delegate { UiUtil.SaveWindowPosition(window); };
        window.Loaded += delegate { UiUtil.RestoreWindowPosition(window); };
    }

    public static Control MakeRowsView<TRow>(SelectLinesViewModelBase<TRow> vm,
        string checkBoxColumnHeader, params SeTableViewColumn[] extraColumnsBeforeText)
        where TRow : SelectLinesRowBase
    {
        var dataGrid = TableViewExtras.MakeTableView();
        Avalonia.Automation.AutomationProperties.SetName(dataGrid, Se.Language.General.Lines);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Rows;

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = checkBoxColumnHeader,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<TRow>(nameof(SelectLinesRowBase.IsSelected)),
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
