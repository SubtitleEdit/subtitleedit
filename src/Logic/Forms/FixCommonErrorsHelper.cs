﻿using Nikse.SubtitleEdit.Core;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Forms
{
    public static class FixCommonErrorsHelper
    {
        public static string FixEllipsesStartHelper(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Trim().Length < 4 || !(text.Contains("..", StringComparison.Ordinal) || text.Contains(". .", StringComparison.Ordinal)))
                return text;

            var pre = string.Empty;
            if (text.StartsWith("<font ", StringComparison.Ordinal) && text.IndexOf('>', 5) >= 0)
            {
                var idx = text.IndexOf('>', 5);
                if (idx >= 0)
                {
                    pre = text.Substring(0, text.IndexOf('>') + 1);
                    text = text.Substring(idx + 1).TrimStart();
                }
            }

            if (text.StartsWith("...", StringComparison.Ordinal))
            {
                text = text.TrimStart('.').TrimStart();
            }

            // "...foobar" / "... foobar" / ". .. foobar"
            if (text.StartsWith("\"") && (text.StartsWith("\"..") || text.StartsWith("\". .") || text.StartsWith("\" ..") || text.StartsWith("\" . .")))
            {
                int removeLength = 0;
                while (removeLength + 1 < text.Length && (text[1 + removeLength] == '.' || text[1 + removeLength] == ' '))
                    removeLength++;
                text = text.Remove(1, removeLength);
            }

            text = text.Replace("-..", "- ..");
            var tag = "- ...";
            if (text.StartsWith(tag, StringComparison.Ordinal))
            {
                text = "- " + text.Substring(tag.Length);
                while (text.StartsWith("- .", StringComparison.Ordinal))
                {
                    text = "- " + text.Substring(3);
                    text = text.Replace("  ", " ");
                }
            }

            tag = "<i>...";
            if (text.StartsWith(tag, StringComparison.Ordinal))
            {
                text = "<i>" + text.Substring(tag.Length);
                while (text.StartsWith("<i>.", StringComparison.Ordinal) || text.StartsWith("<i> ", StringComparison.Ordinal))
                    text = "<i>" + text.Substring(4);
            }
            tag = "<i> ...";
            if (text.StartsWith(tag, StringComparison.Ordinal))
            {
                text = "<i>" + text.Substring(tag.Length);
                while (text.StartsWith("<i>.", StringComparison.Ordinal) || text.StartsWith("<i> ", StringComparison.Ordinal))
                    text = "<i>" + text.Substring(4, text.Length - 4);
            }

            tag = "- <i>...";
            if (text.StartsWith(tag, StringComparison.Ordinal))
            {
                text = "- <i>" + text.Substring(tag.Length);
                while (text.StartsWith("- <i>.", StringComparison.Ordinal))
                    text = "- <i>" + text.Substring(6);
            }
            tag = "- <i> ...";
            if (text.StartsWith(tag, StringComparison.Ordinal))
            {
                text = "- <i>" + text.Substring(tag.Length);
                while (text.StartsWith("- <i>.", StringComparison.Ordinal))
                    text = "- <i>" + text.Substring(6);
            }

            // Narrator:... Hello foo!
            text = text.Replace(":..", ": ..");
            tag = ": ..";
            if (text.Contains(tag, StringComparison.Ordinal))
            {
                text = text.Replace(": ..", ": ");
                while (text.Contains(": ."))
                    text = text.Replace(": .", ": ");
            }

            // <i>- ... Foo</i>
            tag = "<i>- ...";
            if (text.StartsWith(tag, StringComparison.Ordinal))
            {
                text = text.Substring(tag.Length);
                text = text.TrimStart('.', ' ');
                text = "<i>- " + text;
            }
            text = text.Replace("  ", " ");

            // WOMAN 2: <i>...24 hours a day at BabyC.</i>
            var index = text.IndexOf(':');
            if (index > 0 && text.Length > index + 2 && !char.IsDigit(text[index + 1]) && text.Contains("..", StringComparison.Ordinal))
            {
                pre += text.Substring(0, index + 1);
                if (pre.Length < 2)
                    return text;

                text = text.Remove(0, index + 1).TrimStart();
                text = FixEllipsesStartHelper(text);
                if (pre.Length > 0)
                    pre += " ";
            }
            return pre + text;
        }

        public static string FixDialogsOnOneLine(string text, string language)
        {
            if (text.Contains(" - ") && !text.Contains(Environment.NewLine))
            {
                var noTagLines = HtmlUtil.RemoveHtmlTags(text.Replace(" - ", Environment.NewLine), true).SplitToLines();
                if (noTagLines.Length == 2)
                {
                    string part0 = noTagLines[0];
                    string part1 = noTagLines[1];
                    if (part0.Length > 1 && "-—!?.\")]".Contains(part0[part0.Length - 1]) &&
                        part1.Length > 1 && ("'" + Utilities.UppercaseLetters).Contains(part1[0]))
                    {
                        text = text.Replace(" - ", Environment.NewLine + "- ");
                        if (Utilities.AllLettersAndNumbers.Contains(part0[0]))
                        {
                            if (text.Length > 3 && text[0] == '<' && text[2] == '>')
                                text = "<" + text[1] + ">" + "- " + text.Substring(3).TrimStart();
                            else
                                text = "- " + text;
                        }
                    }
                }
            }

            var stringArray = new string[] { ". -", "! -", "? -", "— -", "-- -", ") -", "] -", "> -" };
            var idx = text.IndexOfAny(stringArray, StringComparison.Ordinal);
            if (idx >= 0)
            {
                if (Utilities.GetNumberOfLines(text) == 2)
                {
                    string temp = Utilities.AutoBreakLine(text, 99, 33, language);
                    var arr = text.SplitToLines();
                    var arrTemp = temp.SplitToLines();
                    if (arrTemp.Length == 2 && arr.Length == 2)
                    {
                        var secLine = HtmlUtil.RemoveHtmlTags(arr[1]).TrimStart();
                        var secLineTemp = HtmlUtil.RemoveHtmlTags(arrTemp[1]).TrimStart();
                        if (secLineTemp.StartsWith('-') && !secLine.StartsWith('-'))
                            text = temp;
                    }
                }
                else if (Utilities.GetNumberOfLines(text) == 1)
                {
                    string temp = Utilities.AutoBreakLine(text, language);
                    var arrTemp = temp.SplitToLines();
                    if (arrTemp.Length == 2)
                    {
                        var secLineTemp = HtmlUtil.RemoveHtmlTags(arrTemp[1]).TrimStart();
                        if (secLineTemp.StartsWith('-'))
                            text = temp;
                    }
                    else
                    {
                        int index = text.IndexOfAny(new[] { ". -", "! -", "? -", "— -" }, StringComparison.Ordinal);
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
            }
            return text;
        }

        public static bool IsPreviousTextEndOfParagraph(string prevText)
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

            if (text.TrimStart().StartsWith('-') ||
                text.TrimStart().StartsWith("<i>-", StringComparison.OrdinalIgnoreCase) ||
                text.TrimStart().StartsWith("<i> -", StringComparison.OrdinalIgnoreCase) ||
                text.Contains(Environment.NewLine + '-') ||
                text.Contains(Environment.NewLine + " -") ||
                text.Contains(Environment.NewLine + "<i>-") ||
                text.Contains(Environment.NewLine + "<i> -") ||
                text.Contains(Environment.NewLine + "<I>-") ||
                text.Contains(Environment.NewLine + "<I> -"))
            {
                var prev = subtitle.GetParagraphOrDefault(i - 1);

                if (prev == null || !HtmlUtil.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith('-') || HtmlUtil.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith("--", StringComparison.Ordinal))
                {
                    var noTaglines = HtmlUtil.RemoveHtmlTags(p.Text).SplitToLines();
                    int startHyphenCount = noTaglines.Count(line => line.TrimStart().StartsWith('-'));
                    if (startHyphenCount == 1)
                    {
                        bool remove = true;
                        var noTagparts = HtmlUtil.RemoveHtmlTags(text).SplitToLines();
                        if (noTagparts.Length == 2)
                        {
                            if (noTagparts[0].TrimStart().StartsWith('-') && noTagparts[1].Contains(": "))
                                remove = false;
                            if (noTagparts[1].TrimStart().StartsWith('-') && noTagparts[0].Contains(": "))
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
                                if (indexOfNewLine > 0)
                                {
                                    idx = text.IndexOf('-', indexOfNewLine);
                                    if (idx >= 0 && indexOfNewLine + 5 > indexOfNewLine)
                                    {
                                        text = text.Remove(idx, 1).TrimStart().Replace(Environment.NewLine + " ", Environment.NewLine);

                                        idx = text.IndexOf('-', indexOfNewLine);
                                        if (idx >= 0 && indexOfNewLine + 5 > indexOfNewLine)
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
            }
            else if (text.StartsWith("<font ", StringComparison.Ordinal))
            {
                var prev = subtitle.GetParagraphOrDefault(i - 1);
                if (prev == null || !HtmlUtil.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith('-') || HtmlUtil.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith("--", StringComparison.Ordinal))
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

            if (text.LineStartsWithHtmlTag(true) && text[3] == 0x20)
            {
                text = text.Remove(3, 1).TrimStart();
            }
            if (text.LineStartsWithHtmlTag(false, true))
            {
                var closeIdx = text.IndexOf('>');
                if (closeIdx > 6 && text[closeIdx + 1] == 0x20)
                    text = text.Remove(closeIdx + 1, 1);
            }
            return text;
        }

        public static string RemoveSpacesBeginLineAfterEllipses(string line)
        {
            if (line.StartsWith("... ", StringComparison.Ordinal))
                line = line.Remove(3, 1);
            if (line.Length > 6 && line.LineStartsWithHtmlTag(true)) // <i>... foobar
            {
                var idx = line.IndexOf('>') + 1;
                var pre = line.Substring(0, idx);
                line = line.Remove(0, idx).TrimStart();
                if (line.StartsWith("... ", StringComparison.Ordinal))
                    line = line.Remove(3, 1);
                line = pre + line;
            }
            if (line.LineStartsWithHtmlTag(false, true)) //<font color="#000000"> and <font>
            {
                var closeIdx = line.IndexOf('>', 5);
                if (closeIdx >= 5 && line.Length > closeIdx + 5)
                {
                    var fontTag = line.Substring(0, closeIdx + 1).TrimStart();
                    line = line.Substring(closeIdx + 1).TrimStart();
                    if (line.StartsWith("... ", StringComparison.Ordinal))
                        line = line.Remove("... ".Length - 1, 1);
                    line = fontTag + line;
                }
            }
            return line;
        }

        public static string FixHyphensAdd(Subtitle subtitle, int i, string language)
        {
            Paragraph p = subtitle.Paragraphs[i];
            string text = p.Text;
            var textCache = HtmlUtil.RemoveHtmlTags(text.TrimStart());
            if (textCache.StartsWith('-') || textCache.Contains(Environment.NewLine + "-"))
            {
                Paragraph prev = subtitle.GetParagraphOrDefault(i - 1);

                if (prev == null || !HtmlUtil.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith('-') || HtmlUtil.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith("--", StringComparison.Ordinal))
                {
                    var lines = HtmlUtil.RemoveHtmlTags(p.Text).SplitToLines();
                    int startHyphenCount = lines.Count(line => line.TrimStart().StartsWith('-'));
                    int totalSpaceHyphen = Utilities.CountTagInText(text, " -");
                    if (startHyphenCount == 1 && totalSpaceHyphen == 0)
                    {
                        var parts = HtmlUtil.RemoveHtmlTags(text).SplitToLines();
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
                                    newLineIdx += 2;
                                    if (text.LineBreakStartsWithHtmlTag(true))
                                    {
                                        text = text.Insert(newLineIdx + 3, "- ").TrimEnd();
                                    }
                                    else
                                    {
                                        text = text.Replace(Environment.NewLine, Environment.NewLine + "- ").Replace(Environment.NewLine + "-  ", Environment.NewLine + "- ");
                                    }
                                }
                                else
                                {
                                    // add dash in first line.
                                    if (text.LineStartsWithHtmlTag(true))
                                        text = text.Substring(0, 3) + "- " + text.Remove(0, 3).TrimEnd();
                                    else if (text.StartsWith("{\\an", StringComparison.Ordinal) && text.Length > 6 && text[5] == '}')
                                        text = text.Insert(6, "- ");
                                    else
                                        text = "- " + text.Trim();
                                }
                            }
                        }
                    }
                    // - Shut it off. -Get the fuck<br/>out of here, Darryl.
                    if (totalSpaceHyphen == 1 && startHyphenCount == 1)
                    {
                        var idx = text.IndexOf(" -", StringComparison.Ordinal);
                        if (idx > 1 && ".?!".Contains(text[idx - 1]) && idx + 2 < text.Length)
                        {
                            var firstLine = text.Substring(0, idx).Replace(Environment.NewLine, " ").Trim();
                            var secondLine = text.Substring(idx + 1).Insert(1, " ").Replace(Environment.NewLine, " ").Trim();
                            text = firstLine + Environment.NewLine + secondLine;
                        }
                    }
                }
            }
            return text;
        }

        public static string FixDoubleGreaterThanHelper(string text)
        {
            string post = string.Empty;
            if (text.Length > 3 && text[0] == '<' && text[2] == '>' && (text[1] == 'i' || text[1] == 'b' || text[1] == 'u'))
            {
                post += "<" + text[1] + ">";
                text = text.Remove(0, 3).TrimStart();
            }
            if (text.StartsWith("<font", StringComparison.OrdinalIgnoreCase))
            {
                var endIdx = text.IndexOf('>', 5);
                if (endIdx >= 5 && endIdx < text.Length - 7)
                {
                    post += text.Substring(0, endIdx + 1);
                    text = text.Substring(endIdx + 1).TrimStart();
                }
            }
            if (text.StartsWith(">>", StringComparison.Ordinal) && text.Length > 3)
                text = text.TrimStart('>', ' ').TrimStart();
            return post + text;
        }

        internal static string FixShortLines(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || !text.Contains(Environment.NewLine, StringComparison.Ordinal))
                return text;

            string s = HtmlUtil.RemoveHtmlTags(text, true);
            if (s.Contains(Environment.NewLine) && s.Replace(Environment.NewLine, " ").Replace("  ", " ").Length < Configuration.Settings.Tools.MergeLinesShorterThan)
            {
                s = s.TrimEnd().TrimEnd('.', '?', '!', ':', ';');
                s = s.TrimStart('-');
                if (!s.Contains(new[] { '.', '?', '!', ':', ';', '-', '♪', '♫' }) &&
                    !(s.StartsWith('[') && s.Contains("]" + Environment.NewLine, StringComparison.Ordinal)) &&
                    !(s.StartsWith('(') && s.Contains(")" + Environment.NewLine, StringComparison.Ordinal)) &&
                    s != s.ToUpper())
                {
                    return text.Replace(Environment.NewLine, " ").Replace("  ", " ");
                }
            }
            return text;
        }

    }
}