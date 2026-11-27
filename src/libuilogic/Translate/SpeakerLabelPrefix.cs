using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.UiLogic.Translate;

/// <summary>
/// The speaker label a diarizing speech-to-text engine writes in front of a line:
/// "(Speaker 1)", "[SPEAKER 2]", "[SPEAKER_00] :", "SPEAKER_00:", "Speaker 3:".
/// </summary>
public static partial class SpeakerLabelPrefix
{
    /// <summary>
    /// Bracketed (optionally followed by a colon, as WhisperX writes "[SPEAKER_00] :") or followed
    /// by a colon, at the very start of the line. Anything else is left alone: a line that merely
    /// mentions a speaker is not a label.
    /// </summary>
    [GeneratedRegex(@"^\s*(?:[\(\[]\s*(?<name>speaker[ _-]*\d+)\s*[\)\]](?:\s*:)?|(?<name>speaker[ _-]*\d+)\s*:)\s*",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex Regex();

    /// <summary>
    /// Splits <paramref name="text"/> into its leading speaker label (exactly as written,
    /// trailing whitespace included) and the rest.
    /// </summary>
    /// <returns>False when the text does not start with a speaker label.</returns>
    public static bool TrySplit(string? text, out string label, out string rest)
    {
        label = string.Empty;
        rest = text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var match = Regex().Match(text);
        if (!match.Success)
        {
            return false;
        }

        label = match.Value;
        rest = text[match.Length..];
        return true;
    }
}
