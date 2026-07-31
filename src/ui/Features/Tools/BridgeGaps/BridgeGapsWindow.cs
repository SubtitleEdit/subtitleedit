using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Collections;

namespace Nikse.SubtitleEdit.Features.Tools.BridgeGaps;

public class BridgeGapsWindow : Window
{
    public BridgeGapsWindow(BridgeGapsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.BridgeGaps;
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 900;
        MinHeight = 500;
        vm.Window = this;
        DataContext = vm;

        var labelBridgeGapSmallerThan = UiUtil.MakeLabel(Se.Language.Tools.BridgeGaps.BridgeGapsSmallerThan);
        var numericUpDownBridgeGapSmallerThan = UiUtil.MakeNumericUpDownInt(1, 10000, Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs, 130, vm, nameof(vm.BridgeGapsSmallerThanMs));
        numericUpDownBridgeGapSmallerThan.ValueChanged += vm.ValueChanged;

        var labelMinGap = UiUtil.MakeLabel(Se.Language.Tools.BridgeGaps.MinGap);
        var numericUpDownMinGap = UiUtil.MakeNumericUpDownInt(0, 1000, Se.Settings.Tools.BridgeGaps.MinGapMs, 130, vm, nameof(vm.MinGapMs));
        numericUpDownMinGap.ValueChanged += vm.ValueChanged;

        var labelPercentForLeft = UiUtil.MakeLabel(Se.Language.Tools.BridgeGaps.PercentFoPrevious);
        var numericUpDownPercentForLeft = UiUtil.MakeNumericUpDownInt(0, 100, Se.Settings.Tools.BridgeGaps.PercentForLeft, 130, vm, nameof(vm.PercentForLeft));
        numericUpDownPercentForLeft.ValueChanged += vm.ValueChanged;

        var panelControls = UiUtil.MakeHorizontalPanel(
            labelBridgeGapSmallerThan,
            numericUpDownBridgeGapSmallerThan,
            labelMinGap,
            numericUpDownMinGap,
            labelPercentForLeft,
            numericUpDownPercentForLeft);

        var subtitleView = MakeSubtitleView(vm);

        var labelStatus = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.StatusText)).WithAlignmentTop();

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Bridge gap smaller than
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Subtitle view
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
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

        grid.Add(panelControls, 0);
        grid.Add(subtitleView, 1);
        grid.Add(labelStatus, 2);
        grid.Add(panelButtons, 2);

        Content = grid;

        Activated += delegate { numericUpDownBridgeGapSmallerThan.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Border MakeSubtitleView(BridgeGapsViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        // No header-click sorting (the DataGrid's CanUserSortColumns is not carried
        // over): gap-change previews in subtitle order.
        var dataGridSubtitle = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridSubtitle.Width = double.NaN;
        dataGridSubtitle.Height = double.NaN;
        dataGridSubtitle.DataContext = vm;
        dataGridSubtitle.ItemsSource = vm.Subtitles;
        dataGridSubtitle.Columns.AddRange(new[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.NumberSymbol,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(BridgeGapDisplayItem.Number)),
                Width = new GridLength(60), // content-sized (Auto) on the DataGrid; TableView treats Auto as star
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Show,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(BridgeGapDisplayItem.StartTime)) { Converter = fullTimeConverter },
                Width = new GridLength(115),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Duration,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(BridgeGapDisplayItem.Duration)) { Converter = shortTimeConverter },
                Width = new GridLength(90),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Text,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(BridgeGapDisplayItem.Text)),
                Width = new GridLength(1, GridUnitType.Star),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.Tools.BridgeGaps.GapChange,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(BridgeGapDisplayItem.InfoText)),
                Width = new GridLength(120),
            },
        });

        dataGridSubtitle.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGridSubtitle.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                if (target == null)
                {
                    return;
                }

                dataGridSubtitle.SelectedItem = target;
                dataGridSubtitle.ScrollIntoView(target);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        return UiUtil.MakeBorderForControlNoPadding(dataGridSubtitle);
    }
}
