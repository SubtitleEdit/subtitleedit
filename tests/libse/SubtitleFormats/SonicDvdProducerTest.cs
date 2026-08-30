using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class SonicDvdProducerTest
{
    // layout straight from the DVD Producer user guide
    private const string Sample = "1 00:01:00:00        00:01:19:00 Subtitle line 1\r\n" +
                                  "                                 Subtitle line 2\r\n" +
                                  "2 00:01:20:00        00:01:29:00 Another subtitle\r\n" +
                                  "3 00:01:30:00        00:01:39:00 Third subtitle\r\n";

    [Fact]
    public void LoadSubtitleReadsColumnsAndContinuationLines()
    {
        var format = new SonicDvdProducer();
        var lines = Sample.SplitToLines();
        var subtitle = new Subtitle();

        Assert.True(format.IsMine(lines, "subs.txt"));
        format.LoadSubtitle(subtitle, lines, "subs.txt");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("Subtitle line 1" + Environment.NewLine + "Subtitle line 2", subtitle.Paragraphs[0].Text);
        Assert.Equal(60000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(79000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Another subtitle", subtitle.Paragraphs[1].Text);
        Assert.Equal(99000, subtitle.Paragraphs[2].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void SingleSpacedFilesAreLeftToAdobeEncore()
    {
        // "Adobe Encore w. line#" is the same layout without the column padding - taking those
        // files here would rename the format under everyone who uses it
        const string adobeEncore = "1 00:01:00:00 00:01:19:00 Subtitle line 1\r\n" +
                                   "Subtitle line 2\r\n" +
                                   "2 00:01:20:00 00:01:29:00 Another subtitle\r\n";

        Assert.False(new SonicDvdProducer().IsMine(adobeEncore.SplitToLines(), "subs.txt"));
        Assert.True(new AdobeEncoreWithLineNumbers().IsMine(adobeEncore.SplitToLines(), "subs.txt"));
    }

    [Fact]
    public void SonicWinsDetectionForColumnPaddedFiles()
    {
        var lines = Sample.SplitToLines();
        foreach (var format in SubtitleFormat.AllSubtitleFormats)
        {
            if (format.IsTextBased && format.IsMine(lines, "subs.txt"))
            {
                Assert.Equal(new SonicDvdProducer().Name, format.Name);
                break;
            }
        }
    }

    [Fact]
    public void RoundTrip()
    {
        var format = new SonicDvdProducer();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("First line" + Environment.NewLine + "second line", 60000, 79000));
        subtitle.Paragraphs.Add(new Paragraph("Another one", 80000, 89000));

        var text = format.ToText(subtitle, "title");
        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, text.SplitToLines(), "subs.txt");

        Assert.True(format.IsMine(text.SplitToLines(), "subs.txt"));
        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("First line" + Environment.NewLine + "second line", loaded.Paragraphs[0].Text);
        Assert.Equal(60000, loaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(89000, loaded.Paragraphs[1].EndTime.TotalMilliseconds);
    }
}
