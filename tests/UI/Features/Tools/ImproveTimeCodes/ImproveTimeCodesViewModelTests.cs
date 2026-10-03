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

    private static ImproveTimeCodesViewModel MakeWith(List<SubtitleLineViewModel> lines, List<SubtitleRetimer.LineResult> results)
    {
        var vm = new ImproveTimeCodesViewModel(new StubWindowService());
        vm.Initialize(lines, new AudioVisualizer(), "video.mkv", -1, "en");
        typeof(ImproveTimeCodesViewModel)
            .GetMethod("ShowResults", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new object[] { results });
        return vm;
    }

    private static List<SubtitleLineViewModel> MakeLines(params (double Start, double End)[] times)
        => times.Select((t, i) => new SubtitleLineViewModel
        {
            Number = i + 1,
            Text = $"Line {i}",
            StartTime = TimeSpan.FromSeconds(t.Start),
            EndTime = TimeSpan.FromSeconds(t.End),
        }).ToList();

    private static void AssertNoOverlaps(IReadOnlyList<SubtitleLineViewModel> aligned)
    {
        for (var i = 1; i < aligned.Count; i++)
        {
            Assert.True(aligned[i].StartTime >= aligned[i - 1].EndTime, $"line {i + 1} starts before line {i} ends");
        }
    }

    [Fact]
    public void AfterASync_AnUntickedLine_StaysWithTheSync_NotWhereItCameIn()
    {
        // The subtitle was 0.8 s early and synced first; the aligner's move of line 1 is disputed.
        var lines = MakeLines((10, 12), (12.1, 14), (14.1, 16));
        var vm = MakeWith(lines, new List<SubtitleRetimer.LineResult>
        {
            new(10.8, 12.8, SubtitleRetimer.LineStatus.MovedWithSync),
            new(12.3, 14.7, SubtitleRetimer.LineStatus.DisputedBySpeech) { Fallback = (12.9, 14.8) },
            new(14.9, 16.8, SubtitleRetimer.LineStatus.Retimed) { Fallback = (14.9, 16.8) },
        });

        var aligned = vm.GetAlignedSubtitles();
        Assert.False(vm.Rows[1].Apply);
        Assert.Equal(12.9, aligned[1].StartTime.TotalSeconds, 3);
        AssertNoOverlaps(aligned);
    }

    [Fact]
    public void TickingAMoveThatRunsIntoANeighbour_TrimsTheNeighbour_NotTheMove()
    {
        var lines = MakeLines((10, 12), (12.1, 14));
        var vm = MakeWith(lines, new List<SubtitleRetimer.LineResult>
        {
            new(10.2, 12.0, SubtitleRetimer.LineStatus.Retimed),
            new(11.6, 13.6, SubtitleRetimer.LineStatus.LargeMoveUnconfirmed),
        });

        vm.Rows[1].Apply = true;

        var aligned = vm.GetAlignedSubtitles();
        Assert.Equal(11.6, aligned[1].StartTime.TotalSeconds, 3);
        AssertNoOverlaps(aligned);
        Assert.True(aligned[0].EndTime < aligned[1].StartTime);
        Assert.Equal(aligned[0].EndTime.TotalMilliseconds - 12000, vm.Rows[0].EndShiftMs, 0); // the grid shows what is applied
    }

    [Fact]
    public void UntickingAMove_ThatTheNeighbourHadMadeRoomFor_DoesNotOverlap()
    {
        var lines = MakeLines((10, 12.5), (12.6, 14));
        var vm = MakeWith(lines, new List<SubtitleRetimer.LineResult>
        {
            new(10.2, 12.4, SubtitleRetimer.LineStatus.Retimed),
            new(12.0, 13.6, SubtitleRetimer.LineStatus.Retimed),
        });

        vm.Rows[0].Apply = false; // back to 10 - 12.5, under line 1's new start

        AssertNoOverlaps(vm.GetAlignedSubtitles());
        Assert.Equal(10.0, vm.GetAlignedSubtitles()[0].StartTime.TotalSeconds, 3);
    }

    [Fact]
    public void ALineDraggedInTheAlignedWaveform_IsAdjustedByHand_AndCanBeUndone()
    {
        var lines = MakeLines((10, 12), (13, 15), (16, 18));
        var vm = MakeWith(lines, new List<SubtitleRetimer.LineResult>
        {
            new(10.2, 12.2, SubtitleRetimer.LineStatus.Retimed),
            new(13, 15, SubtitleRetimer.LineStatus.Unchanged),
            new(16, 18, SubtitleRetimer.LineStatus.ShiftTooLarge),
        });

        // What the waveform does to the line it drags.
        var dragged = vm.GetAlignedSubtitles()[2];
        dragged.SetTimes(TimeSpan.FromSeconds(16.4), TimeSpan.FromSeconds(18.9));
        vm.OnAlignedDragEnded();

        var row = vm.Rows[2];
        Assert.Equal(SubtitleRetimer.LineStatus.AdjustedByHand, row.Status);
        Assert.True(row.Apply);
        Assert.Equal("+400 ms", row.StartShift);
        Assert.Equal(16.4, vm.GetAlignedSubtitles()[2].StartTime.TotalSeconds, 3);
        Assert.Equal(SubtitleRetimer.LineStatus.Unchanged, vm.Rows[1].Status); // nothing else was touched

        vm.UndoAdjustmentCommand.Execute(row);

        Assert.Equal(SubtitleRetimer.LineStatus.ShiftTooLarge, row.Status);
        Assert.Equal(16.0, vm.GetAlignedSubtitles()[2].StartTime.TotalSeconds, 3);
    }

    [Fact]
    public void ALineDraggedBeforeAligning_IsKept()
    {
        var lines = MakeLines((10, 12), (13, 15));
        var vm = new ImproveTimeCodesViewModel(new StubWindowService());
        vm.Initialize(lines, new AudioVisualizer(), "video.mkv", -1, "en");

        vm.GetAlignedSubtitles()[0].SetTimes(TimeSpan.FromSeconds(9.5), TimeSpan.FromSeconds(11.5));
        vm.OnAlignedDragEnded();

        Assert.True(vm.HasResult);
        Assert.Equal(9.5, vm.GetAlignedSubtitles()[0].StartTime.TotalSeconds, 3);
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
