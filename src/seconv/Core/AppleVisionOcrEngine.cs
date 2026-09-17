using System.Globalization;
using Nikse.SubtitleEdit.UiLogic.Ocr.AppleVision;
using SkiaSharp;

namespace SeConv.Core;

/// <summary>
/// OCR via macOS's built-in Vision framework - the same recognizer as the GUI's "Apple Vision"
/// engine (<see cref="AppleVisionRecognizer"/>). The one engine that needs nothing installed on
/// a Mac: no binary on PATH, no database file, no model download.
/// </summary>
internal sealed class AppleVisionOcrEngine : IOcrEngine
{
    public string Name => "applevision";

    /// <summary>The Vision language tag in use (e.g. <c>en-US</c>), or null for Vision's own default.</summary>
    public string? Language { get; }

    private AppleVisionOcrEngine(string? language)
    {
        Language = language;
    }

    public static AppleVisionOcrEngine Create(string? language)
    {
        if (!OperatingSystem.IsMacOS())
        {
            throw new InvalidOperationException("The applevision OCR engine uses the macOS Vision framework and is only available on macOS.");
        }

        if (!AppleVisionRecognizer.IsAvailable())
        {
            throw new InvalidOperationException("Vision.framework could not be loaded on this Mac, so the applevision OCR engine is unavailable.");
        }

        return new AppleVisionOcrEngine(ResolveLanguage(language, AppleVisionRecognizer.GetLanguageCodes()));
    }

    public string Recognize(SKBitmap bitmap)
    {
        return AppleVisionRecognizer.Ocr(bitmap, Language, fast: false, CancellationToken.None);
    }

    public void Dispose()
    {
    }

    /// <summary>
    /// Maps <c>--ocr-language</c> onto one of Vision's own tags. Vision silently returns nothing
    /// for a language it does not know, which would turn a typo into a run of empty subtitles, so
    /// anything that does not resolve fails up front with the list of what does.
    /// <para>
    /// Besides the exact tag (<c>de-DE</c>), the codes and names other engines take are accepted:
    /// <c>de</c> (Paddle), <c>deu</c> (Tesseract) and <c>German</c> (Ollama/llama.cpp), so a script
    /// can switch engine without switching language codes.
    /// </para>
    /// </summary>
    /// <param name="requested">The <c>--ocr-language</c> value; null/empty picks <c>en-US</c>,
    /// the GUI's default.</param>
    /// <param name="supported">The tags this Mac's Vision reports. Empty when the list could not
    /// be read, in which case the value is passed through unchecked.</param>
    internal static string? ResolveLanguage(string? requested, IReadOnlyList<string> supported)
    {
        if (string.IsNullOrWhiteSpace(requested))
        {
            return supported.FirstOrDefault(p => p.Equals("en-US", StringComparison.OrdinalIgnoreCase)) ?? supported.FirstOrDefault();
        }

        var value = requested.Trim();
        if (supported.Count == 0)
        {
            return value;
        }

        var exact = supported.FirstOrDefault(p => p.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (exact != null)
        {
            return exact;
        }

        var matches = supported.Where(p => IsSameLanguage(p, value)).ToList();
        if (matches.Count == 1)
        {
            return matches[0];
        }

        if (matches.Count > 1)
        {
            throw new InvalidOperationException(
                $"--ocr-language:{value} matches several Apple Vision languages ({string.Join(", ", matches)}). Pass one of them.");
        }

        throw new InvalidOperationException(
            $"Apple Vision cannot recognize --ocr-language:{value} on this Mac. Use one of: {string.Join(", ", supported)}.");
    }

    private static bool IsSameLanguage(string tag, string requested)
    {
        var dash = tag.IndexOf('-');
        if (dash > 0 && tag[..dash].Equals(requested, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        try
        {
            var culture = CultureInfo.GetCultureInfo(tag);
            if (culture.ThreeLetterISOLanguageName.Equals(requested, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var neutral = CultureInfo.GetCultureInfo(culture.TwoLetterISOLanguageName);
            return neutral.EnglishName.Equals(requested, StringComparison.OrdinalIgnoreCase);
        }
        catch (CultureNotFoundException)
        {
            // No culture data for this tag (or invariant globalization): the prefix check above
            // is all there is to go on.
            return false;
        }
    }
}
