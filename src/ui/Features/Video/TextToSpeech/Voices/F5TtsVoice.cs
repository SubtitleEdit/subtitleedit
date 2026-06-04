namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class F5TtsVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public F5TtsVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public F5TtsVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
