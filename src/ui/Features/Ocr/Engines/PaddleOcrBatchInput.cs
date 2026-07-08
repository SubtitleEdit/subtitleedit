using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class PaddleOcrBatchInput
{
    public int Index { get; set; }
    public SKBitmap? Bitmap { get; set; }
    public string Text { get; set; } = string.Empty;
    public string FileName { get;  set; } = string.Empty;
    public Ocr.OcrSubtitleItem? Item { get; set; }

    /// <summary>
    /// Optional image file to OCR as-is. When set (and Bitmap is null) the file is copied
    /// into the batch folder instead of encoding a bitmap - no decode/encode roundtrip.
    /// </summary>
    public string SourceFileName { get; set; } = string.Empty;
}
