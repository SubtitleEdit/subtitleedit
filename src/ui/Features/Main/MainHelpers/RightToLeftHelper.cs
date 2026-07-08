using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaloniaEdit.Editing;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.MainHelpers;

internal static class RightToLeftHelper
{
    internal static void SetRightToLeftForDataGridAndText(Window window)
    {
        var flowDirection = Se.Settings.Appearance.RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        SetFlowDirectionRecursive(window, flowDirection);
    }

    /// <summary>
    /// In right to left mode a text control follows its content: text with right
    /// to left letters (Arabic, Hebrew, and so on) stays right to left, text in a
    /// left to right script (for example a Turkish or English original subtitle)
    /// aligns left to right. Empty controls keep the requested direction so typing
    /// in a right to left language starts on the correct side.
    /// </summary>
    internal static FlowDirection GetContentDirection(string? text, FlowDirection requested)
    {
        if (requested != FlowDirection.RightToLeft || string.IsNullOrWhiteSpace(text))
        {
            return requested;
        }

        return LanguageAutoDetect.ContainsRightToLeftLetter(text)
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
    }

    /// <summary>
    /// Keeps a text box's flow direction following its content while right to left
    /// mode is enabled: right to left content stays right to left, left to right
    /// content (for example a Turkish original next to an Arabic working language)
    /// aligns left to right. Attach once when the text box is created; text set
    /// while right to left mode is off is left alone.
    /// </summary>
    internal static void FollowContentDirection(TextBox textBox)
    {
        textBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.TextProperty && Se.Settings.Appearance.RightToLeft)
            {
                textBox.FlowDirection = GetContentDirection(textBox.Text, FlowDirection.RightToLeft);
            }
        };
    }

    /// <summary>
    /// Mirrors the main text edit grid so the current and original text boxes keep
    /// matching the mirrored subtitle grid columns (issue #12249). Setting
    /// FlowDirection on a grid does not rearrange its children, so the columns are
    /// swapped for real: the column definitions are reversed (the original column's
    /// width binding travels with its definition object) and every child is
    /// remapped to the mirrored column index. Idempotent via the grid's Tag.
    /// </summary>
    private static void MirrorTextEditGrid(Grid grid, FlowDirection flowDirection)
    {
        var wantMirrored = flowDirection == FlowDirection.RightToLeft;
        var isMirrored = Equals(grid.Tag, "mirrored");
        if (wantMirrored == isMirrored)
        {
            return;
        }

        grid.Tag = wantMirrored ? "mirrored" : null;

        var definitions = grid.ColumnDefinitions.ToList();
        grid.ColumnDefinitions.Clear();
        definitions.Reverse();
        foreach (var definition in definitions)
        {
            grid.ColumnDefinitions.Add(definition);
        }

        var last = definitions.Count - 1;
        foreach (var child in grid.Children)
        {
            Grid.SetColumn(child, last - Grid.GetColumn(child));
        }
    }

    /// <summary>
    /// Re-runs the subtitle grid's cell bindings after the right to left mode
    /// changed: the per line flow direction converter reads the setting, and
    /// bindings do not re-evaluate on their own when a setting changes. Rebinding
    /// the items source re-creates the rows; selection and scroll position are
    /// restored.
    /// </summary>
    internal static void RefreshDataGridBindings(DataGrid? grid, System.Collections.IEnumerable? itemsSource, object? selected)
    {
        if (grid == null)
        {
            return;
        }

        grid.ItemsSource = null;
        grid.ItemsSource = itemsSource;
        if (selected != null)
        {
            grid.SelectedItem = selected;
            grid.ScrollIntoView(selected, null);
        }
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
        else if (visual is Grid grid && grid.Name == "SubtitleTextEditGrid")
        {
            MirrorTextEditGrid(grid, flowDirection);
        }
        else if (visual is TextBox textBox)
        {
            textBox.FlowDirection = GetContentDirection(textBox.Text, flowDirection);
        }
        else if (visual is TextArea textArea)
        {
            textArea.FlowDirection = GetContentDirection(textArea.Document?.Text, flowDirection);
        }

        foreach (var child in visual.GetVisualChildren())
        {
            SetFlowDirectionRecursive(child, flowDirection);
        }
    }
}
