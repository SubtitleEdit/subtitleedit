namespace SeConv.Core;

/// <summary>
/// Creates an <see cref="IOcrEngine"/> from CLI options. Throws with a clear message when
/// the requested engine is unsupported or its prerequisites (binary on PATH, database file)
/// aren't satisfied.
/// </summary>
internal static class OcrEngineFactory
{
    public static IOcrEngine Create(ConversionOptions options)
    {
        var engine = (options.OcrEngine ?? "tesseract").Trim().ToLowerInvariant();
        return engine switch
        {
            "tesseract" or "" => TesseractOcrEngine.Create(options.OcrLanguage),
            "nocr" => new NOcrOcrEngine(ResolveNOcrDbPath(options)),
            "ollama" => new OllamaOcrEngine(options.OllamaUrl, options.OllamaModel, options.OcrLanguage),
            "paddle" or "paddleocr" => PaddleOcrEngine.Create(options.OcrLanguage),
            _ => throw new InvalidOperationException(
                $"OCR engine '{options.OcrEngine}' is not supported. Use one of: tesseract, nocr, ollama, paddle.")
        };
    }

    private static string ResolveNOcrDbPath(ConversionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.OcrDb))
        {
            throw new InvalidOperationException(
                "nOCR engine requires --ocrdb=<path-to-Latin.nocr> (or another .nocr file). " +
                "Find them in `%AppData%\\Subtitle Edit\\Ocr\\` or download from the SE UI.");
        }
        var path = options.OcrDb;
        if (!path.EndsWith(".nocr", StringComparison.OrdinalIgnoreCase))
        {
            path += ".nocr";
        }
        return path;
    }
}
