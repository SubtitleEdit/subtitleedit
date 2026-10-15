using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewConvertColorsToDialog
{
    public static Control Make(BatchConvertViewModel vm)
    {
        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.BatchConvert.ConvertColorsToDialogTitle,
            FunctionViewChrome.MakeSwitchRow(nameof(vm.ConvertColorsToDialogRemoveColorTags), Se.Language.Tools.BatchConvert.ConvertColorsToDialogRemoveColorTags),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.ConvertColorsToDialogAddNewLines), Se.Language.Tools.BatchConvert.ConvertColorsToDialogAddNewLines),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.ConvertColorsToDialogReBreakLines), Se.Language.Tools.BatchConvert.ConvertColorsToDialogReBreakLines));
    }
}
