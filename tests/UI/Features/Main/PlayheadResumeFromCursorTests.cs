using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace UITests.Features.Main;

/// <summary>
/// A play command must start playback from the cursor the user sees. While paused the waveform
/// cursor deliberately holds the spot where the user paused while mpv settles a wind-down further
/// on (#12740 keeps that residual off the cursor). Resuming used to start from mpv's settled spot
/// and yank the cursor forward to meet it - sub-visible while SE forced a 0.05 s audio buffer,
/// but up to ~0.2 s again once the buffer went back to mpv's default for #14523. Every play
/// request funnels through CancelPausePlayheadFreeze, which now seeks the paused player onto the
/// drawn cursor first; seek-then-play paths pin the playhead to their own target and are left alone.
/// </summary>
public class PlayheadResumeFromCursorTests : IDisposable
{
    // Tests that drive commands through GetVideoPlayerControl() must not leave their test-built
    // VideoPlayerControl assigned to the view model: an unparented control left reachable on the
    // per-assembly headless application is the documented contamination-cascade trigger - unrelated
    // windows start failing out of the compositor on CI, a different victim per run (PR #14258).
    private MainViewModel? _vm;

    public void Dispose()
    {
        if (_vm != null)
        {
            _vm.VideoPlayerControl = null;
            _vm.AudioVisualizer = null;
        }
    }

    private sealed class FakeVideoPlayer : IVideoPlayer
    {
        public string Name => "fake";
        public string FileName { get; private set; } = string.Empty;

        public bool CanLoad() => true;

        public Task LoadFile(string fileName, double startPositionSeconds = 0)
        {
            FileName = fileName;
            Position = startPositionSeconds;
            return Task.CompletedTask;
        }

        public void CloseFile() => FileName = string.Empty;

        public void Play() => IsPlaying = true;

        public void PlayOrPause() => IsPlaying = !IsPlaying;

        public void Pause() => IsPlaying = false;

        public void Stop() => IsPlaying = false;

        public AudioTrackInfo? ToggleAudioTrack() => null;

        public bool IsPlaying { get; set; }
        public bool IsPaused => !IsPlaying;
        public double Position { get; set; }
        public double Duration => 60;
        public int VolumeMaximum => 100;
        public double Volume { get; set; } = 50;
        public double Speed { get; set; } = 1.0;
        public bool SupportsPlaybackRestartEvents => false;
        public bool HasPlaybackRestartedSince(long stopwatchTimestamp) => false;
    }

    [AvaloniaFact]
    public void ResumeAfterPause_SeeksThePausedPlayerOntoTheVisibleCursor()
    {
        // Paused with the cursor frozen at 1.0 while mpv's wind-down settled at 1.18 - the
        // standing residual #12740 keeps off the cursor.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0);
        player.Position = 1.18;

        vm.CancelPausePlayheadFreeze(); // what every play path does just before playing

        Assert.Equal(1.0, player.Position, 4); // playback will start at the drawn cursor
        Assert.Equal(1.0, GetField<double?>(vm, "_playheadSeekTarget") ?? -1, 4); // and the cursor is pinned there
        Assert.Equal(1.0, Tick(vm, vp, isPlaying: false), 4);
    }

    [AvaloniaFact]
    public void ResumeAfterPause_WithinAFrame_DoesNotSeek()
    {
        // A residual too small to see is not worth an hr-seek on every resume.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0);
        player.Position = 1.02;

        vm.CancelPausePlayheadFreeze();

        Assert.Equal(1.02, player.Position, 4);
        Assert.Null(GetField<double?>(vm, "_playheadSeekTarget"));
    }

    [AvaloniaFact]
    public void ResumeAfterPause_DiscontinuityGap_DoesNotTrustTheEstimate()
    {
        // A gap at the discontinuity threshold means the estimate is not what stands on screen
        // (the paused branch snaps such a gap to mpv within a tick) - e.g. an unpinned foreign
        // seek still in flight. Resuming must not pull the player back to the stale spot.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0);
        player.Position = 3.0;

        vm.CancelPausePlayheadFreeze();

        Assert.Equal(3.0, player.Position, 4);
        Assert.Null(GetField<double?>(vm, "_playheadSeekTarget"));
    }

    [AvaloniaFact]
    public void SeekThenPlay_PinnedTargetIsLeftAlone()
    {
        // Play line / play next / grid click-to-play: seek to the target, pin, then play. The
        // resume-from-cursor seek must not override the pinned target with the old cursor spot.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0);
        player.Position = 1.18; // standing pause residual, as above

        PinPlayheadTo(vm, 9.0); // the caller's own seek target (its vp.Position write is async)
        vm.CancelPausePlayheadFreeze();

        Assert.Equal(1.18, player.Position, 4); // no second seek was issued
        Assert.Equal(9.0, GetField<double?>(vm, "_playheadSeekTarget") ?? -1, 4);
    }

    [AvaloniaFact]
    public void ResumeWhilePlaying_DoesNothing()
    {
        // With mpv's clock live there is no settled residual; the drift logic owns the cursor.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0);
        player.Position = 1.2;
        player.IsPlaying = true;

        vm.CancelPausePlayheadFreeze();

        Assert.Equal(1.2, player.Position, 4);
        Assert.Null(GetField<double?>(vm, "_playheadSeekTarget"));
    }

    [AvaloniaFact]
    public void TogglePlayPause_WhilePaused_StartsPlaybackAtTheCursor()
    {
        // End to end through the toggle command: the seek lands before the play command.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0);
        player.Position = 1.18;

        vm.TogglePlayPauseCommand.Execute(null);

        Assert.True(player.IsPlaying);
        Assert.Equal(1.0, player.Position, 4);
    }

    private (MainViewModel Vm, VideoPlayerControl Vp, FakeVideoPlayer Player) MakeViewModelWithPlayer(double cursorSeconds)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var vm = Locator.Services.GetRequiredService<MainViewModel>();
        var player = new FakeVideoPlayer { Position = cursorSeconds };
        var vp = new VideoPlayerControl(player);

        vm.VideoPlayerControl = vp;
        _vm = vm; // detached again in Dispose - see the cascade note on the field
        vp.Duration = 60; // published, or the bound position slider clamps the display write
        vp.SetPositionDisplayOnly(cursorSeconds);
        SetField(vm, "_videoFileName", "video.mkv");

        // Paused at cursorSeconds, settled, with the cursor already on that spot.
        SetField(vm, "_playheadEstimateSeconds", cursorSeconds);
        SetField(vm, "_playheadLastRealSeconds", cursorSeconds);
        SetField(vm, "_playheadValid", true);
        SetField(vm, "_playheadPausedSettled", true);
        SetField(vm, "_playheadWasPlaying", false);

        return (vm, vp, player);
    }

    private static void PinPlayheadTo(MainViewModel vm, double seconds) =>
        typeof(MainViewModel)
            .GetMethod("PinPlayheadTo", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(vm, [seconds]);

    private static double Tick(MainViewModel vm, VideoPlayerControl vp, bool isPlaying) =>
        (double)typeof(MainViewModel)
            .GetMethod("UpdatePlayheadEstimate", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(vm, [vp, isPlaying])!;

    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(target, value);

    private static T? GetField<T>(object target, string name) =>
        (T?)target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(target);
}
