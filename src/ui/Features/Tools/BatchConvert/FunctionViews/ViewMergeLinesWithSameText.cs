using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewMergeLinesWithSameText
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var numericUpDownGap = FunctionViewChrome.MakeIntField(vm, nameof(vm.MergeSameTextMaxMillisecondsBetweenLines), 0, 10000, Se.Settings.Tools.MergeSameText.MaxMillisecondsBetweenLines, Se.Language.Tools.MergeLinesWithSameText.MaxMsBetweenLines);

        return FunctionViewChrome.MakePanel(vm, Se.Language.General.MergeLinesWithSameText,
            FunctionViewChrome.MakeRow(Se.Language.Tools.MergeLinesWithSameText.MaxMsBetweenLines, numericUpDownGap),
            EditorChrome.MakeSwitchRow(nameof(vm.MergeSameTextIncludeIncrementingLines), Se.Language.Tools.MergeLinesWithSameText.IncludeIncrementingLines),
            EditorChrome.MakeSwitchRow(nameof(vm.MergeSameTextIncludeRollUpCaptions), Se.Language.Tools.MergeLinesWithSameText.IncludeRollUpCaptions));
    }
}
