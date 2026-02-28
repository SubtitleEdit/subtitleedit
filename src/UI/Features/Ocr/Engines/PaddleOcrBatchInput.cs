using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class PaddleOcrBatchInput
{
    public int Index { get; set; }
    public SKBitmap? Bitmap { get; set; }
    public string Text { get; set; } = string.Empty;
    public string FileName { get;  set; } = string.Empty;
    public Ocr.OcrSubtitleItem? Item { get; set; }
}
