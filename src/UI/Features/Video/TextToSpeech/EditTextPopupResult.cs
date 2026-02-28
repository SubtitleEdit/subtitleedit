namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public record EditTextPopupResult
{
    public string Text { get; init; }
    public bool Regenerate { get; init; }

    public EditTextPopupResult()
    {
        Text = string.Empty;
    }
}