﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixMissingPeriodsAtEndOfLine : IFixCommonError
    {
        public static class Language
        {
            public static string FixMissingPeriodAtEndOfLine { get; set; } = "Add missing period at end of line";
            public static string AddPeriods { get; set; } = "Add missing period at end of line";
        }

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

            var parts = s.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3 && parts[2].Length > 1 && parts[2].Length < 7)
            {
                return true;
            }

            return false;
        }

        // Cached variables
        private static readonly char[] ExpectedChars = { '♪', '♫' };
        private const string DoNotAddPeriodAfterChars = ",.!?:;>-])♪♫…、。";
        private const string ExpectedString2 = ")]*#¶.!?";

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var fixAction = Language.FixMissingPeriodAtEndOfLine;
            var missingPeriodsAtEndOfLine = 0;

            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var next = subtitle.GetParagraphOrDefault(i + 1);
                var nextText = string.Empty;
                if (next != null)
                {
                    nextText = HtmlUtil.RemoveHtmlTags(next.Text, true).TrimStart('-', '"', '„').TrimStart();
                }
                var isNextClose = next != null && next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds < 400;
                var tempNoHtml = HtmlUtil.RemoveHtmlTags(p.Text, true)
                    .Replace("\u200B", string.Empty) // Zero Width Space
                    .Replace("\uFEFF", string.Empty) // Zero Width No-Break Space
                    .TrimEnd();

                if (IsOneLineUrl(p.Text) || p.Text.Contains(ExpectedChars) || p.Text.EndsWith('\''))
                {
                    // ignore urls
                }
                else if (!string.IsNullOrEmpty(nextText) && next != null &&
                    next.Text.Length > 0 &&
                    char.IsUpper(nextText[0]) &&
                    tempNoHtml.Length > 0 &&
                    !DoNotAddPeriodAfterChars.Contains(tempNoHtml[tempNoHtml.Length - 1]))
                {
                    var tempTrimmed = tempNoHtml.TrimEnd().TrimEnd('\'', '"', '“', '”').TrimEnd();
                    if (tempTrimmed.Length > 0 && !ExpectedString2.Contains(tempTrimmed[tempTrimmed.Length - 1]) && p.Text != p.Text.ToUpperInvariant())
                    {
                        //don't end the sentence if the next word is an I word as they're always capped.
                        var isNextCloseAndStartsWithI = isNextClose && (nextText.StartsWith("I ", StringComparison.Ordinal) ||
                                                                        nextText.StartsWith("I'", StringComparison.Ordinal));

                        var isNextCloseAndStartsWithTitle = isNextClose && (nextText.StartsWith("Mr. ", StringComparison.Ordinal) ||
                                                                            nextText.StartsWith("Dr. ", StringComparison.Ordinal));

                        if (!isNextCloseAndStartsWithI && !isNextCloseAndStartsWithTitle)
                        {
                            //test to see if the first word of the next line is a name
                            if (callbacks.AllowFix(p, fixAction))
                            {
                                string oldText = p.Text;
                                if (callbacks.IsName(next.Text.Split(WordSplitChars)[0]))
                                {
                                    if (next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds > 2000)
                                    {
                                        AddPeriod(p, tempNoHtml);
                                    }
                                }
                                else
                                {
                                    AddPeriod(p, tempNoHtml);
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
                    var indexOfNewLine = p.Text.IndexOf(Environment.NewLine + " -", 3, StringComparison.Ordinal);
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
                        var oldText = p.Text;

                        var text = p.Text.Substring(0, indexOfNewLine);
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

            callbacks.UpdateFixStatus(missingPeriodsAtEndOfLine, Language.AddPeriods);
        }

        private static void AddPeriod(Paragraph p, string tempNoHtml)
        {
            if (p.Text.EndsWith('>'))
            {
                var lastLessThan = p.Text.LastIndexOf('<');
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
        }
    }
}
