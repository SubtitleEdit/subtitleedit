using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers;

public interface IVideoPlayerInstance
{
    string Name { get; }
    string FileName { get; }

    bool CanLoad();
    Task LoadFile(string fileName);
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
