using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class YouTubeTimedTextTest
{
    /// <summary>The shape YTSubConverter writes: placeholder id 0 entries, s with pens, a zero-width space after the first s.</summary>
    private const string YtSubConverterFile =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
        "<timedtext format=\"3\">\n" +
        "<head>\n" +
        "<wp id=\"0\" ap=\"7\" ah=\"0\" av=\"0\" />\n" +
        "<wp id=\"1\" ap=\"1\" ah=\"50\" av=\"10\" />\n" +
        "<wp id=\"2\" ap=\"7\" ah=\"50\" av=\"100\" />\n" +
        "<ws id=\"0\" ju=\"2\" pd=\"0\" sd=\"0\" wfo=\"0\" />\n" +
        "<ws id=\"1\" ju=\"2\" pd=\"0\" sd=\"0\" wfo=\"0\" />\n" +
        "<pen id=\"0\" sz=\"100\" fc=\"#000000\" fo=\"0\" bo=\"0\" />\n" +
        "<pen id=\"1\" sz=\"100\" fc=\"#FEFEFE\" fo=\"254\" bc=\"#080808\" bo=\"192\" />\n" +
        "<pen id=\"2\" sz=\"200\" i=\"1\" fc=\"#FF0000\" fo=\"254\" bo=\"0\" et=\"3\" ec=\"#0000FF\" />\n" +
        "</head>\n" +
        "<body>\n" +
        "<p t=\"1\" d=\"1999\" p=\"1\" wp=\"1\" ws=\"1\">Top line</p>\n" +
        "<p t=\"2500\" d=\"2000\" wp=\"2\" ws=\"1\"><s p=\"1\">Plain </s>​<s p=\"2\" t=\"500\">glowing</s></p>\n" +
        "</body>\n" +
        "</timedtext>\n";

    private static Subtitle Load(string text, out YouTubeTimedText format)
    {
        format = new YouTubeTimedText();
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, text.Split('\n').ToList(), "test.ytt");
        return subtitle;
    }

    [Fact]
    public void PrimaryExtensionIsYtt()
    {
        var format = new YouTubeTimedText();
        Assert.Equal(".ytt", format.Extension);
        Assert.Contains(".srv3", format.AlternateExtensions);
    }

    [Fact]
    public void ReadsHeadLinePensAndSpans()
    {
        var subtitle = Load(YtSubConverterFile, out _);

        Assert.Equal(2, subtitle.Paragraphs.Count);

        // t="1" is the 0 ms workaround
        Assert.Equal(0, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(2000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Top line", subtitle.Paragraphs[0].Text);
        Assert.Equal("1", subtitle.Paragraphs[0].Style);
        Assert.Equal("1", subtitle.Paragraphs[0].Region);
        Assert.Equal("1", subtitle.Paragraphs[0].Effect);

        // the zero-width space is gone, p="1" stays a tag since the line has no pen
        Assert.Equal("<s p=\"1\">Plain </s><s p=\"2\" t=\"500\">glowing</s>", subtitle.Paragraphs[1].Text);

        var head = YttHead.Parse(subtitle.Header);
        var pen = head.GetPen(2);
        Assert.NotNull(pen);
        Assert.True(pen.Italic);
        Assert.Equal(200, pen.Size);
        Assert.Equal(3, pen.EdgeType);
        Assert.Equal("#0000FF", pen.EdgeColor);
        Assert.Equal(1, head.GetWindowPosition(1)!.AnchorPoint);
        Assert.Equal(10, head.GetWindowPosition(1)!.Vertical);
        Assert.Equal(2, head.GetWindowStyle(1)!.Justify);
    }

    [Fact]
    public void RoundTripKeepsPensSpansAndTimes()
    {
        var subtitle = Load(YtSubConverterFile, out var format);
        var saved = format.ToText(subtitle, "test");
        var reloaded = Load(saved, out _);

        Assert.Equal(subtitle.Paragraphs.Count, reloaded.Paragraphs.Count);
        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            Assert.Equal(subtitle.Paragraphs[i].Text, reloaded.Paragraphs[i].Text);
            Assert.Equal(subtitle.Paragraphs[i].StartTime.TotalMilliseconds, reloaded.Paragraphs[i].StartTime.TotalMilliseconds);
            Assert.Equal(subtitle.Paragraphs[i].EndTime.TotalMilliseconds, reloaded.Paragraphs[i].EndTime.TotalMilliseconds);
            Assert.Equal(subtitle.Paragraphs[i].Style, reloaded.Paragraphs[i].Style);
            Assert.Equal(subtitle.Paragraphs[i].Region, reloaded.Paragraphs[i].Region);
            Assert.Equal(subtitle.Paragraphs[i].Effect, reloaded.Paragraphs[i].Effect);
        }

        Assert.Equal(subtitle.Header, reloaded.Header);
    }

    [Fact]
    public void WriterAppliesUploadWorkarounds()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<s p=\"1\">A</s><s p=\"2\">B</s>", 0, 1000) { Style = null });
        var head = new YttHead();
        head.Pens.Add(new YttPen { Id = 1, ForeColor = "#FFFFFF", ForeOpacity = 255 });
        head.Pens.Add(new YttPen { Id = 2, BackOpacity = 255 });
        subtitle.Header = head.ToHeader();

        var text = new YouTubeTimedText().ToText(subtitle, "test");

        Assert.DoesNotContain("\r", text);
        Assert.Contains("<pen id=\"0\" />", text);
        Assert.Contains("<wp id=\"0\"", text);
        Assert.Contains("<ws id=\"0\" />", text);
        Assert.Contains("fc=\"#FEFEFE\" fo=\"254\"", text);
        Assert.Contains("bo=\"254\"", text);
        Assert.DoesNotContain("255", text);
        Assert.Contains("<p t=\"1\" d=\"999\">", text);
        Assert.Contains("<s p=\"1\">A</s>​<s p=\"2\">B</s>", text);
    }

    [Fact]
    public void HtmlTagsBecomePens()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hi <i>there</i> <font color=\"red\">red</font>", 1000, 2000));
        subtitle.Paragraphs.Add(new Paragraph("<i>All italic</i>", 3000, 4000));

        var text = new YouTubeTimedText().ToText(subtitle, "test");
        var reloaded = Load(text, out _);
        var head = YttHead.Parse(reloaded.Header);

        var italicPen = head.Pens.Single(p => p.Italic == true);
        var redPen = head.Pens.Single(p => p.ForeColor == "#FF0000");
        Assert.Equal($"Hi <s p=\"{italicPen.Id}\">there</s> <s p=\"{redPen.Id}\">red</s>", reloaded.Paragraphs[0].Text);

        // a whole-line pen is a line pen, and the italic pen is reused
        Assert.Equal("All italic", reloaded.Paragraphs[1].Text);
        Assert.Equal(italicPen.Id.ToString(), reloaded.Paragraphs[1].Style);
    }

    [Fact]
    public void AssaAlignmentBecomesWindowPosition()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("{\\an8}Top", 1000, 2000));

        var reloaded = Load(new YouTubeTimedText().ToText(subtitle, "test"), out _);
        var head = YttHead.Parse(reloaded.Header);
        var wp = head.GetWindowPosition(reloaded.Paragraphs[0].Region);

        Assert.Equal("Top", reloaded.Paragraphs[0].Text);
        Assert.NotNull(wp);
        Assert.Equal(1, wp.AnchorPoint);
        Assert.Equal(50, wp.Horizontal);
        Assert.Equal(0, wp.Vertical);
    }

    [Fact]
    public void RemoveNativeFormattingTurnsPensIntoHtml()
    {
        var subtitle = Load(YtSubConverterFile, out var format);

        format.RemoveNativeFormatting(subtitle, new SubRip());

        Assert.Equal("{\\an8}Top line", subtitle.Paragraphs[0].Text);
        Assert.Equal("Plain <i><font color=\"#FF0000\">glowing</font></i>", subtitle.Paragraphs[1].Text);
        Assert.Null(subtitle.Header);
        Assert.Null(subtitle.Paragraphs[0].Style);
    }

    [Fact]
    public void RealScaleAndPositionFollowYouTube()
    {
        Assert.Equal(1.25, YttPen.GetRealScale(200), 3);
        Assert.Equal(0.75, YttPen.GetRealScale(0), 3);
        Assert.Equal(1.0, YttPen.GetRealScale(null), 3);
        Assert.Equal(200, YttPen.GetSizeFromRealScale(1.25));
        Assert.Equal(2, YttWindowPosition.GetRealPercent(0), 3);
        Assert.Equal(98, YttWindowPosition.GetRealPercent(100), 3);
        Assert.Equal(50, YttWindowPosition.GetCoordinateFromRealPercent(50));
    }

    [Fact]
    public void PreviewConvertsPositionsPensAndKaraoke()
    {
        var subtitle = Load(YtSubConverterFile, out _);

        var assa = YttToAssa.Convert(subtitle, 1280, 720);

        Assert.Contains("PlayResY: 720", assa.Header);
        Assert.StartsWith("{\\an8\\pos(640,83.5)}", assa.Paragraphs[0].Text);
        Assert.Contains("Top line", assa.Paragraphs[0].Text);

        var second = assa.Paragraphs[1].Text;
        Assert.Contains("\\i1", second);
        Assert.Contains("\\1c&H0000FF&", second); // red, ASSA is BGR
        Assert.Contains("\\t(500,501,", second);
        Assert.Contains("glowing", second);
    }

    /// <summary>YTSubConverter's karaoke: s elements in the line pen with only a start time.</summary>
    [Fact]
    public void KaraokeSpansWithoutPenAreKept()
    {
        var file = "<timedtext format=\"3\"><head><pen id=\"15\" fs=\"0\" /></head><body>" +
                   "<p t=\"36000\" d=\"4000\" p=\"15\"><s>Ka</s><s t=\"1000\">ra</s><s t=\"2000\">o</s></p></body></timedtext>";
        var subtitle = Load(file, out var format);

        Assert.Equal("Ka<s t=\"1000\">ra</s><s t=\"2000\">o</s>", subtitle.Paragraphs[0].Text);

        var saved = format.ToText(subtitle, "test");
        Assert.Contains("<s p=\"15\" t=\"1000\">ra</s>", saved);
        Assert.Equal(subtitle.Paragraphs[0].Text, Load(saved, out _).Paragraphs[0].Text);
    }

    [Fact]
    public void PlainTextFileWithoutHeadStillWorks()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Line one" + Environment.NewLine + "Line two & more", 1000, 2000));

        var text = new YouTubeTimedText().ToText(subtitle, "test");
        var reloaded = Load(text, out _);

        Assert.Contains("Line two &amp; more", text);
        Assert.Equal(subtitle.Paragraphs[0].Text, reloaded.Paragraphs[0].Text);
    }
}
