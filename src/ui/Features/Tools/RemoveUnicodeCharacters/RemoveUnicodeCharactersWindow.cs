using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveUnicodeCharacters;

public class RemoveUnicodeCharactersWindow : Window
{
    public RemoveUnicodeCharactersWindow(RemoveUnicodeCharactersViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.RemoveUnicodeCharacters.Title;
        CanResize = true;
        Width = 800;
        Height = 600;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Tools.RemoveUnicodeCharacters;
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
            ItemsSource = vm.Characters,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    IsReadOnly = false,
                    Width = new DataGridLength(55),
                    CellTemplate = new FuncDataTemplate<RemoveUnicodeCharacterItem>((_, _) =>
                        new CheckBox
                        {
                            Focusable = false,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            [!CheckBox.IsCheckedProperty] = new Binding(nameof(RemoveUnicodeCharacterItem.IsChecked)) { Mode = BindingMode.TwoWay },
                        }),
                },
                new DataGridTextColumn
                {
                    Header = l.Character,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(RemoveUnicodeCharacterItem.Character)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Unicode",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(RemoveUnicodeCharacterItem.CodeDisplay)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Count,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(RemoveUnicodeCharacterItem.Count)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = l.ReplaceWith,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(RemoveUnicodeCharacterItem.ReplaceWith)) { Mode = BindingMode.TwoWay },
                    IsReadOnly = false,
                    Width = new DataGridLength(120),
                },
                new DataGridTextColumn
                {
                    Header = l.Lines,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(RemoveUnicodeCharacterItem.LinesDisplay)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        _ = new DataGridCheckboxMultiSelect<RemoveUnicodeCharacterItem>(dataGrid,
            item => item.IsChecked, (item, v) => item.IsChecked = v);

        var buttonSelectAll = UiUtil.MakeButton(Se.Language.General.SelectAll, vm.SelectAllCommand);
        var buttonInvertSelection = UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.InvertSelectionCommand);
        var panelSelection = UiUtil.MakeHorizontalPanel(buttonSelectAll, buttonInvertSelection);

        var labelStatus = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.StatusText)).WithAlignmentTop();

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
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

        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 0);
        grid.Add(panelSelection, 1);
        grid.Add(labelStatus, 2);
        grid.Add(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
