using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Controls;

/// <summary>
/// The appearance font is applied through app-wide styles per control type. The source editor
/// draws its own text, so it is not a TextBox and needs its own setter - without it the source
/// view stayed in Avalonia's default sans while the rest of the window used the chosen font
/// (#14457).
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
}
