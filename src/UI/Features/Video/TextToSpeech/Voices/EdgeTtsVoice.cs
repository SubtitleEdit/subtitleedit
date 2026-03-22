namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class EdgeTtsVoice
{
    public string Name { get; set; }
    public string Gender { get; set; }

    public EdgeTtsVoice(string name, string gender)
    {
        Name = name;
        Gender = gender;
    }

    public override string ToString()
    {
        return $"{Name} ({Gender})";
    }
}
