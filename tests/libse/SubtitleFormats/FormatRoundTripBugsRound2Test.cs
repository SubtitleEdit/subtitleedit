using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Guard tests for the second round of write-then-read defects, found by round-tripping every
/// text based format with richer content: formatting tags, characters that need escaping,
/// multi-line text, and empty/one-line subtitles (2026-08-27 bug hunt).
/// </summary>
public class FormatRoundTripBugsRound2Test
{
    private static Subtitle MakeTagged()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<i>Italic line one.</i>", 2000, 4000));
        subtitle.Paragraphs.Add(new Paragraph("Normal, then <i>italic</i> mid-line.", 6000, 9000));
        subtitle.Paragraphs.Add(new Paragraph("<b>Bold</b> and <u>underline</u>.", 12000, 15000));
        return subtitle;
    }

    private static Subtitle MakeEscaped()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Tom & Jerry", 2000, 4000));
        subtitle.Paragraphs.Add(new Paragraph("She said \"hello\" - that's it.", 6000, 9000));
        subtitle.Paragraphs.Add(new Paragraph("Third line.", 12000, 15000));
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
    public void FinalCutProXml_UnderlineTag_DoesNotMangleTheText()
    {
        // <u> has no FCP style equivalent and no branch handled it, so the writer consumed
        // only the '<' and left "u>underline/u>" in the text.
        var target = RoundTrip(new FinalCutProXml15(), MakeTagged());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.DoesNotContain("u>", target.Paragraphs[2].Text, StringComparison.Ordinal);
        Assert.Contains("underline", target.Paragraphs[2].Text, StringComparison.Ordinal);
    }

    [Fact]
    public void UtSubtitleXml_Markup_IsNotDoubleEscapedIntoCdata()
    {
        // The writer went through InnerText first (escaping the markup) and then wrapped the
        // result in CDATA - and CDATA content is not parsed, so "&lt;i&gt;" came back literally.
        var target = RoundTrip(new UTSubtitleXml(), MakeTagged());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.DoesNotContain("&lt;", target.Paragraphs[0].Text, StringComparison.Ordinal);
        Assert.Equal("<i>Italic line one.</i>", target.Paragraphs[0].Text);
    }

    [Fact]
    public void FlashXml_Entities_AreNotDoubleEscapedIntoCdata()
    {
        var target = RoundTrip(new FlashXml(), MakeEscaped());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Equal("Tom & Jerry", target.Paragraphs[0].Text);
    }

    [Fact]
    public void KanopyHtml_InlineClosingTag_DoesNotTruncateTheLine()
    {
        // The reader cut the text at the FIRST "</", so everything after an inline closing
        // tag was thrown away; only the trailing wrapper should be stripped.
        var target = RoundTrip(new KanopyHtml(), MakeTagged());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Contains("mid-line.", target.Paragraphs[1].Text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(typeof(JsonAeneas))]
    [InlineData(typeof(JsonType17))]
    [InlineData(typeof(Bilibili))]
    public void JsonFormats_EscapedQuotes_AreDecodedOnRead(Type formatType)
    {
        // These writers escape with Json.EncodeJsonText but the readers returned the raw
        // string, so quotes came back as \" in the subtitle text.
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;
        var target = RoundTrip(format, MakeEscaped());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.DoesNotContain("\\\"", target.Paragraphs[1].Text, StringComparison.Ordinal);
        Assert.Contains("\"hello\"", target.Paragraphs[1].Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Csv3_EmbeddedQuotes_SurviveTheRoundTrip()
    {
        // ReadText collapsed the doubled quotes BEFORE the walk that needed them, leaving the
        // loop unable to tell an escaped quote from a field delimiter.
        var target = RoundTrip(new Csv3(), MakeEscaped());

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Contains("\"hello\"", target.Paragraphs[1].Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Csv3_ThreeLines_AreNotSilentlyDropped()
    {
        // The format has two text columns but only rebalanced at four lines or more, so the
        // third line of a three-line subtitle was written nowhere.
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(
            "Line one." + Environment.NewLine + "Line two." + Environment.NewLine + "Line three.", 2000, 4000));

        var target = RoundTrip(new Csv3(), subtitle);

        Assert.Single(target.Paragraphs);
        Assert.Contains("three", target.Paragraphs[0].Text, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonType6_LastWord_IsNotDropped()
    {
        // The merge loop stopped at Count - 1, so the final word was only ever seen as "next".
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Alpha beta gamma.", 2000, 4000));
        subtitle.Paragraphs.Add(new Paragraph("Delta epsilon zeta.", 6000, 9000));
        subtitle.Paragraphs.Add(new Paragraph("Eta theta iota.", 12000, 15000));

        var target = RoundTrip(new JsonType6(), subtitle);

        Assert.Equal(3, target.Paragraphs.Count);
        Assert.Contains("iota.", target.Paragraphs[2].Text, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonType6_LastParagraph_EndsAfterItsStart()
    {
        // GetOptimalDisplayMilliseconds is a DURATION; it was assigned as the absolute end time.
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Alpha beta gamma.", 2000, 4000));
        subtitle.Paragraphs.Add(new Paragraph("Delta epsilon zeta.", 6000, 9000));
        subtitle.Paragraphs.Add(new Paragraph("Eta theta iota.", 12000, 15000));

        var target = RoundTrip(new JsonType6(), subtitle);

        var last = target.Paragraphs[target.Paragraphs.Count - 1];
        Assert.True(last.EndTime.TotalMilliseconds > last.StartTime.TotalMilliseconds,
            $"last line: {last.StartTime} --> {last.EndTime}");
    }

    [Fact]
    public void ESubXf_DeclaredFrameRate_IsHonoredOnRead()
    {
        // ToText writes framerate="..." into the file; the reader ignored it and decoded the
        // SMPTE frames at whatever rate happened to be loaded.
        var savedFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hello world.", 2000, 4000));
            subtitle.Paragraphs.Add(new Paragraph("Second line.", 6000, 9000));

            Configuration.Settings.General.CurrentFrameRate = 29.97;
            var format = new ESubXf();
            var text = format.ToText(subtitle, "title");

            Configuration.Settings.General.CurrentFrameRate = 25;
            var target = new Subtitle();
            format.LoadSubtitle(target, text.SplitToLines(), "test" + format.Extension);

            Assert.Equal(2, target.Paragraphs.Count);
            Assert.Equal(9000, target.Paragraphs[1].EndTime.TotalMilliseconds, 0);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = savedFrameRate;
        }
    }

    [Theory]
    [InlineData(typeof(SoftNicolonSub))]
    [InlineData(typeof(UnknownSubtitle101))]
    public void EmptySubtitle_DoesNotThrowOnWrite(Type formatType)
    {
        // Paragraphs.Last() threw "Sequence contains no elements" - the null check right
        // after it shows LastOrDefault was always the intent.
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;

        var text = format.ToText(new Subtitle(), "title");

        Assert.NotNull(text);
    }
}
