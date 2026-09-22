using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.Find;

/// <summary>
/// The one-line result shown under the buttons of the Find and Replace windows: what Count found,
/// and (#15165) how many occurrences Replace all / Replace &amp; find next changed.
/// </summary>
internal static class FindResultText
{
    public static string Found(int count)
    {
        if (count <= 0)
        {
            return Se.Language.General.FoundNoMatches;
        }

        return count == 1
            ? Se.Language.General.FoundOneMatch
            : string.Format(Se.Language.General.FoundXMatches, count);
    }

    public static string Replaced(int count)
    {
        if (count <= 0)
        {
            return Se.Language.General.FoundNoMatches;
        }

        return count == 1
            ? Se.Language.Edit.Find.ReplacedOneOccurrence
            : string.Format(Se.Language.Edit.Find.ReplacedXOccurrences, count);
    }

    /// <summary>A tick for a hit, an info mark for "nothing found".</summary>
    public static string Icon(int count)
    {
        return count > 0 ? IconNames.CheckCircle : IconNames.Information;
    }
}
