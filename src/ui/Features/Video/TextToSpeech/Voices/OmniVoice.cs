namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class OmniVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public OmniVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public OmniVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
