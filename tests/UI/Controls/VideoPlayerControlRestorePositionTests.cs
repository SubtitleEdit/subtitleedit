using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System.Threading.Tasks;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// Covers the rewind behind issue #14218. A rebuild of the video player (Options/OK, dock or
/// undock, fullscreen) carries the current position over to the player it builds - but a player
/// that is itself still opening reports 0, so a rebuild landing on top of another rebuild used
/// to carry 0 forward and rewind the video to the start. Settings -> Apply -> OK does exactly
/// that: Apply rebuilds the player, OK rebuilds it again a moment later.
/// </summary>
public class VideoPlayerControlRestorePositionTests
{
    private sealed class NotYetLoadedVideoPlayer : IVideoPlayer
    {
        public string Name => "not-yet-loaded";
        public string FileName { get; private set; } = string.Empty;

        public bool CanLoad() => true;

        public Task LoadFile(string fileName, double startPositionSeconds = 0)
        {
            FileName = fileName;

            // A real player reports 0 until its core is up and playback has restarted - that is
            // the whole point of this test.
            Position = 0;
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
        public double Position { get; set; }
        public double Duration => 600;
        public int VolumeMaximum => 100;
        public double Volume { get; set; } = 50;
        public double Speed { get; set; } = 1.0;
    }

    [AvaloniaFact]
    public async Task AnOpenInFlightReportsWhereItIsHeadingRatherThanZero()
    {
        var control = new VideoPlayerControl(new NotYetLoadedVideoPlayer());

        await control.Open("fake.mkv", 321.5);

        // The player has not caught up yet, so the live position is still the start of the file.
        Assert.Equal(0, control.Position);

        // A rebuild sampling now must get the position the open is heading for.
        Assert.Equal(321.5, control.PositionForRestore);
    }

    [AvaloniaFact]
    public async Task ARestoreAnnouncedBeforeAStartLessOpenSurvivesIt()
    {
        var control = new VideoPlayerControl(new NotYetLoadedVideoPlayer());

        // The docked layout rebuild opens without a start position and seeks afterwards.
        control.BeginPositionRestore(210);
        await control.Open("fake.mkv");

        Assert.Equal(210, control.PositionForRestore);
    }

    [AvaloniaFact]
    public async Task TheLivePositionRulesAgainOnceTheRestoreIsDone()
    {
        var control = new VideoPlayerControl(new NotYetLoadedVideoPlayer());

        await control.Open("fake.mkv", 321.5);
        Assert.Equal(321.5, control.PositionForRestore);

        // The restoring code is done - from here the player itself is the truth again, so a
        // pending target must not go on shadowing it for the rest of the control's life.
        control.EndPositionRestore();

        Assert.Equal(control.Position, control.PositionForRestore);
    }

    [AvaloniaFact]
    public async Task ARestoreThatNeverLandedKeepsItsTarget()
    {
        var player = new NotYetLoadedVideoPlayer();
        var control = new VideoPlayerControl(player);

        await control.Open("fake.mkv", 321.5);

        // The restoring code has run out of seeks (a bounded ready wait plus a fixed number of
        // retries) while the player is still at the start of the file. Handing the live position
        // back here is what rewound the video: the next rebuild would sample this player's 0.
        control.EndPositionRestoreIfArrived();

        Assert.Equal(321.5, control.PositionForRestore);
    }

    [AvaloniaFact]
    public async Task ARestoreThatLandedGivesTheLivePositionBack()
    {
        var player = new NotYetLoadedVideoPlayer();
        var control = new VideoPlayerControl(player);

        await control.Open("fake.mkv", 321.5);

        // The player got where it was told to go, so it is the truth again.
        player.Position = 321.5;
        control.EndPositionRestoreIfArrived();

        Assert.Equal(control.Position, control.PositionForRestore);
    }
}
