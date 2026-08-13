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

    internal static OriginalMatch MatchOriginalLines(ObservableCollection<SubtitleLineViewModel> current, Subtitle original)
    {
        var newOriginal = new Subtitle();

        // Tracked by index, not by paragraph identity: two original lines can carry the same text and
        // timings, and each must be accounted for separately. Also fed back into the search so no
        // original line is handed to a second row: the last of the three passes matches on nothing
        // more than overlapping middles, so one original line spanning two working rows - which is
        // exactly what a line-count mismatch usually is - used to be projected onto both of them,
        // showing the same source text twice while the line that really had no row became a
        // reference-only row.
        var used = new bool[original.Paragraphs.Count];

        // Rows claim lines in order, so an earlier row can still take a line by loose middle
        // overlap that a later row would have matched exactly. Matching every row at once would
        // need a global assignment; this only has to stop the same line being used twice.
        foreach (var line in current)
        {
            // A leftover reference-only row (an original is being replaced by another) displays a
            // line of the OLD original - it must not claim a line of the new one. It still emits an
            // empty projection line to keep the projection index-aligned with the rows.
            var index = line.IsReferenceOnly ? -1 : FindOriginalLineIndex(line, original, used);
            if (index >= 0)
            {
                newOriginal.Paragraphs.Add(original.Paragraphs[index]);
                used[index] = true;

                // The row remembers which original line it displays, and the assignment then
                // sticks - see SubtitleLineViewModel.ReferenceParagraphId (#13594).
                line.ReferenceParagraphId = original.Paragraphs[index].Id;
            }
            else
            {
                if (!line.IsReferenceOnly)
                {
                    line.ReferenceParagraphId = null;
                }

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

    /// <param name="used">
    /// Lines already given to an earlier working row; they are skipped so no original line lands in
    /// two rows. A row that finds only used lines gets an empty original, which is the truthful
    /// answer - it has no source line of its own.
    /// </param>
    internal static int FindOriginalLineIndex(SubtitleLineViewModel line, Subtitle original, bool[] used)
    {
        for (var i = 0; i < original.Paragraphs.Count; i++)
        {
            if (used[i])
            {
                continue;
            }

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
            if (used[i])
            {
                continue;
            }

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
            if (used[i])
            {
                continue;
            }

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
