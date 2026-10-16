using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewMergeLinesWithSameTimeCodes
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var numericUpDownMaxDiff = FunctionViewChrome.MakeIntField(vm, nameof(vm.MergeSameTimeMaxMillisecondsDifference), 0, 10000, Se.Settings.Tools.MergeSameTimeCode.MaxMillisecondsDifference, Se.Language.Tools.MergeLinesWithSameTimeCodes.MaxMsDifference);

        return FunctionViewChrome.MakePanel(vm, Se.Language.General.MergeLinesWithSameTimeCodes,
            FunctionViewChrome.MakeRow(Se.Language.Tools.MergeLinesWithSameTimeCodes.MaxMsDifference, numericUpDownMaxDiff),
            EditorChrome.MakeSwitchRow(nameof(vm.MergeSameTimeMergeDialog), Se.Language.Tools.MergeLinesWithSameTimeCodes.MakeDialog),
            EditorChrome.MakeSwitchRow(nameof(vm.MergeSameTimeAutoBreak), Se.Language.General.AutoBreak));
    }
}
