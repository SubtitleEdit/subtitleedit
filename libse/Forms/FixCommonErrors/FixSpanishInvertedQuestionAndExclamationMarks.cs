using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    /// <summary>
    /// Will try to fix issues with Spanish special letters ¿? and ¡!.
    /// Sentences ending with "?" must start with "¿".
    /// Sentences ending with "!" must start with "¡".
    /// </summary>
    public class FixSpanishInvertedQuestionAndExclamationMarks : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.FixSpanishInvertedQuestionAndExclamationMarks;
            int fixCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                Paragraph last = subtitle.GetParagraphOrDefault(i - 1);

                bool wasLastLineClosed = last == null || last.Text.EndsWith('?') || last.Text.EndsWith('!') || last.Text.EndsWith('.') ||
                                         last.Text.EndsWith(':') || last.Text.EndsWith(')') || last.Text.EndsWith(']');
                string trimmedStart = p.Text.TrimStart('-', ' ');
                if (last != null && last.Text.EndsWith("...", StringComparison.Ordinal) && trimmedStart.Length > 0 && char.IsLower(trimmedStart[0]))
                    wasLastLineClosed = false;
                if (!wasLastLineClosed && last.Text == last.Text.ToUpper())
                    wasLastLineClosed = true;

                string oldText = p.Text;

                FixSpanishInvertedLetter('?', "¿", p, last, ref wasLastLineClosed, fixAction, ref fixCount, callbacks);
                FixSpanishInvertedLetter('!', "¡", p, last, ref wasLastLineClosed, fixAction, ref fixCount, callbacks);

                if (p.Text != oldText)
                {
                    fixCount++;
                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }
            callbacks.UpdateFixStatus(fixCount, language.FixSpanishInvertedQuestionAndExclamationMarks, fixCount.ToString(CultureInfo.InvariantCulture));
        }

        private void FixSpanishInvertedLetter(char mark, string inverseMark, Paragraph p, Paragraph last, ref bool wasLastLineClosed, string fixAction, ref int fixCount, IFixCallbacks callbacks)
        {
            if (p.Text.Contains(mark))
            {
                bool skip = last != null && !p.Text.Contains(inverseMark) && last.Text.Contains(inverseMark) && !last.Text.Contains(mark);

                if (!skip && Utilities.CountTagInText(p.Text, mark) == Utilities.CountTagInText(p.Text, inverseMark) &&
                    HtmlUtil.RemoveHtmlTags(p.Text).TrimStart(inverseMark[0]).Contains(inverseMark) == false &&
                    HtmlUtil.RemoveHtmlTags(p.Text).TrimEnd(mark).Contains(mark) == false)
                {
                    skip = true;
                }

                if (!skip)
                {
                    int startIndex = 0;
                    int markIndex = p.Text.IndexOf(mark);
                    if (!wasLastLineClosed && ((p.Text.IndexOf('!') > 0 && p.Text.IndexOf('!') < markIndex) ||
                                               (p.Text.IndexOf('?') > 0 && p.Text.IndexOf('?') < markIndex) ||
                                               (p.Text.IndexOf('.') > 0 && p.Text.IndexOf('.') < markIndex)))
                        wasLastLineClosed = true;
                    while (markIndex > 0 && startIndex < p.Text.Length)
                    {
                        int inverseMarkIndex = p.Text.IndexOf(inverseMark, startIndex, StringComparison.Ordinal);
                        if (wasLastLineClosed && (inverseMarkIndex < 0 || inverseMarkIndex > markIndex))
                        {
                            if (callbacks.AllowFix(p, fixAction))
                            {
                                int j = markIndex - 1;

                                while (j > startIndex && (p.Text[j] == '.' || p.Text[j] == '!' || p.Text[j] == '?'))
                                    j--;

                                while (j > startIndex &&
                                       (p.Text[j] != '.' || IsSpanishAbbreviation(p.Text, j, callbacks)) &&
                                       p.Text[j] != '!' &&
                                       p.Text[j] != '?' &&
                                       !(j > 3 && p.Text.Substring(j - 3, 3) == Environment.NewLine + "-") &&
                                       !(j > 4 && p.Text.Substring(j - 4, 4) == Environment.NewLine + " -") &&
                                       !(j > 6 && p.Text.Substring(j - 6, 6) == Environment.NewLine + "<i>-"))
                                    j--;

                                if (@".!?".Contains(p.Text[j]))
                                {
                                    j++;
                                }
                                if (j + 3 < p.Text.Length && p.Text.Substring(j + 1, 2) == Environment.NewLine)
                                {
                                    j += 3;
                                }
                                else if (j + 2 < p.Text.Length && p.Text.Substring(j, 2) == Environment.NewLine)
                                {
                                    j += 2;
                                }
                                if (j >= startIndex)
                                {
                                    string part = p.Text.Substring(j, markIndex - j + 1);

                                    string speaker = string.Empty;
                                    int speakerEnd = part.IndexOf(')');
                                    if (part.StartsWith('(') && speakerEnd > 0 && speakerEnd < part.IndexOf(mark))
                                    {
                                        while (Environment.NewLine.Contains(part[speakerEnd + 1]))
                                            speakerEnd++;
                                        speaker = part.Substring(0, speakerEnd + 1);
                                        part = part.Substring(speakerEnd + 1);
                                    }
                                    speakerEnd = part.IndexOf(']');
                                    if (part.StartsWith('[') && speakerEnd > 0 && speakerEnd < part.IndexOf(mark))
                                    {
                                        while (Environment.NewLine.Contains(part[speakerEnd + 1]))
                                            speakerEnd++;
                                        speaker = part.Substring(0, speakerEnd + 1);
                                        part = part.Substring(speakerEnd + 1);
                                    }

                                    var st = new StrippableText(part);
                                    if (j == 0 && mark == '!' && st.Pre == "¿" && Utilities.CountTagInText(p.Text, mark) == 1 && HtmlUtil.RemoveHtmlTags(p.Text).EndsWith(mark))
                                    {
                                        p.Text = inverseMark + p.Text;
                                    }
                                    else if (j == 0 && mark == '?' && st.Pre == "¡" && Utilities.CountTagInText(p.Text, mark) == 1 && HtmlUtil.RemoveHtmlTags(p.Text).EndsWith(mark))
                                    {
                                        p.Text = inverseMark + p.Text;
                                    }
                                    else
                                    {
                                        string temp = inverseMark;
                                        int addToIndex = 0;
                                        while (p.Text.Length > markIndex + 1 && p.Text[markIndex + 1] == mark &&
                                            Utilities.CountTagInText(p.Text, mark) > Utilities.CountTagInText(p.Text + temp, inverseMark))
                                        {
                                            temp += inverseMark;
                                            st.Post += mark;
                                            markIndex++;
                                            addToIndex++;
                                        }

                                        p.Text = p.Text.Remove(j, markIndex - j + 1).Insert(j, speaker + st.Pre + temp + st.StrippedText + st.Post);
                                        markIndex += addToIndex;
                                    }
                                }
                            }
                        }
                        else if (last != null && !wasLastLineClosed && inverseMarkIndex == p.Text.IndexOf(mark) && !last.Text.Contains(inverseMark))
                        {
                            string lastOldtext = last.Text;
                            int idx = last.Text.Length - 2;
                            while (idx > 0 && (last.Text.Substring(idx, 2) != ". ") && (last.Text.Substring(idx, 2) != "! ") && (last.Text.Substring(idx, 2) != "? "))
                                idx--;

                            last.Text = last.Text.Insert(idx, inverseMark);
                            fixCount++;
                            callbacks.AddFixToListView(last, fixAction, lastOldtext, last.Text);
                        }

                        startIndex = markIndex + 2;
                        if (startIndex < p.Text.Length)
                            markIndex = p.Text.IndexOf(mark, startIndex);
                        else
                            markIndex = -1;
                        wasLastLineClosed = true;
                    }
                }
                if (p.Text.EndsWith(mark + "...", StringComparison.Ordinal) && p.Text.Length > 4)
                {
                    p.Text = p.Text.Remove(p.Text.Length - 4, 4) + "..." + mark;
                }
            }
            else if (Utilities.CountTagInText(p.Text, inverseMark) == 1)
            {
                int idx = p.Text.IndexOf(inverseMark, StringComparison.Ordinal);
                while (idx < p.Text.Length && !@".!?".Contains(p.Text[idx]))
                {
                    idx++;
                }
                if (idx < p.Text.Length)
                {
                    p.Text = p.Text.Insert(idx, mark.ToString(CultureInfo.InvariantCulture));
                    if (p.Text.Contains("¡¿") && p.Text.Contains("!?"))
                        p.Text = p.Text.Replace("!?", "?!");
                    if (p.Text.Contains("¿¡") && p.Text.Contains("?!"))
                        p.Text = p.Text.Replace("?!", "!?");
                }
            }
        }

        private bool IsSpanishAbbreviation(string text, int index, IFixCallbacks callbacks)
        {
            if (text[index] != '.')
                return false;

            if (index + 3 < text.Length && text[index + 2] == '.') //  X
                return true;                                    // O.R.

            if (index - 3 > 0 && text[index - 1] != '.' && text[index - 2] == '.') //    X
                return true;                          // O.R.

            string word = string.Empty;
            int i = index - 1;
            while (i >= 0 && char.IsLetter(text[i]))
            {
                word = text[i--] + word;
            }

            //Common Spanish abbreviations
            //Dr. (same as English)
            //Sr. (same as Mr.)
            //Sra. (same as Mrs.)
            //Ud.
            //Uds.
            if (word.Equals("dr", StringComparison.OrdinalIgnoreCase) ||
                word.Equals("sr", StringComparison.OrdinalIgnoreCase) ||
                word.Equals("sra", StringComparison.OrdinalIgnoreCase) ||
                word.Equals("ud", StringComparison.OrdinalIgnoreCase) ||
                word.Equals("uds", StringComparison.OrdinalIgnoreCase))
                return true;

            HashSet<string> abbreviations = callbacks.GetAbbreviations();
            return abbreviations.Contains(word + ".");
        }

    }
}
