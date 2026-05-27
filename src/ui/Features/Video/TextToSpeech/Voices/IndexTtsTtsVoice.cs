namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class IndexTtsTtsVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public IndexTtsTtsVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public IndexTtsTtsVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
