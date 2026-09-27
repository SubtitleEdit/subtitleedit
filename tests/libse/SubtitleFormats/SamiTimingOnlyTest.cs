using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Xunit;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// A SAMI file can be a timing template - every SYNC empty, "&amp;nbsp;" SYNCs clearing the
/// screen. It was rejected as having no subtitles; the timing is now kept as empty subtitles.
/// </summary>
public class SamiTimingOnlyTest
{
    private const string TimingOnly =
        "<SAMI>\r\n<HEAD>\r\n<SAMIParam>\r\n  Metrics {time:ms;}\r\n</SAMIParam>\r\n" +
        "<STYLE TYPE=\"text/css\">\r\n<!--\r\n.ENUSCC { Name: English; lang: en-US; SAMIType: CC; }\r\n-->\r\n</STYLE>\r\n</HEAD>\r\n<BODY>\r\n" +
        "<SYNC Start=2984><P Class=ENUSCC>\r\n" +
        "<SYNC Start=4345><P Class=ENUSCC>&nbsp;\r\n" +
        "<SYNC Start=4667><P Class=ENUSCC>\r\n" +
        "<SYNC Start=6447><P Class=ENUSCC>&nbsp;\r\n" +
        "<SYNC Start=7270><P Class=ENUSCC>\r\n" +
        "<SYNC Start=11151><P Class=ENUSCC>&nbsp;\r\n" +
        "</BODY>\r\n</SAMI>\r\n";

    [Fact]
    public void TimingOnlyFileLoadsEmptySubtitles()
    {
        var format = new Sami();
        var lines = TimingOnly.SplitToLines();
        var subtitle = new Subtitle();

        Assert.True(format.IsMine(lines, "a.smi"));
        format.LoadSubtitle(subtitle, lines, "a.smi");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.All(subtitle.Paragraphs, p => Assert.Equal(string.Empty, p.Text));
        Assert.Equal(2984, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(4345, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(7270, subtitle.Paragraphs[2].StartTime.TotalMilliseconds);
        Assert.Equal(11151, subtitle.Paragraphs[2].EndTime.TotalMilliseconds);
    }

    /// <summary>Files with text keep skipping their clear-screen SYNCs as before.</summary>
    [Fact]
    public void FileWithTextIsUnchanged()
    {
        var withText = TimingOnly.Replace("<SYNC Start=4667><P Class=ENUSCC>", "<SYNC Start=4667><P Class=ENUSCC>Hello");
        var subtitle = new Subtitle();
        new Sami().LoadSubtitle(subtitle, withText.SplitToLines(), "a.smi");

        var paragraph = Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Hello", paragraph.Text);
        Assert.Equal(4667, paragraph.StartTime.TotalMilliseconds);
        Assert.Equal(6447, paragraph.EndTime.TotalMilliseconds);
    }
}
