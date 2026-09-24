using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// The fallback for the text formats without rules of their own: time codes, frame numbers and
/// markup get colored, ordinary text - including a clock time or a bare "&lt;" - does not.
/// </summary>
public class GenericSourceSyntaxHighlightingTests
{
    private static List<SourceSyntaxSpan> Spans(string line) =>
        SourceSyntaxTokenizer.Tokenize(line, new GenericSourceSyntaxHighlighting());

    private static bool IsColored(string line, string token)
    {
        var index = line.IndexOf(token, StringComparison.Ordinal);
        Assert.True(index >= 0, $"'{token}' not in line");
        return Spans(line).Exists(s => s.Start <= index && index + token.Length <= s.Start + s.Length);
    }

    private static bool IsUntouched(string line, string token)
    {
        var index = line.IndexOf(token, StringComparison.Ordinal);
        return !Spans(line).Exists(s => s.Start < index + token.Length && s.Start + s.Length > index);
    }

    [AvaloniaFact]
    public void FormatsWithoutOwnRulesGetTheGenericOnes()
    {
        Assert.IsType<GenericSourceSyntaxHighlighting>(SourceSyntaxHighlighterFactory.ForFormat("{1}{25}Hi", new MicroDvd()));
        Assert.IsType<GenericSourceSyntaxHighlighting>(SourceSyntaxHighlighterFactory.ForFormat("00:00:01.00,00:00:02.00", new SubViewer20()));
    }

    [AvaloniaTheory]
    [InlineData("00:00:01:12\t00:00:03:05\tFrame based", "00:00:01:12")]
    [InlineData("00:00:01;12\t9420 9420 94ae", "00:00:01;12")]
    [InlineData("0:00:01.50,0:00:03.00", "0:00:03.00")]
    [InlineData("[00:12.34]Lyrics line", "00:12.34")]
    [InlineData("00:00:01.000,00:00:02.000", "00:00:02.000")]
    public void TimeCodesAreColored(string line, string time)
    {
        Assert.True(IsColored(line, time));
        Assert.Equal(SubRipSourceSyntaxHighlighting.TimeColor, Spans(line).First(s => s.Start == line.IndexOf(time, StringComparison.Ordinal)).Color);
    }

    [AvaloniaFact]
    public void AClockTimeInTheTextIsNotATimeCode()
    {
        Assert.True(IsUntouched("See you at 10:30 tomorrow", "10:30"));
    }

    [AvaloniaFact]
    public void MicroDvdFramesAndControlCodes()
    {
        const string line = "{25}{75}{y:i}Hello|World";
        Assert.True(IsColored(line, "{25}"));
        Assert.True(IsColored(line, "{75}"));
        Assert.True(IsColored(line, "y"));
        Assert.True(IsUntouched(line, "Hello"));
    }

    [AvaloniaFact]
    public void TagsAreColoredButABareLessThanIsText()
    {
        Assert.True(IsColored("00:00:01.000 <i>Hi</i>", "i"));
        Assert.True(IsUntouched("if a < b then c", "then"));
    }

    [AvaloniaFact]
    public void AnArrowIsOnlyASeparatorBetweenTimeCodes()
    {
        Assert.True(IsColored("00:00:01.000 --> 00:00:02.000", "-->"));
        Assert.True(IsUntouched("Go left -> then right", "->"));
    }
}
