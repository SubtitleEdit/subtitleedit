using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

namespace Nikse.SubtitleEdit.Logic.Ocr;

/// <summary>
/// Holds the image source (Blu-ray sup, VobSub, BDN, transport stream, Matroska, ...)
/// from the most recent OCR session so other features (e.g. spell check) can show the
/// original images for the OCR'd lines without re-parsing the source file. (#11719)
/// </summary>
public interface IOcrImageSourceHolder
{
    IOcrSubtitle? Source { get; set; }
    string? FileName { get; set; }
}

public class OcrImageSourceHolder : IOcrImageSourceHolder
{
    public IOcrSubtitle? Source { get; set; }
    public string? FileName { get; set; }
}
