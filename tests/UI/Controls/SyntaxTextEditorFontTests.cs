using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Controls;

/// <summary>
/// The appearance font is applied through app-wide styles per control type. The source editor
/// draws its own text, so it is not a TextBox and needs its own setter - without it the source
/// view stayed in Avalonia's default sans while the rest of the window used the chosen font
/// (#14457). Line numbers and time codes are the exception: they stay in the platform default
/// font, because a text face with old-style numerals makes digits hard to scan.
/// </summary>
public class SyntaxTextEditorFontTests
{
    [AvaloniaFact]
    public void AppearanceFontReachesTheSourceEditorAndItsTextSurface()
    {
        var app = Application.Current!;
        var savedFontName = Se.Settings.Appearance.FontName;
        var styleCount = app.Styles.Count;
        Window? window = null;
        try
        {
            Se.Settings.Appearance.FontName = "Georgia";
            UiUtil.SetFontName("Georgia");

            var editor = new SyntaxTextEditor { Text = "1", Height = 80 };

            // The format preview picks a monospace family locally - that must still win.
            var monoEditor = new SyntaxTextEditor { Text = "1", Height = 80, FontFamily = new FontFamily("Courier New") };

            window = new Window { Content = new StackPanel { Children = { editor, monoEditor } } };
            window.Show();
            window.UpdateLayout();

            Assert.Equal("Georgia", editor.FontFamily.Name);
            Assert.Equal("Georgia", editor.View.FontFamily.Name);
            Assert.Equal("Courier New", monoEditor.FontFamily.Name);
            Assert.Equal("Courier New", monoEditor.View.FontFamily.Name);

            // The line number gutter never follows the editor font.
            var gutter = editor.GetVisualDescendants().OfType<LineNumberGutter>().Single();
            Assert.Equal(FontFamily.Default, gutter.FontFamily);
        }
        finally
        {
            window?.Close();
            while (app.Styles.Count > styleCount)
            {
                app.Styles.RemoveAt(app.Styles.Count - 1);
            }

            Se.Settings.Appearance.FontName = savedFontName;
        }
    }

    [AvaloniaFact]
    public void TimeCodesAndNumbersAreFlaggedForTheDefaultFontButTextIsNot()
    {
        const string text = "12\r\n00:00:01,000 --> 00:00:02,000\r\n<i>Hi</i> there";
        var spans = SourceSyntaxTokenizer.Tokenize(text, new SubRipSourceSyntaxHighlighting());

        string Of(SourceSyntaxSpan s) => text.Substring(s.Start, s.Length);
        Assert.True(spans.Single(s => Of(s) == "12").DefaultFont);
        Assert.True(spans.Single(s => Of(s) == "00:00:01,000").DefaultFont);
        Assert.True(spans.Single(s => Of(s).Contains("-->")).DefaultFont);
        Assert.All(spans.Where(s => s.Start >= text.IndexOf("<i>", StringComparison.Ordinal)), s => Assert.False(s.DefaultFont));

        const string assa = "Dialogue: 0,0:00:01.00,0:00:02.00,Default,,0,0,0,,Hi";
        var assaSpans = SourceSyntaxTokenizer.Tokenize(assa, new AssaSourceSyntaxHighlighting());
        Assert.True(assaSpans.Single(s => assa.Substring(s.Start, s.Length) == "0:00:01.00").DefaultFont);
        Assert.False(assaSpans.Single(s => assa.Substring(s.Start, s.Length) == "Dialogue:").DefaultFont);
    }
}
