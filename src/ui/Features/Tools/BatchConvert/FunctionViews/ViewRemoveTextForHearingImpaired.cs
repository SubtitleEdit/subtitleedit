using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewRemoveTextForHearingImpaired
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.General.RemoveTextForHearingImpaired,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0,0,0, 10),
        };
        
        var buttonSettings = UiUtil.MakeButton(vm.ShowRemoveTextForHearingImpairedSettingsCommand, IconNames.Settings);

        var panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children = 
            { 
                labelHeader,
                buttonSettings,
            }
        };

        return panel;
    }
}
