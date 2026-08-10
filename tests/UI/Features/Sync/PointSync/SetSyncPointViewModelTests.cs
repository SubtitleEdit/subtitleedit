using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace UITests.Features.Sync.PointSync;

/// <summary>
/// "Set sync point" without a video (issue #13341). The video is only one of the ways to drive the
/// sync point - the time code box is the result - so the dialog has to work with no video at all:
/// the box starts at the selected line's time, the nudge buttons move it, and OK returns it.
/// </summary>
public class SetSyncPointViewModelTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static SetSyncPointViewModel MakeViewModel()
        => new(new WindowService(new NullServiceProvider()), new FileHelper());

    private static List<SubtitleLineViewModel> ThreeLines()
        => new()
        {
            new() { StartTime = TimeSpan.FromSeconds(10), EndTime = TimeSpan.FromSeconds(12) },
            new() { StartTime = TimeSpan.FromSeconds(30), EndTime = TimeSpan.FromSeconds(32) },
            new() { StartTime = TimeSpan.FromSeconds(50), EndTime = TimeSpan.FromSeconds(52) },
        };

    [AvaloniaFact]
    public void Initialize_WithoutVideo_SeedsSyncPointFromSelectedLine()
    {
        var lines = ThreeLines();
        var vm = MakeViewModel();

        vm.Initialize(lines, lines[1], videoFileName: null, subtitleFileName: null, audioVisualizer: null);

        Assert.Equal(TimeSpan.FromSeconds(30), vm.SyncPointTimeCode);
    }

    [AvaloniaFact]
    public void Initialize_WithoutVideoOrSelection_SeedsSyncPointFromFirstLine()
    {
        var lines = ThreeLines();
        var vm = MakeViewModel();

        vm.Initialize(lines, null, videoFileName: null, subtitleFileName: null, audioVisualizer: null);

        Assert.Equal(TimeSpan.FromSeconds(10), vm.SyncPointTimeCode);
    }

    [AvaloniaFact]
    public void Ok_WithoutVideo_ReturnsTheTypedTimeCode()
    {
        // The value a user types is the whole point of the dialog when there is no video to scrub -
        // taking the result off the (empty) player instead gave a sync point of zero.
        var lines = ThreeLines();
        var vm = MakeViewModel();
        vm.Initialize(lines, lines[1], videoFileName: null, subtitleFileName: null, audioVisualizer: null);

        vm.SyncPointTimeCode = TimeSpan.FromSeconds(42.5);
        vm.OkCommand.Execute(null);

        Assert.True(vm.OkPressed);
        Assert.Equal(42.5, vm.SyncPosition, 3);
    }

    [AvaloniaFact]
    public void NudgeButtons_WithoutVideo_MoveTheTimeCode()
    {
        var lines = ThreeLines();
        var vm = MakeViewModel();
        vm.Initialize(lines, lines[1], videoFileName: null, subtitleFileName: null, audioVisualizer: null);

        vm.LeftOneSecondForwardCommand.Execute(null);
        vm.LeftHalfSecondForwardCommand.Execute(null);

        Assert.Equal(TimeSpan.FromSeconds(31.5), vm.SyncPointTimeCode);

        vm.LeftOneSecondBackCommand.Execute(null);
        vm.LeftHalfSecondBackCommand.Execute(null);

        Assert.Equal(TimeSpan.FromSeconds(30), vm.SyncPointTimeCode);
    }

    [AvaloniaFact]
    public void NudgeBack_WithoutVideo_StopsAtZero()
    {
        var lines = ThreeLines();
        var vm = MakeViewModel();
        vm.Initialize(lines, lines[0], videoFileName: null, subtitleFileName: null, audioVisualizer: null);

        for (var i = 0; i < 20; i++)
        {
            vm.LeftOneSecondBackCommand.Execute(null);
        }

        Assert.Equal(TimeSpan.Zero, vm.SyncPointTimeCode);
    }

    [AvaloniaFact]
    public void GoToSubtitle_WithoutVideo_MovesTheTimeCodeToThatLine()
    {
        var lines = ThreeLines();
        var vm = MakeViewModel();
        vm.Initialize(lines, lines[0], videoFileName: null, subtitleFileName: null, audioVisualizer: null);

        vm.SelectedParagraphIndex = 2;
        vm.GoToLeftSubtitleCommand.Execute(null);

        Assert.Equal(TimeSpan.FromSeconds(50), vm.SyncPointTimeCode);
    }
}
