using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic;

public class FfmpegFrameRateTests
{
    // Exact known rates must stay exactly themselves (issue #11847: 24 -> 23.976, 30 -> 29.97).
    [Theory]
    [InlineData(24.0, 24.0)]
    [InlineData(30.0, 30.0)]
    [InlineData(25.0, 25.0)]
    [InlineData(50.0, 50.0)]
    [InlineData(60.0, 60.0)]
    [InlineData(48.0, 48.0)]
    [InlineData(120.0, 120.0)]
    public void NormalizeFrameRate_KeepsExactIntegerRates(double input, double expected)
    {
        Assert.Equal((decimal)expected, FfmpegMediaInfo2.NormalizeFrameRate(input));
    }

    // ffmpeg prints NTSC rates with two decimals; snap those to the canonical values.
    [Theory]
    [InlineData(23.98, 23.976)]
    [InlineData(23.976, 23.976)]
    [InlineData(29.97, 29.97)]
    [InlineData(59.94, 59.94)]
    public void NormalizeFrameRate_SnapsNtscRatesToCanonical(double input, double expected)
    {
        Assert.Equal((decimal)expected, FfmpegMediaInfo2.NormalizeFrameRate(input));
    }

    // Values not close to any known rate are returned unchanged (no over-aggressive snapping).
    [Theory]
    [InlineData(26.0)]
    [InlineData(15.0)]
    [InlineData(100.0)]
    public void NormalizeFrameRate_KeepsUnknownRates(double input)
    {
        Assert.Equal((decimal)input, FfmpegMediaInfo2.NormalizeFrameRate(input));
    }
}
