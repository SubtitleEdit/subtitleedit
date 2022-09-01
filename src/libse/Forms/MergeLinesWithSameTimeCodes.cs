using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public static class MergeLinesWithSameTimeCodes
    {
        public static Subtitle Merge(Subtitle subtitle, List<int> mergedIndexes, out int numberOfMerges, bool clearFixes, bool makeDialog, bool reBreak, int maxMsBetween, string language, List<int> removed, Dictionary<int, bool> isFixAllowedList, Subtitle info)
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
                var next = subtitle.GetParagraphOrDefault(i);
                if (next != null)
                {
                    if (QualifiesForMerge(p, next, maxMsBetween) && IsFixAllowed(p, isFixAllowedList))
                    {
                        var nextText = next.Text
                            .Replace("{\\an1}", string.Empty)
                            .Replace("{\\an2}", string.Empty)
                            .Replace("{\\an3}", string.Empty)
                            .Replace("{\\an4}", string.Empty)
                            .Replace("{\\an5}", string.Empty)
                            .Replace("{\\an6}", string.Empty)
                            .Replace("{\\an7}", string.Empty)
                            .Replace("{\\an8}", string.Empty)
                            .Replace("{\\an9}", string.Empty);

                        if (p.Text.StartsWith("<i>", StringComparison.Ordinal) && p.Text.EndsWith("</i>", StringComparison.Ordinal) && nextText.StartsWith("<i>", StringComparison.Ordinal) && nextText.EndsWith("</i>", StringComparison.Ordinal))
                        {
                            p.Text = GetMergedLines(p.Text.Remove(p.Text.Length - 4), nextText.Remove(0, 3), makeDialog);
                        }
                        else
                        {
                            p.Text = GetMergedLines(p.Text, nextText, makeDialog);
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

            if (!lastMerged && subtitle.Paragraphs.Count > 0)
            {
                mergedSubtitle.Paragraphs.Add(new Paragraph(subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1)));
            }

            mergedSubtitle.Renumber();
            return mergedSubtitle;
        }

        private static string GetMergedLines(string line1, string line2, bool makeDialog)
        {
            if (makeDialog)
            {
                switch (Configuration.Settings.General.DialogStyle)
                {
                    case Enums.DialogType.DashBothLinesWithoutSpace:
                        return (line1.StartsWith("-") ? "" : "-") + line1 + Environment.NewLine + "-" + line2;
                    case Enums.DialogType.DashSecondLineWithSpace:
                        return line1 + Environment.NewLine + "- " + line2;
                    case Enums.DialogType.DashSecondLineWithoutSpace:
                        return line1 + Environment.NewLine + "-" + line2;
                    default:
                        return (line1.StartsWith("- ") ? "" : "- ") + line1 + Environment.NewLine + "- " + line2;
                }
            }
            else
            {
                return line1 + Environment.NewLine + line2;
            }
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
