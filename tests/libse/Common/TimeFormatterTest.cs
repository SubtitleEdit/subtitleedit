using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TimeFormatters;

namespace LibSETests.Common;

[Collection("NonParallelTests")]
public class TimeFormatterTest
{
    private static string Format(double frameRate, TimeCode timeCode, ITimeFormatter formatter)
    {
        var original = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = frameRate;
            return timeCode.ToString(formatter);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = original;
        }
    }

    [Theory]
    [InlineData(0, 0, 0, 0, "00:00:00:00")]
    [InlineData(1, 2, 3, 400, "01:02:03:10")]
    [InlineData(0, 0, 1, 999, "00:00:02:00")] // frame count rounds up to frame rate -> carry to next second
    [InlineData(0, 0, 59, 999, "00:01:00:00")] // carry rolls over into minutes
    [InlineData(0, 59, 59, 999, "01:00:00:00")] // carry rolls over into hours
    public void HhMmSsFf25Fps(int hours, int minutes, int seconds, int milliseconds, string expected)
    {
        Assert.Equal(expected, Format(25.0, new TimeCode(hours, minutes, seconds, milliseconds), TimeFormatter.HhMmSsFf));
    }

    [Fact]
    public void HhMmSsFfHoursAbove24()
    {
        Assert.Equal("25:00:00:00", Format(25.0, new TimeCode(25, 0, 0, 0), TimeFormatter.HhMmSsFf));
    }

    [Fact]
    public void HhMmSsFfNegative()
    {
        Assert.Equal("-00:00:05:00", Format(25.0, new TimeCode(-5000.0), TimeFormatter.HhMmSsFf));
    }

    [Fact]
    public void HhMmSsFf2997Fps()
    {
        Assert.Equal("00:00:01:15", Format(29.97, new TimeCode(0, 0, 1, 500), TimeFormatter.HhMmSsFf));
    }

    [Fact]
    public void HhMmSsFf23976FpsCarriesBefore999Milliseconds()
    {
        // at 23.976 fps, 999 ms rounds to 24 frames, which carries to the next second
        Assert.Equal("00:00:02:00", Format(23.976, new TimeCode(0, 0, 1, 999), TimeFormatter.HhMmSsFf));
    }

    [Theory]
    [InlineData(0, 0, 5, 0, "05:00")]
    [InlineData(0, 2, 3, 400, "02:03:10")]
    [InlineData(1, 2, 3, 400, "01:02:03:10")]
    public void ShortHhMmSsFfTrimsLeadingZeroGroups(int hours, int minutes, int seconds, int milliseconds, string expected)
    {
        Assert.Equal(expected, Format(25.0, new TimeCode(hours, minutes, seconds, milliseconds), TimeFormatter.ShortHhMmSsFf));
    }

    [Fact]
    public void ShortHhMmSsFfNegativeKeepsSignAfterTrim()
    {
        Assert.Equal("-05:00", Format(25.0, new TimeCode(-5000.0), TimeFormatter.ShortHhMmSsFf));
    }

    [Theory]
    [InlineData(1, 2, 3, 400, "01:02:03")]
    [InlineData(0, 0, 1, 400, "00:00:01")] // frames below carry threshold do not round seconds up
    [InlineData(0, 0, 59, 999, "00:01:00")] // frame carry rounds the seconds up
    public void HhMmSs25Fps(int hours, int minutes, int seconds, int milliseconds, string expected)
    {
        Assert.Equal(expected, Format(25.0, new TimeCode(hours, minutes, seconds, milliseconds), TimeFormatter.HhMmSs));
    }

    [Theory]
    [InlineData(1, 2, 3, 400, "01:02:03;10")]
    [InlineData(0, 0, 1, 999, "00:00:02;00")]
    public void HhMmSsFfDropFrame25Fps(int hours, int minutes, int seconds, int milliseconds, string expected)
    {
        Assert.Equal(expected, Format(25.0, new TimeCode(hours, minutes, seconds, milliseconds), TimeFormatter.HhMmSsFfDropFrame));
    }

    [Theory]
    [InlineData(0, 0, 5, 400, "05:10")]
    [InlineData(0, 1, 5, 400, "05:10")] // minutes and hours are not included
    [InlineData(0, 0, 59, 999, "60:00")] // carry increments seconds without rolling over into minutes
    public void SsFf25Fps(int hours, int minutes, int seconds, int milliseconds, string expected)
    {
        Assert.Equal(expected, Format(25.0, new TimeCode(hours, minutes, seconds, milliseconds), TimeFormatter.SsFf));
    }

    [Theory]
    [InlineData(1, 2, 3, 400, "01:02:03.10")]
    [InlineData(0, 0, 1, 999, "00:00:02.00")]
    public void HhMmSsPeriodFf25Fps(int hours, int minutes, int seconds, int milliseconds, string expected)
    {
        Assert.Equal(expected, Format(25.0, new TimeCode(hours, minutes, seconds, milliseconds), TimeFormatter.HhMmSsPeriodFf));
    }
}
