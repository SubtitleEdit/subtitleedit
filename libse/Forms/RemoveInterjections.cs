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
                    if (text.Contains(s))
                    {
                        var regex = new Regex("\\b" + Regex.Escape(s) + "\\b");
                        var match = regex.Match(text);
                        if (match.Success)
                        {
                            int index = match.Index;
                            string interjection = text.Remove(index, s.Length);
                            while (index == 0 && interjection.StartsWith("... ", StringComparison.Ordinal))
                            {
                                interjection = interjection.Remove(3, 1);
                            }
                            while (index == 3 && interjection.StartsWith("<i>... ", StringComparison.Ordinal))
                            {
                                interjection = interjection.Remove(6, 1);
                            }
                            while (index > 2 && (" \r\n".Contains(text.Substring(index - 1, 1))) && interjection.Substring(index).StartsWith("... ", StringComparison.Ordinal))
                            {
                                interjection = interjection.Remove(index + 3, 1);
                            }

                            if (interjection.Remove(0, index) == " —" && interjection.EndsWith("—  —", StringComparison.Ordinal))
                            {
                                interjection = interjection.Remove(interjection.Length - 3);
                                if (interjection.EndsWith(Environment.NewLine + "—", StringComparison.Ordinal))
                                    interjection = interjection.Remove(interjection.Length - 1).TrimEnd();
                            }
                            else if (interjection.Remove(0, index) == " —" && interjection.EndsWith("-  —", StringComparison.Ordinal))
                            {
                                interjection = interjection.Remove(interjection.Length - 3);
                                if (interjection.EndsWith(Environment.NewLine + "-", StringComparison.Ordinal))
                                    interjection = interjection.Remove(interjection.Length - 1).TrimEnd();
                            }
                            else if (index == 2 && interjection.StartsWith("-  —", StringComparison.Ordinal))
                            {
                                interjection = interjection.Remove(2, 2);
                            }
                            else if (index == 2 && interjection.StartsWith("- —", StringComparison.Ordinal))
                            {
                                interjection = interjection.Remove(2, 1);
                            }
                            else if (index == 0 && interjection.StartsWith(" —", StringComparison.Ordinal))
                            {
                                interjection = interjection.Remove(0, 2);
                            }
                            else if (index == 0 && interjection.StartsWith('—'))
                            {
                                interjection = interjection.Remove(0, 1);
                            }
                            else if (index > 3 && (interjection.Substring(index - 2) == ".  —" || interjection.Substring(index - 2) == "!  —" || interjection.Substring(index - 2) == "?  —"))
                            {
                                interjection = interjection.Remove(index - 2, 1).Replace("  ", " ");
                            }
                            string pre = string.Empty;
                            if (index > 0)
                                doRepeat = true;

                            bool removeAfter = true;

                            if (index > s.Length)
                            {
                                if (interjection.Length > index - s.Length + 3)
                                {
                                    int subIndex = index - s.Length + 1;
                                    string subTemp = interjection.Substring(subIndex, 3);
                                    if (subTemp == ", !" || subTemp == ", ?" || subTemp == ", .")
                                    {
                                        interjection = interjection.Remove(subIndex, 2);
                                        removeAfter = false;
                                    }
                                }
                                if (removeAfter && interjection.Length > index - s.Length + 2)
                                {
                                    int subIndex = index - s.Length;
                                    string subTemp = interjection.Substring(subIndex, 3);
                                    if (subTemp == ", !" || subTemp == ", ?" || subTemp == ", .")
                                    {
                                        interjection = interjection.Remove(subIndex, 2);
                                        removeAfter = false;
                                    }
                                    else
                                    {
                                        subTemp = interjection.Substring(subIndex);
                                        if (subTemp.StartsWith(", -—", StringComparison.Ordinal))
                                        {
                                            interjection = interjection.Remove(subIndex, 3);
                                            removeAfter = false;
                                        }
                                        else if (subTemp.StartsWith(", --", StringComparison.Ordinal))
                                        {
                                            interjection = interjection.Remove(subIndex, 2);
                                            removeAfter = false;
                                        }
                                        else if (index > 2 && subTemp.StartsWith("-  —", StringComparison.Ordinal))
                                        {
                                            interjection = interjection.Remove(subIndex + 2, 2).Replace("  ", " ");
                                            removeAfter = false;
                                        }
                                    }
                                }
                                if (removeAfter && interjection.Length > index - s.Length + 2)
                                {
                                    int subIndex = index - s.Length + 1;
                                    string subTemp = interjection.Substring(subIndex, 2);
                                    if (subTemp == "-!" || subTemp == "-?" || subTemp == "-.")
                                    {
                                        interjection = interjection.Remove(subIndex, 1);
                                        removeAfter = false;
                                    }
                                    subTemp = interjection.Substring(subIndex);
                                    if (subTemp == " !" || subTemp == " ?" || subTemp == " .")
                                    {
                                        interjection = interjection.Remove(subIndex, 1);
                                        removeAfter = false;
                                    }
                                }
                            }

                            if (index > 3 && index - 2 < interjection.Length)
                            {
                                string subTemp = interjection.Substring(index - 2);
                                if (subTemp.StartsWith(",  —", StringComparison.Ordinal) || subTemp.StartsWith(", —", StringComparison.Ordinal))
                                {
                                    interjection = interjection.Remove(index - 2, 1);
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
                                    if (interjection.StartsWith('-'))
                                        interjection = interjection.Remove(0, 1).Trim();
                                }
                                else if (index == 3 && interjection.StartsWith("<i>-", StringComparison.Ordinal))
                                {
                                    interjection = interjection.Remove(3, 1);
                                }
                                else if (index > 0 && interjection.Length > index)
                                {
                                    pre = text.Substring(0, index);
                                    interjection = interjection.Remove(0, index);
                                    if (interjection.StartsWith('-') && pre.EndsWith('-'))
                                        interjection = interjection.Remove(0, 1);
                                    if (interjection.StartsWith('-') && pre.EndsWith("- ", StringComparison.Ordinal))
                                        interjection = interjection.Remove(0, 1);
                                }

                                if (interjection.StartsWith("..."))
                                {
                                    pre = pre.Trim();
                                }
                                else
                                {
                                    while (interjection.Length > 0 && " ,.?!".Contains(interjection[0]))
                                    {
                                        interjection = interjection.Remove(0, 1);
                                        doRepeat = true;
                                    }
                                }
                                if (interjection.Length > 0 && s[0].ToString(CultureInfo.InvariantCulture) != s[0].ToString(CultureInfo.InvariantCulture).ToLower())
                                {
                                    interjection = char.ToUpper(interjection[0]) + interjection.Substring(1);
                                    doRepeat = true;
                                }

                                if (interjection.StartsWith('-') && pre.EndsWith(' '))
                                    interjection = interjection.Remove(0, 1);

                                if (interjection.StartsWith('—') && pre.EndsWith(','))
                                    pre = pre.TrimEnd(',') + " ";
                                interjection = pre + interjection;
                            }

                            if (interjection.EndsWith(Environment.NewLine + "- ", StringComparison.Ordinal))
                                interjection = interjection.Remove(interjection.Length - 2).TrimEnd();

                            var st = new StripableText(interjection);
                            if (st.StrippedText.Length == 0)
                                return string.Empty;

                            if (interjection.StartsWith('-') && !interjection.Contains(Environment.NewLine) && text.Contains(Environment.NewLine))
                                interjection = interjection.Remove(0, 1).Trim();

                            // TODO: Check if not in _skipSet (optimzie by checking above before going this far)?
                            if (!_skipSet.Contains(interjection)) // O(1)
                                text = interjection;
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

    }
}
