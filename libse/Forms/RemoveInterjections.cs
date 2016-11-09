using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public class RemoveInterjections
    {
        private List<string> _interjectionList;

        // Contains words/interjections that must be skipped.
        private HashSet<string> _skipSet;

        public RemoveInterjections()
            : this(Configuration.Settings.Tools.Interjections.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
        }

        public RemoveInterjections(IList<string> interjections)
        {
            Load(interjections);
            // TODO: Load skip set.
        }

        public void Load(IList<string> interjections)
        {
            _interjectionList = new List<string>(interjections.Count);
            foreach (var s in interjections)
            {
                if (s.Length > 0)
                {
                    _interjectionList.Add(s);
                    var upper = s.ToUpper();
                    _interjectionList.Add(upper);
                    var lower = s.ToLower();
                    _interjectionList.Add(lower);
                    _interjectionList.Add(lower.CapitalizeFirstLetter());
                }
            }
            _interjectionList.Sort((a, b) => b.Length.CompareTo(a.Length)); // TODO: Or a.Length.CompareTo(b.Length);
        }

        public void ReloadFromCSVString(string csvString)
        {
            Load(csvString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public string Remove(string text)
        {
            string oldText = text;
            bool doRepeat = true;
            while (doRepeat)
            {
                doRepeat = false;
                foreach (string s in _interjectionList)
                {
                    // TODO: Check if not in _skipSet (optimzie by checking above before going this far)?
                    //if (!_skipSet.Contains(interjection)) // O(1)
                    if (text.Contains(s))
                    {
                        var regex = new Regex("\\b" + Regex.Escape(s) + "\\b");
                        var match = regex.Match(text);
                        if (match.Success)
                        {
                            int index = match.Index;
                            string newText = text.Remove(index, s.Length);

                            // Remove remaning white-spaces.
                            newText = RemoveExtraWhiteSpaces(newText, index);

                            string pre = string.Empty;
                            if (index > 0)
                                doRepeat = true;

                            bool removeAfter = true;

                            if (index > s.Length)
                            {
                                // foobar, Ahhhh!
                                if (newText.Length > index - s.Length + 3)
                                {
                                    int subIndex = index - s.Length + 1;
                                    string subTemp = newText.Substring(subIndex, 3);
                                    if (subTemp == ", !" || subTemp == ", ?" || subTemp == ", .")
                                    {
                                        newText = newText.Remove(subIndex, 2);
                                        removeAfter = false;
                                    }
                                }
                                if (removeAfter && newText.Length > index - s.Length + 2)
                                {
                                    int subIndex = index - s.Length;
                                    string subTemp = newText.Substring(subIndex, 3);
                                    if (subTemp == ", !" || subTemp == ", ?" || subTemp == ", .")
                                    {
                                        newText = newText.Remove(subIndex, 2);
                                        removeAfter = false;
                                    }
                                    else
                                    {
                                        subTemp = newText.Substring(subIndex);
                                        if (subTemp.StartsWith(", -—", StringComparison.Ordinal))
                                        {
                                            newText = newText.Remove(subIndex, 3);
                                            removeAfter = false;
                                        }
                                        else if (subTemp.StartsWith(", --", StringComparison.Ordinal))
                                        {
                                            newText = newText.Remove(subIndex, 2);
                                            removeAfter = false;
                                        }
                                        else if (index > 2 && subTemp.StartsWith("-  —", StringComparison.Ordinal))
                                        {
                                            newText = newText.Remove(subIndex + 2, 2).Replace("  ", " ");
                                            removeAfter = false;
                                        }
                                    }
                                }
                                if (removeAfter && newText.Length > index - s.Length + 2)
                                {
                                    int subIndex = index - s.Length + 1;
                                    string subTemp = newText.Substring(subIndex, 2);
                                    if (subTemp == "-!" || subTemp == "-?" || subTemp == "-.")
                                    {
                                        newText = newText.Remove(subIndex, 1);
                                        removeAfter = false;
                                    }
                                    subTemp = newText.Substring(subIndex);
                                    if (subTemp == " !" || subTemp == " ?" || subTemp == " .")
                                    {
                                        newText = newText.Remove(subIndex, 1);
                                        removeAfter = false;
                                    }
                                }
                            }

                            if (index > 3 && index - 2 < newText.Length)
                            {
                                string subTemp = newText.Substring(index - 2);
                                if (subTemp.StartsWith(",  —", StringComparison.Ordinal) || subTemp.StartsWith(", —", StringComparison.Ordinal))
                                {
                                    newText = newText.Remove(index - 2, 1);
                                    index--;
                                }
                                if (subTemp.StartsWith("- ...", StringComparison.Ordinal))
                                {
                                    removeAfter = false;
                                }
                            }

                            if (removeAfter)
                            {
                                if (index == 0)
                                {
                                    if (newText.StartsWith('-'))
                                        newText = newText.Remove(0, 1).Trim();
                                }
                                else if (index == 3 && newText.StartsWith("<i>-", StringComparison.Ordinal))
                                {
                                    newText = newText.Remove(3, 1);
                                }
                                else if (index > 0 && newText.Length > index)
                                {
                                    pre = text.Substring(0, index);
                                    newText = newText.Remove(0, index);
                                    if (newText.StartsWith('-') && pre.EndsWith('-'))
                                        newText = newText.Remove(0, 1);
                                    if (newText.StartsWith('-') && pre.EndsWith("- ", StringComparison.Ordinal))
                                        newText = newText.Remove(0, 1);
                                }

                                if (newText.StartsWith("..."))
                                {
                                    pre = pre.Trim();
                                }
                                else
                                {
                                    while (newText.Length > 0 && " ,.?!".Contains(newText[0]))
                                    {
                                        newText = newText.Remove(0, 1);
                                        doRepeat = true;
                                    }
                                }
                                if (newText.Length > 0 && s[0].ToString(CultureInfo.InvariantCulture) != s[0].ToString(CultureInfo.InvariantCulture).ToLower())
                                {
                                    newText = char.ToUpper(newText[0]) + newText.Substring(1);
                                    doRepeat = true;
                                }

                                if (newText.StartsWith('-') && pre.EndsWith(' '))
                                    newText = newText.Remove(0, 1);

                                if (newText.StartsWith('—') && pre.EndsWith(','))
                                    pre = pre.TrimEnd(',') + " ";
                                newText = pre + newText;
                            }

                            if (newText.EndsWith(Environment.NewLine + "- ", StringComparison.Ordinal))
                                newText = newText.Remove(newText.Length - 2).TrimEnd();

                            var st = new StripableText(newText);
                            if (st.StrippedText.Length == 0)
                                return string.Empty;

                            if (newText.StartsWith('-') && !newText.Contains(Environment.NewLine) && text.Contains(Environment.NewLine))
                                newText = newText.Remove(0, 1).Trim();

                            text = newText;
                        }
                    }
                }
            }
            var lines = text.SplitToLines();
            if (lines.Length == 2 && text != oldText)
            {
                if (lines[0] == "-" && lines[1] == "-")
                    return string.Empty;
                if (lines[0].Length > 1 && lines[0][0] == '-' && lines[1].Trim() == "-")
                    return lines[0].Remove(0, 1).Trim();
                if (lines[1].Length > 1 && lines[1][0] == '-' && lines[0].Trim() == "-")
                    return lines[1].Remove(0, 1).Trim();
                if (lines[1].Length > 4 && lines[1].StartsWith("<i>-", StringComparison.Ordinal) && lines[0].Trim() == "-")
                    return "<i>" + lines[1].Remove(0, 4).Trim();
                if (lines[0].Length > 1 && lines[1] == "-" || lines[1] == "." || lines[1] == "!" || lines[1] == "?")
                {
                    if (lines[0].StartsWith('-') && oldText.Contains(Environment.NewLine + "-"))
                        lines[0] = lines[0].Remove(0, 1);
                    return lines[0].Trim();
                }
                var noTags0 = HtmlUtil.RemoveHtmlTags(lines[0]).Trim();
                var noTags1 = HtmlUtil.RemoveHtmlTags(lines[1]).Trim();
                if (noTags0 == "-")
                {
                    if (noTags1 == noTags0)
                        return string.Empty;
                    if (lines[1].Length > 1 && lines[1][0] == '-')
                        return lines[1].Remove(0, 1).Trim();
                    if (lines[1].Length > 4 && lines[1].StartsWith("<i>-", StringComparison.Ordinal))
                        return "<i>" + lines[1].Remove(0, 4).Trim();
                    return lines[1];
                }
                if (noTags1 == "-")
                {
                    if (lines[0].Length > 1 && lines[0][0] == '-')
                        return lines[0].Remove(0, 1).Trim();
                    if (lines[0].Length > 4 && lines[0].StartsWith("<i>-", StringComparison.Ordinal))
                    {
                        if (!lines[0].Contains("</i>") && lines[1].Contains("</i>"))
                            return "<i>" + lines[0].Remove(0, 4).Trim() + "</i>";
                        return "<i>" + lines[0].Remove(0, 4).Trim();
                    }
                    return lines[0];
                }
            }
            if (lines.Length == 2)
            {
                if (string.IsNullOrWhiteSpace(lines[1].Replace(".", string.Empty).Replace("?", string.Empty).Replace("!", string.Empty).Replace("-", string.Empty).Replace("—", string.Empty)))
                {
                    text = lines[0];
                    lines = text.SplitToLines();
                }
                else if (string.IsNullOrWhiteSpace(lines[0].Replace(".", string.Empty).Replace("?", string.Empty).Replace("!", string.Empty).Replace("-", string.Empty).Replace("—", string.Empty)))
                {
                    text = lines[1];
                    lines = text.SplitToLines();
                }
            }
            if (lines.Length == 1 && text != oldText && Utilities.GetNumberOfLines(oldText) == 2)
            {
                if ((oldText.StartsWith('-') || oldText.StartsWith("<i>-", StringComparison.Ordinal)) &&
                    (oldText.Contains("." + Environment.NewLine) || oldText.Contains(".</i>" + Environment.NewLine) ||
                     oldText.Contains("!" + Environment.NewLine) || oldText.Contains("!</i>" + Environment.NewLine) ||
                     oldText.Contains("?" + Environment.NewLine) || oldText.Contains("?</i>" + Environment.NewLine)))
                {
                    if (text.StartsWith("<i>-", StringComparison.Ordinal))
                        text = "<i>" + text.Remove(0, 4).TrimStart();
                    else
                        text = text.TrimStart('-').TrimStart();
                }
                else if ((oldText.Contains(Environment.NewLine + "-") || oldText.Contains(Environment.NewLine + "<i>-")) &&
                    (oldText.Contains("." + Environment.NewLine) || oldText.Contains(".</i>" + Environment.NewLine) ||
                     oldText.Contains("!" + Environment.NewLine) || oldText.Contains("!</i>" + Environment.NewLine) ||
                     oldText.Contains("?" + Environment.NewLine) || oldText.Contains("?</i>" + Environment.NewLine)))
                {
                    if (text.StartsWith("<i>-", StringComparison.Ordinal))
                        text = "<i>" + text.Remove(0, 4).TrimStart();
                    else
                        text = text.TrimStart('-').TrimStart();
                }
            }

            if (oldText != text)
            {
                text = text.Replace(Environment.NewLine + "<i>" + Environment.NewLine, Environment.NewLine + "<i>");
                text = text.Replace(Environment.NewLine + "</i>" + Environment.NewLine, "</i>" + Environment.NewLine);
                if (text.StartsWith("<i>" + Environment.NewLine))
                {
                    text = text.Remove(3, Environment.NewLine.Length);
                }
                if (text.EndsWith(Environment.NewLine + "</i>"))
                {
                    text = text.Remove(text.Length - (Environment.NewLine.Length + 4), Environment.NewLine.Length);
                }
                text = text.Replace(Environment.NewLine + "</i>" + Environment.NewLine, "</i>" + Environment.NewLine);
            }
            return text;
        }

        private static string RemoveExtraWhiteSpaces(string text, int idx)
        {
            while (idx == 0 && text.StartsWith("... ", StringComparison.Ordinal))
            {
                text = text.Remove(3, 1);
            }
            while (idx == 3 && text.StartsWith("<i>... ", StringComparison.Ordinal))
            {
                text = text.Remove(6, 1);
            }
            while (idx > 2 && (" \r\n".Contains(text.Substring(idx - 1, 1))) && text.Substring(idx).StartsWith("... ", StringComparison.Ordinal))
            {
                text = text.Remove(idx + 3, 1);
            }
            if (text.Remove(0, idx) == " —" && text.EndsWith("—  —", StringComparison.Ordinal))
            {
                text = text.Remove(text.Length - 3);
                if (text.EndsWith(Environment.NewLine + "—", StringComparison.Ordinal))
                    text = text.Remove(text.Length - 1).TrimEnd();
            }
            else if (text.Remove(0, idx) == " —" && text.EndsWith("-  —", StringComparison.Ordinal))
            {
                text = text.Remove(text.Length - 3);
                if (text.EndsWith(Environment.NewLine + "-", StringComparison.Ordinal))
                    text = text.Remove(text.Length - 1).TrimEnd();
            }
            else if (idx == 2 && text.StartsWith("-  —", StringComparison.Ordinal))
            {
                text = text.Remove(2, 2);
            }
            else if (idx == 2 && text.StartsWith("- —", StringComparison.Ordinal))
            {
                text = text.Remove(2, 1);
            }
            else if (idx == 0 && text.StartsWith(" —", StringComparison.Ordinal))
            {
                text = text.Remove(0, 2);
            }
            else if (idx == 0 && text.StartsWith('—'))
            {
                text = text.Remove(0, 1);
            }
            else if (idx > 3 && (text.Substring(idx - 2) == ".  —" || text.Substring(idx - 2) == "!  —" || text.Substring(idx - 2) == "?  —"))
            {
                text = text.Remove(idx - 2, 1).Replace("  ", " ");
            }
            return text;
        }

        private static string ProcessNewText(string text, int idx)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // idx = 0;
            if (idx == 0)
            {
                int j = idx;
                while (text.Length > 0 && text[idx] == '?' && text[idx] == '!' && text[idx] == '.')
                {
                    j++;
                }
                if (j > idx)
                {
                    return text.Remove(idx, j).TrimStart();
                }
            }

            // idx = 1;
            if (idx == 1)
            {
                string pre = string.Empty;
                int j = idx;
                if (text[idx - 1] == '-') // TODO: Add more chars which must be removed when it's at index position 0. i.e: music symbols
                {
                    pre = text[idx - 1].ToString();
                }
                while (j < text.Length && (text[j] == ',' || text[j] == ' ' || text[idx] == '?' && text[idx] == '!' && text[idx] == '.'))
                {
                    j++;
                }
                if (j < text.Length)
                {
                    return pre + text.Substring(j);
                }
                else
                {
                    return string.Empty;
                }
            }

            // idx >= 2
            if (idx >= 2)
            {
                // Fo[,!?]\s*([.?!]|Fooar) (include other punctuations) or use char.ispunctiation*
                int j = idx;
                int k = idx;

                // White spaces before interjection.
                if (text[j - 1] == ' ')
                {
                    j--;
                }

                // Punctuation before and after interjection.
                if (char.IsPunctuation(text[j - 1]))
                {
                    // TODO: Filter some chars. ie '['.
                    while (k < text.Length && char.IsPunctuation(text[k])) k++;

                    // ie: "Foobar, !" => Foobar!
                    if (k == text.Length)
                    {
                        text = text.Remove(j - 1, k - (j + 2));
                    }
                    else
                    {
                        text = text.Remove(j, k - j);
                    }
                }
                else
                {
                    while (k < text.Length && text[k] == ' ') k++;
                    if (text[j] == ' ')
                    {
                        j++;
                    }
                    text = text.Remove(j, k - j);
                }
            }

            return text;
        }
    }
}
