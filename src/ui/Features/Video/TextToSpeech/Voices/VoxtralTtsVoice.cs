namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class VoxtralTtsVoice
{
    // Voxtral 4B TTS preset id ("casual_male", "fr_female", ...). The 20 voices are embedded in
    // the GGUF; the open checkpoint cannot clone, so this is the only voice handle.
    public string Voice { get; set; }

    public override string ToString()
    {
        return GetDisplayName(Voice);
    }

    public VoxtralTtsVoice()
    {
        Voice = string.Empty;
    }

    public VoxtralTtsVoice(string voice)
    {
        Voice = voice;
    }

    public static string GetDisplayName(string id) => id switch
    {
        "casual_female" => "English - casual female",
        "casual_male" => "English - casual male",
        "cheerful_female" => "English - cheerful female",
        "neutral_female" => "English - neutral female",
        "neutral_male" => "English - neutral male",
        "fr_female" => "French - female",
        "fr_male" => "French - male",
        "de_female" => "German - female",
        "de_male" => "German - male",
        "es_female" => "Spanish - female",
        "es_male" => "Spanish - male",
        "it_female" => "Italian - female",
        "it_male" => "Italian - male",
        "pt_female" => "Portuguese - female",
        "pt_male" => "Portuguese - male",
        "nl_female" => "Dutch - female",
        "nl_male" => "Dutch - male",
        "hi_female" => "Hindi - female",
        "hi_male" => "Hindi - male",
        "ar_male" => "Arabic - male",
        _ => id ?? string.Empty,
    };
}
