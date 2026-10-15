using System.Collections.Generic;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager;

/// <summary>
/// The editor look (see <see cref="EditorChrome"/>) for the TTS voice windows: voice manager,
/// voice packs, cast, the licence and consent gates and the engine download. Compact inputs,
/// a bottom button bar with the accent button on the right and a progress strip above it.
/// </summary>
internal static class TtsVoicesChrome
{
    /// <summary>
    /// Window background plus the shared editor styles and the compact inputs used here.
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
            },
        });

        // A disabled accent button (Accept before the box is ticked, Download while busy) must not
        // keep the accent - it looked clickable.
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
    /// The bottom bar: optional status on the left, the buttons on the right (accent one last).
    /// </summary>
    internal static Border MakeButtonBar(Control? left, params Button[] buttons)
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
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
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
    /// The strip above the button bar that holds the progress while working.
    /// </summary>
    internal static Border MakeProgressStrip(Control content)
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
    /// A list or grid in a rounded field frame.
    /// </summary>
    internal static Border MakeListFrame(Control content)
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
    /// Flat icon button with its label beside it.
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

    internal static TextBlock MakeHint(string? text = null)
    {
        return new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = 11.5,
        };
    }

    /// <summary>
    /// The title block at the top of a dialog: big title, dim intro under it.
    /// </summary>
    internal static StackPanel MakeTitle(string title, string? intro)
    {
        var panel = new StackPanel { Spacing = 4 };
        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 17,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
        });
        if (!string.IsNullOrEmpty(intro))
        {
            panel.Children.Add(new TextBlock
            {
                Text = intro,
                FontSize = 12.5,
                Foreground = EditorChrome.DimTextBrush,
                TextWrapping = TextWrapping.Wrap,
            });
        }

        return panel;
    }

    /// <summary>
    /// The bullet points of a licence or consent dialog, in a framed card. Every point is its own
    /// wrapping text block, so none can be cut off.
    /// </summary>
    internal static Border MakeBulletCard(IEnumerable<string> points)
    {
        var stack = new StackPanel { Spacing = 8 };
        foreach (var point in points)
        {
            var row = new Grid
            {
                ColumnSpacing = 8,
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
            };
            row.Add(new TextBlock { Text = "•", Foreground = EditorChrome.AccentBrush, FontWeight = FontWeight.Bold, VerticalAlignment = VerticalAlignment.Top }, 0, 0);
            row.Add(new TextBlock { Text = point, TextWrapping = TextWrapping.Wrap, FontSize = 12.5 }, 0, 1);
            stack.Children.Add(row);
        }

        return new Border
        {
            Child = stack,
            Padding = new Thickness(14, 12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.FieldBorderBrush,
            Background = EditorChrome.FieldBrush,
        };
    }
}
