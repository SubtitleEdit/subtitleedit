using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class MergeLinesSameTextUtils
    {
        public static Subtitle MergeLinesWithSameTextInSubtitle(Subtitle subtitle, bool fixIncrementing, int maxMsBetween)
        {
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
