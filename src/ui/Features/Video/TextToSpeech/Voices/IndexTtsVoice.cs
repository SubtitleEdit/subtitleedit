namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class IndexTtsVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public IndexTtsVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public IndexTtsVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
