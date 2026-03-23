using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewDeleteLines
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.General.DeleteLines,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelDeleteContains = UiUtil.MakeLabel(Se.Language.General.DeleteLinesContainingText);
        var textBoxDeleteContains = UiUtil.MakeTextBox(400, vm, nameof(vm.DeleteLinesContains));
        var panelDeleteContains = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children = { labelDeleteContains, textBoxDeleteContains },
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelDeleteFirstLines = UiUtil.MakeLabel(Se.Language.General.DeleteFirstLines);
        var numericUpDownDeleteFirstLines = UiUtil.MakeNumericUpDownInt(0, 100, 0, 150, vm, nameof(vm.DeleteXFirstLines));
        var panelDeleteFirstLines = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children = { labelDeleteFirstLines, numericUpDownDeleteFirstLines },
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelDeleteLastLines = UiUtil.MakeLabel(Se.Language.General.DeleteLastLines);
        var numericUpDownDeleteLastLines = UiUtil.MakeNumericUpDownInt(0, 100, 0, 150, vm, nameof(vm.DeleteXLastLines));
        var panelDeleteLastLines = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children = { labelDeleteLastLines, numericUpDownDeleteLastLines },
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelDeleteActorsOrStyles = UiUtil.MakeLabel(Se.Language.Tools.BatchConvert.DeleteLinesWithSpecificActorsOrStyles);
        var textBoxDeleteActorsOrStyles = UiUtil.MakeTextBox(400, vm, nameof(vm.DeleteActorsOrStyles));
        var panelDeleteActorsOrStyles = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children = { labelDeleteActorsOrStyles, textBoxDeleteActorsOrStyles },
            Margin = new Avalonia.Thickness(0, 10, 0, 10),
        };

        var panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                labelHeader,
                panelDeleteFirstLines,
                panelDeleteLastLines,
                panelDeleteContains,
                panelDeleteActorsOrStyles,
            }
        };

        return panel;
    }
}
