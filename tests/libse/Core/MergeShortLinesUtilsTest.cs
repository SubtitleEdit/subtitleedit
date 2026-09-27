using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

public class MergeShortLinesUtilsTest
{
    [Fact]
    public void ThreeShortLines()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("How", 0, 200));
        subtitle.Paragraphs.Add(new Paragraph("are", 200, 400));
        subtitle.Paragraphs.Add(new Paragraph("you?", 400, 600));
        var mergedSubtitle = MergeShortLinesUtils.MergeShortLinesInSubtitle(subtitle, 500, 80, true);

        Assert.Single(mergedSubtitle.Paragraphs);
        Assert.Equal("How are you?", mergedSubtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void ThreeShortLinesNoMergeDueToLength()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("How", 0, 200));
        subtitle.Paragraphs.Add(new Paragraph("are", 200, 400));
        subtitle.Paragraphs.Add(new Paragraph("you?", 400, 600));
        var mergedSubtitle = MergeShortLinesUtils.MergeShortLinesInSubtitle(subtitle, 500, 2, true);

        Assert.Equal(3, mergedSubtitle.Paragraphs.Count);
    }

    [Fact]
    public void ThreeShortLinesNoMergeDueToGap()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("How", 0, 200));
        subtitle.Paragraphs.Add(new Paragraph("are", 2000, 2400));
        subtitle.Paragraphs.Add(new Paragraph("you?", 4400, 4600));
        var mergedSubtitle = MergeShortLinesUtils.MergeShortLinesInSubtitle(subtitle, 500, 80, true);

        Assert.Equal(3, mergedSubtitle.Paragraphs.Count);
    }

    [Theory]
    [InlineData("Ich weiß")] // not in the configured alphabet
    [InlineData("Das ist groß")]
    [InlineData("Příliš")]
    [InlineData("זה לא")]
    [InlineData("ไม่รู้")]
    [InlineData("Wir treffen uns um 10")]
    [InlineData("See you in room 2B at 3")]
    [InlineData("午後３")] // fullwidth digit
    [InlineData("Where are you,")]
    public void QualifiesForMergeOnlyContinuationLinesAnyLetter(string text)
    {
        var p = new Paragraph(text, 0, 1000);
        var next = new Paragraph("next line.", 1100, 2000);

        Assert.True(Utilities.QualifiesForMerge(p, next, 500, 200, true));
    }

    [Theory]
    [InlineData("Das ist groß.")]
    [InlineData("Wirklich?")]
    [InlineData("Er sagte:")]
    public void QualifiesForMergeOnlyContinuationLinesNotContinuation(string text)
    {
        var p = new Paragraph(text, 0, 1000);
        var next = new Paragraph("next line.", 1100, 2000);

        Assert.False(Utilities.QualifiesForMerge(p, next, 500, 200, true));
    }
}
