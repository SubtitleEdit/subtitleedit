using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic;

public class FfmpegDurationTests
{
    [Fact]
    public void ParseDurationSeconds_ReadsCentiseconds()
    {
        var log = "Input #0, mpegts, from 'Shrek.ts':\n  Duration: 01:35:02.99, start: 1.400000, bitrate: 4021 kb/s\n";
        Assert.Equal(5702.99, FfmpegMediaInfo2.ParseDurationSeconds(log)!.Value, 3);
    }

    [Fact]
    public void ParseDurationSeconds_BitrateEstimate_ReturnsNull()
    {
        var log = "[mpeg @ 0x1] Estimating duration from bitrate, this may be inaccurate\n  Duration: 07:56:43.30, start: 0.000000, bitrate: 900 kb/s\n";
        Assert.Null(FfmpegMediaInfo2.ParseDurationSeconds(log));
    }

    [Fact]
    public void ParseDurationSeconds_NotAvailable_ReturnsNull()
    {
        Assert.Null(FfmpegMediaInfo2.ParseDurationSeconds("  Duration: N/A, bitrate: N/A\n"));
    }
}
