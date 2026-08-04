using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class TimedText10Test
{
    private static Subtitle LoadTimedTextSubtitle(string xml)
    {
        var subtitle = new Subtitle();
        var format = new TimedText10();
        var lines = new List<string>(xml.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
        format.LoadSubtitle(subtitle, lines, null);
        return subtitle;
    }

    private static string MakeTtml(string begin, string end)
    {
        return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
               "<tt xmlns=\"http://www.w3.org/ns/ttml\">\r\n" +
               "  <body>\r\n" +
               "    <div>\r\n" +
               $"      <p begin=\"{begin}\" end=\"{end}\">Hello</p>\r\n" +
               "    </div>\r\n" +
               "  </body>\r\n" +
               "</tt>";
    }

    [Fact]
    public void GetTimeCodeOneDigitFractionIsFractionOfSecond()
    {
        // ".5" is half a second per the TTML spec - not 5 frames, not 5 ms
        var timeCode = TimedText10.GetTimeCode("00:00:01.5", false);
        Assert.Equal(1500, timeCode.TotalMilliseconds);
    }

    [Fact]
    public void GetTimeCodeFourDigitFractionIsFractionOfSecond()
    {
        var timeCode = TimedText10.GetTimeCode("00:00:05.9463", false);
        Assert.Equal(5946, timeCode.TotalMilliseconds);
    }

    [Fact]
    public void GetTimeCodeThreeDigitFractionIsMilliseconds()
    {
        var timeCode = TimedText10.GetTimeCode("00:01:39.946", false);
        Assert.Equal(99946, timeCode.TotalMilliseconds);
    }

    [Fact]
    public void GetTimeCodeTwoDigitFractionIsFractionOfSecond()
    {
        var timeCode = TimedText10.GetTimeCode("00:00:08.12", false);
        Assert.Equal(8120, timeCode.TotalMilliseconds);
    }

    [Fact]
    public void GetTimeCodeColonSeparatedLastPartIsMilliseconds()
    {
        // legacy/malformed files use a colon before the milliseconds
        var timeCode = TimedText10.GetTimeCode("00:00:08:123", false);
        Assert.Equal(8123, timeCode.TotalMilliseconds);
    }

    [Fact]
    public void LoadSubtitleParsesOneDigitFractionAsFractionOfSecond()
    {
        var subtitle = LoadTimedTextSubtitle(MakeTtml("00:00:01.5", "00:00:03.5"));

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3500, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void LoadSubtitleParsesFourDigitFractionAsFractionOfSecond()
    {
        var subtitle = LoadTimedTextSubtitle(MakeTtml("00:00:05.9463", "00:00:08.1000"));

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal(5946, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(8100, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
    }
}
