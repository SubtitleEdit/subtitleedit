using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public class FixCommonErrorsWindow : Window
{
    private readonly FixCommonErrorsViewModel _vm;

    public FixCommonErrorsWindow(FixCommonErrorsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.FixCommonErrors.Title;
        Width = 1024;
        Height = 720;
        MinWidth = 920;
        MinHeight = 600;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelStep1 = new Label
        {
            Content = Se.Language.Tools.FixCommonErrors.FixCommonOcrErrorsStep1,
            VerticalAlignment = VerticalAlignment.Center,
        };
        labelStep1.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));

        var labelStep2 = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(0),
        };
        labelStep2.Bind(Label.ContentProperty, new Binding(nameof(vm.Step2Title)));

        var labelFixesApplied = UiUtil.MakeTextBlock(string.Empty);
        labelFixesApplied.Bind(TextBlock.TextProperty, new Binding(nameof(vm.FixesAppliedText)));
        labelFixesApplied.VerticalAlignment = VerticalAlignment.Center;

        var appliedPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(12, 0, 0, 0),
            Children = { UiUtil.MakeVerticalSeparator(1, 0.4, new Thickness(0, 4, 12, 4)), labelFixesApplied },
        };
        appliedPanel.Bind(IsVisibleProperty, new Binding(nameof(vm.FixesAppliedText))
        {
            Converter = new Avalonia.Data.Converters.FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s))
        });

        var step2HeaderPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { labelStep2, appliedPanel },
        };
        step2HeaderPanel.Bind(IsVisibleProperty, new Binding(nameof(vm.Step2IsVisible)));

        var textBoxSearch = UiUtil.MakeTextBox(250, vm, nameof(vm.SearchText)).WithMarginRight(25);
        textBoxSearch.PlaceholderText = Se.Language.Tools.FixCommonErrors.SearchRulesDotDotDot;
        textBoxSearch.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));
        textBoxSearch.TextChanged += vm.TextBoxSearch_TextChanged;
        var panelTopRight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                UiUtil.MakeTextBlock(Se.Language.General.Language).WithMarginRight(5),
                UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage)),
            },
        };

        var rulesGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            [!DataGrid.ItemsSourceProperty] = new Binding($"{nameof(vm.SelectedProfile)}.{nameof(ProfileDisplayItem.FixRules)}"),
            IsReadOnly = false,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Enabled,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<FixRuleDisplayItem>((item, _) =>
                    {
                        return new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = new CheckBox
                            {
                                Focusable = false,
                                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixRuleDisplayItem.IsSelected)),
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        };
                    }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FixRuleDisplayItem.Name)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Example,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FixRuleDisplayItem.Example)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                },
            },
        };
        rulesGrid.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));
        new DataGridCheckboxMultiSelect<FixRuleDisplayItem>(rulesGrid,
            item => item.IsSelected, (item, v) => item.IsSelected = v);

        var comboProfile = UiUtil.MakeComboBox(vm.Profiles, vm, nameof(vm.SelectedProfile));
        var buttonPanelRules = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.General.SelectAll, vm.RulesSelectAllCommand),
            UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.RulesInverseSelectedCommand),
            UiUtil.MakeTextBlock(Se.Language.General.Profile).WithMarginLeft(25).WithMarginRight(10),
            comboProfile,
            UiUtil.MakeButton("...", vm.ShowProfileCommand).Compact()
        );
        buttonPanelRules.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));

        var buttonToApplyFixes = UiUtil.MakeButton(Se.Language.Tools.FixCommonErrors.GoToApplyFixes, vm.ToApplyFixesCommand)
            .WithIconRight("fa-solid fa-arrow-right")
            .BindIsVisible(vm, nameof(vm.Step1IsVisible));

        var buttonBackToFixList = UiUtil.MakeButton(Se.Language.Tools.FixCommonErrors.BackToFixList, vm.BackToFixListCommand)
            .WithIconLeft("fa-solid fa-arrow-left");

        var buttonDone = UiUtil.MakeButton(Se.Language.General.Done, vm.OkCommand);
        buttonDone.IsDefault = true;

        var step2NavButtons = UiUtil.MakeButtonBar(
            buttonBackToFixList,
            buttonDone,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

        var buttonPanelRight = UiUtil.MakeButtonBar(
            buttonToApplyFixes,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        ).BindIsVisible(vm, nameof(vm.Step1IsVisible));

        var step2Grid = MakeStep2Grid(step2NavButtons);
        step2Grid.Bind(IsVisibleProperty, new Binding(nameof(_vm.Step2IsVisible)));

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

        grid.Children.Add(labelStep1);
        Grid.SetRow(labelStep1, 0);
        Grid.SetColumn(labelStep1, 0);
        grid.Children.Add(step2HeaderPanel);
        Grid.SetRow(step2HeaderPanel, 0);
        Grid.SetColumn(step2HeaderPanel, 0);

        grid.Children.Add(panelTopRight);
        Grid.SetRow(panelTopRight, 0);
        Grid.SetColumn(panelTopRight, 0);
        Grid.SetColumnSpan(panelTopRight, 2);

        grid.Children.Add(rulesGrid);
        Grid.SetRow(rulesGrid, 1);
        Grid.SetColumn(rulesGrid, 0);
        Grid.SetColumnSpan(rulesGrid, 2);

        grid.Children.Add(step2Grid);
        Grid.SetRow(step2Grid, 1);
        Grid.SetColumn(step2Grid, 0);
        Grid.SetColumnSpan(step2Grid, 2);

        grid.Children.Add(buttonPanelRules);
        Grid.SetRow(buttonPanelRules, 2);
        Grid.SetColumn(buttonPanelRules, 0);

        grid.Children.Add(buttonPanelRight);
        Grid.SetRow(buttonPanelRight, 2);
        Grid.SetColumn(buttonPanelRight, 1);

        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Grid MakeStep2Grid(Control navButtons)
    {
        // top
        var dataGridFixes = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = _vm,
            ItemsSource = _vm.Fixes,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<FixDisplayItem>((item, _) =>
                    {
                        return new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = new CheckBox
                            {
                                Focusable = false,
                                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixDisplayItem.IsSelected)),
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        };
                    }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FixDisplayItem.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.Tools.FixCommonErrors.Action,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FixDisplayItem.ActionDisplay)),
                    IsReadOnly = true,
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<FixDisplayItem>((item, _) =>
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
                    CellTemplate = new FuncDataTemplate<FixDisplayItem>((item, _) =>
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
        dataGridFixes.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(_vm.SelectedFix)));
        new DataGridCheckboxMultiSelect<FixDisplayItem>(dataGridFixes,
            item => item.IsSelected, (item, v) => item.IsSelected = v,
            onFocusedItemChanged: item => { if (item != null)
            {
                _vm.SelectAndScrollTo(item);
            } });

        var leftButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 0,
        };
        leftButtons.Children.Add(UiUtil.MakeButton(Se.Language.General.SelectAll, _vm.FixesSelectAllCommand));
        leftButtons.Children.Add(UiUtil.MakeButton(Se.Language.General.InvertSelection, _vm.FixesInverseSelectedCommand));

        var rightButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 0,
        };
        rightButtons.Children.Add(UiUtil.MakeButton(Se.Language.Tools.FixCommonErrors.RefreshFixes, _vm.DoRefreshFixesCommand));
        rightButtons.Children.Add(UiUtil.MakeButton(Se.Language.Tools.FixCommonErrors.ApplySelectedFixes, _vm.DoApplyFixesCommand));

        var buttonBarFixes = new Grid
        {
            Margin = new Thickness(10, 10, 10, 10),
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        buttonBarFixes.Add(leftButtons, 0, 0);
        buttonBarFixes.Add(rightButtons, 0, 1);

        var borderFixes = UiUtil.MakeBorderForControlNoPadding(dataGridFixes).WithMarginBottom(5);

        // bottom
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var syntaxHighlightingConverter = new TextWithSubtitleSyntaxHighlightingConverter();
        var dataGridSubtitles = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = _vm,
            ItemsSource = _vm.Paragraphs,
            FontSize = Se.Settings.Appearance.SubtitleGridFontSize,
            Margin = new Thickness(Se.Settings.Appearance.GridCompactMode ? 0 : 2),
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                    IsReadOnly = true,
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    IsReadOnly = true,
                    CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((_, _) =>
                    {
                        var border = new Border
                        {
                            Padding = new Thickness(4, 2),
                            [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.StartTimeBackgroundBrush)),
                        };
                        border.Child = new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
                        };
                        return border;
                    }),
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Hide,
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    IsReadOnly = true,
                    CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((_, _) =>
                    {
                        var border = new Border
                        {
                            Padding = new Thickness(4, 2),
                            [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.EndTimeBackgroundBrush)),
                        };
                        border.Child = new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter },
                        };
                        return border;
                    }),
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Duration,
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    IsReadOnly = true,
                    CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((_, _) =>
                        new Border
                        {
                            Padding = new Thickness(4, 2),
                            [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.DurationBackgroundBrush)),
                            Child = new TextBlock
                            {
                                VerticalAlignment = VerticalAlignment.Center,
                                [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
                            },
                        }),
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((_, _) =>
                    {
                        var textBlock = new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            TextWrapping = TextWrapping.NoWrap,
                            [!TextBlock.InlinesProperty] = new Binding(nameof(SubtitleLineViewModel.Text)) { Converter = syntaxHighlightingConverter },
                        };
                        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
                        {
                            textBlock.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
                        }
                        return new Border
                        {
                            Padding = new Thickness(4, 2),
                            [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.TextBackgroundBrush)),
                            Child = textBlock,
                        };
                    }),
                },
            },
        };
        dataGridSubtitles.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(_vm.SelectedParagraph)));
        _vm.GridSubtitles = dataGridSubtitles;
        dataGridSubtitles.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGridSubtitles.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                dataGridSubtitles.SelectedItem = target;
                dataGridSubtitles.ScrollIntoView(target, null);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        var gridCurrentSubtbtitle = MakeStep2EditPanel();

        var borderSubtitles = UiUtil.MakeBorderForControlNoPadding(dataGridSubtitles).WithMarginBottom(5);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 80 },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 80 },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 0,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var bottomRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        navButtons.VerticalAlignment = VerticalAlignment.Bottom;
        bottomRow.Add(gridCurrentSubtbtitle, 0, 0);
        bottomRow.Add(navButtons, 0, 2);

        grid.Add(borderFixes, 0, 0);
        grid.Add(buttonBarFixes, 1, 0);
        grid.Add(borderSubtitles, 2, 0);
        grid.Add(bottomRow, 3, 0);

        return grid;
    }

    private Grid MakeStep2EditPanel()
    {
        var textEditGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = new Thickness(0, 5, 0, 5),
            Width = 500,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var textBox = new TextBox
        {
            DataContext = _vm,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 60,
            FontSize = Se.Settings.Appearance.SubtitleTextBoxFontSize,
            FontWeight = Se.Settings.Appearance.SubtitleTextBoxFontBold ? FontWeight.Bold : FontWeight.Normal,
            [!TextBox.TextProperty] = new Binding($"{nameof(_vm.SelectedParagraph)}.{nameof(SubtitleLineViewModel.Text)}")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            },
        };
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            textBox.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }
        textEditGrid.Add(textBox, 0, 0);

        var panelSingleLineLengths = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Orientation = Orientation.Horizontal,
        };
        _vm.PanelSingleLineLengths = panelSingleLineLengths;
        textEditGrid.Add(panelSingleLineLengths, 1, 0);

        var totalLengthLabel = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            FontSize = 12,
            Padding = new Thickness(2),
        };
        totalLengthLabel.Bind(TextBlock.TextProperty, new Binding(nameof(_vm.EditTextTotalLength)) { Source = _vm });
        totalLengthLabel.Bind(TextBlock.BackgroundProperty, new Binding(nameof(_vm.EditTextTotalLengthBackground)) { Source = _vm });
        textEditGrid.Add(totalLengthLabel, 1, 0);

        return textEditGrid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

}
