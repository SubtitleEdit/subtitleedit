using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace UITests.Features.Shared.BinaryEdit;

public class BinaryEditViewModelTests
{
    [Fact]
    public void FindActiveSubtitleIndex_IncludesCueStartAndExcludesCueEnd()
    {
        var subtitles = new List<BinarySubtitleItem>
        {
            new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)),
            new(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5)),
        };

        Assert.Equal(0, BinaryEditViewModel.FindActiveSubtitleIndex(subtitles, TimeSpan.FromSeconds(1)));
        Assert.Equal(1, BinaryEditViewModel.FindActiveSubtitleIndex(subtitles, TimeSpan.FromSeconds(3)));
        Assert.Equal(-1, BinaryEditViewModel.FindActiveSubtitleIndex(subtitles, TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void FindActiveSubtitleIndex_ReturnsMinusOneInsideGap()
    {
        var subtitles = new List<BinarySubtitleItem>
        {
            new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)),
            new(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(5)),
        };

        var result = BinaryEditViewModel.FindActiveSubtitleIndex(subtitles, TimeSpan.FromSeconds(3));

        Assert.Equal(-1, result);
    }

    [Fact]
    public void ShouldAutoOpenMatchingVideo_RequiresSettingAndMatchingPath()
    {
        Assert.True(BinaryEditViewModel.ShouldAutoOpenMatchingVideo(true, "video.mkv"));
        Assert.False(BinaryEditViewModel.ShouldAutoOpenMatchingVideo(false, "video.mkv"));
        Assert.False(BinaryEditViewModel.ShouldAutoOpenMatchingVideo(true, string.Empty));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("video.mkv", false)]
    public void ShouldOpenVideoPickerOnSurfaceClick_OnlyWhenNoVideoIsLoaded(string? fileName, bool expected)
    {
        var result = BinaryEditViewModel.ShouldOpenVideoPickerOnSurfaceClick(fileName);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SeTools_BinaryEditSelectCurrentSubtitleWhilePlaying_DefaultsToFalse()
    {
        var settings = new SeTools();

        Assert.False(settings.BinEditSelectCurrentSubtitleWhilePlaying);
    }

    [Fact]
    public void ScaleBinarySubtitleTimes_ChangeFrameRate_ShortSubtitleAtHighOffset_PreservesPositiveDuration()
    {
        // A short subtitle (200ms) at a high start offset (10000ms) with ratio < 1 (24→25fps)
        // exposed the bug: the old code read s.EndTime after OnStartTimeChanged had already
        // rewritten it to newStart + oldDuration, so the scaled EndTime fell below StartTime.
        var item = new BinarySubtitleItem(TimeSpan.FromMilliseconds(10000), TimeSpan.FromMilliseconds(10200));
        var ratio = ChangeFrameRateViewModel.GetFrameRateRatio(24.0, 25.0); // 0.96

        BinaryEditViewModel.ScaleBinarySubtitleTimes([item], ratio);

        Assert.Equal(9600, item.StartTime.TotalMilliseconds, 3);
        Assert.Equal(9792, item.EndTime.TotalMilliseconds, 3);
        Assert.True(item.Duration > TimeSpan.Zero);
    }

    [Fact]
    public void ScaleBinarySubtitleTimes_ChangeSpeed_ShortSubtitleAtHighOffset_PreservesPositiveDuration()
    {
        // Same callback-corruption bug in ChangeSpeed: speeding up (factor < 1) a short subtitle
        // at a high offset produced EndTime < StartTime with the old sequential assignment.
        var item = new BinarySubtitleItem(TimeSpan.FromMilliseconds(10000), TimeSpan.FromMilliseconds(10200));
        var factor = 100.0 / 110.0; // 110% speed → factor ≈ 0.909

        BinaryEditViewModel.ScaleBinarySubtitleTimes([item], factor);

        Assert.Equal(9090.909, item.StartTime.TotalMilliseconds, 3);
        Assert.Equal(9272.727, item.EndTime.TotalMilliseconds, 3);
        Assert.True(item.Duration > TimeSpan.Zero);
    }
}
