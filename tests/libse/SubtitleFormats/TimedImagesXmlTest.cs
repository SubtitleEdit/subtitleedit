using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class TimedImagesXmlTest
{
    private const string Sample =
        "<TimedImages targetWidth=\"720\" targetHeight=\"576\" aspectRatio=\"16:9\">\r\n" +
        "<I s=\"0.600\" e=\"3.720\" x=\"268\" y=\"458\" w=\"218\" h=\"58\" i=\"AYZ-1.png\" />\r\n" +
        "<I s=\"3.840\" e=\"6.960\" x=\"290\" y=\"400\" w=\"176\" h=\"111\" i=\"AYZ-2.png\" />\r\n" +
        "</TimedImages>\r\n";

    [Fact]
    public void LoadsImageFileNamesTimesAndPositions()
    {
        var format = new TimedImagesXml();
        var lines = Sample.SplitToLines();
        var subtitle = new Subtitle();

        Assert.True(format.IsMine(lines, "AYZ.xml"));
        format.LoadSubtitle(subtitle, lines, "AYZ.xml");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("AYZ-1.png", subtitle.Paragraphs[0].Text);
        Assert.Equal(600, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(3720, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal("268,458", subtitle.Paragraphs[0].Extra);
        Assert.Equal("AYZ-2.png", subtitle.Paragraphs[1].Text);

        Assert.True(TimedImagesXml.TryGetVideoSize(subtitle.Header, out var width, out var height));
        Assert.Equal(720, width);
        Assert.Equal(576, height);
    }

    /// <summary>It is an image list, so ordinary subtitle detection must not pick it.</summary>
    [Fact]
    public void IsNotATextFormat()
    {
        Assert.Null(Subtitle.Parse(Sample.SplitToLines(), ".xml"));
        Assert.DoesNotContain(SubtitleFormat.AllSubtitleFormats, f => f is TimedImagesXml);
        Assert.Contains(SubtitleFormat.GetTextOtherFormats(), f => f is TimedImagesXml);
    }

    [Fact]
    public void OtherXmlIsNotClaimed()
    {
        var bdn = "<BDN Version=\"0.93\"><Events><Event InTC=\"00:00:01:00\" OutTC=\"00:00:03:00\"><Graphic>0001.png</Graphic></Event></Events></BDN>";
        Assert.False(new TimedImagesXml().IsMine(bdn.SplitToLines(), "a.xml"));
    }
}
