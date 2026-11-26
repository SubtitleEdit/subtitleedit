using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Common;

namespace Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

internal static class TranslationLineBreaks
{
    private static readonly char[] ProtectedCharacters = ['<', '>', '{', '[', '♪', '♫', '\u00a0'];
    private static readonly Regex SpeakerMarker = new(@"(?:^|(?<= ))[-–—−](?= )");
    private static readonly Regex SpeakerBoundary = new(@"(?<=[.!?…]) (?=[-–—−] )");
    private static readonly Regex StyleWrapper = new(@"\A(?:[ \t]*</?[ibu]>)*[ \t]*\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    internal static string Restore(StrippedLine source, string translation, int maximumLength, string language = "")
    {
        if (string.IsNullOrWhiteSpace(source.Prefix) && string.IsNullOrWhiteSpace(source.Suffix))
        {
            return source.Restore(Repair(source.Text, translation, maximumLength, language));
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

        return target.Prefix + Repair(source.Text, target.Text, maximumLength, language) + target.Suffix;
    }

    internal static string Repair(string source, string translation, int maximumLength, string language = "")
    {
        if (source.CountLines() == translation.CountLines() ||
            source.IndexOfAny(ProtectedCharacters) >= 0 ||
            translation.IndexOfAny(ProtectedCharacters) >= 0)
        {
            return translation;
        }

        var sourceLines = source.SplitToLines();
        var translatedLines = translation.SplitToLines();
        var flattened = UnspacedScript.JoinLines(translatedLines);
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
                repaired = Utilities.AutoBreakLinePrivate(flattened, maximumLength, 0, language, true);
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
            UnspacedScript.JoinLines(repairedLines) == flattened
            ? repaired
            : translation;
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
