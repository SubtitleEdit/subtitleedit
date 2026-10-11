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

    private static string ReadStyledParagraph(string divAttributes, string paragraph)
    {
        var xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                  "<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\">\r\n" +
                  "  <head><styling>\r\n" +
                  "    <style xml:id=\"it\" tts:fontStyle=\"italic\"/>\r\n" +
                  "    <style xml:id=\"bo\" tts:fontWeight=\"bold\"/>\r\n" +
                  "    <style xml:id=\"un\" tts:textDecoration=\"underline\"/>\r\n" +
                  "  </styling></head>\r\n" +
                  "  <body>\r\n" +
                  $"    <div{divAttributes}>\r\n" +
                  $"      <p begin=\"00:00:01.000\" end=\"00:00:02.000\">{paragraph}</p>\r\n" +
                  "    </div>\r\n" +
                  "  </body>\r\n" +
                  "</tt>";
        var subtitle = LoadTimedTextSubtitle(xml);
        Assert.Single(subtitle.Paragraphs);
        return subtitle.Paragraphs[0].Text;
    }

    [Theory]
    [InlineData("tts:fontStyle=\"italic\"", "tts:fontStyle=\"normal\"", "i")]
    [InlineData("tts:fontWeight=\"bold\"", "tts:fontWeight=\"normal\"", "b")]
    [InlineData("tts:textDecoration=\"underline\"", "tts:textDecoration=\"none\"", "u")]
    public void ReadSpanTurningStyleOffClosesTag(string on, string off, string tag)
    {
        var xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                  "<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\">\r\n" +
                  "  <body><div>\r\n" +
                  $"      <p begin=\"00:00:01.000\" end=\"00:00:02.000\" {on}>a <span {off}>b</span> c</p>\r\n" +
                  "  </div></body>\r\n" +
                  "</tt>";
        var subtitle = LoadTimedTextSubtitle(xml);

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal($"<{tag}>a </{tag}>b<{tag}> c</{tag}>", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void ReadSpanTurningItalicOffKeepsBold()
    {
        var text = ReadStyledParagraph(string.Empty, "<span tts:fontStyle=\"italic\" tts:fontWeight=\"bold\">a <span tts:fontStyle=\"normal\">b</span></span>");

        Assert.Equal("<i><b>a </b></i><b>b</b>", text);
    }

    [Theory]
    [InlineData("it", "i")]
    [InlineData("bo", "b")]
    [InlineData("un", "u")]
    public void ReadDivStyleIsInheritedAsTag(string style, string tag)
    {
        Assert.Equal($"<{tag}>x y</{tag}>", ReadStyledParagraph($" style=\"{style}\"", $"<span style=\"{style}\">x</span> y"));
        Assert.Equal($"<{tag}>z</{tag}>", ReadStyledParagraph($" style=\"{style}\"", "z"));
    }

    [Fact]
    public void ReadDivItalicSpanNormal()
    {
        Assert.Equal("<i>x </i>y", ReadStyledParagraph(" style=\"it\"", "x <span tts:fontStyle=\"normal\">y</span>"));
    }

    [Fact]
    public void ReadSpanTurningItalicOffInsideColorReopensFont()
    {
        var text = ReadStyledParagraph(string.Empty, "<span tts:fontStyle=\"italic\" tts:color=\"yellow\">a <span tts:fontStyle=\"normal\">b</span> c</span>");

        Assert.Equal("<i><font color=\"yellow\">a </font></i><font color=\"yellow\">b</font><i><font color=\"yellow\"> c</font></i>", text);
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
