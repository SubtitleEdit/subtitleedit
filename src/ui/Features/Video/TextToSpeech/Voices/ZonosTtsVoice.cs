namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class ZonosTtsVoice
{
    public string Voice { get; set; }

    /// <summary>
    /// Always empty: Zonos does not clone, and across the cloning engines a non-empty FilePath
    /// means a reference recording (see VoiceCloningConsent.IsCloneVoice).
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// The ZONOS3-Mini preset's file stem (e.g. "Imogen_en-British_F_1"), sent as the request's
    /// "voice"; empty for the unconditioned default voice.
    /// </summary>
    public string PresetStem { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public ZonosTtsVoice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
        PresetStem = string.Empty;
    }

    public ZonosTtsVoice(string voice, string presetStem)
    {
        Voice = voice;
        FilePath = string.Empty;
        PresetStem = presetStem;
    }
}
