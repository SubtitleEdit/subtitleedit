using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features;

/// <summary>
/// The UI font must reach checkbox/radio labels (#15255), but must not override a font set
/// locally on a control - the subtitle edit boxes set their own font, and a style on every
/// TemplatedControl hit the TextBox's inner ScrollViewer so the text was drawn in the UI font.
/// </summary>
public class UiFontStyleTests
{
    [AvaloniaFact]
    public void UiFont_ReachesCheckBox_ButKeepsLocalTextBoxFont()
    {
        const string uiFont = "Courier New";
        const string textBoxFont = "Times New Roman";

        UiUtil.SetFontName(uiFont);
        var textBox = new TextBox { Text = "Hello", FontFamily = FontFamilyHelper.Make(textBoxFont) };
        var checkBox = new CheckBox { Content = "Whole word" };
        var window = new Window { Content = new StackPanel { Children = { textBox, checkBox } } };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(FontFamilyHelper.Make(uiFont).Name, checkBox.FontFamily.Name);

            var presenter = textBox.GetVisualDescendants().OfType<TextPresenter>().First();
            Assert.Equal(FontFamilyHelper.Make(textBoxFont).Name, presenter.FontFamily.Name);
        }
        finally
        {
            window.Close();
            UiUtil.SetFontName(string.Empty);
        }
    }
}
