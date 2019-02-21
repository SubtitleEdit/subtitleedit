using System;
using System.Globalization;
using System.Text;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixStartWithUppercaseLetterAfterParagraph : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
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
            if (p.Text != null && p.Text.Length > 1)
            {
                string text = p.Text;
                string pre = string.Empty;
                if (text.Length > 4 && text.StartsWith("<i> ", StringComparison.Ordinal))
                {
                    pre = "<i> ";
                    text = text.Substring(4);
                }
                if (text.Length > 3 && text.StartsWith("<i>", StringComparison.Ordinal))
                {
                    pre = "<i>";
                    text = text.Substring(3);
                }
                if (text.Length > 4 && text.StartsWith("<I> ", StringComparison.Ordinal))
                {
                    pre = "<I> ";
                    text = text.Substring(4);
                }
                if (text.Length > 3 && text.StartsWith("<I>", StringComparison.Ordinal))
                {
                    pre = "<I>";
                    text = text.Substring(3);
                }
                if (text.Length > 2 && text.StartsWith('♪'))
                {
                    pre = pre + "♪";
                    text = text.Substring(1);
                }
                if (text.Length > 2 && text.StartsWith(' '))
                {
                    pre = pre + " ";
                    text = text.Substring(1);
                }
                if (text.Length > 2 && text.StartsWith('♫'))
                {
                    pre = pre + "♫";
                    text = text.Substring(1);
                }
                if (text.Length > 2 && text.StartsWith(' '))
                {
                    pre = pre + " ";
                    text = text.Substring(1);
                }

                var firstLetter = text[0];

                string prevText = " .";
                if (prev != null)
                {
                    prevText = HtmlUtil.RemoveHtmlTags(prev.Text);
                }

                bool isPrevEndOfLine = Helper.IsPreviousTextEndOfParagraph(prevText);
                if (prevText == " .")
                {
                    isPrevEndOfLine = true;
                }

                if ((!text.StartsWith("www.", StringComparison.Ordinal) && !text.StartsWith("http:", StringComparison.Ordinal) && !text.StartsWith("https:", StringComparison.Ordinal)) &&
                    (char.IsLower(firstLetter) || Helper.IsTurkishLittleI(firstLetter, encoding, language)) &&
                    !char.IsDigit(firstLetter) &&
                    isPrevEndOfLine)
                {
                    bool isMatchInKnowAbbreviations = language == "en" &&
                        (prevText.EndsWith(" o.r.", StringComparison.Ordinal) ||
                         prevText.EndsWith(" a.m.", StringComparison.Ordinal) ||
                         prevText.EndsWith(" p.m.", StringComparison.Ordinal));

                    if (!isMatchInKnowAbbreviations)
                    {
                        if (Helper.IsTurkishLittleI(firstLetter, encoding, language))
                        {
                            p.Text = pre + Helper.GetTurkishUppercaseLetter(firstLetter, encoding) + text.Substring(1);
                        }
                        else if (IsEnglishCandidateForLowercaseLtoUppercaseI(language, text)) // l > I
                        {
                            p.Text = pre + "I" + text.Substring(1);
                        }
                        else
                        {
                            p.Text = pre + char.ToUpper(firstLetter) + text.Substring(1);
                        }
                    }
                }
            }

            if (p.Text != null && p.Text.Contains(Environment.NewLine))
            {
                var arr = p.Text.SplitToLines();
                if (arr.Count == 2 && arr[1].Length > 1)
                {
                    string text = arr[1];
                    string pre = string.Empty;
                    if (text.Length > 4 && text.StartsWith("<i> ", StringComparison.Ordinal))
                    {
                        pre = "<i> ";
                        text = text.Substring(4);
                    }
                    if (text.Length > 3 && text.StartsWith("<i>", StringComparison.Ordinal))
                    {
                        pre = "<i>";
                        text = text.Substring(3);
                    }
                    if (text.Length > 4 && text.StartsWith("<I> ", StringComparison.Ordinal))
                    {
                        pre = "<I> ";
                        text = text.Substring(4);
                    }
                    if (text.Length > 3 && text.StartsWith("<I>", StringComparison.Ordinal))
                    {
                        pre = "<I>";
                        text = text.Substring(3);
                    }
                    if (text.Length > 2 && text.StartsWith('♪'))
                    {
                        pre = pre + "♪";
                        text = text.Substring(1);
                    }
                    if (text.Length > 2 && text.StartsWith(' '))
                    {
                        pre = pre + " ";
                        text = text.Substring(1);
                    }
                    if (text.Length > 2 && text.StartsWith('♫'))
                    {
                        pre = pre + "♫";
                        text = text.Substring(1);
                    }
                    if (text.Length > 2 && text.StartsWith(' '))
                    {
                        pre = pre + " ";
                        text = text.Substring(1);
                    }

                    char firstLetter = text[0];
                    string prevText = HtmlUtil.RemoveHtmlTags(arr[0]);
                    bool isPrevEndOfLine = Helper.IsPreviousTextEndOfParagraph(prevText);
                    if ((!text.StartsWith("www.", StringComparison.Ordinal) && !text.StartsWith("http:", StringComparison.Ordinal) && !text.StartsWith("https:", StringComparison.Ordinal)) &&
                        (char.IsLower(firstLetter) || Helper.IsTurkishLittleI(firstLetter, encoding, language)) &&
                        !prevText.EndsWith("...", StringComparison.Ordinal) &&
                        isPrevEndOfLine)
                    {
                        bool isMatchInKnowAbbreviations = language == "en" &&
                            (prevText.EndsWith(" o.r.", StringComparison.Ordinal) ||
                             prevText.EndsWith(" a.m.", StringComparison.Ordinal) ||
                             prevText.EndsWith(" p.m.", StringComparison.Ordinal));

                        if (!isMatchInKnowAbbreviations)
                        {
                            if (Helper.IsTurkishLittleI(firstLetter, encoding, language))
                            {
                                text = pre + Helper.GetTurkishUppercaseLetter(firstLetter, encoding) + text.Substring(1);
                            }
                            else if (IsEnglishCandidateForLowercaseLtoUppercaseI(language, text)) // l > I
                            {
                                text = pre + "I" + text.Substring(1);
                            }
                            else
                            {
                                text = pre + char.ToUpper(firstLetter) + text.Substring(1);
                            }

                            p.Text = arr[0] + Environment.NewLine + text;
                        }
                    }

                    arr = p.Text.SplitToLines();
                    if ((arr[0].StartsWith('-') || arr[0].StartsWith("<i>-", StringComparison.Ordinal)) &&
                        (arr[1].StartsWith('-') || arr[1].StartsWith("<i>-", StringComparison.Ordinal)) &&
                        !arr[0].StartsWith("--", StringComparison.Ordinal) && !arr[0].StartsWith("<i>--", StringComparison.Ordinal) &&
                        !arr[1].StartsWith("--", StringComparison.Ordinal) && !arr[1].StartsWith("<i>--", StringComparison.Ordinal))
                    {
                        if (isPrevEndOfLine && arr[1].StartsWith("<i>- ", StringComparison.Ordinal) && arr[1].Length > 6)
                        {
                            p.Text = arr[0] + Environment.NewLine + "<i>- " + char.ToUpper(arr[1][5]) + arr[1].Remove(0, 6);
                        }
                        else if (isPrevEndOfLine && arr[1].StartsWith("- ", StringComparison.Ordinal) && arr[1].Length > 3)
                        {
                            p.Text = arr[0] + Environment.NewLine + "- " + char.ToUpper(arr[1][2]) + arr[1].Remove(0, 3);
                        }
                        arr = p.Text.SplitToLines();

                        prevText = " .";
                        if (prev != null && p.StartTime.TotalMilliseconds - 10000 < prev.EndTime.TotalMilliseconds)
                        {
                            prevText = HtmlUtil.RemoveHtmlTags(prev.Text);
                        }

                        bool isPrevLineEndOfLine = Helper.IsPreviousTextEndOfParagraph(prevText);
                        if (isPrevLineEndOfLine && arr[0].StartsWith("<i>- ", StringComparison.Ordinal) && arr[0].Length > 6)
                        {
                            p.Text = "<i>- " + char.ToUpper(arr[0][5]) + arr[0].Remove(0, 6) + Environment.NewLine + arr[1];
                        }
                        else if (isPrevLineEndOfLine && arr[0].StartsWith("- ", StringComparison.Ordinal) && arr[0].Length > 3)
                        {
                            p.Text = "- " + char.ToUpper(arr[0][2]) + arr[0].Remove(0, 3) + Environment.NewLine + arr[1];
                        }
                    }
                    else if (arr[0].Length > 1 && arr[1].Length > 2 &&
                            ".!?".Contains(arr[0].Substring(arr[0].Length - 1, 1)) &&
                            arr[1].StartsWith("- ", StringComparison.Ordinal))
                    {
                        p.Text = arr[0] + Environment.NewLine + "- " + char.ToUpper(arr[1][2]) + arr[1].Remove(0, 3);
                    }
                    else if (arr[0].Length > 1 && arr[1].Length > 2 &&
                                               ".!?".Contains(arr[0].Substring(arr[0].Length - 1, 1)) &&
                                               arr[1].StartsWith("<i>- ", StringComparison.Ordinal))
                    {
                        p.Text = arr[0] + Environment.NewLine + "<i>- " + char.ToUpper(arr[1][5]) + arr[1].Remove(0, 6);
                    }
                }
            }

            if (p.Text != null && p.Text.Length > 4)
            {
                int len = 0;
                int indexOfNewLine = p.Text.IndexOf(Environment.NewLine + " -", 1, StringComparison.Ordinal);
                if (indexOfNewLine < 0)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "- <i> ♪", 1, StringComparison.Ordinal);
                    len = "- <i> ♪".Length;
                }
                if (indexOfNewLine < 0)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "-", 1, StringComparison.Ordinal);
                    len = "-".Length;
                }
                if (indexOfNewLine < 0)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "<i>-", 1, StringComparison.Ordinal);
                    len = "<i>-".Length;
                }
                if (indexOfNewLine < 0)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "<i> -", 1, StringComparison.Ordinal);
                    len = "<i> -".Length;
                }
                if (indexOfNewLine < 0)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "♪ -", 1, StringComparison.Ordinal);
                    len = "♪ -".Length;
                }
                if (indexOfNewLine < 0)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "♪ <i> -", 1, StringComparison.Ordinal);
                    len = "♪ <i> -".Length;
                }
                if (indexOfNewLine < 0)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "♪ <i>-", 1, StringComparison.Ordinal);
                    len = "♪ <i>-".Length;
                }

                if (indexOfNewLine > 0)
                {
                    string text = p.Text.Substring(indexOfNewLine + len);
                    var st = new StrippableText(text);

                    if (st.StrippedText.Length > 0 && Helper.IsTurkishLittleI(st.StrippedText[0], encoding, language) && !st.Pre.EndsWith('[') && !st.Pre.Contains("..."))
                    {
                        text = st.Pre + Helper.GetTurkishUppercaseLetter(st.StrippedText[0], encoding) + st.StrippedText.Substring(1) + st.Post;
                        p.Text = p.Text.Remove(indexOfNewLine + len).Insert(indexOfNewLine + len, text);
                    }
                    else if (st.StrippedText.Length > 0 && st.StrippedText[0] != char.ToUpper(st.StrippedText[0]) && !st.Pre.EndsWith('[') && !st.Pre.Contains("..."))
                    {
                        text = st.Pre + char.ToUpper(st.StrippedText[0]) + st.StrippedText.Substring(1) + st.Post;
                        p.Text = p.Text.Remove(indexOfNewLine + len).Insert(indexOfNewLine + len, text);
                    }
                }
            }
            return p.Text;
        }

        private static bool IsEnglishCandidateForLowercaseLtoUppercaseI(string language, string text)
        {
            return language == "en" && (text.StartsWith("l ", StringComparison.Ordinal) || text.StartsWith("l-I", StringComparison.Ordinal) ||
                                        text.StartsWith("ls") || text.StartsWith("ldiot", StringComparison.Ordinal) || text.StartsWith("ln", StringComparison.Ordinal) ||
                                        text.StartsWith("lm", StringComparison.Ordinal) || text.StartsWith("lt", StringComparison.Ordinal) ||
                                        text.StartsWith("lf ", StringComparison.Ordinal) || text.StartsWith("lc", StringComparison.Ordinal) ||
                                        text.StartsWith("l'm ", StringComparison.Ordinal)) || text.StartsWith("l've ", StringComparison.Ordinal);
        }
    }
}
