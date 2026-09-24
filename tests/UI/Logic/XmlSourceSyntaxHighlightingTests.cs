using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

public class XmlSourceSyntaxHighlightingTests
{
    [AvaloniaTheory]
    [InlineData("<!-- still typing")]
    [InlineData("<p>text</p> <!-- note --")]
    [InlineData("<!--")]
    public void AnUnclosedCommentIsColoredToTheEndOfTheLine(string line)
    {
        var spans = SourceSyntaxTokenizer.Tokenize(line, new XmlSourceSyntaxHighlighting());

        var last = spans[^1];
        Assert.Equal(line.Length, last.Start + last.Length);
        Assert.Equal(line.IndexOf("<!--", StringComparison.Ordinal), last.Start);
    }

    [AvaloniaFact]
    public void AClosedCommentEndsAtItsCloser()
    {
        const string line = "<!-- a --><p>";
        var spans = SourceSyntaxTokenizer.Tokenize(line, new XmlSourceSyntaxHighlighting());

        Assert.Equal(new SourceSyntaxSpan(0, 10, spans[0].Color, spans[0].Bold, spans[0].DefaultFont), spans[0]);
    }
}
