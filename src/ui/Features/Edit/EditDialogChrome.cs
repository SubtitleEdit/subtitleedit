using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Sync;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Edit;

/// <summary>
/// Small building blocks for the Edit menu dialogs (find, replace, modify selection, history,
/// alignment picker) in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
internal static class EditDialogChrome
{
    /// <summary>
    /// Window background, the editor styles, compact combo boxes, and text boxes (class "text",
    /// plus the one inside an AutoCompleteBox) on the field colors.
    /// </summary>
    internal static void Apply(Window window)
    {
        SyncTimingChrome.Apply(window);
        Setter[] textSetters =
        [
            new Setter(TemplatedControl.FontSizeProperty, 12.5),
            new Setter(TemplatedControl.MinHeightProperty, 28.0),
            new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 4)),
            new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            new Setter(TemplatedControl.BackgroundProperty, EditorChrome.FieldBrush),
            new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
            new Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center),
        ];
        var textStyle = new Style(x => x.OfType<TextBox>().Class("text"));
        var autoCompleteStyle = new Style(x => x.OfType<AutoCompleteBox>().Template().OfType<TextBox>());
        foreach (var setter in textSetters)
        {
            textStyle.Setters.Add(setter);
            autoCompleteStyle.Setters.Add(new Setter(setter.Property!, setter.Value));
        }

        window.Styles.Add(textStyle);
        window.Styles.Add(autoCompleteStyle);

        // Keep the field color while hovered/focused (the theme turns a focused box black/white)
        foreach (var state in new[] { ":pointerover", ":focus" })
        {
            window.Styles.Add(new Style(x => x.OfType<TextBox>().Class("text").Class(state).Template().OfType<Border>().Name("PART_BorderElement"))
            {
                Setters = { new Setter(Border.BackgroundProperty, EditorChrome.FieldBrush) },
            });
            window.Styles.Add(new Style(x => x.OfType<AutoCompleteBox>().Template().OfType<TextBox>().Class(state).Template().OfType<Border>().Name("PART_BorderElement"))
            {
                Setters = { new Setter(Border.BackgroundProperty, EditorChrome.FieldBrush) },
            });
        }

        // The accent of a primary button would hide that it is disabled (Restore selected without
        // a selection)
        window.Styles.Add(new Style(x => x.OfType<Button>().Class("primary").Class(":disabled"))
        {
            Setters = { new Setter(Visual.OpacityProperty, 0.45) },
        });
    }

    /// <summary>
    /// A list (table view) in a rounded frame on the field color, like the editor's lists.
    /// </summary>
    internal static Border MakeListFrame(Control list)
    {
        if (list is TemplatedControl templated)
        {
            templated.Background = Brushes.Transparent; // the frame's field color shows through
        }

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
    /// An action column on the button bar color, with a line to its left - the buttons of the
    /// non-modal find/replace windows, which have no OK/Cancel.
    /// </summary>
    internal static Border MakeActionColumn(Control content)
    {
        return new Border
        {
            Child = content,
            Background = EditorChrome.ButtonBarBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
            Padding = new Thickness(14, 14, 14, 12),
        };
    }

    /// <summary>
    /// A text action button of the action column: stretched, left aligned, icon and text, with the
    /// access key (Alt+letter) kept from <see cref="UiUtil.MakeButton(string, CommunityToolkit.Mvvm.Input.IRelayCommand?, object?)"/>.
    /// </summary>
    internal static Button StyleActionButton(Button button)
    {
        button.Margin = new Thickness(0);
        button.MinWidth = 0;
        button.MinHeight = 30;
        button.Padding = new Thickness(10, 5);
        button.FontSize = 12.5;
        button.HorizontalAlignment = HorizontalAlignment.Stretch;
        button.HorizontalContentAlignment = HorizontalAlignment.Left;
        button.CornerRadius = new CornerRadius(6);
        return button;
    }

    /// <summary>
    /// The sections on the panel color, the action column to the right.
    /// </summary>
    internal static Grid MakeSidePanelRoot(Control sections, Control actions)
    {
        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        grid.Add(new Border { Background = EditorChrome.PanelBrush, Child = sections }, 0, 0);
        grid.Add(actions, 0, 1);
        return grid;
    }

    internal static TextBlock MakeLabel(string text)
    {
        var label = EditorChrome.MakeDimLabel(text, new Thickness(0));
        label.TextWrapping = TextWrapping.Wrap;
        return label;
    }
}
