using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

/// <summary>
/// Entry boxes on top, the rules typed so far under them, OK adds them all to the category.
/// The boxes stay put between rules - the window is made for entering a list, not one rule.
/// </summary>
public class AddRulesWindow : Window
{
    public AddRulesWindow(AddRulesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Width = 760;
        Height = 560;
        MinWidth = 560;
        MinHeight = 400;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var textBoxFindWhat = MakeEntryBox(vm, nameof(vm.FindWhat), Se.Language.Edit.MultipleReplace.FindWhat);
        var textBoxReplaceWith = MakeEntryBox(vm, nameof(vm.ReplaceWith), Se.Language.General.ReplaceWith);
        textBoxFindWhat.KeyDown += vm.OnEntryKeyDown;
        textBoxReplaceWith.KeyDown += vm.OnEntryKeyDown;

        var buttonAdd = UiUtil.MakeButton(Se.Language.General.Add, vm.AddCommand);
        buttonAdd.VerticalAlignment = VerticalAlignment.Center;

        // Labels above the boxes: find and replace share the width, the button takes what it needs.
        var entryGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 8,
            RowSpacing = 4,
        };
        entryGrid.Add(EditorChrome.MakeDimLabel(Se.Language.Edit.MultipleReplace.FindWhat, new Thickness(0)), 0, 0);
        entryGrid.Add(textBoxFindWhat, 1, 0);
        entryGrid.Add(EditorChrome.MakeDimLabel(Se.Language.General.ReplaceWith, new Thickness(0)), 0, 1);
        entryGrid.Add(textBoxReplaceWith, 1, 1);
        entryGrid.Add(buttonAdd, 1, 2);

        var radioButtonRegularExpression = UiUtil.MakeRadioButton(Se.Language.General.RegularExpression, vm, nameof(vm.IsRegularExpression));
        var radioButtonCaseSensitive = UiUtil.MakeRadioButton(Se.Language.General.CaseSensitive, vm, nameof(vm.IsCaseSensitive));
        var radioButtonCaseInsensitive = UiUtil.MakeRadioButton(Se.Language.General.CaseInsensitive, vm, nameof(vm.IsCaseInsensitive));
        var switchWholeWord = EditorChrome.MakeSwitch(nameof(vm.IsWholeWord), Se.Language.Edit.Find.WholeWord)
            .WithBindIsEnabled(nameof(vm.IsWholeWordEnabled));
        var panelOptions = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            ItemSpacing = 18,
            Children =
            {
                radioButtonCaseInsensitive,
                radioButtonCaseSensitive,
                radioButtonRegularExpression,
                EditorChrome.MakeSwitchRow(switchWholeWord, Se.Language.Edit.Find.WholeWord),
            }
        };

        var labelHint = EditorChrome.MakeDimLabel(Se.Language.Edit.MultipleReplace.EnterAddsRule, new Thickness(0));
        labelHint.FontSize = 11.5;

        var entryPanel = new StackPanel
        {
            Spacing = 10,
            Children = { entryGrid, panelOptions, labelHint },
        };
        var entrySection = EditorChrome.MakeEditorSection(Se.Language.Edit.MultipleReplace.NewRule, entryPanel, first: true);
        entrySection.Background = EditorChrome.PanelBrush;
        entrySection.BorderBrush = EditorChrome.LineBrush;
        entrySection.BorderThickness = new Thickness(0, 0, 0, 1);

        var rulesList = MakeRulesList(vm);

        var buttonRemove = EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Remove, vm.RemoveSelectedCommand, 26);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        actions.Children.Add(buttonRemove);

        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(actions, Dock.Right);
        header.Children.Add(actions);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Rules));

        var listGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
            Margin = new Thickness(14, 12, 14, 14),
        };
        listGrid.Add(header, 0);
        listGrid.Add(EditorChrome.MakeListFrame(rulesList), 1);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var labelRulesInfo = EditorChrome.MakeBoundHint(nameof(vm.RulesInfo));
        labelRulesInfo.FontSize = 12;
        var buttonPanel = EditorChrome.MakeButtonBar(labelRulesInfo, buttonCancel, buttonOk);

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };
        root.Add(entrySection, 0, 0);
        root.Add(listGrid, 1, 0);
        root.Add(buttonPanel, 2, 0);

        Content = root;

        RegexContextFlyout.Attach(textBoxFindWhat, vm, () => vm.IsRegularExpression);
        RegexContextFlyout.Attach(textBoxReplaceWith, vm, () => vm.IsRegularExpression, isReplaceBox: true);

        vm.FocusFindWhat = () => textBoxFindWhat.Focus();

        UiUtil.FocusOnFirstActivation(this, textBoxFindWhat); // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
        Loaded += (_, _) =>
        {
            Title = vm.Title;
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
    }

    private static TextBox MakeEntryBox(AddRulesViewModel vm, string propertyTextPath, string automationName)
    {
        var textBox = new TextBox
        {
            MinWidth = 100,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Classes = { "text" },
            DataContext = vm,
            [!TextBox.TextProperty] = new Binding(propertyTextPath) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(textBox, automationName);
        return textBox;
    }

    private static TableView MakeRulesList(AddRulesViewModel vm)
    {
        var table = TableViewExtras.MakeTableView(multiSelect: false);
        table.DataContext = vm;
        table.Columns.AddRange(new TableViewColumn[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.Type,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = new FuncDataTemplate<MultipleReplaceRule>((rule, _) =>
                {
                    var icon = new Label
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(0),
                    };
                    if (rule != null)
                    {
                        Attached.SetIcon(icon, RuleTreeNode.GetIconName(rule.Type));
                    }

                    return new Border { Background = Avalonia.Media.Brushes.Transparent, Padding = new Thickness(4), Child = icon };
                }),
                Width = new GridLength(60),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.Edit.MultipleReplace.FindWhat,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(MultipleReplaceRule.Find)),
                Width = new GridLength(1, GridUnitType.Star),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.ReplaceWith,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(MultipleReplaceRule.ReplaceWith)),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });

        table.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.Rules)) { Source = vm });
        table.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedRule)) { Source = vm, Mode = BindingMode.TwoWay });
        table.KeyDown += vm.OnListKeyDown;
        table.DoubleTapped += (_, e) =>
        {
            if (!TableViewExtras.IsInColumnHeader(e.Source as Visual))
            {
                vm.EditSelectedCommand.Execute(null);
            }
        };

        AutomationProperties.SetName(table, Se.Language.General.Rules);
        return table;
    }
}
