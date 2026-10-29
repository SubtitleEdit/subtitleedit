using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewSplitBreakLongLines
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var switchSplitLongLines = EditorChrome.MakeSwitchRow(nameof(vm.SplitBreakSplitLongLines), Se.Language.Tools.SplitBreakLongLines.SplitLongLines);
        var switchRebalanceLongLines = EditorChrome.MakeSwitchRow(nameof(vm.SplitBreakRebalanceLongLines), Se.Language.Tools.SplitBreakLongLines.RebalanceLongLines);

        var switchRebalanceOnlyTooLong = FunctionViewChrome.Indented(
            EditorChrome.MakeSwitchRow(nameof(vm.SplitBreakRebalanceOnlyLinesTooLong), Se.Language.Tools.SplitBreakLongLines.RebalanceOnlyLinesTooLong));
        switchRebalanceOnlyTooLong[!InputElement.IsEnabledProperty] = new Binding(nameof(vm.SplitBreakRebalanceLongLines)) { Source = vm };

        var numericUpDownSingleLineMaxLength = FunctionViewChrome.MakeIntField(vm, nameof(vm.SplitBreakSingleLineMaxLength), 5, 1000, 10, Se.Language.Options.Settings.SingleLineMaxLength);
        var numericUpDownMaxNumberOfLines = FunctionViewChrome.MakeIntField(vm, nameof(vm.SplitBreakMaxNumberOfLines), 1, 10, 2, Se.Language.General.MaxNumberOfLines);
        var numericUpDownUnbreakLinesShorterThan = FunctionViewChrome.MakeIntField(vm, nameof(vm.SplitBreakUnbreakLinesShorterThan), 1, 1000, 33, Se.Language.Options.Settings.UnbreakSubtitlesShortThan);
        var rowUnbreakLinesShorterThan = FunctionViewChrome.MakeRow(Se.Language.Options.Settings.UnbreakSubtitlesShortThan, numericUpDownUnbreakLinesShorterThan);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(rowUnbreakLinesShorterThan, Se.Language.Tools.SplitBreakLongLines.UnbreakLinesShorterThanHint);
            ToolTip.SetTip(numericUpDownUnbreakLinesShorterThan, Se.Language.Tools.SplitBreakLongLines.UnbreakLinesShorterThanHint);
            ToolTip.SetTip(switchRebalanceOnlyTooLong, Se.Language.Tools.SplitBreakLongLines.RebalanceOnlyLinesTooLongHint);
        }

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.SplitBreakLongLines.Title,
            switchSplitLongLines,
            switchRebalanceLongLines,
            switchRebalanceOnlyTooLong,
            FunctionViewChrome.MakeRow(Se.Language.Options.Settings.SingleLineMaxLength, numericUpDownSingleLineMaxLength),
            FunctionViewChrome.MakeRow(Se.Language.General.MaxNumberOfLines, numericUpDownMaxNumberOfLines),
            rowUnbreakLinesShorterThan);
    }
}
