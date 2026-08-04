namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class MossTtsVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public MossTtsVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public MossTtsVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
