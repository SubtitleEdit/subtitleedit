using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

/// <summary>
/// Marks the suggestions that belong to the same sentence unit (see <see cref="AiReviewChunker"/>).
/// Such suggestions are checked and applied together - the model rebalances the wording across the
/// line break, so applying one half of a sentence and not the other leaves duplicated or missing
/// words. Checking one row therefore also checks its siblings, which looks like a selection bug
/// unless the rows say why (issue #13775): every suggestion in a multi-line unit gets a
/// <see cref="ReviewSuggestionItem.LinkedLinesText"/> the grid shows as a link icon and tooltip.
/// </summary>
public static class ReviewUnitLinks
{
    public static void Update(IReadOnlyList<ReviewSuggestionItem> suggestions)
    {
        var l = Se.Language.Tools.AiReview;
        foreach (var unit in suggestions.GroupBy(s => s.UnitId))
        {
            var numbers = unit.Select(s => s.Number).OrderBy(n => n).ToList();
            var text = numbers.Count > 1
                ? string.Format(l.LinkedLinesHint, string.Format(l.LinesXToY, numbers[0], numbers[^1]))
                : string.Empty;

            foreach (var suggestion in unit)
            {
                suggestion.LinkedLinesText = text;
            }
        }
    }
}
