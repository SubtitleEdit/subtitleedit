using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class TimedTextImscRosettaTest
{
    [Fact]
    public void Multiple_Lines()
    {
        var sut = new TimedTextImscRosetta();

        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:ttm=\"http://www.w3.org/ns/ttml#metadata\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" xmlns:xml=\"http://www.w3.org/XML/1998/namespace\" xmlns:ebutts=\"urn:ebu:tt:style\" xmlns:itts=\"http://www.w3.org/ns/ttml/profile/imsc1#styling\" xmlns:rosetta=\"https://github.com/imsc-rosetta/specification\" ttp:timeBase=\"media\" ttp:cellResolution=\"30 15\" xml:space=\"preserve\" ttp:frameRate=\"25\" ttp:frameRateMultiplier=\"1 1\" xml:lang=\"en-US\">\r\n <head>\r\n  <metadata>\r\n   <rosetta:format>imsc-rosetta</rosetta:format>\r\n   <rosetta:version>0.0.0</rosetta:version>\r\n  </metadata>\r\n  <styling>\r\n   <style xml:id=\"r_default\" tts:wrapOption=\"noWrap\" tts:overflow=\"visible\" tts:backgroundColor=\"#00000000\" tts:showBackground=\"whenActive\" tts:fontStyle=\"normal\" tts:fontWeight=\"normal\" tts:fontFamily=\"proportionalSansSerif\" style=\"_r_default\"/>\r\n   <style xml:id=\"d_default\" style=\"_d_default\"/>\r\n   <style xml:id=\"d_outline\" style=\"s_outlineblack\"/>\r\n   <style xml:id=\"p_al_center\" ebutts:multiRowAlign=\"center\" tts:textAlign=\"center\"/>\r\n   <style xml:id=\"s_fg_white\" tts:color=\"#FFFFFF\"/>\r\n   <style xml:id=\"s_outlineblack\" tts:textOutline=\"#000000 0.05em\"/>\r\n   <style xml:id=\"p_font2\" tts:fontFamily=\"proportionalSansSerif\" tts:lineHeight=\"125%\" tts:fontSize=\"100%\"/>\r\n   <style xml:id=\"_d_default\" style=\"d_outline\"/>\r\n   <style xml:id=\"_r_default\" tts:fontSize=\"5rh\" tts:lineHeight=\"125%\" ebutts:linePadding=\"0.25c\" itts:fillLineGap=\"false\" tts:luminanceGain=\"1.0\" style=\"s_fg_white p_al_center\"/>\r\n   <style xml:id=\"_r_quantisationregion\" tts:origin=\"10% 10%\" tts:extent=\"80% 80%\" tts:fontSize=\"5.333rh\" tts:lineHeight=\"125%\"/>\r\n  </styling>\r\n  <layout>\r\n   <region xml:id=\"R0\" tts:origin=\"10% 10%\" tts:extent=\"80% 80%\" tts:displayAlign=\"after\" style=\"r_default\"/>\r\n  </layout>\r\n </head>\r\n <body>\r\n  <div xml:id=\"e_0\" region=\"R0\" begin=\"09:57:30.000\" end=\"09:57:35.000\" style=\"d_default\">\r\n   <p style=\"p_font2\"><span>I am line one of two lines</span></p>\r\n   <p style=\"p_font2\"><span>I am line two of two lines</span></p>\r\n  </div>\r\n  <div xml:id=\"e_1\" region=\"R0\" begin=\"09:57:39.840\" end=\"09:57:43.840\" style=\"d_default\">\r\n   <p style=\"p_font2\"><span>I am line one of three lines</span></p>\r\n   <p style=\"p_font2\"><span>I am line two of three lines</span></p>\r\n   <p style=\"p_font2\"><span>I am line three of three lines</span></p>\r\n  </div>\r\n </body>\r\n</tt>";
        var subtitle = new Subtitle();
        sut.LoadSubtitle(subtitle, xml.SplitToLines(), "test.xml");
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(2, subtitle.Paragraphs[0].Text.SplitToLines().Count);
        Assert.Equal(3, subtitle.Paragraphs[1].Text.SplitToLines().Count);
    }

    [Fact]
    public void Italic_Word()
    {
        var sut = new TimedTextImscRosetta();

        // make xml
        var subtitle = new Subtitle();
        var text = "This line has one <i>italic</i> word.";
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 2000));
        var xml = sut.ToText(subtitle, "test");

        // load xml
        subtitle = new Subtitle();
        sut.LoadSubtitle(subtitle, xml.SplitToLines(), "test.xml");
        Assert.Equal(text, subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void One_Line_Italic_One_Line_Bold()
    {
        var sut = new TimedTextImscRosetta();

        // make xml
        var subtitle = new Subtitle();
        var text = "<i>This is italic.</i>" + Environment.NewLine + "<b>This line is Bold.</b>";
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 2000));
        var xml = sut.ToText(subtitle, "test");

        // load xml
        subtitle = new Subtitle();
        sut.LoadSubtitle(subtitle, xml.SplitToLines(), "test.xml");
        Assert.Equal(text, subtitle.Paragraphs[0].Text);
    }


    [Fact]
    public void Italic_Inside_Bold()
    {
        var sut = new TimedTextImscRosetta();

        // make xml
        var subtitle = new Subtitle();
        var text = "<b>This is bold and <i>italic</i></b> text.";
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 2000)); 
        var xml = sut.ToText(subtitle, "test");

        // load xml
        subtitle = new Subtitle();
        sut.LoadSubtitle(subtitle, xml.SplitToLines(), "test.xml");
        Assert.Equal(text, subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void TopAlignment_An8()
    {
        var sut = new TimedTextImscRosetta();

        // make xml with top alignment
        var subtitle = new Subtitle();
        var text = "{\\an8}This text is top-aligned.";
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 2000));
        var xml = sut.ToText(subtitle, "test");

        // load xml
        subtitle = new Subtitle();
        sut.LoadSubtitle(subtitle, xml.SplitToLines(), "test.xml");
        
        // verify that the text still has top alignment tag
        Assert.Equal(text, subtitle.Paragraphs[0].Text);
    }
}