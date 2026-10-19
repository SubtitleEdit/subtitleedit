using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using System.Windows.Input;

namespace Nikse.SubtitleEdit.Features.Main.AssistedSplit;

/// <summary>
/// The pieces assisted split and assisted move share in the editor look (see
/// <see cref="EditorChrome"/>): the current text on top, the candidates as clickable cards on the
/// field color with a numbered accent badge, and Cancel in the button bar.
/// </summary>
internal static class AssistedCardChrome
{
    /// <summary>
    /// Window chrome plus the card button styles: field color and border, soft accent while
    /// hovered or focused (keyboard users see where they are).
    /// </summary>
    internal static void Apply(Window window)
    {
        EditorChrome.ApplyWindow(window, EditorWindowStyles.Dialog);
        var styles = window.Styles;
        styles.Add(new Style(x => x.OfType<Button>().Class("card"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, EditorChrome.FieldBrush),
                new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(8)),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(10)),
            },
        });
        foreach (var state in new[] { ":pointerover", ":focus-visible", ":pressed" })
        {
            styles.Add(new Style(x => x.OfType<Button>().Class("card").Class(state).Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
            {
                Setters =
                {
                    new Setter(ContentPresenter.BackgroundProperty, EditorChrome.AccentSoftBrush),
                    new Setter(ContentPresenter.BorderBrushProperty, EditorChrome.AccentBrush),
                    new Setter(ContentPresenter.ForegroundProperty, EditorChrome.StrongTextBrush),
                },
            });
        }
    }

    /// <summary>
    /// The heading and the current subtitle text in a field frame.
    /// </summary>
    internal static Border MakeHeader(string heading, string currentText)
    {
        var labelHeader = EditorChrome.MakeDimLabel(heading, new Thickness(0));
        labelHeader.FontSize = EditorChrome.FieldFontSize;
        labelHeader.FontWeight = FontWeight.SemiBold;
        labelHeader.TextWrapping = TextWrapping.Wrap;

        var originalText = new TextBlock
        {
            Text = currentText,
            FontSize = EditorChrome.FieldFontSize,
            TextWrapping = TextWrapping.Wrap,
        };
        var frame = new Border
        {
            Child = originalText,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10, 8),
        };

        return new Border
        {
            Child = new StackPanel { Spacing = 10, Children = { labelHeader, frame } },
            Padding = new Thickness(14, 12, 14, 12),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
        };
    }

    /// <summary>
    /// A candidate card: the number key in an accent badge, the title and the text previews.
    /// </summary>
    internal static Button MakeCard(int number, string title, ICommand command, object parameter, params Control[] previews)
    {
        var badge = new Border
        {
            Width = EditorChrome.ScaledSize(30),
            Height = EditorChrome.ScaledSize(30),
            CornerRadius = new CornerRadius(EditorChrome.ScaledSize(15)),
            Background = EditorChrome.AccentSoftBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0),
            Child = new TextBlock
            {
                Text = number.ToString(),
                FontSize = EditorChrome.ScaledFont(15),
                FontWeight = FontWeight.Bold,
                Foreground = EditorChrome.AccentBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

        var panelTexts = new StackPanel
        {
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        panelTexts.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = EditorChrome.FieldFontSize,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
        });
        foreach (var preview in previews)
        {
            panelTexts.Children.Add(preview);
        }

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        grid.Add(badge, 0, 0);
        grid.Add(panelTexts, 0, 1);

        var button = new Button
        {
            Content = grid,
            Command = command,
            CommandParameter = parameter,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        };
        button.Classes.Add("card");
        Avalonia.Automation.AutomationProperties.SetName(button, number + " " + title);
        return button;
    }

    /// <summary>
    /// One resulting text with its info (duration, CPS, ...) under it.
    /// </summary>
    internal static Border MakePreview(string text, string info)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            FontSize = EditorChrome.FieldFontSize,
            TextWrapping = TextWrapping.Wrap,
        };

        var infoBlock = new TextBlock
        {
            Text = info,
            FontSize = EditorChrome.ScaledFont(11),
            Foreground = EditorChrome.DimTextBrush,
            Margin = new Thickness(0, 3, 0, 0),
        };

        return new Border
        {
            Background = EditorChrome.PanelBrush,
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.LineBrush,
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 5),
            Child = new StackPanel { Children = { textBlock, infoBlock } },
        };
    }

    /// <summary>
    /// Header, the scrolling cards and the button bar with Cancel.
    /// </summary>
    internal static Grid MakeRoot(Border header, ScrollViewer cards, Button buttonCancel)
    {
        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
        };
        sections.Add(header, 0, 0);
        sections.Add(cards, 1, 0);
        return EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel));
    }
}
