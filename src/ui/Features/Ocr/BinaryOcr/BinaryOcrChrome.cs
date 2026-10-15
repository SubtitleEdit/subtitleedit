using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Edit;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

/// <summary>
/// Small building blocks for the binary OCR dialogs, the unknown word prompt, the VobSub color
/// chooser and image pre-processing in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
internal static class BinaryOcrChrome
{
    /// <summary>
    /// A section whose content fills the height left after its header (a list or an image that
    /// should grow with the window) - <see cref="EditorChrome.MakeEditorSection"/> stacks it.
    /// </summary>
    internal static Border MakeFillSection(string title, Control content, Control? headerRight = null, bool first = false)
    {
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            header.Children.Add(headerRight);
        }

        header.Children.Add(EditorChrome.MakeSectionHeader(title));
        var panel = new DockPanel();
        DockPanel.SetDock(header, Dock.Top);
        panel.Children.Add(header);
        panel.Children.Add(content);
        return new Border
        {
            Child = panel,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

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
    /// A compact on/off switch bound two way to <paramref name="property"/>; put it in a row with
    /// <see cref="MakeSwitchRow"/>.
    /// </summary>
    internal static ToggleSwitch MakeSwitch(string property, string text, string? isEnabledProperty = null)
    {
        var toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        if (isEnabledProperty != null)
        {
            toggle.Bind(Avalonia.Input.InputElement.IsEnabledProperty, new Binding(isEnabledProperty));
        }

        AutomationProperties.SetName(toggle, text);
        return toggle;
    }

    internal static StackPanel MakeSwitchRow(ToggleSwitch toggle, string text)
    {
        var label = new TextBlock
        {
            Text = text,
            FontSize = 12.5,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
        };

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
        var button = EditDialogChrome.StyleActionButton(new Button
        {
            Content = text,
            Command = command,
        }.WithIconLeft(icon));
        button.HorizontalAlignment = HorizontalAlignment.Left;
        return button;
    }

    /// <summary>
    /// A text box on the field colors (class "text" of <see cref="EditDialogChrome.Apply"/>).
    /// </summary>
    internal static T AsField<T>(T textBox) where T : TextBox
    {
        textBox.Classes.Add("text");
        textBox.Width = double.NaN;
        textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        return textBox;
    }

    /// <summary>
    /// Dim info text, e.g. resolution and top margin of the current image.
    /// </summary>
    internal static TextBlock MakeInfo(string property)
    {
        return new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(property) { Mode = BindingMode.OneWay },
        };
    }

    internal static TextBlock MakeStatusText(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 0, 12, 0),
        };
    }

    internal static Button AsPrimary(Button button)
    {
        button.Classes.Add("primary");
        button.Margin = new Thickness(0);
        return button;
    }

    internal static Button AsBarButton(Button button)
    {
        button.Margin = new Thickness(0);
        return button;
    }
}
