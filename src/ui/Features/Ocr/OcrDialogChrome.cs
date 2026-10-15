using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Ocr;

/// <summary>
/// Small building blocks for the OCR engine settings and download windows in the editor look
/// (see <see cref="EditorChrome"/>): compact inputs, label rows, switches, list frames, the
/// download body and the bottom button bar.
/// </summary>
internal static class OcrDialogChrome
{
    internal const string TextClass = "text";

    private static readonly IBrush ErrorBrush = new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B));

    /// <summary>
    /// Window background plus the editor styles and the compact combo/text inputs.
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

        // A disabled primary button must not keep the accent - it looked clickable
        styles.Add(new Style(x => x.OfType<Button>().Class("primary").Class(":disabled").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, EditorChrome.HoverBrush),
                new Setter(ContentPresenter.ForegroundProperty, EditorChrome.FaintTextBrush),
            },
        });
    }

    internal static Button MakePrimary(Button button)
    {
        button.Classes.Add("primary");
        return button;
    }

    /// <summary>
    /// The title block at the top of a dialog: title, dim description under it.
    /// </summary>
    internal static Border MakeHeader(string title, string? description, double maxWidth = double.PositiveInfinity)
    {
        var panel = new StackPanel { Spacing = 2 };
        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
        });
        if (!string.IsNullOrEmpty(description))
        {
            panel.Children.Add(new TextBlock
            {
                Text = description,
                FontSize = 12.5,
                Foreground = EditorChrome.DimTextBrush,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = maxWidth,
                HorizontalAlignment = HorizontalAlignment.Left,
            });
        }

        return new Border { Child = panel, Padding = new Thickness(14, 14, 14, 12) };
    }

    /// <summary>
    /// A dim label of fixed width and the control after it, so the rows line up.
    /// </summary>
    internal static Grid Row(string label, Control content, double labelWidth, bool alignTop = false)
    {
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0, alignTop ? 6 : 0, 0, 0));
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        if (alignTop)
        {
            text.VerticalAlignment = VerticalAlignment.Top;
        }

        var grid = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(labelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(text, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
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

    /// <summary>
    /// A colored status dot with its (bound) text, e.g. "Installed" in green.
    /// </summary>
    internal static StackPanel StatusPanel(string brushBindingPath, string labelBindingPath)
    {
        var dot = new Ellipse
        {
            Width = 9,
            Height = 9,
            VerticalAlignment = VerticalAlignment.Center,
            [!Shape.FillProperty] = new Binding(brushBindingPath),
        };
        var text = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12.5,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(8, 0, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(labelBindingPath),
        };
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { dot, text },
        };
    }

    /// <summary>
    /// Turns a text button (icon + text) into a flat editor action.
    /// </summary>
    internal static T Flat<T>(T button) where T : Button
    {
        button.Classes.Add("tool");
        button.Height = 30;
        button.Padding = new Thickness(10, 0);
        button.FontSize = 12.5;
        button.Margin = new Thickness(0);
        button.VerticalAlignment = VerticalAlignment.Center;
        return button;
    }

    internal static TextBlock Hint(string text) => new()
    {
        Text = text,
        FontSize = 12,
        Foreground = EditorChrome.DimTextBrush,
        TextWrapping = TextWrapping.Wrap,
    };

    /// <summary>
    /// The rounded field-colored frame around a list.
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
    /// The bottom bar: optional status/actions on the left, the buttons on the right (accent last).
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
    /// Content on the panel color with the button bar below; the content row takes the rest of
    /// the height (for resizable windows), or sizes to content.
    /// </summary>
    internal static Grid Root(Control content, Border buttonBar, bool stretch = false)
    {
        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(stretch ? new GridLength(1, GridUnitType.Star) : GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
        };
        root.Add(new Border { Child = content, Background = EditorChrome.PanelBrush }, 0, 0);
        root.Add(buttonBar, 1, 0);
        return root;
    }

    /// <summary>
    /// The title of a download dialog (fixed text, or bind it afterwards).
    /// </summary>
    internal static TextBlock DownloadTitle(string? text = null) => new()
    {
        Text = text,
        FontSize = 16,
        FontWeight = FontWeight.SemiBold,
        TextWrapping = TextWrapping.Wrap,
        MaxWidth = 460,
    };

    /// <summary>
    /// The progress bar of a download dialog, full width of the body.
    /// </summary>
    internal static ProgressBar DownloadProgressBar(string valueProperty)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.MinWidth = 440;
        progressBar.Width = double.NaN;
        progressBar.Margin = new Thickness(0, 2, 0, 0);
        progressBar.HorizontalAlignment = HorizontalAlignment.Stretch;
        progressBar.Bind(RangeBase.ValueProperty, new Binding(valueProperty));
        return progressBar;
    }

    /// <summary>
    /// Dim status line under the progress bar of a download dialog.
    /// </summary>
    internal static TextBlock DownloadStatus(string textProperty) => new()
    {
        FontSize = 12,
        Foreground = EditorChrome.DimTextBrush,
        TextTrimming = TextTrimming.CharacterEllipsis,
        MaxWidth = 460,
        HorizontalAlignment = HorizontalAlignment.Left,
        [!TextBlock.TextProperty] = new Binding(textProperty),
    };

    /// <summary>
    /// The view models set Error on every failure path; shown in red, only when there is one.
    /// </summary>
    internal static TextBlock DownloadError(string errorProperty)
    {
        var errorText = new TextBlock
        {
            Foreground = ErrorBrush,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            MaxWidth = 460,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        errorText.Bind(TextBlock.TextProperty, new Binding(errorProperty));
        errorText.Bind(Visual.IsVisibleProperty, new Binding(errorProperty)
        {
            Converter = new FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s)),
        });
        return errorText;
    }

    /// <summary>
    /// The body of a download dialog: the given rows (title, progress, status, error) on the
    /// panel color.
    /// </summary>
    internal static Border DownloadBody(params Control[] rows)
    {
        var stack = new StackPanel { Spacing = 10 };
        foreach (var row in rows)
        {
            stack.Children.Add(row);
        }

        return new Border
        {
            Child = stack,
            Padding = new Thickness(20, 18),
        };
    }
}
