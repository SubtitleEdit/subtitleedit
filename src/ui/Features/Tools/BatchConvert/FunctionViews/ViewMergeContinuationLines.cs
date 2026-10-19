using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewMergeContinuationLines
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var numericMaxMs = FunctionViewChrome.MakeIntField(vm, nameof(vm.MergeContinuationLinesMaxMillisecondsBetweenLines), 0, 10000, vm.MergeContinuationLinesMaxMillisecondsBetweenLines, Se.Language.Tools.MergeContinuationLines.MaxMillisecondsBetweenLines);
        var numericMaxChars = FunctionViewChrome.MakeIntField(vm, nameof(vm.MergeContinuationLinesMaxCharacters), 20, 1000, vm.MergeContinuationLinesMaxCharacters, Se.Language.Tools.MergeContinuationLines.MaxCharacters);

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.MergeContinuationLines.Title,
            FunctionViewChrome.MakeRow(Se.Language.Tools.MergeContinuationLines.MaxMillisecondsBetweenLines, numericMaxMs),
            FunctionViewChrome.MakeRow(Se.Language.Tools.MergeContinuationLines.MaxCharacters, numericMaxChars));
    }
}
