using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

public class LrcSourceSyntaxHighlightingTests
{
    private static List<SourceSyntaxSpan> Spans(string line) =>
        SourceSyntaxTokenizer.Tokenize(line, new LrcSourceSyntaxHighlighting());

    private static SourceSyntaxSpan? SpanOf(string line, string token)
    {
        var index = line.IndexOf(token, StringComparison.Ordinal);
        Assert.True(index >= 0, $"'{token}' not in line");
        var covering = Spans(line).Where(s => s.Start <= index && index + token.Length <= s.Start + s.Length).ToList();
        return covering.Count == 0 ? null : covering[0];
    }

    private static bool IsUntouched(string line, string token)
    {
        var index = line.IndexOf(token, StringComparison.Ordinal);
        return !Spans(line).Exists(s => s.Start < index + token.Length && s.Start + s.Length > index);
    }

    [AvaloniaFact]
    public void EveryLrcVariantGetsTheLrcRules()
    {
        Assert.IsType<LrcSourceSyntaxHighlighting>(SourceSyntaxHighlighterFactory.ForFormat(string.Empty, new Lrc()));
        Assert.IsType<LrcSourceSyntaxHighlighting>(SourceSyntaxHighlighterFactory.ForFormat(string.Empty, new Lrc3DigitsMs()));
        Assert.IsType<LrcSourceSyntaxHighlighting>(SourceSyntaxHighlighterFactory.ForFormat(string.Empty, new LrcNoEndTime()));
    }

    [AvaloniaFact]
    public void HeaderTagsSplitIntoNameAndValue()
    {
        const string line = "[ar:Some Artist]";
        Assert.Equal(AssaSourceSyntaxHighlighting.PropertyColor, SpanOf(line, "ar")?.Color);
        Assert.Equal(AssaSourceSyntaxHighlighting.ValueColor, SpanOf(line, "Some Artist")?.Color);
        Assert.Equal(SubtitleSyntaxTokenizer.CharsColor, SpanOf(line, "]")?.Color);
    }

    [AvaloniaTheory]
    [InlineData("[00:12.34]Lyrics", "00:12.34")]
    [InlineData("[00:12.345]Lyrics", "00:12.345")]
    [InlineData("[00:06.00]", "00:06.00")]
    public void LineTimeStampsAreBoldTimeCodes(string line, string time)
    {
        var span = SpanOf(line, time);
        Assert.Equal(SubRipSourceSyntaxHighlighting.TimeColor, span?.Color);
        Assert.True(span?.Bold);
        Assert.True(IsUntouched(line, "Lyrics") || !line.Contains("Lyrics"));
    }

    [AvaloniaFact]
    public void StackedTimeStampsAreAllColored()
    {
        const string line = "[00:12.00][01:30.00]Chorus";
        Assert.Equal(SubRipSourceSyntaxHighlighting.TimeColor, SpanOf(line, "00:12.00")?.Color);
        Assert.Equal(SubRipSourceSyntaxHighlighting.TimeColor, SpanOf(line, "01:30.00")?.Color);
        Assert.True(IsUntouched(line, "Chorus"));
    }

    [AvaloniaFact]
    public void EnhancedWordTimesAreColoredButNotBold()
    {
        const string line = "[00:12.00]<00:12.00>Hello <00:12.50>world";
        var word = SpanOf(line, "00:12.50");
        Assert.Equal(SubRipSourceSyntaxHighlighting.TimeColor, word?.Color);
        Assert.False(word?.Bold);
        Assert.True(IsUntouched(line, "world"));
    }

    [AvaloniaFact]
    public void TextInBracketsIsNotAHeaderAfterATimeStamp()
    {
        Assert.True(IsUntouched("[00:12.00][Chorus: loud]", "Chorus"));
    }
}
