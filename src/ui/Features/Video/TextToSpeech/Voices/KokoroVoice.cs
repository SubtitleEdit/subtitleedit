namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class KokoroVoice
{
    // Kokoro voice id, e.g. "af_maple", "bf_vale", "zf_001". Maps directly to a
    // pre-trained 256-dim style vector inside voices-v1.1-zh.bin; Kokoro does
    // not support voice cloning, so this is the only voice handle.
    public string Voice { get; set; }

    public override string ToString()
    {
        return GetDisplayName(Voice);
    }

    public KokoroVoice()
    {
        Voice = string.Empty;
    }

    public KokoroVoice(string voice)
    {
        Voice = voice;
    }

    // Convert a Kokoro voice id like "af_maple" or "zm_009" into a UI-friendly
    // label such as "English (US) Female - Maple". Falls back to the raw id if
    // the format is unrecognized.
    public static string GetDisplayName(string id)
    {
        if (string.IsNullOrEmpty(id) || id.Length < 4 || id[2] != '_')
        {
            return id ?? string.Empty;
        }

        var language = id[0] switch
        {
            'a' => "English (US)",
            'b' => "English (UK)",
            'e' => "Spanish",
            'f' => "French",
            'h' => "Hindi",
            'i' => "Italian",
            'j' => "Japanese",
            'p' => "Portuguese (BR)",
            'z' => "Chinese",
            _ => null,
        };

        var gender = id[1] switch
        {
            'f' => "Female",
            'm' => "Male",
            _ => null,
        };

        if (language == null || gender == null)
        {
            return id;
        }

        var name = id.Substring(3);
        if (name.Length > 0 && char.IsLetter(name[0]))
        {
            name = char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        return $"{language} {gender} - {name}";
    }
}
