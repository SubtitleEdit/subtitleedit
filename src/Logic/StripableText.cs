using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
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
            if (text.Length > 0 && !Utilities.AllLettersAndNumbers.Contains(text[0]))
            {
                for (int i = 0; i < 5; i++)
                {
                    while (text.Length > 0 && stripStartCharacters.Contains(text[0]))
                    {
                        Pre += text[0];
                        text = text.Substring(1);
                    }

                    // ASS/SSA codes like {\an9}

                    if (text.StartsWith("{\\", StringComparison.Ordinal))
                    {
                        int endIndex = text.IndexOf('}');
                        if (endIndex > 0 && (!text.Contains('{') || text.IndexOf('{') > endIndex))
                        {
                            int index = text.IndexOf('}') + 1;
                            Pre += text.Substring(0, index);
                            text = text.Substring(index);
                        }
                    }

                    // tags like <i> or <font color="#ff0000">
                    if (text.StartsWith('<') && text.IndexOf('>') <= 21)
                    {
                        int index = text.IndexOf('>') + 1;
                        Pre += text.Substring(0, index);
                        text = text.Substring(index);
                    }
                }
            }

            Post = string.Empty;
            if (text.Length > 0 && !Utilities.AllLettersAndNumbers.Contains(text[text.Length - 1]))
            {
                for (int i = 0; i < 5; i++)
                {
                    while (text.Length > 0 && stripEndCharacters.Contains(text[text.Length - 1]))
                    {
                        Post = text[text.Length - 1] + Post;
                        text = text.Substring(0, text.Length - 1);
                    }

                    if (text.EndsWith('>'))
                    {
                        string lower = text.ToLower();

                        // tags </i> </b> </u>
                        if (lower.EndsWith("</i>", StringComparison.Ordinal) ||
                            lower.EndsWith("</b>", StringComparison.Ordinal) ||
                            lower.EndsWith("</u>", StringComparison.Ordinal))
                        {
                            Post = text.Substring(text.Length - 4, 4) + Post;
                            text = text.Substring(0, text.Length - 4);
                        }

                        // tag </font>
                        if (lower.EndsWith("</font>", StringComparison.Ordinal))
                        {
                            Post = text.Substring(text.Length - 7, 7) + Post;
                            text = text.Substring(0, text.Length - 7);
                        }
                    }
                }
            }

            StrippedText = text;
        }

        private static string GetAndInsertNextId(List<string> replaceIds, List<string> replaceNames, string name)
        {
            int i = 0;
            string id = string.Format("_@{0}_", i);
            while (replaceIds.Contains(id))
            {
                i++;
                id = string.Format("_@{0}_", i);
            }
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

            foreach (string name in namesEtc)
            {
                int start = lower.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                while (start >= 0 && start < lower.Length)
                {
                    bool startOk = (start == 0) || (lower[start - 1] == ' ') || (lower[start - 1] == '-') ||
                                   (lower[start - 1] == '"') || (lower[start - 1] == '\'') || (lower[start - 1] == '>') ||
                                   Environment.NewLine.EndsWith(lower[start - 1]);

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
                            StrippedText = StrippedText.Insert(start, GetAndInsertNextId(replaceIds, replaceNames, name));
                            lower = StrippedText.ToLower();
                        }
                    }
                    if (start + 3 > lower.Length)
                        start = lower.Length + 1;
                    else
                        start = lower.IndexOf(name.ToLower(), start + 3, StringComparison.Ordinal);
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

        public void FixCasing(List<string> namesEtc, bool changeNameCases, bool makeUppercaseAfterBreak, bool checkLastLine, string lastLine)
        {
            var replaceIds = new List<string>();
            var replaceNames = new List<string>();
            var originalNames = new List<string>();
            ReplaceNames1Remove(namesEtc, replaceIds, replaceNames, originalNames);

            if (checkLastLine)
            {
                string s = Utilities.RemoveHtmlTags(lastLine).TrimEnd().TrimEnd('\"').TrimEnd();

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
                    if (!Pre.Contains('♪') && !Pre.Contains('♫'))
                        startWithUppercase = true;
                }

                if (startWithUppercase && StrippedText.Length > 0 && !Pre.Contains("..."))
                {
                    StrippedText = char.ToUpper(StrippedText[0]) + StrippedText.Substring(1);
                }
            }

            if (makeUppercaseAfterBreak &&
                (StrippedText.Contains('.') ||
                StrippedText.Contains('!') ||
                StrippedText.Contains('?') ||
                StrippedText.Contains(':') ||
                StrippedText.Contains(';') ||
                StrippedText.Contains(')') ||
                StrippedText.Contains(']') ||
                StrippedText.Contains('}') ||
                StrippedText.Contains('(') ||
                StrippedText.Contains('[') ||
                StrippedText.Contains('{')))
            {
                var sb = new StringBuilder();
                bool lastWasBreak = false;
                for (int i = 0; i < StrippedText.Length; i++)
                {
                    var s = StrippedText[i];
                    if (lastWasBreak)
                    {
                        if (("\"`´'()<>!?.- " + Environment.NewLine).Contains(s))
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
                            if (@".!?:;)]}([{".Contains(s))
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
                        if (@".!?:;)]}([{".Contains(s))
                        {
                            if (s == ']' && sb.ToString().IndexOf('[') > 1)
                            { // I [Motor roaring] love you!
                                string temp = sb.ToString().Substring(0, sb.ToString().IndexOf('[') - 1).Trim();
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