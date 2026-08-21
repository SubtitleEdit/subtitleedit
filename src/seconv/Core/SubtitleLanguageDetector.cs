using Nikse.SubtitleEdit.Core.Common;

namespace SeConv.Core;

/// <summary>
/// Memoized wrapper around <see cref="LanguageAutoDetect.AutoDetectGoogleLanguageOrNull"/>.
///
/// Detection is expensive — it deep-copies the subtitle, concatenates up to 500 KB of text and
/// scores it against every known language (~60 ms for a 5000-cue file, and it scales with the
/// file). Several conversion steps need the language: Fix Common Errors, Remove text for HI,
/// Balance lines, Redo casing and Convert colors to dialog each used to run their own detection,
/// so a run combining them paid that cost once per operation.
///
/// A hit requires both the same subtitle instance and an unchanged
/// <see cref="SubtitleSignature"/> of its timings and text, so an operation that rewrites the
/// text (Remove text for HI, say) invalidates the cache and the next caller re-detects. The
/// answer is therefore always the one an unmemoized call would have given, in exchange for a
/// sub-millisecond check.
/// </summary>
internal static class SubtitleLanguageDetector
{
    private sealed record Entry(Subtitle Subtitle, long Signature, string? Language);

    // One field so a reader always sees a consistent triple: publishing the entry is a single
    // reference assignment, which is atomic. Worst case under concurrency is a redundant
    // detection, never a mismatched subtitle/language pair.
    private static Entry? _cached;

    /// <summary>
    /// The detected two-letter language code, or null when nothing matched. Same result as
    /// <see cref="LanguageAutoDetect.AutoDetectGoogleLanguageOrNull"/>.
    /// </summary>
    public static string? DetectOrNull(Subtitle subtitle)
    {
        if (subtitle == null)
        {
            return null;
        }

        var signature = SubtitleSignature.Compute(subtitle);
        var cached = _cached;
        if (cached is not null && ReferenceEquals(cached.Subtitle, subtitle) && cached.Signature == signature)
        {
            return cached.Language;
        }

        var language = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle);
        _cached = new Entry(subtitle, signature, language);
        return language;
    }

    /// <summary>
    /// The detected two-letter language code, falling back to <c>en</c>. Same result as
    /// <see cref="LanguageAutoDetect.AutoDetectGoogleLanguage(Subtitle)"/>.
    /// </summary>
    public static string Detect(Subtitle subtitle) => DetectOrNull(subtitle) ?? "en";
}
