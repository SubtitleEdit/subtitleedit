namespace Nikse.SubtitleEdit.Features.Ocr;

public class SkipOnceChar
{
    public int LineIndex { get; set; }
    public int LetterIndex { get; set; }
    public string Text { get; set; }

    public SkipOnceChar(int lineIndex, int letterIndex)
    {
        LineIndex = lineIndex;
        LetterIndex = letterIndex;
        Text = string.Empty;
    }

    public SkipOnceChar(int lineIndex, int letterIndex, string text)
    {
        LineIndex = lineIndex;
        LetterIndex = letterIndex;
        Text = text;
    }
}
