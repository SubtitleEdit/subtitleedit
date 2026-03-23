using Nikse.SubtitleEdit.Core.Common;
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

            var s2Trimmed = S2.Trim();

            // Parse to find the first word with its surrounding tags
            var openTags = new System.Collections.Generic.Stack<(string opening, string closing)>();
            var wordChars = new StringBuilder();
            var leadingTags = new StringBuilder();
            var inWord = false;
            var wordEndPos = -1;

            for (int i = 0; i < s2Trimmed.Length; i++)
            {
                var ch = s2Trimmed[i];

                // Check for tag start
                if (ch == '<' || (ch == '{' && i + 1 < s2Trimmed.Length && s2Trimmed[i + 1] == '\\'))
                {
                    var tagStart = i;
                    var endChar = ch == '<' ? '>' : '}';
                    var tagSb = new StringBuilder();
                    tagSb.Append(ch);
                    i++;

                    while (i < s2Trimmed.Length && s2Trimmed[i] != endChar)
                    {
                        tagSb.Append(s2Trimmed[i]);
                        i++;
                    }

                    if (i < s2Trimmed.Length)
                    {
                        tagSb.Append(s2Trimmed[i]);
                    }

                    var tag = tagSb.ToString();

                    // Determine if it's an opening or closing tag
                    if (tag.StartsWith("</", StringComparison.Ordinal))
                    {
                        // HTML closing tag
                        if (inWord)
                        {
                            // We hit a closing tag after the word started
                            wordEndPos = i + 1;
                            break;
                        }
                        else if (openTags.Count > 0)
                        {
                            openTags.Pop();
                        }
                    }
                    else if (tag.StartsWith("<", StringComparison.Ordinal))
                    {
                        // HTML opening tag
                        var tagName = tag.Substring(1, tag.IndexOf('>') > 1 ? tag.IndexOf('>') - 1 : tag.Length - 2).Split(' ')[0].ToLowerInvariant();
                        var closingTag = $"</{tagName}>";
                        openTags.Push((tag, closingTag));

                        if (inWord)
                        {
                            leadingTags.Append(tag);
                        }
                    }
                    else if (tag.Contains("{\\i0}") || tag.Contains("{\\b0}") || tag.Contains("{\\u0}"))
                    {
                        // ASS closing tag
                        if (inWord)
                        {
                            wordEndPos = i + 1;
                            break;
                        }
                        else if (openTags.Count > 0)
                        {
                            openTags.Pop();
                        }
                    }
                    else if (tag.StartsWith("{\\", StringComparison.Ordinal))
                    {
                        // ASS opening tag
                        var closingTag = tag.Contains("\\i1") ? "{\\i0}" : "";
                        openTags.Push((tag, closingTag));

                        if (inWord)
                        {
                            leadingTags.Append(tag);
                        }
                    }
                }
                else if (char.IsWhiteSpace(ch))
                {
                    if (inWord)
                    {
                        // End of word
                        wordEndPos = i;
                        break;
                    }
                }
                else
                {
                    if (!inWord)
                    {
                        inWord = true;
                        // Capture current open tags
                        foreach (var (opening, closing) in openTags.Reverse())
                        {
                            leadingTags.Append(opening);
                        }
                    }
                    wordChars.Append(ch);
                }
            }

            if (wordChars.Length == 0)
            {
                return;
            }

            if (wordEndPos == -1)
            {
                wordEndPos = s2Trimmed.Length;
            }

            // Build word with tags
            var closingTags = new StringBuilder();
            foreach (var (opening, closing) in openTags)
            {
                if (!string.IsNullOrEmpty(closing))
                {
                    closingTags.Insert(0, closing);
                }
            }

            var wordWithTags = leadingTags.ToString() + wordChars.ToString() + closingTags.ToString();

            // Check for tag merging with S1
            var s1Trimmed = S1.Trim();
            if (!string.IsNullOrWhiteSpace(s1Trimmed))
            {
                if (closingTags.Length > 0 && leadingTags.Length > 0)
                {
                    var firstClosing = closingTags.ToString().Split(new[] { '<', '{' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (!string.IsNullOrEmpty(firstClosing))
                    {
                        var checkTag = (firstClosing.StartsWith("/") ? "<" : "{") + firstClosing;
                        if (s1Trimmed.EndsWith(checkTag, StringComparison.OrdinalIgnoreCase))
                        {
                            // Merge: remove closing from S1, remove opening from word
                            s1Trimmed = s1Trimmed.Substring(0, s1Trimmed.Length - checkTag.Length).TrimEnd();
                            var firstOpening = openTags.Last().opening;
                            wordWithTags = wordWithTags.Substring(firstOpening.Length);
                            S1 = s1Trimmed + " " + wordChars.ToString() + checkTag;
                        }
                        else
                        {
                            S1 = s1Trimmed + " " + wordWithTags.Trim();
                        }
                    }
                    else
                    {
                        S1 = s1Trimmed + " " + wordWithTags.Trim();
                    }
                }
                else
                {
                    S1 = s1Trimmed + " " + wordWithTags.Trim();
                }
            }
            else
            {
                S1 = wordWithTags.Trim();
            }

            S1 = AutoBreakIfNeeded(S1);

            // Remaining S2 content - need to preserve tag structure
            var s2Remaining = wordEndPos < s2Trimmed.Length ? s2Trimmed.Substring(wordEndPos).Trim() : string.Empty;

            // If we had opening tags and there's remaining content, we need to check if we should re-add those tags
            if (!string.IsNullOrEmpty(s2Remaining) && openTags.Count > 0)
            {
                // If s2Remaining contains a closing tag that matches one of our open tags,
                // OR if a tag doesn't have a closing equivalent, we need to add back the opening tag
                var tagsToReopen = new System.Collections.Generic.List<(string opening, string closing)>();

                foreach (var (opening, closing) in openTags.Reverse())
                {
                    if (string.IsNullOrEmpty(closing))
                    {
                        // ASS positioning tag without closing - always re-add it
                        tagsToReopen.Add((opening, closing));
                    }
                    else if (s2Remaining.Contains(closing))
                    {
                        // Has a closing tag in remaining content - re-add opening
                        tagsToReopen.Add((opening, closing));
                    }
                }

                if (tagsToReopen.Count > 0)
                {
                    var reopenTagsSb = new StringBuilder();
                    foreach (var (opening, closing) in tagsToReopen)
                    {
                        reopenTagsSb.Append(opening);
                    }
                    s2Remaining = reopenTagsSb.ToString() + s2Remaining;
                }
            }

            S2 = s2Remaining;
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

            var s1Trimmed = S1.Trim();

            // Find start of last word and any active tags
            var openTags = new System.Collections.Generic.Stack<(string opening, string closing)>();
            var lastWordStart = -1;
            var lastWordTags = new System.Collections.Generic.List<(string opening, string closing)>();
            var inWord = false;

            for (int i = 0; i < s1Trimmed.Length; i++)
            {
                var ch = s1Trimmed[i];

                if (ch == '<' || (ch == '{' && i + 1 < s1Trimmed.Length && s1Trimmed[i + 1] == '\\'))
                {
                    var tagStart = i;
                    var endChar = ch == '<' ? '>' : '}';
                    var tagSb = new StringBuilder();
                    tagSb.Append(ch);
                    i++;

                    while (i < s1Trimmed.Length && s1Trimmed[i] != endChar)
                    {
                        tagSb.Append(s1Trimmed[i]);
                        i++;
                    }

                    if (i < s1Trimmed.Length)
                    {
                        tagSb.Append(s1Trimmed[i]);
                    }

                    var tag = tagSb.ToString();

                    if (tag.StartsWith("</", StringComparison.Ordinal)) // HTML closing
                    {
                        if (inWord)
                        {
                            inWord = false;
                        }
                        if (openTags.Count > 0)
                        {
                            openTags.Pop();
                        }
                    }
                    else if (tag.StartsWith("<", StringComparison.Ordinal)) // HTML opening
                    {
                        var tagName = tag.Substring(1, tag.IndexOf('>') > 1 ? tag.IndexOf('>') - 1 : tag.Length - 2).Split(' ')[0].ToLowerInvariant();
                        var closingTag = $"</{tagName}>";
                        openTags.Push((tag, closingTag));
                    }
                    else if (tag.Contains("{\\i0}") || tag.Contains("{\\b0}") || tag.Contains("{\\u0}")) // ASS closing
                    {
                        if (inWord)
                        {
                            inWord = false;
                        }
                        if (openTags.Count > 0)
                        {
                            openTags.Pop();
                        }
                    }
                    else if (tag.StartsWith("{\\", StringComparison.Ordinal)) // ASS opening
                    {
                        var closingTag = tag.Contains("\\i1") ? "{\\i0}" : "";
                        openTags.Push((tag, closingTag));
                    }
                }
                else if (char.IsWhiteSpace(ch))
                {
                    if (inWord)
                    {
                        inWord = false;
                    }
                }
                else
                {
                    if (!inWord)
                    {
                        inWord = true;
                        lastWordStart = i;
                        lastWordTags = openTags.Reverse().ToList();
                    }
                }
            }

            if (lastWordStart == -1)
            {
                return;
            }

            var lastWordContent = s1Trimmed.Substring(lastWordStart);
            var newS1Pre = s1Trimmed.Substring(0, lastWordStart);
            while (newS1Pre.Contains("  "))
            {
                newS1Pre = newS1Pre.Replace("  ", " ");
            }
            newS1Pre = newS1Pre.TrimEnd();

            // Close tags for S1
            var s1ClosingTags = new StringBuilder();
            var movedWordOpeningTags = new StringBuilder();

            foreach (var (opening, closing) in lastWordTags)
            {
                if (!string.IsNullOrEmpty(closing))
                {
                    s1ClosingTags.Insert(0, closing);
                }
                movedWordOpeningTags.Append(opening);
            }

            S1 = newS1Pre + s1ClosingTags;
            S1 = RemoveEmptyTags(S1).Trim();

            var movedWord = movedWordOpeningTags + lastWordContent;

            // Add word to S2
            var s2Trimmed = S2.Trim();
            if (!string.IsNullOrWhiteSpace(s2Trimmed))
            {
                var movedWordTrimmed = movedWord.Trim();
                foreach (var tag in lastWordTags)
                {
                    if (s2Trimmed.StartsWith(tag.opening, StringComparison.OrdinalIgnoreCase))
                    {
                        s2Trimmed = s2Trimmed.Substring(tag.opening.Length).TrimStart();
                        if (!string.IsNullOrEmpty(tag.closing) && movedWordTrimmed.EndsWith(tag.closing, StringComparison.OrdinalIgnoreCase))
                        {
                            movedWordTrimmed = movedWordTrimmed.Substring(0, movedWordTrimmed.Length - tag.closing.Length).TrimEnd();
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                S2 = movedWordTrimmed + " " + s2Trimmed;
            }
            else
            {
                S2 = movedWord.Trim();
            }

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
            if (s.StartsWith("<b>", StringComparison.OrdinalIgnoreCase) && (s.EndsWith("</b>", StringComparison.OrdinalIgnoreCase) || arr[0].EndsWith("</b>", StringComparison.OrdinalIgnoreCase)))
            {
                return pre + s.Insert(3, word.Trim() + " ").Trim();
            }
            if (s.StartsWith("<u>", StringComparison.OrdinalIgnoreCase) && (s.EndsWith("</u>", StringComparison.OrdinalIgnoreCase) || arr[0].EndsWith("</u>", StringComparison.OrdinalIgnoreCase)))
            {
                return pre + s.Insert(3, word.Trim() + " ").Trim();
            }
            if (s.StartsWith("<font", StringComparison.OrdinalIgnoreCase) && s.Contains(">") && s.Contains("</font>", StringComparison.OrdinalIgnoreCase))
            {
                var endIdx = s.IndexOf('>');
                return pre + s.Insert(endIdx + 1, word.Trim() + " ").Trim();
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
            if (s.EndsWith("</b>", StringComparison.OrdinalIgnoreCase) && (s.StartsWith("<b>", StringComparison.OrdinalIgnoreCase) || arr[arr.Count - 1].StartsWith("<b>", StringComparison.OrdinalIgnoreCase)))
            {
                return s.Insert(s.Length - 4, " " + word.Trim()).Trim();
            }
            if (s.EndsWith("</u>", StringComparison.OrdinalIgnoreCase) && (s.StartsWith("<u>", StringComparison.OrdinalIgnoreCase) || arr[arr.Count - 1].StartsWith("<u>", StringComparison.OrdinalIgnoreCase)))
            {
                return s.Insert(s.Length - 4, " " + word.Trim()).Trim();
            }
            if (s.EndsWith("</font>", StringComparison.OrdinalIgnoreCase) && s.Contains("<font", StringComparison.OrdinalIgnoreCase))
            {
                return s.Insert(s.Length - 7, " " + word.Trim()).Trim();
            }

            return (s.Trim() + " " + word.Trim()).Trim();
        }

        private static string AutoBreakIfNeeded(string s)
        {
            var doBreak = false;
            foreach (var line in s.SplitToLines())
            {
                if (line.CountCharacters(false) > Configuration.Settings.General.SubtitleLineMaximumLength)
                {
                    doBreak = true;
                    break;
                }
            }

            return doBreak ? Utilities.AutoBreakLine(s) : s;
        }
    }
}
