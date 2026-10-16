using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewMergeShortLines
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var numericMaxChars = FunctionViewChrome.MakeIntField(vm, nameof(vm.MergeShortLinesMaxCharacters), 10, 500, vm.MergeShortLinesMaxCharacters, Se.Language.Tools.MergeShortLines.MaxCharacters);
        var numericMaxMs = FunctionViewChrome.MakeIntField(vm, nameof(vm.MergeShortLinesMaxMillisecondsBetweenLines), 0, 10000, vm.MergeShortLinesMaxMillisecondsBetweenLines, Se.Language.Tools.MergeShortLines.MaxMillisecondsBetweenLines);

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.MergeShortLines.Title,
            FunctionViewChrome.MakeRow(Se.Language.Tools.MergeShortLines.MaxCharacters, numericMaxChars),
            FunctionViewChrome.MakeRow(Se.Language.Tools.MergeShortLines.MaxMillisecondsBetweenLines, numericMaxMs),
            EditorChrome.MakeSwitchRow(nameof(vm.MergeShortLinesOnlyContinuationLines), Se.Language.Tools.MergeShortLines.OnlyContinuationLines));
    }
}
