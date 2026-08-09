using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    private static Subtitle LoadWebVttFile(string fileName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);
        var content = File.ReadAllText(path, System.Text.Encoding.UTF8);
        return LoadWebVttSubtitle(content);
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
    public void LoadSubtitleSupportsHourlessEndTimestamp()
    {
        // WebVTT allows each timestamp independently to omit the hour part
        var vtt = "WEBVTT\r\n\r\n00:00:05.000 --> 00:10.000\r\nHello there\r\n\r\n00:11.000 --> 00:00:14.000\r\nSecond cue";
        var subtitle = LoadWebVttSubtitle(vtt);
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("Hello there", subtitle.Paragraphs[0].Text);
        Assert.Equal(5000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(10000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Second cue", subtitle.Paragraphs[1].Text);
        Assert.Equal(11000, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(14000, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
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

    // Regression coverage for https://github.com/SubtitleEdit/subtitleedit/issues/10676
    // Apple TV WebVTT files carry `X-TIMESTAMP-MAP=MPEGTS:900000,LOCAL:00:00:00.000` (HLS segment metadata)
    // and a STYLE block using class selectors like `.styledotAB9216dotitalic` for italic/bold/color.
    // The sample file's first cue is `00:00:30.030 --> 00:00:34.243`.

    [Fact]
    public void AppleTVSample_XTimestampMap_Disabled_KeepsLocalTimeCodes()
    {
        var original = Configuration.Settings.SubtitleSettings.WebVttUseXTimestampMap;
        try
        {
            Configuration.Settings.SubtitleSettings.WebVttUseXTimestampMap = false;

            var subtitle = LoadWebVttFile("sample_WebVTT_AppleTV.webvtt");

            Assert.NotEmpty(subtitle.Paragraphs);
            Assert.Equal(30_030, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.Equal(34_243, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        }
        finally
        {
            Configuration.Settings.SubtitleSettings.WebVttUseXTimestampMap = original;
        }
    }

    [Fact]
    public void AppleTVSample_XTimestampMap_Enabled_ShiftsCuesByTenSeconds()
    {
        var original = Configuration.Settings.SubtitleSettings.WebVttUseXTimestampMap;
        try
        {
            Configuration.Settings.SubtitleSettings.WebVttUseXTimestampMap = true;

            var subtitle = LoadWebVttFile("sample_WebVTT_AppleTV.webvtt");

            // MPEGTS 900000 / 90000 = 10s offset added to LOCAL 00:00:00.000.
            Assert.NotEmpty(subtitle.Paragraphs);
            Assert.Equal(40_030, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.Equal(44_243, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        }
        finally
        {
            Configuration.Settings.SubtitleSettings.WebVttUseXTimestampMap = original;
        }
    }

    [Fact]
    public void AppleTVSample_RemoveNativeFormatting_ConvertsStyleClassesToHtmlTags()
    {
        var subtitle = LoadWebVttFile("sample_WebVTT_AppleTV.webvtt");
        var webVtt = new WebVTT();

        // Cue 1: <c.styledotAB9216>...</c>  → bold + color (font-weight:bold; color:#AB9216)
        // Cue 7: <c.styledotAB9216dotitalic>...</c>  → bold + italic + color
        var cueAB9216 = subtitle.Paragraphs[0].Text;
        var cueAB9216Italic = subtitle.Paragraphs[6].Text;

        Assert.Contains("<c.styledotAB9216>", cueAB9216);
        Assert.Contains("<c.styledotAB9216dotitalic>", cueAB9216Italic);

        webVtt.RemoveNativeFormatting(subtitle, new SubRip());

        var converted1 = subtitle.Paragraphs[0].Text;
        var converted7 = subtitle.Paragraphs[6].Text;

        // After conversion no class-based `<c...>` tags should remain.
        Assert.DoesNotContain("<c.", converted1);
        Assert.DoesNotContain("<c.", converted7);

        // Cue 1 should carry bold + color from the STYLE block.
        Assert.Contains("<b>", converted1);
        Assert.Contains("</b>", converted1);
        Assert.Contains("#AB9216", converted1);

        // Cue 7 should additionally carry italic.
        Assert.Contains("<i>", converted7);
        Assert.Contains("</i>", converted7);
        Assert.Contains("<b>", converted7);
        Assert.Contains("#AB9216", converted7);
    }

    // Covers the "Save as" conversion (#11954): saving a WebVTT to SubRip runs the source format's
    // RemoveNativeFormatting, which must turn bare named color classes (no STYLE block) into
    // <font color="..."> so players that don't understand <c.color> still show the color.
    [Fact]
    public void RemoveNativeFormatting_BareColorClass_ConvertsToFontColor()
    {
        var vtt = "WEBVTT\r\n\r\n00:00:01.000 --> 00:00:04.000\r\n<c.magenta>Hello</c> world";
        var subtitle = LoadWebVttSubtitle(vtt);

        Assert.Contains("<c.magenta>", subtitle.Paragraphs[0].Text);

        new WebVTT().RemoveNativeFormatting(subtitle, new SubRip());
        var converted = subtitle.Paragraphs[0].Text;

        Assert.DoesNotContain("<c.", converted);
        Assert.Contains("<font color=\"magenta\">", converted);
        Assert.Contains("</font>", converted);
        Assert.Contains("world", converted);
    }

    [Fact]
    public void RemoveNativeFormatting_HexColorClass_ConvertsToFontColor()
    {
        var vtt = "WEBVTT\r\n\r\n00:00:01.000 --> 00:00:04.000\r\n<c.color008000>Green</c>";
        var subtitle = LoadWebVttSubtitle(vtt);

        new WebVTT().RemoveNativeFormatting(subtitle, new SubRip());
        var converted = subtitle.Paragraphs[0].Text;

        Assert.DoesNotContain("<c.", converted);
        Assert.Contains("<font color=\"#008000\">", converted);
        Assert.Contains("</font>", converted);
    }

    // yt-dlp "--write-auto-subs" output for a YouTube video: roll-up captions where each spoken
    // line first appears with per-word time codes and then again as the top line of the next cue,
    // joined by 10 ms bridge cues, all tagged "align:start position:0%".
    private const string YouTubeAutoCaptionsVtt =
        "WEBVTT\r\n" +
        "Kind: captions\r\n" +
        "Language: en\r\n" +
        "\r\n" +
        "00:00:00.000 --> 00:00:01.510 align:start position:0%\r\n" +
        " \r\n" +
        "Let<00:00:00.320><c> us</c><00:00:00.440><c> look</c><00:00:00.640><c> at</c><00:00:00.720><c> some</c><00:00:00.920><c> new</c><00:00:01.080><c> features</c><00:00:01.440><c> in</c>\r\n" +
        "\r\n" +
        "00:00:01.510 --> 00:00:01.520 align:start position:0%\r\n" +
        "Let us look at some new features in\r\n" +
        " \r\n" +
        "\r\n" +
        "00:00:01.520 --> 00:00:03.750 align:start position:0%\r\n" +
        "Let us look at some new features in\r\n" +
        "Subtitle<00:00:02.120><c> Edit</c><00:00:02.560><c> that</c><00:00:02.720><c> I</c><00:00:02.800><c> feel</c><00:00:03.200><c> you</c><00:00:03.400><c> need</c><00:00:03.640><c> to</c>\r\n" +
        "\r\n" +
        "00:00:03.750 --> 00:00:03.760 align:start position:0%\r\n" +
        "Subtitle Edit that I feel you need to\r\n" +
        " \r\n" +
        "\r\n" +
        "00:00:03.760 --> 00:00:06.190 align:start position:0%\r\n" +
        "Subtitle Edit that I feel you need to\r\n" +
        "know<00:00:03.880><c> about</c><00:00:04.400><c> now.</c>\r\n";

    [Fact]
    public void LoadSubtitleCleansUpYouTubeAutoCaptions()
    {
        var subtitle = LoadWebVttSubtitle(YouTubeAutoCaptionsVtt);

        Assert.Equal(3, subtitle.Paragraphs.Count);

        Assert.Equal("Let us look at some new features in", subtitle.Paragraphs[0].Text);
        Assert.Equal(0, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(1510, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);

        Assert.Equal("Subtitle Edit that I feel you need to", subtitle.Paragraphs[1].Text);
        Assert.Equal(1520, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(3750, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);

        Assert.Equal("know about now.", subtitle.Paragraphs[2].Text);
        Assert.Equal(3760, subtitle.Paragraphs[2].StartTime.TotalMilliseconds);
        Assert.Equal(6190, subtitle.Paragraphs[2].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void LoadSubtitleYouTubeAutoCaptionsHasNoTagsLeft()
    {
        var subtitle = LoadWebVttSubtitle(YouTubeAutoCaptionsVtt);

        foreach (var p in subtitle.Paragraphs)
        {
            Assert.DoesNotContain("{\\an", p.Text);
            Assert.DoesNotContain("<c", p.Text);
            Assert.DoesNotContain("<00:", p.Text);
        }
    }

    // A karaoke-style WebVTT has the same per-word time codes but no roll-up duplicates - it must
    // be left exactly as it is.
    [Fact]
    public void LoadSubtitleKeepsKaraokeStyleWordTimeCodes()
    {
        var vtt = "WEBVTT\r\n" +
                  "\r\n" +
                  "00:00:01.000 --> 00:00:04.000\r\n" +
                  "One<00:00:02.000><c> two</c>\r\n" +
                  "\r\n" +
                  "00:00:05.000 --> 00:00:08.000\r\n" +
                  "Three<00:00:06.000><c> four</c>\r\n" +
                  "\r\n" +
                  "00:00:09.000 --> 00:00:12.000\r\n" +
                  "Five<00:00:10.000><c> six</c>\r\n" +
                  "\r\n" +
                  "00:00:13.000 --> 00:00:16.000\r\n" +
                  "Seven<00:00:14.000><c> eight</c>\r\n";
        var subtitle = LoadWebVttSubtitle(vtt);

        Assert.Equal(4, subtitle.Paragraphs.Count);
        Assert.Equal("One<00:00:02.000><c> two</c>", subtitle.Paragraphs[0].Text);
    }

    // Cue settings with exact line%/position% values must survive a load/save round trip - before
    // the fix the export re-mapped them to the coarse WebVttCueAnX grid defaults (#10209).
    [Theory]
    [InlineData("line:25% position:25%")]
    [InlineData("line:72.69% align:left position:44.90% size:10.21%")]
    [InlineData("line:90% position:50%")]
    public void ToTextKeepsExactCuePositionSettings(string cueSettings)
    {
        var vtt = "WEBVTT\n\n00:00:01.000 --> 00:00:02.000 " + cueSettings + "\nHello";
        var subtitle = LoadWebVttSubtitle(vtt);

        var output = new WebVTT().ToText(subtitle, null);

        Assert.Contains(cueSettings, output);
    }

    // "region:" is written separately from the raw cue settings, so re-emitting the raw string
    // verbatim wrote it twice whenever the source listed it after the other settings.
    [Theory]
    [InlineData("region:r1 line:10%")]
    [InlineData("line:10% region:r1")]
    [InlineData("line:10% region:r1 align:left")]
    [InlineData("region:r1")]
    public void ToTextWritesRegionOnlyOnce(string cueSettings)
    {
        var vtt = "WEBVTT\n\nREGION\nid:r1\n\n00:00:01.000 --> 00:00:02.000 " + cueSettings + "\nHello";
        var subtitle = LoadWebVttSubtitle(vtt);

        var output = new WebVTT().ToText(subtitle, null);
        var cueLine = output.SplitToLines().First(l => l.Contains("-->"));

        Assert.Equal(1, cueLine.Split(new[] { "region:" }, StringSplitOptions.None).Length - 1);
        Assert.Contains("region:r1", cueLine);
    }

    // The other settings must still survive alongside the region.
    [Fact]
    public void ToTextKeepsPositionSettingsNextToRegion()
    {
        var vtt = "WEBVTT\n\nREGION\nid:r1\n\n00:00:01.000 --> 00:00:02.000 line:72.69% region:r1 position:44.90%\nHello";
        var subtitle = LoadWebVttSubtitle(vtt);

        var output = new WebVTT().ToText(subtitle, null);
        var cueLine = output.SplitToLines().First(l => l.Contains("-->"));

        Assert.Contains("line:72.69%", cueLine);
        Assert.Contains("position:44.90%", cueLine);
        Assert.Equal(1, cueLine.Split(new[] { "region:" }, StringSplitOptions.None).Length - 1);
    }

    [Theory]
    [InlineData("<v Joe>Hello", "Joe")]
    [InlineData("<v Joe Smith>Hello</v>", "Joe Smith")]
    [InlineData("<v Joe>Hello\r\n<v Ann>Hi", "Joe")] // first voice wins - it is the line's speaker
    [InlineData("Hello", "")]
    [InlineData("", "")]
    public void GetVoiceReadsTheFirstVoiceTag(string text, string expected)
    {
        Assert.Equal(expected, WebVTT.GetVoice(text));
    }

    [Fact]
    public void GetVoicesOfOneTextListsEachVoiceOnce()
    {
        var voices = WebVTT.GetVoices("<v Joe>Hello</v> <v Ann>Hi</v> <v Joe>Bye</v>");

        Assert.Equal(new List<string> { "Joe", "Ann" }, voices);
    }
}
