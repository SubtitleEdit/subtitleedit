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
            "nocr" => new NOcrOcrEngine(ResolveOcrDbPath(options, "nocr", ".nocr")),
            "binaryocr" or "binary" => new BinaryOcrOcrEngine(ResolveOcrDbPath(options, "binaryocr", ".db")),
            "ollama" => new OllamaOcrEngine(options.OllamaUrl, options.OllamaModel, options.OcrLanguage),
            "paddle" or "paddleocr" => PaddleOcrEngine.Create(options.OcrLanguage),
            _ => throw new InvalidOperationException(
                $"OCR engine '{options.OcrEngine}' is not supported. Use one of: tesseract, nocr, binaryocr, ollama, paddle.")
        };
    }

    private static string ResolveOcrDbPath(ConversionOptions options, string engineName, string requiredExtension)
    {
        if (string.IsNullOrWhiteSpace(options.OcrDb))
        {
            var displayExt = requiredExtension.TrimStart('.');
            throw new InvalidOperationException(
                $"{engineName} engine requires --ocrdb=<path-to-Latin{requiredExtension}> (or another {requiredExtension} file). " +
                $"Find them in `%AppData%\\Subtitle Edit\\Ocr\\` or download from the SE UI.");
        }
        var path = options.OcrDb;
        if (!path.EndsWith(requiredExtension, StringComparison.OrdinalIgnoreCase))
        {
            path += requiredExtension;
        }
        return path;
    }
}
