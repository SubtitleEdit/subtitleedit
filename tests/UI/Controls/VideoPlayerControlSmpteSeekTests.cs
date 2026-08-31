using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System.Threading.Tasks;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// SMPTE drop-frame seeks. While SMPTE timing is enabled every position READ from the player is
/// compressed by 1000/1001 before it reaches the UI (the control's position timer, the playhead
/// estimator, the waveform time axis), so UI position values live on the drop-frame clock. A seek
/// must expand its UI value back to the player's real clock - passing it through unchanged lands
/// every seek 0.1% early, proportional to the absolute position (about a second per 17 minutes),
/// and the playhead pin then snapped the waveform cursor back once its 600 ms timeout expired.
/// </summary>
public class VideoPlayerControlSmpteSeekTests
{
    private sealed class RecordingVideoPlayer : IVideoPlayer
    {
        public string Name => "recording";
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
        public double Duration => 7200;
        public int VolumeMaximum => 100;
        public double Volume { get; set; } = 50;
        public double Speed { get; set; } = 1.0;
    }

    [AvaloniaFact]
    public void SeekTo_WithSmpteTiming_ExpandsToThePlayersRealClock()
    {
        var player = new RecordingVideoPlayer();
        var control = new VideoPlayerControl(player) { IsSmpteTimingEnabled = true };
        control.Duration = 7200; // published before seeking, or the bound slider clamps the display write

        control.SeekTo(3600);

        // The player's clock runs 1001/1000 relative to the UI's drop-frame clock; a read of the
        // landed position compresses back to exactly the requested UI value.
        Assert.Equal(3600 * 1001.0 / 1000.0, player.Position, 4);
        Assert.Equal(3600, player.Position * 1000.0 / 1001.0, 4);
        Assert.Equal(3600, control.Position, 4); // the display shows the UI value, unexpanded
    }

    [AvaloniaFact]
    public void SeekTo_WithoutSmpteTiming_PassesTheValueThrough()
    {
        var player = new RecordingVideoPlayer();
        var control = new VideoPlayerControl(player);
        control.Duration = 7200; // published before seeking, or the bound slider clamps the display write

        control.SeekTo(3600);

        Assert.Equal(3600, player.Position, 4);
        Assert.Equal(3600, control.Position, 4);
    }
}
