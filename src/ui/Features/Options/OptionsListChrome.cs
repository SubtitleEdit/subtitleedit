using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Edit;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options;

/// <summary>
/// Small building blocks for the Options sub-dialogs (word lists, plugins, the settings item
/// lists, ...) in the editor look (see <see cref="EditorChrome"/>), on top of
/// <see cref="EditDialogChrome"/>.
/// </summary>
internal static class OptionsListChrome
{
    /// <summary>
    /// Window chrome of <see cref="EditDialogChrome.Apply"/> plus compact list rows.
    /// </summary>
    internal static void Apply(Window window)
    {
        EditDialogChrome.Apply(window);
        window.Styles.Add(new Style(x => x.OfType<ListBox>().Class("compact").Descendant().OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(ListBoxItem.PaddingProperty, new Thickness(8, 3)),
                new Setter(ListBoxItem.MarginProperty, new Thickness(2, 1)),
                new Setter(ListBoxItem.CornerRadiusProperty, new CornerRadius(5)),
                new Setter(ListBoxItem.FontSizeProperty, 12.5),
            },
        });
    }

    /// <summary>
    /// A list box with compact rounded rows in a rounded frame on the field color.
    /// </summary>
    internal static Border MakeListFrame(ListBox listBox, bool compact = true)
    {
        if (compact)
        {
            listBox.Classes.Add("compact");
        }

        return EditDialogChrome.MakeListFrame(listBox);
    }

    /// <summary>
    /// A small rounded count badge, e.g. "1,234", bound to the given path.
    /// </summary>
    internal static Border MakeCountBadge(Binding textBinding, IBrush foreground, IBrush background)
    {
        return new Border
        {
            Background = background,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(8, 2),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                [!TextBlock.TextProperty] = textBinding,
                FontSize = 11,
                FontWeight = FontWeight.SemiBold,
                Foreground = foreground,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
    }

    /// <summary>
    /// A section header with optional controls on the right.
    /// </summary>
    internal static DockPanel MakeHeader(Control title, Control? right = null)
    {
        var header = new DockPanel { LastChildFill = true };
        if (right != null)
        {
            DockPanel.SetDock(right, Dock.Right);
            header.Children.Add(right);
        }

        header.Children.Add(title);
        return header;
    }

    /// <summary>
    /// A compact on/off switch with its label to the right; the switch is returned too, for
    /// event hookups.
    /// </summary>
    internal static StackPanel MakeSwitchRow(string property, string text, out ToggleSwitch toggle)
    {
        var row = EditorChrome.MakeToggleRow(property, text);
        toggle = (ToggleSwitch)row.Children[0];
        return row;
    }

    /// <summary>
    /// Turns a NumericUpDown into a compact editor field (no spinner, field colors).
    /// </summary>
    internal static NumericUpDown AsField(this NumericUpDown numericUpDown, string accessibleName)
    {
        numericUpDown.Classes.Add("field");
        numericUpDown.ClearValue(TemplatedControl.ForegroundProperty); // the theme color, not the fixed text color
        AutomationProperties.SetName(numericUpDown, accessibleName);
        return numericUpDown;
    }

    /// <summary>
    /// A text box in the editor field look (class "text" from <see cref="EditDialogChrome.Apply"/>).
    /// </summary>
    internal static TextBox AsField(this TextBox textBox, string accessibleName)
    {
        textBox.Classes.Add("text");
        AutomationProperties.SetName(textBox, accessibleName);
        return textBox;
    }

    /// <summary>
    /// A hint text in the dim color, wrapping.
    /// </summary>
    internal static TextBlock MakeHint(string text)
    {
        var hint = EditDialogChrome.MakeLabel(text);
        hint.FontSize = 12;
        return hint;
    }
}
