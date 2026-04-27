using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits;

public class ApplyDurationLimitsWindow : Window
{
    public ApplyDurationLimitsWindow(ApplyDurationLimitsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ApplyDurationLimits.Title;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
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
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeControlsView(vm), 0);
        grid.Add(MakeFixesView(vm), 1);
        grid.Add(MakeCannotFixView(vm), 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Grid MakeControlsView(ApplyDurationLimitsViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var checkBoxFixMinDuration = UiUtil.MakeCheckBox(Se.Language.Tools.ApplyDurationLimits.FixMinDurationMs, vm, nameof(vm.FixMinDurationMs));
        checkBoxFixMinDuration.IsCheckedChanged += (s, e) => vm.SetChanged();
        var numericUpDownMinDuration = UiUtil.MakeNumericUpDownInt(1, 10000, 1000, 150, vm, nameof(vm.MinDurationMs))
                .WithBindEnabled(nameof(vm.FixMinDurationMs));
        numericUpDownMinDuration.ValueChanged += (s, e) => vm.SetChanged();
        var checkBoxDoNotGoPastShotChange = UiUtil.MakeCheckBox(Se.Language.Tools.ApplyDurationLimits.DoNotGoPastShotChange, vm, nameof(vm.DoNotGoPastShotChange))
            .WithBindEnabled(nameof(vm.FixMinDurationMs))
            .WithMarginLeft(5)
            .BindIsVisible(vm, nameof(vm.IsDoNotGoPastShotChangeVisible));
        checkBoxDoNotGoPastShotChange.IsCheckedChanged += (s, e) => vm.SetChanged();
        var panelMin = UiUtil.MakeHorizontalPanel(numericUpDownMinDuration, checkBoxDoNotGoPastShotChange);

        var checkBoxFixMaxDuration = UiUtil.MakeCheckBox(Se.Language.Tools.ApplyDurationLimits.FixMaxDurationMs, vm, nameof(vm.FixMaxDurationMs));
        checkBoxFixMaxDuration.IsCheckedChanged += (s, e) => vm.SetChanged();
        var numericUpDownMaxDuration = UiUtil.MakeNumericUpDownInt(1, 10000, 1000, 150, vm, nameof(vm.MaxDurationMs))
            .WithBindEnabled(nameof(vm.FixMaxDurationMs));
        numericUpDownMaxDuration.ValueChanged += (s, e) => vm.SetChanged();

        grid.Add(checkBoxFixMinDuration, 0, 0);
        grid.Add(panelMin, 0, 1);

        grid.Add(checkBoxFixMaxDuration, 1, 0);
        grid.Add(numericUpDownMaxDuration, 1, 1);

        return grid;
    }

    private static Grid MakeFixesView(ApplyDurationLimitsViewModel vm)
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var labelFixesAvailable = UiUtil.MakeLabel()
            .WithBindText(vm, nameof(vm.FixesInfo))
            .WithMarginTop(10)
            .WithMarginLeft(10);

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
                    CellTemplate = new FuncDataTemplate<ApplyDurationLimitItem>((item, _) =>
                        new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = new CheckBox
                            {
                                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(ApplyDurationLimitItem.Apply)),
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Binding = new Binding(nameof(ApplyDurationLimitItem.Number)),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Fix,
                    Binding = new Binding(nameof(ApplyDurationLimitItem.Fix)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    IsReadOnly = true,
                },
            },
        };

        grid.Add(labelFixesAvailable, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 1);

        return grid;
    }

    private static Grid MakeCannotFixView(ApplyDurationLimitsViewModel vm)
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var labelFixesAvailable = UiUtil.MakeLabel()
            .WithBindText(vm, nameof(vm.FixesSkippedInfo))
            .WithMarginTop(10)
            .WithMarginLeft(10);

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

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
            ItemsSource = vm.Subtitles,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
                    Width = new DataGridLength(120),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Duration,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
                    Width = new DataGridLength(120),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Text)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    IsReadOnly = true,
                },
            },
        };

        grid.Add(labelFixesAvailable, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 1);

        return grid;
    }
}
