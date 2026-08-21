using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class DaVinciResolveMarkerEdlTest
{
    private const string ResolveSample = """
TITLE: Timeline 1
FCM: NON-DROP FRAME

001  001      V     C        01:00:05:00 01:00:05:01 01:00:05:00 01:00:05:01
 |C:ResolveColorCyan |M:First marker |D:120

002  001      V     C        01:00:10:12 01:00:10:13 01:00:10:12 01:00:10:13
 |C:ResolveColorBlue |M:Second one |D:1
""";

    [Fact]
    public void LoadsResolveMarkerExport()
    {
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        Configuration.Settings.General.CurrentFrameRate = 25;
        try
        {
            var subtitle = new Subtitle();
            new DaVinciResolveMarkerEdl().LoadSubtitle(subtitle, ResolveSample.SplitToLines(), null);

            Assert.Equal(2, subtitle.Paragraphs.Count);
            Assert.Equal("First marker", subtitle.Paragraphs[0].Text);
            Assert.Equal(new TimeCode(1, 0, 5, 0).TotalMilliseconds, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
            Assert.Equal(120 * 40, subtitle.Paragraphs[0].DurationTotalMilliseconds, 3); // |D:120 frames at 25 fps
            Assert.Equal("Second one", subtitle.Paragraphs[1].Text);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }
    }

    [Fact]
    public void RoundTripsTimesAndText()
    {
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        Configuration.Settings.General.CurrentFrameRate = 25;
        try
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Chapter one starts.", 5000, 8000));
            subtitle.Paragraphs.Add(new Paragraph("Late marker.", 3600000, 3603000));

            var format = new DaVinciResolveMarkerEdl();
            var text = format.ToText(subtitle, "Timeline 1");
            var loaded = new Subtitle();
            format.LoadSubtitle(loaded, text.SplitToLines(), null);

            Assert.Equal(2, loaded.Paragraphs.Count);
            Assert.Equal("Chapter one starts.", loaded.Paragraphs[0].Text);
            Assert.Equal(5000, loaded.Paragraphs[0].StartTime.TotalMilliseconds, 3);
            Assert.Equal(8000, loaded.Paragraphs[0].EndTime.TotalMilliseconds, 3);
            Assert.Equal(3600000, loaded.Paragraphs[1].StartTime.TotalMilliseconds, 3);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }
    }

    [Fact]
    public void MarkerEdlWinsDetectionAndCutListStaysEdl()
    {
        var markerLines = ResolveSample.SplitToLines();
        foreach (var format in SubtitleFormat.AllSubtitleFormats)
        {
            if (format.IsMine(markerLines, "markers.edl"))
            {
                Assert.Equal(DaVinciResolveMarkerEdl.NameOfFormat, format.Name);
                break;
            }
        }

        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("A line of text here.", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("Another line of text.", 4000, 6000));
        var cutList = new Edl().ToText(subtitle, "t").SplitToLines();
        foreach (var format in SubtitleFormat.AllSubtitleFormats)
        {
            if (format.IsMine(cutList, "cut.edl"))
            {
                Assert.Equal(new Edl().Name, format.Name);
                break;
            }
        }
    }
}
