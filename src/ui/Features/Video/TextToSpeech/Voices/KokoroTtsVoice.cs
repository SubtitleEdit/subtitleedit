namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class KokoroTtsVoice
{
    // Kokoro voice id, e.g. "af_maple", "bf_vale", "zf_001". Maps directly to a
    // pre-trained 256-dim style vector inside voices-v1.1-zh.bin; Kokoro does
    // not support voice cloning, so this is the only voice handle.
    public string Voice { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public KokoroTtsVoice()
    {
        Voice = string.Empty;
    }

    public KokoroTtsVoice(string voice)
    {
        Voice = voice;
    }
}
