using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

/// <summary>
/// The few layout pieces only the speech-to-text windows use; the rest of their editor look
/// (window styles, rows, switches, button bar, ...) comes from <see cref="EditorChrome"/>.
/// </summary>
internal static class SpeechToTextChrome
{
    /// <summary>
    /// A small dim label above a field - for long labels that would not fit beside it.
    /// </summary>
    internal static StackPanel Stacked(TextBlock label, Control content)
    {
        label.Foreground = EditorChrome.DimTextBrush;
        label.FontSize = 12;
        label.TextWrapping = TextWrapping.Wrap;
        label.Margin = new Thickness(0);
        var panel = new StackPanel { Spacing = 4 };
        panel.Children.Add(label);
        panel.Children.Add(content);
        return panel;
    }

    /// <summary>
    /// A field with a fixed width on the left of a stretching cell, plus optional trailing controls.
    /// </summary>
    internal static Grid WithTrailing(Control main, params Control[] trailing)
    {
        var grid = new Grid { ColumnSpacing = 4 };
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        grid.Add(main, 0, 0);
        for (var i = 0; i < trailing.Length; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            grid.Add(trailing[i], 0, i + 1);
        }

        return grid;
    }

    /// <summary>
    /// UiUtil's API key box (text box + reveal button) in the compact look, stretching.
    /// </summary>
    internal static StackPanel CompactApiKey(StackPanel apiKeyPanel, double width)
    {
        foreach (var child in apiKeyPanel.Children)
        {
            if (child is TextBox textBox)
            {
                textBox.AsTextField();
                textBox.Width = width;
                textBox.Margin = new Thickness(0);
            }
            else if (child is Button button)
            {
                button.AsToolButton();
            }
        }

        apiKeyPanel.Margin = new Thickness(0);
        apiKeyPanel.Spacing = 4;
        return apiKeyPanel;
    }

    /// <summary>
    /// A "LIST NAME ........ [actions]" header line.
    /// </summary>
    internal static DockPanel ListHeader(string title, params Control[] actions)
    {
        var header = EditorChrome.MakeSectionHeaderRow(title, actions.Length > 0 ? EditorChrome.MakeListActions(actions) : null);
        header.Margin = new Thickness(0, 0, 0, 8);
        return header;
    }
}
