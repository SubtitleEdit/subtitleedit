using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
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
            if (!(IsInput(control) || IsContentButton(control)) || HasAccessibleName(control))
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
                LinkToLabel(control, label);
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

    /// <summary>
    /// The input controls a screen reader user tabs to that need a name. A check box or
    /// radio button without text is one too: "[x] Label" next to a separate label announces
    /// only "check box, checked" (#12087). One with text - a string or a wrapping TextBlock -
    /// names itself.
    /// </summary>
    public static bool IsInput(Control control)
    {
        return control is TextBox or ComboBox or NumericUpDown or Slider or AutoCompleteBox
            or SecondsUpDown or TimeCodeUpDown or ListBox or DatePicker or TimePicker
            or CheckBox { Content: not (string or TextBlock) } or RadioButton { Content: not (string or TextBlock) };
    }

    /// <summary>
    /// A button whose content is a control rather than text: a colour swatch (Border), an
    /// icon plus caption (StackPanel), an image. UI Automation names such a button after the
    /// content's type - "Avalonia.Controls.Border" - and that computed name takes precedence
    /// over LabeledBy, so these need an explicit name (#12087). Check boxes and radio buttons
    /// are buttons too in Avalonia; they are handled as inputs instead.
    /// </summary>
    public static bool IsContentButton(Control control)
    {
        return control is Button { Content: Control and not TextBlock } and not CheckBox and not RadioButton;
    }

    /// <summary>
    /// Everything the accessibility test expects to find named: the inputs, the content
    /// buttons, and buttons with no content at all (an icon set as an attached property),
    /// which a screen reader announces as a bare "button".
    /// </summary>
    public static bool NeedsName(Control control)
    {
        return IsInput(control) || IsContentButton(control)
               || control is Button { Content: null } and not CheckBox and not RadioButton;
    }

    /// <summary>
    /// True when the window gave the control a name, a LabeledBy link, or a name binding
    /// (a bound caption such as "Download"/"Re-download" that is still empty before the view
    /// model initializes is a name nonetheless).
    /// </summary>
    public static bool HasAccessibleName(Control control)
    {
        return !string.IsNullOrEmpty(AutomationProperties.GetName(control))
               || AutomationProperties.GetLabeledBy(control) != null
               || control.IsSet(AutomationProperties.NameProperty);
    }

    /// <summary>
    /// Names <paramref name="control"/> after <paramref name="label"/>: a LabeledBy link for
    /// inputs, an explicit name (kept in sync with the label text) for content buttons, whose
    /// computed type name would otherwise win over the link.
    /// </summary>
    public static void LinkToLabel(Control control, Control label)
    {
        if (!IsContentButton(control))
        {
            AutomationProperties.SetLabeledBy(control, label);
            return;
        }

        switch (label)
        {
            case TextBlock textBlock:
                control.Bind(AutomationProperties.NameProperty, new Binding(nameof(TextBlock.Text)) { Source = textBlock });
                break;
            case ContentControl { Content: string text }:
                AutomationProperties.SetName(control, text);
                break;
            default:
                AutomationProperties.SetLabeledBy(control, label);
                break;
        }
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
                // its predecessor. A text-less check box does not count: "Label [x] [1000]"
                // is one setting, and both the box and the number field mean the label.
                var index = panel.Children.IndexOf(node);
                if (panel.Children.Take(index).Any(c => (IsInput(c) && c is not CheckBox and not RadioButton) || c is Panel))
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
        // A text-less check box is usually labeled by the text right after it ("[x] Label"),
        // and must take that over a label further left or the previous row's.
        if (node is CheckBox or RadioButton)
        {
            var right = FindLabelToTheRight(parent, node);
            if (right != null)
            {
                return right;
            }
        }

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

    private static Control? FindLabelToTheRight(Control parent, Control node)
    {
        if (parent is Grid grid)
        {
            var row = Grid.GetRow(node);
            var column = Grid.GetColumn(node);
            return grid.Children.FirstOrDefault(sibling =>
                sibling != node && IsLabel(sibling) &&
                Grid.GetRow(sibling) == row && Grid.GetColumn(sibling) == column + 1);
        }

        if (parent is Panel panel)
        {
            var index = panel.Children.IndexOf(node);
            if (index >= 0 && index + 1 < panel.Children.Count && IsLabel(panel.Children[index + 1]))
            {
                return panel.Children[index + 1];
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
