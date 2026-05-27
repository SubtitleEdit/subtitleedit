namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class VibeVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public VibeVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public VibeVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
