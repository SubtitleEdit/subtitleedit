using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;
using System.Reflection;

namespace UITests.Features.Tools.ImproveTimeCodes;

public class ImproveTimeCodesViewModelTests
{
    private static (ImproveTimeCodesViewModel Vm, List<SubtitleLineViewModel> Lines) MakeWithResults()
    {
        var lines = Enumerable.Range(0, 4).Select(i => new SubtitleLineViewModel
        {
            Number = i + 1,
            Text = $"Line {i}",
            StartTime = TimeSpan.FromSeconds(10 + (i * 3)),
            EndTime = TimeSpan.FromSeconds(12 + (i * 3)),
        }).ToList();

        var vm = new ImproveTimeCodesViewModel(new StubWindowService());
        vm.Initialize(lines, new AudioVisualizer(), "video.mkv", -1, "en");

        var results = new List<SubtitleRetimer.LineResult>
        {
            new(10.3, 11.8, SubtitleRetimer.LineStatus.Retimed),
            new(13, 15, SubtitleRetimer.LineStatus.Unchanged),
            new(16, 18, SubtitleRetimer.LineStatus.ShiftTooLarge),
            new(18.6, 20.9, SubtitleRetimer.LineStatus.Retimed),
        };

        typeof(ImproveTimeCodesViewModel)
            .GetMethod("ShowResults", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new object[] { results });
        return (vm, lines);
    }

    [Fact]
    public void Results_OnlyRetimedLinesCarryNewTimes()
    {
        var (vm, lines) = MakeWithResults();

        var aligned = vm.GetAlignedSubtitles();

        Assert.True(vm.HasResult);
        Assert.Equal(10.3, aligned[0].StartTime.TotalSeconds, 3);
        Assert.Equal(11.8, aligned[0].EndTime.TotalSeconds, 3);
        Assert.Equal(lines[2].StartTime, aligned[2].StartTime);
        Assert.Equal(18.6, aligned[3].StartTime.TotalSeconds, 3);
        Assert.Equal("+300 ms", vm.Rows[0].StartShift);
        Assert.Equal("−200 ms", vm.Rows[0].EndShift);
        Assert.Equal(string.Empty, vm.Rows[2].StartShift);
    }

    [Fact]
    public void UntickingARow_RestoresItsOriginalTimes_AndOkNeedsSomethingTicked()
    {
        var (vm, lines) = MakeWithResults();

        vm.Rows[0].Apply = false;
        Assert.Equal(lines[0].StartTime, vm.GetAlignedSubtitles()[0].StartTime);
        Assert.True(vm.HasResult);

        vm.Rows[3].Apply = false;
        Assert.False(vm.HasResult);
    }

    [Fact]
    public void AnUnconfirmedLargeMove_IsListedButNotTicked()
    {
        var lines = Enumerable.Range(0, 2).Select(i => new SubtitleLineViewModel
        {
            Number = i + 1,
            Text = $"Line {i}",
            StartTime = TimeSpan.FromSeconds(10 + (i * 3)),
            EndTime = TimeSpan.FromSeconds(12 + (i * 3)),
        }).ToList();
        var vm = new ImproveTimeCodesViewModel(new StubWindowService());
        vm.Initialize(lines, new AudioVisualizer(), "video.mkv", -1, "en");

        typeof(ImproveTimeCodesViewModel)
            .GetMethod("ShowResults", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new object[]
            {
                new List<SubtitleRetimer.LineResult>
                {
                    new(10.1, 12.1, SubtitleRetimer.LineStatus.Retimed),
                    new(13.9, 15.9, SubtitleRetimer.LineStatus.LargeMoveUnconfirmed),
                },
            });

        Assert.True(vm.Rows[1].IsChanged);
        Assert.False(vm.Rows[1].Apply);
        Assert.Equal(lines[1].StartTime, vm.GetAlignedSubtitles()[1].StartTime);

        vm.Rows[1].Apply = true;
        Assert.Equal(13.9, vm.GetAlignedSubtitles()[1].StartTime.TotalSeconds, 3);
    }

    [Fact]
    public void SpeechToTextVerdicts_ConfirmedIsTicked_DisputedIsNot_AndTheHeardShareIsShown()
    {
        var lines = Enumerable.Range(0, 3).Select(i => new SubtitleLineViewModel
        {
            Number = i + 1,
            Text = $"Line {i}",
            StartTime = TimeSpan.FromSeconds(10 + (i * 3)),
            EndTime = TimeSpan.FromSeconds(12 + (i * 3)),
        }).ToList();
        var vm = new ImproveTimeCodesViewModel(new StubWindowService());
        vm.Initialize(lines, new AudioVisualizer(), "video.mkv", -1, "en");
        Assert.False(vm.HasHeard);

        typeof(ImproveTimeCodesViewModel)
            .GetMethod("ShowResults", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new object[]
            {
                new List<SubtitleRetimer.LineResult>
                {
                    new(9, 11, SubtitleRetimer.LineStatus.ConfirmedBySpeech) { HeardRatio = 1.0 },
                    new(12, 14, SubtitleRetimer.LineStatus.DisputedBySpeech) { HeardRatio = 0.75 },
                    new(16, 18, SubtitleRetimer.LineStatus.Unchanged) { HeardRatio = 0.0 },
                },
            });

        Assert.True(vm.HasHeard);
        Assert.True(vm.Rows[0].Apply);
        Assert.True(vm.Rows[1].IsChanged);
        Assert.False(vm.Rows[1].Apply);
        Assert.Equal("100%", vm.Rows[0].Heard);
        Assert.Equal("75%", vm.Rows[1].Heard);
        Assert.Equal(9, vm.GetAlignedSubtitles()[0].StartTime.TotalSeconds, 3);
        Assert.Equal(lines[1].StartTime, vm.GetAlignedSubtitles()[1].StartTime);
    }

    [Fact]
    public void SpeechToTextCheck_IsOnlyOfferedForLanguagesItCanHear()
    {
        var lines = new List<SubtitleLineViewModel> { new() { Text = "Hej", StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(2) } };

        var danish = new ImproveTimeCodesViewModel(new StubWindowService());
        danish.Initialize(lines, new AudioVisualizer(), "video.mkv", -1, "da");
        var japanese = new ImproveTimeCodesViewModel(new StubWindowService());
        japanese.Initialize(lines, new AudioVisualizer(), "video.mkv", -1, "ja");

        Assert.True(danish.CanCheckWithSpeechToText);
        Assert.False(japanese.CanCheckWithSpeechToText);
    }

    [Fact]
    public void ChangeNavigation_StepsOverRetimedLinesOnly()
    {
        var (vm, _) = MakeWithResults();

        Assert.Equal(0, vm.SelectedRow!.Index);
        Assert.False(vm.CanGoPrevious);
        Assert.True(vm.CanGoNext);

        vm.NextChangeCommand.Execute(null);

        Assert.Equal(3, vm.SelectedRow!.Index);
        Assert.False(vm.CanGoNext);
        Assert.Equal(string.Format(Nikse.SubtitleEdit.Logic.Config.Se.Language.Tools.ImproveTimeCodes.ChangeXOfY, 2, 2), vm.ChangePositionLabel);
    }
}
