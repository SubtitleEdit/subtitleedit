using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// WebVTT used to share the SubRip rules, which only knew "hh:mm:ss.fff --> hh:mm:ss.fff": the
/// short time codes, cue settings, NOTE/STYLE blocks and the VTT-only cue tags went uncolored.
/// </summary>
public class WebVttSourceSyntaxHighlightingTests
{
    private const string Source =
        "WEBVTT - Example\n" +
        "\n" +
        "NOTE written by hand\n" +
        "\n" +
        "STYLE\n" +
        "::cue(.yellow) {\n" +
        "  color: yellow;\n" +
        "}\n" +
        "\n" +
        "1\n" +
        "01:02.500 --> 01:04.000 align:start position:10%\n" +
        "<v Bob>Hi <c.yellow>there</c> &amp; <00:01:03.000>bye\n" +
        "1984\n" +
        "\n" +
        "intro\n" +
        "00:01:05.000 --> 00:01:06.000\n" +
        "Wait;\n" +
        "Time: now\n";

    private static List<SourceSyntaxSpan> Spans => SourceSyntaxTokenizer.Tokenize(Source, new WebVttSourceSyntaxHighlighting());

    private static SourceSyntaxSpan? SpanAt(string token, int occurrence = 0)
    {
        var index = -1;
        for (var i = 0; i <= occurrence; i++)
        {
            index = Source.IndexOf(token, index + 1, StringComparison.Ordinal);
        }

        Assert.True(index >= 0, $"'{token}' not in source");
        var spans = Spans.Where(s => s.Start <= index && index + token.Length <= s.Start + s.Length).ToList();
        return spans.Count == 0 ? null : spans[0];
    }

    private static Color ColorAt(string token, int occurrence = 0) =>
        SpanAt(token, occurrence)?.Color ?? throw new Xunit.Sdk.XunitException($"'{token}' is not colored");

    [AvaloniaFact]
    public void FactoryPicksTheWebVttRules()
    {
        Assert.IsType<WebVttSourceSyntaxHighlighting>(SourceSyntaxHighlighterFactory.ForFormat(Source, new WebVTT()));
        Assert.IsType<WebVttSourceSyntaxHighlighting>(SourceSyntaxHighlighterFactory.ForFormat(Source, new WebVTTFileWithLineNumber()));
        Assert.IsType<SubRipSourceSyntaxHighlighting>(SourceSyntaxHighlighterFactory.ForFormat(Source, new SubRip()));
    }

    [AvaloniaFact]
    public void ShortTimeCodesAndTheArrowAreColored()
    {
        Assert.Equal(SubRipSourceSyntaxHighlighting.TimeColor, ColorAt("01:02.500"));
        Assert.Equal(SubRipSourceSyntaxHighlighting.TimeColor, ColorAt("01:04.000"));
        Assert.Equal(SubRipSourceSyntaxHighlighting.TimeSeparatorColor, ColorAt("-->"));
        Assert.Equal(SubRipSourceSyntaxHighlighting.TimeColor, ColorAt("00:01:05.000"));
    }

    [AvaloniaFact]
    public void CueSettingsSplitIntoNameAndValue()
    {
        Assert.Equal(AssaSourceSyntaxHighlighting.PropertyColor, ColorAt("align"));
        Assert.Equal(AssaSourceSyntaxHighlighting.ValueColor, ColorAt("start"));
        Assert.Equal(AssaSourceSyntaxHighlighting.ValueColor, ColorAt("10%"));
    }

    [AvaloniaFact]
    public void BlocksAreColoredByTheirFirstLine()
    {
        Assert.Equal(AssaSourceSyntaxHighlighting.SectionColor, ColorAt("WEBVTT"));
        Assert.Null(SpanAt("Example")); // the header's free text is not the keyword
        Assert.Equal(AssaSourceSyntaxHighlighting.CommentColor, ColorAt("NOTE written by hand"));
        Assert.Equal(AssaSourceSyntaxHighlighting.KeywordColor, ColorAt("STYLE"));
    }

    [AvaloniaFact]
    public void StyleBlockCssIsColored()
    {
        Assert.Equal(SubtitleSyntaxTokenizer.ElementColor, ColorAt("::cue"));
        Assert.Equal(AssaSourceSyntaxHighlighting.PropertyColor, ColorAt("color"));
        Assert.Equal(AssaSourceSyntaxHighlighting.ValueColor, ColorAt("yellow", 1));
    }

    [AvaloniaFact]
    public void CueTextTagsTimestampsAndEntities()
    {
        Assert.Equal(SubtitleSyntaxTokenizer.AttributeColor, ColorAt("Bob")); // <v Bob> reads as a tag
        Assert.Equal(SubtitleSyntaxTokenizer.ValuesColor, ColorAt(".yellow", 1)); // 0 is the ::cue selector
        Assert.Equal(SubRipSourceSyntaxHighlighting.TimeColor, ColorAt("00:01:03.000"));
        Assert.Equal(SubtitleSyntaxTokenizer.CharsColor, ColorAt("&amp;"));
    }

    [AvaloniaFact]
    public void NumbersAreCueIdentifiersOnlyWhenTheyOpenABlock()
    {
        var cueId = Source.IndexOf("\n1\n", StringComparison.Ordinal) + 1;
        Assert.Contains(Spans, s => s.Start == cueId && s.Length == 1 && s.Bold);
        Assert.Null(SpanAt("1984")); // cue text under the time code
    }

    [AvaloniaFact]
    public void CueTextEndingInASemicolonDoesNotStartCss()
    {
        Assert.Null(SpanAt("Time: now"));
    }
}
