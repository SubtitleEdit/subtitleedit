using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class MergeLinesSameTextUtils
    {
        public static Subtitle MergeLinesWithSameTextInSubtitle(Subtitle subtitle, bool fixIncrementing, int maxMsBetween)
        {
            var mergedIndexes = new List<int>();
            var removed = new List<int>();
            var mergedSubtitle = new Subtitle();
            for (int i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                if (removed.Contains(i - 1))
                {
                    continue;
                }

                var p = new Paragraph(subtitle.GetParagraphOrDefault(i - 1));
                mergedSubtitle.Paragraphs.Add(p);

                for (int j = i; j < subtitle.Paragraphs.Count; j++)
                {
                    if (removed.Contains(j))
                    {
                        continue;
                    }

                    var next = subtitle.GetParagraphOrDefault(j);
                    var incrementText = string.Empty;
                    if ((QualifiesForMerge(p, next, maxMsBetween) || fixIncrementing && QualifiesForMergeIncrement(p, next, maxMsBetween, out incrementText)))
                    {
                        p.Text = next.Text;
                        if (!string.IsNullOrEmpty(incrementText))
                        {
                            p.Text = incrementText;
                        }

                        p.EndTime.TotalMilliseconds = next.EndTime.TotalMilliseconds;
                        removed.Add(j);
                        if (!mergedIndexes.Contains(j))
                        {
                            mergedIndexes.Add(j);
                        }

                        if (!mergedIndexes.Contains(i - 1))
                        {
                            mergedIndexes.Add(i - 1);
                        }
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
                string currentTextNoTags = HtmlUtil.RemoveHtmlTags(p.Text.Trim());
                string nextTextNoTags = HtmlUtil.RemoveHtmlTags(next.Text.Trim());
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
