using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;

namespace LibSETests.Forms;

/// <summary>
/// Issue #17149: a 25 fps video starting at 0.02 s has its frames - and so its shot changes - at
/// 20, 60, 100... ms on the player's timeline. The beautifier's frame grid counted from 0, so a cue
/// snapped to a shot change landed half a frame (20 ms) before or after it.
/// </summary>
public class TimeCodesBeautifierTimelineOffsetTest
{
    private const double FrameRate = 25.0;
    private const double FrameMs = 40.0;
    private const double OffsetMs = 20.0;
    private const double ShotChangeMs = 24_300; // frame 607 of a video starting at 20 ms

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Line", 21_420, 24_200));
        return subtitle;
    }

    private static double ExpectedOutCueMs() =>
        ShotChangeMs - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap * FrameMs;

    private static void AssertOnOffsetGrid(double ms)
    {
        var frames = (ms - OffsetMs) / FrameMs;
        Assert.True(Math.Abs(frames - Math.Round(frames)) < 0.001, $"{ms} ms is not on the video's frame grid");
    }

    [Fact]
    public void FrameGrid_StartsAtTimelineOffset_SnapsOutCueToShotChange()
    {
        var subtitle = MakeSubtitle();

        new TimeCodesBeautifier(subtitle, FrameRate, new List<double>(), new List<double> { ShotChangeMs / 1000.0 }, OffsetMs).Beautify();

        var p = subtitle.Paragraphs[0];
        AssertOnOffsetGrid(p.StartTime.TotalMilliseconds);
        AssertOnOffsetGrid(p.EndTime.TotalMilliseconds);
        Assert.Equal(ExpectedOutCueMs(), p.EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void ExactTimeCodes_AreShiftedByTimelineOffset()
    {
        // ffmpeg's showinfo reports frame times from 0, whatever the container start time.
        var timeCodes = Enumerable.Range(0, 1000).Select(i => i * FrameMs / 1000.0).ToList();
        var subtitle = MakeSubtitle();

        new TimeCodesBeautifier(subtitle, FrameRate, timeCodes, new List<double> { ShotChangeMs / 1000.0 }, OffsetMs).Beautify();

        var p = subtitle.Paragraphs[0];
        AssertOnOffsetGrid(p.StartTime.TotalMilliseconds);
        AssertOnOffsetGrid(p.EndTime.TotalMilliseconds);
        Assert.Equal(ExpectedOutCueMs(), p.EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void CueBeforeFirstFrame_IsNotMovedBeforeZero()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Line", 0, 2_000));

        new TimeCodesBeautifier(subtitle, FrameRate, new List<double>(), new List<double>(), OffsetMs).Beautify();

        Assert.True(subtitle.Paragraphs[0].StartTime.TotalMilliseconds >= 0);
        AssertOnOffsetGrid(subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void NoOffset_KeepsZeroBasedGrid()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Line", 1_013, 3_987));

        new TimeCodesBeautifier(subtitle, FrameRate, new List<double>(), new List<double>()).Beautify();

        foreach (var ms in new[] { subtitle.Paragraphs[0].StartTime.TotalMilliseconds, subtitle.Paragraphs[0].EndTime.TotalMilliseconds })
        {
            var frames = ms / FrameMs;
            Assert.True(Math.Abs(frames - Math.Round(frames)) < 0.001, $"{ms} ms is not on the 0-based grid");
        }
    }
}
