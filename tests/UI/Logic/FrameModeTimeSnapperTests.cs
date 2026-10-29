using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// Frame mode keeps re-timed lines on frames automatically, instead of the user running
/// "Snap all times to frames" by hand (mail from Ingo, 5.3 beta 3).
/// </summary>
public class FrameModeTimeSnapperTests : IDisposable
{
    private readonly double _currentFrameRate = Configuration.Settings.General.CurrentFrameRate;
    private readonly bool _snapToVideoFrames = Se.Settings.General.FrameModeSnapToVideoFrames;

    public void Dispose()
    {
        Configuration.Settings.General.CurrentFrameRate = _currentFrameRate;
        Se.Settings.General.FrameModeSnapToVideoFrames = _snapToVideoFrames;
    }

    private static SubtitleLineViewModel MakeLine(double startMs, double endMs) =>
        new() { Text = "x", StartTime = TimeSpan.FromMilliseconds(startMs), EndTime = TimeSpan.FromMilliseconds(endMs) };

    [Theory]
    [InlineData(25, 1013, 1000)]
    [InlineData(25, 1021, 1040)]
    [InlineData(25, 1999, 2000)]
    [InlineData(23.976, 1000, 1000)]
    [InlineData(23.976, 600017, 600000)]
    [InlineData(29.97, 3380, 3367)]
    public void Snap_LandsOnTheDisplayedFrame(double frameRate, double ms, double expectedMs)
    {
        Configuration.Settings.General.CurrentFrameRate = frameRate;

        Assert.Equal(expectedMs, FrameModeTimeSnapper.Snap(TimeSpan.FromMilliseconds(ms)).TotalMilliseconds);
    }

    [Theory]
    [InlineData(23.976)]
    [InlineData(24)]
    [InlineData(25)]
    [InlineData(29.97)]
    [InlineData(30)]
    [InlineData(50)]
    [InlineData(59.94)]
    public void Snap_KeepsTheDisplayAndIsStable(double frameRate)
    {
        Configuration.Settings.General.CurrentFrameRate = frameRate;

        for (var ms = 0; ms < 3000; ms++)
        {
            var time = TimeSpan.FromMilliseconds(ms);
            var snapped = FrameModeTimeSnapper.Snap(time);

            Assert.Equal(new TimeCode(time.TotalMilliseconds).ToHHMMSSFF(), new TimeCode(snapped.TotalMilliseconds).ToHHMMSSFF());
            Assert.Equal(snapped, FrameModeTimeSnapper.Snap(snapped));
        }
    }

    [Fact]
    public void SnapChangedLines_OnlyTouchesNewAndRetimedLines()
    {
        Configuration.Settings.General.CurrentFrameRate = 25;
        var untouched = MakeLine(1013, 2013);
        var retimed = MakeLine(3000, 4000);
        var recorded = new[] { new SubtitleLineViewModel(untouched), new SubtitleLineViewModel(retimed) };
        retimed.SetTimes(TimeSpan.FromMilliseconds(3013), TimeSpan.FromMilliseconds(4027));
        var inserted = MakeLine(5001, 6999);

        var changed = FrameModeTimeSnapper.SnapChangedLines(new[] { untouched, retimed, inserted }, recorded);

        Assert.Equal(2, changed);
        Assert.Equal(1013, untouched.StartTime.TotalMilliseconds);
        Assert.Equal(2013, untouched.EndTime.TotalMilliseconds);
        Assert.Equal(3000, retimed.StartTime.TotalMilliseconds);
        Assert.Equal(4040, retimed.EndTime.TotalMilliseconds);
        Assert.Equal(1040, retimed.Duration.TotalMilliseconds);
        Assert.Equal(5000, inserted.StartTime.TotalMilliseconds);
        Assert.Equal(7000, inserted.EndTime.TotalMilliseconds);
    }

    [Fact]
    public void SnapChangedLines_KeepsAtLeastOneFrame()
    {
        Configuration.Settings.General.CurrentFrameRate = 25;
        var line = MakeLine(1005, 1015);

        FrameModeTimeSnapper.SnapChangedLines(new[] { line }, Array.Empty<SubtitleLineViewModel>());

        Assert.Equal(1000, line.StartTime.TotalMilliseconds);
        Assert.Equal(1040, line.EndTime.TotalMilliseconds);
    }

    [Fact]
    public void SnapChangedLines_SkipsReferenceOnlyRows()
    {
        Configuration.Settings.General.CurrentFrameRate = 25;
        var line = MakeLine(1013, 2013);
        line.IsReferenceOnly = true;

        Assert.Equal(0, FrameModeTimeSnapper.SnapChangedLines(new[] { line }, Array.Empty<SubtitleLineViewModel>()));
        Assert.Equal(1013, line.StartTime.TotalMilliseconds);
    }

    [Theory]
    [InlineData(23.976, 118918, 118910)] // 00:01:58:22 - the frame the Blu-ray sup export uses (#16007)
    [InlineData(23.976, 126334, 126335)] // 00:02:06:08
    [InlineData(23.976, 118910, 118910)]
    [InlineData(25, 1013, 1000)]
    public void Snap_ToVideoFrames_LandsOnTheVideoFrame(double frameRate, double ms, double expectedMs)
    {
        Configuration.Settings.General.CurrentFrameRate = frameRate;
        Se.Settings.General.FrameModeSnapToVideoFrames = true;

        Assert.Equal(expectedMs, FrameModeTimeSnapper.Snap(TimeSpan.FromMilliseconds(ms)).TotalMilliseconds);
    }

    [Theory]
    [InlineData(23.976)]
    [InlineData(29.97)]
    [InlineData(59.94)]
    [InlineData(25)]
    public void Snap_ToVideoFrames_KeepsVideoFramesAndTimeCodes(double frameRate)
    {
        Configuration.Settings.General.CurrentFrameRate = frameRate;
        Se.Settings.General.FrameModeSnapToVideoFrames = true;

        // Three hours, so the drift between the time code and the video frames goes round many times.
        var lastFrame = FrameNumbers.FromMilliseconds(3 * 3600 * 1000);
        for (var frame = 0; frame <= lastFrame; frame += 7)
        {
            var frameTime = TimeSpan.FromMilliseconds(FrameNumbers.ToMilliseconds(frame));
            var snapped = FrameModeTimeSnapper.Snap(frameTime);

            Assert.Equal(frameTime, snapped);

            // Typing the shown time code back in lands on a video frame with that time code (two
            // video frames can share one, as times are whole milliseconds).
            var timeCode = new TimeCode(frameTime.TotalMilliseconds).ToHHMMSSFF();
            var typed = FrameModeTimeSnapper.Snap(TimeSpan.FromMilliseconds(TimeCode.ParseHHMMSSFFToMilliseconds(timeCode)));
            Assert.Equal(typed, FrameNumbers.Snap(typed));
            Assert.Equal(timeCode, new TimeCode(typed.TotalMilliseconds).ToHHMMSSFF());
        }
    }

    [Fact]
    public void Snap_ToVideoFramesOff_KeepsTheTimeCodeGrid()
    {
        Configuration.Settings.General.CurrentFrameRate = 23.976;
        Se.Settings.General.FrameModeSnapToVideoFrames = false;

        Assert.Equal(118918, FrameModeTimeSnapper.Snap(TimeSpan.FromMilliseconds(118918)).TotalMilliseconds);
    }
}
