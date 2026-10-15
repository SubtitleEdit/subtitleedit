using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewFixCommonErrors
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var buttonShowFixCommonErrorsSettings = FunctionViewChrome.MakeButton("fa-solid fa-sliders", Se.Language.General.Settings, vm.ShowFixCommonRulesCommand);

        return FunctionViewChrome.MakePanel(vm, Se.Language.General.FixCommonErrors,
            buttonShowFixCommonErrorsSettings);
    }
}
