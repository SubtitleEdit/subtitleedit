namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class Confucius4TtsVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public Confucius4TtsVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public Confucius4TtsVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
