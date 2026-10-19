using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class CategoryPickerWindow : Window
{
    public CategoryPickerWindow(CategoryPickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.Title; // export or import - the view model is initialized before this runs
        CanResize = true;
        Width = 1100;
        Height = 750;
        MinWidth = 800;
        MinHeight = 700;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var buttonSelectAll = EditorChrome.CreateToolButton("fa-solid fa-square-check", Se.Language.General.SelectAll, vm.SelectAllCommand, 26);
        var buttonSelectNone = EditorChrome.CreateToolButton("fa-regular fa-square", Se.Language.General.SelectNone, vm.SelectNoneCommand, 26);
        var buttonInvertSelection = EditorChrome.CreateToolButton("fa-solid fa-right-left", Se.Language.General.InvertSelection, vm.InvertSelectionCommand, 26);
        var panelSelectionButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonSelectAll, buttonSelectNone, buttonInvertSelection },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        // The list header carries the select all/none/invert actions, the button bar OK/Cancel:
        // each in its own row. Sharing one cell (the button bar spanning both columns) only kept
        // them apart because one is left- and the other right-aligned - in a locale with longer
        // button labels they overlapped.
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(panelSelectionButtons, Dock.Right);
        header.Children.Add(panelSelectionButtons);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Category));

        var listPanel = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
            Margin = new Thickness(14, 12, 14, 14),
        };
        listPanel.Add(header, 0);
        listPanel.Add(MakeDataGrid(vm, out var dataGrid), 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(listPanel, 0, 0);
        grid.Add(panelButtons, 1, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(dataGrid); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private static Border MakeDataGrid(CategoryPickerViewModel vm, out TableView tableView)
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

        // No header sorting (the DataGrid's CanUserSortColumns is not carried over):
        // the caller writes result.Rules to the exported file in collection order, so
        // reordering the backing collection would reorder the export.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.DataContext = vm;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.Enabled,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = new FuncDataTemplate<RuleTreeNode>((item, _) =>
                new Border
                {
                    Background = Brushes.Transparent, // Prevents highlighting
                    Padding = new Thickness(4),
                    Child = new CheckBox
                    {
                        Focusable = false, // the row keeps the focus, so Space toggles via AddSpaceToggle below
                        [!ToggleButton.IsCheckedProperty] = new Binding(nameof(RuleTreeNode.IsSelected)),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                }),
                // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                Width = new GridLength(80)
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Name,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(RuleTreeNode.CategoryName)),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });
        dataGrid.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.Rules)) { Source = vm });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedRule)) { Source = vm });

        // Space toggles the checkbox (tunnel handler, so it runs before the ListBox
        // machinery can swallow the key).
        TableViewExtras.AddSpaceToggle<RuleTreeNode>(dataGrid,
            item => item.IsSelected, (item, v) => item.IsSelected = v);

        AutomationProperties.SetName(dataGrid, Se.Language.General.Category);
        tableView = dataGrid;

        grid.Add(dataGrid, 0);

        return EditorChrome.MakeListFrame(grid);
    }
}
