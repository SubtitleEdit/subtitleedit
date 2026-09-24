using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using WordKind = Nikse.SubtitleEdit.Logic.SccSourceSyntaxHighlighting.WordKind;

namespace UITests.Logic;

/// <summary>
/// SCC words are CEA-608 byte pairs with odd parity; the highlighter colors each by its decoded
/// kind so the caption text stands out from the codes around it.
/// </summary>
public class SccSourceSyntaxHighlightingTests
{
    [AvaloniaFact]
    public void BothSccVariantsGetTheSccRules()
    {
        Assert.IsType<SccSourceSyntaxHighlighting>(SourceSyntaxHighlighterFactory.ForFormat(string.Empty, new ScenaristClosedCaptions()));
        Assert.IsType<SccSourceSyntaxHighlighting>(SourceSyntaxHighlighterFactory.ForFormat(string.Empty, new ScenaristClosedCaptionsDropFrame()));
    }

    [Theory]
    [InlineData("9420", WordKind.Command)]         // RCL, channel 1
    [InlineData("1c20", WordKind.Command)]         // RCL, channel 2
    [InlineData("942c", WordKind.Command)]         // EDM
    [InlineData("942f", WordKind.Command)]         // EOC
    [InlineData("94ae", WordKind.Command)]         // ENM
    [InlineData("97a2", WordKind.Command)]         // tab offset 2
    [InlineData("9452", WordKind.PreambleAddress)] // row 15, indent 4
    [InlineData("91d0", WordKind.PreambleAddress)] // row 1
    [InlineData("91ae", WordKind.MidRow)]          // italics
    [InlineData("9137", WordKind.SpecialCharacter)] // music note
    [InlineData("92a7", WordKind.SpecialCharacter)] // extended character
    [InlineData("c8e5", WordKind.Text)]            // "He"
    [InlineData("e580", WordKind.Text)]            // "e" + null
    [InlineData("8080", WordKind.Padding)]
    [InlineData("zz12", WordKind.Unknown)]
    public void WordsAreDecodedByTheirCea608Bytes(string word, WordKind expected)
    {
        Assert.Equal(expected, SccSourceSyntaxHighlighting.Classify(word));
    }

    [AvaloniaFact]
    public void ALineColorsCodesAndLeavesTheTextPlain()
    {
        const string line = "00:00:01;12\t9420 9420 94ae 9452 c8e5 ecec ef80 942f";
        var spans = SourceSyntaxTokenizer.Tokenize(line, new SccSourceSyntaxHighlighting());

        SourceSyntaxSpan At(string token) => spans.Single(s => s.Start == line.IndexOf(token, StringComparison.Ordinal));

        Assert.Equal(SubRipSourceSyntaxHighlighting.TimeColor, At("00:00:01;12").Color);
        Assert.Equal(AssaSourceSyntaxHighlighting.KeywordColor, At("9420").Color);
        Assert.True(At("9420").Bold);
        Assert.Equal(AssaSourceSyntaxHighlighting.PropertyColor, At("9452").Color);
        Assert.Equal(AssaSourceSyntaxHighlighting.KeywordColor, At("942f").Color);

        var text = line.IndexOf("c8e5", StringComparison.Ordinal);
        Assert.DoesNotContain(spans, s => s.Start < text + 14 && s.Start + s.Length > text); // "c8e5 ecec ef80"
    }

    [Fact]
    public void EveryWordTheSccWriterProducesIsDecoded()
    {
        var subtitle = new Nikse.SubtitleEdit.Core.Common.Subtitle();
        subtitle.Paragraphs.Add(new Nikse.SubtitleEdit.Core.Common.Paragraph("Hello there!", 1000, 3000));
        subtitle.Paragraphs.Add(new Nikse.SubtitleEdit.Core.Common.Paragraph("<i>Two lines</i>" + Environment.NewLine + "♪ Café ½", 3500, 6000));

        var source = subtitle.ToText(new ScenaristClosedCaptions());
        var words = source.Split(['\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length == 4 && w.All(Uri.IsHexDigit))
            .ToList();

        Assert.NotEmpty(words);
        Assert.All(words, w => Assert.NotEqual(WordKind.Unknown, SccSourceSyntaxHighlighting.Classify(w)));
        Assert.Contains(words, w => SccSourceSyntaxHighlighting.Classify(w) == WordKind.MidRow); // the italics
        Assert.Contains(words, w => SccSourceSyntaxHighlighting.Classify(w) == WordKind.SpecialCharacter); // ♪ and ½
    }

    [AvaloniaFact]
    public void TheHeaderIsColored()
    {
        var spans = SourceSyntaxTokenizer.Tokenize("Scenarist_SCC V1.0", new SccSourceSyntaxHighlighting());
        Assert.Equal(AssaSourceSyntaxHighlighting.SectionColor, Assert.Single(spans).Color);
    }
}
