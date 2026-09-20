using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;

namespace UITests.Features.Sync;

/// <summary>
/// Initialize posts the start of the position pump to the dispatcher. A dialog closed before
/// that post ran had its players disposed by OnClosing, and then got a brand new pump that
/// nothing ever stopped: it polled the dead players for the rest of the process, every poll an
/// "LibMpvDynamicPlayer method called after disposal" entry - 2.8 million lines of them in a
/// real error log, most written by this very test suite, which closes dialogs without ever
/// pumping the dispatcher.
/// </summary>
public class SyncDialogClosedBeforeTimerStartTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class NoPreviewSubtitle : IVideoPreviewSubtitle
    {
        public void Refresh(IVideoPlayer? videoPlayer, Func<Subtitle> getSubtitle, VideoPreviewSubtitleContext context)
        {
        }

        public void Invalidate()
        {
        }

        public void Reset()
        {
        }
    }

    private static List<SubtitleLineViewModel> TwoLines()
        => new()
        {
            new() { StartTime = TimeSpan.FromSeconds(10), EndTime = TimeSpan.FromSeconds(12), Text = "One" },
            new() { StartTime = TimeSpan.FromSeconds(30), EndTime = TimeSpan.FromSeconds(32), Text = "Two" },
        };

    [AvaloniaFact]
    public void VisualSync_ClosedBeforeThePostedStart_NeverStartsThePositionTimer()
    {
        var vm = new VisualSyncViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), new NoPreviewSubtitle(), new NoPreviewSubtitle());
        vm.Initialize(TwoLines(), videoFileName: null, subtitleFileName: null, VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        vm.OnClosing();
        Dispatcher.UIThread.RunJobs();

        Assert.False(vm.IsPositionTimerRunning);
    }

    [AvaloniaFact]
    public void VisualSync_LeftOpen_StartsThePositionTimer_AndClosingStopsIt()
    {
        var vm = new VisualSyncViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), new NoPreviewSubtitle(), new NoPreviewSubtitle());
        vm.Initialize(TwoLines(), videoFileName: null, subtitleFileName: null, VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        Dispatcher.UIThread.RunJobs();
        Assert.True(vm.IsPositionTimerRunning);

        vm.OnClosing();
        Assert.False(vm.IsPositionTimerRunning);
    }

    [AvaloniaFact]
    public void SetSyncPoint_ClosedBeforeThePostedStart_NeverStartsThePositionTimer()
    {
        var vm = new SetSyncPointViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), new NoPreviewSubtitle());
        var lines = TwoLines();
        vm.Initialize(lines, lines[0], videoFileName: null, subtitleFileName: null, VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        vm.OnClosing();
        Dispatcher.UIThread.RunJobs();

        Assert.False(vm.IsPositionTimerRunning);
    }

    [AvaloniaFact]
    public void SetSyncPoint_LeftOpen_StartsThePositionTimer_AndClosingStopsIt()
    {
        var vm = new SetSyncPointViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), new NoPreviewSubtitle());
        var lines = TwoLines();
        vm.Initialize(lines, lines[0], videoFileName: null, subtitleFileName: null, VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        Dispatcher.UIThread.RunJobs();
        Assert.True(vm.IsPositionTimerRunning);

        vm.OnClosing();
        Assert.False(vm.IsPositionTimerRunning);
    }
}
