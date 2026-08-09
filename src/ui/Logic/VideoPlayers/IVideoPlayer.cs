using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers;

public interface IVideoPlayer
{
    string Name { get; }
    string FileName { get; }

    bool CanLoad();

    /// <summary>
    /// Loads a media file. Playback never starts by itself - the player comes up paused and
    /// only <see cref="Play"/> / <see cref="PlayOrPause"/> start it (issue #13329).
    /// </summary>
    /// <param name="startPositionSeconds">
    /// Where the first presented frame should be. Seeking after the file is up leaves the player
    /// showing 0:00 for a few hundred milliseconds and then visibly jumping, so callers that
    /// already know the wanted position (session restore, fullscreen, undock) pass it here.
    /// </param>
    Task LoadFile(string fileName, double startPositionSeconds = 0);
    void CloseFile();

    void Play();
    void PlayOrPause();
    void Pause();
    void Stop();
    AudioTrackInfo? ToggleAudioTrack();

    bool IsPlaying { get; }
    bool IsPaused { get; }

    double Position { get; set; }
    double Duration { get; }

    int VolumeMaximum { get; }
    double Volume { get; set; }

    double Speed { get; set; }
}
