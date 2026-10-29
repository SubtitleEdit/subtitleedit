using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewApplyMinGap
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var numericUpDownMinGap = FunctionViewChrome.MakeIntField(vm, nameof(vm.MinGapMs), 0, 1000, Se.Settings.Tools.ApplyMinGapMilliseconds, Se.Language.Tools.ApplyMinGaps.MinMsBetweenLines);

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.ApplyMinGaps.Title,
            FunctionViewChrome.MakeRow(Se.Language.Tools.ApplyMinGaps.MinMsBetweenLines, numericUpDownMinGap));
    }
}
