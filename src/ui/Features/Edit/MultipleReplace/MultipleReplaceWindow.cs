using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class MultipleReplaceWindow : Window
{
    public MultipleReplaceWindow(MultipleReplaceViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Edit.MultipleReplace.Title;
        Width = 1110;
        Height = 740;
        MinWidth = 850;
        MinHeight = 400;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var rulesView = MakeRulesView(vm);
        var fixesView = MakeFixesView(vm);

        var buttonCollapseAll = UiUtil.MakeButton(vm.CollapseAllCommand, IconNames.Minus);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonCollapseAll, Se.Language.General.Collapse);
        }

        var buttonExpandAll = UiUtil.MakeButton(vm.ExpandAllCommand, IconNames.Plus);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonExpandAll, Se.Language.General.Expand);
        }

        var panelExpandCollapse = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                buttonCollapseAll,
                buttonExpandAll,
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 300, },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // splitter
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star), MinWidth = 300, },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 2,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        // Create the vertical splitter
        var splitter = new GridSplitter
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch,
            ResizeDirection = GridResizeDirection.Columns,
            Margin = new Thickness(0, 0, 0, 10),
        };

        grid.Add(rulesView, 0, 0);
        grid.Add(splitter, 0, 1);
        grid.Add(fixesView, 0, 2);
        grid.Add(panelExpandCollapse, 1, 0);
        grid.Add(panelButtons, 1, 0, 1, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        Closing += (_, _) => vm.OnClosing();
        Loaded += (_, _) => vm.OnLoaded();
        KeyDown += vm.OnKeyDown;
    }

    private static Border MakeRulesView(MultipleReplaceViewModel vm)
    {
        var treeView = new TreeView
        {
            SelectionMode = SelectionMode.Single,
            DataContext = vm,
            MinWidth = 300,
        };

        treeView[!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Nodes));
        treeView[!TreeView.SelectedItemProperty] = new Binding(nameof(vm.SelectedNode));

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

            if (node.IsCategory)
            {
                var label = UiUtil.MakeLabel(string.Empty).WithBindText(node, nameof(RuleTreeNode.CategoryName));
                label.FontWeight = FontWeight.Bold;
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
                    Padding = new Thickness(4),
                    DataContext = vm,
                };
                buttonCategoryActions.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsMultipleReplaceDotDotDotButtonsVisible)));
                Attached.SetIcon(buttonCategoryActions, IconNames.DotsVertical);

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

            var buttonActions = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Command = vm.NodeOpenContextMenuCommand,
                CommandParameter = node,
                Margin = new Thickness(5, 0, 0, 0),
                Padding = new Thickness(4),
                DataContext = vm,
            };
            buttonActions.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsMultipleReplaceDotDotDotButtonsVisible)));
            Attached.SetIcon(buttonActions, IconNames.DotsVertical);

            var labelDescription = UiUtil.MakeLabel().WithBindText(node, nameof(RuleTreeNode.Description));
            labelDescription.Opacity = 0.6;
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
        treeView.DoubleTapped += (_, e) => vm.TreeViewDoubleTapped(e);

        var scrollViewer = new ScrollViewer
        {
            Content = treeView,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };

        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderBrush(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = scrollViewer,
        };

        return border;
    }

    private static Border MakeFixesView(MultipleReplaceViewModel vm)
    {
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = new Thickness(0, 0, 0, 10),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 5,
        }.WithBindVisible(vm, nameof(vm.IsEditPanelVisible));

        var labelFind = UiUtil.MakeLabel(Se.Language.General.Find);
        var textBoxFind = new TextBox
        {
            MinWidth = 100,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SelectedNode) + "." + nameof(RuleTreeNode.Find)) { Source = vm }
        };
        textBoxFind.TextChanged += vm.RuleTextChanged;

        var labelReplaceWith = UiUtil.MakeLabel(Se.Language.General.ReplaceWith);
        var textBoxReplaceWith = new TextBox
        {
            Width = 190,
            MinWidth = 100,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SelectedNode) + "." + nameof(RuleTreeNode.ReplaceWith)) { Source = vm }
        };
        textBoxReplaceWith.TextChanged += vm.RuleTextChanged;

        var labelType = UiUtil.MakeLabel(Se.Language.General.Type);
        var comboBoxType = new ComboBox
        {
            Width = 190,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.RuleTypes)) { Source = vm },
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedRuleType)) { Source = vm, Mode = BindingMode.TwoWay },
        };
        // no event handler needed; binding to SelectedRuleType updates RuleTreeNode.Type

        editGrid.Add(labelFind, 0, 0);
        editGrid.Add(textBoxFind, 1, 0);

        editGrid.Add(labelReplaceWith, 0, 1);
        editGrid.Add(textBoxReplaceWith, 1, 1);

        editGrid.Add(labelType, 0, 2);
        editGrid.Add(comboBoxType, 1, 2);

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
            ItemsSource = vm.Fixes,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<MultipleReplaceFix>((item, _) =>
                        new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = new CheckBox
                            {
                                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(MultipleReplaceFix.Apply)),
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MultipleReplaceFix.Number)),
                    IsReadOnly = true,
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
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
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
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
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedFix)) { Source = vm });

        var hitsItemsControl = new ItemsControl
        {
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.SelectedFixHits)) { Source = vm },
            ItemTemplate = new FuncDataTemplate<ReplaceExpression>((hit, _) =>
            {
                var isDark = UiTheme.IsDarkThemeEnabled();
                var redFg = new SolidColorBrush(isDark ? Color.FromRgb(255, 100, 100) : Color.FromRgb(183, 28, 28));
                var greenFg = new SolidColorBrush(isDark ? Color.FromRgb(100, 220, 100) : Color.FromRgb(27, 94, 32));
                var chipBg = new SolidColorBrush(isDark ? Color.FromArgb(40, 200, 200, 200) : Color.FromArgb(25, 0, 0, 0));

                var iconLabel = new Label { Padding = new Thickness(0, 0, 2, 0), VerticalAlignment = VerticalAlignment.Center };
                if (hit.RuleTreeNode != null)
                    Attached.SetIcon(iconLabel, hit.RuleTreeNode.IconName);

                var findBlock = new TextBlock
                {
                    Text = hit.FindWhat,
                    Foreground = redFg,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = new FontFamily("Consolas"),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                };

                var arrowLabel = UiUtil.MakeLabel("→");
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
                        Opacity = 0.6,
                        FontStyle = FontStyle.Italic,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                    };
                    row.Children.Add(descBlock);
                }

                var chip = new Border
                {
                    Background = chipBg,
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(6, 2, 6, 2),
                    Margin = new Thickness(0, 1, 0, 1),
                    Child = row,
                    Cursor = hit.RuleTreeNode != null ? new Cursor(StandardCursorType.Hand) : null,
                };
                ToolTip.SetTip(chip, hit.RuleInfo);
                if (hit.RuleTreeNode != null)
                {
                    chip.Tapped += (_, _) => vm.NavigateToRule(hit.RuleTreeNode);
                }
                return chip;
            }),
        };

        var hitsPanel = new StackPanel
        {
            Margin = new Thickness(0, 4, 0, 0),
            Children =
            {
                new TextBlock
                {
                    Text = Se.Language.Edit.MultipleReplace.AppliedRules,
                    FontWeight = FontWeight.SemiBold,
                    Margin = new Thickness(0, 0, 0, 2),
                },
                new ScrollViewer
                {
                    Content = hitsItemsControl,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    MaxHeight = 120,
                },
            },
        }.WithBindVisible(vm, nameof(vm.IsFixDetailPanelVisible));

        var gridFixes = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };

        var labelFixesInfo = new TextBlock
        {
            Margin = new Thickness(0, 0, 0, 4),
            Opacity = 0.7,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.FixesInfo)) { Source = vm },
        };

        gridFixes.Add(editGrid, 0, 0);
        gridFixes.Add(dataGrid, 1, 0);
        gridFixes.Add(labelFixesInfo, 2, 0);
        gridFixes.Add(hitsPanel, 3, 0);

        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderBrush(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = gridFixes,
        };

        return border;
    }
}