using Avalonia.Controls;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewOffsetTimeCodes
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.General.OffsetTimeCodes,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0,0,0, 10),
        };

        var labelTimeCodeFormat = new Label
        {
            Content = Se.Language.General.Offset,
        };

        var timeUpDown = new TimeCodeUpDown();

        var panelTimeCode = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children = 
            { 
                labelTimeCodeFormat,
                timeUpDown
            }
        };


        var radioForward = UiUtil.MakeRadioButton(Se.Language.General.Forward, vm, nameof(vm.OffsetTimeCodesForward));
        var radioBackward = UiUtil.MakeRadioButton(Se.Language.General.Backward, vm, nameof(vm.OffsetTimeCodesBack));

        var panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children = 
            { 
                labelHeader,
                panelTimeCode,
                radioForward,
                radioBackward
            }
        };

        return panel;
    }
}
