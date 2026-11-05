using Nikse.SubtitleEdit.Features.Video.VideoOcr;

namespace UITests.Features.Video.VideoOcr;

public class VideoOcrTimeRangesTests
{
    private const double Duration = 3600;

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Parse_Empty_IsWholeVideo(string? text)
    {
        var ranges = VideoOcrTimeRanges.Parse(text, Duration, out _);

        Assert.Equal(new[] { new VideoOcrTimeRange(0, Duration) }, ranges);
    }

    [Theory]
    [InlineData("16:00-32:00; 36:00-41:00")]
    [InlineData("16:00 - 32:00, 36:00 - 41:00")]
    [InlineData("16:00-32:00,36:00-41:00")]
    [InlineData("16:00-32:00 36:00-41:00")]
    [InlineData("0:16:00-0:32:00\n0:36:00-0:41:00")]
    [InlineData("960-1920;2160-2460")]
    public void Parse_IssueExample_TwoRanges(string text)
    {
        var ranges = VideoOcrTimeRanges.Parse(text, Duration, out _);

        Assert.Equal(new[] { new VideoOcrTimeRange(960, 1920), new VideoOcrTimeRange(2160, 2460) }, ranges);
    }

    [Fact]
    public void Parse_Milliseconds_DotOrComma()
    {
        var ranges = VideoOcrTimeRanges.Parse("1:02.5-1:03,250", Duration, out _);

        Assert.NotNull(ranges);
        Assert.Equal(62.5, ranges![0].StartSeconds, 3);
        Assert.Equal(63.25, ranges[0].EndSeconds, 3);
    }

    [Fact]
    public void Parse_OpenSides_UseVideoStartAndEnd()
    {
        var ranges = VideoOcrTimeRanges.Parse("-5:00; 50:00-", Duration, out _);

        Assert.Equal(new[] { new VideoOcrTimeRange(0, 300), new VideoOcrTimeRange(3000, Duration) }, ranges);
    }

    [Fact]
    public void Parse_SortsAndJoinsOverlaps_ClampsToDuration()
    {
        var ranges = VideoOcrTimeRanges.Parse("50:00-2:00:00; 10:00-20:00; 15:00-25:00", Duration, out _);

        Assert.Equal(new[] { new VideoOcrTimeRange(600, 1500), new VideoOcrTimeRange(3000, Duration) }, ranges);
    }

    [Theory]
    [InlineData("abc", "abc")]
    [InlineData("10:00", "10:00")]
    [InlineData("20:00-10:00", "20:00-10:00")]
    [InlineData("1:10:00-1:20:00", "1:10:00-1:20:00")] // starts after the video ends
    [InlineData("1:00-2:00; x", "x")]
    [InlineData("-", "-")]
    public void Parse_Invalid_ReturnsNullWithPart(string text, string expectedPart)
    {
        var ranges = VideoOcrTimeRanges.Parse(text, Duration, out var invalidPart);

        Assert.Null(ranges);
        Assert.Equal(expectedPart, invalidPart);
    }

    [Theory]
    [InlineData("", 960, "16:00-")]
    [InlineData("16:00-", 1920, "16:00-32:00")]
    [InlineData("16:00-32:00", 2160, "16:00-32:00; 36:00-")]
    [InlineData("16:00-32:00; ", 4000, "16:00-32:00; 1:06:40-")]
    public void AppendTime_StartsOrEndsRange(string text, double seconds, string expected)
    {
        Assert.Equal(expected, VideoOcrTimeRanges.AppendTime(text, seconds));
    }

    [Fact]
    public void FrameGroup_OffsetPutsTimesOnVideoClock()
    {
        // Frame 0 of a range scanned from 16:00 at 5 fps is 16:00.000.
        var group = new VideoOcrFrameGroup { StartFrame = 10, EndFrame = 14, OffsetMs = 960_000 };

        Assert.Equal(962_000, group.GetStartMs(5));
        Assert.Equal(963_000, group.GetEndMs(5));
    }
}
