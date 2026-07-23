using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.IO;
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

    [Fact]
    public void LoadAndSave_PreservesCueSettingsThatDoNotMatchAnPreset()
    {
        var vtt = "WEBVTT\r\n\r\n00:00:21.731 --> 00:00:23.356 position:3% line:5% align:left\r\nHello";
        var subtitle = LoadWebVttSubtitle(vtt);

        Assert.Equal("position:3% line:5% align:left", subtitle.Paragraphs[0].Style);
        Assert.Equal("Hello", subtitle.Paragraphs[0].Text);

        var saved = new WebVTT().ToText(subtitle, null);

        Assert.Contains("00:00:21.731 --> 00:00:23.356 position:3% line:5% align:left", saved);
    }

    [Fact]
    public void LoadSubtitle_ConvertsCueSettingsMatchingAnPresetToAssTag()
    {
        var vtt = "WEBVTT\r\n\r\n00:00:21.731 --> 00:00:23.356 position:20% line:20%\r\nHello";
        var subtitle = LoadWebVttSubtitle(vtt);

        Assert.Equal("{\\an7}Hello", subtitle.Paragraphs[0].Text);
        Assert.Equal(string.Empty, subtitle.Paragraphs[0].Style);
    }

    [Fact]
    public void Save_RemovingLoadedAnTagDoesNotKeepItsOriginalCueSettings()
    {
        var vtt = "WEBVTT\r\n\r\n00:00:21.731 --> 00:00:23.356 position:20% line:20%\r\nHello";
        var subtitle = LoadWebVttSubtitle(vtt);
        subtitle.Paragraphs[0].Text = "Hello";

        var saved = new WebVTT().ToText(subtitle, null);

        Assert.DoesNotContain("position:20% line:20%", saved);
    }

    [Fact]
    public void FileWithLineNumber_LoadAndSave_PreservesCueSettingsThatDoNotMatchAnPreset()
    {
        var vtt = "WEBVTT FILE\r\n\r\n1\r\n00:00.000 --> 00:02.000 position:3% line:5% align:left\r\nHello";
        var subtitle = new Subtitle();
        var format = new WebVTTFileWithLineNumber();
        format.LoadSubtitle(subtitle, new List<string>(vtt.Split(new[] { "\r\n" }, StringSplitOptions.None)), null);

        Assert.Equal("position:3% line:5% align:left", subtitle.Paragraphs[0].Extra);

        var saved = format.ToText(subtitle, null);

        Assert.Contains("00:00.000 --> 00:02.000 position:3% line:5% align:left", saved);
    }

    [Fact]
    public void Save_AnTagOverridesExistingCueSettings()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph
        {
            StartTime = new TimeCode(0, 0, 21, 731),
            EndTime = new TimeCode(0, 0, 23, 356),
            Style = "position:10% line:3%",
            Text = "{\\an7}Hello"
        });

        var saved = new WebVTT().ToText(subtitle, null);

        Assert.Contains("00:00:21.731 --> 00:00:23.356 position:20% line:20%", saved);
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
}
