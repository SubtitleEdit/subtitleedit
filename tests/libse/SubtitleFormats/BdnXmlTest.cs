using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class BdnXmlTest
{
    private static string MakeBdnXml(string videoFormat, string graphicAttributes)
    {
        return """
               <?xml version="1.0" encoding="UTF-8"?>
               <BDN Version="0.93">
               <Description>
               <Name Title="subtitle_exp" Content=""/>
               <Language Code="eng"/>
               <Format VideoFormat="@VIDEOFORMAT@" FrameRate="25" DropFrame="false"/>
               <Events Type="Graphic" FirstEventInTC="00:00:01:00" LastEventOutTC="00:00:03:00" NumberofEvents="1"/>
               </Description>
               <Events>
               <Event InTC="00:00:01:00" OutTC="00:00:03:00" Forced="False">
               <Graphic Width="400" Height="60" @GRAPHICATTRIBUTES@>0001.png</Graphic>
               </Event>
               </Events>
               </BDN>
               """
            .Replace("@VIDEOFORMAT@", videoFormat)
            .Replace("@GRAPHICATTRIBUTES@", graphicAttributes);
    }

    private static Subtitle Load(string xml)
    {
        var subtitle = new Subtitle();
        new BdnXml().LoadSubtitle(subtitle, xml.SplitToLines(), "test.xml");
        return subtitle;
    }

    [Fact]
    public void LoadsGraphicFileNameAndPosition()
    {
        var subtitle = Load(MakeBdnXml("1080p", "X=\"760\" Y=\"930\""));

        var p = Assert.Single(subtitle.Paragraphs);
        Assert.Equal("0001.png", p.Text);
        Assert.Equal("760,930", p.Extra);
        Assert.Equal(1000, p.StartTime.TotalMilliseconds);
        Assert.Equal(3000, p.EndTime.TotalMilliseconds);
    }

    // "X" without "Y" used to throw a NullReferenceException, which dropped the whole event.
    [Fact]
    public void LoadsGraphicWithOnlyOneOfXAndY()
    {
        var subtitle = Load(MakeBdnXml("1080p", "X=\"760\""));

        var p = Assert.Single(subtitle.Paragraphs);
        Assert.Equal("0001.png", p.Text);
        Assert.True(string.IsNullOrEmpty(p.Extra));
    }

    [Theory]
    [InlineData("1080p", 1920, 1080)]
    [InlineData("1080i", 1920, 1080)]
    [InlineData("720p", 1280, 720)]
    [InlineData("576i", 720, 576)]
    [InlineData("480p", 720, 480)]
    [InlineData("2160p", 3840, 2160)]
    [InlineData("1440x1080", 1440, 1080)] // the Subtitle Edit exporter writes this for non-standard sizes
    public void ReadsVideoSize(string videoFormat, int expectedWidth, int expectedHeight)
    {
        var subtitle = Load(MakeBdnXml(videoFormat, "X=\"0\" Y=\"0\""));

        Assert.True(BdnXml.TryGetVideoSize(subtitle.Header, out var width, out var height));
        Assert.Equal(expectedWidth, width);
        Assert.Equal(expectedHeight, height);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-size")]
    public void ReturnsFalseForUnknownVideoSize(string videoFormat)
    {
        var subtitle = Load(MakeBdnXml(videoFormat, "X=\"0\" Y=\"0\""));

        Assert.False(BdnXml.TryGetVideoSize(subtitle.Header, out _, out _));
    }

    [Fact]
    public void ReturnsFalseForNonBdnXml()
    {
        Assert.False(BdnXml.TryGetVideoSize("<Subtitle><Paragraph/></Subtitle>", out _, out _));
        Assert.False(BdnXml.TryGetVideoSize(null, out _, out _));
    }

    private static Paragraph LoadOneEvent(string frameRate, string dropFrame, string inTc, string outTc)
    {
        var xml = $"""
                   <?xml version="1.0" encoding="UTF-8"?>
                   <BDN Version="0.93">
                   <Description>
                   <Format VideoFormat="1080i" FrameRate="{frameRate}" DropFrame="{dropFrame}"/>
                   </Description>
                   <Events>
                   <Event InTC="{inTc}" OutTC="{outTc}" Forced="False">
                   <Graphic Width="400" Height="60" X="760" Y="930">0001.png</Graphic>
                   </Event>
                   </Events>
                   </BDN>
                   """;
        return Assert.Single(Load(xml).Paragraphs);
    }

    // 29.97 DF: 01:00:00;00 is frame 107892 (3.6 s less than the 108000 its label reads as)
    [Theory]
    [InlineData("00:00:01:00", 1001.0)]
    [InlineData("00:01:00;02", 1800 * 1001 / 30.0)]
    [InlineData("00:10:00;00", 17982 * 1001 / 30.0)]
    [InlineData("01:00:00;00", 107892 * 1001 / 30.0)]
    public void DropFrame2997_CountsDroppedFrameNumbers(string inTc, double expectedMs)
    {
        var p = LoadOneEvent("29.97", "true", inTc, "01:00:10;00");

        Assert.Equal(expectedMs, p.StartTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void DropFrame5994_DropsFourPerMinute()
    {
        var p = LoadOneEvent("59.94", "True", "00:01:00:04", "00:01:01:00");

        Assert.Equal(3600 * 1001 / 60.0, p.StartTime.TotalMilliseconds, 3);
    }
}
