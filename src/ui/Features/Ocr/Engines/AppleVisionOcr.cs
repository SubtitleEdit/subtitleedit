using Nikse.SubtitleEdit.UiLogic.Ocr.AppleVision;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// The GUI's face of Apple Vision OCR. The Vision interop itself lives in
/// <see cref="AppleVisionRecognizer"/> (libuilogic) so seconv runs the very same recognizer;
/// this adds what only the UI needs - the engine's display name and the language list as
/// <see cref="OcrLanguage2"/> items for the combo boxes.
/// </summary>
public static class AppleVisionOcr
{
    public const string StaticName = "Apple Vision";

    private static readonly object LanguagesLock = new();
    private static List<OcrLanguage2>? _languages;

    /// <inheritdoc cref="AppleVisionRecognizer.IsAvailable"/>
    public static bool IsAvailable() => AppleVisionRecognizer.IsAvailable();

    /// <summary>
    /// The languages this machine's Vision can recognize, straight from the framework rather
    /// than a hard-coded list: the set grows with macOS releases, and the recognizer rejects a
    /// language it does not know.
    /// </summary>
    public static List<OcrLanguage2> GetLanguages()
    {
        lock (LanguagesLock)
        {
            return _languages ??= AppleVisionRecognizer.GetLanguageCodes()
                .Select(code => new OcrLanguage2(code, AppleVisionRecognizer.DisplayName(code)))
                .ToList();
        }
    }

    /// <inheritdoc cref="AppleVisionRecognizer.Ocr"/>
    public static string Ocr(SKBitmap? bitmap, string? languageCode, bool fast, CancellationToken cancellationToken) =>
        AppleVisionRecognizer.Ocr(bitmap, languageCode, fast, cancellationToken);

    /// <inheritdoc cref="AppleVisionRecognizer.OcrObservations"/>
    public static List<AppleVisionObservation> OcrObservations(SKBitmap? bitmap, string? languageCode, bool fast, CancellationToken cancellationToken) =>
        AppleVisionRecognizer.OcrObservations(bitmap, languageCode, fast, cancellationToken);
}
