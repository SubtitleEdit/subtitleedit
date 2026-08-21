using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class RealTimeTest
{
    [Fact]
    public void LoadSubtitleReadsRealWorldBeginOnlyCues()
    {
        // real RealText files (RealPlayer captions) use lowercase <time> tags with only a
        // begin= attribute; the text runs until the next <time> tag replaces it
        var format = new RealTime();
        var subtitle = new Subtitle();
        var lines = new List<string>
        {
            "<window duration=\"30\" bgcolor=\"yellow\">",
            "Mary had a little lamb,",
            "<br/><time begin=\"3\"/>little lamb,",
            "<br/><time begin=\"6\"/>little lamb.",
            "<br/><time begin=\"9\"/><clear/>Mary had a little lamb,",
            "</window>",
        };

        Assert.True(format.IsMine(lines, null));
        format.LoadSubtitle(subtitle, lines, null);

        Assert.Equal(4, subtitle.Paragraphs.Count);
        Assert.Equal("Mary had a little lamb,", subtitle.Paragraphs[0].Text);
        Assert.Equal(0, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal("little lamb,", subtitle.Paragraphs[1].Text);
        Assert.Equal(3000, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(6000, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.Equal(9000, subtitle.Paragraphs[3].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void LoadSubtitleReadsExplicitEndAndMultiLineText()
    {
        var format = new RealTime();
        var subtitle = new Subtitle();
        var lines = new List<string>
        {
            "<window type=\"generic\" duration=\"01:01:20.17\">",
            "<font face=\"Verdana\"><center>",
            " <time begin=\"00:00:05.44\" end=\"00:00:08.5\"/><clear/>FIRST LINE.<br/>",
            "SECOND LINE.<br/>",
            " <time begin=\"00:00:09.98\"/><clear/>NEXT CUE.<br/>",
            "</center></font>",
            "</window>",
        };

        format.LoadSubtitle(subtitle, lines, null);

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("FIRST LINE." + Environment.NewLine + "SECOND LINE.", subtitle.Paragraphs[0].Text);
        Assert.Equal(5440, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(8500, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("NEXT CUE.", subtitle.Paragraphs[1].Text);
        Assert.Equal(9980, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void LoadSubtitleReadsStaticWindowWithoutTimeTags()
    {
        var format = new RealTime();
        var subtitle = new Subtitle();
        var lines = new List<string>
        {
            "<window",
            "\ttype=\"generic\"",
            "\tduration=\"5.760\"",
            ">",
            "<font size=\"+2\" color=\"white\">",
            "<center>",
            "Video Clip Demo",
            "</center>",
            "</font>",
            "</window>",
        };

        format.LoadSubtitle(subtitle, lines, null);

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Video Clip Demo", subtitle.Paragraphs[0].Text);
        Assert.Equal(0, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(5760, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void RoundTripKeepsTextAndTimes()
    {
        var format = new RealTime();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello world", 3400, 5800));
        subtitle.Paragraphs.Add(new Paragraph("Second cue", 6000, 8000));

        var text = format.ToText(subtitle, "title");
        var reloaded = new Subtitle();
        format.LoadSubtitle(reloaded, new List<string>(text.SplitToLines()), null);

        Assert.Equal(2, reloaded.Paragraphs.Count);
        Assert.Equal("Hello world", reloaded.Paragraphs[0].Text);
        Assert.Equal(3400, reloaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(5800, reloaded.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Second cue", reloaded.Paragraphs[1].Text);
    }

    [Theory]
    [InlineData("3", 3000)]
    [InlineData("3.5", 3500)]
    [InlineData(".5", 500)]
    [InlineData("1:20", 80000)]
    [InlineData("1:20s", 80000)]
    [InlineData("0:03:24.8", 204800)]
    public void LoadSubtitleParsesShortTimestampForms(string timestamp, int expectedMilliseconds)
    {
        var format = new RealTime();
        var subtitle = new Subtitle();
        var lines = new List<string>
        {
            "<window duration=\"200:00\">",
            $"<time begin=\"{timestamp}\"/>Some text",
            "</window>",
        };

        format.LoadSubtitle(subtitle, lines, null);

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal(expectedMilliseconds, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
    }
}
