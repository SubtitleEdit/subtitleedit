namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class MurfVoice
{
    public string DisplayName { get; set; }
    public string VoiceId { get; set; }
    public string Locale { get; set; }
    public string Description { get; set; }
    public string Gender { get; set; }
    public string[] AvailableStyles { get; set; }

    public override string ToString()
    {
        return $"{DisplayName} - {Description}";
    }

    public MurfVoice(string displayName, string voiceId, string locale, string description, string gender, string[] styles)
    {
        DisplayName = displayName;
        VoiceId = voiceId;
        Locale = locale;
        Description = description;
        Gender = gender;
        AvailableStyles = styles;
    }
}