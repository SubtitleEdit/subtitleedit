namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class DotsTtsVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public DotsTtsVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public DotsTtsVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
