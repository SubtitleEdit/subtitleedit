using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Files;

/// <summary>
/// Small building blocks for the File menu's import/export/backup dialogs in the editor look
/// (see <see cref="EditorChrome"/>), next to the shared EditDialogChrome/LineToolsChrome ones.
/// </summary>
internal static class FileIoChrome
{
    /// <summary>
    /// A section whose content (a text area, a list, ...) fills the rest of it: the small caps
    /// header (with optional content on the right) above the content.
    /// </summary>
    internal static Border MakeFillSection(string title, Control content, Control? headerRight = null, bool first = false, bool lineLeft = false)
    {
        var header = new DockPanel { MinHeight = 26 };
        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            header.Children.Add(headerRight);
        }

        header.Children.Add(EditorChrome.MakeSectionHeader(title));

        var grid = new Grid
        {
            RowSpacing = 8,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(header, 0, 0);
        grid.Add(content, 1, 0);
        return new Border
        {
            Child = grid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = lineLeft ? new Thickness(1, 0, 0, 0) : new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    /// <summary>
    /// A multi-line text box on the field colors (class "text" from EditDialogChrome), text from the top.
    /// </summary>
    internal static T StyleTextArea<T>(T textBox) where T : TextBox
    {
        textBox.Classes.Add("text");
        textBox.VerticalContentAlignment = VerticalAlignment.Top;
        textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBox.VerticalAlignment = VerticalAlignment.Stretch;
        textBox.Padding = new Thickness(8, 6);
        return textBox;
    }

    /// <summary>
    /// A single-line text box on the field colors (class "text" from EditDialogChrome).
    /// </summary>
    internal static T StyleTextField<T>(T textBox) where T : TextBox
    {
        textBox.Classes.Add("text");
        textBox.VerticalAlignment = VerticalAlignment.Center;
        textBox.Margin = new Thickness(0);
        return textBox;
    }

    /// <summary>
    /// A dim text for the left side of the button bar or a section header.
    /// </summary>
    internal static TextBlock MakeDimText()
    {
        return new TextBlock
        {
            Foreground = EditorChrome.DimTextBrush,
            FontSize = 12.5,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
    }
}
