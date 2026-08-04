namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// A downloadable GGUF model (one quantization) for a CrispEmbed OCR backend.
/// Detector-based backends (PP-OCRv6) need a second GGUF - the text detector - which is
/// downloaded alongside the recognizer and passed to the CLI as --ocr-det.
/// </summary>
public class CrispEmbedModel
{
    public string Name { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    /// <summary>File name of the companion text-detection GGUF, or empty for single-model backends.</summary>
    public string DetectorName { get; set; } = string.Empty;

    /// <summary>Download URL of the companion text-detection GGUF, or empty for single-model backends.</summary>
    public string DetectorUrl { get; set; } = string.Empty;

    /// <summary>
    /// Smallest plausible size of the downloaded recognizer, used to spot a truncated download.
    /// The VLM backends are hundreds of MB; the PP-OCRv6 pair is only a few MB, so the floor
    /// cannot be a single constant.
    /// </summary>
    public long MinimumSizeBytes { get; set; } = 10_000_000;

    /// <summary>Same truncation floor as <see cref="MinimumSizeBytes"/>, for the detector GGUF.</summary>
    public long MinimumDetectorSizeBytes { get; set; } = 10_000_000;

    public bool HasDetector => !string.IsNullOrEmpty(DetectorName);

    public override string ToString()
    {
        return Name;
    }
}
