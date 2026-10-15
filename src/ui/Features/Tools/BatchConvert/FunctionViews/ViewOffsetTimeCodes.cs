using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewOffsetTimeCodes
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var timeUpDown = new TimeCodeUpDown
        {
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        timeUpDown.Bind(TimeCodeUpDown.ValueProperty, new Binding { Path = nameof(vm.OffsetTimeCodesTime), Mode = BindingMode.TwoWay });
        AutomationProperties.SetName(timeUpDown, Se.Language.General.Offset);

        var radioForward = FunctionViewChrome.Compact(UiUtil.MakeRadioButton(Se.Language.General.Forward, vm, nameof(vm.OffsetTimeCodesForward)));
        var radioBackward = FunctionViewChrome.Compact(UiUtil.MakeRadioButton(Se.Language.General.Backward, vm, nameof(vm.OffsetTimeCodesBack)));

        // Both radio buttons in one parent - they have no group name, so the parent groups them
        var panelDirection = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            Children = { radioForward, radioBackward },
        };

        return FunctionViewChrome.MakePanel(vm, Se.Language.General.OffsetTimeCodes,
            FunctionViewChrome.MakeRow(Se.Language.General.Offset, timeUpDown),
            FunctionViewChrome.MakeRowWithControl(new Panel(), panelDirection));
    }
}
