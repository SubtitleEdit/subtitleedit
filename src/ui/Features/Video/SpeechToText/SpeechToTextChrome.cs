using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

/// <summary>
/// The editor look (see <see cref="EditorChrome"/>) for the speech-to-text windows: compact
/// combo boxes and text fields, label/field rows, switch rows and the bottom button bar.
/// </summary>
internal static class SpeechToTextChrome
{
    internal const string TextClass = "text";

    /// <summary>
    /// Editor styles plus compact combo boxes, text fields (class "text"), toggle tool buttons
    /// and the accent split button.
    /// </summary>
    internal static void AddStyles(Styles styles)
    {
        EditorChrome.AddEditorStyles(styles);

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
        styles.Add(new Style(x => x.OfType<TextBox>().Class(TextClass))
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

        // Keep the field color when hovered/focused (the theme turns it black/white otherwise)
        foreach (var state in new[] { ":pointerover", ":focus" })
        {
            var focus = state == ":focus";
            styles.Add(new Style(x => x.OfType<TextBox>().Class(TextClass).Class(state).Template().OfType<Border>().Name("PART_BorderElement"))
            {
                Setters =
                {
                    new Setter(Border.BackgroundProperty, EditorChrome.FieldBrush),
                    new Setter(Border.BorderBrushProperty, focus ? EditorChrome.AccentBrush : EditorChrome.SeparatorBrush),
                },
            });
        }

        // Toggle tool buttons (the advanced dialog's "Enable VAD", "Highlight word", ...)
        styles.Add(new Style(x => x.OfType<ToggleButton>().Class("tool"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1)),
                new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        styles.Add(new Style(x => x.OfType<ToggleButton>().Class("tool").Class(":checked").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, EditorChrome.AccentBrush),
                new Setter(ContentPresenter.BorderBrushProperty, EditorChrome.AccentBrush),
                new Setter(ContentPresenter.ForegroundProperty, Brushes.White),
            },
        });
        styles.Add(new Style(x => x.OfType<ToggleButton>().Class("tool").Class(":checked").Class(":pointerover").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, EditorChrome.AccentHoverBrush),
                new Setter(ContentPresenter.BorderBrushProperty, EditorChrome.AccentHoverBrush),
                new Setter(ContentPresenter.ForegroundProperty, Brushes.White),
            },
        });

        // Accent split button: both halves
        foreach (var hover in new[] { false, true })
        {
            var isHover = hover;
            styles.Add(new Style(x =>
            {
                var selector = x.OfType<SplitButton>().Class("primary").Template().OfType<Button>();
                return isHover
                    ? selector.Class(":pointerover").Template().OfType<ContentPresenter>()
                    : selector.Template().OfType<ContentPresenter>();
            })
            {
                Setters =
                {
                    new Setter(ContentPresenter.BackgroundProperty, isHover ? EditorChrome.AccentHoverBrush : EditorChrome.AccentBrush),
                    new Setter(ContentPresenter.ForegroundProperty, Brushes.White),
                },
            });
        }
    }

    /// <summary>
    /// Window background and styles in the editor look.
    /// </summary>
    internal static void Apply(Window window)
    {
        window.Background = EditorChrome.WindowBrush;
        AddStyles(window.Styles);
    }

    /// <summary>
    /// A dim label of fixed width and the control after it, so the rows line up.
    /// </summary>
    internal static Grid Row(TextBlock label, Control content, double labelWidth)
    {
        label.Foreground = EditorChrome.DimTextBrush;
        label.FontSize = 12;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.TextTrimming = TextTrimming.CharacterEllipsis;
        label.Margin = new Thickness(0);
        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(labelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(label, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
    }

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

    internal static ToggleSwitch Switch(object viewModel, string property, string name)
    {
        var toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggle, name);
        return toggle;
    }

    /// <summary>
    /// A switch with its label beside it (the label wraps instead of being cut off), and
    /// optional controls on the right.
    /// </summary>
    internal static Grid SwitchRow(ToggleSwitch toggle, TextBlock label, params Control[] trailing)
    {
        label.FontSize = 12.5;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.TextWrapping = TextWrapping.Wrap;
        label.Margin = new Thickness(0);
        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        row.Add(toggle, 0, 0);
        row.Add(label, 0, 1);
        for (var i = 0; i < trailing.Length; i++)
        {
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            row.Add(trailing[i], 0, i + 2);
        }

        return row;
    }

    /// <summary>
    /// A flat tool button made from a button that already carries an icon (UiUtil's icon
    /// buttons, whose icon a view model may swap).
    /// </summary>
    internal static T Tool<T>(T button, double size = 28) where T : Button
    {
        button.Classes.Add("tool");
        button.Width = size;
        button.Height = size;
        button.MinWidth = 0;
        button.MinHeight = 0;
        button.Padding = new Thickness(0);
        button.FontSize = 14;
        return button;
    }

    internal static T EnabledBy<T>(T control, object viewModel, string property) where T : Control
    {
        control.Bind(InputElement.IsEnabledProperty, new Binding(property) { Source = viewModel });
        return control;
    }

    internal static T AsText<T>(T textBox) where T : TextBox
    {
        textBox.Classes.Add(TextClass);
        return textBox;
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
                AsText(textBox);
                textBox.Width = width;
                textBox.Margin = new Thickness(0);
            }
            else if (child is Button button)
            {
                Tool(button);
            }
        }

        apiKeyPanel.Margin = new Thickness(0);
        apiKeyPanel.Spacing = 4;
        return apiKeyPanel;
    }

    internal static Button Primary(Button button)
    {
        button.Classes.Add("primary");
        return button;
    }

    /// <summary>
    /// The bottom bar: status/summary on the left, buttons on the right.
    /// </summary>
    internal static Border ButtonBar(Control? left, params Control[] buttons)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
        foreach (var button in buttons)
        {
            button.Margin = new Thickness(0);
            panel.Children.Add(button);
        }

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        if (left != null)
        {
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

    /// <summary>
    /// A small dim text bound to a view model property, for status lines.
    /// </summary>
    internal static TextBlock Hint(string property, double fontSize = 12)
    {
        return new TextBlock
        {
            FontSize = fontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(property),
        };
    }

    /// <summary>
    /// A list or log in a rounded field frame.
    /// </summary>
    internal static Border FieldFrame(Control child)
    {
        return new Border
        {
            Child = child,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
        };
    }

    /// <summary>
    /// A "LIST NAME ........ [actions]" header line.
    /// </summary>
    internal static DockPanel ListHeader(string title, params Control[] actions)
    {
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8), MinHeight = 26 };
        if (actions.Length > 0)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
            foreach (var action in actions)
            {
                panel.Children.Add(action);
            }

            DockPanel.SetDock(panel, Dock.Right);
            header.Children.Add(panel);
        }

        header.Children.Add(EditorChrome.MakeSectionHeader(title));
        return header;
    }
}
