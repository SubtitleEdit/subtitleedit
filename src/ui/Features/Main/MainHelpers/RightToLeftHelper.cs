using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaloniaEdit.Editing;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.MainHelpers;

internal static class RightToLeftHelper
{
    internal static void SetRightToLeftForDataGridAndText(Window window)
    {
        var flowDirection = Se.Settings.Appearance.RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        SetFlowDirectionRecursive(window, flowDirection);
    }

    private static void SetFlowDirectionRecursive(Visual visual, FlowDirection flowDirection)
    {
        if (visual is ComboBox)
        {
            return;
        }

        if (visual is NumericUpDown)
        {
            return;
        }

        if (visual is TimeCodeUpDown)
        {
            return;
        }

        if (visual is SecondsUpDown)
        {
            return;
        }

        if (visual is DataGrid dataGrid)
        {
            dataGrid.FlowDirection = flowDirection;
        }
        else if (visual is TextBox textBox)
        {
            textBox.FlowDirection = flowDirection;
        }
        else if (visual is TextArea textArea)
        {
            textArea.FlowDirection = flowDirection;
        }

        foreach (var child in visual.GetVisualChildren())
        {
            SetFlowDirectionRecursive(child, flowDirection);
        }
    }
}
