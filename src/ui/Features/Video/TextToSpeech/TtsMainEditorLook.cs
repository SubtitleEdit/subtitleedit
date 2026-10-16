using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// The pieces only the text-to-speech windows (generate, review, history, speak from line, the
/// select-lines dialogs, auto cast) use; the rest of their editor look (window styles, rows,
/// switches, list frames, button bar, ...) comes from <see cref="EditorChrome"/>.
/// </summary>
internal static class TtsMainEditorLook
{
    internal static StackPanel Horizontal(params Control[] controls)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
        foreach (var control in controls)
        {
            panel.Children.Add(control);
        }

        return panel;
    }

    /// <summary>
    /// A muted explanation line.
    /// </summary>
    internal static TextBlock Hint(string? text = null, string? binding = null)
    {
        var hint = EditorChrome.MakeHint(text);
        hint.VerticalAlignment = VerticalAlignment.Center;
        if (binding != null)
        {
            hint.Bind(TextBlock.TextProperty, new Binding(binding) { Mode = BindingMode.OneWay });
        }

        return hint;
    }

    /// <summary>
    /// A switch with its label beside it (the label wraps instead of being cut off), optional
    /// extra controls after the label (a settings button) and an optional hint under the label.
    /// </summary>
    internal static Grid SwitchRow(string property, string text, string? hint = null, Control? after = null)
    {
        var row = new Grid
        {
            ColumnSpacing = 8,
            RowSpacing = 1,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
        };
        row.Add(EditorChrome.MakeSwitch(property, text), 0, 0);
        row.Add(EditorChrome.MakeSwitchLabel(text), 0, 1);
        if (after != null)
        {
            after.VerticalAlignment = VerticalAlignment.Center;
            row.Add(after, 0, 2);
        }

        if (hint != null)
        {
            row.Add(Hint(hint), 1, 1, 1, 2);
        }

        return row;
    }

    /// <summary>
    /// Small grey pill, e.g. a voice count or the video file name.
    /// </summary>
    internal static Border MakeChip(Control content)
    {
        return new Border
        {
            Background = EditorChrome.HoverBrush,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(9, 2, 9, 3),
            VerticalAlignment = VerticalAlignment.Center,
            Child = content,
        };
    }

    internal static Button MakeHelpButton(System.Windows.Input.ICommand command, string name)
    {
        return EditorChrome.CreateToolButton("fa-solid fa-circle-question", name, command, 26);
    }

    internal static string SettingsName(string what) => $"{what} - {Se.Language.General.Settings}";
}
