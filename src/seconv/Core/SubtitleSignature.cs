using Nikse.SubtitleEdit.Core.Common;

namespace SeConv.Core;

/// <summary>
/// A 64-bit FNV-1a signature of a subtitle's timings + text. Cheap enough to run between
/// operations (sub-millisecond for a 5000-cue file), so callers can ask "has anything changed
/// since I last looked?" without materialising a full-subtitle string.
///
/// Used to detect convergence of the Fix Common Errors passes
/// (<see cref="FixCommonErrorsRunner"/>) and to invalidate the memoized language detection in
/// <see cref="SubtitleLanguageDetector"/>. Only ever compared against another signature taken
/// in the same process, so no stability guarantee across runs is needed.
/// </summary>
internal static class SubtitleSignature
{
    public static long Compute(Subtitle subtitle)
    {
        const long fnvPrime = 1099511628211L;
        var hash = unchecked((long)14695981039346656037UL); // FNV offset basis
        unchecked
        {
            foreach (var p in subtitle.Paragraphs)
            {
                hash = (hash ^ BitConverter.DoubleToInt64Bits(p.StartTime.TotalMilliseconds)) * fnvPrime;
                hash = (hash ^ BitConverter.DoubleToInt64Bits(p.EndTime.TotalMilliseconds)) * fnvPrime;

                // Span iteration: no bounds check per character, and null text is just an empty span.
                foreach (var c in p.Text.AsSpan())
                {
                    hash = (hash ^ c) * fnvPrime;
                }

                hash = (hash ^ '\n') * fnvPrime;
            }
        }

        return hash;
    }
}
