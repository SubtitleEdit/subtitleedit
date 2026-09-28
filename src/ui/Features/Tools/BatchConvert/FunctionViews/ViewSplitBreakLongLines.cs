using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewSplitBreakLongLines
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.SplitBreakLongLines.Title,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 50),
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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

        var checkBoxSplitLongLines = UiUtil.MakeCheckBox(Se.Language.Tools.SplitBreakLongLines.SplitLongLines, vm, nameof(vm.SplitBreakSplitLongLines))
            .WithMarginRight(40);

        var checkBoxRebalanceLongLines = UiUtil.MakeCheckBox(Se.Language.Tools.SplitBreakLongLines.RebalanceLongLines, vm, nameof(vm.SplitBreakRebalanceLongLines))
            .WithMarginRight(40);

        var labelSingleLineMaxLength = UiUtil.MakeLabel(Se.Language.Options.Settings.SingleLineMaxLength);
        var numericUpDownSingleLineMaxLength = UiUtil.MakeNumericUpDownInt(5, 1000, 10, 130, vm, nameof(vm.SplitBreakSingleLineMaxLength));

        var labelMaxNumberOfLines = UiUtil.MakeLabel(Se.Language.Options.Settings.MaxLines);
        var numericUpDownMaxNumberOfLines = UiUtil.MakeNumericUpDownInt(1, 10, 2, 130, vm, nameof(vm.SplitBreakMaxNumberOfLines));

        var checkBoxRebalanceOnlyTooLong = UiUtil.MakeCheckBox(Se.Language.Tools.SplitBreakLongLines.RebalanceOnlyLinesTooLong, vm, nameof(vm.SplitBreakRebalanceOnlyLinesTooLong))
            .WithMarginLeft(25)
            .WithMarginRight(40);
        checkBoxRebalanceOnlyTooLong[!InputElement.IsEnabledProperty] = new Binding(nameof(vm.SplitBreakRebalanceLongLines)) { Source = vm };

        var labelUnbreakLinesShorterThan = UiUtil.MakeLabel(Se.Language.Options.Settings.UnbreakSubtitlesShortThan);
        var numericUpDownUnbreakLinesShorterThan = UiUtil.MakeNumericUpDownInt(1, 1000, 33, 130, vm, nameof(vm.SplitBreakUnbreakLinesShorterThan));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(labelUnbreakLinesShorterThan, Se.Language.Tools.SplitBreakLongLines.UnbreakLinesShorterThanHint);
            ToolTip.SetTip(numericUpDownUnbreakLinesShorterThan, Se.Language.Tools.SplitBreakLongLines.UnbreakLinesShorterThanHint);
            ToolTip.SetTip(checkBoxRebalanceOnlyTooLong, Se.Language.Tools.SplitBreakLongLines.RebalanceOnlyLinesTooLongHint);
        }

        grid.Add(labelHeader, 0, 0, 2);

        grid.Add(checkBoxSplitLongLines, 1);
        grid.Add(labelSingleLineMaxLength, 1, 1);
        grid.Add(numericUpDownSingleLineMaxLength, 1, 2);

        grid.Add(checkBoxRebalanceLongLines, 2);
        grid.Add(labelMaxNumberOfLines, 2, 1);
        grid.Add(numericUpDownMaxNumberOfLines, 2, 2);

        grid.Add(checkBoxRebalanceOnlyTooLong, 3);
        grid.Add(labelUnbreakLinesShorterThan, 3, 1);
        grid.Add(numericUpDownUnbreakLinesShorterThan, 3, 2);

        return grid;
    }
}
