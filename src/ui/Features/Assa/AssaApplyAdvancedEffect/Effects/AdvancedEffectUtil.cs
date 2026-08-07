using Nikse.SubtitleEdit.Features.Main;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Shared helpers for the advanced effect generators.
/// </summary>
internal static class AdvancedEffectUtil
{
    /// <summary>
    /// Safety cap for particle generators whose event count scales with the covered time
    /// span (rain, snow, grain, ...). Keeps the 750 ms preview rebuild and the temp-file
    /// serialization bounded when the effect is applied across a long subtitle file.
    /// </summary>
    public const int MaxGeneratedEvents = 20_000;

    /// <summary>
    /// Formats a fractional override tag value with an invariant decimal point. Tag
    /// arguments must never use the current culture: a decimal comma both corrupts the
    /// number and acts as an argument separator inside \t, \fad, \move, ...
    /// </summary>
    public static string Tag(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <inheritdoc cref="Tag(double)"/>
    public static string Tag(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>
    /// Clones a line that an effect passes through unchanged, so the returned list never
    /// aliases the dialog's working set.
    /// </summary>
    public static SubtitleLineViewModel PassThrough(SubtitleLineViewModel sub) => new(sub, generateNewId: true);

    private static readonly Regex PositionTagRegex = new(@"\\(?:pos|move)\([^)]*\)|\\an\d|\\a\d+", RegexOptions.Compiled);

    /// <summary>
    /// Strips positioning override tags (\pos, \move, \an, \a) from a line's text so an
    /// effect can apply its own positioning without emitting conflicting tags.
    /// </summary>
    public static string RemovePositionTags(string text)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains('\\'))
        {
            return text;
        }

        return PositionTagRegex.Replace(text, string.Empty).Replace("{}", string.Empty);
    }
}
