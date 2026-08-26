namespace Nikse.SubtitleEdit.Features.Ocr;

public class PaddleOcrBatchProgress
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;

    /// <summary>Average recognition confidence (0-1) of the text regions kept in Text; 1 when unknown.</summary>
    public double Confidence { get; set; } = 1.0;

    public Ocr.OcrSubtitleItem? Item { get; set; }
}
