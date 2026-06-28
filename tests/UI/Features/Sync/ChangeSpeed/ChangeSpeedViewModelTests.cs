using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UITests.Features.Sync.ChangeSpeed;

public class ChangeSpeedViewModelTests
{
    private static ObservableCollection<SubtitleLineViewModel> OneLine()
        => new()
        {
            new()
            {
                StartTime = TimeSpan.FromMilliseconds(1000),
                EndTime = TimeSpan.FromMilliseconds(3000),
            },
        };

    [Fact]
    public void ApplyThenOk_DoesNotCompoundTheFactor()
    {
        // Regression (#bug-hunt): "Apply" then "Change"(OK) used to apply the factor twice
        // (125% -> x0.8 -> x0.8 = x0.64). The dialog now recomputes from the original snapshot,
        // so the result is the same as applying once.
        var subtitles = OneLine();
        var vm = new ChangeSpeedViewModel();
        vm.Initialize(subtitles, new List<int>());
        vm.SpeedPercent = 125.0;

        vm.ApplyCommand.Execute(null);
        vm.OkCommand.Execute(null);

        Assert.Equal(800, subtitles[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(2400, subtitles[0].EndTime.TotalMilliseconds, 3);
        Assert.True(vm.OkPressed);
    }

    [Fact]
    public void ApplyRepeatedly_IsIdempotent()
    {
        var subtitles = OneLine();
        var vm = new ChangeSpeedViewModel();
        vm.Initialize(subtitles, new List<int>());
        vm.SpeedPercent = 125.0;

        vm.ApplyCommand.Execute(null);
        vm.ApplyCommand.Execute(null);
        vm.ApplyCommand.Execute(null);

        Assert.Equal(800, subtitles[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(2400, subtitles[0].EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void Ok_WithoutApply_AppliesOnce()
    {
        var subtitles = OneLine();
        var vm = new ChangeSpeedViewModel();
        vm.Initialize(subtitles, new List<int>());
        vm.SpeedPercent = 125.0;

        vm.OkCommand.Execute(null);

        Assert.Equal(800, subtitles[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(2400, subtitles[0].EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void ZeroSpeed_IsIgnored_NoDivideByZero()
    {
        var subtitles = OneLine();
        var vm = new ChangeSpeedViewModel();
        vm.Initialize(subtitles, new List<int>());
        vm.SpeedPercent = 0.0;

        vm.OkCommand.Execute(null);

        // Times unchanged (no Infinity/overflow).
        Assert.Equal(1000, subtitles[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(3000, subtitles[0].EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void ChangeSpeed_125Percent_ScalesAllLines()
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new()
            {
                Text = "First",
                StartTime = TimeSpan.FromMilliseconds(1000),
                EndTime = TimeSpan.FromMilliseconds(3000),
            },
            new()
            {
                Text = "Second",
                StartTime = TimeSpan.FromMilliseconds(10000),
                EndTime = TimeSpan.FromMilliseconds(11000),
            },
            new()
            {
                Text = "Third",
                StartTime = TimeSpan.FromMilliseconds(20000),
                EndTime = TimeSpan.FromMilliseconds(22500),
            },
        };

        ChangeSpeedViewModel.ChangeSpeed(subtitles, 125.0);

        Assert.Equal(800, subtitles[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(2400, subtitles[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal(8000, subtitles[1].StartTime.TotalMilliseconds, 3);
        Assert.Equal(8800, subtitles[1].EndTime.TotalMilliseconds, 3);
        Assert.Equal(16000, subtitles[2].StartTime.TotalMilliseconds, 3);
        Assert.Equal(18000, subtitles[2].EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void ChangeSpeed_125Percent_AllStartTimesArePositive()
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>();
        for (var i = 1; i <= 10; i++)
        {
            subtitles.Add(new SubtitleLineViewModel
            {
                Text = "Line " + i,
                StartTime = TimeSpan.FromSeconds(i * 5),
                EndTime = TimeSpan.FromSeconds(i * 5 + 2),
            });
        }

        ChangeSpeedViewModel.ChangeSpeed(subtitles, 125.0);

        foreach (var s in subtitles)
        {
            Assert.True(s.StartTime.TotalMilliseconds >= 0, $"StartTime negative: {s.StartTime}");
            Assert.True(s.EndTime > s.StartTime, $"EndTime {s.EndTime} not after StartTime {s.StartTime}");
        }
    }

    [Fact]
    public void ChangeSpeed_50Percent_DoublesTimes()
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new()
            {
                Text = "Line",
                StartTime = TimeSpan.FromMilliseconds(1000),
                EndTime = TimeSpan.FromMilliseconds(2000),
            },
        };

        ChangeSpeedViewModel.ChangeSpeed(subtitles, 50.0);

        Assert.Equal(2000, subtitles[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(4000, subtitles[0].EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void ChangeSpeed_100Percent_LeavesTimesUnchanged()
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new()
            {
                Text = "Line",
                StartTime = TimeSpan.FromMilliseconds(1234),
                EndTime = TimeSpan.FromMilliseconds(5678),
            },
        };

        ChangeSpeedViewModel.ChangeSpeed(subtitles, 100.0);

        Assert.Equal(1234, subtitles[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(5678, subtitles[0].EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void ChangeSpeed_PreservesDurationRatio()
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new()
            {
                StartTime = TimeSpan.FromMilliseconds(1000),
                EndTime = TimeSpan.FromMilliseconds(3000),
            },
            new()
            {
                StartTime = TimeSpan.FromMilliseconds(5000),
                EndTime = TimeSpan.FromMilliseconds(8000),
            },
        };

        ChangeSpeedViewModel.ChangeSpeed(subtitles, 125.0);

        Assert.Equal(1600, subtitles[0].Duration.TotalMilliseconds, 3);
        Assert.Equal(2400, subtitles[1].Duration.TotalMilliseconds, 3);
    }
}
