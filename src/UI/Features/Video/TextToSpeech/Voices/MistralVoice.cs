namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class MistralVoice
{
    public string Name { get; set; }
    public string VoiceId { get; set; }
    public string Gender { get; set; }
    public string Language { get; set; }

    public MistralVoice()
    {
        Name = string.Empty;
        VoiceId = string.Empty;
        Gender = string.Empty;
        Language = string.Empty;
    }

    public MistralVoice(string name, string voiceId, string gender, string language)
    {
        Name = name;
        VoiceId = voiceId;
        Gender = gender;
        Language = language;
    }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Gender))
        {
            return $"{Name} ({Gender})";
        }

        return Name;
    }
}
