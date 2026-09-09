using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class MergeLinesSameTextUtils
    {
        public static Subtitle MergeLinesWithSameTextInSubtitle(Subtitle subtitle, bool fixIncrementing, int maxMsBetween)
        {
            return MergeLinesWithSameTextInSubtitle(subtitle, fixIncrementing, maxMsBetween, false);
        }

        public static Subtitle MergeLinesWithSameTextInSubtitle(Subtitle subtitle, bool fixIncrementing, int maxMsBetween, bool includeRollUp)
        {
            if (includeRollUp)
            {
                subtitle = MergeRollUpCaptions(subtitle, maxMsBetween);
            }

            var mergedIndexes = new HashSet<int>();
            var removed = new HashSet<int>();
            var mergedSubtitle = new Subtitle();

            // With start times in order, once a candidate starts more than maxMsBetween after the
            // current end nothing later can qualify either (both predicates reject on that gap
            // first, and the end only moves on a merge) - so the inner scan can stop there. The
            // old scan went to the end of the subtitle for every paragraph.
            var startTimesInOrder = true;
            for (var i = 1; i < subtitle.Paragraphs.Count && startTimesInOrder; i++)
            {
                startTimesInOrder = subtitle.Paragraphs[i].StartTime.TotalMilliseconds >= subtitle.Paragraphs[i - 1].StartTime.TotalMilliseconds;
            }

            for (var i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                if (removed.Contains(i - 1))
                {
                    continue;
                }

                var p = new Paragraph(subtitle.GetParagraphOrDefault(i - 1));
                mergedSubtitle.Paragraphs.Add(p);

                for (var j = i; j < subtitle.Paragraphs.Count; j++)
                {
                    if (removed.Contains(j))
                    {
                        continue;
                    }

                    var next = subtitle.GetParagraphOrDefault(j);
                    if (startTimesInOrder && next != null && next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds > maxMsBetween)
                    {
                        break;
                    }

                    var incrementText = string.Empty;
                    if (QualifiesForMerge(p, next, maxMsBetween) || fixIncrementing && QualifiesForMergeIncrement(p, next, maxMsBetween, out incrementText))
                    {
                        p.Text = next.Text;
                        if (!string.IsNullOrEmpty(incrementText))
                        {
                            p.Text = incrementText;
                        }

                        p.EndTime.TotalMilliseconds = next.EndTime.TotalMilliseconds;
                        removed.Add(j);
                        mergedIndexes.Add(j);
                        mergedIndexes.Add(i - 1);
                    }
                }
            }

            if (subtitle.Paragraphs.Count > 0 && !mergedIndexes.Contains(subtitle.Paragraphs.Count - 1))
            {
                mergedSubtitle.Paragraphs.Add(new Paragraph(subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1)));
            }

            mergedSubtitle.Renumber();
            return mergedSubtitle;
        }


        /// <summary>
        /// Rewrites roll-up (scrolling) caption chains - where each caption shows the tail of the
        /// previous one plus new lines, e.g. "A", "A/B", "A/B/C", "B/C/D", "C/D/E" - so every line
        /// appears once, timed from when it first scrolled in and re-chunked into paragraphs of
        /// <see cref="GeneralSettings.MaxNumberOfLines"/> lines. Non-roll-up paragraphs are copied as is.
        /// </summary>
        public static Subtitle MergeRollUpCaptions(Subtitle subtitle, int maxMsBetween)
        {
            var maxLines = Math.Max(1, Configuration.Settings.General.MaxNumberOfLines);
            var result = new Subtitle(subtitle);
            result.Paragraphs.Clear();
            var i = 0;
            while (i < subtitle.Paragraphs.Count)
            {
                if (TryGetRollUpChain(subtitle.Paragraphs, i, maxMsBetween, maxLines, out var endIndex, out var merged))
                {
                    result.Paragraphs.AddRange(merged);
                    i = endIndex + 1;
                }
                else
                {
                    result.Paragraphs.Add(new Paragraph(subtitle.Paragraphs[i]));
                    i++;
                }
            }

            result.Renumber();
            return result;
        }

        /// <summary>
        /// Detects a roll-up chain starting at <paramref name="startIndex"/>. A chain step is a
        /// paragraph whose lines begin with a non-empty suffix of the previous paragraph's lines,
        /// optionally followed by new lines. The chain must contain at least one real scroll (a
        /// top line dropping off), otherwise it is a plain incrementing sequence handled by
        /// <see cref="QualifiesForMergeIncrement"/>.
        /// </summary>
        public static bool TryGetRollUpChain(List<Paragraph> paragraphs, int startIndex, int maxMsBetween, int maxLines, out int endIndex, out List<Paragraph> merged)
        {
            endIndex = startIndex;
            merged = new List<Paragraph>();
            if (paragraphs == null || startIndex < 0 || startIndex >= paragraphs.Count)
            {
                return false;
            }

            var first = paragraphs[startIndex];
            var prevLines = GetComparableLines(first);
            if (prevLines.Count == 0)
            {
                return false;
            }

            // Every distinct line, with its raw (tagged) text and the time it first scrolled in.
            var lineTexts = new List<string>(first.Text.SplitToLines());
            var lineStarts = new List<double>();
            for (var k = 0; k < lineTexts.Count; k++)
            {
                lineStarts.Add(first.StartTime.TotalMilliseconds);
            }

            var chainEndMs = first.EndTime.TotalMilliseconds;
            var sawScroll = false;
            var lastIndex = startIndex;
            for (var j = startIndex + 1; j < paragraphs.Count; j++)
            {
                var next = paragraphs[j];
                if (next.StartTime.TotalMilliseconds - chainEndMs > maxMsBetween)
                {
                    break;
                }

                var nextLines = GetComparableLines(next);
                var overlap = GetRollUpOverlap(prevLines, nextLines);
                if (overlap <= 0)
                {
                    break;
                }

                if (overlap < prevLines.Count)
                {
                    sawScroll = true;
                }

                var nextRawLines = next.Text.SplitToLines();
                for (var k = overlap; k < nextLines.Count; k++)
                {
                    lineTexts.Add(k < nextRawLines.Count ? nextRawLines[k] : nextLines[k]);
                    lineStarts.Add(next.StartTime.TotalMilliseconds);
                }

                chainEndMs = Math.Max(chainEndMs, next.EndTime.TotalMilliseconds);
                prevLines = nextLines;
                lastIndex = j;
            }

            if (!sawScroll || lastIndex == startIndex)
            {
                return false;
            }

            for (var k = 0; k < lineTexts.Count; k += maxLines)
            {
                var count = Math.Min(maxLines, lineTexts.Count - k);
                var text = string.Join(Environment.NewLine, lineTexts.GetRange(k, count));
                var start = lineStarts[k];
                var end = k + maxLines < lineTexts.Count ? lineStarts[k + maxLines] : chainEndMs;
                if (end < start)
                {
                    end = start;
                }

                var p = new Paragraph(first) { Text = text };
                p.StartTime.TotalMilliseconds = start;
                p.EndTime.TotalMilliseconds = end;
                merged.Add(p);
            }

            endIndex = lastIndex;
            return true;
        }

        private static List<string> GetComparableLines(Paragraph p)
        {
            var lines = new List<string>();
            if (p?.Text == null)
            {
                return lines;
            }

            foreach (var line in HtmlUtil.RemoveHtmlTags(p.Text, true).SplitToLines())
            {
                var trimmed = line.Trim();
                if (trimmed.Length > 0)
                {
                    lines.Add(trimmed);
                }
            }

            return lines;
        }

        /// <summary>
        /// Returns the number of leading lines in <paramref name="next"/> that equal a suffix of
        /// <paramref name="prev"/> (the longest such suffix), or 0 when the captions do not chain.
        /// </summary>
        private static int GetRollUpOverlap(List<string> prev, List<string> next)
        {
            if (prev.Count == 0 || next.Count == 0)
            {
                return 0;
            }

            for (var overlap = Math.Min(prev.Count, next.Count); overlap > 0; overlap--)
            {
                var matches = true;
                for (var k = 0; k < overlap && matches; k++)
                {
                    matches = string.Equals(prev[prev.Count - overlap + k], next[k], StringComparison.OrdinalIgnoreCase);
                }

                if (matches)
                {
                    return overlap;
                }
            }

            return 0;
        }

        public static bool QualifiesForMerge(Paragraph p, Paragraph next, int maxMsBetween)
        {
            if (p == null || next == null)
            {
                return false;
            }

            if (next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds > maxMsBetween)
            {
                return false;
            }

            if (p.Text != null && next.Text != null)
            {
                var currentTextNoTags = HtmlUtil.RemoveHtmlTags(p.Text.Trim());
                var nextTextNoTags = HtmlUtil.RemoveHtmlTags(next.Text.Trim());
                return string.Compare(currentTextNoTags, nextTextNoTags, StringComparison.OrdinalIgnoreCase) == 0;
            }

            return false;
        }

        public static bool QualifiesForMergeIncrement(Paragraph current, Paragraph next, int maxMsBetween, out string text)
        {
            text = string.Empty;
            if (current == null || next == null)
            {
                return false;
            }

            if (next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds > maxMsBetween)
            {
                return false;
            }

            if (current.Text != null && next.Text != null)
            {
                var currentTextNoTags = HtmlUtil.RemoveHtmlTags(current.Text.Trim());
                var nextTextNoTags = HtmlUtil.RemoveHtmlTags(next.Text.Trim());
                if (string.IsNullOrEmpty(currentTextNoTags) || string.IsNullOrEmpty(nextTextNoTags))
                {
                    return false;
                }

                if (nextTextNoTags.StartsWith(currentTextNoTags, StringComparison.OrdinalIgnoreCase))
                {
                    text = next.Text;
                    return true;
                }

                var lines = currentTextNoTags.SplitToLines();
                if (lines.Count > 1 && lines.Last().Equals(nextTextNoTags, StringComparison.OrdinalIgnoreCase))
                {
                    text = current.Text;
                    return true;
                }
            }
            return false;
        }
    }
}
