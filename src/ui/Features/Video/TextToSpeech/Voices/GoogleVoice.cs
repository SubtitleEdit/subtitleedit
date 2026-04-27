namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class GoogleVoice
{
    public string Name { get; set; }
    public string LanguageCode { get; set; }
    public string SsmlGender { get; set; }
    public string NaturalSampleRateHertz { get; set; }

    public GoogleVoice()
    {
        Name = string.Empty;
        LanguageCode = string.Empty;
        SsmlGender = string.Empty;
        NaturalSampleRateHertz = string.Empty;
    }

    public override string ToString()
    {
        return $"{Name} ({SsmlGender})";
    }
}