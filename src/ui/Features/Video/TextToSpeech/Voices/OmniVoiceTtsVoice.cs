namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class OmniVoiceTtsVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public OmniVoiceTtsVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public OmniVoiceTtsVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
