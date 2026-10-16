using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewRemoveLineBreaks
{
    public static Control Make(BatchConvertViewModel vm)
    {
        return FunctionViewChrome.MakePanel(vm, Se.Language.General.UnbreakLines,
            EditorChrome.MakeSwitchRow(nameof(vm.RemoveLineBreaksOnlyShortLines), Se.Language.Tools.FixCommonErrors.RemoveLineBreaks));
    }
}
