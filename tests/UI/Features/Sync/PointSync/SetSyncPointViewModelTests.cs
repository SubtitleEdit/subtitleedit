using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
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

    private const string VideoFileName = "/no/such/video.mkv";

    private static SetSyncPointViewModel MakeViewModel()
        => new(new WindowService(new NullServiceProvider()), new FileHelper(), new VideoPreviewSubtitle(new MpvReloader(), new VlcReloader()));

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

        vm.Initialize(lines, lines[1], videoFileName: null, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        Assert.Equal(TimeSpan.FromSeconds(30), vm.SyncPointTimeCode);
    }

    [AvaloniaFact]
    public void Initialize_WithoutVideoOrSelection_SeedsSyncPointFromFirstLine()
    {
        var lines = ThreeLines();
        var vm = MakeViewModel();

        vm.Initialize(lines, null, videoFileName: null, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        Assert.Equal(TimeSpan.FromSeconds(10), vm.SyncPointTimeCode);
    }

    [AvaloniaFact]
    public void Ok_WithoutVideo_ReturnsTheTypedTimeCode()
    {
        // The value a user types is the whole point of the dialog when there is no video to scrub -
        // taking the result off the (empty) player instead gave a sync point of zero.
        var lines = ThreeLines();
        var vm = MakeViewModel();
        vm.Initialize(lines, lines[1], videoFileName: null, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);

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
        vm.Initialize(lines, lines[1], videoFileName: null, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);

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
        vm.Initialize(lines, lines[0], videoFileName: null, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);

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
        vm.Initialize(lines, lines[0], videoFileName: null, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        vm.SelectedParagraphIndex = 2;
        vm.GoToLeftSubtitleCommand.Execute(null);

        Assert.Equal(TimeSpan.FromSeconds(50), vm.SyncPointTimeCode);
    }

    [AvaloniaFact]
    public void EmptyPlayer_WithVideoFile_HidesTheVideo()
    {
        // No libmpv/libVLC could be loaded, so the window got the EmptyVideoPlayer fallback - a
        // player that never plays and always sits at 0:00 (issue #15787).
        var lines = ThreeLines();
        var vm = MakeViewModel();
        vm.Initialize(lines, lines[1], VideoFileName, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);
        Assert.True(vm.IsVideoVisible);

        vm.SetVideoPlayerControl(new VideoPlayerControl(new EmptyVideoPlayer()));

        Assert.False(vm.IsVideoVisible);
        Assert.Equal(TimeSpan.FromSeconds(30), vm.SyncPointTimeCode);
    }

    [AvaloniaFact]
    public void EmptyPlayer_WithVideoFile_TimeCodeIsNotPinnedToZero()
    {
        // The player used to own the sync point, so the nudge buttons seeked the empty player and
        // read its 0 back, and OK always gave 0:00 (issue #15787).
        var lines = ThreeLines();
        var vm = MakeViewModel();
        vm.Initialize(lines, lines[1], VideoFileName, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);
        vm.SetVideoPlayerControl(new VideoPlayerControl(new EmptyVideoPlayer()));

        vm.LeftOneSecondForwardCommand.Execute(null);
        Assert.Equal(TimeSpan.FromSeconds(31), vm.SyncPointTimeCode);

        vm.SyncPointTimeCode = TimeSpan.FromSeconds(42.5);
        vm.OkCommand.Execute(null);

        Assert.True(vm.OkPressed);
        Assert.Equal(42.5, vm.SyncPosition, 3);
        Assert.Equal(VideoFileName, vm.VideoFileName);
    }

    [AvaloniaFact]
    public void WorkingPlayer_WithVideoFile_ShowsTheVideo()
    {
        var lines = ThreeLines();
        var vm = MakeViewModel();
        vm.Initialize(lines, lines[1], VideoFileName, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        vm.SetVideoPlayerControl(new VideoPlayerControl(new WorkingVideoPlayer()));

        Assert.True(vm.IsVideoVisible);
    }
}
