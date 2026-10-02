using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Export;
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

        // Scaled times are rounded to the whole millisecond subtitle formats store (#14056).
        Assert.Equal(9091, item.StartTime.TotalMilliseconds); // round(10000 * 100/110)
        Assert.Equal(9273, item.EndTime.TotalMilliseconds);   // round(10200 * 100/110)
        Assert.True(item.Duration > TimeSpan.Zero);
    }

    [Fact]
    public void GetLoadedSupFrameRate_DeclaredRateThatFits_IsDeclared()
    {
        var (frameRate, source) = BinaryEditViewModel.GetLoadedSupFrameRate(25.0, 25.0, 23.976);

        Assert.Equal(25.0, frameRate);
        Assert.Equal(BinaryEditFrameRateSource.Declared, source);
    }

    [Fact]
    public void GetLoadedSupFrameRate_RateOtherThanDeclared_IsDetected()
    {
        var (frameRate, source) = BinaryEditViewModel.GetLoadedSupFrameRate(24000.0 / 1001, 25.0, 25.0);

        Assert.Equal(24000.0 / 1001, frameRate);
        Assert.Equal(BinaryEditFrameRateSource.Detected, source);
    }

    [Fact]
    public void GetLoadedSupFrameRate_NoRateFits_FallsBackToCurrent()
    {
        var (frameRate, source) = BinaryEditViewModel.GetLoadedSupFrameRate(0, 25.0, 29.97);

        Assert.Equal(29.97, frameRate);
        Assert.Equal(BinaryEditFrameRateSource.Current, source);
    }

    [Theory]
    [InlineData(BinaryEditFrameRateSource.Current, 25.0, 23.976, BinaryEditViewModel.VideoFrameRateAction.Use)]
    [InlineData(BinaryEditFrameRateSource.Declared, 25.0, 23.976, BinaryEditViewModel.VideoFrameRateAction.Ask)]
    [InlineData(BinaryEditFrameRateSource.Detected, 25.0, 23.976, BinaryEditViewModel.VideoFrameRateAction.Ask)]
    [InlineData(BinaryEditFrameRateSource.Manual, 25.0, 23.976, BinaryEditViewModel.VideoFrameRateAction.Ask)]
    [InlineData(BinaryEditFrameRateSource.Video, 25.0, 23.976, BinaryEditViewModel.VideoFrameRateAction.Ask)]
    [InlineData(BinaryEditFrameRateSource.Declared, 23.976, 23.976023976, BinaryEditViewModel.VideoFrameRateAction.Keep)]
    [InlineData(BinaryEditFrameRateSource.Current, 25.0, 0, BinaryEditViewModel.VideoFrameRateAction.Keep)]
    public void GetVideoFrameRateAction_AsksOnlyWhenFileOrUserChoseADifferentRate(
        BinaryEditFrameRateSource source, double current, double video, BinaryEditViewModel.VideoFrameRateAction expected)
    {
        Assert.Equal(expected, BinaryEditViewModel.GetVideoFrameRateAction(source, current, video));
    }

    [Fact]
    public void GetExportFrameRate_UsesBinaryEditRate_ExceptForDCinemaSmpte()
    {
        Assert.Equal(23.976, BinaryEditViewModel.GetExportFrameRate(new ExportHandlerBluRaySup(), 23.976, 25.0));
        Assert.Equal(23.976, BinaryEditViewModel.GetExportFrameRate(new ExportHandlerBdnXml(), 23.976, 25.0));
        Assert.Equal(25.0, BinaryEditViewModel.GetExportFrameRate(new ExportHandlerDCinemaSmpte2014Png(), 23.976, 25.0));
    }

    [Theory]
    [InlineData(24000.0 / 1001, 23.976)]
    [InlineData(30000.0 / 1001, 29.97)]
    [InlineData(60000.0 / 1001, 59.94)]
    [InlineData(25.0, 25.0)]
    public void NormalizeFrameRate_MapsNtscFractionsToListedRates(double frameRate, double expected)
    {
        Assert.Equal(expected, BinaryEditViewModel.NormalizeFrameRate(frameRate));
    }
}
