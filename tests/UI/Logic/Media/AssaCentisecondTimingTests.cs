using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

/// <summary>
/// The ASSA writer rounds to the nearest centisecond, so a frame-aligned start like 0.167 s
/// (frame 4 at 23.976 fps, pts 0.16683 s) became 0.17 s and libass skipped the line's first
/// frame in the mpv preview and in burn-in (issue #15520).
/// </summary>
public class AssaCentisecondTimingTests
{
    [Theory]
    [InlineData(167, 160)]
    [InlineData(125, 120)]
    [InlineData(1043, 1040)]
    [InlineData(1040, 1040)]
    [InlineData(1129.9999999, 1130)]
    [InlineData(0, 0)]
    public void FloorToCentiseconds_RoundsDown(double input, double expected)
    {
        Assert.Equal(expected, AssaCentisecondTiming.FloorToCentiseconds(input), 6);
    }

    [Fact]
    public void FrameAlignedStart_NeverLandsAfterTheFrame_NorOnThePreviousOne()
    {
        foreach (var fps in new[] { 24000.0 / 1001, 24, 25, 30000.0 / 1001, 30, 50, 60000.0 / 1001 })
        {
            for (var frame = 1; frame < 5000; frame++)
            {
                var pts = frame * 1000.0 / fps;
                var floored = AssaCentisecondTiming.FloorToCentiseconds(Math.Round(pts));
                Assert.True(floored <= Math.Round(pts), $"{fps} fps frame {frame}: {floored} after {pts}");
                Assert.True(floored > (frame - 1) * 1000.0 / fps, $"{fps} fps frame {frame}: {floored} on previous frame");
            }
        }
    }

    [Fact]
    public void FloorToCentiseconds_WritesAssaTimesNotAfterTheSource()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hi", 167, 2085));

        AssaCentisecondTiming.FloorToCentiseconds(subtitle);
        var text = subtitle.ToText(new AdvancedSubStationAlpha());

        Assert.Contains("0:00:00.16,0:00:02.08", text);
    }

    [Fact]
    public void FloorToCentiseconds_ReplacesChangedParagraphsInsteadOfMutatingThem()
    {
        // The merged secondary subtitle shares the caller's paragraphs with the preview copy.
        var shared = new Paragraph("Shared", 167, 2085);
        var untouched = new Paragraph("Exact", 3000, 4000);
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(shared);
        subtitle.Paragraphs.Add(untouched);

        AssaCentisecondTiming.FloorToCentiseconds(subtitle);

        Assert.Equal(167, shared.StartTime.TotalMilliseconds);
        Assert.Equal(2085, shared.EndTime.TotalMilliseconds);
        Assert.NotSame(shared, subtitle.Paragraphs[0]);
        Assert.Equal(160, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(2080, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Shared", subtitle.Paragraphs[0].Text);
        Assert.Same(untouched, subtitle.Paragraphs[1]);
    }

    [Fact]
    public void FloorToCentiseconds_KeepsMaxTime()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Open", 1005, TimeCode.MaxTimeTotalMilliseconds));

        AssaCentisecondTiming.FloorToCentiseconds(subtitle);

        Assert.True(subtitle.Paragraphs[0].EndTime.IsMaxTime);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
    }
}
