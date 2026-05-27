namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class VibeVoiceVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public VibeVoiceVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public VibeVoiceVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
