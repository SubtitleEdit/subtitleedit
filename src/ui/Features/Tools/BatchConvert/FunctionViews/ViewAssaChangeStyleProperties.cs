using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAssaChangeStyleProperties
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var numericUpDownSpacing = FunctionViewChrome.Compact(UiUtil.MakeNumericUpDownOneDecimal(-100, 100, FunctionViewChrome.FieldWidth, vm, nameof(vm.AssaChangeStylePropertiesSpacing)), Se.Language.Tools.BatchConvert.AssaChangeStylePropertiesSetSpacing);
        var comboBoxAlignment = FunctionViewChrome.Compact(UiUtil.MakeComboBox(vm.AssaChangeStylePropertiesAlignmentOptions, vm, nameof(vm.SelectedAssaChangeStylePropertiesAlignment)), Se.Language.Tools.BatchConvert.AssaChangeStylePropertiesSetAlignment, 120);

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.BatchConvert.AssaChangeStylePropertiesTitle,
            EditorChrome.MakeHint(Se.Language.Tools.BatchConvert.AssaChangeResolutionOnlyAppliesToAssa),
            EditorChrome.MakeHint(Se.Language.Tools.BatchConvert.AssaChangeStylePropertiesInfo),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.AssaChangeStylePropertiesSetSpacing), Se.Language.Tools.BatchConvert.AssaChangeStylePropertiesSetSpacing, numericUpDownSpacing),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.AssaChangeStylePropertiesSetAlignment), Se.Language.Tools.BatchConvert.AssaChangeStylePropertiesSetAlignment, comboBoxAlignment));
    }
}
