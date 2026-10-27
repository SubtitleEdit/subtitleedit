using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewChangeSpeed
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var numericUpDownSpeed = FunctionViewChrome.Compact(new NumericUpDown
        {
            Minimum = 1, // a speed of 0% means a 100/0 factor (divide by zero / infinite times)
            Maximum = 1000,
            Increment = 0.1m,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.ChangeSpeedPercent)) { Mode = BindingMode.TwoWay },
        }, Se.Language.Sync.SpeedInPercentage);

        var buttonFromDropFrame = FunctionViewChrome.MakeButton(null, Se.Language.Sync.FromDropFrameValue, vm.ChangeSpeedSetFromDropFrameValueCommand);
        var buttonToDropFrame = FunctionViewChrome.MakeButton(null, Se.Language.Sync.ToDropFrameValue, vm.ChangeSpeedSetToDropFrameValueCommand);

        var panelDropFrame = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            ItemSpacing = 8,
            LineSpacing = 8,
            Children =
            {
                buttonFromDropFrame,
                buttonToDropFrame,
            }
        };

        return FunctionViewChrome.MakePanel(vm, Se.Language.General.ChangeSpeed,
            FunctionViewChrome.MakeRow(Se.Language.Sync.SpeedInPercentage, numericUpDownSpeed),
            FunctionViewChrome.MakeRowWithControl(new Panel(), panelDropFrame));
    }
}
