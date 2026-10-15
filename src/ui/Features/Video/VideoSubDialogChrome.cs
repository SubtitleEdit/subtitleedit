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

namespace Nikse.SubtitleEdit.Features.Video;

/// <summary>
/// The pieces the smaller video dialogs (embedded subtitles, re-encode, background music, the
/// burn-in sub-dialogs, ...) share to get the editor look of <see cref="EditorChrome"/>: compact
/// combo boxes and text fields, an accent primary (also as a split button), the button bar at the
/// bottom and the progress strip above it.
/// </summary>
internal static class VideoSubDialogChrome
{
    /// <summary>
    /// Window background, the editor styles and the compact field styles.
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

        // A disabled primary (while generating) must not keep the accent - it looked clickable
        styles.Add(new Style(x => x.OfType<Button>().Class("primary").Class(":disabled").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, EditorChrome.HoverBrush),
                new Setter(ContentPresenter.ForegroundProperty, EditorChrome.FaintTextBrush),
            },
        });

        // A split button as the main action: both halves in the accent
        foreach (var isHover in new[] { false, true })
        {
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

        styles.Add(new Style(x => x.OfType<SplitButton>().Class("primary").Class(":disabled").Template().OfType<Button>().Template().OfType<ContentPresenter>())
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, EditorChrome.HoverBrush),
                new Setter(ContentPresenter.ForegroundProperty, EditorChrome.FaintTextBrush),
            },
        });
    }

    /// <summary>
    /// The bottom bar: optional status/summary on the left, the buttons on the right (pass them
    /// as Cancel, ..., primary).
    /// </summary>
    internal static Border MakeButtonBar(Control? left, params Control[] buttons)
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

    /// <summary>
    /// Progress bar with the status text beside it, on the status color, shown while
    /// <paramref name="visibleProperty"/> is true.
    /// </summary>
    internal static Border MakeProgressStrip(ProgressBar progressBar, string textProperty, string visibleProperty)
    {
        progressBar.VerticalAlignment = VerticalAlignment.Center;
        var statusText = MakeHint(textProperty);
        statusText.FontSize = 12;
        statusText.MaxWidth = 360;

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        grid.Add(progressBar, 0, 0);
        grid.Add(statusText, 0, 1);

        return new Border
        {
            Child = grid,
            Background = EditorChrome.StatusBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 8),
            [!Visual.IsVisibleProperty] = new Binding(visibleProperty),
        };
    }

    /// <summary>
    /// Dim one-line text bound to <paramref name="property"/>, cut with an ellipsis.
    /// </summary>
    internal static TextBlock MakeHint(string property) => new()
    {
        FontSize = 11.5,
        Foreground = EditorChrome.DimTextBrush,
        VerticalAlignment = VerticalAlignment.Center,
        TextTrimming = TextTrimming.CharacterEllipsis,
        [!TextBlock.TextProperty] = new Binding(property),
    };

    /// <summary>
    /// Dim explanatory text that wraps.
    /// </summary>
    internal static TextBlock MakeHintText(string text) => new()
    {
        Text = text,
        FontSize = 11.5,
        Foreground = EditorChrome.DimTextBrush,
        TextWrapping = TextWrapping.Wrap,
    };

    /// <summary>
    /// A label of fixed width and the control(s) after it, so the rows line up.
    /// </summary>
    internal static Grid MakeRow(string label, Control content, double labelWidth)
    {
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0));
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        var grid = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(labelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(text, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
    }

    /// <summary>
    /// A compact text field in the editor look.
    /// </summary>
    internal static TextBox MakeTextBox(string property, string accessibleName, double width = double.NaN)
    {
        var textBox = new TextBox
        {
            Width = width,
            HorizontalAlignment = double.IsNaN(width) ? HorizontalAlignment.Stretch : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(property) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };
        textBox.Classes.Add("text");
        AutomationProperties.SetName(textBox, accessibleName);
        return textBox;
    }

    /// <summary>
    /// A compact on/off switch; its label is placed beside it by <see cref="MakeSwitchRow"/>.
    /// </summary>
    internal static ToggleSwitch MakeSwitch(string property, string name)
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
    internal static Grid MakeSwitchRow(ToggleSwitch toggle, string text)
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

    /// <summary>
    /// A list (table view, list box) in a rounded field frame.
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
    /// A plain frame around a video player: no corner radius and no clipping, as the native video
    /// surface on macOS disappears inside either.
    /// </summary>
    internal static Border MakeVideoFrame(Control player)
    {
        return new Border
        {
            Child = player,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1),
        };
    }

    /// <summary>
    /// The panel color behind <paramref name="content"/>.
    /// </summary>
    internal static Border MakePanel(Control content)
    {
        return new Border
        {
            Child = content,
            Background = EditorChrome.PanelBrush,
        };
    }

    /// <summary>
    /// Icon button with its label beside it, for a toolbar.
    /// </summary>
    internal static Button MakeAction(string icon, string text, System.Windows.Input.ICommand? command)
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
    /// A toolbar strip on the panel color with a line under it.
    /// </summary>
    internal static Border MakeToolbar(Control content)
    {
        return new Border
        {
            Child = content,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 6),
        };
    }
}
