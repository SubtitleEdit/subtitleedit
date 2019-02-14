using System;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public static class SplitLongLinesHelper
    {
        public static bool QualifiesForSplit(string text, int singleLineMaxCharacters, int totalLineMaxCharacters)
        {
            string s = HtmlUtil.RemoveHtmlTags(text.Trim(), true);
            if (s.Length > totalLineMaxCharacters)
            {
                return true;
            }

            foreach (var line in s.SplitToLines())
            {
                if (line.Length > singleLineMaxCharacters)
                {
                    return true;
                }
            }

            var tempText = Utilities.UnbreakLine(s);
            if (Utilities.CountTagInText(tempText, '-') == 2 && (text.StartsWith('-') || text.StartsWith("<i>-")))
            {
                var idx = tempText.IndexOfAny(new[] { ". -", "! -", "? -" }, StringComparison.Ordinal);
                if (idx > 1)
                {
                    idx++;
                    string dialogText = tempText.Remove(idx, 1).Insert(idx, Environment.NewLine);
                    foreach (string line in dialogText.SplitToLines())
                    {
                        if (line.Length > singleLineMaxCharacters)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
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
                string noTagText = HtmlUtil.RemoveHtmlTags(oldParagraph.Text, true);
                double millisecondsPerChar = oldParagraph.Duration.TotalMilliseconds / (noTagText.Length - Environment.NewLine.Length);

                oldParagraph.Text = Utilities.AutoBreakLine(lines[FirstLine], language);

                // use optimal time to adjust duration
                oldParagraph.EndTime.TotalMilliseconds = oldParagraph.StartTime.TotalMilliseconds + millisecondsPerChar * oldParagraph.Text.Length - halfMinGaps;

                // build second paragraph
                var newParagraph = new Paragraph(oldParagraph);
                newParagraph.Text = Utilities.AutoBreakLine(lines[SecondLine], language);
                newParagraph.StartTime.TotalMilliseconds = oldParagraph.EndTime.TotalMilliseconds + halfMinGapsMood;

                // only remove dash (of dialog) if first line is fully closed
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
                return false;
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
            if (string.IsNullOrEmpty(text) || text.Length == 0)
            {
                return false;
            }
            string textNoTags = HtmlUtil.RemoveHtmlTags(text);
            char lastChar = textNoTags[textNoTags.Length];
            return lastChar == '.' || lastChar == '!' || lastChar == '?' || lastChar == ':' || lastChar == ')' || lastChar == ']' || lastChar == '♪';
        }

    }
}
