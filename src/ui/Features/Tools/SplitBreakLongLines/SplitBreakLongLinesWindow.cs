using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections;

namespace Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;

/// <summary>
/// Split/break long lines, in the editor look (see <see cref="EditorChrome"/>): the options on
/// top, the preview of the fixes below, the summary plus Cancel and OK in the button bar.
/// </summary>
public class SplitBreakLongLinesWindow : Window
{
    private Control _checkBoxSplitLongLines = null!;

    public SplitBreakLongLinesWindow(SplitBreakLongLinesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.SplitBreakLongLines.Title);
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeCancelOkRoot(EditorChrome.MakeOptionsAndLists(MakeControlsView(vm), MakeFixesView(vm)), EditorChrome.MakeStatusText(nameof(vm.FixesInfo), vm), buttonCancel, buttonOk);

        UiUtil.FocusOnFirstActivation(this, _checkBoxSplitLongLines); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
        Loaded += (_, _)  => vm.Loaded();

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Border MakeControlsView(SplitBreakLongLinesViewModel vm)
    {
        var rowSplitLongLines = EditorChrome.MakeToggleRow(nameof(vm.SplitLongLines), Se.Language.Tools.SplitBreakLongLines.SplitLongLines, out var checkBoxSplitLongLines);
        _checkBoxSplitLongLines = checkBoxSplitLongLines;
        checkBoxSplitLongLines.IsCheckedChanged += (s, e) => vm.SetChanged();

        var rowRebalanceLongLines = EditorChrome.MakeToggleRow(nameof(vm.RebalanceLongLines), Se.Language.Tools.SplitBreakLongLines.RebalanceLongLines, out var checkBoxRebalanceLongLines);
        checkBoxRebalanceLongLines.IsCheckedChanged += (s, e) => vm.SetChanged();

        var rowRebalanceOnlyTooLong = EditorChrome.MakeToggleRow(nameof(vm.RebalanceOnlyLinesTooLong), Se.Language.Tools.SplitBreakLongLines.RebalanceOnlyLinesTooLong, out var checkBoxRebalanceOnlyTooLong);
        rowRebalanceOnlyTooLong.Margin = new Thickness(25, 0, 0, 0);
        rowRebalanceOnlyTooLong[!InputElement.IsEnabledProperty] = new Binding(nameof(vm.RebalanceLongLines));
        rowRebalanceOnlyTooLong[!Visual.OpacityProperty] = new Binding(nameof(vm.RebalanceLongLines)) { Converter = new FuncValueConverter<bool, double>(on => on ? 1.0 : 0.5) };
        checkBoxRebalanceOnlyTooLong.IsCheckedChanged += (s, e) => vm.SetChanged();

        var labelSingleLineMaxLength = Se.Language.Options.Settings.SingleLineMaxLength;
        var numericUpDownSingleLineMaxLength = EditorChrome.AsField(UiUtil.MakeNumericUpDownInt(5, 1000, 10, 130, vm, nameof(vm.SingleLineMaxLength)), labelSingleLineMaxLength.TrimEnd(':'), 100, alignLeft: true, center: true);
        numericUpDownSingleLineMaxLength.ValueChanged += (s, e) => vm.SetChanged();

        var labelMaxNumberOfLines = Se.Language.General.MaxNumberOfLines;
        var numericUpDownMaxNumberOfLines = EditorChrome.AsField(UiUtil.MakeNumericUpDownInt(1, 10, 2, 130, vm, nameof(vm.MaxNumberOfLines)), labelMaxNumberOfLines.TrimEnd(':'), 100, alignLeft: true, center: true);
        numericUpDownMaxNumberOfLines.ValueChanged += (s, e) => vm.SetChanged();

        var labelUnbreakLinesShorterThan = Se.Language.Options.Settings.UnbreakSubtitlesShortThan;
        var numericUpDownUnbreakLinesShorterThan = EditorChrome.AsField(UiUtil.MakeNumericUpDownInt(1, 1000, 33, 130, vm, nameof(vm.UnbreakLinesShorterThan)), labelUnbreakLinesShorterThan.TrimEnd(':'), 100, alignLeft: true, center: true);
        numericUpDownUnbreakLinesShorterThan.ValueChanged += (s, e) => vm.SetChanged();

        var fields = EditorChrome.MakeFieldGrid((labelSingleLineMaxLength, numericUpDownSingleLineMaxLength), (labelMaxNumberOfLines, numericUpDownMaxNumberOfLines), (labelUnbreakLinesShorterThan, numericUpDownUnbreakLinesShorterThan));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(fields.Children[4], Se.Language.Tools.SplitBreakLongLines.UnbreakLinesShorterThanHint); // the label
            ToolTip.SetTip(numericUpDownUnbreakLinesShorterThan, Se.Language.Tools.SplitBreakLongLines.UnbreakLinesShorterThanHint);
            ToolTip.SetTip(rowRebalanceOnlyTooLong, Se.Language.Tools.SplitBreakLongLines.RebalanceOnlyLinesTooLongHint);
        }

        var switches = new StackPanel
        {
            Spacing = 8,
            Children = { rowSplitLongLines, rowRebalanceLongLines, rowRebalanceOnlyTooLong },
        };

        var grid = new Grid
        {
            ColumnSpacing = 40,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
        };
        grid.Add(switches, 0, 0);
        grid.Add(fields, 0, 1);

        return EditorChrome.MakeEditorSection(Se.Language.General.Options, grid, first: true);
    }

    private static Border MakeFixesView(SplitBreakLongLinesViewModel vm)
    {
        // Sorting dropped in the DataGrid -> TableView conversion: the grid previews
        // split/rebalance fixes in subtitle order.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false).WithAccessibleName(Se.Language.General.Preview);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Fixes;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<SplitBreakLongLinesItem>(nameof(SplitBreakLongLinesItem.IsSelected), nameof(SplitBreakLongLinesItem.Name),
                        configure: cb => cb[!Visual.IsVisibleProperty] = new Binding(nameof(SplitBreakLongLinesItem.IsSelectable))),
                    Width = new GridLength(70),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Binding = new Binding(nameof(SplitBreakLongLinesItem.Number)),
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(60),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = new FuncDataTemplate<SplitBreakLongLinesItem>((item, _) =>
                    {
                        if (item == null)
                        {
                            return new Border();
                        }

                        var isSplit = item.Name == Se.Language.Tools.SplitBreakLongLines.SplitLongLine;
                        var color = isSplit ? Color.FromRgb(0x5f, 0xc6, 0xd8) : Color.FromRgb(0xb4, 0x8c, 0xe8);
                        // The pastels are tuned for the dark theme; darker shades of the same
                        // hues keep the pill text readable on the light theme (#15768).
                        var textColor = UiTheme.IsDarkThemeEnabled()
                            ? color
                            : isSplit ? Color.FromRgb(0x00, 0x6b, 0x7d) : Color.FromRgb(0x6a, 0x3d, 0xb8);
                        return new Border
                        {
                            Background = Brushes.Transparent,
                            Padding = new Thickness(4),
                            Child = new Border
                            {
                                Background = new SolidColorBrush(Color.FromArgb(0x20, color.R, color.G, color.B)),
                                CornerRadius = new CornerRadius(5),
                                Padding = new Thickness(7, 2),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center,
                                Child = new TextBlock
                                {
                                    Text = item.Name,
                                    FontSize = UiUtil.ScaledFontSize(12),
                                    Foreground = new SolidColorBrush(textColor),
                                    VerticalAlignment = VerticalAlignment.Center,
                                },
                            },
                        };
                    }),
                    // Content-sized (Auto) on the DataGrid; fits the "Split long line" /
                    // "Rebalance long line" pill.
                    Width = new GridLength(150),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Fix,
                    Binding = new Binding(nameof(SplitBreakLongLinesItem.Fix)),
                    Width = new GridLength(1, GridUnitType.Star),
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                },
        });

        TableViewExtras.AddSpaceToggle<SplitBreakLongLinesItem>(dataGrid,
            item => item.IsSelected, (item, v) => { if (item.IsSelectable) { item.IsSelected = v; } });

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

        // Select all / none only touch the rebalance rows (the ones with a checkbox).
        var actions = EditorChrome.MakeListActions(EditorChrome.MakeSelectAllButton(vm.SelectAllCommand), EditorChrome.MakeSelectNoneButton(vm.SelectNoneCommand));

        return EditorChrome.MakeListSection(Se.Language.General.Preview, dataGrid, actions);
    }
}
