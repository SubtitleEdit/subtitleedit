using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewRemoveLineBreaks
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.General.UnbreakLines,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var checkBoxOnlyShortLines = UiUtil.MakeCheckBox(Se.Language.Tools.FixCommonErrors.RemoveLineBreaks, vm, nameof(vm.RemoveLineBreaksOnlyShortLines));

        return new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children = { labelHeader, checkBoxOnlyShortLines }
        };
    }
}
