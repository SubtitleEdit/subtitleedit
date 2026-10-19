using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewConvertColorsToDialog
{
    public static Control Make(BatchConvertViewModel vm)
    {
        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.BatchConvert.ConvertColorsToDialogTitle,
            EditorChrome.MakeSwitchRow(nameof(vm.ConvertColorsToDialogRemoveColorTags), Se.Language.Tools.BatchConvert.ConvertColorsToDialogRemoveColorTags),
            EditorChrome.MakeSwitchRow(nameof(vm.ConvertColorsToDialogAddNewLines), Se.Language.Tools.BatchConvert.ConvertColorsToDialogAddNewLines),
            EditorChrome.MakeSwitchRow(nameof(vm.ConvertColorsToDialogReBreakLines), Se.Language.Tools.BatchConvert.ConvertColorsToDialogReBreakLines));
    }
}
