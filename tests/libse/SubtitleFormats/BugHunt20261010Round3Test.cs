using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Guard tests for the 2026-10-10 random bug hunt (round 3): save/load round trips that lost
/// alignment, colors, bold, italics, text after a backslash, the D-Cinema font id and the
/// 1000/1001 frame rate.
/// </summary>
[Collection("NonParallelTests")]
public class BugHunt20261010Round3Test
{
    private static Subtitle RoundTrip(SubtitleFormat format, params Paragraph[] paragraphs)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.AddRange(paragraphs);
        var text = format.ToText(subtitle, "title");
        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, new List<string>(text.SplitToLines()), "x" + format.Extension);
        return loaded;
    }

    [Fact]
    public void MsOfficeWorkbook_KeepsAlignmentAndActor()
    {
        var loaded = RoundTrip(new MsOfficeWorkbook(), new Paragraph("{\\an8}Hello", 1000, 2000) { Actor = "Bob" });

        Assert.Equal("{\\an8}Hello", loaded.Paragraphs[0].Text);
        Assert.Equal("Bob", loaded.Paragraphs[0].Actor);
    }

    [Fact]
    public void DCinemaInterop_MixedItalicKeepsColorTag()
    {
        Assert.Equal("<i>a</i><font color=\"#FF0000\">b</font>",
            DCinemaInterop.FixInvalidItalicTags("<i>a<non-italic><font color=\"#FF0000\">b</font></non-italic></i>"));
    }

    [Fact]
    public void DCinemaInterop_ReadsFontId()
    {
        var ss = Configuration.Settings.SubtitleSettings;
        var old = ss.CurrentDCinemaFontId;
        try
        {
            ss.CurrentDCinemaFontId = "Font1";
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><DCSubtitle Version=\"1.0\"><SubtitleID>4EB245B8-4D3A-4158-9516-95DD20E8322E</SubtitleID><MovieTitle>x</MovieTitle><ReelNumber>1</ReelNumber><Language>English</Language><LoadFont Id=\"MyFont\" URI=\"a.ttf\"/>" +
                      "<Font Id=\"MyFont\" Color=\"FFFFFFFF\" Effect=\"border\" EffectColor=\"FF000000\" Italic=\"no\" Underlined=\"no\" Script=\"normal\" Size=\"42\">" +
                      "<Subtitle SpotNumber=\"1\" TimeIn=\"00:00:01:000\" TimeOut=\"00:00:02:000\" FadeUpTime=\"20\" FadeDownTime=\"20\"><Text VAlign=\"bottom\" VPosition=\"10\">a</Text></Subtitle></Font></DCSubtitle>";
            var subtitle = new Subtitle();
            new DCinemaInterop().LoadSubtitle(subtitle, new List<string>(xml.SplitToLines()), "x.xml");

            Assert.Single(subtitle.Paragraphs);
            Assert.Equal("MyFont", ss.CurrentDCinemaFontId);
        }
        finally
        {
            ss.CurrentDCinemaFontId = old;
        }
    }

    [Theory]
    [InlineData("C:\\path\\x y")]
    [InlineData("<i>a</i> b")]
    [InlineData("<i>L1" + "\r\n" + "L2</i>")]
    public void ProjectionSubtitleList_RoundTrip(string text)
    {
        text = text.Replace("\r\n", Environment.NewLine);
        var loaded = RoundTrip(new ProjectionSubtitleList(), new Paragraph(text, 1000, 2000));

        Assert.Equal(text, loaded.Paragraphs[0].Text);
    }

    [Fact]
    public void FinalCutProTextXml_KeepsBold()
    {
        var loaded = RoundTrip(new FinalCutProTestXml(), new Paragraph("<b>Bold</b>", 1000, 2000));

        Assert.Equal("<b>Bold</b>", loaded.Paragraphs[0].Text);
    }

    [Fact]
    public void TimedTextImsc11_FrameRateMultiplierScalesTheRate()
    {
        var general = Configuration.Settings.General;
        var old = general.CurrentFrameRate;
        try
        {
            general.CurrentFrameRate = 59.94;
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hello", 1000, 2000));
            var text = new TimedTextImsc11().ToText(subtitle, "title");
            Assert.Contains("1000 1001", text);

            general.CurrentFrameRate = 25;
            new TimedTextImsc11().LoadSubtitle(new Subtitle(), new List<string>(text.SplitToLines()), "x.xml");

            Assert.Equal(59.94, general.CurrentFrameRate, 2);
        }
        finally
        {
            general.CurrentFrameRate = old;
        }
    }
}
