using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

/// <summary>
/// Small building blocks for the video OCR, open from URL, shot changes and go to position
/// windows in the editor look (see <see cref="EditorChrome"/>): compact inputs, label rows,
/// switches, list frames, progress strip and the bottom button bar.
/// </summary>
internal static class VideoDialogChrome
{
    /// <summary>
    /// Window background plus the editor styles and the compact combo/text/time code inputs.
    /// </summary>
    internal static void Apply(Window window)
    {
        window.Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(window.Styles);
        var styles = window.Styles;

        styles.Add(new Style(x => x.OfType<ComboBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 2, 4, 2)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        styles.Add(new Style(x => x.OfType<TextBox>().Class("text"))
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 4)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(TemplatedControl.BackgroundProperty, EditorChrome.FieldBrush),
                new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                new Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center),
            },
        });
        styles.Add(new Style(x => x.OfType<TimeCodeUpDown>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
            },
        });
        styles.Add(new Style(x => x.OfType<TimeCodeUpDown>().Descendant().OfType<TextBox>())
        {
            Setters = { new Setter(TemplatedControl.MinHeightProperty, 26.0) },
        });
        styles.Add(new Style(x => x.OfType<TimeCodeUpDown>().Descendant().OfType<ButtonSpinner>())
        {
            Setters = { new Setter(TemplatedControl.MinHeightProperty, 26.0) },
        });

        // Tabs: small headers instead of the big Fluent ones
        styles.Add(new Style(x => x.OfType<TabItem>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 13.0),
                new Setter(Layoutable.MinHeightProperty, 34.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(12, 0)),
            },
        });

        // A disabled primary button (while working) must not keep the accent - it looked clickable
        styles.Add(new Style(x => x.OfType<Button>().Class("primary").Class(":disabled").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, EditorChrome.HoverBrush),
                new Setter(ContentPresenter.ForegroundProperty, EditorChrome.FaintTextBrush),
            },
        });
    }

    /// <summary>
    /// A dim label of fixed width and the control after it, so the rows line up.
    /// </summary>
    internal static Grid Row(string label, Control content, double labelWidth, out TextBlock labelControl)
    {
        labelControl = EditorChrome.MakeDimLabel(label, new Thickness(0));
        labelControl.TextTrimming = TextTrimming.CharacterEllipsis;
        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(labelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(labelControl, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
    }

    internal static Grid Row(string label, Control content, double labelWidth) => Row(label, content, labelWidth, out _);

    /// <summary>
    /// A dim label above its control (for wide inputs like URLs).
    /// </summary>
    internal static StackPanel LabelAbove(string label, Control content)
    {
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0));
        AutomationProperties.SetLabeledBy(content, text);
        var panel = new StackPanel { Spacing = 4 };
        panel.Children.Add(text);
        panel.Children.Add(content);
        return panel;
    }

    internal static ToggleSwitch Switch(string property, string name)
    {
        var toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggle, name);
        return toggle;
    }

    /// <summary>
    /// A switch with its label beside it; the label wraps instead of being cut off.
    /// </summary>
    internal static Grid SwitchRow(ToggleSwitch toggle, string text)
    {
        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        row.Add(toggle, 0, 0);
        row.Add(new TextBlock { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap }, 0, 1);
        return row;
    }

    internal static Grid SwitchRow(string property, string text) => SwitchRow(Switch(property, text), text);

    /// <summary>
    /// Dim one-line text bound to a view model property.
    /// </summary>
    internal static TextBlock Hint(string property) => new()
    {
        FontSize = 12,
        Foreground = EditorChrome.DimTextBrush,
        VerticalAlignment = VerticalAlignment.Center,
        TextTrimming = TextTrimming.CharacterEllipsis,
        [!TextBlock.TextProperty] = new Binding(property),
    };

    /// <summary>
    /// Flat button with an icon and its label beside it, for toolbars and list actions.
    /// </summary>
    internal static Button Action(string icon, string text, System.Windows.Input.ICommand? command)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7, Margin = new Thickness(10, 0) };
        content.Children.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 13, VerticalAlignment = VerticalAlignment.Center });
        content.Children.Add(new TextBlock { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button
        {
            Content = content,
            Height = 30,
            Command = command,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");

        // Icon + label in a panel: a screen reader needs the name spelled out (#12087)
        AutomationProperties.SetName(button, text);
        return button;
    }

    /// <summary>
    /// The rounded field-colored frame around a list or text area.
    /// </summary>
    internal static Border ListFrame(Control content)
    {
        return new Border
        {
            Child = content,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
        };
    }

    /// <summary>
    /// A list section: small caps header (optional actions on the right) above a framed list that
    /// takes the rest of the height.
    /// </summary>
    internal static Grid ListSection(string title, Control list, Control? headerRight = null, Thickness? margin = null)
    {
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8), MinHeight = 26 };
        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            header.Children.Add(headerRight);
        }

        header.Children.Add(EditorChrome.MakeSectionHeader(title));
        var grid = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
            Margin = margin ?? new Thickness(14, 12, 14, 14),
        };
        grid.Add(header, 0, 0);
        grid.Add(list is Border ? list : ListFrame(list), 1, 0);
        return grid;
    }

    /// <summary>
    /// The strip above the button bar that holds a progress bar while working.
    /// </summary>
    internal static Border ProgressStrip(Control content)
    {
        return new Border
        {
            Child = content,
            Background = EditorChrome.StatusBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 8),
        };
    }

    /// <summary>
    /// The bottom bar: optional status/summary on the left, the buttons on the right.
    /// </summary>
    internal static Border ButtonBar(Control? left, params Control[] buttons)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
        foreach (var button in buttons)
        {
            panel.Children.Add(button);
        }

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

        grid.Add(panel, 0, 1);
        return new Border
        {
            Child = grid,
            Background = EditorChrome.ButtonBarBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
        };
    }

    internal static T Primary<T>(this T button) where T : Button
    {
        button.Classes.Add("primary");
        return button;
    }
}
