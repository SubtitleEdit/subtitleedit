using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// The source view highlighters use the VS Code dark palette, which is washed out on white, so
/// each of them carries a darker light-mode set (#14457). The shared tag pastels are covered by
/// <see cref="SubtitleSyntaxThemeColorTests"/> and deliberately stay the same in both themes.
/// </summary>
public class SourceSyntaxThemeColorTests
{
    private static SourceSyntaxSpan SpanCovering(string text, ISourceSyntaxHighlighter highlighter, string token)
    {
        var index = text.IndexOf(token, StringComparison.Ordinal);
        Assert.True(index >= 0, $"'{token}' not in text");
        return SourceSyntaxTokenizer.Tokenize(text, highlighter)
            .Single(s => s.Start <= index && index + token.Length <= s.Start + s.Length);
    }

    private static (SourceSyntaxSpan Dark, SourceSyntaxSpan Light) InBothThemes(string text, ISourceSyntaxHighlighter highlighter, string token)
    {
        var savedTheme = Se.Settings.Appearance.Theme;
        try
        {
            Se.Settings.Appearance.Theme = UiTheme.ThemeNameDark;
            var dark = SpanCovering(text, highlighter, token);
            Se.Settings.Appearance.Theme = UiTheme.ThemeNameLight;
            var light = SpanCovering(text, highlighter, token);
            return (dark, light);
        }
        finally
        {
            Se.Settings.Appearance.Theme = savedTheme;
        }
    }

    private static double Luminance(Color c)
    {
        static double Channel(byte v)
        {
            var s = v / 255.0;
            return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);
    }

    private static double ContrastOnWhite(Color c) => 1.05 / (Luminance(c) + 0.05);

    private static void AssertLightIsDarkerAndReadable((SourceSyntaxSpan Dark, SourceSyntaxSpan Light) spans)
    {
        Assert.NotEqual(spans.Dark.Color, spans.Light.Color);
        Assert.Equal(byte.MaxValue, spans.Light.Color.A);
        Assert.True(ContrastOnWhite(spans.Light.Color) >= 4.4, $"{spans.Light.Color} is too faint on white");
        Assert.True(Luminance(spans.Light.Color) < Luminance(spans.Dark.Color), "light-mode color should be the darker one");
    }

    [AvaloniaFact]
    public void SubRipTimeCodeIsSolidAndReadableInLightMode()
    {
        const string text = "1\r\n00:00:01,000 --> 00:00:02,000\r\nHi";
        var highlighter = new SubRipSourceSyntaxHighlighting();

        // The dark time color is half transparent - what made it nearly invisible on white.
        var time = InBothThemes(text, highlighter, "00:00:01,000");
        Assert.NotEqual(byte.MaxValue, time.Dark.Color.A);
        AssertLightIsDarkerAndReadable(time);

        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "-->"));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "1"));
    }

    [AvaloniaFact]
    public void XmlColorsFollowTheme()
    {
        const string text = "<p begin=\"1s\">Hi</p><!-- c -->";
        var highlighter = new XmlSourceSyntaxHighlighting();

        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "<p"));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "begin"));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "\"1s\""));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "<!-- c -->"));
    }

    [AvaloniaFact]
    public void JsonColorsFollowTheme()
    {
        const string text = "{\r\n  \"start\": 12,\r\n  \"ok\": true,\r\n  \"tags\": [\r\n    \"Hi\"\r\n  ]\r\n}";
        var highlighter = new JsonSourceSyntaxHighlighting();

        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "\"start\""));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "12"));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "\"Hi\""));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "true"));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "{"));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, ","));
    }

    [AvaloniaFact]
    public void AssaColorsFollowTheme()
    {
        const string text = "[Events]\r\n; note\r\nTitle: Movie\r\nDialogue: 0,0:00:01.00,0:00:02.00,Default,,0,0,0,,Hi";
        var highlighter = new AssaSourceSyntaxHighlighting();

        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "[Events]"));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "; note"));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "Title"));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "Movie"));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "Dialogue:"));
        AssertLightIsDarkerAndReadable(InBothThemes(text, highlighter, "0:00:01.00"));
    }
}
