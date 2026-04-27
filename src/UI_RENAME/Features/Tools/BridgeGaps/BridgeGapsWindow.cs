using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.BridgeGaps;

public class BridgeGapsWindow : Window
{
    public BridgeGapsWindow(BridgeGapsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BridgeGaps.Title;
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

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private Border MakeSubtitleView(BridgeGapsViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var dataGridSubtitle = new DataGrid
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
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BridgeGapDisplayItem.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BridgeGapDisplayItem.StartTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Duration,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BridgeGapDisplayItem.Duration)) { Converter = shortTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BridgeGapDisplayItem.Text)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.Tools.BridgeGaps.GapChange,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BridgeGapDisplayItem.InfoText)),
                    IsReadOnly = true,
                    Width = new DataGridLength(120),
                },
            },
        };

        return UiUtil.MakeBorderForControlNoPadding(dataGridSubtitle);
    }
}
