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

/// <summary>
/// Merge continuation lines, in the editor look (see <see cref="EditorChrome"/>): the options on
/// top, the candidates below, the summary plus Cancel and OK in the button bar.
/// </summary>
public class MergeContinuationLinesWindow : Window
{
    private NumericUpDown _numericGap = null!;

    public MergeContinuationLinesWindow(MergeContinuationLinesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.MergeContinuationLines.Title);
        CanResize = true;
        Width = 1000;
        Height = 700;
        MinWidth = 700;
        MinHeight = 400;
        LineToolsChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = LineToolsChrome.MakeRoot(
            LineToolsChrome.MakeSections(MakeControlsView(vm), MakeCandidatesView(vm)),
            LineToolsChrome.MakeStatusText(vm, nameof(vm.CandidatesInfo)),
            buttonCancel,
            buttonOk);

        UiUtil.FocusOnFirstActivation(this, _numericGap); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
        Loaded += delegate { vm.Loaded(); };

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Border MakeControlsView(MergeContinuationLinesViewModel vm)
    {
        var labelGap = Se.Language.Tools.MergeContinuationLines.MaxMillisecondsBetweenLines;
        var numericGap = LineToolsChrome.MakeCompact(UiUtil.MakeNumericUpDownInt(0, 10000, 250, 150, vm, nameof(vm.MaxMillisecondsBetweenLines)), labelGap);
        _numericGap = numericGap;
        numericGap.ValueChanged += (_, _) => vm.SetChanged();

        var labelMax = Se.Language.Tools.MergeContinuationLines.MaxCharacters;
        var numericMax = LineToolsChrome.MakeCompact(UiUtil.MakeNumericUpDownInt(20, 1000, 100, 150, vm, nameof(vm.MaxCharacters)), labelMax);
        numericMax.ValueChanged += (_, _) => vm.SetChanged();

        var fields = LineToolsChrome.MakeFieldGrid((labelGap, numericGap), (labelMax, numericMax));
        return EditorChrome.MakeEditorSection(Se.Language.General.Options, fields, first: true);
    }

    private static Border MakeCandidatesView(MergeContinuationLinesViewModel vm)
    {
        // The DataGrid this replaces used DataGridCheckboxMultiSelect for extended
        // selection + Space toggling; shift/ctrl multi-select is native ListBox behavior
        // on TableView, so only the Space toggle needs wiring (below).
        var dataGrid = TableViewExtras.MakeTableView().WithAccessibleName(Se.Language.General.Preview);
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

        var actions = LineToolsChrome.MakeListActions(
            LineToolsChrome.MakeSelectAllButton(vm.SelectAllCommand),
            LineToolsChrome.MakeInvertSelectionButton(vm.InverseSelectionCommand));

        return LineToolsChrome.MakeListSection(Se.Language.General.Preview, dataGrid, actions);
    }
}
