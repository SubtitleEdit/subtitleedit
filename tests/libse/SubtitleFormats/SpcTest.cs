using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class SpcTest
{
    private const string Sample = "00:00:05:00&00:00:08:00#come here\r\n" +
                                  "00:00:09:00&00:00:12:00#Line one|Line two\r\n" +
                                  "00:00:13:00&00:00:16:00#Last one\r\n";

    [Fact]
    public void LoadSubtitleReadsTimeCodesAndLineBreaks()
    {
        var format = new Spc();
        var subtitle = new Subtitle();

        format.LoadSubtitle(subtitle, Sample.SplitToLines(), "subs.spc");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("come here", subtitle.Paragraphs[0].Text);
        Assert.Equal(5000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(8000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Line one" + Environment.NewLine + "Line two", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void SpcWinsDetectionOverTmPlayer()
    {
        // TMPlayer reads "00:00:05:00&..." as "h:mm:ss:text" and keeps "00&00:00:08:00#come here"
        // as the subtitle text, so SPC has to be checked first
        var lines = Sample.SplitToLines();
        foreach (var format in SubtitleFormat.AllSubtitleFormats)
        {
            if (format.IsTextBased && format.IsMine(lines, "subs.spc"))
            {
                Assert.Equal(new Spc().Name, format.Name);
                break;
            }
        }
    }

    [Fact]
    public void RoundTrip()
    {
        var format = new Spc();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("First line" + Environment.NewLine + "second line", 5000, 8000));
        subtitle.Paragraphs.Add(new Paragraph("Another one", 9000, 12000));

        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, format.ToText(subtitle, "title").SplitToLines(), "subs.spc");

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("First line" + Environment.NewLine + "second line", loaded.Paragraphs[0].Text);
        Assert.Equal(5000, loaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(12000, loaded.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void SpruceStlIsNotClaimed()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", 5000, 8000));
        subtitle.Paragraphs.Add(new Paragraph("There", 9000, 12000));

        Assert.False(new Spc().IsMine(new Spruce().ToText(subtitle, "title").SplitToLines(), "subs.stl"));
    }
}
