using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class StrippableText
    {
        public string Pre { get; set; }
        public string Post { get; set; }
        public string StrippedText { get; set; }
        public string OriginalText { get; }

        public string MergedString => Pre + StrippedText + Post;

        public StrippableText(string text)
            : this(text, " >-\"„”“['‘`´¶(♪¿¡.…—", " -\"”“]'`´¶)♪.!?:…—؛،؟")
        {
        }

        public StrippableText(string input, string stripStartCharacters, string stripEndCharacters)
        {
            // A StrippableText is constructed per paragraph in several FixCommonErrors rules,
            // so strip by moving [start, end) cursors over the input. The previous version did
            // "Pre += text[0]; text = text.Remove(0, 1)" per stripped character (and the
            // mirrored Substring dance at the end), reallocating the whole remaining string for
            // every dash/quote/bracket/tag - quadratic on decorated lines. The cursor logic
            // below mirrors the old flow exactly; only the final three Substring calls allocate.
            OriginalText = input;
            var text = input;
            var start = 0;
            var end = text.Length;

            if (end > 0 && ("<{" + stripStartCharacters).Contains(text[0]))
            {
                int beginStart;
                do
                {
                    beginStart = start;

                    while (start < end && stripStartCharacters.Contains(text[start]))
                    {
                        start++;
                    }

                    // ASS/SSA codes like {\an9}
                    int endIndex = text.IndexOf('}', start);
                    if (endIndex > start && start + 1 < end && text[start] == '{' && text[start + 1] == '\\')
                    {
                        int nextStartIndex = start + 2 < end ? text.IndexOf('{', start + 2) : -1;
                        if (nextStartIndex == -1 || nextStartIndex > endIndex)
                        {
                            start = endIndex + 1;
                        }
                    }

                    // tags like <i> or <font face="Segoe Print" color="#ff0000">
                    endIndex = start < end ? text.IndexOf('>', start) : -1;
                    if (start < end && text[start] == '<' && endIndex - start >= 2)
                    {
                        start = endIndex + 1;
                    }
                }
                while (start > beginStart);
            }

            if (end > start && (">" + stripEndCharacters).Contains(text[end - 1]))
            {
                int beginEnd;
                do
                {
                    beginEnd = end;

                    while (end > start && stripEndCharacters.Contains(text[end - 1]))
                    {
                        end--;
                    }

                    if (end > start && text[end - 1] == '>')
                    {
                        // tags </i> </b> </u>
                        if (end - start >= 4 && text[end - 4] == '<' && text[end - 3] == '/' &&
                            (EndsWithTag(text, end, "</i>") || EndsWithTag(text, end, "</b>") || EndsWithTag(text, end, "</u>")))
                        {
                            end -= 4;
                        }

                        // tag </font>
                        if (end - start >= 7 && EndsWithTag(text, end, "</font>"))
                        {
                            end -= 7;
                        }

                        if (end > start && text[end - 1] == '>')
                        {
                            var lastIndexOfStart = text.LastIndexOf('<', end - 1);
                            if (lastIndexOfStart >= start)
                            {
                                var tag = text.Substring(lastIndexOfStart, end - lastIndexOfStart);
                                tag = tag.TrimStart('<').TrimEnd('>');
                                if (tag.StartsWith("/c.", StringComparison.Ordinal) && !tag.Contains(' ') && !tag.Contains('\n'))
                                {
                                    end = lastIndexOfStart;
                                }
                            }
                        }
                    }
                }
                while (end < beginEnd);
            }

            Pre = text.Substring(0, start);
            Post = text.Substring(end);
            StrippedText = text.Substring(start, end - start);
        }

        // True when text[..end] ends with the given lowercase tag, ignoring case.
        private static bool EndsWithTag(string text, int end, string tag) =>
            string.Compare(text, end - tag.Length, tag, 0, tag.Length, StringComparison.OrdinalIgnoreCase) == 0;

        private static string GetAndInsertNextId(List<string> replaceIds, List<string> replaceNames, string name, int idName)
        {
            string id = $"_@{idName}_";
            replaceIds.Add(id);
            replaceNames.Add(name);
            return id;
        }

        private void ReplaceNames1Remove(IEnumerable<string> nameList, List<string> replaceIds, List<string> replaceNames, List<string> originalNames)
        {
            if (Post.StartsWith('.'))
            {
                StrippedText += ".";
                Post = Post.Remove(0, 1);
            }

            string lower = StrippedText.ToLowerInvariant();
            int idName = 0;
            foreach (string name in nameList)
            {
                int start = lower.IndexOf(name.ToLowerInvariant(), StringComparison.Ordinal);
                while (start >= 0 && start < lower.Length)
                {
                    bool startOk = (start == 0) || (lower[start - 1] == ' ') || (lower[start - 1] == '-') ||
                                   (lower[start - 1] == '"') || (lower[start - 1] == '\'') || (lower[start - 1] == '>') || (lower[start - 1] == '[') || (lower[start - 1] == '“') ||
                                   Environment.NewLine.EndsWith(lower[start - 1]);

                    if (startOk && string.CompareOrdinal(name, "Don") == 0 && lower.Substring(start).StartsWith("don't", StringComparison.Ordinal))
                    {
                        startOk = false;
                    }

                    if (startOk)
                    {
                        int end = start + name.Length;
                        bool endOk = end <= lower.Length;
                        if (endOk)
                        {
                            endOk = end == lower.Length || (@" ,.!?:;')]- <”""" + Environment.NewLine).Contains(lower[end]);
                        }

                        if (endOk && StrippedText.Length >= start + name.Length)
                        {
                            string originalName = StrippedText.Substring(start, name.Length);
                            originalNames.Add(originalName);
                            StrippedText = StrippedText.Remove(start, name.Length);
                            StrippedText = StrippedText.Insert(start, GetAndInsertNextId(replaceIds, replaceNames, name, idName++));
                            lower = StrippedText.ToLowerInvariant();
                        }
                    }
                    if (start + 3 > lower.Length)
                    {
                        start = lower.Length + 1;
                    }
                    else
                    {
                        start = lower.IndexOf(name, start + 3, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }

            if (StrippedText.EndsWith('.'))
            {
                Post = "." + Post;
                StrippedText = StrippedText.TrimEnd('.');
            }
        }

        private void ReplacAssaTagsRemove(List<string> replaceIds, List<string> replaceNames, List<string> originalNames)
        {
            int idName = 1000;
            var idx = 0;
            while (StrippedText.IndexOf("{", idx) >= 0 && StrippedText.IndexOf('}', idx) > 0)
            {
                var start = StrippedText.IndexOf("{", idx);
                var end = StrippedText.IndexOf("}", idx);
                if (end < start)
                {
                    return;
                }

                var tag = StrippedText.Substring(start, end - start + 1);
                StrippedText = StrippedText.Remove(start, tag.Length);
                StrippedText = StrippedText.Insert(start, GetAndInsertNextId(replaceIds, replaceNames, tag, idName++));
                originalNames.Add(tag);

                idx = end + 1;
                if (idx >= StrippedText.Length)
                {
                    return;
                }   
            }
        }

        private void ReplaceNames2Fix(List<string> replaceIds, List<string> replaceNames)
        {
            for (var i = 0; i < replaceIds.Count; i++)
            {
                StrippedText = StrippedText.Replace(replaceIds[i], replaceNames[i]);
            }
        }

        private static readonly char[] ExpectedCharsArray = { '.', '!', '?', ':', ';', ')', ']', '}', '(', '[', '{' };
        public void FixCasing(IEnumerable<string> nameList, bool changeNameCases, bool makeUppercaseAfterBreak, bool checkLastLine, string lastLine, double millisecondsFromLast = 0)
        {
            var replaceIds = new List<string>();
            var replaceNames = new List<string>();
            var originalNames = new List<string>();
            ReplaceNames1Remove(nameList, replaceIds, replaceNames, originalNames);
            ReplacAssaTagsRemove(replaceIds, replaceNames, originalNames);

            if (checkLastLine && ShouldStartWithUpperCase(lastLine, millisecondsFromLast))
            {
                if (StrippedText.StartsWith("_@", StringComparison.Ordinal))
                {
                    for (var i = 0; i < replaceIds.Count; i++)
                    {
                        var id = $"_@{i}_";
                        if (StrippedText.StartsWith(id, StringComparison.Ordinal))
                        {
                            if (!string.IsNullOrEmpty(originalNames[i]))
                            {
                                originalNames[i] = originalNames[i].CapitalizeFirstLetter();
                            }

                            break;
                        }
                    }
                }
                else
                {
                    StrippedText = StrippedText.CapitalizeFirstLetter();
                }
            }

            if (makeUppercaseAfterBreak && StrippedText.Contains(ExpectedCharsArray))
            {
                const string breakAfterChars = @".!?:;)]}([{";
                const string expectedChars = "\"“`´'()<>!?.- \r\n";
                var sb = new StringBuilder(StrippedText.Length);
                bool lastWasBreak = false;
                for (var i = 0; i < StrippedText.Length; i++)
                {
                    var s = StrippedText[i];
                    if (lastWasBreak)
                    {
                        if (expectedChars.Contains(s))
                        {
                            sb.Append(s);
                        }
                        else if ((sb.EndsWith('<') || sb.ToString().EndsWith("</", StringComparison.Ordinal)) && i + 1 < StrippedText.Length && StrippedText[i + 1] == '>')
                        { // tags
                            sb.Append(s);
                        }
                        else if (sb.EndsWith('<') && s == '/' && i + 2 < StrippedText.Length && StrippedText[i + 2] == '>')
                        { // tags
                            sb.Append(s);
                        }
                        else if (sb.ToString().EndsWith("... ", StringComparison.Ordinal))
                        {
                            sb.Append(s);
                            lastWasBreak = false;
                        }
                        else
                        {
                            if (breakAfterChars.Contains(s))
                            {
                                sb.Append(s);
                            }
                            else
                            {
                                lastWasBreak = false;
                                sb.Append(char.ToUpper(s));

                                if (StrippedText.Substring(i).StartsWith("_@", StringComparison.Ordinal))
                                {
                                    var ks = StrippedText.Substring(i);
                                    for (var k = 0; k < replaceIds.Count; k++)
                                    {
                                        var id = $"_@{k}_";
                                        if (ks.StartsWith(id, StringComparison.Ordinal))
                                        {
                                            if (!string.IsNullOrEmpty(originalNames[k]))
                                            {
                                                originalNames[k] = char.ToUpper(originalNames[k][0]) + originalNames[k].Remove(0, 1);
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        sb.Append(s);
                        if (breakAfterChars.Contains(s))
                        {
                            var idx = sb.ToString().IndexOf('[');
                            if (s == ']' && idx > 1)
                            { // I [Motor roaring] love you!
                                var temp = sb.ToString(0, idx - 1).Trim();
                                if (temp.Length > 0 && !char.IsLetterOrDigit(temp[temp.Length - 1]))
                                {
                                    lastWasBreak = true;
                                }
                            }
                            else if (s == ']' && idx == -1 && Pre.IndexOf('[') >= 0)
                            { // [ Motor roaring ] Hallo!
                                lastWasBreak = true;
                            }
                            else if (s == ':') // seems to be the rule (in subtitles) to nearly always capitalize first letter efter semicolon
                            {
                                lastWasBreak = true;
                            }
                            else
                            {
                                idx = sb.ToString().LastIndexOf(' ');
                                if (idx >= 0 && idx < sb.Length - 2 && !IsInMiddleOfUrl(i - idx, StrippedText.Substring(idx + 1)))
                                {
                                    lastWasBreak = true;
                                }
                                else if (StrippedText.Length > i + 1 && " \r\n".Contains(StrippedText[i + 1]))
                                {
                                    lastWasBreak = true;
                                }
                            }
                        }
                        else if (s == '-' && Pre.IndexOf('-') >= 0)
                        {
                            if (sb.ToString().EndsWith(Environment.NewLine + "-"))
                            {
                                var prevLine = HtmlUtil.RemoveHtmlTags(sb.ToString().Substring(0, sb.Length - 2).TrimEnd());
                                if (prevLine.EndsWith('.') ||
                                    prevLine.EndsWith('!') ||
                                    prevLine.EndsWith('?') ||
                                    prevLine.EndsWith(". ♪", StringComparison.Ordinal) ||
                                    prevLine.EndsWith("! ♪", StringComparison.Ordinal) ||
                                    prevLine.EndsWith("? ♪", StringComparison.Ordinal) ||
                                    prevLine.EndsWith(']') ||
                                    prevLine.EndsWith(')') ||
                                    prevLine.EndsWith(':'))
                                {
                                    lastWasBreak = true;
                                }
                            }
                        }
                    }
                }
                StrippedText = sb.ToString();
            }

            ReplaceNames2Fix(replaceIds, changeNameCases ? replaceNames : originalNames);
        }

        private static bool IsInMiddleOfUrl(int idx, string s)
        {
            if (idx < s.Length - 1 && (char.IsWhiteSpace(s[idx]) || char.IsPunctuation(s[idx])))
            {
                return false;
            }

            return s.StartsWith("www.", StringComparison.OrdinalIgnoreCase) || s.StartsWith("http", StringComparison.OrdinalIgnoreCase);
        }

        public string CombineWithPrePost(string text)
        {
            return Pre + text + Post;
        }

        private bool ShouldStartWithUpperCase(string lastLine, double millisecondsgaps)
        {
            // do not capitalize url
            if (StrippedText.StartsWith("www.", StringComparison.OrdinalIgnoreCase) || StrippedText.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // do not capitalize word like iPhone
            if (StrippedText.Length > 1 && StrippedText[0] == 'i' && char.IsUpper(StrippedText[1]))
            {
                return false;
            }

            // shouldn't capitalize current line not closed
            if (Pre.Contains("...", StringComparison.Ordinal) || Pre.Contains("…", StringComparison.Ordinal))
            {
                return false;
            }

            // too much gaps between lines, so should be considered as closed
            if (millisecondsgaps > 5000)
            {
                return true;
            }

            var preLine = HtmlUtil.RemoveHtmlTags(lastLine).TrimEnd().TrimEnd('\"', '”').TrimEnd();

            // check if previous line was fully closed
            if (string.IsNullOrEmpty(preLine))
            {
                return true;
            }

            char lastChar = preLine[preLine.Length - 1];
            if (lastChar == '♪')
            {
                string tempPreLine = preLine.Substring(0, preLine.Length - 1).TrimEnd();
                // update last char
                if (tempPreLine.Length > 0)
                {
                    lastChar = tempPreLine[tempPreLine.Length - 1];
                }
            }
            if (lastChar == '.' || lastChar == '!' || lastChar == '?' || lastChar == ']' || lastChar == ')' || lastChar == ':' || lastChar == '_')
            {
                return true;
            }

            // previous line ends with music symbol but current line doesn't contains any music symbol
            if ((preLine.EndsWith('♪') || preLine.EndsWith('♫')) && !Pre.Contains(new[] { '♪', '♫' }))
            {
                return true;
            }

            // do not capitalize
            return false;
        }
    }
}
