using System;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public static class Helper
    {
        public static bool IsTurkishLittleI(char firstLetter, Encoding encoding, string language)
        {
            if (language != "tr")
            {
                return false;
            }

            return encoding.Equals(Encoding.UTF8)
                ? firstLetter == 'ı' || firstLetter == 'i'
                : firstLetter == 'ý' || firstLetter == 'i';
        }

        public static char GetTurkishUppercaseLetter(char letter, Encoding encoding)
        {
            if (encoding.Equals(Encoding.UTF8))
            {
                if (letter == 'ı')
                {
                    return 'I';
                }

                if (letter == 'i')
                {
                    return 'İ';
                }
            }
            else
            {
                if (letter == 'i')
                {
                    return 'Ý';
                }

                if (letter == 'ý')
                {
                    return 'I';
                }
            }
            return letter;
        }

        public static string FixEllipsesStartHelper(string input)
        {
            var text = input;
            if (string.IsNullOrEmpty(text) || text.Trim().Length < 4 || !(text.Contains("..", StringComparison.Ordinal) || text.Contains(". .", StringComparison.Ordinal)))
            {
                return text;
            }

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
                {
                    removeLength++;
                }
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
                {
                    text = "<i>" + text.Substring(4);
                }
            }
            tag = "<i> ...";
            if (text.StartsWith(tag, StringComparison.Ordinal))
            {
                text = "<i>" + text.Substring(tag.Length);
                while (text.StartsWith("<i>.", StringComparison.Ordinal) || text.StartsWith("<i> ", StringComparison.Ordinal))
                {
                    text = "<i>" + text.Substring(4, text.Length - 4);
                }
            }

            tag = "- <i>...";
            if (text.StartsWith(tag, StringComparison.Ordinal))
            {
                text = "- <i>" + text.Substring(tag.Length);
                while (text.StartsWith("- <i>.", StringComparison.Ordinal))
                {
                    text = "- <i>" + text.Substring(6);
                }
            }
            tag = "- <i> ...";
            if (text.StartsWith(tag, StringComparison.Ordinal))
            {
                text = "- <i>" + text.Substring(tag.Length);
                while (text.StartsWith("- <i>.", StringComparison.Ordinal))
                {
                    text = "- <i>" + text.Substring(6);
                }
            }

            // Narrator:... Hello foo!
            text = text.Replace(":..", ": ..");
            tag = ": ..";
            if (text.Contains(tag, StringComparison.Ordinal))
            {
                text = text.Replace(": ..", ": ");
                while (text.Contains(": ."))
                {
                    text = text.Replace(": .", ": ");
                }
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
                {
                    return text;
                }

                text = text.Remove(0, index + 1).TrimStart();
                text = FixEllipsesStartHelper(text);
                pre += " ";
            }
            return pre + text;
        }

        private static readonly string[] EndPlusDashList = { ". -", "! -", "? -", "— -", "-- -", ") -", "] -", "> -", ".\" -", "!\" -", "?\" -", ")\" -", "]\" -" };
        private static readonly string[] EndPlusDashListShort = { ". -", "! -", "? -", "— -" };

        public static string FixDialogsOnOneLine(string text, string language)
        {
            if (text.Contains(" - ") && !text.Contains(Environment.NewLine))
            {
                var noTagLines = HtmlUtil.RemoveHtmlTags(text.Replace(" - ", Environment.NewLine), true).SplitToLines();
                if (noTagLines.Count == 2)
                {
                    string part0 = noTagLines[0];
                    string part1 = noTagLines[1];
                    if (part0.Length > 1 && "-—!?.\")]".Contains(part0[part0.Length - 1]) &&
                        part1.Length > 1 && (char.IsUpper(part1[0]) || "\"'{[(".Contains(part1[0])))
                    {
                        text = text.Replace(" - ", Environment.NewLine + "- ");
                        if (char.IsLetter((part0[0])) || CharUtils.IsDigit(part0[0]))
                        {
                            if (text.Length > 3 && text[0] == '<' && text[2] == '>')
                            {
                                text = "<" + text[1] + ">" + "- " + text.Substring(3).TrimStart();
                            }
                            else
                            {
                                text = "- " + text;
                            }
                        }
                    }
                }
            }

            var idx = text.IndexOfAny(EndPlusDashList, StringComparison.Ordinal);
            if (idx >= 0)
            {
                int lineCount = Utilities.GetNumberOfLines(text);
                if (lineCount == 2)
                {
                    string temp = Utilities.AutoBreakLine(text, 99, 33, language);
                    var arr = text.SplitToLines();
                    var arrTemp = temp.SplitToLines();
                    if (arrTemp.Count == 2 && arr.Count == 2)
                    {
                        var secLine = HtmlUtil.RemoveHtmlTags(arr[1]).TrimStart();
                        var secLineTemp = HtmlUtil.RemoveHtmlTags(arrTemp[1]).TrimStart();
                        if (secLineTemp.StartsWith('-') && !secLine.StartsWith('-'))
                        {
                            text = temp;
                        }
                    }
                }
                else if (lineCount == 1)
                {
                    string temp = Utilities.AutoBreakLine(text, language);
                    var arrTemp = temp.SplitToLines();
                    if (arrTemp.Count == 2)
                    {
                        var secLineTemp = HtmlUtil.RemoveHtmlTags(arrTemp[1]).TrimStart();
                        if (secLineTemp.StartsWith('-'))
                        {
                            text = temp;
                        }
                    }
                    else
                    {
                        int index = text.IndexOfAny(EndPlusDashListShort, StringComparison.Ordinal);
                        if (index < 0 && text.IndexOf("-- -", StringComparison.Ordinal) > 0)
                        {
                            index = text.IndexOf("-- -", StringComparison.Ordinal) + 1;
                        }

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
            {
                return true;
            }

            prevText = prevText.Replace("♪", string.Empty).Replace("♫", string.Empty).Trim();
            bool isPrevEndOfLine = prevText.Length > 1 &&
                                   !prevText.EndsWith("...", StringComparison.Ordinal) &&
                                   (".!?—".Contains(prevText[prevText.Length - 1]) || // em dash, unicode character
                                    prevText.EndsWith("--", StringComparison.Ordinal));

            if (isPrevEndOfLine && prevText.Length > 5 && prevText.EndsWith('.') &&
                prevText[prevText.Length - 3] == '.' &&
                char.IsLetter(prevText[prevText.Length - 2]))
            {
                isPrevEndOfLine = false;
            }
            return isPrevEndOfLine;
        }

        public static bool IsOneSentence(string text)
        {
            var lines = HtmlUtil.RemoveHtmlTags(text).SplitToLines();
            if (lines.Count == 1)
            {
                return true;
            }

            if (lines.Count > 2)
            {
                return false;
            }

            return !lines[0].HasSentenceEnding();
        }

        public static string FixHyphensRemoveForSingleLine(Subtitle subtitle, string input, int i)
        {
            if (string.IsNullOrEmpty(input) || !IsOneSentence(input))
            {
                return input;
            }

            var text = input;
            if (HtmlUtil.RemoveHtmlTags(text, true).TrimStart().StartsWith('-') ||
                text.Contains(Environment.NewLine + '-') ||
                text.Contains(Environment.NewLine + " -") ||
                text.Contains(Environment.NewLine + "<i>-", StringComparison.OrdinalIgnoreCase) ||
                text.Contains(Environment.NewLine + "<i> -", StringComparison.OrdinalIgnoreCase))
            {
                var prev = subtitle.GetParagraphOrDefault(i - 1);

                if (prev == null || !HtmlUtil.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith('-') || HtmlUtil.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith("--", StringComparison.Ordinal))
                {
                    var noTagLines = HtmlUtil.RemoveHtmlTags(text, true).SplitToLines();
                    int startHyphenCount = noTagLines.Count(line => line.TrimStart().StartsWith('-'));
                    if (startHyphenCount == 1)
                    {
                        bool remove = true;
                        var noTagParts = HtmlUtil.RemoveHtmlTags(text).SplitToLines();
                        if (noTagParts.Count == 2)
                        {
                            if (noTagParts[0].TrimStart().StartsWith('-') && noTagParts[1].Contains(": ") ||
                                noTagParts[1].TrimStart().StartsWith('-') && noTagParts[0].Contains(": "))
                            {
                                remove = false;
                            }
                        }

                        if (remove)
                        {
                            int idx = text.IndexOf('-');
                            var st = new StrippableText(text);
                            if (st.Pre.Length >= idx)
                            {
                                text = text.Remove(idx, 1).TrimStart();
                                idx = text.IndexOf('-');
                                st = new StrippableText(text);
                                if (idx >= 0 && st.Pre.Length >= idx)
                                {
                                    text = text.Remove(idx, 1).TrimStart();
                                    st = new StrippableText(text);
                                }
                                idx = text.IndexOf('-');
                                if (idx >= 0 && st.Pre.Length >= idx)
                                {
                                    text = text.Remove(idx, 1).TrimStart();
                                }
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
                                        text = RemoveSpacesBeginLine(text);
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
                    var st = new StrippableText(text);
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
            if (text.StartsWith("{\\"))
            {
                int end = text.IndexOf('}');
                if (end > 0 && end + 1 < text.Length && text[end + 1] == ' ')
                {
                    text = text.Remove(end + 1, 1);
                }
            }

            if (text.LineStartsWithHtmlTag(true) && text[3] == 0x20)
            {
                text = text.Remove(3, 1).TrimStart();
            }
            if (text.LineStartsWithHtmlTag(false, true))
            {
                var closeIdx = text.IndexOf('>');
                if (closeIdx > 6 && text[closeIdx + 1] == 0x20)
                {
                    text = text.Remove(closeIdx + 1, 1);
                }
            }
            return text;
        }

        public static string RemoveSpacesBeginLineAfterEllipses(string line)
        {
            if (line.StartsWith("... ", StringComparison.Ordinal))
            {
                line = line.Remove(3, 1);
            }
            if (line.Length > 6 && line.LineStartsWithHtmlTag(true)) // <i>... foobar
            {
                var idx = line.IndexOf('>') + 1;
                var pre = line.Substring(0, idx);
                line = line.Remove(0, idx).TrimStart();
                if (line.StartsWith("... ", StringComparison.Ordinal))
                {
                    line = line.Remove(3, 1);
                }
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
                    {
                        line = line.Remove("... ".Length - 1, 1);
                    }
                    line = fontTag + line;
                }
            }
            return line;
        }

        public static string FixHyphensAdd(Subtitle subtitle, int i, string language)
        {
            Paragraph p = subtitle.Paragraphs[i];
            string text = p.Text;
            var textCache = HtmlUtil.RemoveHtmlTags(text.TrimStart(), true);
            if (textCache.StartsWith('-') || textCache.Contains(Environment.NewLine + "-"))
            {
                Paragraph prev = subtitle.GetParagraphOrDefault(i - 1);

                if (prev == null || !HtmlUtil.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith('-') || HtmlUtil.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith("--", StringComparison.Ordinal))
                {
                    var lines = textCache.SplitToLines();
                    int startHyphenCount = lines.Count(line => line.TrimStart().StartsWith('-'));
                    int totalSpaceHyphen = Utilities.CountTagInText(text, " -");
                    if (startHyphenCount == 1 && totalSpaceHyphen == 0)
                    {
                        var parts = textCache.SplitToLines();
                        if (parts.Count == 2 && !string.IsNullOrWhiteSpace(parts[0]))
                        {
                            var part0 = parts[0].TrimEnd().Trim('"');
                            bool doAdd = "!?.".Contains(part0[part0.Length - 1]) || language == "ko";
                            if (parts[0].TrimStart().StartsWith('-') && parts[1].Contains(':') && doAdd ||
                                parts[1].TrimStart().StartsWith('-') && parts[0].Contains(':') && doAdd)
                            {
                                doAdd = false;
                            }

                            if (doAdd)
                            {
                                int idx = text.IndexOf('-');
                                int newLineIdx = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                                bool addSecondLine = idx < newLineIdx;

                                if (addSecondLine && idx > 0 && char.IsLetter(text[idx - 1]))
                                {
                                    addSecondLine = false;
                                }

                                if (addSecondLine)
                                {
                                    // add dash in second line.
                                    var originalParts = text.SplitToLines();
                                    if (originalParts[1].LineStartsWithHtmlTag(true))
                                    {
                                        originalParts[1] = originalParts[1].Substring(0, 3) + "- " + originalParts[1].Remove(0, 3).TrimEnd();
                                    }
                                    else if (originalParts[1].StartsWith("{\\an", StringComparison.Ordinal) && originalParts[1].Length > 6 && originalParts[1][5] == '}')
                                    {
                                        originalParts[1] = originalParts[1].Insert(6, "- ");
                                    }
                                    else
                                    {
                                        originalParts[1] = "- " + originalParts[1].Trim();
                                    }
                                    text = originalParts[0] + Environment.NewLine + originalParts[1];
                                }
                                else
                                {
                                    // add dash in first line.
                                    if (text.LineStartsWithHtmlTag(true))
                                    {
                                        text = text.Substring(0, 3) + "- " + text.Remove(0, 3).TrimEnd();
                                    }
                                    else if (text.StartsWith("{\\an", StringComparison.Ordinal) && text.Length > 6 && text[5] == '}')
                                    {
                                        text = text.Insert(6, "- ");
                                    }
                                    else
                                    {
                                        text = "- " + text.Trim();
                                    }
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
            {
                text = text.TrimStart('>', ' ').TrimStart();
            }
            return post + text;
        }

        private static readonly char[] NoShortLineList = { '.', '?', '!', ':', ';', '…', '♪', '♫' };

        public static string FixShortLines(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || !text.Contains(Environment.NewLine, StringComparison.Ordinal))
            {
                return text;
            }

            string s = HtmlUtil.RemoveHtmlTags(text, true);
            if (s.Contains(Environment.NewLine) && s.Replace(Environment.NewLine, " ").Replace("  ", " ").Length < Configuration.Settings.General.MergeLinesShorterThan)
            {
                s = s.TrimEnd().TrimEnd('.', '?', '!', ':', ';');
                s = s.TrimStart('-');
                if (!s.Contains(NoShortLineList) &&
                    !s.Contains(Environment.NewLine + "-") &&
                    !(s.StartsWith('[') && s.Contains("]" + Environment.NewLine, StringComparison.Ordinal)) &&
                    !(s.StartsWith('(') && s.Contains(")" + Environment.NewLine, StringComparison.Ordinal)) &&
                    s != s.ToUpperInvariant())
                {
                    return Utilities.UnbreakLine(text);
                }
            }
            return text;
        }

    }
}
