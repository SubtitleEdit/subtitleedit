using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
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
        MinWidth = 800;
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
        };
        labelStep2.Bind(Label.ContentProperty, new Binding(nameof(vm.Step2Title)));
        labelStep2.Bind(IsVisibleProperty, new Binding(nameof(vm.Step2IsVisible)));

        var textBoxSearch = UiUtil.MakeTextBox(250, vm, nameof(vm.SearchText)).WithMarginRight(25);
        textBoxSearch.Watermark = Se.Language.Tools.FixCommonErrors.SearchRulesDotDotDot;
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
                        new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = new CheckBox
                            {
                                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixRuleDisplayItem.IsSelected)),
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
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
        rulesGrid.CellPointerPressed += (sender, e) =>
        {
            if (e.Column is DataGridCheckBoxColumn column && e.Row.DataContext is FixRuleDisplayItem item)
            {
                item.IsSelected = !item.IsSelected;
            }
        };

        var step2Grid = MakeStep2Grid();
        step2Grid.Bind(IsVisibleProperty, new Binding(nameof(_vm.Step2IsVisible)));
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
            .WithIconLeft("fa-solid fa-arrow-left")
            .BindIsVisible(vm, nameof(vm.Step2IsVisible));

        var buttonApplyFixes = UiUtil.MakeButton(Se.Language.Tools.FixCommonErrors.ApplyFixesAndClose, vm.OkCommand)
            .BindIsVisible(vm, nameof(vm.Step2IsVisible));

        var buttonPanelRight = UiUtil.MakeButtonBar(
            buttonBackToFixList,
            buttonToApplyFixes,
            buttonApplyFixes,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

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
        grid.Children.Add(labelStep2);
        Grid.SetRow(labelStep2, 0);
        Grid.SetColumn(labelStep2, 0);

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
    }

    private Grid MakeStep2Grid()
    {
        // top
        var gridFixes = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        var dataGridFixes = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
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
                        new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = new CheckBox
                            {
                                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixDisplayItem.IsSelected)),
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = "#",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FixDisplayItem.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Action",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FixDisplayItem.Action)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Before",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FixDisplayItem.Before)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "After",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FixDisplayItem.After)),
                    IsReadOnly = true,
                },
            },
        };
        dataGridFixes.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(_vm.SelectedFix)));
        dataGridFixes.SelectionChanged += DataGridFixes_SelectionChanged;

        var buttonBarFixes = UiUtil.MakeButtonBar(
            UiUtil.MakeButton("Select all", _vm.FixesSelectAllCommand),
            UiUtil.MakeButton("Inverse selection", _vm.FixesInverseSelectedCommand),
            UiUtil.MakeButton("Refresh fixes", _vm.DoRefreshFixesCommand),
            UiUtil.MakeButton("Apply selected fixes", _vm.DoApplyFixesCommand)
        );
        buttonBarFixes.WithMarginTop(2);

        gridFixes.Children.Add(dataGridFixes);
        Grid.SetRow(dataGridFixes, 0);
        Grid.SetColumn(dataGridFixes, 0);

        gridFixes.Children.Add(buttonBarFixes);
        Grid.SetRow(buttonBarFixes, 1);
        Grid.SetColumn(buttonBarFixes, 0);

        var borderFixes = UiUtil.MakeBorderForControlNoPadding(gridFixes).WithMarginBottom(5);

        // bottom
        var gridSubtitles = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
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
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                    IsReadOnly = true,
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Hide,
                    Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Duration,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
                    IsReadOnly = true,
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Text)),
                    IsReadOnly = true,
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
            },
        };
        dataGridSubtitles.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(_vm.SelectedParagraph)));
        _vm.GridSubtitles = dataGridSubtitles;

        var gridCurrentSubtbtitle = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 0,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        gridSubtitles.Children.Add(dataGridSubtitles);
        Grid.SetRow(dataGridSubtitles, 0);
        Grid.SetColumn(dataGridSubtitles, 0);

        gridSubtitles.Children.Add(gridCurrentSubtbtitle);
        Grid.SetRow(gridCurrentSubtbtitle, 1);
        Grid.SetColumn(gridCurrentSubtbtitle, 0);

        var borderSubtitles = UiUtil.MakeBorderForControlNoPadding(gridSubtitles);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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
        grid.Children.Add(borderFixes);
        Grid.SetRow(borderFixes, 0);
        Grid.SetColumn(borderFixes, 0);

        grid.Children.Add(borderSubtitles);
        Grid.SetRow(borderSubtitles, 1);
        Grid.SetColumn(borderSubtitles, 0);

        return grid;
    }

    private void DataGridFixes_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 1 && e.AddedItems[0] is FixDisplayItem fixDisplayItem)
        {
            _vm.SelectAndScrollTo(fixDisplayItem);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}