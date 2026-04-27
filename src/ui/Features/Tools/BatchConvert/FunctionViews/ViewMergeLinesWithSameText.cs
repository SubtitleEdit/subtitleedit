using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewMergeLinesWithSameText
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.General.MergeLinesWithSameText,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0,0,0, 10),
        };
        
        var labelGap = UiUtil.MakeLabel(Se.Language.Tools.MergeLinesWithSameText.MaxMsBetweenLines);
        var numericUpDownGap = UiUtil.MakeNumericUpDownInt(0, 10000, Se.Settings.Tools.MergeSameText.MaxMillisecondsBetweenLines, 130, vm, nameof(vm.MergeSameTextMaxMillisecondsBetweenLines));
        var checkBoxIncludeIncrementText = UiUtil.MakeCheckBox(Se.Language.Tools.MergeLinesWithSameText.IncludeIncrementingLines, vm, nameof(vm.MergeSameTextIncludeIncrementingLines));
        var panelGap = UiUtil.MakeHorizontalPanel(labelGap, numericUpDownGap);


        var panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children = 
            { 
                labelHeader,
                panelGap,
                checkBoxIncludeIncrementText,
            }
        };

        return panel;
    }
}
