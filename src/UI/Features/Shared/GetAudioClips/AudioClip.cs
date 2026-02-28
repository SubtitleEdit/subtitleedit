using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Shared.GetAudioClips;

public class AudioClip
{
    public string AudioFileName { get; set; }
    public SubtitleLineViewModel Line { get; set; }
    public Subtitle Transcription { get; set; }

    public AudioClip(string audioFileName, SubtitleLineViewModel line)
    {
        AudioFileName = audioFileName;
        Line = line;
        Transcription = new Subtitle();
    }

    public AudioClip(AudioClip audioClip)
    {
        AudioFileName = audioClip.AudioFileName;
        Line = audioClip.Line;
        Transcription = audioClip.Transcription;
    }
}
