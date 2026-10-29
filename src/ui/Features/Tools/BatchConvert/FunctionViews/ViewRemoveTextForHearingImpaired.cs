using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewRemoveTextForHearingImpaired
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var buttonSettings = FunctionViewChrome.MakeButton("fa-solid fa-sliders", Se.Language.General.Settings, vm.ShowRemoveTextForHearingImpairedSettingsCommand);

        return FunctionViewChrome.MakePanel(vm, Se.Language.General.RemoveTextForHearingImpaired,
            buttonSettings);
    }
}
