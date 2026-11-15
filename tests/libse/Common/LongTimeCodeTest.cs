using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Common;

// Time codes past 99:59:59,999 (#16937): videos with huge start timestamps need subtitles shifted
// by hundreds of hours, and the text formats players read (SubRip, WebVTT, TTML, ASSA/SSA) all
// accept 3+ digit hours.
public class LongTimeCodeTest
{
    // 123:04:05,678
    private const double LongStart = ((123 * 60 + 4) * 60 + 5) * 1000.0 + 678;
    private const double LongEnd = LongStart + 2000;

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", LongStart, LongEnd));
        return subtitle;
    }

    private static Subtitle RoundTrip(SubtitleFormat format)
    {
        var text = format.ToText(MakeSubtitle(), "test");
        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, text.SplitToLines(), null);
        return loaded;
    }

    [Fact]
    public void TimeCodeKeepsHoursPast99()
    {
        var tc = new TimeCode(LongStart);
        Assert.Equal(123, tc.Hours);
        Assert.Equal("123:04:05,678", tc.ToString());
        Assert.Equal("123:04:05,678", tc.ToShortString());
        Assert.False(tc.IsMaxTime);
    }

    [Fact]
    public void TimeCodeHoldsUpToTheSupportedMax()
    {
        var tc = new TimeCode(TimeCode.MaxSupportedTotalMilliseconds);
        Assert.Equal("9999:59:59,999", tc.ToString());
        Assert.Equal(new TimeCode(9999, 59, 59, 999).TotalMilliseconds, TimeCode.MaxSupportedTotalMilliseconds);
    }

    [Fact]
    public void MaxTimeMarkerIsUnchanged()
    {
        Assert.True(new TimeCode(99, 59, 59, 999).IsMaxTime);
        Assert.Equal("99:59:59,999", new TimeCode(TimeCode.MaxTimeTotalMilliseconds).ToString());
    }

    [Fact]
    public void SubRipWritesAndReadsHoursPast99()
    {
        var text = new SubRip().ToText(MakeSubtitle(), "test");
        Assert.Contains("123:04:05,678 --> 123:04:07,678", text);

        var loaded = RoundTrip(new SubRip());
        Assert.Single(loaded.Paragraphs);
        Assert.Equal(LongStart, loaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(LongEnd, loaded.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void SubRipWritesFourDigitHours()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", new TimeCode(1234, 0, 0, 0).TotalMilliseconds, new TimeCode(1234, 0, 1, 0).TotalMilliseconds));
        var text = new SubRip().ToText(subtitle, "test");
        Assert.Contains("1234:00:00,000 --> 1234:00:01,000", text);
    }

    [Fact]
    public void WebVttWritesAndReadsHoursPast99()
    {
        var text = new WebVTT().ToText(MakeSubtitle(), "test");
        Assert.Contains("123:04:05.678 --> 123:04:07.678", text);

        var loaded = RoundTrip(new WebVTT());
        Assert.Single(loaded.Paragraphs);
        Assert.Equal(LongStart, loaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(LongEnd, loaded.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void AdvancedSubStationAlphaWritesAndReadsHoursPast99()
    {
        var text = new AdvancedSubStationAlpha().ToText(MakeSubtitle(), "test");
        Assert.Contains("123:04:05.68,123:04:07.68", text);

        var loaded = RoundTrip(new AdvancedSubStationAlpha());
        Assert.Single(loaded.Paragraphs);
        Assert.Equal(LongStart + 2, loaded.Paragraphs[0].StartTime.TotalMilliseconds); // centiseconds
    }

    [Fact]
    public void SubStationAlphaWritesAndReadsHoursPast99()
    {
        var text = new SubStationAlpha().ToText(MakeSubtitle(), "test");
        Assert.Contains("123:04:05.67,123:04:07.67", text);

        var loaded = RoundTrip(new SubStationAlpha());
        Assert.Single(loaded.Paragraphs);
        Assert.Equal(LongStart - 8, loaded.Paragraphs[0].StartTime.TotalMilliseconds); // centiseconds
    }

    [Fact]
    public void FormatsDefaultToTheOldLimit()
    {
        Assert.Equal(TimeCode.MaxTimeTotalMilliseconds, new Ebu().MaxTimeCodeTotalMilliseconds);
        Assert.Equal(TimeCode.MaxTimeTotalMilliseconds, new MicroDvd().MaxTimeCodeTotalMilliseconds);
        Assert.Equal(TimeCode.MaxTimeTotalMilliseconds, new ScenaristClosedCaptions().MaxTimeCodeTotalMilliseconds);
    }

    [Fact]
    public void LongTimeCodeFormatsAllow9999Hours()
    {
        SubtitleFormat[] formats =
        {
            new SubRip(), new WebVTT(), new TimedText10(), new AdvancedSubStationAlpha(), new SubStationAlpha(),
        };
        foreach (var format in formats)
        {
            Assert.Equal(TimeCode.MaxSupportedTotalMilliseconds, format.MaxTimeCodeTotalMilliseconds);
            Assert.Null(SubtitleFormatLimits.ForFormat(format)); // nothing to warn about
        }
    }

    [Fact]
    public void FormatLimitsListLinesPastTheTimeCodeLimit()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Fine", 1000, 2000));
        subtitle.Paragraphs.Add(new Paragraph("No time", TimeCode.MaxTimeTotalMilliseconds, TimeCode.MaxTimeTotalMilliseconds));
        subtitle.Paragraphs.Add(new Paragraph("Too late", LongStart, LongEnd));

        var limits = SubtitleFormatLimits.ForFormat(new MicroDvd());
        Assert.NotNull(limits);
        Assert.Equal(TimeCode.MaxTimeTotalMilliseconds, limits!.MaxTimeTotalMilliseconds);
        Assert.True(limits.HasTimeCodeTooLarge(subtitle));
        Assert.Equal(new List<int> { 3 }, limits.GetViolatingParagraphNumbers(subtitle));
    }

    [Fact]
    public void FormatLimitsKeepTextLimitsAndAddTheTimeCodeLimit()
    {
        var limits = SubtitleFormatLimits.ForFormat(new ScenaristClosedCaptions());
        Assert.NotNull(limits);
        Assert.Equal(32, limits!.MaxCharactersPerLine);
        Assert.Equal(TimeCode.MaxTimeTotalMilliseconds, limits.MaxTimeTotalMilliseconds);
    }

    [Fact]
    public void TimedTextReadsHoursPast99()
    {
        var xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                  "<tt xmlns=\"http://www.w3.org/ns/ttml\">\r\n" +
                  "  <body>\r\n" +
                  "    <div>\r\n" +
                  "      <p begin=\"123:04:05.678\" end=\"123:04:07.678\">Hello</p>\r\n" +
                  "    </div>\r\n" +
                  "  </body>\r\n" +
                  "</tt>";
        var subtitle = new Subtitle();
        new TimedText10().LoadSubtitle(subtitle, xml.SplitToLines(), null);
        Assert.Single(subtitle.Paragraphs);
        Assert.Equal(LongStart, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(LongEnd, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
    }
}
