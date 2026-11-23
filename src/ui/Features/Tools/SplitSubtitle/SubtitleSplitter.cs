using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;

/// <summary>
/// Pure split logic for the Split Subtitle tool. Kept side-effect free (no UI, no settings) so it
/// can be unit tested. The number of returned parts always equals <paramref name="numberOfParts"/>
/// (clamped to at least 1 and at most the paragraph count) - this is what guarantees the dialog
/// preview and the files written to disk can never disagree.
/// </summary>
public static class SubtitleSplitter
{
    public enum SplitMode
    {
        Lines,
        Characters,
        Time,
    }

    public static List<Subtitle> Split(Subtitle subtitle, int numberOfParts, SplitMode mode)
    {
        var parts = new List<Subtitle>();
        if (subtitle == null || subtitle.Paragraphs.Count == 0)
        {
            return parts;
        }

        if (numberOfParts < 1)
        {
            numberOfParts = 1;
        }

        if (numberOfParts > subtitle.Paragraphs.Count)
        {
            numberOfParts = subtitle.Paragraphs.Count;
        }

        return mode switch
        {
            SplitMode.Characters => SplitByCharacters(subtitle, numberOfParts),
            SplitMode.Time => SplitByTime(subtitle, numberOfParts),
            _ => SplitByLines(subtitle, numberOfParts),
        };
    }

    private static List<Subtitle> SplitByLines(Subtitle subtitle, int numberOfParts)
    {
        var parts = new List<Subtitle>();
        var partSize = subtitle.Paragraphs.Count / numberOfParts;
        var startNumber = 0;
        for (var i = 0; i < numberOfParts; i++)
        {
            var noOfLines = partSize;
            if (i == numberOfParts - 1)
            {
                noOfLines = subtitle.Paragraphs.Count - (numberOfParts - 1) * partSize;
            }

            var temp = new Subtitle { Header = subtitle.Header };
            for (var number = 0; number < noOfLines; number++)
            {
                temp.Paragraphs.Add(new Paragraph(subtitle.Paragraphs[startNumber + number]));
            }

            startNumber += noOfLines;
            parts.Add(temp);
        }

        return parts;
    }

    private static List<Subtitle> SplitByCharacters(Subtitle subtitle, int numberOfParts)
    {
        var parts = new List<Subtitle>();
        var totalNumberOfCharacters = 0;
        foreach (var p in subtitle.Paragraphs)
        {
            totalNumberOfCharacters += HtmlUtil.RemoveHtmlTags(p.Text, true).Length;
        }

        var partSize = totalNumberOfCharacters / numberOfParts;
        var nextLimit = partSize;
        var currentSize = 0;
        var temp = new Subtitle { Header = subtitle.Header };
        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var p = subtitle.Paragraphs[i];
            var size = HtmlUtil.RemoveHtmlTags(p.Text, true).Length;
            // Never close an empty part: when the very first paragraph already exceeds the
            // limit, pushing the untouched accumulator produced a 0-byte "Part1" file with all
            // the content in Part2.
            if (temp.Paragraphs.Count > 0 && parts.Count < numberOfParts - 1 &&
                (currentSize + size > nextLimit + 4 || MustStartNewPart(subtitle, i, parts.Count, numberOfParts)))
            {
                parts.Add(temp);
                currentSize = size;
                temp = new Subtitle { Header = subtitle.Header };
                temp.Paragraphs.Add(new Paragraph(p));
            }
            else
            {
                currentSize += size;
                temp.Paragraphs.Add(new Paragraph(p));
            }
        }

        // The trailing part must be added too, otherwise the dialog shows N parts but only N-1
        // files are written (the reported "6 parts shown, 5 files generated" bug).
        parts.Add(temp);

        return parts;
    }

    private static List<Subtitle> SplitByTime(Subtitle subtitle, int numberOfParts)
    {
        var parts = new List<Subtitle>();
        var startMs = subtitle.Paragraphs[0].StartTime.TotalMilliseconds;
        var endMs = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds;
        var partSize = (endMs - startMs) / numberOfParts;
        var nextLimit = startMs + partSize;
        var temp = new Subtitle { Header = subtitle.Header };
        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var p = subtitle.Paragraphs[i];
            if (temp.Paragraphs.Count > 0 && parts.Count < numberOfParts - 1 &&
                (p.StartTime.TotalMilliseconds > nextLimit - 10 || MustStartNewPart(subtitle, i, parts.Count, numberOfParts)))
            {
                parts.Add(temp);
                temp = new Subtitle { Header = subtitle.Header };
                temp.Paragraphs.Add(new Paragraph(p));

                // Move the limit past this paragraph, not just one step: after a long gap a single
                // step left the limit behind, so every following cue started a tiny part of its own.
                do
                {
                    nextLimit += partSize;
                } while (partSize > 0 && p.StartTime.TotalMilliseconds > nextLimit - 10);
            }
            else
            {
                temp.Paragraphs.Add(new Paragraph(p));
            }
        }

        // See note in SplitByCharacters - the final part must be added as well.
        parts.Add(temp);

        return parts;
    }

    /// <summary>
    /// True when paragraph <paramref name="index"/> must open a new part so the requested part count
    /// can still be reached: if it joined the current part, the paragraphs left after it would be
    /// fewer than the parts still to fill. Without this, short texts (character mode, because of the
    /// +4 slack) or cues bunched at the start (time mode) ended with fewer parts than requested.
    /// </summary>
    private static bool MustStartNewPart(Subtitle subtitle, int index, int closedParts, int numberOfParts)
    {
        var paragraphsLeftIncludingThis = subtitle.Paragraphs.Count - index;
        var partsLeftIncludingCurrent = numberOfParts - closedParts;
        return paragraphsLeftIncludingThis < partsLeftIncludingCurrent;
    }
}
