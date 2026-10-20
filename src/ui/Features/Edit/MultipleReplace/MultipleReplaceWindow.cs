using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class MultipleReplaceWindow : Window
{
    // Follow the app theme, also when it changes while the window is open
    private static readonly IBrush ErrorBrush = EditorChrome.Themed(Color.FromRgb(255, 100, 100), Color.FromRgb(183, 28, 28));
    private static readonly IBrush ReplacedBrush = EditorChrome.Themed(Color.FromRgb(100, 220, 100), Color.FromRgb(27, 94, 32));

    public MultipleReplaceWindow(MultipleReplaceViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.General.MultipleReplace);
        Width = 1180;
        Height = 820;
        MinWidth = 850;
        MinHeight = 560;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var categoriesView = MakeCategoriesView(vm);
        var rulesView = MakeRulesView(vm, out var textBoxNewFind);
        var fixesView = MakeFixesView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        // How many lines the checked fixes change, on the left of the button bar.
        var labelFixesInfo = EditorChrome.MakeBoundHint(nameof(vm.FixesInfo));
        labelFixesInfo.FontSize = 12;
        var panelButtons = EditorChrome.MakeButtonBar(labelFixesInfo, buttonApply, buttonCancel, buttonOk);

        // The rules of the selected category on top, the preview of what they change under them.
        var right = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(4, GridUnitType.Star), MinHeight = 220 },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // splitter
                new RowDefinition { Height = new GridLength(3, GridUnitType.Star), MinHeight = 160 },
            },
        };
        right.Add(rulesView, 0, 0);
        right.Add(new GridSplitter
        {
            Height = 5,
            Background = EditorChrome.LineBrush,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ResizeDirection = GridResizeDirection.Rows,
        }, 1, 0);
        right.Add(fixesView, 2, 0);

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(230), MinWidth = 150, },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // splitter
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 500, },
            },
        };

        content.Add(categoriesView, 0, 0);
        content.Add(new GridSplitter
        {
            Width = 5,
            Background = EditorChrome.LineBrush,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ResizeDirection = GridResizeDirection.Columns,
        }, 0, 1);
        content.Add(right, 0, 2);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Add(content, 0, 0);
        grid.Add(panelButtons, 1, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxNewFind); // initial focus on an input, not an action button - a focused button clicks on bare Space
        Closing += (_, _) => vm.OnClosing();
        Loaded += (_, _) => vm.OnLoaded();
        KeyDown += vm.OnKeyDown;
    }

    private static bool IsContextClick(PointerReleasedEventArgs e)
    {
        return e.InitialPressMouseButton == MouseButton.Right ||
            (System.OperatingSystem.IsMacOS() && e.InitialPressMouseButton == MouseButton.Left && e.KeyModifiers.HasFlag(KeyModifiers.Control));
    }

    /// <summary>
    /// The categories: a flat list with an "active" box and the number of rules in each, and the
    /// category actions under it.
    /// </summary>
    private static Border MakeCategoriesView(MultipleReplaceViewModel vm)
    {
        var listBox = new ListBox
        {
            SelectionMode = SelectionMode.Single,
            DataContext = vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Nodes)),
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedCategory)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<RuleTreeNode>((_, _) =>
            {
                var checkBox = new CheckBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    MinWidth = 0,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(RuleTreeNode.IsActive)) { Mode = BindingMode.TwoWay },
                    [!AutomationProperties.NameProperty] = new Binding(nameof(RuleTreeNode.CategoryName)),
                };
                checkBox.IsCheckedChanged += vm.OnActiveChanged;

                var label = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(6, 0, 6, 0),
                    [!TextBlock.TextProperty] = new Binding(nameof(RuleTreeNode.CategoryName)),
                };

                var count = new Border
                {
                    Background = EditorChrome.HoverBrush,
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(7, 1, 7, 1),
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = new TextBlock
                    {
                        FontSize = 11.5,
                        Foreground = EditorChrome.DimTextBrush,
                        [!TextBlock.TextProperty] = new Binding(nameof(RuleTreeNode.RuleCount)),
                    },
                };

                var buttonActions = new Button
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Command = vm.NodeCategoryOpenContextMenuCommand,
                    [!Button.CommandParameterProperty] = new Binding("."),
                    Margin = new Thickness(4, 0, 0, 0),
                    Padding = new Thickness(0),
                    Width = 22,
                    Height = 22,
                    IsVisible = vm.IsMultipleReplaceDotDotDotButtonsVisible,
                };
                buttonActions.Classes.Add("tool");
                Attached.SetIcon(buttonActions, IconNames.DotsVertical);
                AutomationProperties.SetName(buttonActions, Se.Language.General.More);

                var row = new DockPanel { LastChildFill = true, Background = Brushes.Transparent };
                DockPanel.SetDock(checkBox, Dock.Left);
                DockPanel.SetDock(buttonActions, Dock.Right);
                DockPanel.SetDock(count, Dock.Right);
                row.Children.Add(checkBox);
                row.Children.Add(buttonActions);
                row.Children.Add(count);
                row.Children.Add(label);
                row.AddHandler(InputElement.PointerReleasedEvent, (_, e) =>
                {
                    if (IsContextClick(e))
                    {
                        vm.NodeCategoryOpenContextMenuCommand.Execute(row.DataContext as RuleTreeNode);
                        e.Handled = true;
                    }
                }, RoutingStrategies.Tunnel);
                return row;
            }),
        };
        AutomationProperties.SetName(listBox, Se.Language.Edit.MultipleReplace.Categories);
        vm.CategoriesListBox = listBox;
        listBox.KeyDown += vm.CategoriesListBox_KeyDown;
        listBox.AddHandler(InputElement.KeyDownEvent, vm.CategoriesListBox_PreviewKeyDown, RoutingStrategies.Tunnel);
        listBox.DoubleTapped += (_, _) => vm.CategoriesListBoxDoubleTapped();
        listBox.AddHandler(InputElement.PointerReleasedEvent, (_, e) =>
        {
            if (IsContextClick(e))
            {
                vm.TreeOpenContextMenuCommand.Execute(null);
                e.Handled = true;
            }
        }, RoutingStrategies.Bubble);

        var buttonNewCategory = EditorChrome.CreateToolButton("fa-solid fa-folder-plus", Se.Language.Edit.MultipleReplace.NewCategory, vm.CategoryAddCategoryCommand, 26);
        var buttonEditCategory = EditorChrome.CreateToolButton("fa-solid fa-pen", Se.Language.Edit.MultipleReplace.EditCategory, vm.EditSelectedCategoryCommand, 26, "F2");
        var buttonDeleteCategory = EditorChrome.CreateToolButton("fa-solid fa-trash-can", Se.Language.General.Delete, vm.DeleteSelectedCategoryCommand, 26);
        var buttonImport = EditorChrome.CreateToolButton("fa-solid fa-file-import", Se.Language.Edit.MultipleReplace.ImportReplaceRules, vm.CategoryImportCommand, 26);
        var buttonMore = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.More, vm.OpenSelectedCategoryMenuCommand, 26);

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Margin = new Thickness(0, 8, 0, 0),
            Children = { buttonNewCategory, buttonEditCategory, buttonDeleteCategory, buttonImport, buttonMore },
        };

        var panel = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
            Margin = new Thickness(14, 12, 10, 14),
        };
        panel.Add(new DockPanel { Margin = new Thickness(0, 0, 0, 8), Children = { EditorChrome.MakeSectionHeader(Se.Language.Edit.MultipleReplace.Categories) } }, 0);
        panel.Add(EditorChrome.MakeListFrame(listBox), 1);
        panel.Add(actions, 2);

        return EditorChrome.MakePanel(panel);
    }

    /// <summary>
    /// The rules of the selected category: a header with "Newest first" and a filter, the type-in
    /// row for new rules, the rules list and the rule actions under it (#16215).
    /// </summary>
    private static Border MakeRulesView(MultipleReplaceViewModel vm, out TextBox textBoxNewFind)
    {
        // --- header ---
        var title = EditorChrome.MakeBoundSectionHeader(nameof(vm.SelectedCategoryTitle));
        title.DataContext = vm;

        var checkBoxNewestFirst = new CheckBox
        {
            Content = Se.Language.Edit.MultipleReplace.NewestFirst,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.NewestFirst)) { Source = vm, Mode = BindingMode.TwoWay },
        };
        var textBoxFilter = EditorChrome.MakeSearchBox(vm, nameof(vm.RulesFilter), Se.Language.Edit.MultipleReplace.FilterRules, 220);

        var headerRight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { checkBoxNewestFirst, textBoxFilter },
        };
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(headerRight, Dock.Right);
        header.Children.Add(headerRight);
        header.Children.Add(title);

        // --- type-in row ---
        var newFind = new TextBox
        {
            PlaceholderText = Se.Language.Edit.MultipleReplace.FindWhat,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.NewFind)) { Source = vm, Mode = BindingMode.TwoWay },
        };
        newFind.Classes.Add("text");
        AutomationProperties.SetName(newFind, Se.Language.Edit.MultipleReplace.FindWhat);
        newFind.KeyDown += vm.NewRuleTextBoxKeyDown;
        vm.NewFindTextBox = newFind;
        textBoxNewFind = newFind;

        var newReplaceWith = new TextBox
        {
            PlaceholderText = Se.Language.General.ReplaceWith,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.NewReplaceWith)) { Source = vm, Mode = BindingMode.TwoWay },
        };
        newReplaceWith.Classes.Add("text");
        AutomationProperties.SetName(newReplaceWith, Se.Language.General.ReplaceWith);
        newReplaceWith.KeyDown += vm.NewRuleTextBoxKeyDown;

        var arrow = new Label { VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(0) };
        Attached.SetIcon(arrow, IconNames.ArrowRightThick);

        var comboBoxNewType = new ComboBox
        {
            Width = 170,
            VerticalAlignment = VerticalAlignment.Center,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.RuleTypes)) { Source = vm },
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.NewRuleType)) { Source = vm, Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(comboBoxNewType, Se.Language.General.Type);

        var checkBoxNewWholeWord = new CheckBox
        {
            Content = Se.Language.Edit.Find.WholeWord,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.NewWholeWord)) { Source = vm, Mode = BindingMode.TwoWay },
            [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.CanUseNewWholeWord)) { Source = vm },
        };

        var buttonAdd = UiUtil.MakeButton(Se.Language.General.Add, vm.AddNewRuleCommand);
        buttonAdd.Classes.Add("primary");
        buttonAdd.VerticalAlignment = VerticalAlignment.Center;
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonAdd, Se.Language.Edit.MultipleReplace.EnterAddsRuleToCategory);
        }

        var entryRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            ColumnSpacing = 8,
            Margin = new Thickness(0, 0, 0, 8),
        };
        entryRow.Add(newFind, 0, 0);
        entryRow.Add(arrow, 0, 1);
        entryRow.Add(newReplaceWith, 0, 2);
        entryRow.Add(comboBoxNewType, 0, 3);
        entryRow.Add(checkBoxNewWholeWord, 0, 4);
        entryRow.Add(buttonAdd, 0, 5);

        // --- rules list ---
        var table = TableViewExtras.MakeTableView(alwaysSelected: false, multiSelect: false);
        table.DataContext = vm;
        table.ItemsSource = vm.VisibleRules;
        table.Columns.AddRange(new TableViewColumn[]
        {
            new SeTableViewColumn
            {
                Header = string.Empty,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<RuleTreeNode>(nameof(RuleTreeNode.IsActive), nameof(RuleTreeNode.Find),
                    configure: checkBox => checkBox.IsCheckedChanged += vm.OnActiveChanged),
                Width = new GridLength(44),
            },
            new SeTableViewColumn
            {
                Header = string.Empty,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = new FuncDataTemplate<RuleTreeNode>((_, _) =>
                {
                    var buttonActions = new Button
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Command = vm.NodeOpenContextMenuCommand,
                        [!Button.CommandParameterProperty] = new Binding("."),
                        Padding = new Thickness(0),
                        Width = 22,
                        Height = 22,
                    };
                    buttonActions.Classes.Add("tool");
                    Attached.SetIcon(buttonActions, IconNames.DotsVertical);
                    AutomationProperties.SetName(buttonActions, Se.Language.General.More);
                    return buttonActions;
                }),
                Width = new GridLength(32),
                IsVisible = vm.IsMultipleReplaceDotDotDotButtonsVisible,
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.NumberSymbol,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(RuleTreeNode.Number)),
                CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(RuleTreeNode.Number)),
                Width = new GridLength(56),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.Edit.MultipleReplace.FindWhat,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                NameBinding = new Binding(nameof(RuleTreeNode.Find)),
                CellTemplate = new FuncDataTemplate<RuleTreeNode>((_, _) => MakeFindCell()),
                Width = new GridLength(1, GridUnitType.Star),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.ReplaceWith,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(RuleTreeNode.ReplaceWith)),
                CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(RuleTreeNode.ReplaceWith)),
                Width = new GridLength(1, GridUnitType.Star),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.Edit.Find.WholeWord,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<RuleTreeNode>(nameof(RuleTreeNode.WholeWord), nameof(RuleTreeNode.Find),
                    configure: checkBox =>
                    {
                        checkBox[!InputElement.IsEnabledProperty] = new Binding(nameof(RuleTreeNode.CanUseWholeWord));
                        checkBox.IsCheckedChanged += vm.OnActiveChanged;
                    }),
                Width = new GridLength(100),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Description,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(RuleTreeNode.Description)),
                CellTemplate = new FuncDataTemplate<RuleTreeNode>((_, _) => new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = EditorChrome.FaintTextBrush,
                    FontStyle = FontStyle.Italic,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    [!TextBlock.TextProperty] = new Binding(nameof(RuleTreeNode.Description)),
                }),
                Width = new GridLength(0.8, GridUnitType.Star),
            },
        });
        AutomationProperties.SetName(table, Se.Language.General.Rules);
        vm.RulesTable = table;
        table.SelectionChanged += (_, _) => vm.RulesTableSelectionChanged();
        table.KeyDown += vm.RulesTable_KeyDown;
        table.AddHandler(InputElement.KeyDownEvent, vm.RulesTable_PreviewKeyDown, RoutingStrategies.Tunnel);
        table.DoubleTapped += (_, e) =>
        {
            if (!TableViewExtras.IsInColumnHeader(e.Source as Visual))
            {
                vm.RulesTableDoubleTapped();
            }
        };
        table.AddHandler(InputElement.PointerReleasedEvent, (_, e) =>
        {
            if (IsContextClick(e) && vm.SelectedNode != null)
            {
                vm.NodeOpenContextMenuCommand.Execute(vm.SelectedNode);
                e.Handled = true;
            }
        }, RoutingStrategies.Bubble);

        // --- rule actions ---
        var buttonRemove = EditorChrome.MakeTextToolButton("fa-solid fa-trash-can", Se.Language.General.Remove, vm.RemoveSelectedRuleCommand);
        var buttonEdit = EditorChrome.MakeTextToolButton("fa-solid fa-pen", Se.Language.General.EditDotDotDot, vm.EditSelectedRuleCommand);
        var buttonMoveTo = EditorChrome.MakeTextToolButton("fa-solid fa-folder-open", Se.Language.Edit.MultipleReplace.MoveToCategory, null);
        buttonMoveTo.Click += (_, _) => vm.OpenMoveToCategoryMenu(buttonMoveTo);
        var buttonUp = EditorChrome.CreateToolButton("fa-solid fa-arrow-up", Se.Language.General.MoveUp, vm.MoveSelectedRuleUpCommand, 26);
        var buttonDown = EditorChrome.CreateToolButton("fa-solid fa-arrow-down", Se.Language.General.MoveDown, vm.MoveSelectedRuleDownCommand, 26);
        buttonUp.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.CanReorderRules)) { Source = vm });
        buttonDown.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.CanReorderRules)) { Source = vm });
        var buttonAddRules = EditorChrome.MakeTextToolButton("fa-solid fa-layer-group", Se.Language.Edit.MultipleReplace.AddRulesDotDotDot, vm.CategoryAddRulesCommand);

        foreach (var button in new Control[] { buttonRemove, buttonEdit, buttonMoveTo, buttonUp, buttonDown })
        {
            button.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsEditPanelVisible)) { Source = vm });
        }

        var ruleActions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Children = { buttonRemove, buttonEdit, buttonMoveTo, buttonUp, buttonDown },
        };
        var actions = new DockPanel { Margin = new Thickness(0, 8, 0, 0), MinHeight = EditorChrome.FieldHeight };
        DockPanel.SetDock(buttonAddRules, Dock.Right);
        actions.Children.Add(buttonAddRules);
        actions.Children.Add(ruleActions);

        var panel = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
            Margin = new Thickness(14, 12, 14, 12),
        };
        panel.Add(header, 0);
        panel.Add(entryRow, 1);
        panel.Add(EditorChrome.MakeListFrame(table), 2);
        panel.Add(actions, 3);

        return EditorChrome.MakePanel(panel);
    }

    /// <summary>
    /// "Find what" with the rule type's icon in front, and a warning marker for a rule that
    /// cannot run - today only a regular expression that will not compile; it is skipped
    /// silently otherwise, which reads as a rule that just does nothing (#13534).
    /// </summary>
    private static Control MakeFindCell()
    {
        var labelError = new Label
        {
            Foreground = ErrorBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 4, 0),
            Padding = new Thickness(0),
            [!Visual.IsVisibleProperty] = new Binding(nameof(RuleTreeNode.HasError)),
        };
        Attached.SetIcon(labelError, IconNames.Alert);
        if (Se.Settings.Appearance.ShowHints)
        {
            labelError[!ToolTip.TipProperty] = new Binding(nameof(RuleTreeNode.ErrorMessage));
        }

        var labelIcon = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0),
            Padding = new Thickness(0),
            Foreground = EditorChrome.DimTextBrush,
        };
        labelIcon.Bind(Attached.IconProperty, new Binding(nameof(RuleTreeNode.IconName)));

        var text = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(RuleTreeNode.Find)),
        };

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { labelError, labelIcon, text },
        };
    }

    /// <summary>
    /// The selected rule's fields on top (only while a rule is selected), the preview of the
    /// fixes under them and the rules that made the selected fix at the bottom.
    /// </summary>
    private static Grid MakeFixesView(MultipleReplaceViewModel vm)
    {
        var textBoxFind = new TextBox
        {
            MinWidth = 100,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SelectedNode) + "." + nameof(RuleTreeNode.Find)) { Source = vm }
        };
        textBoxFind.Classes.Add("text");
        AutomationProperties.SetName(textBoxFind, Se.Language.General.Find);
        textBoxFind.TextChanged += vm.RuleTextChanged;

        var textBoxReplaceWith = new TextBox
        {
            MinWidth = 100,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SelectedNode) + "." + nameof(RuleTreeNode.ReplaceWith)) { Source = vm }
        };
        textBoxReplaceWith.Classes.Add("text");
        AutomationProperties.SetName(textBoxReplaceWith, Se.Language.General.ReplaceWith);
        textBoxReplaceWith.TextChanged += vm.RuleTextChanged;

        var comboBoxType = new ComboBox
        {
            Width = 170,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.RuleTypes)) { Source = vm },
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedRuleType)) { Source = vm, Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(comboBoxType, Se.Language.General.Type);
        // no event handler needed; binding to SelectedRuleType updates RuleTreeNode.Type

        var switchWholeWord = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.SelectedNode) + "." + nameof(RuleTreeNode.WholeWord)) { Source = vm, Mode = BindingMode.TwoWay },
            [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.SelectedNode) + "." + nameof(RuleTreeNode.CanUseWholeWord)) { Source = vm },
        };
        AutomationProperties.SetName(switchWholeWord, Se.Language.Edit.Find.WholeWord);
        switchWholeWord.IsCheckedChanged += vm.OnActiveChanged;
        var panelWholeWord = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                switchWholeWord,
                new TextBlock { Text = Se.Language.Edit.Find.WholeWord, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center },
            },
        };

        // Labels above the fields: find and replace share the width, type and whole word take what they need.
        var editGrid = new Grid
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 8,
            RowSpacing = 4,
        };

        editGrid.Add(EditorChrome.MakeDimLabel(Se.Language.General.Find, new Thickness(0)), 0, 0);
        editGrid.Add(textBoxFind, 1, 0);

        editGrid.Add(EditorChrome.MakeDimLabel(Se.Language.General.ReplaceWith, new Thickness(0)), 0, 1);
        editGrid.Add(textBoxReplaceWith, 1, 1);

        editGrid.Add(EditorChrome.MakeDimLabel(Se.Language.General.Type, new Thickness(0)), 0, 2);
        editGrid.Add(comboBoxType, 1, 2);

        editGrid.Add(panelWholeWord, 1, 3);

        var editSection = EditorChrome.MakeEditorSection(Se.Language.Edit.MultipleReplace.EditRule, editGrid, first: true);
        editSection.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsEditPanelVisible)) { Source = vm });
        editSection.Background = EditorChrome.PanelBrush;
        editSection.BorderBrush = EditorChrome.LineBrush;
        editSection.BorderThickness = new Thickness(0, 0, 0, 1);

        // No header sorting (the DataGrid's CanUserSortColumns is not carried over):
        // this is a fix preview in subtitle order, and the replace rules themselves run
        // in list order - reordering the backing collection would scramble the preview.
        // Multi-select so a range picked with Shift+click can have its "Apply" checkbox flipped
        // in one go with Space (#13502); the fix detail panel below still follows SelectedItem.
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Fixes;
        AutomationProperties.SetName(dataGrid, Se.Language.General.Preview);
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.Apply,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<MultipleReplaceFix>(nameof(MultipleReplaceFix.Apply), focusable: true),
                // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                Width = new GridLength(70)
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.NumberSymbol,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(MultipleReplaceFix.Number)),
                Width = new GridLength(60),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Before,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = new FuncDataTemplate<MultipleReplaceFix>((item, _) =>
                {
                    var (beforeBlock, _) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                    return new Border
                    {
                        Background = Brushes.Transparent,
                        Padding = new Thickness(4),
                        Child = beforeBlock,
                    };
                }),
                Width = new GridLength(1, GridUnitType.Star),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.After,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = new FuncDataTemplate<MultipleReplaceFix>((item, _) =>
                {
                    var (_, afterBlock) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                    return new Border
                    {
                        Background = Brushes.Transparent,
                        Padding = new Thickness(4),
                        Child = afterBlock,
                    };
                }),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedFix)) { Source = vm });

        TableViewExtras.AddSpaceToggle<MultipleReplaceFix>(dataGrid,
            item => item.Apply, (item, v) => item.Apply = v);

        dataGrid.ContextMenu = MakeFixesContextMenu(vm);

        // Tunneling: the TableView (a ListBox) would otherwise take Ctrl+A as "select all rows",
        // and the window's key handler takes Ctrl+D as "duplicate rule" (#13502).
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (vm.HandleFixesSelectionKey(e))
            {
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        var hitsItemsControl = new ItemsControl
        {
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.SelectedFixHits)) { Source = vm },
            ItemTemplate = new FuncDataTemplate<ReplaceExpression>((hit, _) =>
            {
                var redFg = ErrorBrush;
                var greenFg = ReplacedBrush;

                var iconLabel = new Label { Padding = new Thickness(0, 0, 2, 0), VerticalAlignment = VerticalAlignment.Center };
                if (hit.RuleTreeNode != null)
                {
                    Attached.SetIcon(iconLabel, hit.RuleTreeNode.IconName);
                }

                var findBlock = new TextBlock
                {
                    Text = hit.FindWhat,
                    Foreground = redFg,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = new FontFamily("Consolas"),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                };

                var arrowLabel = UiUtil.MakeLabel("→");
                arrowLabel.Foreground = EditorChrome.FaintTextBrush;
                arrowLabel.Margin = new Thickness(4, 0, 4, 0);
                arrowLabel.VerticalAlignment = VerticalAlignment.Center;
                arrowLabel.Padding = new Thickness(0);

                var replaceBlock = new TextBlock
                {
                    Text = hit.ReplaceWith,
                    Foreground = greenFg,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = new FontFamily("Consolas"),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                };

                var row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Spacing = 0,
                    Children = { iconLabel, findBlock, arrowLabel, replaceBlock },
                };

                if (!string.IsNullOrEmpty(hit.RuleTreeNode?.Description))
                {
                    var descBlock = new TextBlock
                    {
                        Text = "  · " + hit.RuleTreeNode.Description,
                        Foreground = EditorChrome.FaintTextBrush,
                        FontStyle = FontStyle.Italic,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                    };
                    row.Children.Add(descBlock);
                }

                var chip = new Border
                {
                    Background = EditorChrome.HoverBrush,
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(8, 3, 8, 3),
                    Margin = new Thickness(0, 1, 0, 1),
                    Child = row,
                    Cursor = hit.RuleTreeNode != null ? new Cursor(StandardCursorType.Hand) : null,
                };
                if (Se.Settings.Appearance.ShowHints)
                {
                    ToolTip.SetTip(chip, hit.RuleInfo);
                }
                if (hit.RuleTreeNode != null)
                {
                    chip.Tapped += (_, _) => vm.NavigateToRule(hit.RuleTreeNode);
                }
                return chip;
            }),
        };

        var hitsPanel = new StackPanel
        {
            Margin = new Thickness(0, 10, 0, 0),
            Spacing = 6,
            Children =
            {
                EditorChrome.MakeSectionHeader(Se.Language.Edit.MultipleReplace.AppliedRules),
                new ScrollViewer
                {
                    Content = hitsItemsControl,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    MaxHeight = 120,
                },
            },
        }.WithBindVisible(vm, nameof(vm.IsFixDetailPanelVisible));

        // Tick all / untick all / invert, also on the preview's context menu (#13502).
        var buttonSelectAll = EditorChrome.CreateToolButton("fa-solid fa-square-check", Se.Language.General.SelectAll, vm.SelectAllFixesCommand, 26);
        var buttonSelectNone = EditorChrome.CreateToolButton("fa-regular fa-square", Se.Language.General.SelectNone, vm.SelectNoFixesCommand, 26);
        var buttonInvert = EditorChrome.CreateToolButton("fa-solid fa-right-left", Se.Language.General.InvertSelection, vm.InvertFixesSelectionCommand, 26);
        var previewActions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonSelectAll, buttonSelectNone, buttonInvert },
        };
        var previewHeader = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(previewActions, Dock.Right);
        previewHeader.Children.Add(previewActions);
        previewHeader.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Preview));

        var gridPreview = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = new Thickness(14, 12, 14, 14),
        };
        gridPreview.Add(previewHeader, 0, 0);
        gridPreview.Add(EditorChrome.MakeListFrame(dataGrid), 1, 0);
        gridPreview.Add(hitsPanel, 2, 0);

        var gridFixes = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
        };
        gridFixes.Add(editSection, 0, 0);
        gridFixes.Add(gridPreview, 1, 0);

        return gridFixes;
    }

    // A preview with one row per changed line is far too long to untick by hand, so offer
    // tick all / untick all / invert with the gestures the sibling lists advertise (#13502).
    private static ContextMenu MakeFixesContextMenu(MultipleReplaceViewModel vm)
    {
        var commandModifier = System.OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;

        return new ContextMenu
        {
            Items =
            {
                new Avalonia.Controls.MenuItem
                {
                    Header = Se.Language.General.SelectAll,
                    DataContext = vm,
                    Command = vm.SelectAllFixesCommand,
                    InputGesture = new KeyGesture(Key.A, commandModifier),
                },
                new Avalonia.Controls.MenuItem
                {
                    Header = Se.Language.General.SelectNone,
                    DataContext = vm,
                    Command = vm.SelectNoFixesCommand,
                    InputGesture = new KeyGesture(Key.D, commandModifier),
                },
                new Avalonia.Controls.MenuItem
                {
                    Header = Se.Language.General.InvertSelection,
                    DataContext = vm,
                    Command = vm.InvertFixesSelectionCommand,
                    InputGesture = new KeyGesture(Key.I, commandModifier | KeyModifiers.Shift),
                },
            },
        };
    }
}