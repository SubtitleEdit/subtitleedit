using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.MergeContinuationLines;

public class MergeContinuationLinesWindow : Window
{
    public MergeContinuationLinesWindow(MergeContinuationLinesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.MergeContinuationLines.Title;
        CanResize = true;
        Width = 1000;
        Height = 700;
        MinWidth = 700;
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
        grid.Add(MakeCandidatesView(vm), 1);
        grid.Add(MakeSelectionButtonsView(vm), 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += vm.KeyDown;
        Loaded += delegate { vm.Loaded(); };
        Closed += delegate { vm.OnClosed(); };
    }

    private static Grid MakeControlsView(MergeContinuationLinesViewModel vm)
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var labelGap = UiUtil.MakeLabel(Se.Language.Tools.MergeContinuationLines.MaxMillisecondsBetweenLines);
        var numericGap = UiUtil.MakeNumericUpDownInt(0, 10000, 250, 150, vm, nameof(vm.MaxMillisecondsBetweenLines));
        numericGap.ValueChanged += (_, _) => vm.SetChanged();

        var labelMax = UiUtil.MakeLabel(Se.Language.Tools.MergeContinuationLines.MaxCharacters);
        var numericMax = UiUtil.MakeNumericUpDownInt(20, 1000, 100, 150, vm, nameof(vm.MaxCharacters));
        numericMax.ValueChanged += (_, _) => vm.SetChanged();

        grid.Add(labelGap, 0, 0);
        grid.Add(numericGap, 1, 0);

        grid.Add(labelMax, 0, 1);
        grid.Add(numericMax, 1, 1);

        return grid;
    }

    private static Grid MakeCandidatesView(MergeContinuationLinesViewModel vm)
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

        var labelInfo = UiUtil.MakeLabel()
            .WithBindText(vm, nameof(vm.CandidatesInfo))
            .WithMarginTop(10)
            .WithMarginLeft(10);

        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.Candidates,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.Tools.MergeContinuationLines.ColumnMerge,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<MergeContinuationLinesCandidate>((_, _) =>
                    {
                        return new Border
                        {
                            Background = Brushes.Transparent,
                            Padding = new Thickness(4),
                            Child = new CheckBox
                            {
                                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(MergeContinuationLinesCandidate.IsSelected))
                                {
                                    Mode = BindingMode.TwoWay,
                                },
                                HorizontalAlignment = HorizontalAlignment.Center,
                            },
                        };
                    }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Binding = new Binding(nameof(MergeContinuationLinesCandidate.Number)),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.Tools.MergeContinuationLines.ColumnFirst,
                    Binding = new Binding(nameof(MergeContinuationLinesCandidate.Text1)),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.Tools.MergeContinuationLines.ColumnSecond,
                    Binding = new Binding(nameof(MergeContinuationLinesCandidate.Text2)),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.Tools.MergeContinuationLines.ColumnMerged,
                    Binding = new Binding(nameof(MergeContinuationLinesCandidate.MergedTextDisplay)),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    Width = new DataGridLength(1.4, DataGridLengthUnitType.Star),
                    IsReadOnly = true,
                },
            },
        };

        grid.Add(labelInfo, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 1);

        return grid;
    }

    private static StackPanel MakeSelectionButtonsView(MergeContinuationLinesViewModel vm)
    {
        return UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.General.SelectAll, vm.SelectAllCommand),
            UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.InverseSelectionCommand));
    }
}
