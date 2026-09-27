using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

public class VideoStreamDurationTests
{
    [Theory]
    [InlineData("out_time_us=59966732", 59966732)]
    [InlineData("out_time_us=0", 0)]
    public void TryParseOutTime_ProgressLine_ReturnsMicroseconds(string line, long expected)
    {
        Assert.True(VideoStreamDuration.TryParseOutTime(line, out var microseconds));
        Assert.Equal(expected, microseconds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("out_time_us=N/A")]
    [InlineData("out_time_ms=59966732")]
    [InlineData("out_time_us=-64000")]
    [InlineData("frame=1802")]
    public void TryParseOutTime_OtherLines_ReturnsFalse(string? line)
    {
        Assert.False(VideoStreamDuration.TryParseOutTime(line, out _));
    }

    [Fact]
    public void IsTruncated_VideoStoppedAfterAMinute_True()
    {
        // #15265: video stream 62 s in the output, 1723.5 s in the source.
        Assert.True(VideoStreamDuration.IsTruncated(1723.5, 62.033333));
    }

    [Theory]
    [InlineData(1723.5, 1723.5)]
    [InlineData(1723.5, 1723.4)]
    [InlineData(1723.5, 1720.0)] // 3.5 s short: stream-copy edge drift, not a loss
    [InlineData(20.0, 17.0)]     // 15% short but only 3 s
    [InlineData(1000.0, 990.0)]  // 10 s short but only 1%
    [InlineData(0, 10)]
    public void IsTruncated_SmallDifferences_False(double source, double output)
    {
        Assert.False(VideoStreamDuration.IsTruncated(source, output));
    }
}
