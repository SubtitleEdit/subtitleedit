namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class ElevenLabVoice  
{
    public string Voice { get; set; }
    public string VoiceId { get; set; }
    public string Language { get; set; }
    public string Gender { get; set; }
    public string Model { get; set; }

    public override string ToString()
    {
        return $"{Language} - {Voice} ({Gender})";
    }

    public ElevenLabVoice(string language, string voice, string gender, string description, string useCase, string accent, string voiceId)
    {
        Voice = voice;
        Language = accent;
        Gender = gender;
        Model = voiceId;
        VoiceId = voiceId;
    }
}