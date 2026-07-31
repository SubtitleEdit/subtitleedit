using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.MergeContinuationLines;

public class MergeContinuationLinesWindow : Window
{
    private NumericUpDown _numericGap = null!;

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

        Activated += delegate { _numericGap.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
        Loaded += delegate { vm.Loaded(); };

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Grid MakeControlsView(MergeContinuationLinesViewModel vm)
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
        _numericGap = numericGap;
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

        // The DataGrid this replaces used DataGridCheckboxMultiSelect for extended
        // selection + Space toggling; shift/ctrl multi-select is native ListBox behavior
        // on TableView, so only the Space toggle needs wiring (below).
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Candidates;

        // The checkbox and number columns were content-sized (Auto) on the DataGrid;
        // TableView treats Auto as star, so they get fixed widths instead.
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Tools.MergeContinuationLines.ColumnMerge,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<MergeContinuationLinesCandidate>((_, _) =>
            {
                return new Border
                {
                    Background = Brushes.Transparent,
                    Padding = new Thickness(4),
                    Child = new CheckBox
                    {
                        Focusable = false,
                        [!ToggleButton.IsCheckedProperty] = new Binding(nameof(MergeContinuationLinesCandidate.IsSelected))
                        {
                            Mode = BindingMode.TwoWay,
                        },
                        HorizontalAlignment = HorizontalAlignment.Center,
                    },
                };
            }),
            Width = new GridLength(80),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(MergeContinuationLinesCandidate.Number)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(60),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Tools.MergeContinuationLines.ColumnFirst,
            Binding = new Binding(nameof(MergeContinuationLinesCandidate.Text1)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Tools.MergeContinuationLines.ColumnSecond,
            Binding = new Binding(nameof(MergeContinuationLinesCandidate.Text2)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Tools.MergeContinuationLines.ColumnMerged,
            Binding = new Binding(nameof(MergeContinuationLinesCandidate.MergedTextDisplay)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1.4, GridUnitType.Star),
        });

        // Space toggles the checkbox of every selected row - the piece of the old
        // DataGridCheckboxMultiSelect helper that TableView does not provide natively.
        TableViewExtras.AddSpaceToggle<MergeContinuationLinesCandidate>(dataGrid,
            item => item.IsSelected,
            (item, value) => item.IsSelected = value);

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
