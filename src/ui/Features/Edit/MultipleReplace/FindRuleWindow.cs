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
            IsReadOnly = true,
            DataContext = vm,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Category,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding("Parent.CategoryName"),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Find,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(RuleTreeNode.Find)),
                    IsReadOnly = true,
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.ReplaceWith,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(RuleTreeNode.ReplaceWith)),
                    IsReadOnly = true,
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Description,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(RuleTreeNode.Description)),
                    IsReadOnly = true,
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Type,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(RuleTreeNode.SearchType)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
            },
        };

        dataGrid.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(vm.Rules)) { Source = vm });
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedRule)) { Source = vm, Mode = BindingMode.TwoWay });
        dataGrid.DoubleTapped += vm.DataGridDoubleTapped;

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
