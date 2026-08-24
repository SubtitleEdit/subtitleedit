using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using System.Collections.ObjectModel;

namespace UITests.Features.Sync.ChangeFrameRate;

public class ChangeFrameRateViewModelTests
{
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
}
