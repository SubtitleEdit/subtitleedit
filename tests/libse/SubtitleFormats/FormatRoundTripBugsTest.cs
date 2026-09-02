using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Guard tests for write-then-read defects found by round-tripping every text based
/// format through its own writer and reader (2026-08-27 bug hunt).
/// </summary>
public class FormatRoundTripBugsTest
{
    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello world.", 2000, 4000));
        subtitle.Paragraphs.Add(new Paragraph("Second line here," + Environment.NewLine + "with a line break.", 6000, 9500));
        subtitle.Paragraphs.Add(new Paragraph("Third - the last one.", 12000, 15000));
        return subtitle;
    }

    private static Subtitle RoundTrip(SubtitleFormat format, Subtitle subtitle)
    {
        var text = format.ToText(subtitle, "title");
        var target = new Subtitle();
        format.LoadSubtitle(target, text.SplitToLines(), "test" + format.Extension);
        return target;
    }

    [Fact]
    public void JsonType13_Duration_UsesTotalMilliseconds()
    {
        // TimeSpan.FromSeconds(2).Milliseconds is 0 (the 0-999 component), so whole-second
        // durations collapsed every line to zero length.
        var target = RoundTrip(new JsonType13(), MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Equal(4000, target.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal("Second line here," + Environment.NewLine + "with a line break.", target.Paragraphs[1].Text);
    }

    [Fact]
    public void JsonType16_Text_IsJsonDecodedOnRead()
    {
        var target = RoundTrip(new JsonType16(), MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Equal("Second line here," + Environment.NewLine + "with a line break.", target.Paragraphs[1].Text);
    }

    [Fact]
    public void Bilibili_LineBreaks_SurviveTheRoundTrip()
    {
        // The writer used to escape a pre-replaced "\n", so line breaks came back as a
        // literal backslash-n in the text.
        var target = RoundTrip(new Bilibili(), MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Equal("Second line here," + Environment.NewLine + "with a line break.", target.Paragraphs[1].Text);
    }

    [Fact]
    public void FlashXml_BrInsideCdata_BecomesALineBreak()
    {
        var target = RoundTrip(new FlashXml(), MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Equal("Second line here," + Environment.NewLine + "with a line break.", target.Paragraphs[1].Text);
    }

    [Fact]
    public void VocapiaSplit_Times_KeepSubSecondPrecision()
    {
        // The old integer format mask rounded every time code to whole seconds on save.
        var target = RoundTrip(new VocapiaSplit(), MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Equal(9500, target.Paragraphs[1].EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void InqScribe_LastParagraph_KeepsItsEndTime()
    {
        // The writer never emitted an end stamp for the last paragraph, so readers could
        // only guess its duration.
        var target = RoundTrip(new InqScribe(), MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Equal(15000, target.Paragraphs[2].EndTime.TotalMilliseconds, 0);
    }

    [Fact]
    public void InqScribe_Frames_AreWrittenAtTheDeclaredFps()
    {
        // The template declares timecode.fps=30 and the reader honors it - the writer used
        // to compute frames at the globally loaded video's rate instead.
        var savedFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 23.976;
            var format = new InqScribe();
            var text = format.ToText(MakeSubtitle(), "title");
            var target = new Subtitle();
            format.LoadSubtitle(target, text.SplitToLines(), "test" + format.Extension);

            Assert.Equal(3, target.Paragraphs.Count);
            Assert.Equal(9500, target.Paragraphs[1].EndTime.TotalMilliseconds, 0);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = savedFrameRate;
        }
    }

    [Fact]
    public void NetflixImsc11Japanese_Frames_AreWrittenAtTheDeclaredFrameRate()
    {
        // The fixed header declares 24 fps with a 1000/1001 multiplier; writing the frames
        // at the open video's rate (e.g. 29.97) shifted every time code on read.
        var savedFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 29.97;
            var format = new NetflixImsc11Japanese();
            var text = format.ToText(MakeSubtitle(), "title");
            var target = new Subtitle();
            format.LoadSubtitle(target, text.SplitToLines(), "test" + format.Extension);

            Assert.Equal(3, target.Paragraphs.Count);
            Assert.Equal(9500, target.Paragraphs[1].EndTime.TotalMilliseconds, 0);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = savedFrameRate;
        }
    }

    [Fact]
    public void OresmeDocXDocument_LastLine_EndsAfterItsStart()
    {
        // The last row's fallback end time was assigned as an ABSOLUTE 2500 ms, placing it
        // before the line's own start.
        var target = RoundTrip(new OresmeDocXDocument(), MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        var last = target.Paragraphs[2];
        Assert.True(last.EndTime.TotalMilliseconds > last.StartTime.TotalMilliseconds,
            $"last line: {last.StartTime} --> {last.EndTime}");
    }

    [Fact]
    public void F4Text_LeadingText_GetsTheFirstStampAsEndTime()
    {
        // Text before the first stamp used to be added as a 0 -> 0 paragraph AND repeated
        // inside the following paragraph.
        var target = RoundTrip(new F4Text(), MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Equal("Hello world.", target.Paragraphs[0].Text);
        Assert.Equal(4000, target.Paragraphs[0].EndTime.TotalMilliseconds, 0);
        Assert.DoesNotContain("Hello", target.Paragraphs[1].Text);
    }

    [Theory]
    [InlineData(typeof(QubeMasterImport))]
    [InlineData(typeof(Titra))]
    [InlineData(typeof(DvdSubtitleSystem))]
    [InlineData(typeof(Rtf1))]
    [InlineData(typeof(Rtf2))]
    public void IsMine_RecognizesTheFormatsOwnOutput(Type formatType)
    {
        // Header/blank lines (or, for QubeMaster, an inverted end-time guard) were counted
        // as errors, so short files in these formats were never auto-detected.
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;
        var text = format.ToText(MakeSubtitle(), "title");

        Assert.True(format.IsMine(text.SplitToLines(), "test" + format.Extension),
            format.Name + " does not recognize its own output");
    }

    [Fact]
    public void Rtf1_LastLine_KeepsItsRealEndTime()
    {
        // The tail fix-up overwrote the last line's end time with an "optimal" duration
        // even though the time-code line supplied one (Rtf2 already guarded this).
        var target = RoundTrip(new Rtf1(), MakeSubtitle());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Equal(15000, target.Paragraphs[2].EndTime.TotalMilliseconds, 0);
    }
}
