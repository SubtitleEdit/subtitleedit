using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

namespace UITests.Features.Sync.ChangeFrameRate;

public class ChangeFrameRateViewModelTests : IDisposable
{
    private readonly double _savedFrom = Se.Settings.Synchronization.ChangeFrameRateFrom;
    private readonly double _savedTo = Se.Settings.Synchronization.ChangeFrameRateTo;

    public void Dispose()
    {
        Se.Settings.Synchronization.ChangeFrameRateFrom = _savedFrom;
        Se.Settings.Synchronization.ChangeFrameRateTo = _savedTo;
    }

    private static ChangeFrameRateViewModel MakeViewModel(double savedFrom, double savedTo)
    {
        Se.Settings.Synchronization.ChangeFrameRateFrom = savedFrom;
        Se.Settings.Synchronization.ChangeFrameRateTo = savedTo;
        return new ChangeFrameRateViewModel(new StubFileHelper());
    }

    [Fact]
    public void Initialize_CurrentFrameRate_IsTheFromRate_LikeSe4()
    {
        // #15177: a subtitle demuxed from a 23.976 video was authored at 23.976 - that is the
        // source rate. SE 5 used to put the video rate in "to" and guess 25 for "from".
        var vm = MakeViewModel(savedFrom: 25, savedTo: 30);

        vm.Initialize("/videos/movie.mkv", videoFrameRate: 23.976, currentFrameRate: 23.976);

        Assert.Equal(23.976, vm.SelectedFromFrameRate, 3);
        Assert.Equal(30, vm.SelectedToFrameRate, 3); // the last used target is kept
    }

    [Fact]
    public void Initialize_ToEqualToFrom_PicksTheCounterpart()
    {
        // A from == to preset would make OK a no-op; PAL <-> NTSC film is the usual conversion.
        var vm = MakeViewModel(savedFrom: 25, savedTo: 25);

        vm.Initialize(null, videoFrameRate: 0, currentFrameRate: 25);
        Assert.Equal(25, vm.SelectedFromFrameRate, 3);
        Assert.Equal(23.976, vm.SelectedToFrameRate, 3);

        vm = MakeViewModel(savedFrom: 25, savedTo: 29.97);
        vm.Initialize(null, videoFrameRate: 0, currentFrameRate: 29.97);
        Assert.Equal(29.97, vm.SelectedFromFrameRate, 3);
        Assert.Equal(25, vm.SelectedToFrameRate, 3);
    }

    [Fact]
    public void Initialize_ToolbarRateWinsOverVideoRate()
    {
        // SE 4 passed the toolbar frame rate, which the user may have changed after the video
        // loaded - the toolbar is what the rest of the app converts with.
        var vm = MakeViewModel(savedFrom: 25, savedTo: 30);

        vm.Initialize("/videos/movie.mp4", videoFrameRate: 29.97, currentFrameRate: 24);

        Assert.Equal(24, vm.SelectedFromFrameRate, 3);
        Assert.Contains(29.97, vm.FromFrameRates); // the video's rate is still one click away
        Assert.Contains(29.97, vm.ToFrameRates);
    }

    [Fact]
    public void Initialize_NonStandardRate_IsAddedToBothLists()
    {
        var vm = MakeViewModel(savedFrom: 25, savedTo: 30);

        vm.Initialize("/videos/game.mkv", videoFrameRate: 48, currentFrameRate: 48);

        Assert.Equal(48, vm.SelectedFromFrameRate, 3);
        Assert.Contains(48, vm.FromFrameRates);
        Assert.Contains(48, vm.ToFrameRates);
        Assert.Equal(vm.FromFrameRates.OrderBy(r => r), vm.FromFrameRates);
    }

    [Fact]
    public void Initialize_WithVideo_ShowsFileNameAndFrameRate()
    {
        var vm = MakeViewModel(savedFrom: 25, savedTo: 30);

        vm.Initialize("/videos/movie.mkv", videoFrameRate: 23.976, currentFrameRate: 23.976);

        Assert.True(vm.HasVideo);
        Assert.Equal("/videos/movie.mkv", vm.VideoFileName);
        Assert.Contains("movie.mkv", vm.VideoInfoText);
        Assert.DoesNotContain("/videos/", vm.VideoInfoText); // the folder lives in the tooltip
        Assert.Contains("23.976", vm.VideoInfoText);
    }

    [Fact]
    public void Initialize_WithoutVideo_HidesTheVideoLine_AndKeepsSavedRates()
    {
        var vm = MakeViewModel(savedFrom: 24, savedTo: 30);

        vm.Initialize(null, videoFrameRate: 0, currentFrameRate: 0);

        Assert.False(vm.HasVideo);
        Assert.Equal(string.Empty, vm.VideoInfoText);
        Assert.Equal(24, vm.SelectedFromFrameRate, 3);
        Assert.Equal(30, vm.SelectedToFrameRate, 3);
    }

    [Theory]
    [InlineData(25.0, 30.0)]
    [InlineData(23.976, 25.0)]
    [InlineData(30.0, 25.0)]
    [InlineData(60.0, 23.976)]
    public void GetFrameRateRatio_IsFromOverTo(double from, double to)
    {
        // Guards against re-inverting the ratio (see PR #11665): must match
        // libse Subtitle.ChangeFrameRate, which scales by oldFrameRate / newFrameRate.
        var ratio = ChangeFrameRateViewModel.GetFrameRateRatio(from, to);

        var expected = SubtitleFormat.GetFrameForCalculation(from) / SubtitleFormat.GetFrameForCalculation(to);
        Assert.Equal(expected, ratio);
    }

    [Fact]
    public void GetFrameRateRatio_HigherTargetRate_MakesTimeCodesEarlier()
    {
        var ratio = ChangeFrameRateViewModel.GetFrameRateRatio(25.0, 30.0);

        Assert.True(ratio < 1.0);
        Assert.Equal(1000.0 * 25.0 / 30.0, 1000.0 * ratio, 6); // 1000 ms -> ~833 ms
    }

    [Fact]
    public void ChangeFrameRate_ScalesStartAndEndByFromOverTo()
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new() { Text = "a", StartTime = TimeSpan.FromMilliseconds(1000), EndTime = TimeSpan.FromMilliseconds(3000) },
            new() { Text = "b", StartTime = TimeSpan.FromMilliseconds(6000), EndTime = TimeSpan.FromMilliseconds(9000) },
        };

        ChangeFrameRateViewModel.ChangeFrameRate(subtitles, 25.0, 30.0);

        // Scaled times land on whole milliseconds - the only resolution subtitle formats
        // have - rounded as start + scaled duration so equal lengths stay equal (#14056).
        Assert.Equal(833, subtitles[0].StartTime.TotalMilliseconds);  // round(1000 * 25/30)
        Assert.Equal(2500, subtitles[0].EndTime.TotalMilliseconds);   // 833 + round(2000 * 25/30)
        Assert.Equal(5000, subtitles[1].StartTime.TotalMilliseconds); // round(6000 * 25/30)
        Assert.Equal(7500, subtitles[1].EndTime.TotalMilliseconds);   // 5000 + round(3000 * 25/30)
    }

    [Fact]
    public void ChangeFrameRate_BackToBackLines_NeverGainAnOverlap()
    {
        // Rounding start and scaled duration separately can land an end 1 ms past the next
        // start; a clean join must stay a join, not become an overlap. Chain enough lines with
        // varied durations that several joins hit the both-halves-round-up case at 25 -> 23.976.
        var subtitles = new ObservableCollection<SubtitleLineViewModel>();
        var startMs = 0;
        for (var i = 0; i < 50; i++)
        {
            var durationMs = 1000 + i * 137;
            subtitles.Add(new SubtitleLineViewModel
            {
                Text = i.ToString(),
                StartTime = TimeSpan.FromMilliseconds(startMs),
                EndTime = TimeSpan.FromMilliseconds(startMs + durationMs),
            });
            startMs += durationMs;
        }

        ChangeFrameRateViewModel.ChangeFrameRate(subtitles, 25.0, 23.976);

        for (var i = 0; i + 1 < subtitles.Count; i++)
        {
            Assert.True(subtitles[i].EndTime <= subtitles[i + 1].StartTime,
                $"line {i} ends at {subtitles[i].EndTime.TotalMilliseconds} ms, past line {i + 1} starting at {subtitles[i + 1].StartTime.TotalMilliseconds} ms");
        }
    }

    [Fact]
    public void ChangeFrameRate_SourceOverlap_IsKeptNotExtended()
    {
        // An overlap already in the source is the author's problem, not the conversion's -
        // the clip only removes overlaps the rounding manufactured.
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new() { Text = "a", StartTime = TimeSpan.FromMilliseconds(1000), EndTime = TimeSpan.FromMilliseconds(3100) },
            new() { Text = "b", StartTime = TimeSpan.FromMilliseconds(3000), EndTime = TimeSpan.FromMilliseconds(5000) },
        };

        ChangeFrameRateViewModel.ChangeFrameRate(subtitles, 25.0, 23.976);

        // Still overlapping by roughly the scaled 100 ms - not clipped away.
        var overlapMs = subtitles[0].EndTime.TotalMilliseconds - subtitles[1].StartTime.TotalMilliseconds;
        Assert.InRange(overlapMs, 100, 110);
    }
}
