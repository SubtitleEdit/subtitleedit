using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewFixRightToLeft
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.General.FixRightToLeft,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0,0,0, 10),
        };

        var radioButtonRtlFixViaUnicode = UiUtil.MakeRadioButton(Se.Language.General.FixRightToLeftViaUnicodeTags, vm, nameof(vm.RtlFixViaUniCode) , "rtl").WithMarginLeft(20);
        var radioButtonRtlRemoveUnicode = UiUtil.MakeRadioButton(Se.Language.General.RemoveRightToLeftUnicodeTags, vm, nameof(vm.RtlRemoveUniCode), "rtl").WithMarginLeft(20);
        var radioButtonRtlReverseStartEnd = UiUtil.MakeRadioButton(Se.Language.General.ReverseRightToLeftStartEnd, vm, nameof(vm.RtlReverseStartEnd), "rtl").WithMarginLeft(20);

        var panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children = 
            { 
                labelHeader,
                radioButtonRtlFixViaUnicode,
                radioButtonRtlRemoveUnicode,
                radioButtonRtlReverseStartEnd,
            }
        };

        return panel;
    }
}
