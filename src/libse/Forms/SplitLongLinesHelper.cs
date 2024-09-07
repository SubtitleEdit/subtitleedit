using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public static class SplitLongLinesHelper
    {
        public static bool QualifiesForSplit(string text, int singleLineMaxCharacters, int totalLineMaxCharacters)
        {
            var noTagText = HtmlUtil.RemoveHtmlTags(text.Trim(), true);
            if (noTagText.CountCharacters(false) > totalLineMaxCharacters)
            {
                return true;
            }

            foreach (var noTagLine in noTagText.SplitToLines())
            {
                if (noTagLine.Length > singleLineMaxCharacters)
                {
                    return true;
                }
            }

            if (!noTagText.StartsWith('-') || Utilities.CountTagInText(text, "- ") < 2)
            {
                return false;
            }

            var noTagSingleLine = Utilities.UnbreakLine(noTagText);
            var lineSplitIdx = noTagSingleLine.IndexOfAny(new[] { ". -", "! -", "? -", "] -", ") -" }, StringComparison.Ordinal);
            if (lineSplitIdx < 0)
            {
                return false;
            }

            var maxLineLen = Math.Max(lineSplitIdx + 1, noTagSingleLine.Length - (lineSplitIdx + 2));
            return maxLineLen > singleLineMaxCharacters;
        }

        public static Subtitle SplitLongLinesInSubtitle(Subtitle subtitle, int totalLineMaxCharacters, int singleLineMaxCharacters)
        {
            var splitSubtitle = new Subtitle(subtitle);
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);

            // calculate gaps
            var halfMinGaps = Configuration.Settings.General.MinimumMillisecondsBetweenLines / 2.0;
            var halfMinGapsMood = halfMinGaps + Configuration.Settings.General.MinimumMillisecondsBetweenLines % 2;

            const int firstLine = 0;
            const int secondLine = 1;

            for (var i = splitSubtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                var oldParagraph = splitSubtitle.Paragraphs[i];

                // don't split into two paragraph if it can be balanced
                var text = oldParagraph.Text;
                var noHtmlLines = HtmlUtil.RemoveHtmlTags(text, true).SplitToLines();

                var autoBreak = !(noHtmlLines.Count == 2 && noHtmlLines[0].Length <= totalLineMaxCharacters && noHtmlLines[1].Length < totalLineMaxCharacters &&
                                  (noHtmlLines[0].EndsWith('.') || noHtmlLines[0].EndsWith('!') || noHtmlLines[0].EndsWith('?') || noHtmlLines[0].EndsWith('؟')));

                if (autoBreak)
                {
                    text = Utilities.AutoBreakLine(oldParagraph.Text, language, true);
                }

                if (noHtmlLines.Count == 1 && text.SplitToLines().Count <= 2 && noHtmlLines[0].Length > singleLineMaxCharacters && !QualifiesForSplit(text, singleLineMaxCharacters, totalLineMaxCharacters))
                {
                    oldParagraph.Text = text;
                    continue;
                }

                if (!QualifiesForSplit(text, singleLineMaxCharacters, totalLineMaxCharacters))
                {
                    continue;
                }

                // continue if paragraph doesn't contain exactly two lines
                var lines = text.SplitToLines();
                if (lines.Count > 2 || lines.Count == 1)
                {
                    continue; // ignore 3+ lines or one liners
                }

                // calculate milliseconds per char
                var millisecondsPerChar = oldParagraph.DurationTotalMilliseconds / (HtmlUtil.RemoveHtmlTags(text, true).Length - Environment.NewLine.Length);

                oldParagraph.Text = lines[firstLine];

                // use optimal time to adjust duration
                oldParagraph.EndTime.TotalMilliseconds = oldParagraph.StartTime.TotalMilliseconds + millisecondsPerChar * HtmlUtil.RemoveHtmlTags(oldParagraph.Text, true).Length - halfMinGaps;

                // build second paragraph
                var newParagraph = new Paragraph(oldParagraph) { Text = lines[secondLine] };
                newParagraph.StartTime.TotalMilliseconds = oldParagraph.EndTime.TotalMilliseconds + halfMinGapsMood;
                newParagraph.EndTime.TotalMilliseconds = newParagraph.StartTime.TotalMilliseconds + millisecondsPerChar * HtmlUtil.RemoveHtmlTags(newParagraph.Text, true).Length;

                // only remove dash (if dialog) if first line is fully closed and "Split removes dashes" is true
                if (Configuration.Settings.General.SplitRemovesDashes && IsTextClosed(oldParagraph.Text))
                {
                    RemoveInvalidDash(oldParagraph, newParagraph);
                }

                // handle invalid tags
                if (oldParagraph.Text.Contains('<'))
                {
                    oldParagraph.Text = HtmlUtil.FixInvalidItalicTags(oldParagraph.Text);
                }
                if (newParagraph.Text.Contains('<'))
                {
                    newParagraph.Text = HtmlUtil.FixInvalidItalicTags(newParagraph.Text);
                }

                oldParagraph.Text = Utilities.AutoBreakLine(oldParagraph.Text, language);
                newParagraph.Text = Utilities.AutoBreakLine(newParagraph.Text, language);

                // insert new paragraph after the current/old one
                splitSubtitle.Paragraphs.Insert(i + 1, newParagraph);
            }

            splitSubtitle.Renumber();
            return splitSubtitle;
        }

        private const char Dash = '-'; // U+002D - HYPHEN-MINUS

        private static void RemoveInvalidDash(Paragraph p1, Paragraph p2)
        {
            // return if not dialog
            if ((StartsWithDash(p1.Text) && StartsWithDash(p2.Text)) == false)
            {
                return;
            }

            // update first text
            var dashIdx = p1.Text.IndexOf(Dash);
            p1.Text = p1.Text.Substring(0, dashIdx) + p1.Text.Substring(dashIdx + 1).TrimStart();

            // update second text
            dashIdx = p2.Text.IndexOf(Dash);
            p2.Text = p2.Text.Substring(0, dashIdx) + p2.Text.Substring(dashIdx + 1).TrimStart();
        }

        private static bool StartsWithDash(string text)
        {
            if (!text.LineStartsWithHtmlTag(true, true))
            {
                return text.StartsWith(Dash);
            }

            var closeIdx = text.IndexOf('>');
            if (closeIdx + 1 == text.Length) // found in last position
            {
                return false;
            }

            return text[closeIdx + 1] == Dash;
        }

        private static bool IsTextClosed(string text)
        {
            var noTagText = HtmlUtil.RemoveHtmlTags(text);
            if (string.IsNullOrEmpty(noTagText))
            {
                return false;
            }

            var lastChar = noTagText[noTagText.Length - 1];
            return ".!?؟:)]♪".Contains(lastChar);
        }
    }
}
