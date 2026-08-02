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

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
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

        var comboBoxRules = UiUtil.MakeComboBox(vm.Rules, vm, nameof(vm.SelectedRule)).WithWidth(230).WithTopAlignment();
        comboBoxRules.SelectionChanged += (sender, args) => vm.OnRuleChanged();

        textBoxRuleText = UiUtil.MakeTextBox(150, vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.Text));
        textBoxRuleText.BindIsVisible(vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.HasText));
        textBoxRuleText.TextChanged += (sender, args) => vm.OnRuleChanged();

        var numericUpDownRuleNumber = UiUtil.MakeNumericUpDownInt(0, 10000, 0, 150, vm);
        numericUpDownRuleNumber.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.Number),
            Mode = BindingMode.TwoWay,
            Converter = new NullableDoubleConverter(),
        });
        numericUpDownRuleNumber.BindIsVisible(vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.HasNumber));
        numericUpDownRuleNumber.ValueChanged += (sender, args) => vm.OnRuleChanged();

        var dataGridMultiSelect = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridMultiSelect.CanUserResizeColumns = false;
        dataGridMultiSelect.Width = 280;
        dataGridMultiSelect.MaxHeight = 200;
        dataGridMultiSelect.DataContext = vm;
        dataGridMultiSelect.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<MultiSelectItem>((item, _) =>
            new Border
            {
                Background = Brushes.Transparent, // Prevents highlighting
                Padding = new Thickness(4),
                Child = MakeCheckBox(vm)
            }),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(80)
        });
        dataGridMultiSelect.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(StyleDisplay.Name)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGridMultiSelect.BindIsVisible(vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.HasMultiSelect));
        dataGridMultiSelect.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.MultiSelectItems)) { Source = vm });

        var buttonRuleSettings = UiUtil.MakeButton(Se.Language.General.Settings, vm.ShowRuleSettingsCommand).WithTopAlignment();
        buttonRuleSettings.BindIsVisible(vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.HasSettings));

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
                buttonRuleSettings,
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

        // No header sorting (the DataGrid's CanUserSortColumns is not carried over):
        // this is a subtitle-line preview in subtitle order, and Ok() iterates the
        // collection in order to build the selection.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.DataContext = vm;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.Apply,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
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
                // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                Width = new GridLength(80)
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.NumberSymbol,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(PreviewItem.Number)),
                Width = new GridLength(60),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Show,
                Binding = new Binding(nameof(PreviewItem.Show)) { Converter = fullTimeConverter },
                Width = new GridLength(120),
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Duration,
                Binding = new Binding(nameof(PreviewItem.Duration)) { Converter = shortTimeConverter },
                Width = new GridLength(120),
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Text,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(PreviewItem.Text)),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });

        // Bind ItemsSource to the property (rather than assigning the instance once)
        // so the grid follows the collection when the view model swaps it on preview.
        dataGrid.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.Subtitles)) { Source = vm });

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
