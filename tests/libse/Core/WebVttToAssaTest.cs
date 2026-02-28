using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Core;

public class WebVttToAssaTest
{
    [Fact]
    public void TestStyles1()
    {
        var subtitle = new Subtitle();
        subtitle.Header = "WEBVTT\r\n\r\nSTYLE\r\n::cue(.background-color_transparent) {\r\n  background-color: rgba(255,255,255,0.0);\r\n}\r\n::cue(.color_EBEBEB) {\r\n  color: rgba(235,235,235,1.000000);\r\n}\r\n::cue(.font-family_Arial) {\r\n  font-family: Arial;\r\n}\r\n::cue(.font-style_normal) {\r\n  font-style: normal;\r\n}\r\n::cue(.font-weight_normal) {\r\n  font-weight: normal;\r\n}\r\n::cue(.text-shadow_#101010-3px) {\r\n  text-shadow: #101010 3px;\r\n}\r\n::cue(.font-style_italic) {\r\n  font-style: italic;\r\n}";
        var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);
        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(converted.Header);

        Assert.Equal(".background-color_transparent", styles[1].Name);
        Assert.Equal(".color_EBEBEB", styles[2].Name);
        Assert.Equal(".font-family_Arial", styles[3].Name);
        Assert.Equal(".font-style_normal", styles[4].Name);
        Assert.Equal(".font-weight_normal", styles[5].Name);
        Assert.Equal(".text-shadow_#101010-3px", styles[6].Name);
        Assert.Equal(".font-style_italic", styles[7].Name);

        Assert.Equal(235, styles[2].Primary.Red);
        Assert.Equal("Arial", styles[3].FontName);
        Assert.False(styles[4].Italic);
        Assert.False(styles[5].Bold);
        Assert.Equal(3, styles[6].ShadowWidth);
        Assert.True(styles[7].Italic);
    }

    [Fact]
    public void TestStyles2()
    {
        var subtitle = new Subtitle();
        subtitle.Header = "STYLE\r\n::cue(.styledotEAC118) { color:#EAC118 }\r\n::cue(.styledotaqua) { color:aqua }\r\n::cue(.styledotaquadotitalic) { color:aqua;font-style:italic }\r\n::cue(.styledotitalic) { font-style:italic }\r\n::cue(.styledotEAC118dotitalic) { color:#EAC118;font-style:italic }";
        var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);
        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(converted.Header);

        Assert.Equal(".styledotEAC118", styles[1].Name);
        Assert.Equal(".styledotaqua", styles[2].Name);
        Assert.Equal(".styledotaquadotitalic", styles[3].Name);
        Assert.Equal(".styledotitalic", styles[4].Name);
        Assert.Equal(".styledotEAC118dotitalic", styles[5].Name);

        Assert.Equal(234, styles[1].Primary.Red);
        Assert.Equal(193, styles[1].Primary.Green);
        Assert.Equal(24, styles[1].Primary.Blue);
        Assert.Equal(0, styles[2].Primary.Red);
        Assert.Equal(255, styles[2].Primary.Green);
        Assert.Equal(255, styles[2].Primary.Blue);
        Assert.True(styles[3].Italic);
        Assert.True(styles[4].Italic);
        Assert.True(styles[5].Italic);
        Assert.Equal(234, styles[5].Primary.Red);
        Assert.Equal(193, styles[5].Primary.Green);
        Assert.Equal(24, styles[5].Primary.Blue);
    }

    [Fact]
    public void TestLineStyles1()
    {
        var subtitle = new Subtitle();
        subtitle.Header = "STYLE\r\n::cue(.styledotEAC118) { color:#EAC118 }";
        subtitle.Paragraphs.Add(new Paragraph("<c.styledotEAC118>Hi</c>", 0, 0));
        var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);

        Assert.Equal("Hi", converted.Paragraphs[0].Text);
        Assert.Equal(".styledotEAC118", converted.Paragraphs[0].Extra);
    }

    [Fact]
    public void TestLineStyles2()
    {
        var subtitle = new Subtitle();
        subtitle.Header = "STYLE\r\n::cue(.styleItalic) { font-style:italic }\r\n::cue(.styleColor123456) {  color:#123456 }";
        subtitle.Paragraphs.Add(new Paragraph("<c.styleItalic.styleColor123456>Hi</c>", 0, 0));
        var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);

        Assert.Equal("{\\c&H563412\\i1}Hi", converted.Paragraphs[0].Text);
    }

    [Fact]
    public void TestItalicInline()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hallo <i>italic</i> world.", 0, 0));
        var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);
        var text = converted.ToText(new AdvancedSubStationAlpha());
        Assert.Equal("Hallo {\\i1}italic{\\i0} world.", converted.Paragraphs[0].Text);
        Assert.Contains("Hallo {\\i1}italic{\\i0} world.", text);
    }

    [Fact]
    public void TestBoldInline()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hallo <b>bold</b> world.", 0, 0));
        var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);
        var text = converted.ToText(new AdvancedSubStationAlpha());
        Assert.Contains("Hallo {\\b1}bold{\\b0} world.", text);
    }

    [Fact]
    public void TestUnderlineInline()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hallo <u>underline</u> world.", 0, 0));
        var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);
        var text = converted.ToText(new AdvancedSubStationAlpha());
        Assert.Contains("Hallo {\\u1}underline{\\u0} world.", text);
    }
}
