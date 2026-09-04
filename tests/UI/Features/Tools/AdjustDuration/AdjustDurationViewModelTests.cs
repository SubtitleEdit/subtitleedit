using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.UiLogic.AdjustDuration;
using System.Collections.ObjectModel;

namespace UITests.Features.Tools.AdjustDuration;

public class AdjustDurationViewModelTests
{
    [Fact]
    public void AdjustDuration_NegativeSeconds_DoesNotPushEndBeforeStart()
    {
        var vm = new AdjustDurationViewModel
        {
            SelectedAdjustType = new AdjustDurationDisplay { Type = AdjustDurationType.Seconds },
            AdjustSeconds = -10,
        };
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new()
            {
                Text = "First",
                StartTime = TimeSpan.FromSeconds(1),
                EndTime = TimeSpan.FromSeconds(3),
            },
        };

        vm.AdjustDuration(subtitles);

        // Clamped to a 100 ms minimum duration instead of ending before it starts
        Assert.Equal(TimeSpan.FromMilliseconds(1100), subtitles[0].EndTime);
    }

    [Fact]
    public void AdjustDuration_Recalculate_IgnoresHtmlTagsLikeTheCpsColumn()
    {
        var vm = new AdjustDurationViewModel
        {
            SelectedAdjustType = new AdjustDurationDisplay { Type = AdjustDurationType.Recalculate },
            AdjustRecalculateOptimalCharacterPerSecond = 5,
            AdjustRecalculateMaxCharacterPerSecond = 25,
        };
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new()
            {
                Text = "<i>Hello</i>",
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromSeconds(10),
            },
        };

        vm.AdjustDuration(subtitles);

        // 5 visible characters at 5 CPS = 1 s; counting the tags would give 12 / 5 = 2.4 s
        Assert.Equal(TimeSpan.FromSeconds(1), subtitles[0].EndTime);
    }

    [Fact]
    public void AdjustDuration_Recalculate_StaysWithinTheTargetCps()
    {
        // Issue #14418: 20 chars at 15 CPS is 1333.33 ms; rounding to the nearest ms gave 1333 ms,
        // which is 15.004 CPS - over the limit the duration was computed from. Rounds up instead.
        var vm = new AdjustDurationViewModel
        {
            SelectedAdjustType = new AdjustDurationDisplay { Type = AdjustDurationType.Recalculate },
            AdjustRecalculateOptimalCharacterPerSecond = 15,
            AdjustRecalculateMaxCharacterPerSecond = 15,
        };
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new()
            {
                Text = "It is not like that.",
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromSeconds(10),
            },
        };

        vm.AdjustDuration(subtitles);

        Assert.Equal(1334, subtitles[0].EndTime.TotalMilliseconds);
        Assert.True(subtitles[0].CharactersPerSecond <= 15);
    }

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
