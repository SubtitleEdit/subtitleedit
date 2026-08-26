using Nikse.SubtitleEdit.Features.Ocr.Engines;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeVideoOcr
{
    public string Engine { get; set; }
    public string PaddleLanguage { get; set; }

    /// <summary>Apple Vision's recognition language, as its BCP-47 tag. macOS only.</summary>
    public string AppleVisionLanguage { get; set; }
    public string OllamaUrl { get; set; }
    public string OllamaModel { get; set; }
    public string OllamaLanguage { get; set; }
    public string GlmUrl { get; set; }
    public string GlmModel { get; set; }
    public string GlmApiKey { get; set; }
    public string GlmLanguage { get; set; }
    public string LlamaCppModel { get; set; }
    public string LlamaCppLanguage { get; set; }
    public string CrispEmbedBackend { get; set; }
    public string CrispEmbedModel { get; set; }
    public int FramesPerSecond { get; set; }
    public int ImageSimilarityPercent { get; set; }
    public int TextSimilarityPercent { get; set; }
    public int MaxGapMs { get; set; }
    public int MinDurationMs { get; set; }
    public int BrightnessMinimum { get; set; }
    public int MaxImageWidth { get; set; }
    public bool AddAssaPositionTag { get; set; }
    public double CropXPercent { get; set; }
    public double CropYPercent { get; set; }
    public double CropWidthPercent { get; set; }
    public double CropHeightPercent { get; set; }

    public SeVideoOcr()
    {
        // Unlike the other two, this setting stores the enum name rather than the display name.
        // macOS had been defaulting to the Python PaddleOCR, which needs a pip install before it
        // does anything; Apple Vision is there already.
        Engine = System.OperatingSystem.IsMacOS()
            ? OcrEngineType.AppleVision.ToString()
            : OcrEngineType.PaddleOcrStandalone.ToString();
        PaddleLanguage = "en";
        AppleVisionLanguage = "en-US";
        OllamaUrl = "http://localhost:11434/api/chat";
        OllamaModel = "glm-ocr";
        OllamaLanguage = "English";
        GlmUrl = GlmOcr.DefaultUrl;
        GlmModel = GlmOcr.DefaultModel;
        GlmApiKey = string.Empty;
        GlmLanguage = "English";
        LlamaCppModel = string.Empty;
        LlamaCppLanguage = "English";

        // Same defaults as the OCR window (SeOcr), but tracked per feature: the best backend for
        // burned-in video text is not necessarily the best one for subtitle bitmaps.
        CrispEmbedBackend = "GLM-OCR";
        CrispEmbedModel = "glm-ocr-q8_0.gguf";
        FramesPerSecond = 5;
        ImageSimilarityPercent = 92;
        TextSimilarityPercent = 80;
        MaxGapMs = 250;
        MinDurationMs = 250;
        BrightnessMinimum = 190;
        MaxImageWidth = 720;
        AddAssaPositionTag = false;

        // Default scan area: bottom third, full width.
        CropXPercent = 0;
        CropYPercent = 200.0 / 3.0;
        CropWidthPercent = 100;
        CropHeightPercent = 100.0 / 3.0;
    }
}
