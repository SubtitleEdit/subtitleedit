namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class VibeVoiceTtsVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public VibeVoiceTtsVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public VibeVoiceTtsVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
