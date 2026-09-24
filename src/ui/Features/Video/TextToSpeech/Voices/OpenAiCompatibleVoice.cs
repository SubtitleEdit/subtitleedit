namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class OpenAiCompatibleVoice
{
    public string Name { get; set; }
    public string VoiceId { get; set; }

    public OpenAiCompatibleVoice()
    {
        Name = string.Empty;
        VoiceId = string.Empty;
    }

    public OpenAiCompatibleVoice(string name, string voiceId)
    {
        Name = name;
        VoiceId = voiceId;
    }

    public override string ToString()
    {
        return Name;
    }
}
