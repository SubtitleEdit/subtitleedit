using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

public class ModifySelectionWindow : Window
{
    public ModifySelectionWindow(ModifySelectionViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Edit.ModifySelection.Title;
        CanResize = true;
        Width = 900;
        Height = 700;
        MinWidth = 825;
        MinHeight = 450;
        vm.Window = this;
        DataContext = vm;

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
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeRulesView(vm, out TextBox textbox), 0);
        grid.Add(MakeSelectionView(vm), 0, 1);
        grid.Add(MakeSubtitleView(vm), 1, 0, 1, 2);
        grid.Add(panelButtons, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate
        {
            buttonOk.Focus();
            if (textbox.IsVisible)
            {
                textbox.Focus();
            }
        };
        KeyDown += vm.KeyDown;
    }

    private static Border MakeRulesView(ModifySelectionViewModel vm, out TextBox textBoxRuleText)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            VerticalAlignment = VerticalAlignment.Top,
        };

        var comboBoxRules = UiUtil.MakeComboBox(vm.Rules, vm, nameof(vm.SelectedRule)).WithWidth(175).WithTopAlignment();
        comboBoxRules.SelectionChanged += (sender, args) => vm.OnRuleChanged();

        textBoxRuleText = UiUtil.MakeTextBox(150, vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.Text));
        textBoxRuleText.BindIsVisible(vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.HasText));
        textBoxRuleText.TextChanged += (sender, args) => vm.OnRuleChanged();

        var numericUpDownRuleNumber = UiUtil.MakeNumericUpDownInt(0, 10000, 100, 150, vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.Number));
        numericUpDownRuleNumber.BindIsVisible(vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.HasNumber));
        numericUpDownRuleNumber.ValueChanged += (sender, args) => vm.OnRuleChanged();

        var dataGridMultiSelect = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            MaxHeight = 200,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Enabled,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<MultiSelectItem>((item, _) =>
                    new Border
                    {
                        Background = Brushes.Transparent, // Prevents highlighting
                        Padding = new Thickness(4),
                        Child = MakeCheckBox(vm)
                    }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.Name)),
                    IsReadOnly = true,
                },
            },
        };
        dataGridMultiSelect.BindIsVisible(vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.HasMultiSelect));
        dataGridMultiSelect.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.MultiSelectItems)) { Source = vm });

        var panelRule = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                comboBoxRules,
                textBoxRuleText,
                numericUpDownRuleNumber,
                dataGridMultiSelect,
            },
        };

        var checkBoxRuleCaseSensitive = UiUtil.MakeCheckBox(Se.Language.General.CaseSensitive, vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.MatchCase));
        checkBoxRuleCaseSensitive.BindIsVisible(vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.HasMatchCase));
        checkBoxRuleCaseSensitive.IsCheckedChanged += (sender, args) => vm.OnRuleChanged();

        grid.Add(panelRule, 0);
        grid.Add(checkBoxRuleCaseSensitive, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static CheckBox MakeCheckBox(ModifySelectionViewModel vm)
    {
        var checkBox = new CheckBox
        {
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(MultiSelectItem.Apply)),
            HorizontalAlignment = HorizontalAlignment.Center,
        };


        checkBox.IsCheckedChanged += (sender, args) => vm.OnRuleChanged();

        return checkBox;
    }

    private static Border MakeSelectionView(ModifySelectionViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(UiUtil.MakeRadioButton(Se.Language.Edit.ModifySelection.SelectionNew, vm, nameof(vm.SelectionNew), "selection"), 0);
        grid.Add(UiUtil.MakeRadioButton(Se.Language.Edit.ModifySelection.SelectionAdd, vm, nameof(vm.SelectionAdd), "selection"), 1);
        grid.Add(UiUtil.MakeRadioButton(Se.Language.Edit.ModifySelection.SelectionSubtract, vm, nameof(vm.SelectionSubtract), "selection"), 2);
        grid.Add(UiUtil.MakeRadioButton(Se.Language.Edit.ModifySelection.SelectionIntersect, vm, nameof(vm.SelectionIntersect), "selection"), 3);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeSubtitleView(ModifySelectionViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

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
            ItemsSource = vm.Subtitles,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<PreviewItem>((item, _) =>
                        new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = new CheckBox
                            {
                                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(PreviewItem.Apply)),
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(PreviewItem.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    Binding = new Binding(nameof(PreviewItem.Show)) { Converter = fullTimeConverter },
                    Width = new DataGridLength(120),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Duration,
                    Binding = new Binding(nameof(PreviewItem.Duration)) { Converter = shortTimeConverter },
                    Width = new DataGridLength(120),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(PreviewItem.Text)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
