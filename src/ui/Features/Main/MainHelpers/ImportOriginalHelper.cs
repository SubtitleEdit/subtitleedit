using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Main.MainHelpers;

public static class ImportOriginalHelper
{
    /// <summary>
    /// The result of aligning an original subtitle with the working subtitle by time code.
    /// </summary>
    /// <param name="Projection">
    /// One paragraph per working row - the matched original line, or an empty paragraph with the
    /// working row's timings when nothing matched. This is what the original text column shows.
    /// </param>
    /// <param name="Unmatched">
    /// The original paragraphs no working row matched, in file order. These are the lines that used
    /// to be dropped silently; a read-only reference shows them as reference-only rows (#13449).
    /// </param>
    internal record OriginalMatch(Subtitle Projection, List<Paragraph> Unmatched);

    internal static Subtitle GetMatchingOriginalLines(ObservableCollection<SubtitleLineViewModel> current, Subtitle original)
    {
        return MatchOriginalLines(current, original).Projection;
    }

    internal static OriginalMatch MatchOriginalLines(ObservableCollection<SubtitleLineViewModel> current, Subtitle original)
    {
        var newOriginal = new Subtitle();

        // Tracked by index, not by paragraph identity: two original lines can carry the same text and
        // timings, and each must be accounted for separately.
        var used = new bool[original.Paragraphs.Count];

        foreach (var line in current)
        {
            var index = FindOriginalLineIndex(line, original);
            if (index >= 0)
            {
                newOriginal.Paragraphs.Add(original.Paragraphs[index]);
                used[index] = true;
            }
            else
            {
                var emptyLine = new Paragraph
                {
                    StartTime = TimeCode.FromSeconds(line.StartTime.TotalSeconds),
                    EndTime = TimeCode.FromSeconds(line.EndTime.TotalSeconds),
                    Text = string.Empty
                };
                newOriginal.Paragraphs.Add(emptyLine);
            }
        }

        var unmatched = new List<Paragraph>();
        for (var i = 0; i < original.Paragraphs.Count; i++)
        {
            if (!used[i])
            {
                unmatched.Add(original.Paragraphs[i]);
            }
        }

        return new OriginalMatch(newOriginal, unmatched);
    }

    private static int FindOriginalLineIndex(SubtitleLineViewModel line, Subtitle original)
    {
        for (var i = 0; i < original.Paragraphs.Count; i++)
        {
            var originalLine = original.Paragraphs[i];
            if (line.StartTime.TotalMilliseconds == originalLine.StartTime.TotalMilliseconds &&
                line.EndTime.TotalMilliseconds == originalLine.EndTime.TotalMilliseconds)
            {
                return i;
            }
        }

        // try with some tolerance
        for (var i = 0; i < original.Paragraphs.Count; i++)
        {
            var originalLine = original.Paragraphs[i];
            if (Math.Abs(line.StartTime.TotalMilliseconds - originalLine.StartTime.TotalMilliseconds) < 250 &&
                Math.Abs(line.EndTime.TotalMilliseconds - originalLine.EndTime.TotalMilliseconds) < 500)
            {
                return i;
            }
        }

        // try with middle time only
        var lineMiddle = (line.StartTime.TotalMilliseconds + line.EndTime.TotalMilliseconds) / 2.0;
        for (var i = 0; i < original.Paragraphs.Count; i++)
        {
            var originalLine = original.Paragraphs[i];
            if (originalLine.StartTime.TotalMilliseconds <= lineMiddle && originalLine.EndTime.TotalMilliseconds >= lineMiddle)
            {
                return i;
            }

            var originalMiddle = (originalLine.StartTime.TotalMilliseconds + originalLine.EndTime.TotalMilliseconds) / 2.0;
            if (line.StartTime.TotalMilliseconds <= originalMiddle && line.EndTime.TotalMilliseconds >= originalMiddle)
            {
                return i;
            }
        }

        return -1;
    }
}
