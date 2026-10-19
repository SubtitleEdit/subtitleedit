using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties;

/// <summary>
/// One form layout for the format properties windows (D-Cinema, Timed Text, WebVTT, ...) and the
/// export settings windows (EBU STL, Cavena 890, DVB Teletext, PAC) in the editor look (see
/// <see cref="EditorChrome"/>): sections on the panel color, dim labels in one label column shared
/// by all sections, compact fields and color buttons.
/// </summary>
internal static class FormatPropertiesChrome
{
    private const string LabelGroup = "FormLabel";

    /// <summary>
    /// The sections on the panel color (scrolling on short screens) above the button bar.
    /// </summary>
    internal static Grid MakeRoot(Border buttonBar, params Control[] sections)
    {
        var stack = new StackPanel();
        foreach (var section in sections)
        {
            stack.Children.Add(section);
        }

        return MakeContentRoot(stack, buttonBar);
    }

    internal static Grid MakeContentRoot(Control content, Border buttonBar)
    {
        // One label column for all sections, so every field starts at the same x
        Grid.SetIsSharedSizeScope(content, true);
        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        root.Add(new ScrollViewer
        {
            Content = content,
            Background = EditorChrome.PanelBrush,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        }, 0, 0);
        root.Add(buttonBar, 1, 0);
        return root;
    }

    /// <summary>
    /// Flat icon + text actions side by side (for the left of the button bar).
    /// </summary>
    internal static StackPanel MakeActions(params Control[] actions)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
        foreach (var action in actions)
        {
            panel.Children.Add(action);
        }

        return panel;
    }

    internal static Button MakeAction(string icon, string text, System.Windows.Input.ICommand? command)
    {
        return EditorChrome.MakeTextToolButton(icon, text.Replace("_", string.Empty), command);
    }

    /// <summary>
    /// The rows of a section, stacked.
    /// </summary>
    internal static StackPanel MakeRows(params Control[] rows)
    {
        var panel = new StackPanel { Spacing = 8 };
        foreach (var row in rows)
        {
            panel.Children.Add(row);
        }

        return panel;
    }

    /// <summary>
    /// "Label  [control] [control]" - the labels share one column (see <see cref="MakeContentRoot"/>).
    /// </summary>
    internal static Grid Row(string label, params Control[] content)
    {
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0));
        text.MaxWidth = 260;
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        return Row(text, content);
    }

    internal static Grid Row(Control label, params Control[] content)
    {
        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto) { SharedSizeGroup = LabelGroup, MinWidth = 110 },
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        grid.Add(label, 0, 0);

        if (content.Length == 1)
        {
            grid.Add(content[0], 0, 1);
            return grid;
        }

        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        foreach (var control in content)
        {
            panel.Children.Add(control);
        }

        grid.Add(panel, 0, 1);
        return grid;
    }

    /// <summary>
    /// A dim label inside a row, before a second field ("Language", "Time code rate", ...).
    /// </summary>
    internal static TextBlock InlineLabel(string text)
    {
        return EditorChrome.MakeDimLabel(text, new Thickness(10, 0, 0, 0));
    }

    /// <summary>
    /// A dim unit after a field ("frames").
    /// </summary>
    internal static TextBlock Unit(string text)
    {
        return EditorChrome.MakeDimLabel(text, new Thickness(0));
    }

    /// <summary>
    /// Dim explanatory text that wraps.
    /// </summary>
    internal static TextBlock Hint(string text, double maxWidth = 420)
    {
        var hint = EditorChrome.MakeHint(text);
        hint.MaxWidth = maxWidth;
        hint.HorizontalAlignment = HorizontalAlignment.Left;
        hint.VerticalAlignment = VerticalAlignment.Center;
        return hint;
    }

    /// <summary>
    /// A combo box in the editor look (the style comes from <see cref="EditorChrome.ApplyWindow"/>), named for screen readers.
    /// </summary>
    internal static ComboBox Combo(ComboBox comboBox, string name, double width = 260)
    {
        comboBox.MinWidth = width;
        comboBox.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(comboBox, name);
        return comboBox;
    }

    /// <summary>
    /// A compact number field (see <see cref="EditorChrome.MakeField"/>), named for screen readers.
    /// </summary>
    internal static NumericUpDown Number(NumericUpDown field, string name, double width = 90)
    {
        return field.AsField(name, width);
    }

    /// <summary>
    /// A "choose color" button showing the current color in a swatch.
    /// </summary>
    internal static Button ColorButton(string text, string name, System.Windows.Input.ICommand? command, string colorProperty)
    {
        var swatch = new Border
        {
            Width = 22,
            Height = 14,
            CornerRadius = new CornerRadius(3),
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.FieldBorderBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!Border.BackgroundProperty] = new Binding(colorProperty) { Mode = BindingMode.OneWay, Converter = ColorToBrushConverter.Instance },
        };
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        content.Children.Add(swatch);
        content.Children.Add(new TextBlock { Text = text, FontSize = EditorChrome.FieldFontSize, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button
        {
            Content = content,
            Command = command,
            MinHeight = EditorChrome.FieldHeight,
            Padding = new Thickness(8, 0, 10, 0),
            CornerRadius = new CornerRadius(6),
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            VerticalContentAlignment = VerticalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        AutomationProperties.SetName(button, name);
        return button;
    }

    /// <summary>
    /// Two sections side by side with a line between them.
    /// </summary>
    internal static Grid SideBySide(Control left, Control right)
    {
        Grid.SetIsSharedSizeScope(left, true);
        Grid.SetIsSharedSizeScope(right, true);
        return EditorChrome.MakeSideBySide(left, right);
    }

    private sealed class ColorToBrushConverter : IValueConverter
    {
        public static readonly ColorToBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is Color color ? new SolidColorBrush(color) : new SolidColorBrush(Colors.White);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is SolidColorBrush brush ? brush.Color : Colors.White;
        }
    }
}
