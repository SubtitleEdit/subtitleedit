using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Controls.VideoPlayer;

public class VideoPlayerInstanceNone : IVideoPlayerInstance
{
    public string Name => string.Empty;

    private string _fileName = string.Empty;
    public string FileName => _fileName;

    public bool IsPlaying => false;

    public bool IsPaused => true;

    public double Position
    {
        get => 0;
        set { }
    }

    public double Duration => 0;

    public int VolumeMaximum => LibMpvDynamicPlayer.MaxVolume;

    public double Volume
    {
        get => 0;
        set
        {
        }
    }

    public double Speed 
    {
        get => 0;
        set
        {
        }
    }

    public bool CanLoad()
    {
        return true;
    }

    public void CloseFile()
    {
        _fileName = string.Empty;
    }

    public Task LoadFile(string fileName)
    {
        _fileName = fileName;
        return Task.CompletedTask;
    }

    public void Pause()
    {
    }

    public void Play()
    {
    }

    public void PlayOrPause()
    {
    }

    public void Stop()
    {
    }

    public AudioTrackInfo? ToggleAudioTrack()
    {
        return null;
    }
}
