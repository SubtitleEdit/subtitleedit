namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class SupertonicVoice
{
    // Supertonic-3 preset id: "F1".."F5" or "M1".."M5". The ten style vectors are baked into the
    // GGUF; Supertonic does not support voice cloning, so this is the only voice handle.
    public string Voice { get; set; }

    public override string ToString()
    {
        return GetDisplayName(Voice);
    }

    public SupertonicVoice()
    {
        Voice = string.Empty;
    }

    public SupertonicVoice(string voice)
    {
        Voice = voice;
    }

    // "F2" -> "Female 2", "M5" -> "Male 5". Falls back to the raw id if the format is unrecognized.
    public static string GetDisplayName(string id)
    {
        if (string.IsNullOrEmpty(id) || id.Length < 2)
        {
            return id ?? string.Empty;
        }

        var gender = char.ToUpperInvariant(id[0]) switch
        {
            'F' => "Female",
            'M' => "Male",
            _ => null,
        };

        return gender == null ? id : $"{gender} {id.Substring(1)}";
    }
}
