using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public static class MergeLinesWithSameTimeCodes
    {
        public static Subtitle Merge(Subtitle subtitle, List<int> mergedIndexes, out int numberOfMerges, bool clearFixes, bool reBreak, int maxMsBetween, string language, List<int> removed, Dictionary<int, bool> isFixAllowedList, Subtitle info)
        {
            numberOfMerges = 0;
            var mergedSubtitle = new Subtitle();
            bool lastMerged = false;
            Paragraph p = null;
            var lineNumbers = new StringBuilder();
            for (int i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                if (!lastMerged)
                {
                    p = new Paragraph(subtitle.GetParagraphOrDefault(i - 1));
                    mergedSubtitle.Paragraphs.Add(p);
                }
                Paragraph next = subtitle.GetParagraphOrDefault(i);
                if (next != null)
                {
                    if (QualifiesForMerge(p, next, maxMsBetween) && IsFixAllowed(p, isFixAllowedList))
                    {
                        if (p.Text.StartsWith("<i>", StringComparison.Ordinal) && p.Text.EndsWith("</i>", StringComparison.Ordinal) && next.Text.StartsWith("<i>", StringComparison.Ordinal) && next.Text.EndsWith("</i>", StringComparison.Ordinal))
                        {
                            p.Text = p.Text.Remove(p.Text.Length - 4) + Environment.NewLine + next.Text.Remove(0, 3);
                        }
                        else
                        {
                            p.Text = p.Text + Environment.NewLine + next.Text;
                        }
                        if (reBreak)
                        {
                            p.Text = Utilities.AutoBreakLine(p.Text, language);
                        }

                        lastMerged = true;
                        removed.Add(i);
                        numberOfMerges++;
                        if (!mergedIndexes.Contains(i))
                        {
                            mergedIndexes.Add(i);
                        }

                        if (!mergedIndexes.Contains(i - 1))
                        {
                            mergedIndexes.Add(i - 1);
                        }

                        if (!("," + lineNumbers).Contains("," + p.Number + ","))
                        {
                            lineNumbers.Append(p.Number);
                            lineNumbers.Append(',');
                        }
                        if (!("," + lineNumbers).Contains("," + next.Number + ","))
                        {
                            lineNumbers.Append(next.Number);
                            lineNumbers.Append(',');
                        }
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

                if (!removed.Contains(i) && lineNumbers.Length > 0 && clearFixes)
                {
                    info.Paragraphs.Add(new Paragraph(p) { Extra = lineNumbers.ToString() });
                    lineNumbers.Clear();
                }
            }
            if (lineNumbers.Length > 0 && clearFixes && p != null)
            {
                info.Paragraphs.Add(new Paragraph(p) { Extra = lineNumbers.ToString() });
            }
            if (!lastMerged)
            {
                mergedSubtitle.Paragraphs.Add(new Paragraph(subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1)));
            }

            mergedSubtitle.Renumber();
            return mergedSubtitle;
        }

        private static bool QualifiesForMerge(Paragraph p, Paragraph next, int maxMsBetween)
        {
            if (p == null || next == null)
            {
                return false;
            }

            return Math.Abs(next.StartTime.TotalMilliseconds - p.StartTime.TotalMilliseconds) <= maxMsBetween &&
                   Math.Abs(next.EndTime.TotalMilliseconds - p.EndTime.TotalMilliseconds) <= maxMsBetween;
        }

        private static bool IsFixAllowed(Paragraph p, Dictionary<int, bool> isFixAllowedList)
        {
            if (isFixAllowedList.ContainsKey(p.Number))
            {
                return isFixAllowedList[p.Number];
            }

            return true;
        }
    }
}
