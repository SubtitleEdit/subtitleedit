using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewDeleteLines
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var numericUpDownDeleteFirstLines = FunctionViewChrome.MakeIntField(vm, nameof(vm.DeleteXFirstLines), 0, 100, 0, Se.Language.General.DeleteFirstLines);
        var numericUpDownDeleteLastLines = FunctionViewChrome.MakeIntField(vm, nameof(vm.DeleteXLastLines), 0, 100, 0, Se.Language.General.DeleteLastLines);
        var textBoxDeleteContains = EditorChrome.MakeTextField(nameof(vm.DeleteLinesContains), Se.Language.General.DeleteLinesContainingText);
        var textBoxDeleteActorsOrStyles = EditorChrome.MakeTextField(nameof(vm.DeleteActorsOrStyles), Se.Language.Tools.BatchConvert.DeleteLinesWithSpecificActorsOrStyles);

        return FunctionViewChrome.MakePanel(vm, Se.Language.General.DeleteLines,
            FunctionViewChrome.MakeRow(Se.Language.General.DeleteFirstLines, numericUpDownDeleteFirstLines),
            FunctionViewChrome.MakeRow(Se.Language.General.DeleteLastLines, numericUpDownDeleteLastLines),
            FunctionViewChrome.MakeRow(Se.Language.General.DeleteLinesContainingText, textBoxDeleteContains),
            FunctionViewChrome.MakeRow(Se.Language.Tools.BatchConvert.DeleteLinesWithSpecificActorsOrStyles, textBoxDeleteActorsOrStyles));
    }
}
