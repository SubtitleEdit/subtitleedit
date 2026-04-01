namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class Qwen3TtsVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public Qwen3TtsVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public Qwen3TtsVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
