using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// The pieces the text-to-speech windows (generate, review, history, speak from line, the
/// select-lines dialogs, auto cast) share for the editor look (see <see cref="EditorChrome"/>):
/// compact combo/text boxes, label rows, switches, list frames and the bottom button bar.
/// </summary>
internal static class TtsMainEditorLook
{
    /// <summary>
    /// The editor styles plus compact combo boxes and text boxes, and a disabled primary button
    /// that drops the accent (a disabled Generate looked clickable).
    /// </summary>
    internal static void Apply(Window window)
    {
        window.Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(window.Styles);

        window.Styles.Add(new Style(x => x.OfType<ComboBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 2, 4, 2)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        window.Styles.Add(new Style(x => x.OfType<TextBox>().Class("text"))
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(TemplatedControl.BackgroundProperty, EditorChrome.FieldBrush),
                new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
            },
        });
        window.Styles.Add(new Style(x => x.OfType<Button>().Class("primary").Class(":disabled").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, EditorChrome.HoverBrush),
                new Setter(ContentPresenter.ForegroundProperty, EditorChrome.FaintTextBrush),
            },
        });
    }

    /// <summary>
    /// A label of fixed width and the control(s) after it, so the rows line up. The label can
    /// be bound instead (e.g. "Region" / "Style" depending on the engine).
    /// </summary>
    internal static Grid Row(string? label, Control content, double labelWidth, string? labelBinding = null)
    {
        var text = EditorChrome.MakeDimLabel(label ?? string.Empty, new Thickness(0));
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        if (labelBinding != null)
        {
            text.Bind(TextBlock.TextProperty, new Binding(labelBinding) { Mode = BindingMode.OneWay });
        }

        var grid = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(labelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(text, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
    }

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
        var hint = new TextBlock
        {
            Text = text,
            FontSize = 11.5,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
        };
        if (binding != null)
        {
            hint.Bind(TextBlock.TextProperty, new Binding(binding) { Mode = BindingMode.OneWay });
        }

        return hint;
    }

    internal static ToggleSwitch Switch(string property, string name)
    {
        var toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggle, name);
        return toggle;
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
        row.Add(Switch(property, text), 0, 0);
        row.Add(new TextBlock { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap }, 0, 1);
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
    /// Flat icon button with its label beside it, for tool bars.
    /// </summary>
    internal static Button MakeAction(string icon, string? text, System.Windows.Input.ICommand? command, string? textBinding = null)
    {
        var label = new TextBlock { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center };
        if (textBinding != null)
        {
            label.Bind(TextBlock.TextProperty, new Binding(textBinding) { Mode = BindingMode.OneWay });
        }

        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7, Margin = new Thickness(10, 0) };
        content.Children.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 13, VerticalAlignment = VerticalAlignment.Center });
        content.Children.Add(label);
        var button = new Button
        {
            Content = content,
            Height = 30,
            Command = command,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");

        // Icon + label in a panel: a screen reader needs the name spelled out (#12087)
        if (textBinding != null)
        {
            button.Bind(AutomationProperties.NameProperty, new Binding(textBinding) { Mode = BindingMode.OneWay });
        }
        else
        {
            AutomationProperties.SetName(button, text);
        }

        return button;
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

    /// <summary>
    /// The rounded field frame a list (TableView) sits in.
    /// </summary>
    internal static Border MakeListFrame(Control list)
    {
        return new Border
        {
            Child = list,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
        };
    }

    /// <summary>
    /// The bottom bar: <paramref name="left"/> (status, summary, options) on the left, the
    /// buttons on the right - Cancel before the accent primary.
    /// </summary>
    internal static Border MakeButtonBar(Control? left, params Control[] buttons)
    {
        var right = Horizontal(buttons);
        right.Spacing = 10;

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        if (left != null)
        {
            left.VerticalAlignment = VerticalAlignment.Center;
            grid.Add(left, 0, 0);
        }

        grid.Add(right, 0, 1);
        return new Border
        {
            Child = grid,
            Background = EditorChrome.ButtonBarBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
        };
    }

    internal static Button MakePrimary(Button button)
    {
        button.Classes.Add("primary");
        return button;
    }

    internal static Button MakeHelpButton(System.Windows.Input.ICommand command, string name)
    {
        return EditorChrome.CreateToolButton("fa-solid fa-circle-question", name, command, 26);
    }

    internal static string SettingsName(string what) => $"{what} - {Se.Language.General.Settings}";
}
