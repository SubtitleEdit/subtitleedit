using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

internal static class TtsSpokenText
{
    internal static string Clean(string? text) => Utilities.UnbreakLine(HtmlUtil.RemoveHtmlTags(text ?? string.Empty, alsoSsaTags: true));

    internal static string? NormalizeOverride(string? spokenText) =>
        string.IsNullOrWhiteSpace(spokenText) ? null : spokenText.Trim();

    internal static string Resolve(string subtitleText, string? spokenText) =>
        Clean(NormalizeOverride(spokenText) ?? subtitleText);

    /// <summary>
    /// Carries spoken-text overrides over to the new paragraphs a "merge continuation lines" pass
    /// produced. Overrides are keyed by paragraph instance and the merge builds every paragraph
    /// anew, so without this each override was silently dropped. A merged paragraph spans the
    /// time of the lines it was built from: one source keeps its override as is, several are
    /// joined, using the subtitle text for the parts that had no override.
    /// </summary>
    internal static Dictionary<Paragraph, string> RemapAfterMerge(
        IReadOnlyList<Paragraph> oldParagraphs,
        IReadOnlyDictionary<Paragraph, string> overrides,
        IEnumerable<Paragraph> mergedParagraphs)
    {
        const double toleranceMs = 1;
        var result = new Dictionary<Paragraph, string>(ReferenceEqualityComparer.Instance);
        if (overrides.Count == 0)
        {
            return result;
        }

        foreach (var merged in mergedParagraphs)
        {
            var sources = oldParagraphs
                .Where(p => p.StartTime.TotalMilliseconds >= merged.StartTime.TotalMilliseconds - toleranceMs &&
                            p.EndTime.TotalMilliseconds <= merged.EndTime.TotalMilliseconds + toleranceMs)
                .ToList();
            if (!sources.Any(overrides.ContainsKey))
            {
                continue;
            }

            var spoken = sources.Count == 1
                ? overrides[sources[0]]
                : string.Join(" ", sources.Select(p => overrides.TryGetValue(p, out var o) ? o : Clean(p.Text))
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
            if (NormalizeOverride(spoken) is { } normalized)
            {
                result[merged] = normalized;
            }
        }

        return result;
    }
}
