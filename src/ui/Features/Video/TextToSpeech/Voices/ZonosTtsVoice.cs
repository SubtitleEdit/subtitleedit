namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class ZonosTtsVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public ZonosTtsVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public ZonosTtsVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
