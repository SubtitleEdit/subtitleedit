namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// A downloadable GGUF model (one quantization) for a CrispEmbed OCR backend.
/// </summary>
public class CrispEmbedModel
{
    public string Name { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    public override string ToString()
    {
        return Name;
    }
}
