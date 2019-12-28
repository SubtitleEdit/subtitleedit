using System;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public class MoveWordUpDown
    {
        public string S1 { get; private set; }
        public string S2 { get; private set; }

        public MoveWordUpDown(string s1, string s2)
        {
            S1 = s1 ?? string.Empty;
            S2 = s2 ?? string.Empty;
        }

        /// <summary>
        /// Move first word in S2 to up as last word in S1
        /// </summary>
        public void MoveWordUp()
        {
            if (string.IsNullOrEmpty(S2))
            {
                return;
            }

            var assTagOn = false;
            var htmlTagOn = false;
            var sbWord = new StringBuilder();
            var done = false;
            var sbS2 = new StringBuilder();
            for (int i = 0; i < S2.Length; i++)
            {
                var ch = S2[i];
                if (done)
                {
                    sbS2.Append(ch);
                }
                else if (assTagOn)
                {
                    if (ch == '}')
                    {
                        assTagOn = false;
                    }
                    sbS2.Append(ch);
                }
                else if (htmlTagOn)
                {
                    if (ch == '>')
                    {
                        htmlTagOn = false;
                    }
                    sbS2.Append(ch);
                }
                else if (ch == '{' && S2.Substring(i).StartsWith("{\\", StringComparison.Ordinal))
                {
                    assTagOn = true;
                    sbS2.Append(ch);
                }
                else if (S2.Substring(i).StartsWith("<font", StringComparison.OrdinalIgnoreCase) ||
                         S2.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase) ||
                         S2.Substring(i).StartsWith("<b>", StringComparison.OrdinalIgnoreCase) ||
                         S2.Substring(i).StartsWith("<u>", StringComparison.OrdinalIgnoreCase) ||
                         S2.Substring(i).StartsWith("</font", StringComparison.OrdinalIgnoreCase) ||
                         S2.Substring(i).StartsWith("</i>", StringComparison.OrdinalIgnoreCase) ||
                         S2.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase) ||
                         S2.Substring(i).StartsWith("</u>", StringComparison.OrdinalIgnoreCase))
                {
                    htmlTagOn = true;
                    sbS2.Append(ch);
                }
                else if (sbWord.ToString().Trim().Length > 0 && (ch == ' ' || ch == '\r' || ch == '\n'))
                {
                    done = true;
                }
                else
                {
                    sbWord.Append(ch);
                }
            }
            S1 = AddWordAfter(sbWord.ToString().Trim(), S1);
            S1 = AutoBreakIfNeeded(S1);
            S2 = sbS2.ToString().Trim();
            S2 = RemoveEmptyTags(S2);
        }

        /// <summary>
        /// Move last word from S1 down as first word in S2
        /// </summary>
        public void MoveWordDown()
        {
            if (string.IsNullOrEmpty(S1))
            {
                return;
            }

            var assTagOn = false;
            var htmlTagOn = false;
            var sbWord = new StringBuilder();
            var done = false;
            var sbS1 = new StringBuilder();
            for (int i = S1.Length - 1; i >= 0; i--)
            {
                var ch = S1[i];
                if (done)
                {
                    sbS1.Append(ch);
                }
                else if (assTagOn)
                {
                    if (ch == '{' && S2.Substring(i).StartsWith("{\\", StringComparison.Ordinal))
                    {
                        assTagOn = false;
                    }
                    sbS1.Append(ch);
                }
                else if (htmlTagOn)
                {
                    if (S1.Substring(i).StartsWith("<font", StringComparison.OrdinalIgnoreCase) ||
                        S1.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase) ||
                        S1.Substring(i).StartsWith("<b>", StringComparison.OrdinalIgnoreCase) ||
                        S1.Substring(i).StartsWith("<u>", StringComparison.OrdinalIgnoreCase) ||
                        S1.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase) ||
                        S1.Substring(i).StartsWith("</i>", StringComparison.OrdinalIgnoreCase) ||
                        S1.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase) ||
                        S1.Substring(i).StartsWith("</u>", StringComparison.OrdinalIgnoreCase))
                    {
                        htmlTagOn = false;
                    }
                    sbS1.Append(ch);
                }
                else if (ch == '}' && S1.StartsWith("{\\", StringComparison.Ordinal))
                {
                    assTagOn = true;
                    sbS1.Append(ch);
                }
                else if (ch == '>' && S1.Substring(0, i + 1).Contains("<font ", StringComparison.OrdinalIgnoreCase) && IsPartOfFontTag(S1, i) ||
                         S1.Substring(0, i + 1).EndsWith("</font>", StringComparison.OrdinalIgnoreCase) ||
                         S1.Substring(0, i + 1).EndsWith("</i>", StringComparison.OrdinalIgnoreCase) ||
                         S1.Substring(0, i + 1).EndsWith("</b>", StringComparison.OrdinalIgnoreCase) ||
                         S1.Substring(0, i + 1).EndsWith("</u>", StringComparison.OrdinalIgnoreCase) ||
                         S1.Substring(0, i + 1).EndsWith("<i>", StringComparison.OrdinalIgnoreCase) ||
                         S1.Substring(0, i + 1).EndsWith("<b>", StringComparison.OrdinalIgnoreCase) ||
                         S1.Substring(0, i + 1).EndsWith("<u>", StringComparison.OrdinalIgnoreCase))
                {
                    htmlTagOn = true;
                    sbS1.Append(ch);
                }
                else if (sbWord.ToString().Trim().Length > 0 && (ch == ' ' || ch == '\r' || ch == '\n'))
                {
                    done = true;
                }
                else
                {
                    sbWord.Append(ch);
                }
            }
            S1 = string.Join(string.Empty, sbS1.ToString().Trim().ToCharArray().Reverse());
            S1 = RemoveEmptyTags(S1);
            S2 = AddWordBefore(string.Join(string.Empty, sbWord.ToString().Trim().ToCharArray().Reverse()), S2);
            S2 = AutoBreakIfNeeded(S2);
        }

        private static bool IsPartOfFontTag(string s, int i)
        {
            var indexOfFontTag = s.Substring(0, i).LastIndexOf("<font ", StringComparison.OrdinalIgnoreCase);
            if (indexOfFontTag < 0)
            {
                return false;
            }

            var indexOfEndFontTag = s.IndexOf(">", indexOfFontTag, StringComparison.Ordinal);
            if (indexOfEndFontTag < 0)
            {
                return false;
            }

            return i >= indexOfFontTag && i <= indexOfEndFontTag;
        }

        private static string RemoveEmptyTags(string s)
        {
            var noTags = HtmlUtil.RemoveHtmlTags(s, true);
            if (noTags.Length == 0)
            {
                return string.Empty;
            }

            return s
                .Replace("<i></i>", string.Empty)
                .Replace("<u></u>", string.Empty)
                .Replace("<b></b>", string.Empty);
        }

        private static string AddWordBefore(string word, string input)
        {
            var pre = string.Empty;
            var s = input;
            if (s.StartsWith("{\\") && s.Contains("}"))
            {
                var idx = s.IndexOf('}');
                pre = s.Substring(0, idx + 1);
                s = s.Remove(0, idx + 1);
            }
            var arr = s.SplitToLines();
            if (s.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) && (s.EndsWith("</i>", StringComparison.OrdinalIgnoreCase) || arr[0].EndsWith("</i>", StringComparison.OrdinalIgnoreCase)))
            {
                return pre + s.Insert(3, word.Trim() + " ").Trim();
            }

            return pre + (word.Trim() + " " + s.Trim()).Trim();
        }

        private static string AddWordAfter(string word, string s)
        {
            var arr = s.SplitToLines();
            if (s.EndsWith("</i>", StringComparison.OrdinalIgnoreCase) && (s.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) || arr[arr.Count - 1].StartsWith("<i>", StringComparison.OrdinalIgnoreCase)))
            {
                return s.Insert(s.Length - 4, " " + word.Trim()).Trim();
            }

            return (s.Trim() + " " + word.Trim()).Trim();
        }

        private static string AutoBreakIfNeeded(string s)
        {
            bool doBreak = false;
            foreach (var line in s.SplitToLines())
            {
                if (HtmlUtil.RemoveHtmlTags(line, true).Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                {
                    doBreak = true;
                    break;
                }
            }

            return doBreak ? Utilities.AutoBreakLine(s) : s;
        }

    }
}
