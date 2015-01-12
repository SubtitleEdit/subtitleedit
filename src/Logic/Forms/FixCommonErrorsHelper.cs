using Nikse.SubtitleEdit.Core;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Forms
{
    public static class FixCommonErrorsHelper
    {
        public static string FixInvalidEllises(string text)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"\b\.{2}(?!\.)");
            int addOne = 0;
            foreach (System.Text.RegularExpressions.Match match in regex.Matches(text))
            {
                text = text.Insert((match.Index + 2) + addOne, ".");
                addOne++;
            }
            return text;
        }

        public static string FixEllipsesStartHelper(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Trim().Length < 4)
                return text;
            if (!text.Contains(".."))
                return text;

            if (text.StartsWith("...", StringComparison.Ordinal))
            {
                text = text.TrimStart('.').TrimStart();
            }

            text = text.Replace("-..", "- ..");
            var tag = "- ...";
            if (text.StartsWith(tag))
            {
                text = "- " + text.Substring(tag.Length);
                while (text.StartsWith("- .", StringComparison.Ordinal))
                {
                    text = "- " + text.Substring(3);
                    text = text.Replace("  ", " ");
                }
            }

            tag = "<i>...";
            if (text.StartsWith(tag))
            {
                text = "<i>" + text.Substring(tag.Length);
                while (text.StartsWith("<i>.", StringComparison.Ordinal) || text.StartsWith("<i> ", StringComparison.Ordinal))
                    text = "<i>" + text.Substring(4);
            }
            tag = "<i> ...";
            if (text.StartsWith(tag))
            {
                text = "<i>" + text.Substring(tag.Length);
                while (text.StartsWith("<i>.", StringComparison.Ordinal) || text.StartsWith("<i> ", StringComparison.Ordinal))
                    text = "<i>" + text.Substring(4, text.Length - 4);
            }

            tag = "- <i>...";
            if (text.StartsWith(tag))
            {
                text = "- <i>" + text.Substring(tag.Length);
                while (text.StartsWith("- <i>.", StringComparison.Ordinal))
                    text = "- <i>" + text.Substring(6);
            }
            tag = "- <i> ...";
            if (text.StartsWith(tag))
            {
                text = "- <i>" + text.Substring(tag.Length);
                while (text.StartsWith("- <i>.", StringComparison.Ordinal))
                    text = "- <i>" + text.Substring(6);
            }

            // Narrator:... Hello foo!
            text = text.Replace(":..", ": ..");
            tag = ": ..";
            if (text.Contains(tag))
            {
                text = text.Replace(": ..", ": ");
                while (text.Contains(": ."))
                    text = text.Replace(": .", ": ");
            }

            // <i>- ... Foo</i>
            tag = "<i>- ...";
            if (text.StartsWith(tag))
            {
                text = text.Substring(tag.Length);
                text = text.TrimStart('.', ' ');
                text = "<i>- " + text;
            }
            text = text.Replace("  ", " ");

            // WOMAN 2: <i>...24 hours a day at BabyC.</i>
            var index = text.IndexOf(':');
            if (index > 0 && text.Length > index + 2 && !char.IsDigit(text[index + 1]) && text.Contains(".."))
            {
                var post = text.Substring(0, index + 1);
                if (post.Length < 2)
                    return text;

                text = text.Remove(0, index + 1);
                text = text.Trim();
                text = FixEllipsesStartHelper(text);
                text = post + " " + text;
            }
            return text;
        }

        public static string FixDialogsOnOneLine(string text, string language)
        {
            if (text.Contains(" - ") && !text.Contains(Environment.NewLine))
            {
                string[] parts = text.Replace(" - ", Environment.NewLine).Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    string part0 = Utilities.RemoveHtmlTags(parts[0]).Trim();
                    string part1 = Utilities.RemoveHtmlTags(parts[1]).Trim();
                    if (part0.Length > 1 && "-—!?.\"".Contains(part0[part0.Length - 1]) &&
                        part1.Length > 1 && ("'" + Utilities.UppercaseLetters).Contains(part1[0]))
                    {
                        text = text.Replace(" - ", Environment.NewLine + "- ");
                        if (Utilities.AllLettersAndNumbers.Contains(part0[0]))
                        {
                            if (text.StartsWith("<i>", StringComparison.Ordinal))
                                text = "<i>- " + text;
                            else
                                text = "- " + text;
                        }
                    }
                }
            }

            if ((text.Contains(". -") || text.Contains("! -") || text.Contains("? -") || text.Contains("— -") || text.Contains("-- -")) && Utilities.CountTagInText(text, Environment.NewLine) == 1)
            {
                string temp = Utilities.AutoBreakLine(text, 99, 33, language);
                var arr = text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                var arrTemp = temp.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 2 && arrTemp.Length == 2 && !arr[1].TrimStart().StartsWith('-') && arrTemp[1].TrimStart().StartsWith('-'))
                    text = temp;
                else if (arr.Length == 2 && arrTemp.Length == 2 && !arr[1].TrimStart().StartsWith("<i>-", StringComparison.Ordinal) && arrTemp[1].TrimStart().StartsWith("<i>-", StringComparison.Ordinal))
                    text = temp;
            }
            else if ((text.Contains(". -") || text.Contains("! -") || text.Contains("? -") || text.Contains("-- -") || text.Contains("— -")) && !text.Contains(Environment.NewLine))
            {
                string temp = Utilities.AutoBreakLine(text, language);
                var arrTemp = temp.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                if (arrTemp.Length == 2)
                {
                    if (arrTemp[1].TrimStart().StartsWith('-') || arrTemp[1].TrimStart().StartsWith("<i>-", StringComparison.Ordinal))
                        text = temp;
                }
                else
                {
                    int index = text.IndexOf(". -", StringComparison.Ordinal);
                    if (index < 0)
                        index = text.IndexOf("! -", StringComparison.Ordinal);
                    if (index < 0)
                        index = text.IndexOf("? -", StringComparison.Ordinal);
                    if (index < 0)
                        index = text.IndexOf("— -", StringComparison.Ordinal);
                    if (index < 0 && text.IndexOf("-- -", StringComparison.Ordinal) > 0)
                        index = text.IndexOf("-- -", StringComparison.Ordinal) + 1;
                    if (index > 0)
                    {
                        text = text.Remove(index + 1, 1).Insert(index + 1, Environment.NewLine);
                        text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                        text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                    }
                }
            }
            return text;
        }

        public static bool IsPrevoiusTextEndOfParagraph(string prevText)
        {
            if (string.IsNullOrEmpty(prevText) || prevText.Length < 3)
                return true;

            prevText = prevText.Replace("♪", string.Empty).Replace("♫", string.Empty).Trim();
            bool isPrevEndOfLine = prevText.Length > 1 &&
                                   !prevText.EndsWith("...", StringComparison.Ordinal) &&
                                   (".!?—".Contains(prevText[prevText.Length - 1]) || // em dash, unicode character
                                    prevText.EndsWith("--", StringComparison.Ordinal));

            if (isPrevEndOfLine && prevText.Length > 5 && prevText.EndsWith('.') &&
                prevText[prevText.Length - 3] == '.' &&
                Utilities.AllLetters.Contains(prevText[prevText.Length - 2]))
                isPrevEndOfLine = false;
            return isPrevEndOfLine;
        }

        public static string FixHyphensRemove(Subtitle subtitle, int i)
        {
            Paragraph p = subtitle.Paragraphs[i];
            string text = p.Text;
            var textCache = Utilities.RemoveHtmlTags(text.TrimStart());
            if (textCache.StartsWith('-') || textCache.Contains(Environment.NewLine + "-"))
            {
                var prev = subtitle.GetParagraphOrDefault(i - 1);

                if (prev == null || !Utilities.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith('-') || Utilities.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith("--", StringComparison.Ordinal))
                {
                    var lines = Utilities.RemoveHtmlTags(p.Text).Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                    int startHyphenCount = lines.Count(line => line.TrimStart().StartsWith('-'));
                    if (startHyphenCount == 1)
                    {
                        var parts = Utilities.RemoveHtmlTags(text).Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        bool remove = true;
                        var part0 = parts[0].TrimEnd();
                        bool doRemove = "!?.".Contains(part0[part0.Length - 1]);
                        if (parts.Length == 2)
                        {
                            if (parts[0].TrimStart().StartsWith('-') && parts[1].Contains(": ") && !doRemove)
                                remove = false;
                            if (parts[1].TrimStart().StartsWith('-') && parts[0].Contains(": ") && !doRemove)
                                remove = false;
                        }

                        if (remove)
                        {
                            int idx = text.IndexOf('-');
                            var st = new StripableText(text);
                            if (idx < 5 && st.Pre.Length >= idx)
                            {
                                text = text.Remove(idx, 1).TrimStart();
                                idx = text.IndexOf('-');
                                st = new StripableText(text);
                                if (idx < 5 && idx >= 0 && st.Pre.Length >= idx)
                                {
                                    text = text.Remove(idx, 1).TrimStart();
                                    st = new StripableText(text);
                                }
                                idx = text.IndexOf('-');
                                if (idx < 5 && idx >= 0 && st.Pre.Length >= idx)
                                    text = text.Remove(idx, 1).TrimStart();

                                text = RemoveSpacesBeginLine(text);
                            }
                            else
                            {
                                int indexOfNewLine = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                                if (indexOfNewLine == -1)
                                    return text;
                                idx = text.IndexOf('-', indexOfNewLine);
                                if (idx > indexOfNewLine && indexOfNewLine + 5 < text.Length)
                                {
                                    text = text.Remove(idx, 1).TrimStart().Replace(Environment.NewLine + " ", Environment.NewLine);
                                    idx = text.IndexOf('-', indexOfNewLine);
                                    if (idx > indexOfNewLine && indexOfNewLine + 5 < text.Length)
                                    {
                                        text = text.Remove(idx, 1).TrimStart();
                                        text = RemoveSpacesBeginLine(text);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (text.StartsWith("<font ", StringComparison.Ordinal))
            {
                var prev = subtitle.GetParagraphOrDefault(i - 1);
                if (prev == null || !Utilities.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith('-') || Utilities.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith("--", StringComparison.Ordinal))
                {
                    var st = new StripableText(text);
                    if (st.Pre.EndsWith('-') || st.Pre.EndsWith("- ", StringComparison.Ordinal))
                    {
                        text = st.Pre.TrimEnd('-', ' ') + st.StrippedText + st.Post;
                    }
                }
            }
            return text;
        }

        private static string RemoveSpacesBeginLine(string text)
        {
            text = text.TrimStart();
            text = text.Replace("  ", " ");
            text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
            text = text.Replace(Environment.NewLine + "<i> ", Environment.NewLine + "<i>");
            text = text.Replace(Environment.NewLine + "<b> ", Environment.NewLine + "<b>");
            text = text.Replace(Environment.NewLine + "<u> ", Environment.NewLine + "<u>");
            if (text.StartsWith("<", StringComparison.OrdinalIgnoreCase) && text.Length > 3 && "ibu".Contains(text[1].ToString(), StringComparison.OrdinalIgnoreCase) && text[2] == '>')
                text = text.Remove(3, 1);

            // Todo: do the same for font tags...
            return text;
        }

        public static string FixHyphensAdd(Subtitle subtitle, int i, string language)
        {
            Paragraph p = subtitle.Paragraphs[i];
            string text = p.Text;
            var textCache = Utilities.RemoveHtmlTags(text.TrimStart());
            if (textCache.StartsWith('-') || textCache.Contains(Environment.NewLine + "-"))
            {
                Paragraph prev = subtitle.GetParagraphOrDefault(i - 1);

                if (prev == null || !Utilities.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith('-') || Utilities.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith("--", StringComparison.Ordinal))
                {
                    var lines = Utilities.RemoveHtmlTags(p.Text).Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                    int startHyphenCount = lines.Count(line => line.TrimStart().StartsWith('-'));
                    if (startHyphenCount == 1)
                    {
                        var parts = Utilities.RemoveHtmlTags(text).Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            var part0 = parts[0].TrimEnd();
                            bool doAdd = "!?.".Contains(part0[part0.Length - 1]) || language == "ko";
                            if (parts[0].TrimStart().StartsWith('-') && parts[1].Contains(':') && !doAdd)
                                doAdd = false;
                            if (parts[1].TrimStart().StartsWith('-') && parts[0].Contains(':') && !doAdd)
                                doAdd = false;

                            if (doAdd)
                            {
                                int idx = text.IndexOf('-');
                                int newLineIdx = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                                bool addSecondLine = idx < newLineIdx ? true : false;

                                if (addSecondLine && idx > 0 && Utilities.AllLetters.Contains(text[idx - 1]))
                                    addSecondLine = false;
                                if (addSecondLine)
                                {
                                    // add dash in second line.
                                    if (text.Contains(Environment.NewLine + "<i>"))
                                        text = text.Replace(Environment.NewLine + "<i>", Environment.NewLine + "<i>- ");
                                    else
                                        text = text.Replace(Environment.NewLine, Environment.NewLine + "- ").Replace(Environment.NewLine + "-  ", Environment.NewLine + "- ");
                                }
                                else
                                {
                                    // add dash in first line.
                                    if (text.StartsWith("<i>", StringComparison.Ordinal))
                                        text = "<i>- " + text.Remove(0, 3).Trim();
                                    else if (text.StartsWith("{\\an", StringComparison.Ordinal) && text.Length > 6 && text[5] == '}')
                                        text = text.Insert(6, "- ");
                                    else
                                        text = "- " + text.Trim();
                                }
                            }
                        }
                    }
                }
            }
            return text;
        }

    }
}