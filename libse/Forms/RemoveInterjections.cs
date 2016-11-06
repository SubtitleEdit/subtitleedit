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

        public RemoveInterjections()
        {
            Load();
        }

        private void Load()
        {
            var interjectionList = new HashSet<string>();
            foreach (var s in Configuration.Settings.Tools.Interjections.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (s.Length > 0)
                {
                    interjectionList.Add(s);
                    var upper = s.ToUpper();
                    interjectionList.Add(upper);
                    var lower = s.ToLower();
                    interjectionList.Add(lower);
                    interjectionList.Add(lower.CapitalizeFirstLetter());
                }
            }
            _interjectionList = new List<string>(interjectionList);
            interjectionList.Clear();
            interjectionList.TrimExcess();
            _interjectionList.Sort(CompareLength);
        }

        public void Reset()
        {
            _interjectionList = null;
        }

        private static int CompareLength(string a, string b)
        {
            return b.Length.CompareTo(a.Length);
        }

        public string Remove(string text)
        {
            if (_interjectionList == null)
                Load();

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
                            string temp = text.Remove(index, s.Length);
                            while (index == 0 && temp.StartsWith("... ", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(3, 1);
                            }
                            while (index == 3 && temp.StartsWith("<i>... ", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(6, 1);
                            }
                            while (index > 2 && (" \r\n".Contains(text.Substring(index - 1, 1))) && temp.Substring(index).StartsWith("... ", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(index + 3, 1);
                            }

                            if (temp.Remove(0, index) == " —" && temp.EndsWith("—  —", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(temp.Length - 3);
                                if (temp.EndsWith(Environment.NewLine + "—", StringComparison.Ordinal))
                                    temp = temp.Remove(temp.Length - 1).TrimEnd();
                            }
                            else if (temp.Remove(0, index) == " —" && temp.EndsWith("-  —", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(temp.Length - 3);
                                if (temp.EndsWith(Environment.NewLine + "-", StringComparison.Ordinal))
                                    temp = temp.Remove(temp.Length - 1).TrimEnd();
                            }
                            else if (index == 2 && temp.StartsWith("-  —", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(2, 2);
                            }
                            else if (index == 2 && temp.StartsWith("- —", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(2, 1);
                            }
                            else if (index == 0 && temp.StartsWith(" —", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(0, 2);
                            }
                            else if (index == 0 && temp.StartsWith('—'))
                            {
                                temp = temp.Remove(0, 1);
                            }
                            else if (index > 3 && (temp.Substring(index - 2) == ".  —" || temp.Substring(index - 2) == "!  —" || temp.Substring(index - 2) == "?  —"))
                            {
                                temp = temp.Remove(index - 2, 1).Replace("  ", " ");
                            }
                            string pre = string.Empty;
                            if (index > 0)
                                doRepeat = true;

                            bool removeAfter = true;

                            if (index > s.Length)
                            {
                                if (temp.Length > index - s.Length + 3)
                                {
                                    int subIndex = index - s.Length + 1;
                                    string subTemp = temp.Substring(subIndex, 3);
                                    if (subTemp == ", !" || subTemp == ", ?" || subTemp == ", .")
                                    {
                                        temp = temp.Remove(subIndex, 2);
                                        removeAfter = false;
                                    }
                                }
                                if (removeAfter && temp.Length > index - s.Length + 2)
                                {
                                    int subIndex = index - s.Length;
                                    string subTemp = temp.Substring(subIndex, 3);
                                    if (subTemp == ", !" || subTemp == ", ?" || subTemp == ", .")
                                    {
                                        temp = temp.Remove(subIndex, 2);
                                        removeAfter = false;
                                    }
                                    else
                                    {
                                        subTemp = temp.Substring(subIndex);
                                        if (subTemp.StartsWith(", -—", StringComparison.Ordinal))
                                        {
                                            temp = temp.Remove(subIndex, 3);
                                            removeAfter = false;
                                        }
                                        else if (subTemp.StartsWith(", --", StringComparison.Ordinal))
                                        {
                                            temp = temp.Remove(subIndex, 2);
                                            removeAfter = false;
                                        }
                                        else if (index > 2 && subTemp.StartsWith("-  —", StringComparison.Ordinal))
                                        {
                                            temp = temp.Remove(subIndex + 2, 2).Replace("  ", " ");
                                            removeAfter = false;
                                        }
                                    }
                                }
                                if (removeAfter && temp.Length > index - s.Length + 2)
                                {
                                    int subIndex = index - s.Length + 1;
                                    string subTemp = temp.Substring(subIndex, 2);
                                    if (subTemp == "-!" || subTemp == "-?" || subTemp == "-.")
                                    {
                                        temp = temp.Remove(subIndex, 1);
                                        removeAfter = false;
                                    }
                                    subTemp = temp.Substring(subIndex);
                                    if (subTemp == " !" || subTemp == " ?" || subTemp == " .")
                                    {
                                        temp = temp.Remove(subIndex, 1);
                                        removeAfter = false;
                                    }
                                }
                            }

                            if (index > 3 && index - 2 < temp.Length)
                            {
                                string subTemp = temp.Substring(index - 2);
                                if (subTemp.StartsWith(",  —", StringComparison.Ordinal) || subTemp.StartsWith(", —", StringComparison.Ordinal))
                                {
                                    temp = temp.Remove(index - 2, 1);
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
                                    if (temp.StartsWith('-'))
                                        temp = temp.Remove(0, 1).Trim();
                                }
                                else if (index == 3 && temp.StartsWith("<i>-", StringComparison.Ordinal))
                                {
                                    temp = temp.Remove(3, 1);
                                }
                                else if (index > 0 && temp.Length > index)
                                {
                                    pre = text.Substring(0, index);
                                    temp = temp.Remove(0, index);
                                    if (temp.StartsWith('-') && pre.EndsWith('-'))
                                        temp = temp.Remove(0, 1);
                                    if (temp.StartsWith('-') && pre.EndsWith("- ", StringComparison.Ordinal))
                                        temp = temp.Remove(0, 1);
                                }

                                if (temp.StartsWith("..."))
                                {
                                    pre = pre.Trim();
                                }
                                else
                                {
                                    while (temp.Length > 0 && " ,.?!".Contains(temp[0]))
                                    {
                                        temp = temp.Remove(0, 1);
                                        doRepeat = true;
                                    }
                                }
                                if (temp.Length > 0 && s[0].ToString(CultureInfo.InvariantCulture) != s[0].ToString(CultureInfo.InvariantCulture).ToLower())
                                {
                                    temp = char.ToUpper(temp[0]) + temp.Substring(1);
                                    doRepeat = true;
                                }

                                if (temp.StartsWith('-') && pre.EndsWith(' '))
                                    temp = temp.Remove(0, 1);

                                if (temp.StartsWith('—') && pre.EndsWith(','))
                                    pre = pre.TrimEnd(',') + " ";
                                temp = pre + temp;
                            }

                            if (temp.EndsWith(Environment.NewLine + "- ", StringComparison.Ordinal))
                                temp = temp.Remove(temp.Length - 2).TrimEnd();

                            var st = new StripableText(temp);
                            if (st.StrippedText.Length == 0)
                                return string.Empty;

                            if (temp.StartsWith('-') && !temp.Contains(Environment.NewLine) && text.Contains(Environment.NewLine))
                                temp = temp.Remove(0, 1).Trim();

                            text = temp;
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
