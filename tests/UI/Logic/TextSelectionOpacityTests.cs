using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// The "Text selection opacity" setting (#14744) makes the text box selection highlight
/// translucent and lets the selected text keep its own color; at 100% the theme is untouched.
/// </summary>
public class TextSelectionOpacityTests
{
    [AvaloniaFact]
    public void Reduced_Opacity_Overrides_TextBox_Selection_Brushes()
    {
        var old = Se.Settings.Appearance.TextSelectionOpacity;
        var window = new Window();
        try
        {
            Se.Settings.Appearance.TextSelectionOpacity = 40;
            UiTheme.ApplyTextSelectionStyle();

            var textBox = new TextBox { Text = "Hello" };
            window.Content = textBox;
            window.Show();

            var brush = Assert.IsAssignableFrom<ISolidColorBrush>(textBox.SelectionBrush);
            Assert.Equal(0.4, brush.Opacity, 3);
            Assert.Null(textBox.SelectionForegroundBrush);

            Se.Settings.Appearance.TextSelectionOpacity = 100;
            UiTheme.ApplyTextSelectionStyle();

            Assert.NotEqual(0.4, (textBox.SelectionBrush as ISolidColorBrush)?.Opacity ?? 1.0, 3);
        }
        finally
        {
            window.Close();
            Se.Settings.Appearance.TextSelectionOpacity = old;
            UiTheme.ApplyTextSelectionStyle();
        }
    }
}
