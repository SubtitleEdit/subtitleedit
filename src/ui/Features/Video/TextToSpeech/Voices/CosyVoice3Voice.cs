namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

/// <summary>
/// Either a baked-in CosyVoice3 preset (<see cref="Preset"/> set, <see cref="FilePath"/> empty)
/// or a user-imported reference WAV for zero-shot cloning (<see cref="FilePath"/> set,
/// <see cref="Preset"/> empty). The CosyVoice3CrispAsr engine picks which value to forward
/// to crispasr's <c>--voice</c> flag based on whether FilePath is populated.
/// </summary>
public class CosyVoice3Voice
{
    public string Voice { get; set; }
    public string Preset { get; set; }
    public string FilePath { get; set; }
    public string RefText { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public CosyVoice3Voice()
    {
        Voice = string.Empty;
        Preset = string.Empty;
        FilePath = string.Empty;
        RefText = string.Empty;
    }

    public CosyVoice3Voice(string voice, string preset)
    {
        Voice = voice;
        Preset = preset;
        FilePath = string.Empty;
        RefText = string.Empty;
    }

    public CosyVoice3Voice(string voice, string filePath, string refText)
    {
        Voice = voice;
        Preset = string.Empty;
        FilePath = filePath;
        RefText = refText;
    }
}
