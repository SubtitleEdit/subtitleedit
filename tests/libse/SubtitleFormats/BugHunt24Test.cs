using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Guard tests for the 2026-09-06 random-file bug hunt: writers whose own readers could not
/// round-trip them (culture-sensitive millisecond output, hundredths or frames read as
/// milliseconds, cue order swapped, an hour-long time code parsed as minutes), a JSON field
/// written escaped but read raw, and an unguarded Substring in the italic-tag fixer.
/// </summary>
public class BugHunt24Test
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

    private static T WithCulture<T>(string name, Func<T> action)
    {
        var old = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo(name);
        try
        {
            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = old;
        }
    }

    public static TheoryData<SubtitleFormat> CommaCultureWriters => new()
    {
        new FLVCoreCuePoints(),
        new HollyStarJson(),
        new JsonType5(),
        new JsonType7(),
        new JsonType17(),
        new OpenDvt(),
    };

    [Theory]
    [MemberData(nameof(CommaCultureWriters))]
    public void MillisecondWriters_UnderCommaDecimalCulture_RoundTrip(SubtitleFormat format)
    {
        // fractional milliseconds (e.g. after a frame-rate conversion) formatted with the current
        // culture gave "1500,5", which the integer-parsing readers rejected or split on the comma
        var subtitle = Make(("Hello there", 1500.5, 3000.25), ("World again", 4000.75, 6000.5));

        var loaded = WithCulture("da-DK", () => RoundTrip(format, subtitle));

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal(1500.5, loaded.Paragraphs[0].StartTime.TotalMilliseconds, 1.0);
        Assert.Equal(4000.75, loaded.Paragraphs[1].StartTime.TotalMilliseconds, 1.0);
        if (format is not OpenDvt) // OpenDVT carries start times only; the reader estimates the duration
        {
            Assert.Equal(6000.5, loaded.Paragraphs[1].EndTime.TotalMilliseconds, 1.0);
        }
    }

    [Fact]
    public void UnknownSubtitle2_WritesThreeDigitMilliseconds()
    {
        // the sample header reads "00:00:48,862" (milliseconds) and the reader takes the field as
        // milliseconds, but the writer emitted hundredths - 1.520 s came back as 1.052 s
        var loaded = RoundTrip(new UnknownSubtitle2(), Make(("Hi", 1520, 3480)));

        Assert.Equal(1520, loaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3480, loaded.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void UnknownSubtitle4_TwoDigitFraction_IsHundredths()
    {
        // sample: "00:00:22.00, 00:00:27.00" - two digits; reading them as milliseconds put 22.50 at 22.050
        var format = new UnknownSubtitle4();
        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, "00:00:22.50, 00:00:27.05\r\nHi".SplitToLines(), null);

        Assert.Equal(22500, loaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(27050, loaded.Paragraphs[0].EndTime.TotalMilliseconds);

        var roundTripped = RoundTrip(format, Make(("Hi", 1520, 3480)));
        Assert.Equal(1520, roundTripped.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3480, roundTripped.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void UnknownSubtitle3_WritesMilliseconds_NotFrames()
    {
        // the sample header "155822||160350" is milliseconds and the reader takes them as such;
        // the writer emitted frame counts, so 1.520 s came back as 0.036 s
        var loaded = RoundTrip(new UnknownSubtitle3(), Make(("Hi", 1520, 3480)));

        Assert.Equal(1520, loaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3480, loaded.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void UnknownSubtitle31_RestFieldIsFramesWithinFoot()
    {
        // "footage.frames" with 16 frames per foot: the writer scaled the remainder by the frame
        // rate, so a cue at 1.520 s reloaded 60 ms late
        var loaded = RoundTrip(new UnknownSubtitle31(), Make(("Hi", 1520, 3480), ("There", 4000, 6000)));

        var frameMs = 1000.0 / Configuration.Settings.General.CurrentFrameRate;
        Assert.Equal(1520, loaded.Paragraphs[0].StartTime.TotalMilliseconds, frameMs);
        Assert.Equal(3480, loaded.Paragraphs[0].EndTime.TotalMilliseconds, frameMs);
        Assert.Equal(4000, loaded.Paragraphs[1].StartTime.TotalMilliseconds, frameMs);
    }

    [Fact]
    public void UnknownSubtitle45_SecondsFieldTruncates()
    {
        // "{0:00000}" on the seconds double rounded 1.52 s up to "00002" and then appended the frames
        // of the 520 ms as well - 1.520 s came back as 2.500 s
        var loaded = RoundTrip(new UnknownSubtitle45(), Make(("Hi", 1520, 3480)));

        var frameMs = 1000.0 / Configuration.Settings.General.CurrentFrameRate;
        Assert.Equal(1520, loaded.Paragraphs[0].StartTime.TotalMilliseconds, frameMs);
        Assert.Equal(3480, loaded.Paragraphs[0].EndTime.TotalMilliseconds, frameMs);
    }

    [Fact]
    public void UnknownSubtitle50_LastFieldIsFrames()
    {
        // the sample header "00.00.05.09-00.00.08.29" carries frames (00-29), which FormatTime
        // writes, but the reader took the field as milliseconds
        var loaded = RoundTrip(new UnknownSubtitle50(), Make(("Hi", 1520, 3480)));

        var frameMs = 1000.0 / Configuration.Settings.General.CurrentFrameRate;
        Assert.Equal(1520, loaded.Paragraphs[0].StartTime.TotalMilliseconds, frameMs);
        Assert.Equal(3480, loaded.Paragraphs[0].EndTime.TotalMilliseconds, frameMs);
    }

    [Fact]
    public void UnknownSubtitle60_WritesStartTimeOnly()
    {
        // the format has one time code per cue; the writer also emitted the end time, which the
        // reader took as the start of a new (empty) cue, so every cue reloaded at its old end time
        var loaded = RoundTrip(new UnknownSubtitle60(), Make(("Hi", 1520, 3480), ("There", 4000, 6000)));

        var frameMs = 1000.0 / Configuration.Settings.General.CurrentFrameRate;
        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal(1520, loaded.Paragraphs[0].StartTime.TotalMilliseconds, frameMs);
        Assert.Equal("Hi", loaded.Paragraphs[0].Text);
        Assert.Equal(4000, loaded.Paragraphs[1].StartTime.TotalMilliseconds, frameMs);
        Assert.Equal("There", loaded.Paragraphs[1].Text);
    }

    [Fact]
    public void UnknownSubtitle105_TextPrecedesItsWait()
    {
        // the reader shows the text collected before a [WAIT] line for that wait's duration;
        // the writer put the wait first, so every text got the next cue's timing
        var loaded = RoundTrip(new UnknownSubtitle105(), Make(("Hi", 1520, 3480), ("There", 4000, 6000)));

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("Hi", loaded.Paragraphs[0].Text);
        Assert.Equal(1520, loaded.Paragraphs[0].StartTime.TotalMilliseconds, 1);
        Assert.Equal(3480, loaded.Paragraphs[0].EndTime.TotalMilliseconds, 1);
        Assert.Equal("There", loaded.Paragraphs[1].Text);
        Assert.Equal(4000, loaded.Paragraphs[1].StartTime.TotalMilliseconds, 1);
        Assert.Equal(6000, loaded.Paragraphs[1].EndTime.TotalMilliseconds, 1);
    }

    [Fact]
    public void YouTubeTranscriptOneLine_HourLongTimeCode_KeepsHoursAndText()
    {
        // "1:01:01 Third one" matched the minutes:seconds regex first and loaded as 1:01 with ":01 Third one" as text
        var loaded = RoundTrip(new YouTubeTranscriptOneLine(), Make(("Hi", 1000, 3000), ("Third one", 3661000, 3663000)));

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal(3661000, loaded.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal("Third one", loaded.Paragraphs[1].Text);
    }

    [Fact]
    public void PodcastIndexer_SpeakerIsDecodedLikeBody()
    {
        var subtitle = Make(("Hello", 1000, 3000));
        subtitle.Paragraphs[0].Actor = "Bob \"The Builder\"";

        var loaded = RoundTrip(new PodcastIndexer(), subtitle);

        Assert.Equal("Bob \"The Builder\"", loaded.Paragraphs[0].Actor);
    }

    [Fact]
    public void DcPropertiesSmpte_EscapedValuesReloadUnescaped()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var props = new DcPropertiesSmpte
            {
                GenerateIdAuto = "true",
                ReelNumber = "1",
                Language = "en",
                EditRate = "24 1",
                TimeCodeRate = "24",
                StartTime = "00:00:00:00",
                FontId = "Font1",
                FontUri = "C:\\fonts\\a.ttf",
                FontColor = "FFFFFFFF",
                Effect = "border",
                EffectColor = "FF000000",
                FontSize = "42",
                TopBottomMargin = "8",
                FadeUpTime = "0",
                FadeDownTime = "0",
            };
            Assert.True(props.Save(fileName));

            var loaded = new DcPropertiesSmpte();
            Assert.True(loaded.Load(fileName));
            Assert.Equal("C:\\fonts\\a.ttf", loaded.FontUri);
            Assert.Equal("1", loaded.ReelNumber);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void FixInvalidItalicTags_ColonBranch_BareNewline_DoesNotThrow()
    {
        // GetNumberOfLines counts '\n', so a bare "\n" reaches the two-line branch; on Windows
        // IndexOf(Environment.NewLine) is then -1 and the "FALCONE:" sub-branch did Substring(0, -1)
        var text = "<i>FALCONE: I didn't think</i>\n<i>it was you</i>";

        var result = HtmlUtil.FixInvalidItalicTags(text);

        Assert.Contains("FALCONE", result);
        Assert.Contains("it was you", result);
    }
}
