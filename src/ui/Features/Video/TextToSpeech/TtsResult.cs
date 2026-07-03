namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public record TtsResult
{
    public string FileName { get; init; }
    public string Text { get; init; }
    public bool Error { get; init; }

    // Human-readable reason when Error is true (e.g. the HTTP status + response body of a failed
    // API call), so the UI can tell the user *why* segments failed instead of a bare count (#12093).
    public string ErrorMessage { get; init; } = string.Empty;

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
