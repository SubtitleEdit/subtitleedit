using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class CategoryExportWindow : Window
{
    public CategoryExportWindow(CategoryExportViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Edit.MultipleReplace.ExportReplaceRules;
        CanResize = true;
        Width = 1100;
        Height = 750;
        MinWidth = 800;
        MinHeight = 700;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
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
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeDataGrid(vm, out var dataGrid), 0, 0);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        Activated += delegate { TableViewExtras.FocusRow(dataGrid); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private static Border MakeDataGrid(CategoryExportViewModel vm, out TableView tableView)
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
                        [!CheckBox.IsCheckedProperty] = new Binding(nameof(RuleTreeNode.IsSelected)),
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

        tableView = dataGrid;

        grid.Add(dataGrid, 0);

        return UiUtil.MakeBorderForControlNoPadding(grid);
    }
}
