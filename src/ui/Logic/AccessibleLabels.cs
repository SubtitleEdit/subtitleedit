using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Gives input controls an accessible name from the visible label next to them.
///
/// SE's tool windows follow one visual convention: a label to the left of (or above) the
/// control it describes - a label/control pair in a grid row, or a label followed by the
/// control in a stack panel. Sighted users read the pairing from the layout, but UI
/// Automation knows nothing about it, so a screen reader announced "190, spin button" or
/// "combo box" with no hint of which setting was focused (#12087). Rather than wiring
/// <c>AutomationProperties.LabeledBy</c> by hand in every window, this walks a window once
/// after it opens and links each unnamed input to the label the layout puts next to it.
/// An explicit name or LabeledBy set by the window always wins - this only fills gaps.
/// </summary>
public static class AccessibleLabels
{
    /// <summary>
    /// Labels every unnamed input control under <paramref name="root"/>. Returns the
    /// controls that were linked to a label.
    /// </summary>
    public static List<Control> Apply(Control root)
    {
        var labeled = new List<Control>();
        foreach (var control in root.GetLogicalDescendants().OfType<Control>().ToList())
        {
            if (!IsInput(control) || HasAccessibleName(control))
            {
                continue;
            }

            // Template parts (e.g. the text box inside a NumericUpDown) get their name
            // forwarded from their templated parent; label the parent instead.
            if (control.TemplatedParent != null)
            {
                continue;
            }

            var label = FindLabel(control);
            if (label != null)
            {
                AutomationProperties.SetLabeledBy(control, label);
                labeled.Add(control);
                continue;
            }

            // A placeholder text is the label sighted users see in an unlabeled box.
            if (control is TextBox { Watermark.Length: > 0 and <= 80 } textBox)
            {
                AutomationProperties.SetName(control, textBox.Watermark);
                labeled.Add(control);
            }
        }

        // A window with a single list (the pick-a-track/pick-a-font dialogs) is about that
        // list, so the window title names it.
        if (root is Window { Title.Length: > 0 } window)
        {
            var lists = root.GetLogicalDescendants().OfType<Control>()
                .Where(c => c is ListBox && c.TemplatedParent == null).ToList();
            if (lists.Count == 1 && !HasAccessibleName(lists[0]))
            {
                AutomationProperties.SetName(lists[0], window.Title);
                labeled.Add(lists[0]);
            }
        }

        return labeled;
    }

    /// <summary>The input controls a screen reader user tabs to that need a name.</summary>
    public static bool IsInput(Control control)
    {
        return control is TextBox or ComboBox or NumericUpDown or Slider or AutoCompleteBox
            or SecondsUpDown or TimeCodeUpDown or ListBox or DatePicker or TimePicker;
    }

    public static bool HasAccessibleName(Control control)
    {
        return !string.IsNullOrEmpty(AutomationProperties.GetName(control))
               || AutomationProperties.GetLabeledBy(control) != null;
    }

    /// <summary>
    /// Finds the label the layout pairs with <paramref name="control"/>: the closest
    /// label-like sibling to the left in the same grid row, the label in the cell above,
    /// or the previous sibling in a stack/wrap panel. When the control is the first child
    /// of a wrapper (e.g. a combo box plus a button in a horizontal panel that sits in the
    /// grid cell), the search continues one level up for the wrapper.
    /// </summary>
    public static Control? FindLabel(Control control)
    {
        Control node = control;
        for (var depth = 0; depth < 3; depth++)
        {
            if (node.Parent is not Control parent)
            {
                return null;
            }

            var label = FindLabelInParent(parent, node);
            if (label != null)
            {
                return label;
            }

            if (parent is Panel panel)
            {
                // Only the first input in a wrapper inherits the wrapper's label - a second
                // input (or one after a nested panel) would otherwise get a label meant for
                // its predecessor.
                var index = panel.Children.IndexOf(node);
                if (panel.Children.Take(index).Any(c => IsInput(c) || c is Panel))
                {
                    return null;
                }

                node = parent;
            }
            else if (parent is Decorator or ContentControl and not Window)
            {
                node = parent;
            }
            else
            {
                return null;
            }
        }

        return null;
    }

    private static Control? FindLabelInParent(Control parent, Control node)
    {
        if (parent is Grid grid)
        {
            return FindLabelInGrid(grid, node);
        }

        if (parent is Panel panel)
        {
            // The nearest label before the control. Other inputs and buttons in between are
            // skipped ("Filter: [combo] [text box]" names both after the label); a nested
            // panel or border starts a different group and ends the search.
            var index = panel.Children.IndexOf(node);
            for (var i = index - 1; i >= 0; i--)
            {
                var sibling = panel.Children[i];
                if (IsLabel(sibling))
                {
                    return sibling;
                }

                if (sibling is Panel or Decorator)
                {
                    return null;
                }
            }
        }

        return null;
    }

    private static Control? FindLabelInGrid(Grid grid, Control node)
    {
        var row = Grid.GetRow(node);
        var column = Grid.GetColumn(node);

        Control? best = null;
        var bestColumn = -1;
        foreach (var sibling in grid.Children)
        {
            if (sibling == node || !IsLabel(sibling))
            {
                continue;
            }

            var siblingRow = Grid.GetRow(sibling);
            var siblingColumn = Grid.GetColumn(sibling);
            if (siblingRow == row && siblingColumn < column && siblingColumn > bestColumn)
            {
                best = sibling;
                bestColumn = siblingColumn;
            }
        }

        if (best != null)
        {
            return best;
        }

        // Label above (same column, previous row).
        return grid.Children.FirstOrDefault(sibling =>
            sibling != node && IsLabel(sibling) &&
            Grid.GetColumn(sibling) == column && Grid.GetRow(sibling) == row - 1);
    }

    /// <summary>
    /// A label, a short text block, or a check box/radio button with text ("Fix minimum
    /// duration [x] 1000" - the check box text is what the number field means). Long or
    /// multi-line text blocks are descriptions or status text, not labels.
    /// </summary>
    private static bool IsLabel(Control control)
    {
        if (control is Label label)
        {
            return label.Content is not Control;
        }

        if (control is CheckBox or RadioButton)
        {
            return ((ContentControl)control).Content is string;
        }

        if (control is TextBlock textBlock)
        {
            var text = textBlock.Text;
            return text == null || (text.Length <= 80 && !text.Contains('\n'));
        }

        return false;
    }
}
