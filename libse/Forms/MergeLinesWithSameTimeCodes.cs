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
                        p.Text = JoinLines(p.Text, next.Text);
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

        private static string JoinLines(string firstLine, string secondLine)
        {
            // handle tag in both lines
            if (IsPairedTagWrap(firstLine) && IsPairedTagWrap(secondLine))
            {
                // get starting tags from both lines
                string firstLineStartTag = GetFirstOpenTagFromLast(firstLine);
                string secondlineStartTag = GetFirstOpenTagFromLast(secondLine);

                // congruent tags
                if (firstLineStartTag.Equals(secondlineStartTag, StringComparison.OrdinalIgnoreCase))
                {
                    // get last closing position
                    int closingIdx = firstLine.LastIndexOf('<');

                    // remove last closing tag, 'cause it will be closed in second line
                    firstLine = firstLine.Remove(closingIdx);

                    // last closing tag position
                    int lastClosingIdx = secondLine.IndexOf("</", secondLine.Length - 4);

                    if (lastClosingIdx - 1 >= 0)
                    {
                        // not try to find first opening tag using last-closing tag position
                        int lastOpeningTag = secondLine.LastIndexOf('<', lastClosingIdx - 1);

                        // calc remove len
                        int removeLen = secondLine.IndexOf('>', lastOpeningTag + 2) - lastOpeningTag + 1;

                        // remove first opening tag from second line which will be use with first line its opening tag to make one tag pair
                        secondLine = secondLine.Remove(lastOpeningTag, removeLen);
                    }
                }
            }

            return (firstLine + Environment.NewLine + secondLine).Trim();
        }

        private static string GetFirstOpenTagFromLast(string input)
        {
            // last tag closing position
            int lastCLosingStartIdx = input.LastIndexOf('<', input.Length - 4);
            // last tag open start position
            int lastOpenStartIdx = input.LastIndexOf('<', lastCLosingStartIdx);

            if (lastOpenStartIdx >= 0)
            {
                int lastOpenCloseIdx = input.IndexOf('>', lastOpenStartIdx + 2);
                if (lastOpenCloseIdx >= 0)
                {
                    return input.Substring(lastOpenStartIdx, lastOpenCloseIdx - lastOpenStartIdx + 1);
                }
            }

            // invalid tag
            return string.Empty;
        }

        private static bool IsPairedTagWrap(string input)
        {
            if (!input.LineEndsWithHtmlTag(true, true))
            {
                return false;
            }

            string lastClosingTag = input.Substring(input.LastIndexOf('<'));

            // build opening tag from closing tag
            string openTag = GetOpeningTagFromClose(lastClosingTag);

            // ensure that there is only one opening tag
            if (Utilities.CountTagInText(input, openTag) != 1)
            {
                return false;
            }

            return true;
        }

        private static string GetOpeningTagFromClose(string closingTag)
        {
            switch (closingTag)
            {
                case "</i>":
                    return "<i>";
                case "</b>":
                    return "<b>";
                case "</u>":
                    return "<u>";
                default:
                    return "<font ";
            }
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
