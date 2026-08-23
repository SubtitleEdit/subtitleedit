using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Core;

public class BugHunt20260823Round2Test
{
    [Fact]
    public void DvdStudioProSpace_ShortTimeCodesAndSeparatorInText()
    {
        var sub = new Subtitle();
        new DvdStudioProSpace().LoadSubtitle(sub, new List<string> { "0:0:0:0 , 0:0:0:1 , Hi , there" }, null);
        Assert.Single(sub.Paragraphs);
        Assert.Equal("Hi , there", sub.Paragraphs[0].Text);
        Assert.Equal(0, sub.Paragraphs[0].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void DvdStudioProSpaceGraphic_ShortTimeCodes()
    {
        var sub = new Subtitle();
        new DvdStudioProSpaceGraphic().LoadSubtitle(sub, new List<string> { "0:0:0:0 , 0:0:0:1 , <<Graphic>>a.png" }, null);
        Assert.Single(sub.Paragraphs);
        Assert.EndsWith("a.png", sub.Paragraphs[0].Text);
        Assert.Equal(0, sub.Paragraphs[0].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void DvdStudioProSpaceOne_ShortTimeCodes()
    {
        var sub = new Subtitle();
        new DvdStudioProSpaceOne().LoadSubtitle(sub, new List<string> { "0:0:0:0,0:0:0:1, Hi, there" }, null);
        Assert.Single(sub.Paragraphs);
        Assert.Equal("Hi, there", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void DvdStudioProSpaceOneSemicolon_ShortTimeCodes()
    {
        new DvdStudioProSpaceOneSemicolon().LoadSubtitle(new Subtitle(), new List<string> { "0:0:0;0,0:0:0;1, Hi" }, null);
    }

    [Fact]
    public void Csv_RoundTripWithSeparatorInText()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Hello; world", 0, 1000));
        sub.Paragraphs.Add(new Paragraph("Plain line", 2000, 3000));

        var loaded = new Subtitle();
        new Csv().LoadSubtitle(loaded, new Csv().ToText(sub, "t").SplitToLines(), null);

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("Hello; world", loaded.Paragraphs[0].Text);
        Assert.Equal("Plain line", loaded.Paragraphs[1].Text);
    }

    [Fact]
    public void AssaResampler_WhitespaceBeforeParenthesis()
    {
        Assert.Equal("{\\pos(20,22)}Hi", AssaResampler.ResampleOverrideTagsPosition(720, 1440, 480, 960, "{\\pos(10,11)}Hi"));
        Assert.Equal("{\\pos(20,22)}Hi", AssaResampler.ResampleOverrideTagsPosition(720, 1440, 480, 960, "{\\pos (10,11)}Hi"));
        Assert.Equal("{\\move(20,22,40,42)}Hi", AssaResampler.ResampleOverrideTagsPosition(720, 1440, 480, 960, "{\\move ( 10 , 11 , 20 , 21 )}Hi"));
    }

    [Fact]
    public void AssaResampler_OneOddTagDoesNotAbandonTheRest()
    {
        var result = AssaResampler.ResampleOverrideTagsPosition(720, 1440, 480, 960, "{\\pos (10,11)}A{\\pos(30,31)}B");
        Assert.Equal("{\\pos(20,22)}A{\\pos(60,62)}B", result);
    }

    [Fact]
    public void WordsPerMinute_IsFiniteForZeroAndNegativeDuration()
    {
        Assert.True(double.IsFinite(new Paragraph("Hello world", 1000, 1000).WordsPerMinute));
        Assert.True(new Paragraph("Hello world", 2000, 1000).WordsPerMinute >= 0);
        // unchanged for a normal line: 2 words in 1 second = 120 wpm
        Assert.Equal(120, new Paragraph("Hello world", 0, 1000).WordsPerMinute, 3);
    }

    [Fact]
    public void WebVttThumbnail_AcceptsJpeg()
    {
        var sub = new Subtitle();
        new WebVttThumbnail().LoadSubtitle(sub, new List<string>
        {
            "WEBVTT", "", "00:00:00.000 --> 00:00:10.000", "sheet.jpeg#xywh=0,0,120,67", ""
        }, null);
        Assert.Single(sub.Paragraphs);
    }

    [Fact]
    public void SsaStyle_StrikeoutIsWrittenFromTheStyle()
    {
        const string format = "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding";
        var style = new SsaStyle { Name = "Test", Strikeout = true };
        var raw = style.ToRawAss(format);
        // StrikeOut is field 11 (0-based 10) - it must carry the style's own value, not a constant
        Assert.Equal("-1", raw.Substring("Style: ".Length).Split(',')[10].Trim());
    }

    [Fact]
    public void AssaCheckForErrors_IndentedHeaderStillFindsEmptyName()
    {
        const string header = @"[V4+ Styles]
  Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
  Style: ,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,2,2,10,10,10,1";
        Assert.Contains("'Name' is empty", AdvancedSubStationAlpha.CheckForErrors(header));
    }
}
