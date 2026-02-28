namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public record TtsResult
{
    public string FileName { get; init; }
    public string Text { get; init; }
    public bool Error { get; init; }

    public TtsResult()
    {
        FileName = string.Empty;
        Text = string.Empty;
    }

    public TtsResult(string fileName, string text)
    {
        FileName = fileName;
        Text = text;
    }
}
