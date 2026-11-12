using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// D-Cinema SMPTE EditRate: a rational written as two space-separated positive integers ("24 1").
    /// </summary>
    public static class DCinemaEditRate
    {
        /// <summary>
        /// Composition edit rates for a DCP: 24 and 48 (SMPTE ST 429-2) plus the additional
        /// frame rates 25, 30, 50 and 60 (SMPTE ST 429-13 / ST 428-11).
        /// </summary>
        public static readonly string[] StandardValues = { "24 1", "48 1", "25 1", "30 1", "50 1", "60 1" };

        /// <summary>
        /// Time code rates matching <see cref="StandardValues"/>.
        /// </summary>
        public static readonly string[] StandardTimeCodeRates = { "24", "25", "30", "48", "50", "60" };

        /// <summary>
        /// True for one of the standard D-Cinema composition edit rates.
        /// </summary>
        public static bool IsStandard(string editRate)
        {
            return TryParse(editRate, out var numerator, out var denominator) &&
                   Array.IndexOf(StandardValues, numerator.ToString(CultureInfo.InvariantCulture) + " " + denominator.ToString(CultureInfo.InvariantCulture)) >= 0;
        }

        /// <summary>
        /// Parses "numerator denominator" where both are positive integers.
        /// </summary>
        public static bool TryParse(string editRate, out int numerator, out int denominator)
        {
            numerator = 0;
            denominator = 0;
            var parts = (editRate ?? string.Empty).Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 2 &&
                   int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out numerator) &&
                   int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out denominator) &&
                   numerator > 0 && denominator > 0;
        }

        /// <summary>
        /// The edit rate to write: the given value normalized to "n d" when well-formed,
        /// otherwise "24 1" for a 24 fps project and "25 1" for anything else.
        /// </summary>
        public static string ToValidOrDefault(string editRate, double currentFrameRate)
        {
            if (TryParse(editRate, out var numerator, out var denominator))
            {
                return numerator.ToString(CultureInfo.InvariantCulture) + " " + denominator.ToString(CultureInfo.InvariantCulture);
            }

            return Math.Abs(currentFrameRate - 24) < 0.01 ? "24 1" : "25 1";
        }
    }
}
