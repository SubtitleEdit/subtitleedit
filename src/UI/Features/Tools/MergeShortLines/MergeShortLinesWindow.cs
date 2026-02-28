using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.MergeShortLines;

public class MergeShortLinesWindow : Window
{
    public MergeShortLinesWindow(MergeShortLinesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.MergeShortLines.Title;
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
        grid.Add(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
        Loaded += delegate { vm.Loaded(); };
    }

    private static Grid MakeControlsView(MergeShortLinesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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

        var labelSingleLineMaxLength = UiUtil.MakeLabel(Se.Language.Options.Settings.SingleLineMaxLength);
        var numericUpDownSingleLineMaxLength = UiUtil.MakeNumericUpDownInt(5, 1000, 10, 130, vm, nameof(vm.SingleLineMaxLength));
        numericUpDownSingleLineMaxLength.ValueChanged += (s, e) => vm.SetChanged();

        var labelMaxNumberOfLines = UiUtil.MakeLabel(Se.Language.Options.Settings.MaxLines);
        var numericUpDownMaxNumberOfLines = UiUtil.MakeNumericUpDownInt(2, 10, 2, 130, vm, nameof(vm.MaxNumberOfLines));
        numericUpDownMaxNumberOfLines.ValueChanged += (s, e) => vm.SetChanged();

        var checkBoxHighlightParts = UiUtil.MakeCheckBox(Se.Language.Tools.MergeShortLines.HighlightParts, vm, nameof(vm.HighLight));
        checkBoxHighlightParts.IsCheckedChanged += (s, e) => vm.SetChanged();

        grid.Add(labelSingleLineMaxLength, 0, 0);
        grid.Add(numericUpDownSingleLineMaxLength, 0, 1);

        grid.Add(labelMaxNumberOfLines, 1, 0);
        grid.Add(numericUpDownMaxNumberOfLines, 1, 1);

        grid.Add(checkBoxHighlightParts, 2, 1);

        return grid;
    }

    private static Grid MakeFixesView(MergeShortLinesViewModel vm)
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
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Binding = new Binding(nameof(MergeShortLinesItem.Number)),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Fix,
                    Binding = new Binding(nameof(MergeShortLinesItem.Fix)),
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
