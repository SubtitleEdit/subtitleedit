using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using System.Collections.ObjectModel;

namespace UITests.Features.Tools.AdjustDuration;

public class AdjustDurationViewModelTests
{
    [Fact]
    public void AdjustDuration_Percent_ScalesDurationFromStartTime()
    {
        var vm = new AdjustDurationViewModel
        {
            SelectedAdjustType = new AdjustDurationDisplay { Type = AdjustDurationType.Percent },
            AdjustPercent = 120,
        };
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new()
            {
                Text = "First",
                StartTime = TimeSpan.FromSeconds(1),
                EndTime = TimeSpan.FromSeconds(3),
            },
            new()
            {
                Text = "Second",
                StartTime = TimeSpan.FromSeconds(10),
                EndTime = TimeSpan.FromSeconds(11),
            },
        };

        vm.AdjustDuration(subtitles);

        Assert.Equal(TimeSpan.FromMilliseconds(3400), subtitles[0].EndTime);
    }
}
