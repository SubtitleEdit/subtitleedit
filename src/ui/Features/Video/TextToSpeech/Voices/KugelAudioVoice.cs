namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class KugelAudioVoice
{
    // KugelAudio-0-Open preset id ("default", "clear", "english_female", "english_male"). The
    // four voices' acoustic features are embedded in the GGUF; the open model cannot clone, so
    // this is the only voice handle.
    public string Voice { get; set; }

    public override string ToString()
    {
        return GetDisplayName(Voice);
    }

    public KugelAudioVoice()
    {
        Voice = string.Empty;
    }

    public KugelAudioVoice(string voice)
    {
        Voice = voice;
    }

    public static string GetDisplayName(string id) => id switch
    {
        "default" => "German female",
        "clear" => "German female (clear)",
        "english_female" => "British female",
        "english_male" => "British male",
        _ => id ?? string.Empty,
    };
}
