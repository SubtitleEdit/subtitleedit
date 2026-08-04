namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

/// <summary>
/// A voice for the OmniVoice (CrispASR) engine. An empty <see cref="FilePath"/> means the
/// model's own built-in voice (no reference audio); a path means zero-shot cloning from that
/// reference WAV. Kept separate from <see cref="OmniVoice"/> so the two OmniVoice engines can
/// never be handed each other's voices by the shared voice combo.
/// </summary>
public class OmniVoiceCrispAsrVoice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public OmniVoiceCrispAsrVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public OmniVoiceCrispAsrVoice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
