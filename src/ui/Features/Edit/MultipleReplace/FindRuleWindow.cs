using Avalonia;
using Avalonia.Automation;
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var textBoxSearch = UiUtil.MakeTextBox(300, vm, nameof(vm.SearchText)).WithSearchAndClearIcons();
        textBoxSearch.Classes.Add("text");
        textBoxSearch.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(textBoxSearch, Se.Language.General.Search);

        var searchPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(4, 2), // lines up with the list's 14 px margin
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.General.Search, new Thickness(0)),
                textBoxSearch,
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var rulesList = MakeDataGrid(vm);
        rulesList.Margin = new Thickness(14, 12, 14, 14);

        grid.Add(EditorChrome.MakeToolbar(searchPanel), 0, 0);
        grid.Add(rulesList, 1, 0);
        grid.Add(panelButtons, 2, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxSearch);
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
            Binding = new Binding($"{nameof(RuleTreeNode.Parent)}.{nameof(RuleTreeNode.CategoryName)}"),
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

        AutomationProperties.SetName(dataGrid, Se.Language.General.Rules);
        return EditorChrome.MakeListFrame(dataGrid);
    }
}
