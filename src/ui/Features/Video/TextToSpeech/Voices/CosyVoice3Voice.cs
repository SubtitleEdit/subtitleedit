namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class CosyVoice3Voice
{
    public string Voice { get; set; }
    public string Preset { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public CosyVoice3Voice()
    {
        Voice = string.Empty;
        Preset = string.Empty;
    }

    public CosyVoice3Voice(string voice, string preset)
    {
        Voice = voice;
        Preset = preset;
    }
}
