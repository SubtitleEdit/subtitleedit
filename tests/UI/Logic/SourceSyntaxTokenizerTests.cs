using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Shared.MediaInfoView;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// The span model behind <see cref="ISourceSyntaxHighlighter"/>: the rules are written once and
/// rendered by both the source editor and the syntax text box, so the spans they produce have to
/// be sorted, non-overlapping and correctly offset.
/// </summary>
public class SourceSyntaxTokenizerTests
{
    private static readonly Color Red = Colors.Red;
    private static readonly Color Blue = Colors.Blue;

    private static List<SourceSyntaxSpan> Flatten(int length, Action<SourceSyntaxLineStyler> apply, int offset = 0)
    {
        var styler = new SourceSyntaxLineStyler();
        styler.Reset(length);
        apply(styler);
        var spans = new List<SourceSyntaxSpan>();
        styler.Flatten(offset, spans);
        return spans;
    }

    [Fact]
    public void UncoloredTextProducesNoSpans()
    {
        Assert.Empty(Flatten(10, _ => { }));
    }

    [Fact]
    public void AdjacentEqualStylesMergeIntoOneSpan()
    {
        var spans = Flatten(6, s =>
        {
            s.Apply(0, 3, Red);
            s.Apply(3, 3, Red);
        });

        var span = Assert.Single(spans);
        Assert.Equal(new SourceSyntaxSpan(0, 6, Red, false), span);
    }

    [Fact]
    public void LaterApplyWinsPerCharacter()
    {
        // Overlapping rules layer: the last color wins.
        var spans = Flatten(6, s =>
        {
            s.Apply(0, 6, Red);
            s.Apply(2, 2, Blue);
        });

        Assert.Equal(
        [
            new SourceSyntaxSpan(0, 2, Red, false),
            new SourceSyntaxSpan(2, 2, Blue, false),
            new SourceSyntaxSpan(4, 2, Red, false),
        ], spans);
    }

    [Fact]
    public void BoldSurvivesALaterRecolor()
    {
        // Bold set by an earlier rule survives a later rule that only changes the foreground
        // (e.g. a number inside a bold media info track header).
        var spans = Flatten(4, s =>
        {
            s.Apply(0, 4, Red, bold: true);
            s.Apply(1, 2, Blue);
        });

        Assert.All(spans, span => Assert.True(span.Bold));
        Assert.Equal(3, spans.Count);
    }

    [Fact]
    public void ApplyClipsToTheLine()
    {
        var spans = Flatten(4, s => s.Apply(-3, 20, Red));

        Assert.Equal(new SourceSyntaxSpan(0, 4, Red, false), Assert.Single(spans));
    }

    [Fact]
    public void StylerBuffersAreClearedBetweenLines()
    {
        var styler = new SourceSyntaxLineStyler();
        var spans = new List<SourceSyntaxSpan>();

        styler.Reset(8);
        styler.Apply(0, 8, Red, bold: true);
        styler.Flatten(0, spans);
        spans.Clear();

        styler.Reset(8);
        styler.Apply(4, 4, Blue);
        styler.Flatten(0, spans);

        Assert.Equal(new SourceSyntaxSpan(4, 4, Blue, false), Assert.Single(spans));
    }

    [Fact]
    public void SpansAreOffsetPerLineAcrossMixedLineBreaks()
    {
        const string text = "1\r\n2\n3\r4";
        var spans = SourceSyntaxTokenizer.Tokenize(text, new EveryCharHighlighter());

        // One span per line, each at the line's offset in the whole text.
        Assert.Equal(
        [
            new SourceSyntaxSpan(0, 1, Red, false),
            new SourceSyntaxSpan(3, 1, Red, false),
            new SourceSyntaxSpan(5, 1, Red, false),
            new SourceSyntaxSpan(7, 1, Red, false),
        ], spans);
    }

    [Fact]
    public void EmptyLinesProduceNoSpans()
    {
        Assert.Empty(SourceSyntaxTokenizer.Tokenize("\r\n\n\r", new EveryCharHighlighter()));
        Assert.Empty(SourceSyntaxTokenizer.Tokenize(string.Empty, new EveryCharHighlighter()));
    }

    [Fact]
    public void SubRipSpansStaySortedAndInsideTheText()
    {
        const string text = "1\r\n00:00:01,000 --> 00:00:02,000\r\nHello <i>world</i>\r\n";
        var spans = SourceSyntaxTokenizer.Tokenize(text, new SubRipSourceSyntaxHighlighting());

        Assert.NotEmpty(spans);
        var position = 0;
        foreach (var span in spans)
        {
            Assert.True(span.Start >= position, $"span at {span.Start} overlaps or is out of order");
            Assert.True(span.Start + span.Length <= text.Length);
            position = span.Start + span.Length;
        }
    }

    [Fact]
    public void SubRipNumberLineIsBoldAndFullyColored()
    {
        const string text = "12\r\nHello";
        var spans = SourceSyntaxTokenizer.Tokenize(text, new SubRipSourceSyntaxHighlighting());

        var numberSpan = spans[0];
        Assert.Equal(0, numberSpan.Start);
        Assert.Equal(2, numberSpan.Length);
        Assert.True(numberSpan.Bold);
    }

    [Fact]
    public void SubRipTimecodeSeparatorGetsItsOwnColor()
    {
        const string text = "00:00:01,000 --> 00:00:02,000";
        var spans = SourceSyntaxTokenizer.Tokenize(text, new SubRipSourceSyntaxHighlighting());

        Assert.Equal(3, spans.Count);
        Assert.Equal("00:00:01,000", text.Substring(spans[0].Start, spans[0].Length));
        Assert.Equal(" --> ", text.Substring(spans[1].Start, spans[1].Length));
        Assert.Equal("00:00:02,000", text.Substring(spans[2].Start, spans[2].Length));
        Assert.NotEqual(spans[0].Color, spans[1].Color);
        Assert.Equal(spans[0].Color, spans[2].Color);
    }

    [AvaloniaFact]
    public void MediaInfoFieldHeaderIsBoldAndValueIsColoredSeparately()
    {
        const string text = "Duration: 00:01:02";
        var spans = SourceSyntaxTokenizer.Tokenize(text, new MediaInfoSyntaxHighlighting());

        var header = spans[0];
        Assert.Equal("Duration:", text.Substring(header.Start, header.Length));
        Assert.True(header.Bold);
        Assert.Contains(spans, s => s.Start >= header.Length && s.Color != header.Color);
    }

    [Fact]
    public void SingleLineXmlIsReformattedButMultiLineXmlIsLeftAlone()
    {
        var singleLine = "<tt>" + string.Concat(Enumerable.Repeat("<p begin=\"1\">hi</p>", 30)) + "</tt>";
        ISourceSyntaxDocumentFormatter formatter = new XmlSourceSyntaxHighlighting();

        Assert.True(formatter.TryFormat(singleLine, out var formatted));
        Assert.Contains('\n', formatted);

        Assert.False(formatter.TryFormat(formatted, out var unchanged));
        Assert.Equal(formatted, unchanged);
    }

    private sealed class EveryCharHighlighter : ISourceSyntaxHighlighter
    {
        public void HighlightLine(string lineText, SourceSyntaxLineStyler styler)
        {
            styler.Apply(0, lineText.Length, Red);
        }
    }
}
