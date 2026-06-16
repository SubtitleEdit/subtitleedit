using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;

namespace UITests.Features.Sync.PointSyncViaOther;

public class PointSyncerTests
{
    private static SubtitleLineViewModel Line(double startMs, double endMs) => new()
    {
        Text = "Hello",
        StartTime = TimeSpan.FromMilliseconds(startMs),
        EndTime = TimeSpan.FromMilliseconds(endMs),
    };

    [Fact]
    public void SinglePoint_ShiftsBothStartAndEnd_PreservingDuration()
    {
        var subtitles = new List<SubtitleLineViewModel>
        {
            Line(1000, 3000),
            Line(5000, 6000),
        };

        // One sync point: the line currently at 1000 ms should move to 5000 ms
        // (a +4000 ms shift applied to the whole subtitle).
        var syncPoint = new SyncPoint(Line(1000, 3000), 0, Line(5000, 6000), 0);

        var result = PointSyncer.PointSync(subtitles, new List<SyncPoint> { syncPoint });

        // Both start and end move by +4000 ms; durations are preserved.
        Assert.Equal(5000, result[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(7000, result[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal(2000, result[0].Duration.TotalMilliseconds, 3);

        Assert.Equal(9000, result[1].StartTime.TotalMilliseconds, 3);
        Assert.Equal(10000, result[1].EndTime.TotalMilliseconds, 3);
        Assert.Equal(1000, result[1].Duration.TotalMilliseconds, 3);
    }
}
