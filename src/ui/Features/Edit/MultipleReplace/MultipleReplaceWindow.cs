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
        Width = 1110;
        Height = 740;
        MinWidth = 850;
        MinHeight = 400;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var rulesView = MakeRulesView(vm, out var rulesTreeView);
        var fixesView = MakeFixesView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        // How many lines the checked fixes change, on the left of the button bar.
        var labelFixesInfo = EditorChrome.MakeBoundHint(nameof(vm.FixesInfo));
        labelFixesInfo.FontSize = 12;
        var panelButtons = EditorChrome.MakeButtonBar(labelFixesInfo, buttonApply, buttonCancel, buttonOk);

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 300, },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // splitter
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star), MinWidth = 300, },
            },
        };

        // The splitter doubles as the line between the rules and the preview.
        var splitter = new GridSplitter
        {
            Width = 5,
            Background = EditorChrome.LineBrush,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ResizeDirection = GridResizeDirection.Columns,
        };

        content.Add(rulesView, 0, 0);
        content.Add(splitter, 0, 1);
        content.Add(fixesView, 0, 2);

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

        UiUtil.FocusOnFirstActivation(this, rulesTreeView); // initial focus on an input, not an action button - a focused button clicks on bare Space
        Closing += (_, _) => vm.OnClosing();
        Loaded += (_, _) => vm.OnLoaded();
        KeyDown += vm.OnKeyDown;
    }

    /// <summary>
    /// The rules pane: a "Rules" header with the tree actions, the category/rule tree under it.
    /// </summary>
    private static Border MakeRulesView(MultipleReplaceViewModel vm, out TreeView rulesTreeView)
    {
        var treeView = new TreeView
        {
            SelectionMode = SelectionMode.Single,
            DataContext = vm,
            MinWidth = 300,

            // Expanded/collapsed lives on the node, not on the container: the containers only
            // exist while they are realized, so reading the expansion state back off them at save
            // time would record any category the tree had not realized as collapsed.
            ItemContainerTheme = new ControlTheme(typeof(TreeViewItem))
            {
                BasedOn = Application.Current?.FindResource(typeof(TreeViewItem)) as ControlTheme,
                Setters =
                {
                    new Setter(TreeViewItem.IsExpandedProperty,
                        new Binding(nameof(RuleTreeNode.IsExpanded)) { Mode = BindingMode.TwoWay }),
                },
            },
        };

        treeView[!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Nodes));
        treeView[!TreeView.SelectedItemProperty] = new Binding(nameof(vm.SelectedNode));

        rulesTreeView = treeView;

        var factory = new FuncTreeDataTemplate<RuleTreeNode>(_ => true, (node, _) =>
        {
            var checkBox = new CheckBox
            {
                DataContext = node,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0),
                Padding = new Thickness(0)
            };
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(RuleTreeNode.IsActive))
            {
                Mode = BindingMode.TwoWay,
                Source = node,
            });
            checkBox.IsCheckedChanged += vm.OnActiveChanged;
            // The row text lives in separate labels; name the box after the category or rule (#12087).
            checkBox.Bind(AutomationProperties.NameProperty,
                new Binding(node.IsCategory ? nameof(RuleTreeNode.CategoryName) : nameof(RuleTreeNode.Find)) { Source = node });

            if (node.IsCategory)
            {
                var label = UiUtil.MakeLabel(string.Empty).WithBindText(node, nameof(RuleTreeNode.CategoryName));
                label.FontWeight = FontWeight.SemiBold;
                label.VerticalAlignment = VerticalAlignment.Center;
                label.VerticalContentAlignment = VerticalAlignment.Center;
                label.Margin = new Thickness(5, 0, 0, 0);
                label.Padding = new Thickness(0);

                var buttonCategoryActions = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Command = vm.NodeCategoryOpenContextMenuCommand,
                    CommandParameter = node,
                    Margin = new Thickness(5, 0, 0, 0),
                    Padding = new Thickness(0),
                    Width = 22,
                    Height = 22,
                    DataContext = vm,
                };
                buttonCategoryActions.Classes.Add("tool");
                buttonCategoryActions.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsMultipleReplaceDotDotDotButtonsVisible)));
                Attached.SetIcon(buttonCategoryActions, IconNames.DotsVertical);
                AutomationProperties.SetName(buttonCategoryActions, Se.Language.General.More);

                var panelCategory = new DockPanel
                {
                    LastChildFill = true,
                    Width = double.NaN,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = Brushes.Transparent,
                    Margin = new Thickness(0),
                };
                DockPanel.SetDock(checkBox, Dock.Left);
                DockPanel.SetDock(buttonCategoryActions, Dock.Left);
                panelCategory.Children.Add(checkBox);
                panelCategory.Children.Add(buttonCategoryActions);
                panelCategory.Children.Add(label);
                panelCategory.AddHandler(InputElement.PointerReleasedEvent, (sender, e) =>
                {
                    var isContextClick = e.InitialPressMouseButton == MouseButton.Right ||
                        (System.OperatingSystem.IsMacOS() && e.InitialPressMouseButton == MouseButton.Left && e.KeyModifiers.HasFlag(KeyModifiers.Control));
                    if (isContextClick)
                    {
                        vm.NodeCategoryOpenContextMenuCommand.Execute(node);
                        e.Handled = true;
                    }
                }, RoutingStrategies.Tunnel);

                return panelCategory;
            }

            var panelRow = new DockPanel
            {
                LastChildFill = true,
                Width = double.NaN,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0),
            };

            var labelFind = UiUtil.MakeLabel(string.Empty).WithBindText(node, nameof(RuleTreeNode.Find));
            labelFind.VerticalAlignment = VerticalAlignment.Center;
            labelFind.VerticalContentAlignment = VerticalAlignment.Center;
            labelFind.Margin = new Thickness(0);
            labelFind.Padding = new Thickness(0);

            var labelSeparator = UiUtil.MakeLabel(string.Empty);
            Attached.SetIcon(labelSeparator, IconNames.ArrowRightThick);
            labelSeparator.VerticalAlignment = VerticalAlignment.Center;
            labelSeparator.VerticalContentAlignment = VerticalAlignment.Center;
            labelSeparator.Margin = new Thickness(2, 0, 2, 0);
            labelSeparator.Padding = new Thickness(0);

            var labelReplaceWith = UiUtil.MakeLabel(string.Empty).WithBindText(node, nameof(RuleTreeNode.ReplaceWith));
            labelReplaceWith.VerticalAlignment = VerticalAlignment.Center;
            labelReplaceWith.VerticalContentAlignment = VerticalAlignment.Center;
            labelReplaceWith.Margin = new Thickness(0);
            labelReplaceWith.Padding = new Thickness(0);

            var labelIcon = new Label();
            Attached.SetIcon(labelIcon, node.IconName);
            labelIcon.VerticalAlignment = VerticalAlignment.Center;
            labelIcon.VerticalContentAlignment = VerticalAlignment.Center;
            labelIcon.Margin = new Thickness(5, 0, 2, 0);
            labelIcon.Padding = new Thickness(0);

            // A rule the preview cannot run - today only a regular expression that will not
            // compile. It is skipped silently otherwise, which reads as a rule that just does
            // nothing (#13534).
            var labelError = new Label
            {
                Foreground = ErrorBrush,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0),
                Padding = new Thickness(0),
            };
            Attached.SetIcon(labelError, IconNames.Alert);
            labelError.Bind(Visual.IsVisibleProperty, new Binding(nameof(RuleTreeNode.HasError)) { Source = node });
            if (Se.Settings.Appearance.ShowHints)
            {
                labelError.Bind(ToolTip.TipProperty, new Binding(nameof(RuleTreeNode.ErrorMessage)) { Source = node });
            }

            var buttonActions = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Command = vm.NodeOpenContextMenuCommand,
                CommandParameter = node,
                Margin = new Thickness(5, 0, 0, 0),
                Padding = new Thickness(0),
                Width = 22,
                Height = 22,
                DataContext = vm,
            };
            buttonActions.Classes.Add("tool");
            buttonActions.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsMultipleReplaceDotDotDotButtonsVisible)));
            Attached.SetIcon(buttonActions, IconNames.DotsVertical);
            AutomationProperties.SetName(buttonActions, Se.Language.General.More);

            var labelDescription = UiUtil.MakeLabel().WithBindText(node, nameof(RuleTreeNode.Description));
            labelDescription.Foreground = EditorChrome.FaintTextBrush;
            labelDescription.FontStyle = FontStyle.Italic;
            labelDescription.VerticalAlignment = VerticalAlignment.Center;
            labelDescription.VerticalContentAlignment = VerticalAlignment.Center;
            labelDescription.Margin = new Thickness(7, 0, 0, 0);
            labelDescription.Padding = new Thickness(0);

            var panelFindAndReplaceWith = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 0),
                Children =
                {
                    labelError,
                    labelIcon,
                    labelFind,
                    labelSeparator,
                    labelReplaceWith,
                    labelDescription,
                }
            };

            DockPanel.SetDock(checkBox, Dock.Left);
            DockPanel.SetDock(buttonActions, Dock.Left);
            panelRow.Children.Add(checkBox);
            panelRow.Children.Add(buttonActions);
            panelRow.Children.Add(new Border { ClipToBounds = true, Background = Brushes.Transparent, Child = panelFindAndReplaceWith });
            panelRow.AddHandler(InputElement.PointerReleasedEvent, (sender, e) =>
            {
                var isContextClick = e.InitialPressMouseButton == MouseButton.Right ||
                    (System.OperatingSystem.IsMacOS() && e.InitialPressMouseButton == MouseButton.Left && e.KeyModifiers.HasFlag(KeyModifiers.Control));
                if (isContextClick)
                {
                    vm.NodeOpenContextMenuCommand.Execute(node);
                    e.Handled = true;
                }
            }, RoutingStrategies.Tunnel);

            return panelRow;
        },
            node => node.SubNodes ?? []
        );

        treeView.ItemTemplate = factory;
        vm.RulesTreeView = treeView;
        treeView.SelectionChanged += vm.RulesTreeView_SelectionChanged;
        treeView.KeyDown += vm.RulesTreeView_KeyDown;
        treeView.AddHandler(InputElement.KeyDownEvent, vm.RulesTreeView_PreviewKeyDown, RoutingStrategies.Tunnel);
        treeView.DoubleTapped += (_, e) => vm.TreeViewDoubleTapped(e);
        treeView.AddHandler(InputElement.PointerReleasedEvent, (_, e) =>
        {
            var isContextClick = e.InitialPressMouseButton == MouseButton.Right ||
                (System.OperatingSystem.IsMacOS() && e.InitialPressMouseButton == MouseButton.Left && e.KeyModifiers.HasFlag(KeyModifiers.Control));
            if (isContextClick)
            {
                var node = vm.SelectedNode;
                if (node == null)
                {
                    vm.TreeOpenContextMenuCommand.Execute(null);
                }
                else if (node.IsCategory)
                {
                    vm.NodeCategoryOpenContextMenuCommand.Execute(node);
                }
                else
                {
                    vm.NodeOpenContextMenuCommand.Execute(node);
                }
                e.Handled = true;
            }
        }, RoutingStrategies.Bubble);

        AutomationProperties.SetName(treeView, Se.Language.General.Rules);

        var scrollViewer = new ScrollViewer
        {
            Content = treeView,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Padding = new Thickness(4),
        };

        // New category and import were only on the right-click menu of the empty tree area, and
        // "New rule" only on the context menus (the "..." row buttons are off by default) - so a
        // rule took a right-click each (#16215).
        var buttonNewCategory = EditorChrome.CreateToolButton("fa-solid fa-folder-plus", Se.Language.Edit.MultipleReplace.NewCategory, vm.CategoryAddCategoryCommand, 26);
        var buttonNewRule = EditorChrome.CreateToolButton("fa-solid fa-circle-plus", Se.Language.Edit.MultipleReplace.NewRule, vm.AddRuleCommand, 26);
        var buttonAddRules = EditorChrome.CreateToolButton("fa-solid fa-layer-group", Se.Language.Edit.MultipleReplace.AddRulesDotDotDot, vm.CategoryAddRulesCommand, 26);
        var buttonImport = EditorChrome.CreateToolButton("fa-solid fa-file-import", Se.Language.Edit.MultipleReplace.ImportReplaceRules, vm.CategoryImportCommand, 26);
        var buttonCollapseAll = EditorChrome.CreateToolButton("fa-solid fa-minus", Se.Language.General.Collapse, vm.CollapseAllCommand, 26);
        var buttonExpandAll = EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.Expand, vm.ExpandAllCommand, 26);

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        actions.Children.Add(buttonNewCategory);
        actions.Children.Add(buttonNewRule);
        actions.Children.Add(buttonAddRules);
        actions.Children.Add(buttonImport);
        actions.Children.Add(EditorChrome.MakeToolbarSeparator());
        actions.Children.Add(buttonCollapseAll);
        actions.Children.Add(buttonExpandAll);

        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(actions, Dock.Right);
        header.Children.Add(actions);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Rules));

        var panel = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
            Margin = new Thickness(14, 12, 14, 14),
        };
        panel.Add(header, 0);
        panel.Add(EditorChrome.MakeListFrame(scrollViewer), 1);

        return EditorChrome.MakePanel(panel);
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