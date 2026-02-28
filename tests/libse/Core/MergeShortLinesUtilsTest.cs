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
}
