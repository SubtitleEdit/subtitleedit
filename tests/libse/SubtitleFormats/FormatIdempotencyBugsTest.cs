using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Guard tests for defects found by checking IDEMPOTENCY - writing a subtitle, reading it back
/// and writing it again must produce the same file. Anything that changes on the second write
/// is either lost or accumulating with every save (2026-08-27 bug hunt).
/// </summary>
public class FormatIdempotencyBugsTest
{
    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello world.", 2000, 4000));
        subtitle.Paragraphs.Add(new Paragraph("Second line here," + Environment.NewLine + "with a line break.", 6000, 9000));
        subtitle.Paragraphs.Add(new Paragraph("Tom & Jerry \"quoted\".", 12000, 15000));
        return subtitle;
    }

    private static Subtitle RoundTrip(SubtitleFormat format, Subtitle subtitle)
    {
        var text = format.ToText(subtitle, "title");
        var target = new Subtitle();
        format.LoadSubtitle(target, text.SplitToLines(), "test" + format.Extension);
        return target;
    }

    private static string SecondWrite(SubtitleFormat format, Subtitle subtitle)
        => format.ToText(RoundTrip(format, subtitle), "title");

    [Theory]
    [InlineData(typeof(TmpegEncAW5))]
    [InlineData(typeof(TmpegEncXml))]
    public void TmpegEnc_Escaping_DoesNotAccumulate(Type formatType)
    {
        // The CDATA was built from the XML-ESCAPED text, and since CDATA content is not parsed
        // that escaping was read back as literal text and escaped AGAIN - so an ampersand grew
        // an "amp;" on every single save.
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;
        var second = SecondWrite(format, MakeSubtitle());

        Assert.DoesNotContain("&amp;amp;", second, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(typeof(JacoSub))]
    [InlineData(typeof(Tmx14))]
    [InlineData(typeof(Speechmatics))]
    [InlineData(typeof(Captionate))]
    [InlineData(typeof(CaptionateMs))]
    public void LineBreak_SurvivesTheRoundTrip(Type formatType)
    {
        // Four different causes, one symptom: JACOsub's "\n" escape was undone by the ASSA tag
        // stripper (dropping the second line entirely), TMX and Captionate looked for a <br/>
        // ELEMENT in InnerText (which never contains markup), and Speechmatics glued the
        // physical lines of the file together.
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;
        var target = RoundTrip(format, MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Equal("Second line here," + Environment.NewLine + "with a line break.", target.Paragraphs[1].Text);
    }

    [Fact]
    public void Tsv2_TextWithQuotes_IsNotDropped()
    {
        // The line regex demanded a text field with no quote characters, so any cue whose text
        // contained a quotation mark failed the match and vanished on read.
        var target = RoundTrip(new Tsv2(), MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Contains("quoted", target.Paragraphs[2].Text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(typeof(SmilTimesheetData))]
    [InlineData(typeof(FLVCoreCuePoints))]
    public void EndTimes_ParsedFromTheFile_AreNotOverwritten(Type formatType)
    {
        // Both readers parsed the end time / duration the writer stores and then a post-pass
        // threw it away, replacing it with "next start minus a gap" or an estimated duration.
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;
        var target = RoundTrip(format, MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Equal(4000, target.Paragraphs[0].EndTime.TotalMilliseconds, 0);
        Assert.Equal(9000, target.Paragraphs[1].EndTime.TotalMilliseconds, 0);
    }

    [Theory]
    [InlineData(typeof(Captionate))]
    [InlineData(typeof(CaptionateMs))]
    public void Captionate_EndMarker_SetsTheEndTime(Type formatType)
    {
        // The blank "end marker" caption has no <tracks> child at all, so the reader never
        // cleared its current paragraph - the NEXT caption then overwrote the end time and
        // every cue ran on until the following one started.
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;
        var target = RoundTrip(format, MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.True(target.Paragraphs[0].EndTime.TotalMilliseconds < 4500,
            $"first cue should end near 4000 ms, was {target.Paragraphs[0].EndTime.TotalMilliseconds}");
    }

    [Fact]
    public void Captionate_TimeCode_HasNoThreeDigitField()
    {
        // "{Milliseconds / 10.0:00}" formatted 999 ms as "100" - a three digit field no reader
        // can parse back.
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello.", 2000, 3999));

        var text = new Captionate().ToText(subtitle, "title");

        Assert.DoesNotContain(":100\"", text, StringComparison.Ordinal);
    }

    [Fact]
    public void AqTitle_Frames_DoNotDriftOnEverySave()
    {
        // EncodeTimeCode added one frame that DecodeTimeCode never subtracted, so the whole
        // file moved one frame later with each save.
        var format = new AQTitle();
        var first = format.ToText(MakeSubtitle(), "title");
        var second = SecondWrite(format, MakeSubtitle());

        Assert.Equal(first, second);
    }

    [Theory]
    [InlineData(typeof(MagicVideoTitler))]
    [InlineData(typeof(SmartTitler))]
    public void Titler_Digraphs_AreEncodedBack(Type formatType)
    {
        // These charsets encode the Croatian/Serbian digraphs as single ASCII letters ("nj" is
        // written as "w"). EncodeText inverted the map but looked values up ONE character at a
        // time, so the two-character values never matched: a digraph was written through
        // unchanged and then read back as something else entirely.
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Konjic i Ljubljana.", 2000, 4000));
        subtitle.Paragraphs.Add(new Paragraph("Druga linija.", 6000, 9000));

        var target = RoundTrip(format, subtitle);

        Assert.Equal(2, target.Paragraphs.Count);
        Assert.Equal("Konjic i Ljubljana.", target.Paragraphs[0].Text);
    }

    [Fact]
    public void ImageLogicAutocaption_CueLength_DoesNotShrinkOnEverySave()
    {
        // The numbered row with no text carries the cue's END time; subtracting the minimum gap
        // from it made every cue a little shorter with each save.
        var target = RoundTrip(new ImageLogicAutocaption(), MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Equal(4000, target.Paragraphs[0].EndTime.TotalMilliseconds, 0);
    }

    [Fact]
    public void NciTimedRollUpCaptions_LastCue_KeepsItsEndTime()
    {
        // The 0x14 "end of captions" row carries the last cue's real end time; treating it as
        // another caption made that cue end one minimum-gap early, accumulating per save.
        var target = RoundTrip(new NciTimedRollUpCaptions(), MakeSubtitle());

        Assert.NotEmpty(target.Paragraphs);
        Assert.Equal(15000, target.Paragraphs[target.Paragraphs.Count - 1].EndTime.TotalMilliseconds, 0);
    }

    [Theory]
    [InlineData(typeof(KanopyHtml))]
    [InlineData(typeof(Xif))]
    public void Whitespace_DoesNotGrowAroundLineBreaks(Type formatType)
    {
        // Both join their line runs with a leading space, so every save added one more space
        // at the start of the second line.
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;
        var target = RoundTrip(format, MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        var lines = target.Paragraphs[1].Text.SplitToLines();
        Assert.All(lines, l => Assert.Equal(l.Trim(), l));
    }

    [Theory]
    [InlineData(typeof(FinalCutProXml))]
    [InlineData(typeof(FinalCutProTest2Xml))]
    public void FinalCutPro_NtscFlag_IsHonoredOnRead(Type formatType)
    {
        // A Final Cut rate is a whole "timebase" plus an "ntsc" flag: 24 + TRUE means 23.976.
        // Ignoring the flag read every NTSC file 0.1% too fast (~3.6 seconds an hour).
        var savedFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 23.976;
            var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;
            var target = RoundTrip(format, MakeSubtitle());

            Assert.Equal(3, target.Paragraphs.Count);

            // Within one frame: the residue is frame quantization, not a scale error (reading
            // at a flat 24 fps put the last cue ~14 ms out and grew with the time code).
            Assert.True(Math.Abs(target.Paragraphs[2].EndTime.TotalMilliseconds - 15000) < 42,
                $"last cue ended at {target.Paragraphs[2].EndTime.TotalMilliseconds} ms");
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = savedFrameRate;
        }
    }
}
