using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewRemoveFormatting
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.General.RemoveFormatting,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0,0,0, 10),
        };

        var checkBoxRemoveAll = UiUtil.MakeCheckBox(Se.Language.General.RemoveAllFormatting, vm, nameof(vm.FormattingRemoveAll));
        var checkBoxRemoveItalic = UiUtil.MakeCheckBox(Se.Language.General.RemoveItalic, vm, nameof(vm.FormattingRemoveItalic));
        var checkBoxRemoveBold = UiUtil.MakeCheckBox(Se.Language.General.RemoveBold, vm, nameof(vm.FormattingRemoveBold));
        var checkBoxRemoveUnderline = UiUtil.MakeCheckBox(Se.Language.General.RemoveUnderline, vm, nameof(vm.FormattingRemoveUnderline));
        var checkBoxRemoveFontTags = UiUtil.MakeCheckBox(Se.Language.General.RemoveFontName, vm, nameof(vm.FormattingRemoveFontTags));
        var checkBoxRemoveAlignmentTags = UiUtil.MakeCheckBox(Se.Language.General.RemoveAlignment, vm, nameof(vm.FormattingRemoveAlignmentTags));
        var checkBoxRemoveColors = UiUtil.MakeCheckBox(Se.Language.General.RemoveColor, vm, nameof(vm.FormattingRemoveColors));

        var panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children = 
            { 
                labelHeader,
                checkBoxRemoveAll,
                checkBoxRemoveItalic,
                checkBoxRemoveBold,
                checkBoxRemoveUnderline,
                checkBoxRemoveFontTags,
                checkBoxRemoveAlignmentTags,
                checkBoxRemoveColors,
            }
        };

        return panel;
    }
}
