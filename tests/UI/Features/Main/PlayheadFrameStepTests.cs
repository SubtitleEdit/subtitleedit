using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace UITests.Features.Main;

/// <summary>
/// The waveform playhead across a native mpv frame step (issue #14245). mpv's `frame-step` is a
/// real un-pause - it sets pause=no, plays until the next video frame, then sets pause=yes - so
/// the observed pause flag flickers and used to read as a play->pause cycle: the edge cleared the
/// "paused and settled" flag on the very tick mpv reported the stepped-to frame, the
/// follow-a-paused-seek branch skipped it, and settling never snaps (#12740). The frame was
/// dropped from the cursor for good and stepping forward drifted a frame per press until the
/// 0.5 s discontinuity snap caught up. (`frame-back-step` seeks and stays paused, which is why
/// stepping backwards always landed correctly.)
/// </summary>
public class PlayheadFrameStepTests
{
    private const double FrameSeconds = 1.0 / 23.976;

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
    }

    [AvaloniaFact]
    public void FrameStepForward_LandsTheCursorOnTheSteppedFrame()
    {
        var (vm, vp, player) = MakeViewModelWithPlayer(startSeconds: 1.0);

        BeginFrameStep(vm);

        // mpv's one-frame un-pause: the pause flag says "playing" while its clock still sits on
        // the old frame. The cursor must not move yet - and must not lose its authority either.
        Assert.Equal(1.0, Tick(vm, vp, isPlaying: true), 4);
        Assert.Equal(1.0, Tick(vm, vp, isPlaying: true), 4);

        // The step lands: mpv reports the new frame and pauses again.
        player.Position = 1.0 + FrameSeconds;
        player.IsPlaying = false;
        Assert.Equal(1.0 + FrameSeconds, Tick(vm, vp, isPlaying: false), 4);
    }

    [AvaloniaFact]
    public void FrameStepForward_Repeated_DoesNotDrift()
    {
        var (vm, vp, player) = MakeViewModelWithPlayer(startSeconds: 1.0);

        for (var step = 1; step <= 10; step++)
        {
            BeginFrameStep(vm);
            player.IsPlaying = true;
            Tick(vm, vp, isPlaying: true);

            player.Position = 1.0 + (step * FrameSeconds);
            player.IsPlaying = false;
            var estimate = Tick(vm, vp, isPlaying: false);

            // Before the fix the cursor stayed at 1.0 while mpv walked forward, so the error grew
            // by a frame per press until the 0.5 s snap.
            Assert.Equal(player.Position, estimate, 4);

            // Idle ticks between key presses must not move it either.
            Assert.Equal(player.Position, Tick(vm, vp, isPlaying: false), 4);
        }
    }

    [AvaloniaFact]
    public void FrameStepWindow_ClosesImmediatelyWhenTheCoreIsReallyPlaying()
    {
        // mpv's frame-step is a no-op without a video track, so an audio-only file keeps playing
        // right through it. The window must not hold the cursor frozen on a running clock: a moving
        // position means the step is not in flight, so it closes on the first tick.
        var (vm, vp, player) = MakeViewModelWithPlayer(startSeconds: 1.0);

        BeginFrameStep(vm);
        player.IsPlaying = true;
        player.Position = 1.2;
        Tick(vm, vp, isPlaying: true);

        Assert.Equal(0L, GetField<long>(vm, "_frameStepFollowUntilTs"));
    }

    [AvaloniaFact]
    public void PauseWindDown_IsStillNotFollowed()
    {
        // The frame-step window is the only thing that lets a paused position change through on a
        // play->pause edge. An ordinary pause must keep dead-freezing the cursor at the keypress
        // instant while mpv decodes on past it (#12740).
        var (vm, vp, player) = MakeViewModelWithPlayer(startSeconds: 1.0);
        SetField(vm, "_playheadWasPlaying", true);

        player.Position = 1.08; // mpv's wind-down running past the frozen spot
        Assert.Equal(1.0, Tick(vm, vp, isPlaying: false), 4);
    }

    private static (MainViewModel Vm, VideoPlayerControl Vp, FakeVideoPlayer Player) MakeViewModelWithPlayer(double startSeconds)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var vm = Locator.Services.GetRequiredService<MainViewModel>();
        var player = new FakeVideoPlayer { Position = startSeconds };
        var vp = new VideoPlayerControl(player);

        // Paused at startSeconds, settled, with the cursor already on that spot.
        SetField(vm, "_playheadEstimateSeconds", startSeconds);
        SetField(vm, "_playheadLastRealSeconds", startSeconds);
        SetField(vm, "_playheadValid", true);
        SetField(vm, "_playheadPausedSettled", true);
        SetField(vm, "_playheadWasPlaying", false);

        return (vm, vp, player);
    }

    private static void BeginFrameStep(MainViewModel vm) =>
        typeof(MainViewModel)
            .GetMethod("BeginFrameStepPlayheadFollow", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(vm, null);

    private static double Tick(MainViewModel vm, VideoPlayerControl vp, bool isPlaying) =>
        (double)typeof(MainViewModel)
            .GetMethod("UpdatePlayheadEstimate", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(vm, [vp, isPlaying])!;

    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(target, value);

    private static T GetField<T>(object target, string name) =>
        (T)target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(target)!;
}
