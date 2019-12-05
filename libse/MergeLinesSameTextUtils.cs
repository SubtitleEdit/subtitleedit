using System;

namespace Nikse.SubtitleEdit.Core
{
    public static class MergeLinesSameTextUtils
    {
        public static Subtitle MergeLinesWithSameTextInSubtitle(Subtitle subtitle, bool fixIncrementing, bool lineAfterNext, int maxMsBetween)
        {
            var mergedSubtitle = new Subtitle();
            bool lastMerged = false;
            Paragraph p = null;
            for (int i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                if (!lastMerged)
                {
                    p = new Paragraph(subtitle.GetParagraphOrDefault(i - 1));
                    mergedSubtitle.Paragraphs.Add(p);
                }
                var next = subtitle.GetParagraphOrDefault(i);
                var afterNext = subtitle.GetParagraphOrDefault(i + 1);
                if (next != null)
                {
                    if (QualifiesForMerge(p, next, maxMsBetween) || fixIncrementing && QualifiesForMergeIncrement(p, next, maxMsBetween))
                    {
                        p.Text = next.Text;
                        p.EndTime = next.EndTime;
                        lastMerged = true;
                    }
                    else if (lineAfterNext && QualifiesForMerge(p, afterNext, maxMsBetween) && p.Duration.TotalMilliseconds > afterNext.Duration.TotalMilliseconds)
                    {
                        lastMerged = true;
                    }
                    else
                    {
                        lastMerged = false;
                    }
                }
                else
                {
                    lastMerged = false;
                }
            }
            if (!lastMerged)
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
                string s = HtmlUtil.RemoveHtmlTags(p.Text.Trim());
                string s2 = HtmlUtil.RemoveHtmlTags(next.Text.Trim());
                return string.Compare(s, s2, StringComparison.OrdinalIgnoreCase) == 0;
            }
            return false;
        }

        public static bool QualifiesForMergeIncrement(Paragraph p, Paragraph next, int maxMsBetween)
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
                var s = HtmlUtil.RemoveHtmlTags(p.Text.Trim());
                var s2 = HtmlUtil.RemoveHtmlTags(next.Text.Trim());
                if (!string.IsNullOrEmpty(s) && s2.Length > 0 && s2.StartsWith(s, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
