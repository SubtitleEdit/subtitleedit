using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

internal static class TranslationLineBreaks
{
    private static readonly char[] ProtectedCharacters = ['<', '>', '{', '[', '♪', '♫', '\u00a0'];
    private static readonly Regex SpeakerMarker = new(@"(?:^|(?<= ))[-–—−](?= )");
    private static readonly Regex SpeakerBoundary = new(@"(?<=[.!?…]) (?=[-–—−] )");
    private static readonly Regex StyleWrapper = new(@"\A(?:[ \t]*</?[ibu]>)*[ \t]*\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    internal static string Restore(StrippedLine source, string translation, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(source.Prefix) && string.IsNullOrWhiteSpace(source.Suffix))
        {
            return source.Restore(Repair(source.Text, translation, maximumLength));
        }

        var restored = source.Restore(translation);
        if (!source.HasWrappers || !StyleWrapper.IsMatch(source.Prefix) || !StyleWrapper.IsMatch(source.Suffix))
        {
            return restored;
        }

        // Verify that stripping the restored cue still finds the same whole-cue tags.
        var target = StrippedLine.StripForTranslation(restored);
        if (target.Prefix != source.Prefix || target.Suffix != source.Suffix)
        {
            return restored;
        }

        return target.Prefix + Repair(source.Text, target.Text, maximumLength) + target.Suffix;
    }

    internal static string Repair(string source, string translation, int maximumLength)
    {
        if (source.CountLines() == translation.CountLines() ||
            source.IndexOfAny(ProtectedCharacters) >= 0 ||
            translation.IndexOfAny(ProtectedCharacters) >= 0)
        {
            return translation;
        }

        var sourceLines = source.SplitToLines();
        var translatedLines = translation.SplitToLines();
        var flattened = JoinLines(translatedLines);
        string? repaired = null;
        var sourceHasSpeakers = sourceLines.Any(StartsWithSpeakerMarker);
        if (sourceHasSpeakers)
        {
            if (sourceLines.All(HasExplicitSpeakerMarker) &&
                translatedLines.Count == 1 && HasExplicitSpeakerMarker(translation) &&
                SpeakerMarker.Matches(translation).Count == sourceLines.Count &&
                SpeakerBoundary.Matches(translation).Count == sourceLines.Count - 1)
            {
                repaired = SpeakerBoundary.Replace(translation, Environment.NewLine);
            }
        }
        else if (!translatedLines.Any(StartsWithSpeakerMarker))
        {
            if (sourceLines.Count == 1)
            {
                repaired = flattened;
            }
            else if (sourceLines.Count == 2 && translatedLines.Count == 1)
            {
                repaired = Utilities.AutoBreakLinePrivate(flattened, maximumLength, 0, string.Empty, true);
            }
        }

        if (repaired == null)
        {
            return translation;
        }

        var repairedLines = repaired.SplitToLines();
        // AutoBreakLine may normalize spaces, so accept only substitutions between spaces and line breaks.
        return repairedLines.Count == sourceLines.Count &&
            repairedLines.All(line => StartsWithSpeakerMarker(line) == sourceHasSpeakers) &&
            repairedLines.All(line => !string.IsNullOrWhiteSpace(line) && line.Length <= maximumLength) &&
            string.Join(" ", repairedLines) == flattened
            ? repaired
            : translation;
    }

    /// <summary>
    /// Joins lines with a space, except where a line break sits next to a script written without
    /// spaces between words (Chinese, Japanese, Thai...) - a space there would be a new character
    /// in the translation, not a restored layout.
    /// </summary>
    private static string JoinLines(List<string> lines)
    {
        var sb = new StringBuilder(lines[0]);
        for (var i = 1; i < lines.Count; i++)
        {
            var previous = sb.Length > 0 ? sb[sb.Length - 1] : ' ';
            var next = lines[i].Length > 0 ? lines[i][0] : ' ';
            if (!IsUnspacedScript(previous) && !IsUnspacedScript(next))
            {
                sb.Append(' ');
            }

            sb.Append(lines[i]);
        }

        return sb.ToString();
    }

    private static bool IsUnspacedScript(char c)
    {
        return c is >= '฀' and <= '໿' // Thai, Lao
            or >= 'က' and <= '႟' // Myanmar
            or >= 'ក' and <= '៿' // Khmer
            or >= '⺀' and <= '⿟' // CJK radicals
            or >= '　' and <= 'ヿ' // CJK punctuation, Hiragana, Katakana
            or >= '㄀' and <= 'ㇿ' // Bopomofo, Katakana extensions
            or >= '㐀' and <= '䶿' // CJK extension A
            or >= '一' and <= '鿿' // CJK unified ideographs
            or >= '豈' and <= '﫿' // CJK compatibility ideographs
            or >= '＀' and <= '￯'; // full-width and half-width forms
    }

    private static bool StartsWithSpeakerMarker(string line)
    {
        var trimmed = line.AsSpan().TrimStart();
        return trimmed.Length > 0 && trimmed[0] is '-' or '–' or '—' or '−';
    }

    private static bool HasExplicitSpeakerMarker(string line)
    {
        return line.Length > 2 && line[1] == ' ' && StartsWithSpeakerMarker(line);
    }
}
