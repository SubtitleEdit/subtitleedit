namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class AllTalkVoice  
{
    public string Voice { get; set; }

    public override string ToString()
    {
        return Voice;
    }

    public AllTalkVoice()
    {
        Voice = string.Empty;
    }

    public AllTalkVoice(string voice)
    {
        Voice = voice;
    }
}