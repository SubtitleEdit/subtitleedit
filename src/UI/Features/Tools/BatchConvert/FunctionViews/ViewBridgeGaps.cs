using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewBridgeGaps
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.BridgeGaps.Title,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelBridgeGapSmallerThan = UiUtil.MakeLabel(Se.Language.Tools.BridgeGaps.BridgeGapsSmallerThan);
        var numericUpDownBridgeGapSmallerThan = UiUtil.MakeNumericUpDownInt(1, 10000, Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs, 130, vm, nameof(vm.BridgeGapsSmallerThanMs));

        var labelMinGap = UiUtil.MakeLabel(Se.Language.Tools.BridgeGaps.MinGap);
        var numericUpDownMinGap = UiUtil.MakeNumericUpDownInt(0, 1000, Se.Settings.Tools.BridgeGaps.MinGapMs, 130, vm, nameof(vm.BridgeGapsMinGapMs));

        var labelPercentForLeft = UiUtil.MakeLabel(Se.Language.Tools.BridgeGaps.PercentFoPrevious);
        var numericUpDownPercentForLeft = UiUtil.MakeNumericUpDownInt(0, 100, Se.Settings.Tools.BridgeGaps.PercentForLeft, 130, vm, nameof(vm.BridgeGapsPercentForLeft));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Header
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Bridge gap smaller than
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Min gap
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Percent for left
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Avalonia.Thickness(10),
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        grid.Add(labelHeader, 0);

        grid.Add(labelBridgeGapSmallerThan, 1);
        grid.Add(numericUpDownBridgeGapSmallerThan, 1, 1);

        grid.Add(labelMinGap, 2);
        grid.Add(numericUpDownMinGap, 2, 1);

        grid.Add(labelPercentForLeft, 3);
        grid.Add(numericUpDownPercentForLeft, 3, 1);

        return grid;
    }
}
