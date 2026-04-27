using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAddFormatting
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.BatchConvert.AddFormatting,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var checkBoxAddItalic = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.AddItalic, vm, nameof(vm.FormattingAddItalic));
        var checkBoxAddBold = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.AddBold, vm, nameof(vm.FormattingAddBold));
        var checkBoxAddUnderline = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.AddUnderline, vm, nameof(vm.FormattingAddUnderline));

        var checkBoxAddAlignmentTag = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.AddAlignment, vm, nameof(vm.FormattingAddAlignmentTag));
        var comboBoxAlignment = UiUtil.MakeComboBox(vm.AlignmentTagOptions, vm, nameof(vm.SelectedAlignmentTagOption));
        var panelAlignment = UiUtil.MakeHorizontalPanel(checkBoxAddAlignmentTag, comboBoxAlignment);

        var checkBoxAddColor = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.AddColor, vm, nameof(vm.FormattingAddColor));
        var colorPickerAdd = new ColorPicker()
        {
            Width = 200,
            IsAlphaEnabled = true,
            IsAlphaVisible = true,
            IsColorSpectrumSliderVisible = false,
            IsColorComponentsVisible = true,
            IsColorModelVisible = false,
            IsColorPaletteVisible = false,
            IsAccentColorsVisible = false,
            IsColorSpectrumVisible = true,
            IsComponentTextInputVisible = true,
            [!ColorPicker.ColorProperty] = new Binding(nameof(vm.FormattingAddColorValue))
            {
                Source = vm,
                Mode = BindingMode.TwoWay
            },
        };
        var panelColor = UiUtil.MakeHorizontalPanel(checkBoxAddColor, colorPickerAdd);

        var panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                labelHeader,
                checkBoxAddItalic,
                checkBoxAddBold,
                checkBoxAddUnderline,
                panelAlignment,
                panelColor,
            }
        };

        return panel;
    }
}
