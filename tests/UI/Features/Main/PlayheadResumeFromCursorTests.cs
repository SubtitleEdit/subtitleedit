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
using System.Diagnostics;
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
/// drawn cursor first; seek-then-play paths pin the playhead to their own target and are left
/// alone. The fake player applies seeks asynchronously, like mpv: the first fix pinned the
/// playhead to the seek target, and the pin's 0.15 s arrive tolerance read the NOT-yet-seeked
/// position as already arrived - the cursor was seeded from the stale clock and jumped forward,
/// then back when the seek landed. A synchronous fake could never see that.
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

    /// <summary>Models the LibMpv player's seek semantics, which are what broke the first two
    /// attempts at this fix: a seek is asynchronous (the observed clock keeps the old value until
    /// <see cref="LandSeek"/>), the paused-value cache makes Position read as the seek target
    /// while paused, Play/PlayOrPause clear that cache (so Position flips back to the stale
    /// observed clock mid-resume), and seek completion is reported via the playback-restart
    /// signal. A synchronous fake could never see the phantom pin arrival or the flip.</summary>
    private sealed class FakeVideoPlayer : IVideoPlayer
    {
        private double _observedPosition;
        private double? _pausedSeekTarget; // mirrors LibMpvDynamicPlayer._pausedValue
        private long? _restartTimestamp;

        public FakeVideoPlayer(double startSeconds) => _observedPosition = startSeconds;

        public double? SeekTarget { get; private set; }
        public int SeekCount { get; private set; }

        /// <summary>The async seek completes: the observed clock lands on the target and mpv
        /// posts MPV_EVENT_PLAYBACK_RESTART.</summary>
        public void LandSeek()
        {
            if (SeekTarget is { } t)
            {
                _observedPosition = t;
                _restartTimestamp = Stopwatch.GetTimestamp();
            }
        }

        public string Name => "fake";
        public string FileName { get; private set; } = string.Empty;

        public bool CanLoad() => true;

        public Task LoadFile(string fileName, double startPositionSeconds = 0)
        {
            FileName = fileName;
            _observedPosition = startPositionSeconds;
            return Task.CompletedTask;
        }

        public void CloseFile() => FileName = string.Empty;

        public void Play()
        {
            _pausedSeekTarget = null;
            IsPlaying = true;
        }

        public void PlayOrPause()
        {
            _pausedSeekTarget = null;
            IsPlaying = !IsPlaying;
        }

        public void Pause() => IsPlaying = false;

        public void Stop() => IsPlaying = false;

        public AudioTrackInfo? ToggleAudioTrack() => null;

        public bool IsPlaying { get; set; }
        public bool IsPaused => !IsPlaying;

        public double Position
        {
            get => !IsPlaying && _pausedSeekTarget.HasValue ? _pausedSeekTarget.Value : _observedPosition;
            set
            {
                SeekTarget = value;
                SeekCount++;
                _pausedSeekTarget = value;
            }
        }

        public double Duration => 60;
        public int VolumeMaximum => 100;
        public double Volume { get; set; } = 50;
        public double Speed { get; set; } = 1.0;
        public bool SupportsPlaybackRestartEvents => true;
        public bool HasPlaybackRestartedSince(long stopwatchTimestamp) =>
            _restartTimestamp is { } ts && ts >= stopwatchTimestamp;
    }

    [AvaloniaFact]
    public void ResumeAfterPause_SeeksThePausedPlayerOntoTheVisibleCursor()
    {
        // Paused with the cursor frozen at 1.0 while mpv's wind-down settled at 1.18 - the
        // standing residual #12740 keeps off the cursor.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0, rawSeconds: 1.18);

        vm.CancelPausePlayheadFreeze(); // what every play path does just before playing

        Assert.Equal(1.0, player.SeekTarget ?? -1, 4); // playback will start at the drawn cursor
        Assert.Equal(1.0, GetField<double?>(vm, "_playheadSeekTarget") ?? -1, 4); // pinned there

        // While paused-with-seek-in-flight, Position reads as the seek target (paused-value
        // cache); the restart gate keeps the pin from treating that echo as arrival.
        Assert.Equal(1.0, Tick(vm, vp, isPlaying: false), 3);

        // The unpause clears the paused-value cache: Position flips back to the stale observed
        // clock until the hr-seek lands. Following this flip was the double jump of the second
        // attempt; releasing the pin onto it was the jump of the first.
        player.PlayOrPause();
        Assert.Equal(1.18, player.Position, 4); // the flip is really being exercised
        Assert.Equal(1.0, Tick(vm, vp, isPlaying: true), 3);

        player.LandSeek(); // mpv's restart: the seek landed right where the cursor is held
        Assert.Equal(1.0, Tick(vm, vp, isPlaying: true), 3);
        Assert.Null(GetField<double?>(vm, "_playheadSeekTarget")); // released cleanly onto the target
    }

    [AvaloniaFact]
    public void ResumeAfterPause_ResidualInsideArriveTolerance_PinHoldsUntilTheSeekLands()
    {
        // The typical wind-down residual (~0.05-0.15 s) is smaller than the pin's arrive
        // tolerance, so the stale clock after the unpause flip already reads as "arrived" -
        // the phantom arrival that seeded the cursor from the stale position and made the
        // first fix jump. Only mpv's restart signal may release the pin.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0, rawSeconds: 1.1);

        vm.CancelPausePlayheadFreeze();
        player.PlayOrPause(); // flip: Position reads 1.1 again, within tolerance of the 1.0 pin

        Assert.Equal(1.1, player.Position, 4);
        Assert.Equal(1.0, Tick(vm, vp, isPlaying: true), 3); // held - no restart yet
        Assert.Equal(1.0, GetField<double?>(vm, "_playheadSeekTarget") ?? -1, 4);

        player.LandSeek();
        Assert.Equal(1.0, Tick(vm, vp, isPlaying: true), 3);
    }

    [AvaloniaFact]
    public void PauseWindDown_OnceSettled_AlignsThePlayerWithTheCursor()
    {
        // The preferred moment for the alignment seek is the pause settle, not the play: aligned
        // while paused, mpv has the whole pause to re-prime and play stays a hot, instant
        // unpause. Seeking at play time started the audio output starved - a brief stutter,
        // then a forward hop when the clock recovered.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0, rawSeconds: 1.18);
        SetField(vm, "_playheadPausedSettled", false); // the wind-down has just come to rest

        Assert.Equal(1.0, Tick(vm, vp, isPlaying: false), 3); // the settle tick holds the cursor...
        Assert.Equal(1, player.SeekCount); // ...and moves mpv onto it instead (#12740 intact)
        Assert.Equal(1.0, player.SeekTarget ?? -1, 4);

        player.LandSeek();
        Assert.Equal(1.0, Tick(vm, vp, isPlaying: false), 3); // pin releases onto the cursor spot

        // Play much later: everything is already aligned, so no seek and no cold start.
        vm.CancelPausePlayheadFreeze();
        Assert.Equal(1, player.SeekCount);
        player.PlayOrPause();
        Assert.Equal(1.0, Tick(vm, vp, isPlaying: true), 3);
    }

    [AvaloniaFact]
    public void ResumeAfterPause_WithinAFrame_DoesNotSeek()
    {
        // A residual too small to see is not worth an hr-seek on every resume.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0, rawSeconds: 1.02);

        vm.CancelPausePlayheadFreeze();

        Assert.Equal(0, player.SeekCount);
    }

    [AvaloniaFact]
    public void ResumeAfterPause_DiscontinuityGap_DoesNotTrustTheEstimate()
    {
        // A gap at the discontinuity threshold means the estimate is not what stands on screen
        // (the paused branch snaps such a gap to mpv within a tick) - e.g. an unpinned foreign
        // seek still in flight. Resuming must not pull the player back to the stale spot.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0, rawSeconds: 3.0);

        vm.CancelPausePlayheadFreeze();

        Assert.Equal(0, player.SeekCount);
    }

    [AvaloniaFact]
    public void SeekThenPlay_PinnedTargetIsLeftAlone()
    {
        // Play line / play next / grid click-to-play: seek to the target, pin, then play. The
        // resume-from-cursor seek must not override the pinned target with the old cursor spot.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0, rawSeconds: 1.18);

        PinPlayheadTo(vm, 9.0); // the caller's own seek target (its vp.Position write is async too)
        vm.CancelPausePlayheadFreeze();

        Assert.Equal(0, player.SeekCount); // no second seek was issued
        Assert.Equal(9.0, GetField<double?>(vm, "_playheadSeekTarget") ?? -1, 4);
    }

    [AvaloniaFact]
    public void SecondFunnelPass_DoesNotSeekAgain()
    {
        // TogglePlayPause funnels through CancelPausePlayheadFreeze twice (the view model command
        // and the control's PlayPauseRequested wiring); the pin set by the first pass makes the
        // second a no-op, else it would issue the same seek again.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0, rawSeconds: 1.18);

        vm.CancelPausePlayheadFreeze();
        vm.CancelPausePlayheadFreeze();

        Assert.Equal(1, player.SeekCount);
    }

    [AvaloniaFact]
    public void ResumeWhilePlaying_DoesNothing()
    {
        // With mpv's clock live there is no settled residual; the drift logic owns the cursor.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0, rawSeconds: 1.2);
        player.IsPlaying = true;

        vm.CancelPausePlayheadFreeze();

        Assert.Equal(0, player.SeekCount);
    }

    [AvaloniaFact]
    public void TogglePlayPause_WhilePaused_StartsPlaybackAtTheCursor()
    {
        // End to end through the toggle command: the seek is issued before the play command.
        var (vm, vp, player) = MakeViewModelWithPlayer(cursorSeconds: 1.0, rawSeconds: 1.18);

        vm.TogglePlayPauseCommand.Execute(null);

        Assert.True(player.IsPlaying);
        Assert.Equal(1, player.SeekCount);
        Assert.Equal(1.0, player.SeekTarget ?? -1, 4);

        Assert.Equal(1.0, Tick(vm, vp, isPlaying: true), 2); // stale clock until the seek lands
        player.LandSeek();
        Assert.Equal(1.0, player.Position, 4);
        Assert.Equal(1.0, Tick(vm, vp, isPlaying: true), 2);
    }

    private (MainViewModel Vm, VideoPlayerControl Vp, FakeVideoPlayer Player) MakeViewModelWithPlayer(double cursorSeconds, double rawSeconds)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var vm = Locator.Services.GetRequiredService<MainViewModel>();
        var player = new FakeVideoPlayer(rawSeconds);
        var vp = new VideoPlayerControl(player);

        vm.VideoPlayerControl = vp;
        _vm = vm; // detached again in Dispose - see the cascade note on the field
        vp.Duration = 60; // published, or the bound position slider clamps the display write
        vp.SetPositionDisplayOnly(rawSeconds);
        SetField(vm, "_videoFileName", "video.mkv");

        // A settled pause: the cursor froze at cursorSeconds when the user paused, mpv's
        // wind-down then came to rest at rawSeconds, and the settle logic saw it stop there.
        SetField(vm, "_playheadEstimateSeconds", cursorSeconds);
        SetField(vm, "_playheadLastRealSeconds", rawSeconds);
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
