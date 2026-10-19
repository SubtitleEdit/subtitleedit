using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameTimeCodes;

/// <summary>
/// Merge lines with same time codes, in the editor look (see <see cref="EditorChrome"/>): the options on top, the
/// lines to merge and a preview of the result below, Cancel and OK in the button bar.
/// </summary>
public class MergeSameTimeCodesWindow : Window
{
    private readonly MergeSameTimeCodesViewModel _vm;
    private NumericUpDown _numericUpDownMaxDiff = null!;

    public MergeSameTimeCodesWindow(MergeSameTimeCodesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.General.MergeLinesWithSameTimeCodes);
        CanResize = true;
        Width = 800;
        Height = 750;
        MinWidth = 600;
        MinHeight = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindEnabled(nameof(vm.IsOkEnabled));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeCancelOkRoot(EditorChrome.MakeOptionsAndLists(EditorChrome.MakeEditorSection(Se.Language.General.Options, MakeControlsView(vm), first: true), EditorChrome.MakeListSection(Se.Language.General.MergeLines, MakeMergesView(vm)), EditorChrome.MakeListSection(Se.Language.General.Preview, MakeSubtitlesView(vm))), null, buttonCancel, buttonOk);

        UiUtil.FocusOnFirstActivation(this, _numericUpDownMaxDiff); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += _vm.OnKeyDown;

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Grid MakeControlsView(MergeSameTimeCodesViewModel vm)
    {
        var numericUpDownMaxDiff = EditorChrome.AsField(UiUtil.MakeNumericUpDownInt(0, 10000, vm.MaxMsOrFramesDifference, 130, vm, nameof(vm.MaxMsOrFramesDifference)), vm.MaxDifferenceLabel.TrimEnd(':'), 100, alignLeft: true, center: true);
        _numericUpDownMaxDiff = numericUpDownMaxDiff;
        numericUpDownMaxDiff.ValueChanged += (s, e) => { vm.SetDirty(); };
        var rowMergeAsDialog = EditorChrome.MakeToggleRow(nameof(vm.MergeDialog), Se.Language.Tools.MergeLinesWithSameTimeCodes.MakeDialog, out var checkBoxMergeAsDialog);
        checkBoxMergeAsDialog.IsCheckedChanged += (s, e) => { vm.SetDirty(); };
        var rowAutoBreak = EditorChrome.MakeToggleRow(nameof(vm.AutoBreak), Se.Language.General.AutoBreak, out var checkBoxAutoBreak);
        checkBoxAutoBreak.IsCheckedChanged += (s, e) => { vm.SetDirty(); };

        var grid = new Grid
        {
            ColumnSpacing = 40,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
        };
        grid.Add(EditorChrome.MakeFieldGrid((vm.MaxDifferenceLabel, numericUpDownMaxDiff)), 0, 0);
        grid.Add(new StackPanel { Spacing = 8, Children = { rowMergeAsDialog, rowAutoBreak } }, 0, 1);
        return grid;
    }

    private static TableView MakeMergesView(MergeSameTimeCodesViewModel vm)
    {
        // Sorting dropped in the DataGrid -> TableView conversion: the grid shows
        // merge candidates in subtitle order.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.MergeItems;
        dataGrid.WithAccessibleName(Se.Language.General.MergeLinesWithSameTimeCodes); // no heading above the list (#12087)
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<MergeDisplayItem>(nameof(MergeDisplayItem.Apply), focusable: true),
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(70)
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Lines,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(MergeDisplayItem.Lines)),
                    Width = new GridLength(110),
                },
                new SeTableViewColumn
                {
                    // The merged text is the wide content, so it takes the star width
                    // (the DataGrid content-sized it and gave Group the star).
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(MergeDisplayItem.MergedText)),
                    Width = new GridLength(1, GridUnitType.Star),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Group,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(MergeDisplayItem.MergedGroup)),
                    Width = new GridLength(90),
                },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedMergeItem)) { Source = vm });
        dataGrid.SelectionChanged += vm.MergeItemChanged;
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var index = e.Key == Key.Home ? 0 : items.Count - 1;
                dataGrid.SelectedIndex = index;
                dataGrid.ScrollIntoView(index);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        return dataGrid;
    }

    private static TableView MakeSubtitlesView(MergeSameTimeCodesViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();

        // Sorting dropped in the DataGrid -> TableView conversion: the grid previews
        // subtitle lines in timeline order.
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.MergeSubtitles;
        dataGrid.WithAccessibleName(Se.Language.General.Preview);
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(60),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
                    Width = new GridLength(120),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Hide,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter },
                    Width = new GridLength(120),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.Text)),
                    NameBinding = new Binding(nameof(SubtitleLineViewModel.Text)),
                    Width = new GridLength(1, GridUnitType.Star),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Group,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Extra)),
                    Width = new GridLength(90),
                },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedMergeSubtitle)) { Source = vm });
        vm.SubtitleGrid = dataGrid;
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var index = e.Key == Key.Home ? 0 : items.Count - 1;
                dataGrid.SelectedIndex = index;
                dataGrid.ScrollIntoView(index);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        return dataGrid;
    }
}
