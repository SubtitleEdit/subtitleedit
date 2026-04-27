namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class AzureVoice
{
    public string DisplayName { get; set; }
    public string LocalName { get; set; }
    public string ShortName { get; set; }
    public string Gender { get; set; }
    public string Locale { get; set; }

    public AzureVoice()
    {
        DisplayName = string.Empty;
        LocalName = string.Empty;
        ShortName = string.Empty;
        Gender = string.Empty;
        Locale = string.Empty;
    }
    
    public override string ToString()
    {
        return $"{Locale} - {DisplayName} ({Gender})";
    }
}