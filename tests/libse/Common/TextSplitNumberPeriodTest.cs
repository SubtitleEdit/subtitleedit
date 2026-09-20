using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

public class TextSplitNumberPeriodTest
{
    // Issue #14230: the "make every paragraph end on a whole sentence" pass took the last
    // '.' in the text as a sentence ending, so a decimal separator or a clock time split the
    // number in two ("04." + "00 uur").
    [Fact]
    public void SplitMultiDoesNotSplitInsideAClockTime()
    {
        var parts = TextSplit.SplitMulti(
            "je dat Zak jou en Carl Wilsher rond 04.00 uur samen terug naar Chislehurst heeft gereden.",
            2,
            "nl");

        Assert.Equal(2, parts.Count);
        Assert.DoesNotContain(parts, p => p.EndsWith("04."));
        Assert.DoesNotContain(parts, p => p.StartsWith("00 uur"));
        Assert.Contains("04.00 uur", string.Join(" ", parts));
    }

    [Fact]
    public void SplitMultiDoesNotSplitInsideADecimalNumber()
    {
        var parts = TextSplit.SplitMulti(
            "Hij zei dat het huis ongeveer 1.500 euro per maand kost in deze buurt.",
            2,
            "nl");

        Assert.Equal(2, parts.Count);
        Assert.Contains("1.500 euro", string.Join(" ", parts));
    }

    // The real sentence ending must still move: this is what the pass exists for.
    [Fact]
    public void TryForWholeSentencesStillMovesARealSentenceEnding()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("That is all. And then", 0, 3000));
        subtitle.Paragraphs.Add(new Paragraph("we went home", 3000, 6000));

        var result = TextSplit.TryForWholeSentences(subtitle, "en", 43);

        Assert.Equal("That is all.", result.Paragraphs[0].Text);
        Assert.Equal("And then we went home", result.Paragraphs[1].Text);
    }
}
