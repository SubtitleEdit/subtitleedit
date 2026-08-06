using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Tools.GrammarCheck;

public class GrammarCheckWindow : Window
{
    public GrammarCheckWindow(GrammarCheckViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.GrammarCheck.Title;
        Width = 1024;
        Height = 720;
        MinWidth = 800;
        MinHeight = 500;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Tools.GrammarCheck;

        // ---------- toolbar ----------
        var textBoxServer = UiUtil.MakeTextBox(260, vm, nameof(vm.ServerUrl))
            .WithAccessibleName(l.Server);
        ToolTip.SetTip(textBoxServer, l.ServerHint);

        var buttonRefresh = UiUtil.MakeButton(vm.RefreshLanguagesCommand, IconNames.Refresh)
            .WithAccessibleName(l.TestConnection);
        ToolTip.SetTip(buttonRefresh, l.TestConnection);

        var comboLanguage = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage))
            .WithAccessibleName(Se.Language.General.Language);
        comboLanguage.MinWidth = 190;

        var checkBoxPicky = UiUtil.MakeCheckBox(l.Picky, vm, nameof(vm.IsPicky));
        ToolTip.SetTip(checkBoxPicky, l.PickyHint);

        var buttonSettings = UiUtil.MakeButton(Se.Language.General.Settings, vm.ShowSettingsCommand)
            .WithIconLeft(IconNames.Settings);

        var toolbar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,*,Auto"),
            ColumnSpacing = 8,
        };
        toolbar.Add(UiUtil.MakeTextBlock(l.Server), 0, 0);
        toolbar.Add(textBoxServer, 0, 1);
        toolbar.Add(buttonRefresh, 0, 2);
        toolbar.Add(UiUtil.MakeTextBlock(Se.Language.General.Language).WithMarginLeft(6), 0, 3);
        toolbar.Add(comboLanguage, 0, 4);
        toolbar.Add(checkBoxPicky, 0, 5);
        toolbar.Add(buttonSettings, 0, 7);

        // ---------- filter chips ----------
        var chipsItems = new ItemsControl
        {
            ItemsSource = vm.FilterChips,
            ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 }),
            ItemTemplate = new FuncDataTemplate<ReviewFilterChip>((chip, _) =>
            {
                var content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                if (chip?.Category != null)
                {
                    content.Children.Add(new Avalonia.Controls.Shapes.Ellipse
                    {
                        Width = 7,
                        Height = 7,
                        VerticalAlignment = VerticalAlignment.Center,
                        Fill = ReviewSuggestionItem.GetBrushForCategory(chip.Category.Value),
                    });
                }

                var chipText = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
                chipText.Bind(TextBlock.TextProperty, new Binding(nameof(ReviewFilterChip.Display)));
                content.Children.Add(chipText);

                var toggle = new ToggleButton
                {
                    Padding = new Thickness(10, 3),
                    CornerRadius = new CornerRadius(12),
                    Command = vm.SetFilterCommand,
                    CommandParameter = chip,
                    Content = content,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(ReviewFilterChip.IsActive)),
                };
                AutomationProperties.SetName(toggle, chip?.Label ?? string.Empty);
                return toggle;
            }),
        };

        // ---------- issues grid ----------
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.Suggestions,
            IsReadOnly = false,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<GrammarCheckSuggestionItem>((item, _) => new Border
                    {
                        Background = Brushes.Transparent,
                        Padding = new Thickness(4),
                        Child = new CheckBox
                        {
                            Focusable = false,
                            // Rules without a replacement are information only - nothing to tick.
                            IsEnabled = item?.CanApply ?? false,
                            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(GrammarCheckSuggestionItem.IsSelected)),
                            [!AutomationProperties.NameProperty] = new Binding(nameof(GrammarCheckSuggestionItem.CategoryDisplay)),
                            HorizontalAlignment = HorizontalAlignment.Center,
                        },
                    }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(GrammarCheckSuggestionItem.Number)),
                    IsReadOnly = true,
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.Tools.FixCommonErrors.Action,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<GrammarCheckSuggestionItem>((item, _) =>
                    {
                        var panel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 6,
                            VerticalAlignment = VerticalAlignment.Center,
                        };
                        if (item != null)
                        {
                            panel.Children.Add(new Border
                            {
                                Background = item.CategoryBackgroundBrush,
                                CornerRadius = new CornerRadius(5),
                                Padding = new Thickness(7, 2),
                                VerticalAlignment = VerticalAlignment.Center,
                                Child = new StackPanel
                                {
                                    Orientation = Orientation.Horizontal,
                                    Spacing = 5,
                                    Children =
                                    {
                                        new Optris.Icons.Avalonia.Icon
                                        {
                                            Value = item.CategoryIconName,
                                            FontSize = 13,
                                            Foreground = item.CategoryBrush,
                                            VerticalAlignment = VerticalAlignment.Center,
                                        },
                                        new TextBlock
                                        {
                                            Text = item.CategoryDisplay,
                                            FontSize = 12,
                                            Foreground = item.CategoryBrush,
                                            VerticalAlignment = VerticalAlignment.Center,
                                        },
                                    },
                                },
                            });
                        }

                        return new Border
                        {
                            Background = Brushes.Transparent,
                            Padding = new Thickness(4),
                            Child = panel,
                        };
                    }),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
                new DataGridTextColumn
                {
                    Header = l.Issue,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(GrammarCheckSuggestionItem.IssueDisplay)),
                    IsReadOnly = true,
                    Width = new DataGridLength(0.9, DataGridLengthUnitType.Star),
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<GrammarCheckSuggestionItem>((item, _) => MakeDiffCell(item, showAfter: false)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<GrammarCheckSuggestionItem>((item, _) => MakeDiffCell(item, showAfter: true)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        AutomationProperties.SetName(dataGrid, l.Title);
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedSuggestion)));
        _ = new DataGridCheckboxMultiSelect<GrammarCheckSuggestionItem>(dataGrid,
            item => item.IsSelected,
            (item, value) => item.IsSelected = value,
            item => item.CanApply);

        var borderGrid = UiUtil.MakeBorderForControlNoPadding(dataGrid);

        // ---------- progress ----------
        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Height = 6,
            VerticalAlignment = VerticalAlignment.Center,
            [!RangeBase.ValueProperty] = new Binding(nameof(vm.ProgressValue)),
        };
        var statusText = MakeBoundTextBlock(nameof(vm.StatusText));
        statusText.Opacity = 0.8;

        var progressRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"),
            ColumnSpacing = 10,
        };
        progressRow.Add(new Optris.Icons.Avalonia.Icon
        {
            Value = "mdi-spellcheck",
            FontSize = 15,
            Opacity = 0.8,
            VerticalAlignment = VerticalAlignment.Center,
        }, 0, 0);
        progressRow.Add(statusText, 0, 1);
        progressRow.Add(progressBar, 0, 2);

        // ---------- message strip ----------
        var messageTextBlock = MakeBoundTextBlock(nameof(vm.MessageText));
        messageTextBlock.Opacity = 0.8;
        messageTextBlock.TextWrapping = TextWrapping.Wrap;

        // Only shown when LanguageTool offers more than one way to fix the selected issue.
        var comboReplacement = UiUtil.MakeComboBox(vm.ReplacementOptions, vm, nameof(vm.SelectedReplacementOption),
                nameof(vm.HasReplacementOptions))
            .WithAccessibleName(l.Replacement);
        comboReplacement.MinWidth = 160;

        var messageStrip = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7,
            MinHeight = 20,
            Children =
            {
                new Optris.Icons.Avalonia.Icon
                {
                    Value = "mdi-message-text-outline",
                    FontSize = 14,
                    Opacity = 0.7,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                messageTextBlock,
                comboReplacement,
            },
        };
        messageStrip.Bind(IsVisibleProperty, new Binding(nameof(vm.HasMessage)));

        // ---------- bottom bar ----------
        var summaryText = MakeBoundTextBlock(nameof(vm.SummaryText));
        summaryText.Opacity = 0.8;

        var leftButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                summaryText.WithMarginRight(10),
                UiUtil.MakeButton(Se.Language.General.SelectAll, vm.SelectAllCommand),
                UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.InvertSelectionCommand),
            },
        };

        var buttonCheck = UiUtil.MakeButton(l.Check, vm.CheckCommand)
            .WithIconLeft("fa-solid fa-spell-check");
        buttonCheck.Bind(IsVisibleProperty, new Binding(nameof(vm.IsNotChecking)));

        var buttonStop = UiUtil.MakeButton(l.Stop, vm.StopCheckCommand)
            .WithIconLeft("fa-solid fa-stop");
        buttonStop.Bind(IsVisibleProperty, new Binding(nameof(vm.IsChecking)));

        var buttonApply = UiUtil.MakeButton(string.Empty, vm.OkCommand);
        buttonApply.Bind(ContentControl.ContentProperty, new Binding(nameof(vm.ApplyButtonText)));
        buttonApply.WithIconLeft("fa-solid fa-check");

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var bottomBar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
        };
        bottomBar.Add(leftButtons, 0, 0);
        bottomBar.Add(UiUtil.MakeButtonBar(buttonCheck, buttonStop, buttonApply, buttonCancel), 0, 2);

        // ---------- layout ----------
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 8,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(toolbar, 0, 0);
        grid.Add(chipsItems, 1, 0);
        grid.Add(borderGrid, 2, 0);
        grid.Add(progressRow, 3, 0);
        grid.Add(messageStrip, 4, 0);
        grid.Add(bottomBar, 5, 0);

        Content = grid;

        Loaded += delegate
        {
            buttonCheck.Focus();
            UiUtil.RestoreWindowPosition(this);
            vm.OnLoaded();
        };
        Closing += delegate { vm.OnClosing(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// A word-diff cell. The diff is built in code rather than bound, so the cell has to redo it when
    /// the row's replacement changes - the picker under the grid can swap it while the row is on screen.
    /// </summary>
    private static Control MakeDiffCell(GrammarCheckSuggestionItem? item, bool showAfter)
    {
        var border = new Border
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(4),
        };

        if (item == null)
        {
            return border;
        }

        void Refresh()
        {
            var (beforeBlock, afterBlock) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
            border.Child = showAfter ? afterBlock : beforeBlock;
        }

        void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GrammarCheckSuggestionItem.After))
            {
                Refresh();
            }
        }

        Refresh();

        // Rows are recycled, so unsubscribe on detach - and defensively before subscribing.
        border.AttachedToVisualTree += (_, _) =>
        {
            item.PropertyChanged -= OnItemPropertyChanged;
            item.PropertyChanged += OnItemPropertyChanged;
        };
        border.DetachedFromVisualTree += (_, _) => item.PropertyChanged -= OnItemPropertyChanged;

        return border;
    }

    private static TextBlock MakeBoundTextBlock(string textPropertyPath)
    {
        var textBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
        };
        textBlock.Bind(TextBlock.TextProperty, new Binding(textPropertyPath));
        return textBlock;
    }
}
