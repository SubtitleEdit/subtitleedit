using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewConvertColorsToDialog
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.BatchConvert.ConvertColorsToDialogTitle,
            FontWeight = FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var checkBoxRemoveColorTags = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.ConvertColorsToDialogRemoveColorTags, vm, nameof(vm.ConvertColorsToDialogRemoveColorTags));
        var checkBoxAddNewLines = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.ConvertColorsToDialogAddNewLines, vm, nameof(vm.ConvertColorsToDialogAddNewLines));
        var checkBoxReBreakLines = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.ConvertColorsToDialogReBreakLines, vm, nameof(vm.ConvertColorsToDialogReBreakLines));

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Avalonia.Thickness(10),
            Spacing = 5,
            Children =
            {
                labelHeader,
                checkBoxRemoveColorTags,
                checkBoxAddNewLines,
                checkBoxReBreakLines,
            }
        };
    }
}
