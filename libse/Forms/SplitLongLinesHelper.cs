using System;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public static class SplitLongLinesHelper
    {
        public static bool QualifiesForSplit(string text, int singleLineMaxCharacters, int totalLineMaxCharacters)
        {
            string noTagText = HtmlUtil.RemoveHtmlTags(text.Trim(), true);
            if (noTagText.Length > totalLineMaxCharacters)
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

            string singleLineNoTag = Utilities.UnbreakLine(noTagText);
            int lineSplitIdx = singleLineNoTag.IndexOfAny(new[] { ". -", "! -", "? -", "] -", ") -" }, StringComparison.OrdinalIgnoreCase);
            if (lineSplitIdx < 0)
            {
                return false;
            }

            int maxLineLen = Math.Max(lineSplitIdx + 1, singleLineNoTag.Length - (lineSplitIdx + 2));
            return maxLineLen > singleLineMaxCharacters;
        }

        public static Subtitle SplitLongLinesInSubtitle(Subtitle subtitle, int totalLineMaxCharacters, int singleLineMaxCharacters)
        {
            var splittedSubtitle = new Subtitle(subtitle);
            string language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);

            // calculate gaps
            var halfMinGaps = Configuration.Settings.General.MinimumMillisecondsBetweenLines / 2.0;
            var halfMinGapsMood = halfMinGaps + Configuration.Settings.General.MinimumMillisecondsBetweenLines % 2;

            const int FirstLine = 0;
            const int SecondLine = 1;

            for (int i = splittedSubtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                var oldParagraph = splittedSubtitle.Paragraphs[i];

                // don't split into two paragraph if it can be balanced
                var text = Utilities.AutoBreakLine(oldParagraph.Text, language);
                if (!QualifiesForSplit(text, singleLineMaxCharacters, totalLineMaxCharacters))
                {
                    oldParagraph.Text = text;
                    continue;
                }

                // continue if paragraph doesn't contain exactly two lines
                var lines = text.SplitToLines();
                if (lines.Count != 2)
                {
                    continue; // ignore 3+ lines
                }

                // calculate milliseconds per char
                double millisecondsPerChar = oldParagraph.Duration.TotalMilliseconds / (HtmlUtil.RemoveHtmlTags(text, true).Length - Environment.NewLine.Length);

                oldParagraph.Text = lines[FirstLine];

                // use optimal time to adjust duration
                oldParagraph.EndTime.TotalMilliseconds = oldParagraph.StartTime.TotalMilliseconds + millisecondsPerChar * HtmlUtil.RemoveHtmlTags(oldParagraph.Text, true).Length - halfMinGaps;

                // build second paragraph
                var newParagraph = new Paragraph(oldParagraph) { Text = lines[SecondLine] };
                newParagraph.StartTime.TotalMilliseconds = oldParagraph.EndTime.TotalMilliseconds + halfMinGapsMood;
                newParagraph.EndTime.TotalMilliseconds = newParagraph.StartTime.TotalMilliseconds + millisecondsPerChar * HtmlUtil.RemoveHtmlTags(newParagraph.Text, true).Length;

                // only remove dash (if dialog) if first line is fully closed
                if (IsTextClosed(oldParagraph.Text))
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
                splittedSubtitle.Paragraphs.Insert(i + 1, newParagraph);
            }

            splittedSubtitle.Renumber();
            return splittedSubtitle;
        }

        private static void RemoveInvalidDash(Paragraph p1, Paragraph p2)
        {
            // return if not dialog
            if ((StartsWithDash(p1.Text) && StartsWithDash(p2.Text)) == false)
            {
                return;
            }
            const char Dash = '-';
            // update first text
            int dashIdx = p1.Text.IndexOf(Dash);
            p1.Text = p1.Text.Substring(0, dashIdx) + p1.Text.Substring(dashIdx + 1).TrimStart();
            // update second text
            dashIdx = p2.Text.IndexOf(Dash);
            p2.Text = p2.Text.Substring(0, dashIdx) + p2.Text.Substring(dashIdx + 1).TrimStart();
        }

        private static bool StartsWithDash(string text)
        {
            if (!text.LineStartsWithHtmlTag(true, true))
            {
                return text.StartsWith('-');
            }
            int closeIdx = text.IndexOf('>');
            if (closeIdx + 1 == text.Length) // found in last position
            {
                return false;
            }
            return text[closeIdx + 1] == '-';
        }

        private static bool IsTextClosed(string text)
        {
            string textNoTags = HtmlUtil.RemoveHtmlTags(text);
            if (string.IsNullOrEmpty(textNoTags))
            {
                {
                    return false;
                }
            }
            char lastChar = textNoTags[textNoTags.Length - 1];
            return lastChar == '.' || lastChar == '!' || lastChar == '?' || lastChar == ':' || lastChar == ')' || lastChar == ']' || lastChar == '♪';
        }

    }
}
