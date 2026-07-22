namespace Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;

public class OcrFixLineResult
{
    public int LineIndex { get; set; }
    public List<OcrFixLinePartResult> Words { get; set; } = new();
    public ReplacementUsedItem ReplacementUsed { get; set; } = new();

    public OcrFixLineResult()
    {
    }

    public OcrFixLineResult(int index, string text)
    {
        LineIndex = index;
        Words = new List<OcrFixLinePartResult> { new() { Word = text, IsSpellCheckedOk = null } };
    }

    public string GetText()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var w in Words)
        {
            sb.Append(string.IsNullOrEmpty(w.FixedWord) ? w.Word : w.FixedWord);
        }

        return sb.ToString();
    }
}