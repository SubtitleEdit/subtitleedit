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
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

/// <summary>
/// Modify selection, in the editor look (see <see cref="EditorChrome"/>): the rule and how to combine
/// with the current selection side by side, the preview list below, Cancel and OK at the bottom.
/// </summary>
public class ModifySelectionWindow : Window
{
    public ModifySelectionWindow(ModifySelectionViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Edit.ModifySelection.Title);
        CanResize = true;
        Width = 900;
        Height = 700;
        MinWidth = 825;
        MinHeight = 450;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var top = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        top.Add(MakeRulesView(vm, out var comboBoxRules, out var textbox), 0, 0);
        top.Add(new Border
        {
            Child = MakeSelectionView(vm),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        sections.Add(top, 0, 0);
        sections.Add(MakeSubtitleView(vm), 1, 0);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        // The rule (or its text) gets first focus, not an action button - a focused button
        // clicks on bare Space.
        UiUtil.FocusOnFirstActivation(this, () =>
        {
            comboBoxRules.Focus();
            if (textbox.IsVisible)
            {
                textbox.Focus();
            }
        });
        KeyDown += vm.KeyDown;

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Border MakeRulesView(ModifySelectionViewModel vm, out ComboBox comboBoxRules, out TextBox textBoxRuleText)
    {
        comboBoxRules = UiUtil.MakeComboBox(vm.Rules, vm, nameof(vm.SelectedRule)).WithWidth(230).WithTopAlignment()
            .WithAccessibleName(Se.Language.General.Rules); // #12087: the rule row has no visible labels
        comboBoxRules.SelectionChanged += (sender, args) => vm.OnRuleChanged();

        textBoxRuleText = UiUtil.MakeTextBox(150, vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.Text));
        textBoxRuleText.Classes.Add("text");
        textBoxRuleText.BindIsVisible(vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.HasText));
        textBoxRuleText.WithAccessibleName(Se.Language.General.Text);
        textBoxRuleText.TextChanged += (sender, args) => vm.OnRuleChanged();

        var numericUpDownRuleNumber = UiUtil.MakeNumericUpDownInt(0, 10000, 0, 150, vm);
        numericUpDownRuleNumber.Classes.Add("field");
        numericUpDownRuleNumber.VerticalAlignment = VerticalAlignment.Top;
        numericUpDownRuleNumber.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.Number),
            Mode = BindingMode.TwoWay,
            Converter = new NullableDoubleConverter(),
        });
        numericUpDownRuleNumber.BindIsVisible(vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.HasNumber));
        numericUpDownRuleNumber.WithAccessibleName(Se.Language.General.Number);
        numericUpDownRuleNumber.ValueChanged += (sender, args) => vm.OnRuleChanged();

        var dataGridMultiSelect = TableViewExtras.MakeTableView(multiSelect: false);
        // The list holds the styles or the actors depending on the rule, so the rule name names it (#12087).
        dataGridMultiSelect.Bind(Avalonia.Automation.AutomationProperties.NameProperty, new Binding(nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.Name))
        {
            Source = vm,
            FallbackValue = Se.Language.General.Rules, // no rule selected until Initialize
            TargetNullValue = Se.Language.General.Rules,
        });
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
        var frameMultiSelect = EditorChrome.MakeListFrame(dataGridMultiSelect, clearListBackground: true);
        frameMultiSelect.VerticalAlignment = VerticalAlignment.Top;
        frameMultiSelect.BindIsVisible(vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.HasMultiSelect));
        dataGridMultiSelect.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.MultiSelectItems)) { Source = vm });

        var buttonRuleSettings = EditorChrome.MakeTextToolButton("fa-solid fa-gear", Se.Language.General.Settings, vm.ShowRuleSettingsCommand);
        buttonRuleSettings.VerticalAlignment = VerticalAlignment.Top;
        buttonRuleSettings.BindIsVisible(vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.HasSettings));

        var panelRule = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                comboBoxRules,
                textBoxRuleText,
                numericUpDownRuleNumber,
                frameMultiSelect,
                buttonRuleSettings,
            },
        };

        var rowCaseSensitive = EditorChrome.MakeToggleRow(nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.MatchCase), Se.Language.General.CaseSensitive);
        rowCaseSensitive.DataContext = vm;
        rowCaseSensitive.BindIsVisible(vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.HasMatchCase));
        rowCaseSensitive.Children.OfType<ToggleSwitch>().First().IsCheckedChanged += (sender, args) => vm.OnRuleChanged();

        var rows = new StackPanel { Spacing = 10 };
        rows.Children.Add(panelRule);
        rows.Children.Add(rowCaseSensitive);

        return EditorChrome.MakeEditorSection(Se.Language.General.Rules, rows, first: true);
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
        var panel = new StackPanel { Spacing = 0, MinWidth = 200 };

        panel.Children.Add(MakeRadio(UiUtil.MakeRadioButton(Se.Language.Edit.ModifySelection.SelectionNew, vm, nameof(vm.SelectionNew), "selection")));
        panel.Children.Add(MakeRadio(UiUtil.MakeRadioButton(Se.Language.Edit.ModifySelection.SelectionAdd, vm, nameof(vm.SelectionAdd), "selection")));
        panel.Children.Add(MakeRadio(UiUtil.MakeRadioButton(Se.Language.Edit.ModifySelection.SelectionSubtract, vm, nameof(vm.SelectionSubtract), "selection")));
        panel.Children.Add(MakeRadio(UiUtil.MakeRadioButton(Se.Language.Edit.ModifySelection.SelectionIntersect, vm, nameof(vm.SelectionIntersect), "selection")));

        return EditorChrome.MakeEditorSection(Se.Language.Edit.ModifySelection.Selection, panel, first: true);
    }

    private static RadioButton MakeRadio(RadioButton radioButton)
    {
        radioButton.FontSize = 12.5;
        return radioButton;
    }

    private static Border MakeSubtitleView(ModifySelectionViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        // No header sorting (the DataGrid's CanUserSortColumns is not carried over):
        // this is a subtitle-line preview in subtitle order, and Ok() iterates the
        // collection in order to build the selection.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false).WithAccessibleName(Se.Language.General.Preview);
        dataGrid.DataContext = vm;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.Apply,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<PreviewItem>(nameof(PreviewItem.Apply), focusable: true),
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

        // The preview fills the rest of the window, so the section header and list are laid out
        // in a grid rather than in a section's stack panel.
        var content = new Grid
        {
            RowSpacing = 10,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        content.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Preview), 0, 0);
        content.Add(EditorChrome.MakeListFrame(dataGrid, clearListBackground: true), 1, 0);
        return new Border
        {
            Child = content,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }
}
