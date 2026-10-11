using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System.Threading.Tasks;

namespace UITests.Features.Sync.PointSync;

/// <summary>
/// A player that loaded - anything but the EmptyVideoPlayer fallback, which "Set sync point" treats
/// as no video (issue #15787). Lets window tests run the with-video layout without libmpv/libVLC.
/// </summary>
internal sealed class WorkingVideoPlayer : IVideoPlayer
{
    public static VideoPlayerControl MakeControl() => new(new WorkingVideoPlayer());

    public string Name => "working";
    public string FileName { get; private set; } = string.Empty;
    public bool CanLoad() => true;
    public Task LoadFile(string fileName, double startPositionSeconds = 0)
    {
        FileName = fileName;
        Position = startPositionSeconds;
        return Task.CompletedTask;
    }
    public void CloseFile() => FileName = string.Empty;
    public void Play() { }
    public void PlayOrPause() { }
    public void Pause() { }
    public void Stop() { }
    public AudioTrackInfo? ToggleAudioTrack() => null;
    public bool IsPlaying => false;
    public bool IsPaused => true;
    public double Position { get; set; }
    public double Duration => 3600;
    public int VolumeMaximum => 100;
    public double Volume { get; set; }
    public double Speed { get; set; } = 1;
}
