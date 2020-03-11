using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixMissingPeriodsAtEndOfLine : IFixCommonError
    {
        private static readonly char[] WordSplitChars = { ' ', '.', ',', '-', '?', '!', ':', ';', '"', '(', ')', '[', ']', '{', '}', '|', '<', '>', '/', '+', '\r', '\n' };

        private static bool IsOneLineUrl(string s)
        {
            if (s.Contains(' ') || s.Contains(Environment.NewLine))
            {
                return false;
            }

            if (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (s.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string[] parts = s.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3 && parts[2].Length > 1 && parts[2].Length < 7)
            {
                return true;
            }

            return false;
        }

        // Cached variables
        private static readonly char[] ExpectedChars = { '♪', '♫' };
        private const string ExpectedString1 = ",.!?:;>-])♪♫…";
        private const string ExpectedString2 = ")]*#¶.!?";

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.FixMissingPeriodAtEndOfLine;
            int missingPeriodsAtEndOfLine = 0;

            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var next = subtitle.GetParagraphOrDefault(i + 1);
                string nextText = string.Empty;
                if (next != null)
                {
                    nextText = HtmlUtil.RemoveHtmlTags(next.Text, true).TrimStart('-', '"', '„').TrimStart();
                }
                bool isNextClose = next != null && next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds < 400;
                string tempNoHtml = HtmlUtil.RemoveHtmlTags(p.Text).TrimEnd();

                if (IsOneLineUrl(p.Text) || p.Text.Contains(ExpectedChars) || p.Text.EndsWith('\''))
                {
                    // ignore urls
                }
                else if (!string.IsNullOrEmpty(nextText) && next != null &&
                    next.Text.Length > 0 &&
                    char.IsUpper(nextText[0]) &&
                    tempNoHtml.Length > 0 &&
                    !ExpectedString1.Contains(tempNoHtml[tempNoHtml.Length - 1]))
                {
                    string tempTrimmed = tempNoHtml.TrimEnd().TrimEnd('\'', '"', '“', '”').TrimEnd();
                    if (tempTrimmed.Length > 0 && !ExpectedString2.Contains(tempTrimmed[tempTrimmed.Length - 1]) && p.Text != p.Text.ToUpperInvariant())
                    {
                        //don't end the sentence if the next word is an I word as they're always capped.
                        bool isNextCloseAndStartsWithI = isNextClose && (nextText.StartsWith("I ", StringComparison.Ordinal) ||
                                                                         nextText.StartsWith("I'", StringComparison.Ordinal));

                        if (!isNextCloseAndStartsWithI)
                        {
                            //test to see if the first word of the next line is a name
                            if (!callbacks.IsName(next.Text.Split(WordSplitChars)[0]) && callbacks.AllowFix(p, fixAction))
                            {
                                string oldText = p.Text;
                                if (p.Text.EndsWith('>'))
                                {
                                    int lastLessThan = p.Text.LastIndexOf('<');
                                    if (lastLessThan > 0)
                                    {
                                        p.Text = p.Text.Insert(lastLessThan, ".");
                                    }
                                }
                                else
                                {
                                    if (p.Text.EndsWith('“') && tempNoHtml.StartsWith('„'))
                                    {
                                        p.Text = p.Text.TrimEnd('“') + ".“";
                                    }
                                    else if (p.Text.EndsWith('"') && tempNoHtml.StartsWith('"'))
                                    {
                                        p.Text = p.Text.TrimEnd('"') + ".\"";
                                    }
                                    else
                                    {
                                        var lines = p.Text.SplitToLines();
                                        if (!IsOneLineUrl(lines.Last()))
                                        {
                                            p.Text += ".";
                                        }
                                    }
                                }
                                if (p.Text != oldText)
                                {
                                    missingPeriodsAtEndOfLine++;
                                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                                }
                            }
                        }
                    }
                }
                else if (next != null && !string.IsNullOrEmpty(p.Text) && Utilities.AllLettersAndNumbers.Contains(p.Text[p.Text.Length - 1]))
                {
                    if (p.Text != p.Text.ToUpperInvariant())
                    {
                        var st = new StrippableText(next.Text);
                        if (st.StrippedText.Length > 0 && st.StrippedText != st.StrippedText.ToUpperInvariant() &&
                            char.IsUpper(st.StrippedText[0]))
                        {
                            if (callbacks.AllowFix(p, fixAction))
                            {
                                int j = p.Text.Length - 1;
                                while (j >= 0 && !@".!?¿¡".Contains(p.Text[j]))
                                {
                                    j--;
                                }

                                string endSign = ".";
                                if (j >= 0 && p.Text[j] == '¿')
                                {
                                    endSign = "?";
                                }

                                if (j >= 0 && p.Text[j] == '¡')
                                {
                                    endSign = "!";
                                }

                                string oldText = p.Text;
                                missingPeriodsAtEndOfLine++;
                                p.Text += endSign;
                                callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                            }
                        }
                    }
                }

                if (p.Text.Length > 4)
                {
                    int indexOfNewLine = p.Text.IndexOf(Environment.NewLine + " -", 3, StringComparison.Ordinal);
                    if (indexOfNewLine < 0)
                    {
                        indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "-", 3, StringComparison.Ordinal);
                    }

                    if (indexOfNewLine < 0)
                    {
                        indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "<i>-", 3, StringComparison.Ordinal);
                    }

                    if (indexOfNewLine < 0)
                    {
                        indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "<i> -", 3, StringComparison.Ordinal);
                    }

                    if (indexOfNewLine > 0 && char.IsUpper(char.ToUpper(p.Text[indexOfNewLine - 1])) && callbacks.AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;

                        string text = p.Text.Substring(0, indexOfNewLine);
                        var st = new StrippableText(text);
                        if (st.Pre.TrimEnd().EndsWith('¿')) // Spanish ¿
                        {
                            p.Text = p.Text.Insert(indexOfNewLine, "?");
                        }
                        else if (st.Pre.TrimEnd().EndsWith('¡')) // Spanish ¡
                        {
                            p.Text = p.Text.Insert(indexOfNewLine, "!");
                        }
                        else
                        {
                            p.Text = p.Text.Insert(indexOfNewLine, ".");
                        }

                        missingPeriodsAtEndOfLine++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(missingPeriodsAtEndOfLine, language.AddPeriods, language.XPeriodsAdded);
        }

    }
}
