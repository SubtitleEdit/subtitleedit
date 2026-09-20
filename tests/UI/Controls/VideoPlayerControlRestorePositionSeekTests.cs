using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Threading.Tasks;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// Covers issue #14741: any settings change rebuilds the video player, and the rebuild has to
/// seek the fresh player back to where the user was. That restore used to be a fixed 100 ms of
/// assignments to <c>VideoPlayerControl.Position</c>, which a long file loses routinely - mpv
/// swallows seeks while it is still loading, and the property reaches the player only through
/// the position slider, which clamps the write to a duration that is not published yet. The
/// video then sat at 0:00 after Options/OK, and the waveform, which follows the play-head, sat
/// at the first line of the file while the subtitle grid kept the user's row.
/// </summary>
public class VideoPlayerControlRestorePositionSeekTests
{
    /// <summary>A player that swallows seeks for a while after the file is loaded, the way mpv
    /// does while its core is still coming up on a big file.</summary>
    private sealed class SlowToSeekVideoPlayer : IVideoPlayer
    {
        private double _position;
        private long _acceptSeeksFromTicks;

        /// <summary>How long after the load this player ignores seeks. Longer than the old
        /// restore's whole budget, shorter than the new one's.</summary>
        public int LoadingMs { get; set; } = 700;

        public bool SwallowEverySeek { get; set; }

        public string Name => "slow-to-seek";
        public string FileName { get; private set; } = string.Empty;

        public bool CanLoad() => true;

        public Task LoadFile(string fileName, double startPositionSeconds = 0)
        {
            FileName = fileName;
            _position = 0;
            _acceptSeeksFromTicks = Environment.TickCount64 + LoadingMs;
            return Task.CompletedTask;
        }

        public void CloseFile() => FileName = string.Empty;

        public void Play()
        {
        }

        public void PlayOrPause()
        {
        }

        public void Pause()
        {
        }

        public void Stop()
        {
        }

        public AudioTrackInfo? ToggleAudioTrack() => null;

        public bool IsPlaying => false;
        public bool IsPaused => true;

        /// <summary>Every write to <see cref="Position"/>, taken or swallowed.</summary>
        public int SeekCount { get; private set; }

        public double Position
        {
            get => _position;
            set
            {
                SeekCount++;
                if (!SwallowEverySeek && Environment.TickCount64 >= _acceptSeeksFromTicks)
                {
                    _position = value;
                }
            }
        }

        public double Duration => 2595; // a 43 minute file, like the one in the issue
        public int VolumeMaximum => 100;
        public double Volume { get; set; } = 50;
        public double Speed { get; set; } = 1.0;
    }

    [AvaloniaFact]
    public async Task ARestoreKeepsSeekingUntilTheLoadingPlayerTakesIt()
    {
        var player = new SlowToSeekVideoPlayer();
        var control = new VideoPlayerControl(player);
        control.BeginPositionRestore(54);
        await control.Open("fake.mkv");

        await control.RestorePositionAsync(54);

        Assert.Equal(54, player.Position, 3);

        // The player is there, so a later rebuild reads the live position rather than a target
        // left pinned for the rest of this control's life (issue #14218).
        Assert.Equal(control.Position, control.PositionForRestore);
    }

    [AvaloniaFact]
    public async Task ARestoreThatNeverLandsGivesUpButKeepsItsTarget()
    {
        var player = new SlowToSeekVideoPlayer { SwallowEverySeek = true };
        var control = new VideoPlayerControl(player);
        control.BeginPositionRestore(54);
        await control.Open("fake.mkv");

        await control.RestorePositionAsync(54, 300);

        // Nothing to hand a rebuild but the target: the live position is the 0 of a player that
        // never got where it was told to go (issue #14218).
        Assert.Equal(54, control.PositionForRestore);
    }

    /// <summary>
    /// Issue #15027: while the rebuilt player is still loading it reports 0, and the waveform
    /// cursor followed that to the start of the waveform and back. The control offers the restore
    /// target to hold the play-head on instead - for as long as the restore is really under way.
    /// </summary>
    [AvaloniaFact]
    public async Task ThePlayheadHoldIsOfferedWhileTheRestoreIsInFlightAndDroppedOnArrival()
    {
        var player = new SlowToSeekVideoPlayer();
        var control = new VideoPlayerControl(player);
        Assert.Null(control.PositionRestoreHoldSeconds);

        control.BeginPositionRestore(54);
        await control.Open("fake.mkv");

        // Loading: the player says 0, the hold says where the video is going to be.
        Assert.Equal(0, player.Position, 3);
        Assert.Equal(54, control.PositionRestoreHoldSeconds);

        await control.RestorePositionAsync(54);

        Assert.Equal(54, player.Position, 3);
        Assert.Null(control.PositionRestoreHoldSeconds);
    }

    [AvaloniaFact]
    public async Task ThePlayheadHoldFollowsASeekMadeDuringTheRestore()
    {
        var player = new SlowToSeekVideoPlayer();
        var control = new VideoPlayerControl(player);
        control.BeginPositionRestore(54);
        await control.Open("fake.mkv");

        control.SeekTo(120);

        Assert.Equal(120, control.PositionRestoreHoldSeconds);
    }

    [AvaloniaFact]
    public async Task ARestoreThatNeverLandsLetsGoOfThePlayheadHold()
    {
        var player = new SlowToSeekVideoPlayer { SwallowEverySeek = true };
        var control = new VideoPlayerControl(player);
        control.BeginPositionRestore(54);
        await control.Open("fake.mkv");

        await control.RestorePositionAsync(54, 300);

        // The rebuild target stays (issue #14218), but a cursor held on a spot the player never
        // reaches would sit frozen through playback.
        Assert.Equal(54, control.PositionForRestore);
        Assert.Null(control.PositionRestoreHoldSeconds);
    }

    /// <summary>
    /// The control's own time text and slider follow the same hold: they used to drop to 0:00
    /// while the rebuilt player was loading and jump back when the restore seek landed.
    /// </summary>
    [AvaloniaFact]
    public async Task TheTimeTextAndSliderShowTheRestoreTargetWhileThePlayerLoads()
    {
        var player = new SlowToSeekVideoPlayer { SwallowEverySeek = true };
        var control = new VideoPlayerControl(player);
        control.BeginPositionRestore(54);
        await control.Open("fake.mkv");

        await WaitForAsync(() => control.ProgressText?.Contains("00:54") == true);

        Assert.Equal(0, player.Position, 3); // still loading
        Assert.Equal(54, control.Position, 3); // what the slider is bound to
        Assert.Contains("00:54", control.ProgressText);

        // Display only: holding it must not reach the player as a seek (a clamped slider echo
        // doing that is how a restore used to rewind the video, issue #14741).
        Assert.Equal(0, player.SeekCount);
    }

    [AvaloniaFact]
    public async Task TheTimeTextGoesBackToThePlayerWhenTheRestoreGivesUp()
    {
        var player = new SlowToSeekVideoPlayer { SwallowEverySeek = true };
        var control = new VideoPlayerControl(player);
        control.BeginPositionRestore(54);
        await control.Open("fake.mkv");
        await WaitForAsync(() => control.ProgressText?.Contains("00:54") == true);

        await control.RestorePositionAsync(54, 100);

        await WaitForAsync(() => Math.Abs(control.Position) < 0.001);
        Assert.Equal(0, control.Position, 3);
        Assert.DoesNotContain("00:54 /", control.ProgressText);
    }

    private static async Task WaitForAsync(Func<bool> condition, int timeoutMs = 3000)
    {
        var end = Environment.TickCount64 + timeoutMs;
        while (!condition() && Environment.TickCount64 < end)
        {
            await Task.Delay(25);
        }
    }

    [AvaloniaFact]
    public void ADisposedControlOffersNoPlayheadHold()
    {
        var control = new VideoPlayerControl(new SlowToSeekVideoPlayer());
        control.BeginPositionRestore(54);

        control.CloseAndDisposePlayer();

        Assert.Null(control.PositionRestoreHoldSeconds);
    }
}
