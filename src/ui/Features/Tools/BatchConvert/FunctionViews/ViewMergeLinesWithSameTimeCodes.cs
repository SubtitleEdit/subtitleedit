using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewMergeLinesWithSameTimeCodes
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.General.MergeLinesWithSameTimeCodes,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0,0,0, 10),
        };
        
        var labelMaxDiff = UiUtil.MakeLabel(Se.Language.Tools.MergeLinesWithSameTimeCodes.MaxMsDifference);
        var numericUpDownMaxDiff = UiUtil.MakeNumericUpDownInt(0, 10000, Se.Settings.Tools.MergeSameTimeCode.MaxMillisecondsDifference, 130, vm, nameof(vm.MergeSameTimeMaxMillisecondsDifference));
        var checkBoxMergeAsDialog = UiUtil.MakeCheckBox(Se.Language.Tools.MergeLinesWithSameTimeCodes.MakeDialog, vm, nameof(vm.MergeSameTimeMergeDialog));
        var checkBoxAutoBreak = UiUtil.MakeCheckBox(Se.Language.General.AutoBreak, vm, nameof(vm.MergeSameTimeAutoBreak));
        var panelHor = UiUtil.MakeHorizontalPanel(labelMaxDiff, numericUpDownMaxDiff);

        var panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children = 
            { 
                labelHeader,
                panelHor,
                checkBoxMergeAsDialog,
                checkBoxAutoBreak
            }
        };

        return panel;
    }
}
