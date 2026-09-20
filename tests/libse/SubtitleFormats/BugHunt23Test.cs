using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Guard tests for the 2026-09-02 random-file bug hunt: writers whose own readers could not
/// round-trip them (wrong time-code arithmetic, invalid JSON, missing fields), a Cyrillic letter
/// in a TTML region id, and a TTML-to-SSA colour written in the wrong byte order.
/// </summary>
public class BugHunt23Test
{
    private static Subtitle Make(params (string text, double start, double end)[] cues)
    {
        var subtitle = new Subtitle();
        foreach (var (text, start, end) in cues)
        {
            subtitle.Paragraphs.Add(new Paragraph(text, start, end));
        }

        return subtitle;
    }

    private static Subtitle RoundTrip(SubtitleFormat format, Subtitle subtitle)
    {
        var text = format.ToText(subtitle, "title");
        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, text.SplitToLines(), null);
        return loaded;
    }

    [Fact]
    public void FinalCutProXml14Text_FractionAboveHalf_IsNotRoundedToWholeSecond()
    {
        // Convert.ToInt64 rounds to nearest, so 1.6 s passed the "nearly whole" test and was
        // written as "2s" - every cue with a fraction >= .5 s shifted by up to half a second
        var text = new FinalCutProXml14Text().ToText(Make(("Hello", 1600, 3100)), "t");
        var title = Regex.Match(text, "<title [^>]*>").Value;

        Assert.DoesNotContain("offset=\"2s\"", title);
        Assert.DoesNotContain("start=\"2s\"", title);
        Assert.DoesNotContain("duration=\"2s\"", title);
        // the exact fraction depends on the current frame-rate setting; the fractional form is what matters
        Assert.Matches("offset=\"\\d+/\\d+s\"", title);
    }

    [Fact]
    public void FinalCutProXml14Text_WholeSecond_StillWrittenAsWholeSecond()
    {
        var text = new FinalCutProXml14Text().ToText(Make(("Hello", 2000, 4000)), "t");
        var title = Regex.Match(text, "<title [^>]*>").Value;

        Assert.Contains("offset=\"2s\"", title);
        Assert.Contains("duration=\"2s\"", title);
    }

    [Fact]
    public void TimedText10_UntimedParagraph_StartsWherePreviousEnded()
    {
        // fallback used EndTime.Milliseconds (the 0-999 component) so the cue jumped to the file start
        var xml = "<?xml version=\"1.0\"?><tt xmlns=\"http://www.w3.org/ns/ttml\"><body><div>" +
                  "<p begin=\"00:01:00.000\" end=\"00:01:05.500\">a</p><p>b</p></div></body></tt>";
        var subtitle = new Subtitle();
        new TimedText10().LoadSubtitle(subtitle, new List<string> { xml }, null);

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(65500, subtitle.Paragraphs[1].StartTime.TotalMilliseconds, 0.1);
        Assert.Equal(68500, subtitle.Paragraphs[1].EndTime.TotalMilliseconds, 0.1);
    }

    [Fact]
    public void TimedText10_TwoDigitFraction_CarriesIntoSecond()
    {
        var old = Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormat;
        Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormat = "hh:mm:ss.ms-two-digits";
        try
        {
            // 1.996 s used to be written as "00:00:01.100" (three digits, no carry) and reload 0.9 s early
            var text = new TimedText10().ToText(Make(("World", 1996, 7000)), "t");
            Assert.Contains("begin=\"00:00:02.00\"", text);
            Assert.DoesNotContain("00:00:01.100", text);

            var loaded = new Subtitle();
            new TimedText10().LoadSubtitle(loaded, text.SplitToLines(), null);
            Assert.Equal(2000, loaded.Paragraphs[0].StartTime.TotalMilliseconds, 0.1);
        }
        finally
        {
            Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormat = old;
        }
    }

    [Fact]
    public void TimedText_TwoDigitFraction_AlwaysWritesTwoDigitsWithCarry()
    {
        var old = Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource;
        Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = "hh:mm:ss.ms-two-digits";
        try
        {
            var text = new TimedText().ToText(Make(("a", 1050, 1996)), "t");
            Assert.Contains("begin=\"00:00:01.05\"", text);
            Assert.Contains("end=\"00:00:02.00\"", text);
        }
        finally
        {
            Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = old;
        }
    }

    [Fact]
    public void TimedText10_An5_RegionIdIsAscii()
    {
        // the literal contained a Cyrillic capital Es, so the <p> referenced a region that was never defined
        var text = new TimedText10().ToText(Make(("{\\an5}mid", 1000, 2000)), "t");
        var region = Regex.Match(text, "<p [^>]*region=\"([^\"]*)\"").Groups[1].Value;

        Assert.Equal("centerCenter", region);
        Assert.All(region, c => Assert.True(c < 128, "non-ASCII char in region id"));
        Assert.Contains("xml:id=\"centerCenter\"", text);

        var loaded = new Subtitle();
        new TimedText10().LoadSubtitle(loaded, text.SplitToLines(), null);
        Assert.StartsWith("{\\an5}", loaded.Paragraphs[0].Text);
    }

    [Fact]
    public void SubStationAlpha_StyleFromTimedText_KeepsRedRed()
    {
        // ToArgb() wrote ARGB into SSA's BGR colour field, so a red TTML style reloaded as blue
        var ttml = "<?xml version=\"1.0\"?><tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\">" +
                   "<head><styling><style xml:id=\"s1\" tts:color=\"#ff0000\" tts:fontFamily=\"Arial\" tts:fontSize=\"20\"/></styling></head>" +
                   "<body><div><p begin=\"00:00:01.000\" end=\"00:00:02.000\" style=\"s1\">a</p></div></body></tt>";
        var subtitle = new Subtitle();
        new TimedText10().LoadSubtitle(subtitle, new List<string> { ttml }, null);

        var ssa = new SubStationAlpha().ToText(subtitle, "t");
        var style = AdvancedSubStationAlpha.GetSsaStylesFromHeader(ssa).Single(s => s.Name == "s1");

        Assert.Equal(255, style.Primary.Red);
        Assert.Equal(0, style.Primary.Green);
        Assert.Equal(0, style.Primary.Blue);
    }

    [Fact]
    public void UnknownSubtitle15_RoundTrip_KeepsHundredths()
    {
        // the reader multiplied the hundredths field by 100, so 4.67 s loaded as 10.8 s
        var loaded = RoundTrip(new UnknownSubtitle15(), Make(("x", 4670, 7500)));

        Assert.Single(loaded.Paragraphs);
        Assert.Equal(4670, loaded.Paragraphs[0].StartTime.TotalMilliseconds, 0.1);
        Assert.Equal(7500, loaded.Paragraphs[0].EndTime.TotalMilliseconds, 0.1);
    }

    [Fact]
    public void UnknownSubtitle15_995Ms_CarriesIntoSecond()
    {
        var text = new UnknownSubtitle15().ToText(Make(("x", 995, 2000)), "t");
        Assert.Contains("00:00:01:00", text);
        Assert.DoesNotContain("00:00:00:100", text);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("da-DK")]
    public void UnknownSubtitle6_RoundTrip_KeepsEveryCue(string cultureName)
    {
        // the writer emitted "612,3" / "612.3" for 6123 ms, which the reader's integer regex rejected -
        // the cue's text was then glued onto the previous cue
        var oldCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
        try
        {
            var loaded = RoundTrip(new UnknownSubtitle6(), Make(("a", 2000, 4000), ("b", 6123, 9456), ("c", 12000, 15000)));

            Assert.Equal(3, loaded.Paragraphs.Count);
            Assert.Equal("b", loaded.Paragraphs[1].Text);
            Assert.Equal(6120, loaded.Paragraphs[1].StartTime.TotalMilliseconds, 0.1);
            Assert.Equal(9460, loaded.Paragraphs[1].EndTime.TotalMilliseconds, 0.1);
        }
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Fact]
    public void UnknownSubtitle59_RoundTrip_KeepsEndTime()
    {
        // the writer never emitted the end time, so every reloaded cue had EndTime 0
        var loaded = RoundTrip(new UnknownSubtitle59(), Make(("Would you like some?", 372000, 373500)));

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("Would you like some?", loaded.Paragraphs[0].Text);
        Assert.Equal(372000, loaded.Paragraphs[0].StartTime.TotalMilliseconds, 0.1);
        Assert.Equal(374000, loaded.Paragraphs[0].EndTime.TotalMilliseconds, 0.1);
    }

    [Fact]
    public void PodcastIndexer_WritesValidJson()
    {
        // missing commas after "version" and "speaker" - only SE's own lenient parser could read it
        var subtitle = Make(("Hello \"there\"", 1000, 2000), ("World", 3000, 4000));
        subtitle.Paragraphs[0].Actor = "Bob \"the\" Host";

        var text = new PodcastIndexer().ToText(subtitle, "t");

        using var doc = JsonDocument.Parse(text);
        var segments = doc.RootElement.GetProperty("segments");
        Assert.Equal(2, segments.GetArrayLength());
        Assert.Equal("Bob \"the\" Host", segments[0].GetProperty("speaker").GetString());
        Assert.Equal("Hello \"there\"", segments[0].GetProperty("body").GetString());

        var loaded = RoundTrip(new PodcastIndexer(), subtitle);
        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("Hello \"there\"", loaded.Paragraphs[0].Text);
    }

    [Fact]
    public void JsonTypeOnlyLoad5_TopAlignment_IsOneTagPerParagraph()
    {
        // "{\an8}" was appended before every line and then run through DecodeJsonText, which ate the backslash
        var json = "{\"Paragraphs\":[{\"Start\":1.0,\"End\":2.0,\"VAlign\":\"Top\",\"Lines\":[{\"Text\":\"line one\"},{\"Text\":\"line two\"}]}," +
                   "{\"Start\":3.0,\"End\":4.0,\"VAlign\":\"Bottom\",\"Lines\":[{\"Text\":\"plain\"}]}]}";
        var subtitle = new Subtitle();
        new JsonTypeOnlyLoad5().LoadSubtitle(subtitle, new List<string> { json }, null);

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("{\\an8}line one" + Environment.NewLine + "line two", subtitle.Paragraphs[0].Text);
        Assert.Equal("plain", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void UnknownSubtitle56_NumbersCuesSequentially()
    {
        var text = new UnknownSubtitle56().ToText(Make(("a", 1000, 2000), ("b", 3000, 4000), ("c", 5000, 6000)), "t");
        var numbers = text.SplitToLines().Where(l => l.Contains('\t')).Select(l => l.Split('\t')[0]).ToList();

        Assert.Equal(new[] { "0001", "0002", "0003" }, numbers);
    }

    [Fact]
    public void UnknownSubtitle10_QuotesInText_RoundTrip()
    {
        // the text was pasted into the JSON string unescaped, so a quote produced invalid JSON
        // and the reader stripped the closing quote from the text
        var subtitle = Make(("He said \"hi\"" + Environment.NewLine + "and left \\ fast", 1000, 2000));
        var text = new UnknownSubtitle10().ToText(subtitle, "t");

        using var doc = JsonDocument.Parse(text);
        Assert.Equal("He said \\\"hi\\\" <br> and left \\\\ fast".Replace("\\\"", "\"").Replace("\\\\", "\\"),
            doc.RootElement.GetProperty("subtitles")[0].GetProperty("content").GetString());

        var loaded = new Subtitle();
        new UnknownSubtitle10().LoadSubtitle(loaded, text.SplitToLines(), null);
        Assert.Single(loaded.Paragraphs);
        Assert.Equal(subtitle.Paragraphs[0].Text, loaded.Paragraphs[0].Text);
    }

    [Fact]
    public void UnknownSubtitle94_DoubledTab_LoadsInsteadOfThrowing()
    {
        // the regex accepts "\t+" between the time codes, but the reader split on every tab and
        // handed the empty column to double.Parse
        var subtitle = new Subtitle();
        var format = new UnknownSubtitle94();
        var lines = new List<string> { "0:13\t\t0:14\tHello", "0:15\t0:16\tWorld" };

        Assert.True(format.IsMine(lines, null));
        format.LoadSubtitle(subtitle, lines, null);

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("Hello", subtitle.Paragraphs[0].Text);
        Assert.Equal(13000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 0.1);
        Assert.Equal(14000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 0.1);
    }
}
