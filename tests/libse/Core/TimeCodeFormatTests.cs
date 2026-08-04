using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

/// <summary>
/// The five frame-based formatters share one implementation. These pin their output - especially
/// the "frames round up to a whole second" carry, and the fact that ToSSFF carries into seconds
/// only, never into minutes or hours.
/// </summary>
public class TimeCodeFormatTests : IDisposable
{
    private readonly double _originalFrameRate = Configuration.Settings.General.CurrentFrameRate;

    public TimeCodeFormatTests()
    {
        Configuration.Settings.General.CurrentFrameRate = 25;
    }

    public void Dispose()
    {
        Configuration.Settings.General.CurrentFrameRate = _originalFrameRate;
    }

    [Theory]
    // totalMs,   HHMMSSFF,      HHMMSS,     DropFrame,      SSFF,    PeriodFF
    [InlineData(0, "00:00:00:00", "00:00:00", "00:00:00;00", "00:00", "00:00:00.00")]
    [InlineData(500, "00:00:00:13", "00:00:00", "00:00:00;13", "00:13", "00:00:00.13")]
    // 999 ms rounds to 25 frames at 25 fps, i.e. a whole second - every formatter must carry.
    [InlineData(999, "00:00:01:00", "00:00:01", "00:00:01;00", "01:00", "00:00:01.00")]
    [InlineData(59_999, "00:01:00:00", "00:01:00", "00:01:00;00", "60:00", "00:01:00.00")]
    [InlineData(3_600_000, "01:00:00:00", "01:00:00", "01:00:00;00", "00:00", "01:00:00.00")]
    [InlineData(90_061_500, "25:01:01:13", "25:01:01", "25:01:01;13", "01:13", "25:01:01.13")]
    public void FrameFormatters_Match(double totalMilliseconds, string hhmmssff, string hhmmss,
        string dropFrame, string ssff, string periodFf)
    {
        var tc = new TimeCode(totalMilliseconds);

        Assert.Equal(hhmmssff, tc.ToHHMMSSFF());
        Assert.Equal(hhmmss, tc.ToHHMMSS());
        Assert.Equal(dropFrame, tc.ToHHMMSSFFDropFrame());
        Assert.Equal(ssff, tc.ToSSFF());
        Assert.Equal(periodFf, tc.ToHHMMSSPeriodFF());
    }

    /// <summary>
    /// ToSSFF carries into the seconds field without touching minutes or hours: at 59.999 s it
    /// reports "60:00", not "00:00". That is long-standing behaviour, pinned here so the shared
    /// implementation cannot quietly "fix" it.
    /// </summary>
    [Fact]
    public void ToSSFF_CarriesIntoSecondsOnly()
    {
        Assert.Equal("60:00", new TimeCode(59_999).ToSSFF());
        Assert.Equal("00:00", new TimeCode(3_600_000).ToSSFF());
    }

    [Theory]
    [InlineData(-1000, "-00:00:01:00")]
    [InlineData(-90_000, "-00:01:30:00")]
    public void FrameFormatters_PrefixASingleSign(double totalMilliseconds, string expected)
    {
        Assert.Equal(expected, new TimeCode(totalMilliseconds).ToHHMMSSFF());
    }

    [Theory]
    [InlineData(-1000, "-00:00:01,000")]
    [InlineData(-3_723_456, "-01:02:03,456")]
    public void ToString_PrefixesASingleSign(double totalMilliseconds, string expected)
    {
        Assert.Equal(expected, new TimeCode(totalMilliseconds).ToString(false));
    }

    [Fact]
    public void ToShortStringHHMMSSFF_DropsLeadingZeroGroups()
    {
        Assert.Equal("00:00", new TimeCode(0).ToShortStringHHMMSSFF());
        Assert.Equal("01:00", new TimeCode(999).ToShortStringHHMMSSFF());
        Assert.Equal("01:00:00", new TimeCode(59_999).ToShortStringHHMMSSFF());
        Assert.Equal("01:00:00:00", new TimeCode(3_600_000).ToShortStringHHMMSSFF());
    }
}
