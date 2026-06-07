namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class VoxCPM2Voice
{
    public string Voice { get; set; }
    public string FilePath { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public VoxCPM2Voice()
    {
        Voice = string.Empty;
        FilePath = string.Empty;
    }

    public VoxCPM2Voice(string voice, string filePath)
    {
        Voice = voice;
        FilePath = filePath;
    }
}
