namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class Voice
{
    public object? EngineVoice { get; set; }
    public string Name => ToString();

    public Voice(object voice)
    {
        EngineVoice = voice;
    }

    public override string ToString()
    {
        return (EngineVoice == null ? "Unknown" : EngineVoice.ToString()) ?? "Unknown";
    }
}
