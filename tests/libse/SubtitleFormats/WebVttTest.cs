using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class WebVttTest
{
    private static Subtitle LoadWebVttSubtitle(string vttContent)
    {
        var subtitle = new Subtitle();
        var format = new WebVTT();
        var lines = new List<string>(vttContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
        format.LoadSubtitle(subtitle, lines, null);
        return subtitle;
    }

    [Fact]
    public void LoadSubtitleMergesCuesWithIdenticalTimeCodes()
    {
        var vtt = "WEBVTT\r\n\r\n00:00:01.000 --> 00:00:04.000\r\nHello\r\n\r\n00:00:01.000 --> 00:00:04.000\r\nWorld";
        var subtitle = LoadWebVttSubtitle(vtt);
        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Hello" + Environment.NewLine + "World", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void LoadSubtitleMergesThreeCuesWithIdenticalTimeCodes()
    {
        var vtt = "WEBVTT\r\n\r\n00:00:01.000 --> 00:00:04.000\r\nLine1\r\n\r\n00:00:01.000 --> 00:00:04.000\r\nLine2\r\n\r\n00:00:01.000 --> 00:00:04.000\r\nLine3";
        var subtitle = LoadWebVttSubtitle(vtt);
        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Line1" + Environment.NewLine + "Line2" + Environment.NewLine + "Line3", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void LoadSubtitleDoesNotMergeCuesWithDifferentTimeCodes()
    {
        var vtt = "WEBVTT\r\n\r\n00:00:01.000 --> 00:00:04.000\r\nHello\r\n\r\n00:00:05.000 --> 00:00:08.000\r\nWorld";
        var subtitle = LoadWebVttSubtitle(vtt);
        Assert.Equal(2, subtitle.Paragraphs.Count);
    }

    [Fact]
    public void LoadSubtitleDoesNotMergeCuesWithSameTimeCodesButDifferentRegions()
    {
        var vtt = "WEBVTT\r\n\r\n00:00:01.000 --> 00:00:04.000 region:top\r\nHello\r\n\r\n00:00:01.000 --> 00:00:04.000 region:bottom\r\nWorld";
        var subtitle = LoadWebVttSubtitle(vtt);
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("Hello", subtitle.Paragraphs[0].Text);
        Assert.Equal("World", subtitle.Paragraphs[1].Text);
    }
}
