using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Video;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

/// <summary>
/// Small pieces the batch convert dialogs share on top of <see cref="VideoSubDialogChrome"/>.
/// </summary>
internal static class BatchConvertChrome
{
    /// <summary>
    /// A label of fixed width and the control after it, so the rows line up. The label wraps
    /// (translated labels like "Language post-fix (mkv/mp4/auto-translate)" are long), and the row
    /// can follow <paramref name="visibleProperty"/>.
    /// </summary>
    internal static Grid MakeRow(string label, Control content, double labelWidth, string? visibleProperty = null)
    {
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0));
        text.TextWrapping = TextWrapping.Wrap;
        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(labelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(text, 0, 0);
        grid.Add(content, 0, 1);
        if (visibleProperty != null)
        {
            grid.Bind(Visual.IsVisibleProperty, new Binding(visibleProperty));
        }

        return grid;
    }

    /// <summary>
    /// An on/off switch bound to <paramref name="property"/> with its label beside it.
    /// </summary>
    internal static Grid MakeSwitchRow(string property, string text, out ToggleSwitch toggle)
    {
        toggle = VideoSubDialogChrome.MakeSwitch(property, text);
        return VideoSubDialogChrome.MakeSwitchRow(toggle, text);
    }

    internal static Grid MakeSwitchRow(string property, string text) => MakeSwitchRow(property, text, out _);

    /// <summary>
    /// The sections column of a dialog on the panel color.
    /// </summary>
    internal static StackPanel MakeSections(params Control[] sections)
    {
        var panel = new StackPanel();
        foreach (var section in sections)
        {
            panel.Children.Add(section);
        }

        return panel;
    }
}
