namespace Nikse.SubtitleEdit.Features.Ocr;

public class PaddleOcrBatchProgress
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public Ocr.OcrSubtitleItem? Item { get; set; }
}
