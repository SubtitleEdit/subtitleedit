namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public sealed record IrodoriTtsVoice(string Voice, string FilePath)
{
    public override string ToString() => Voice;
}
