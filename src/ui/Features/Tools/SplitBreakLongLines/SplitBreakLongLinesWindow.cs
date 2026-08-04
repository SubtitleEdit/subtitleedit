using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections;

namespace Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;

public class SplitBreakLongLinesWindow : Window
{
    private CheckBox _checkBoxSplitLongLines = null!;

    public SplitBreakLongLinesWindow(SplitBreakLongLinesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.SplitBreakLongLines.Title;
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

        Activated += delegate { _checkBoxSplitLongLines.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
        Loaded += (_, _)  => vm.Loaded();

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Grid MakeControlsView(SplitBreakLongLinesViewModel vm)
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var checkBoxSplitLongLines = UiUtil.MakeCheckBox(Se.Language.Tools.SplitBreakLongLines.SplitLongLines, vm, nameof(vm.SplitLongLines))
            .WithMarginRight(40);
        _checkBoxSplitLongLines = checkBoxSplitLongLines;
        checkBoxSplitLongLines.IsCheckedChanged += (s, e) => vm.SetChanged();

        var checkBoxRebalanceLongLines = UiUtil.MakeCheckBox(Se.Language.Tools.SplitBreakLongLines.RebalanceLongLines, vm, nameof(vm.RebalanceLongLines))
            .WithMarginRight(40);
        checkBoxRebalanceLongLines.IsCheckedChanged += (s, e) => vm.SetChanged();

        var labelSingleLineMaxLength = UiUtil.MakeLabel(Se.Language.Options.Settings.SingleLineMaxLength);
        var numericUpDownSingleLineMaxLength = UiUtil.MakeNumericUpDownInt(5, 1000, 10, 130, vm, nameof(vm.SingleLineMaxLength));
        numericUpDownSingleLineMaxLength.ValueChanged += (s, e) => vm.SetChanged();

        var labelMaxNumberOfLines = UiUtil.MakeLabel(Se.Language.Options.Settings.MaxLines);
        var numericUpDownMaxNumberOfLines = UiUtil.MakeNumericUpDownInt(1, 10, 2, 130, vm, nameof(vm.MaxNumberOfLines));
        numericUpDownMaxNumberOfLines.ValueChanged += (s, e) => vm.SetChanged();

        var labelUnbreakLinesShorterThan = UiUtil.MakeLabel(Se.Language.Options.Settings.UnbreakSubtitlesShortThan);
        var numericUpDownUnbreakLinesShorterThan = UiUtil.MakeNumericUpDownInt(1, 1000, 33, 130, vm, nameof(vm.UnbreakLinesShorterThan));
        numericUpDownUnbreakLinesShorterThan.ValueChanged += (s, e) => vm.SetChanged();
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(labelUnbreakLinesShorterThan, Se.Language.Tools.SplitBreakLongLines.UnbreakLinesShorterThanHint);
            ToolTip.SetTip(numericUpDownUnbreakLinesShorterThan, Se.Language.Tools.SplitBreakLongLines.UnbreakLinesShorterThanHint);
        }

        grid.Add(checkBoxSplitLongLines, 0);
        grid.Add(labelSingleLineMaxLength, 0, 1);
        grid.Add(numericUpDownSingleLineMaxLength, 0, 2);

        grid.Add(checkBoxRebalanceLongLines, 1);
        grid.Add(labelMaxNumberOfLines, 1, 1);
        grid.Add(numericUpDownMaxNumberOfLines, 1, 2);

        grid.Add(labelUnbreakLinesShorterThan, 2, 1);
        grid.Add(numericUpDownUnbreakLinesShorterThan, 2, 2);

        return grid;
    }

    private static Grid MakeFixesView(SplitBreakLongLinesViewModel vm)
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

        // Sorting dropped in the DataGrid -> TableView conversion: the grid previews
        // split/rebalance fixes in subtitle order.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Fixes;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
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
                                    FontSize = 12,
                                    Foreground = new SolidColorBrush(color),
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

        grid.Add(labelFixesAvailable, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 1);

        return grid;
    }
}
