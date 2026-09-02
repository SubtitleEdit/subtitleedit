namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class PocketTtsVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public PocketTtsVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public PocketTtsVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
