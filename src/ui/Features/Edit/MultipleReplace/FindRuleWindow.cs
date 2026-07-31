using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class FindRuleWindow : Window
{
    public FindRuleWindow(FindRuleViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Edit.MultipleReplace.FindRule;
        Width = 800;
        Height = 500;
        MinWidth = 600;
        MinHeight = 300;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var labelSearch = UiUtil.MakeLabel(Se.Language.General.Search);
        var textBoxSearch = UiUtil.MakeTextBox(300, vm, nameof(vm.SearchText));

        var searchPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 5),
            Spacing = 8,
            Children =
            {
                labelSearch,
                textBoxSearch,
            }
        };

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
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(searchPanel, 0, 0);
        grid.Add(MakeDataGrid(vm), 1, 0);
        grid.Add(panelButtons, 2, 0);

        Content = grid;

        Activated += delegate { textBoxSearch.Focus(); };
        KeyDown += vm.OnKeyDown;
    }

    private static Border MakeDataGrid(FindRuleViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.DataContext = vm;

        var columnCategory = new SeTableViewColumn
        {
            Header = Se.Language.General.Category,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding("Parent.CategoryName"),
            Width = new GridLength(1, GridUnitType.Star),
        };
        var columnFind = new SeTableViewColumn
        {
            Header = Se.Language.General.Find,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(RuleTreeNode.Find)),
            Width = new GridLength(2, GridUnitType.Star),
        };
        var columnReplaceWith = new SeTableViewColumn
        {
            Header = Se.Language.General.ReplaceWith,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(RuleTreeNode.ReplaceWith)),
            Width = new GridLength(2, GridUnitType.Star),
        };
        var columnDescription = new SeTableViewColumn
        {
            Header = Se.Language.General.Description,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(RuleTreeNode.Description)),
            Width = new GridLength(2, GridUnitType.Star),
        };
        var columnType = new SeTableViewColumn
        {
            Header = Se.Language.General.Type,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(RuleTreeNode.SearchType)),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(140),
        };
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
            columnCategory, columnFind, columnReplaceWith, columnDescription, columnType,
        });

        // Search results whose order is presentation-only (the caller consumes just
        // SelectedRule), so header sorting is safe to wire.
        var sorter = new TableViewHeaderSorter(dataGrid);
        sorter.AddSortable<RuleTreeNode, string>(columnCategory, x => x.Parent?.CategoryName ?? string.Empty)
            .AddSortable<RuleTreeNode, string>(columnFind, x => x.Find)
            .AddSortable<RuleTreeNode, string>(columnReplaceWith, x => x.ReplaceWith)
            .AddSortable<RuleTreeNode, string>(columnDescription, x => x.Description)
            .AddSortable<RuleTreeNode, string>(columnType, x => x.SearchType);

        dataGrid.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.Rules)) { Source = vm });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedRule)) { Source = vm, Mode = BindingMode.TwoWay });
        dataGrid.DoubleTapped += (sender, e) =>
        {
            // A fast double-click on a sortable header must sort, not accept the dialog.
            if (!TableViewExtras.IsInColumnHeader(e.Source as Avalonia.Visual))
            {
                vm.DataGridDoubleTapped(sender, e);
            }
        };

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
