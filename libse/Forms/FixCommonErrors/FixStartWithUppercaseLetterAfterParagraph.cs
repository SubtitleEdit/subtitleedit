using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixStartWithUppercaseLetterAfterParagraph : IFixCommonError
    {
        private static bool _isEnglish;

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            _isEnglish = callbacks.Language.Equals("en", StringComparison.Ordinal);
            string fixAction = language.FixFirstLetterToUppercaseAfterParagraph;
            int fixedStartWithUppercaseLetterAfterParagraphTicked = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                Paragraph prev = subtitle.GetParagraphOrDefault(i - 1);

                string oldText = p.Text;
                string fixedText = DoFix(new Paragraph(p), prev, callbacks.Encoding, callbacks.Language);

                if (oldText != fixedText && callbacks.AllowFix(p, fixAction))
                {
                    p.Text = fixedText;
                    fixedStartWithUppercaseLetterAfterParagraphTicked++;
                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }
            callbacks.UpdateFixStatus(fixedStartWithUppercaseLetterAfterParagraphTicked, language.StartWithUppercaseLetterAfterParagraph, fixedStartWithUppercaseLetterAfterParagraphTicked.ToString(CultureInfo.InvariantCulture));
        }

        private static string DoFix(Paragraph p, Paragraph prev, Encoding encoding, string language)
        {
            string preTextNoTags = null;
            bool isPrevParagraphClosed = IsPreParagraphClose(prev, p);
            if (isPrevParagraphClosed && prev != null)
            {
                preTextNoTags = HtmlUtil.RemoveHtmlTags(prev.Text, true);
            }
            string[] lines = p.Text.SplitToLines();
            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    if (!isPrevParagraphClosed)
                    {
                        continue;
                    }
                    if (preTextNoTags != null && IsEndWithAbbreviation(preTextNoTags))
                    {
                        continue;
                    }
                }
                if (i > 0 && !IsLineClosed(lines[i - 1]))
                {
                    continue;
                }
                string line = lines[i];
                var st = new StrippableText(line);
                string preText = st.Pre;
                if (preText.Contains("...", StringComparison.Ordinal) || preText.EndsWith('[') || preText.EndsWith('('))
                {
                    continue;
                }
                string strippedText = st.StrippedText;
                if (strippedText.Length == 0)
                {
                    continue;
                }
                if (IsUrl(strippedText))
                {
                    continue;
                }
                if (_isEnglish)
                {
                    if (IsLowerCaseLInPlaceofI(strippedText))
                    {
                        strippedText = strippedText.Remove(0, 1);
                        strippedText = strippedText.Insert(0, "I");
                    }
                }
                if (Helper.IsTurkishLittleI(strippedText[0], encoding, language))
                {
                    strippedText = strippedText.Remove(0, 1);
                    string turkishChar = Helper.GetTurkishUppercaseLetter(strippedText[0], encoding).ToString();
                    strippedText = strippedText.Insert(0, turkishChar);
                }
                strippedText = strippedText.CapitalizeFirstLetter();
                lines[i] = st.CombineWithPrePost(strippedText);
            }
            return string.Join(Environment.NewLine, lines);
        }

        private static bool IsLineClosed(string line)
        {
            line = HtmlUtil.RemoveHtmlTags(line, true);
            if (line.Length == 0)
            {
                return true;
            }
            char lastChar = line[line.Length - 1];
            if (lastChar == '!' || lastChar == '?' || lastChar == ')' || lastChar == ']')
            {
                return true;
            }
            if (lastChar != '.')
            {
                return false;
            }
            return !(IsEndWithAbbreviation(line) || line.EndsWith("...", StringComparison.Ordinal));
        }

        private static bool IsPreParagraphClose(Paragraph prev, Paragraph p)
        {
            if (prev == null)
            {
                return true;
            }
            if (IsLineClosed(prev.Text))
            {
                return true;
            }
            double timeGaps = Math.Abs(prev.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds);
            // if gaps is greater than 5 seconds then pre-paragraph is close.
            return timeGaps > 5 * TimeCode.BaseUnit; ;
        }

        private static bool IsUrl(string input)
        {
            // passionpairing.com. | www.passionpairing.com. | http://passionpairing.com. | https://passionpairing.com.
            input = HtmlUtil.RemoveHtmlTags(input, true).TrimEnd('.', '!', '?', ')', ']');
            return HtmlUtil.IsUrl(input);
        }

        private static bool IsEndWithAbbreviation(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            foreach (var abbreviation in GetKnownAbbreviations())
            {
                if (input.EndsWith(abbreviation, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsLowerCaseLInPlaceofI(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            if (input[0] != 'l')
            {
                return false;
            }
            foreach (string ocrError in GetEnglishOcrErrors())
            {
                if (input.StartsWith(ocrError, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        private static IEnumerable<string> GetEnglishOcrErrors()
        {
            yield return "lnterested";
            yield return "lsn't ";
            yield return "l'm  ";
            yield return "l am ";
            yield return "l-I";
            yield return "lm";
            yield return "lc ";
            yield return "l ";
            yield return "ls ";
            yield return "ln";
            yield return "ls ";
            yield return "ls";
            yield return "lt ";
            yield return "lf ";
        }

        private static IEnumerable<string> GetKnownAbbreviations()
        {
            yield return " o.r.";
            yield return " a.m.";
            yield return " p.m.";
        }

    }
}
