using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Guard tests for the 2026-09-06 bug hunt (round 25): formats whose readers could not detect
/// their own output, writers that lost the second line of a cue, last cues that reloaded with
/// EndTime 0, quote and tab handling in CSV/TSV, the TTML fallback that turned tags into text,
/// a "multiply by ten" heuristic that corrupted correct times, and a CDATA escape hatch that
/// dropped the element the reader requires.
/// </summary>
public class BugHunt25Test
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

    private static Subtitle TwoLineSample() =>
        Make(("He said go and left", 1000, 3000), ("Two lines" + Environment.NewLine + "here now", 4000, 6000), ("Last one", 7000, 9000));

    public static TheoryData<SubtitleFormat> FormatsThatRejectedOwnOutput => new()
    {
        new UnknownSubtitle8(),
        new UnknownSubtitle23(),
        new UnknownSubtitle37(),
        new UnknownSubtitle59(),
        new UnknownSubtitle85(),
    };

    [Theory]
    [MemberData(nameof(FormatsThatRejectedOwnOutput))]
    public void IsMine_AcceptsOwnOutput(SubtitleFormat format)
    {
        // each of these counted something it writes itself as an error (the text line after a
        // time code, a dozen header lines, an RTF wrapper, a superset regex, a header/footer line)
        var text = format.ToText(TwoLineSample(), "title");

        Assert.True(format.IsMine(text.SplitToLines(), "file" + format.Extension));
    }

    [Fact]
    public void UnknownSubtitle85_TwoCueFile_IsDetected()
    {
        var format = new UnknownSubtitle85();
        var text = format.ToText(Make(("One", 1000, 3000), ("Two", 4000, 6000)), "title");

        Assert.True(format.IsMine(text.SplitToLines(), "file.txt"));
    }

    public static TheoryData<SubtitleFormat> FormatsThatDroppedTheSecondLine => new()
    {
        new UnknownSubtitle8(),
        new UnknownSubtitle21(),
        new UnknownSubtitle53(),
        new UnknownSubtitle85(),
        new UnknownSubtitle93(),
        new UnknownSubtitle100(),
        new ImageLogicAutocaption(),
    };

    [Theory]
    [MemberData(nameof(FormatsThatDroppedTheSecondLine))]
    public void TwoLineCue_RoundTrips(SubtitleFormat format)
    {
        var loaded = RoundTrip(format, TwoLineSample());

        Assert.Equal(3, loaded.Paragraphs.Count);
        Assert.Equal("Two lines" + Environment.NewLine + "here now", loaded.Paragraphs[1].Text);
        Assert.Equal("Last one", loaded.Paragraphs[2].Text);
    }

    [Fact]
    public void UnknownSubtitle9_TagsAreStrippedInsteadOfTruncatingTheText()
    {
        // the reader cuts the text at the first "</", so a raw <i>Two</i> came back as "Two"
        var loaded = RoundTrip(new UnknownSubtitle9(), Make(("<i>Two</i> lines" + Environment.NewLine + "here now", 4000, 6000)));

        Assert.Equal("Two lines" + Environment.NewLine + "here now", loaded.Paragraphs[0].Text);
    }

    public static TheoryData<SubtitleFormat> FormatsWithZeroEndOnLastCue => new()
    {
        new UnknownSubtitle33(),
        new UnknownSubtitle46(),
        new UnknownSubtitle98(),
        new UnknownSubtitle99(),
    };

    [Theory]
    [MemberData(nameof(FormatsWithZeroEndOnLastCue))]
    public void LastCue_GetsADuration(SubtitleFormat format)
    {
        // start-only formats derive the end from the next cue - the last one was left at 0
        var loaded = RoundTrip(format, Make(("One", 1000, 3000), ("Two", 4000, 6000), ("Last one", 7000, 9000)));

        var last = loaded.Paragraphs.Last();
        Assert.Equal("Last one", last.Text);
        Assert.True(last.EndTime.TotalMilliseconds > last.StartTime.TotalMilliseconds, $"end {last.EndTime.TotalMilliseconds} <= start {last.StartTime.TotalMilliseconds}");
    }

    [Fact]
    public void Csv_EmbeddedQuote_RoundTrips()
    {
        // the writer quoted the text but did not double an embedded quote, and the reader regex
        // ([^"]*) rejected the line - the cue merged into the previous one and IsMine failed
        var format = new Csv();
        var subtitle = Make(("He said \"go\" and left", 1000, 3000), ("Plain", 4000, 6000));
        var text = format.ToText(subtitle, "title");

        Assert.True(format.IsMine(text.SplitToLines(), "file.csv"));
        var loaded = RoundTrip(format, subtitle);
        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("He said \"go\" and left", loaded.Paragraphs[0].Text);
    }

    [Fact]
    public void Tsv1_QuoteInText_IsKept()
    {
        var loaded = RoundTrip(new Tsv1(), Make(("He said \"go\" and left", 1000, 3000), ("Plain", 4000, 6000)));

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("He said \"go\" and left", loaded.Paragraphs[0].Text);
    }

    [Fact]
    public void Tsv2_TabInText_DoesNotDropTheCue()
    {
        // a tab in the text made a fourth field and the reader silently skipped the line
        var loaded = RoundTrip(new Tsv2(), Make(("Yes\tNo", 1000, 3000), ("", 4000, 6000), ("Plain", 7000, 9000)));

        Assert.Equal(3, loaded.Paragraphs.Count);
        Assert.Equal("Yes No", loaded.Paragraphs[0].Text);
        Assert.Equal("Plain", loaded.Paragraphs[2].Text);
    }

    [Fact]
    public void Csv3_EmptyText_ReloadsEmpty()
    {
        // two empty cells ("","") walked back as the literal ","
        var loaded = RoundTrip(new Csv3(), Make(("Hello", 1000, 3000), ("", 4000, 6000)));

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal(string.Empty, loaded.Paragraphs[1].Text);
    }

    public static TheoryData<SubtitleFormat> TtmlWritersWithMarkupFallback => new()
    {
        new TimedText10(),
        new TimedTextNoNs(),
        new TimedTextImsc11(),
        new NetflixImsc11Japanese(),
    };

    [Theory]
    [MemberData(nameof(TtmlWritersWithMarkupFallback))]
    public void Ttml_LiteralLessThan_KeepsWordsAndLineBreak(SubtitleFormat format)
    {
        // "5 < 6" is not XML, so the paragraph went through the fallback, which stripped every <
        // and > - the tags became text ("iTwo/i linesbr/here 5 6") and the line break was lost
        var loaded = RoundTrip(format, Make(("<i>Two</i> lines" + Environment.NewLine + "here 5 < 6", 4000, 6000)));

        var text = HtmlUtil.RemoveHtmlTags(loaded.Paragraphs[0].Text, true);
        Assert.Equal("Two lines" + Environment.NewLine + "here 5 < 6", text);
    }

    [Theory]
    [InlineData(typeof(TimedText200604))]
    [InlineData(typeof(TimedText200604Ooyala))]
    public void TimedText2006_SubHundredMilliseconds_AreNotMultipliedByTen(Type formatType)
    {
        // when every cue had ms < 100 the reader multiplied them by ten: 1.050 s -> 1.500 s
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;
        var loaded = RoundTrip(format, Make(("Hello", 1050, 2080), ("World", 3020, 4090)));

        Assert.Equal(1050, loaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(2080, loaded.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(4090, loaded.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Theory]
    [InlineData(typeof(TmpegEncAW5))]
    [InlineData(typeof(TmpegEncXml))]
    public void TmpegEnc_CdataTerminatorInText_KeepsTheCue(Type formatType)
    {
        // "]]>" cannot go in a CDATA section; the escape hatch dropped the <Text> element and the
        // reader threw on the missing node and lost the cue
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;
        var loaded = RoundTrip(format, Make(("foo]]>bar", 1000, 3000), ("Plain", 4000, 6000)));

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("foo]]>bar", loaded.Paragraphs[0].Text);
    }

    [Fact]
    public void MsOfficeWorkbook_OneCueFile_IsDetected()
    {
        // the header row was parsed as a cue and counted as an error: 1 cue, 1 error -> rejected
        var format = new MsOfficeWorkbook();
        var text = format.ToText(Make(("Only one", 1000, 3000)), "title");

        Assert.True(format.IsMine(text.SplitToLines(), "file.xml"));
        Assert.Equal(0, format.ErrorCount);
    }

    [Theory]
    [InlineData(typeof(Rtf1))]
    [InlineData(typeof(Rtf2))]
    public void Rtf_FrameRateHeader_IsCultureInvariant(Type formatType)
    {
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;
        var old = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("da-DK");
        try
        {
            var text = format.ToText(Make(("Hi", 1000, 3000)), "title");
            Assert.DoesNotContain("23,976", text);
        }
        finally
        {
            CultureInfo.CurrentCulture = old;
        }
    }

    [Fact]
    public void AribB24Tables_HaveNoFiveDigitUnicodeEscapes()
    {
        // " b" is U+2000 followed by the letter b in C#; the supplementary-plane entries
        // needed the eight-digit \U form. A decoded table entry must never be a BMP char + ASCII.
        var tableStrings = typeof(AribB24Tables)
            .GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            .Select(f => f.GetValue(null))
            .SelectMany(v => v switch
            {
                string[] arr => arr,
                Dictionary<int, string> dict => dict.Values.ToArray(),
                _ => Array.Empty<string>(),
            })
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        Assert.NotEmpty(tableStrings);
        var suspicious = tableStrings.Where(s => s.Length == 2 && !char.IsSurrogatePair(s[0], s[1]) && char.IsAscii(s[1]) && s[0] > 0x7f).ToList();
        Assert.Empty(suspicious);
        Assert.Contains(tableStrings, s => s.Length == 2 && char.IsSurrogatePair(s[0], s[1]));
    }
}
