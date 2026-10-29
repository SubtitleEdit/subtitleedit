using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAdjustImageColors
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var colorPicker = UiUtil.MakeColorPickerButton(vm, nameof(vm.ImageAdjustColorValue));
        colorPicker[!Control.IsEnabledProperty] = new Binding(nameof(vm.ImageAdjustColorOn)) { Source = vm };
        AutomationProperties.SetName(colorPicker, Se.Language.Tools.ImageBasedEdit.AdjustColor);

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.BatchConvert.AdjustImageColorsTitle,
            EditorChrome.MakeHint(Se.Language.Tools.BatchConvert.AdjustImageColorsInfo),

            EditorChrome.MakeSwitchRow(nameof(vm.ImageAdjustBrightnessOn), Se.Language.Tools.ImageBasedEdit.AdjustBrightness),
            MakePair(vm, Se.Language.General.Brightness, nameof(vm.ImageAdjustBrightness), nameof(vm.ImageAdjustBrightnessOn), -100, 100, 1),
            MakePair(vm, Se.Language.Tools.ImageBasedEdit.Contrast, nameof(vm.ImageAdjustContrast), nameof(vm.ImageAdjustBrightnessOn), -100, 100, 1),
            MakePair(vm, Se.Language.Tools.ImageBasedEdit.Gamma + " (%)", nameof(vm.ImageAdjustGamma), nameof(vm.ImageAdjustBrightnessOn), 20, 400, 5),

            EditorChrome.MakeSwitchRow(nameof(vm.ImageAdjustAlphaOn), Se.Language.General.AdjustAlpha),
            MakePair(vm, Se.Language.General.AlphaAdjustment, nameof(vm.ImageAdjustAlpha), nameof(vm.ImageAdjustAlphaOn), -255, 255, 5),
            MakePair(vm, Se.Language.General.AlphaThreshold, nameof(vm.ImageAdjustAlphaThreshold), nameof(vm.ImageAdjustAlphaOn), 0, 255, 5),

            FunctionViewChrome.MakeSwitchRow(nameof(vm.ImageAdjustColorOn), Se.Language.Tools.ImageBasedEdit.AdjustColor, colorPicker));
    }

    /// <summary>
    /// A value of the switch above it: indented, and only enabled while the switch is on.
    /// </summary>
    private static Grid MakePair(BatchConvertViewModel vm, string labelText, string bindingProperty, string enabledProperty, int minimum, int maximum, int increment)
    {
        var field = FunctionViewChrome.Compact(new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
            Increment = increment,
            FormatString = "0",
            [!NumericUpDown.ValueProperty] = new Binding(bindingProperty) { Source = vm, Mode = BindingMode.TwoWay },
            [!Control.IsEnabledProperty] = new Binding(enabledProperty) { Source = vm },
        }, labelText);

        // Indented under the switch; the label column is narrowed by the indent so the
        // fields still line up with the other rows.
        var label = EditorChrome.MakeDimLabel(labelText, new Thickness(FunctionViewChrome.Indent, 0, 0, 0));
        label.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        return FunctionViewChrome.MakeRowWithControl(label, field);
    }
}
