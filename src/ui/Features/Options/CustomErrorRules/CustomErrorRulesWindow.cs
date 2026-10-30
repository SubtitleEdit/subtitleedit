using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.CustomErrorRules;

/// <summary>
/// Custom error rules (#10136) in the editor look (see <see cref="EditorChrome"/>): the on/off
/// switch, the rule list with its tool buttons, the selected rule's fields and a test box,
/// Cancel + OK in the button bar.
/// </summary>
public class CustomErrorRulesWindow : Window
{
    private const double LabelWidth = 140;

    public CustomErrorRulesWindow(CustomErrorRulesViewModel vm)
    {
        var l = Se.Language.Options.Settings;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = l.CustomErrorRules;
        CanResize = true;
        Width = 760;
        Height = 760;
        MinWidth = 560;
        MinHeight = 520;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);
        vm.Window = this;
        DataContext = vm;

        var generalContent = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeToggleRow(nameof(vm.IsHighlightEnabled), l.CustomErrorRulesHighlight),
                EditorChrome.MakeWrappingDimLabel(l.CustomErrorRulesHint),
            },
        };

        var toolButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Children =
            {
                EditorChrome.MakeTextToolButton(IconNames.Plus, Se.Language.General.Add, vm.AddCommand),
                EditorChrome.MakeTextToolButton(IconNames.Trash, Se.Language.General.Remove, vm.RemoveCommand)
                    .WithBindIsEnabled(nameof(vm.HasSelectedRule)),
                EditorChrome.MakeTextToolButton(IconNames.ArrowUpThin, Se.Language.General.MoveUp, vm.MoveUpCommand)
                    .WithBindIsEnabled(nameof(vm.HasSelectedRule)),
                EditorChrome.MakeTextToolButton(IconNames.ArrowDownThin, Se.Language.General.MoveDown, vm.MoveDownCommand)
                    .WithBindIsEnabled(nameof(vm.HasSelectedRule)),
            },
        };

        var rulesList = new Border
        {
            Child = MakeRulesGrid(vm),
            Height = 200,
        };

        var selectedRuleEditor = MakeSelectedRuleEditor(vm, out var textBoxName);
        var sections = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(l.CustomErrorRules, generalContent, first: true),
                EditorChrome.MakeEditorSection(Se.Language.General.Rules, rulesList, toolButtons),
                EditorChrome.MakeEditorSection(l.CustomErrorRuleSelected, selectedRuleEditor),
                EditorChrome.MakeEditorSection(l.CustomErrorRuleTest, MakeTestSection(vm)),
            },
        };

        var scrollViewer = new ScrollViewer
        {
            Content = sections,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(scrollViewer, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, textBoxName); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Border MakeRulesGrid(CustomErrorRulesViewModel vm)
    {
        var grid = TableViewExtras.MakeTableView();
        grid.ItemsSource = vm.Rules;

        grid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Enabled,
            Width = new GridLength(80),
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<CustomErrorRuleItem>((_, _) =>
            {
                var checkBox = new CheckBox
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(CustomErrorRuleItem.IsEnabled)) { Mode = BindingMode.TwoWay },
                };
                AutomationProperties.SetName(checkBox, Se.Language.General.Enabled);
                return checkBox;
            }),
        });
        grid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            Binding = new Binding(nameof(CustomErrorRuleItem.Name)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        grid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.RegularExpression,
            Binding = new Binding(nameof(CustomErrorRuleItem.Pattern)),
            Width = new GridLength(2, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        grid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedRule))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });

        AutomationProperties.SetName(grid, Se.Language.Options.Settings.CustomErrorRules);
        return EditorChrome.MakeListFrame(grid, clearListBackground: true);
    }

    private static StackPanel MakeSelectedRuleEditor(CustomErrorRulesViewModel vm, out TextBox textBoxName)
    {
        textBoxName = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding($"{nameof(vm.SelectedRule)}.{nameof(CustomErrorRuleItem.Name)}") { Mode = BindingMode.TwoWay },
        }.AsTextField(Se.Language.General.Name);

        var textBoxPattern = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding($"{nameof(vm.SelectedRule)}.{nameof(CustomErrorRuleItem.Pattern)}") { Mode = BindingMode.TwoWay },
        }.AsTextField(Se.Language.General.RegularExpression);

        var labelInvalid = new TextBlock
        {
            Text = Se.Language.SourceView.InvalidRegularExpression,
            Foreground = EditorChrome.DimTextBrush,
            [!IsVisibleProperty] = new Binding($"{nameof(vm.SelectedRule)}.{nameof(CustomErrorRuleItem.IsPatternValid)}")
            {
                Converter = BoolConverters.Not,
                FallbackValue = false,
            },
        };

        var toggleMatchCase = EditorChrome.MakeToggleRow($"{nameof(vm.SelectedRule)}.{nameof(CustomErrorRuleItem.MatchCase)}", Se.Language.General.CaseSensitive);

        var colorButton = UiUtil.MakeColorPickerButton(vm, $"{nameof(vm.SelectedRule)}.{nameof(CustomErrorRuleItem.Color)}");
        colorButton.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(colorButton, Se.Language.General.Color);

        var panel = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.Name, textBoxName, LabelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.RegularExpression, textBoxPattern, LabelWidth),
                labelInvalid,
                toggleMatchCase,
                EditorChrome.MakeLabeledRow(Se.Language.General.Color, colorButton, LabelWidth),
            },
        };
        panel.Bind(IsEnabledProperty, new Binding(nameof(vm.HasSelectedRule)));
        return panel;
    }

    private static StackPanel MakeTestSection(CustomErrorRulesViewModel vm)
    {
        var textBoxTest = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            AcceptsReturn = true,
            Height = 60,
            [!TextBox.TextProperty] = new Binding(nameof(vm.TestText)) { Mode = BindingMode.TwoWay },
        }.AsTextField(Se.Language.General.SampleText);

        var labelResult = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.TestResult)),
        };

        return new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.SampleText, textBoxTest, LabelWidth),
                labelResult,
            },
        };
    }
}
