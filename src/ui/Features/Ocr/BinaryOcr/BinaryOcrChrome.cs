using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

/// <summary>
/// The pieces only the binary OCR dialogs, the unknown word prompt, the VobSub color chooser and
/// image pre-processing use (subtitle image frames, clickable switch labels, ...); the rest of their
/// editor look comes from <see cref="EditorChrome"/>.
/// </summary>
internal static class BinaryOcrChrome
{
    /// <summary>
    /// A rounded frame for a subtitle bitmap. Subtitle bitmaps are light or dark text on
    /// transparent pixels, so they keep their own backdrop (checker/preview background) instead
    /// of the panel color, or white text vanishes on the light theme (#15111).
    /// </summary>
    internal static Border MakeImageFrame(Control image, IBrush background, double padding = 6)
    {
        return new Border
        {
            Child = image,
            Background = background,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(padding),
            ClipToBounds = true,
        };
    }

    /// <summary>
    /// A switch with its label beside it; clicking the label toggles too and it dims with the switch.
    /// </summary>
    internal static StackPanel MakeSwitchRow(ToggleSwitch toggle, string text)
    {
        var label = EditorChrome.MakeSwitchLabel(text);

        // Clicking the label toggles too, like the check box it replaces
        label.PointerPressed += (_, e) =>
        {
            if (toggle.IsEnabled && e.GetCurrentPoint(label).Properties.IsLeftButtonPressed)
            {
                toggle.IsChecked = toggle.IsChecked != true;
                e.Handled = true;
            }
        };
        label.Bind(Visual.OpacityProperty, new Binding(nameof(ToggleSwitch.IsEnabled))
        {
            Source = toggle,
            Converter = new Avalonia.Data.Converters.FuncValueConverter<bool, double>(enabled => enabled ? 1.0 : 0.45),
        });

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { toggle, label },
        };
    }

    /// <summary>
    /// A text action with an icon (Update, Delete, ...), sized like the spell check actions.
    /// </summary>
    internal static Button MakeActionButton(string text, string icon, System.Windows.Input.ICommand? command)
    {
        var button = EditorChrome.StyleActionColumnButton(new Button
        {
            Content = text,
            Command = command,
        }.WithIconLeft(icon));
        button.HorizontalAlignment = HorizontalAlignment.Left;
        return button;
    }

    /// <summary>
    /// A text box on the field colors (class "text" of <see cref="EditorChrome"/>).
    /// </summary>
    internal static T AsField<T>(T textBox) where T : TextBox
    {
        textBox.AsTextField();
        textBox.Width = double.NaN;
        textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        return textBox;
    }

    /// <summary>
    /// Dim info text, e.g. resolution and top margin of the current image.
    /// </summary>
    internal static TextBlock MakeInfo(string property)
    {
        var info = EditorChrome.MakeBoundInfo(property);
        info.VerticalAlignment = VerticalAlignment.Center;
        return info;
    }
}
