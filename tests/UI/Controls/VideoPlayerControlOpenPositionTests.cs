using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System.Threading.Tasks;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// Covers the "video plays for a moment and then jumps" behaviour behind issue #13329.
/// Restoring a session used to open the video at 0:00 and seek only once the player reported
/// ready, which showed the start of the file for a few hundred milliseconds before jumping to
/// where the user actually was. The wanted position now travels with the open request, so the
/// very first frame the player presents is the right one.
/// </summary>
public class VideoPlayerControlOpenPositionTests
{
    private sealed class RecordingVideoPlayer : IVideoPlayer
    {
        public double LastStartPositionSeconds { get; private set; } = -1;
        public int LoadFileCount { get; private set; }
        public int PauseCount { get; private set; }

        public string Name => "recording";
        public string FileName { get; private set; } = string.Empty;

        public bool CanLoad() => true;

        public Task LoadFile(string fileName, double startPositionSeconds = 0)
        {
            FileName = fileName;
            LastStartPositionSeconds = startPositionSeconds;
            LoadFileCount++;
            return Task.CompletedTask;
        }

        public void CloseFile() => FileName = string.Empty;

        public void Play()
        {
        }

        public void PlayOrPause()
        {
        }

        public void Pause() => PauseCount++;

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
    public async Task OpenForwardsTheStartPositionToThePlayer()
    {
        var player = new RecordingVideoPlayer();
        var control = new VideoPlayerControl(player);

        await control.Open("fake.mkv", 123.5);

        Assert.Equal(123.5, player.LastStartPositionSeconds);
    }

    [AvaloniaFact]
    public async Task OpenWithoutAStartPositionLoadsFromTheBeginning()
    {
        var player = new RecordingVideoPlayer();
        var control = new VideoPlayerControl(player);

        await control.Open("fake.mkv");

        Assert.Equal(0, player.LastStartPositionSeconds);
    }

    [AvaloniaFact]
    public async Task OpenPausesThePlayer()
    {
        var player = new RecordingVideoPlayer();
        var control = new VideoPlayerControl(player);

        await control.Open("fake.mkv");

        // Opening a video is an editing action - it must never leave playback running.
        Assert.True(player.PauseCount > 0);
    }
}
