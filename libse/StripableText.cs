using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class StripableText
    {
        public string Pre { get; set; }
        public string Post { get; set; }
        public string StrippedText { get; set; }
        public string OriginalText { get; private set; }

        public string MergedString
        {
            get { return Pre + StrippedText + Post; }
        }

        public StripableText(string text)
            : this(text, " >-\"”“['‘`´¶(♪¿¡.…—", " -\"”“]'`´¶)♪.!?:…—")
        {
        }

        public StripableText(string text, string stripStartCharacters, string stripEndCharacters)
        {
            OriginalText = text;

            Pre = string.Empty;
            if (text.Length > 0 && ("<{" + stripStartCharacters).Contains(text[0]))
            {
                int beginLength;
                do
                {
                    beginLength = text.Length;

                    while (text.Length > 0 && stripStartCharacters.Contains(text[0]))
                    {
                        Pre += text[0];
                        text = text.Remove(0, 1);
                    }

                    // ASS/SSA codes like {\an9}
                    int endIndex = text.IndexOf('}');
                    if (endIndex > 0 && text.StartsWith("{\\", StringComparison.Ordinal))
                    {
                        int nextStartIndex = text.IndexOf('{', 2);
                        if (nextStartIndex == -1 || nextStartIndex > endIndex)
                        {
                            endIndex++;
                            Pre += text.Substring(0, endIndex);
                            text = text.Remove(0, endIndex);
                        }
                    }

                    // tags like <i> or <font face="Segoe Print" color="#ff0000">
                    endIndex = text.IndexOf('>');
                    if (text.StartsWith('<') && endIndex >= 2)
                    {
                        endIndex++;
                        Pre += text.Substring(0, endIndex);
                        text = text.Remove(0, endIndex);
                    }
                }
                while (text.Length < beginLength);
            }

            Post = string.Empty;
            if (text.Length > 0 && (">" + stripEndCharacters).Contains(text[text.Length - 1]))
            {
                int beginLength;
                do
                {
                    beginLength = text.Length;

                    while (text.Length > 0 && stripEndCharacters.Contains(text[text.Length - 1]))
                    {
                        Post = text[text.Length - 1] + Post;
                        text = text.Substring(0, text.Length - 1);
                    }

                    if (text.EndsWith('>'))
                    {
                        // tags </i> </b> </u>
                        if (text.EndsWith("</i>", StringComparison.OrdinalIgnoreCase) ||
                            text.EndsWith("</b>", StringComparison.OrdinalIgnoreCase) ||
                            text.EndsWith("</u>", StringComparison.OrdinalIgnoreCase))
                        {
                            Post = text.Substring(text.Length - 4) + Post;
                            text = text.Substring(0, text.Length - 4);
                        }

                        // tag </font>
                        if (text.EndsWith("</font>", StringComparison.OrdinalIgnoreCase))
                        {
                            Post = text.Substring(text.Length - 7) + Post;
                            text = text.Substring(0, text.Length - 7);
                        }
                    }
                }
                while (text.Length < beginLength);
            }

            StrippedText = text;
        }

        private static string GetAndInsertNextId(List<string> replaceIds, List<string> replaceNames, string name, int idName)
        {
            string id = $"_@{idName}_";
            replaceIds.Add(id);
            replaceNames.Add(name);
            return id;
        }

        private void ReplaceNames1Remove(List<string> namesEtc, List<string> replaceIds, List<string> replaceNames, List<string> originalNames)
        {
            if (Post.StartsWith('.'))
            {
                StrippedText += ".";
                Post = Post.Remove(0, 1);
            }

            string lower = StrippedText.ToLower();
            int idName = 0;
            foreach (string name in namesEtc)
            {
                int start = lower.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                while (start >= 0 && start < lower.Length)
                {
                    bool startOk = (start == 0) || (lower[start - 1] == ' ') || (lower[start - 1] == '-') ||
                                   (lower[start - 1] == '"') || (lower[start - 1] == '\'') || (lower[start - 1] == '>') ||
                                   Environment.NewLine.EndsWith(lower[start - 1]);

                    if (startOk && string.CompareOrdinal(name, "Don") == 0 && lower.Substring(start).StartsWith("don't"))
                        startOk = false;

                    if (startOk)
                    {
                        int end = start + name.Length;
                        bool endOk = end <= lower.Length;
                        if (endOk)
                            endOk = end == lower.Length || (@" ,.!?:;')- <""" + Environment.NewLine).Contains(lower[end]);

                        if (endOk && StrippedText.Length >= start + name.Length)
                        {
                            string originalName = StrippedText.Substring(start, name.Length);
                            originalNames.Add(originalName);
                            StrippedText = StrippedText.Remove(start, name.Length);
                            StrippedText = StrippedText.Insert(start, GetAndInsertNextId(replaceIds, replaceNames, name, idName++));
                            lower = StrippedText.ToLower();
                        }
                    }
                    if (start + 3 > lower.Length)
                        start = lower.Length + 1;
                    else
                        start = lower.IndexOf(name, start + 3, StringComparison.OrdinalIgnoreCase);
                }
            }

            if (StrippedText.EndsWith('.'))
            {
                Post = "." + Post;
                StrippedText = StrippedText.TrimEnd('.');
            }
        }

        private void ReplaceNames2Fix(List<string> replaceIds, List<string> replaceNames)
        {
            for (int i = 0; i < replaceIds.Count; i++)
            {
                StrippedText = StrippedText.Replace(replaceIds[i], replaceNames[i]);
            }
        }

        private static readonly char[] ExpectedCharsArray = { '.', '!', '?', ':', ';', ')', ']', '}', '(', '[', '{' };
        public void FixCasing(List<string> namesEtc, bool changeNameCases, bool makeUppercaseAfterBreak, bool checkLastLine, string lastLine)
        {
            var replaceIds = new List<string>();
            var replaceNames = new List<string>();
            var originalNames = new List<string>();
            ReplaceNames1Remove(namesEtc, replaceIds, replaceNames, originalNames);

            if (checkLastLine)
            {
                string s = HtmlUtil.RemoveHtmlTags(lastLine).TrimEnd().TrimEnd('\"').TrimEnd();

                bool startWithUppercase = string.IsNullOrEmpty(s) ||
                                          s.EndsWith('.') ||
                                          s.EndsWith('!') ||
                                          s.EndsWith('?') ||
                                          s.EndsWith(". ♪", StringComparison.Ordinal) ||
                                          s.EndsWith("! ♪", StringComparison.Ordinal) ||
                                          s.EndsWith("? ♪", StringComparison.Ordinal) ||
                                          s.EndsWith(']') ||
                                          s.EndsWith(')') ||
                                          s.EndsWith(':');

                // start with uppercase after music symbol - but only if next line does not start with music symbol
                if (!startWithUppercase && (s.EndsWith('♪') || s.EndsWith('♫')))
                {
                    if (!Pre.Contains(new[] { '♪', '♫' }))
                        startWithUppercase = true;
                }

                if (startWithUppercase && StrippedText.Length > 0 && !Pre.Contains("..."))
                {
                    StrippedText = char.ToUpper(StrippedText[0]) + StrippedText.Substring(1);
                }
            }

            if (makeUppercaseAfterBreak && StrippedText.Contains(ExpectedCharsArray))
            {
                const string breakAfterChars = @".!?:;)]}([{";
                const string expectedChars = "\"`´'()<>!?.- \r\n";
                var sb = new StringBuilder();
                bool lastWasBreak = false;
                for (int i = 0; i < StrippedText.Length; i++)
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
                                string temp = sb.ToString(0, idx - 1).Trim();
                                if (temp.Length > 0 && !Utilities.LowercaseLetters.Contains(temp[temp.Length - 1]))
                                    lastWasBreak = true;
                            }
                            else
                            {
                                lastWasBreak = true;
                            }
                        }
                    }
                }
                StrippedText = sb.ToString();
            }

            if (changeNameCases)
                ReplaceNames2Fix(replaceIds, replaceNames);
            else
                ReplaceNames2Fix(replaceIds, originalNames);
        }

    }
}
