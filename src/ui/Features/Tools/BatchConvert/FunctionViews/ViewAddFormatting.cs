using Avalonia.Automation;
using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAddFormatting
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var comboBoxAlignment = FunctionViewChrome.Compact(UiUtil.MakeComboBox(vm.AlignmentTagOptions, vm, nameof(vm.SelectedAlignmentTagOption)), Se.Language.Tools.BatchConvert.AddAlignment, 120);

        var colorPickerAdd = UiUtil.MakeColorPickerButton(vm, nameof(vm.FormattingAddColorValue));
        AutomationProperties.SetName(colorPickerAdd, Se.Language.Tools.BatchConvert.AddColor);

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.BatchConvert.AddFormatting,
            EditorChrome.MakeSwitchRow(nameof(vm.FormattingAddItalic), Se.Language.Tools.BatchConvert.AddItalic),
            EditorChrome.MakeSwitchRow(nameof(vm.FormattingAddBold), Se.Language.Tools.BatchConvert.AddBold),
            EditorChrome.MakeSwitchRow(nameof(vm.FormattingAddUnderline), Se.Language.Tools.BatchConvert.AddUnderline),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.FormattingAddAlignmentTag), Se.Language.Tools.BatchConvert.AddAlignment, comboBoxAlignment),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.FormattingAddColor), Se.Language.Tools.BatchConvert.AddColor, colorPickerAdd));
    }
}
