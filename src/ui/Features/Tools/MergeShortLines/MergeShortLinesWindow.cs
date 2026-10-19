using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections;

namespace Nikse.SubtitleEdit.Features.Tools.MergeShortLines;

/// <summary>
/// Merge short lines, in the editor look (see <see cref="EditorChrome"/>): the options on top,
/// the preview of the merges below, the summary plus Cancel and OK in the button bar.
/// </summary>
public class MergeShortLinesWindow : Window
{
    private NumericUpDown _numericUpDownSingleLineMaxLength = null!;

    public MergeShortLinesWindow(MergeShortLinesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.MergeShortLines.Title);
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

        UiUtil.FocusOnFirstActivation(this, _numericUpDownSingleLineMaxLength); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
        Loaded += delegate { vm.Loaded(); };

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Border MakeControlsView(MergeShortLinesViewModel vm)
    {
        var labelSingleLineMaxLength = Se.Language.Options.Settings.SingleLineMaxLength;
        var numericUpDownSingleLineMaxLength = EditorChrome.AsField(UiUtil.MakeNumericUpDownInt(5, 1000, 10, 130, vm, nameof(vm.SingleLineMaxLength)), labelSingleLineMaxLength.TrimEnd(':'), 100, alignLeft: true, center: true);
        _numericUpDownSingleLineMaxLength = numericUpDownSingleLineMaxLength;
        numericUpDownSingleLineMaxLength.ValueChanged += (s, e) => vm.SetChanged();

        var labelMaxNumberOfLines = Se.Language.General.MaxNumberOfLines;
        var numericUpDownMaxNumberOfLines = EditorChrome.AsField(UiUtil.MakeNumericUpDownInt(2, 10, 2, 130, vm, nameof(vm.MaxNumberOfLines)), labelMaxNumberOfLines.TrimEnd(':'), 100, alignLeft: true, center: true);
        numericUpDownMaxNumberOfLines.ValueChanged += (s, e) => vm.SetChanged();

        var rowHighlightParts = EditorChrome.MakeToggleRow(nameof(vm.HighLight), Se.Language.Tools.MergeShortLines.HighlightParts, out var checkBoxHighlightParts);
        checkBoxHighlightParts.IsCheckedChanged += (s, e) => vm.SetChanged();

        var grid = new Grid
        {
            ColumnSpacing = 40,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
        };
        grid.Add(EditorChrome.MakeFieldGrid((labelSingleLineMaxLength, numericUpDownSingleLineMaxLength), (labelMaxNumberOfLines, numericUpDownMaxNumberOfLines)), 0, 0);
        rowHighlightParts.VerticalAlignment = VerticalAlignment.Top;
        grid.Add(rowHighlightParts, 0, 1);

        return EditorChrome.MakeEditorSection(Se.Language.General.Options, grid, first: true);
    }

    private static Border MakeFixesView(MergeShortLinesViewModel vm)
    {
        // Sorting dropped in the DataGrid -> TableView conversion: the grid previews
        // merge candidates in subtitle order.
        var dataGrid = TableViewExtras.MakeTableView().WithAccessibleName(Se.Language.General.Preview);
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
                    CellTemplate = new FuncDataTemplate<MergeShortLinesItem>((item, _) =>
                        new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = new CheckBox
                            {
                                Focusable = false,
                                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(MergeShortLinesItem.Apply))
                                {
                                    Mode = BindingMode.TwoWay,
                                },
                                // The first line of a highlighted group is merged into, not merged: no checkbox.
                                [!Visual.IsVisibleProperty] = new Binding(nameof(MergeShortLinesItem.CanToggle)),
                                HorizontalAlignment = HorizontalAlignment.Center,
                            }
                        }),
                    Width = new GridLength(70),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Binding = new Binding(nameof(MergeShortLinesItem.Number)),
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(60),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Fix,
                    Binding = new Binding(nameof(MergeShortLinesItem.Fix)),
                    Width = new GridLength(1, GridUnitType.Star),
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                },
        });

        // Space toggles the checkbox of every selected row
        TableViewExtras.AddSpaceToggle<MergeShortLinesItem>(dataGrid,
            item => !item.CanToggle || item.Apply,
            (item, value) =>
            {
                if (item.CanToggle)
                {
                    item.Apply = value;
                }
            });

        var commandModifier = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
        dataGrid.ContextMenu = new ContextMenu
        {
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.General.SelectAll,
                    DataContext = vm,
                    Command = vm.SelectAllCommand,
                    InputGesture = new KeyGesture(Key.A, commandModifier),
                },
                new MenuItem
                {
                    Header = Se.Language.General.SelectNone,
                    DataContext = vm,
                    Command = vm.SelectNoneCommand,
                    InputGesture = new KeyGesture(Key.D, commandModifier),
                },
                new MenuItem
                {
                    Header = Se.Language.General.InvertSelection,
                    DataContext = vm,
                    Command = vm.InvertSelectionCommand,
                    InputGesture = new KeyGesture(Key.I, commandModifier | KeyModifiers.Shift),
                },
            },
        };

        // The context menu gestures must mean the same with the grid focused, so take them
        // before the TableView (a ListBox) turns Ctrl+A into "select all rows".
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (vm.HandleFixesSelectionKey(e))
            {
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

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

        var actions = EditorChrome.MakeListActions(EditorChrome.MakeSelectAllButton(vm.SelectAllCommand), EditorChrome.MakeSelectNoneButton(vm.SelectNoneCommand), EditorChrome.MakeInvertSelectionButton(vm.InvertSelectionCommand));

        return EditorChrome.MakeListSection(Se.Language.General.Preview, dataGrid, actions);
    }
}
