using Nikse.SubtitleEdit.Features.Ocr.FixEngine;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class UnknownWordItem
{
    public OcrSubtitleItem Item { get; set; }
    public OcrFixLineResult Result { get; set; }
    public OcrFixLinePartResult Word { get; set; }

    public UnknownWordItem(OcrSubtitleItem item, OcrFixLineResult result, OcrFixLinePartResult word)
    {
        Item = item;
        Result = result;
        Word = word;
    }

    public override string ToString()
    {
        return $"#{Item.Number}: {Word.Word}";
    }
}
