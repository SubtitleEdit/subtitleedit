using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public class InterjectionRemoveContext
    {
        /// <summary>
        /// True if interjection should be removed only if exists in separated line otherwise false.
        /// </summary>
        public bool OnlySeparatedLines { get; set; }

        /// <summary>
        /// The check list that will be used to check interjections.
        /// </summary>
        public IList<string> Interjections { get; set; }

        /// <summary>
        /// Text from which the interjections will be removed from.
        /// </summary>
        public string Text { get; set; }
    }

    public class RemoveInterjection
    {
        // https://github.com/SubtitleEdit/subtitleedit/issues/1421
        private IList<string> _ignoreList = new List<string>();

        public string Invoke(InterjectionRemoveContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Text))
            {
                return context.Text;
            }

            string text = context.Text;
            string oldText = text;
            bool doRepeat = true;
            while (doRepeat)
            {
                doRepeat = false;
                foreach (string s in context.Interjections)
                {
                    if (text.Contains(s))
                    {
                        var regex = new Regex("\\b" + Regex.Escape(s) + "\\b");
                        var match = regex.Match(text);
                        if (match.Success)
                        {
                            int index = match.Index;
                            string temp = text.Remove(index, s.Length);
                            if (index == 0 && temp.StartsWith("... ", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(0, 4);
                            }

                            if (index == 3 && temp.StartsWith("<i>... ", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(3, 4);
                            }

                            if (index > 2 && " \r\n".Contains(text.Substring(index - 1, 1)) && temp.Substring(index).StartsWith("... ", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(index, 4);
                            }

                            if (index > 4 && temp.Substring(index - 4).StartsWith("\n<i>... ", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(index, 4);
                            }

                            if (temp.Remove(0, index) == " —" && temp.EndsWith("—  —", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(temp.Length - 3);
                                if (temp.EndsWith(Environment.NewLine + "—", StringComparison.Ordinal))
                                {
                                    temp = temp.Remove(temp.Length - 1).TrimEnd();
                                }
                            }
                            else if (temp.Remove(0, index) == " —" && temp.EndsWith("-  —", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(temp.Length - 3);
                                if (temp.EndsWith(Environment.NewLine + "-", StringComparison.Ordinal))
                                {
                                    temp = temp.Remove(temp.Length - 1).TrimEnd();
                                }
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
                            else if (index > 3 && temp.Length == index + 1 && ".!?".Contains(temp[index - 2]) && temp[index - 1] == ' ' && ".!?".Contains(temp[index]))
                            {
                                temp = temp.Remove(index, 1).TrimEnd();
                            }

                            string pre = string.Empty;
                            if (index > 0)
                            {
                                doRepeat = true;
                            }

                            bool removeAfter = true;

                            if (index > 2 && temp.Length > index)
                            {
                                var ending = temp.Substring(index - 2, 3);
                                if (ending == ", ." || ending == ", !" || ending == ", ?" || ending == ", …")
                                {
                                    temp = temp.Remove(index - 2, 2);
                                    removeAfter = false;
                                }
                            }

                            if (removeAfter && index > s.Length)
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
                                    else if (subIndex > 3 && ".!?".Contains(temp.Substring(subIndex - 1, 1)))
                                    {
                                        subTemp = temp.Substring(subIndex);
                                        if (subTemp == " ..." || subTemp.StartsWith(" ..." + Environment.NewLine, StringComparison.InvariantCulture))
                                        {
                                            temp = temp.Remove(subIndex, 4).Trim();
                                            removeAfter = false;
                                        }
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
                                    {
                                        temp = temp.Remove(0, 1).Trim();
                                    }
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
                                    {
                                        temp = temp.Remove(0, 1);
                                    }

                                    if (temp.StartsWith('-') && pre.EndsWith("- ", StringComparison.Ordinal))
                                    {
                                        temp = temp.Remove(0, 1);
                                    }
                                }

                                if (temp.StartsWith("...", StringComparison.Ordinal))
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

                                if (temp.Length > 0 && s[0].ToString(CultureInfo.InvariantCulture) != s[0].ToString(CultureInfo.InvariantCulture).ToLowerInvariant())
                                {
                                    temp = char.ToUpper(temp[0]) + temp.Substring(1);
                                    doRepeat = true;
                                }

                                if (temp.StartsWith('-') && pre.EndsWith(' '))
                                {
                                    temp = temp.Remove(0, 1);
                                }

                                if (temp.StartsWith('—') && pre.EndsWith(','))
                                {
                                    pre = pre.TrimEnd(',') + " ";
                                }

                                temp = pre + temp;
                            }

                            if (temp.EndsWith(Environment.NewLine + "- ", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(temp.Length - 2).TrimEnd();
                            }

                            var st = new StrippableText(temp);
                            if (st.StrippedText.Length == 0)
                            {
                                return string.Empty;
                            }

                            if (temp.StartsWith('-') && !temp.Contains(Environment.NewLine) && text.Contains(Environment.NewLine))
                            {
                                temp = temp.Remove(0, 1).Trim();
                            }

                            text = temp;
                        }
                    }
                }
            }

            var lines = text.SplitToLines();
            if (lines.Count == 2 && text != oldText)
            {
                if (lines[0] == "-" && lines[1] == "-")
                {
                    return string.Empty;
                }

                if (lines[0].Length > 1 && lines[0][0] == '-' && lines[1].Trim() == "-")
                {
                    var oldFirstLine = oldText.SplitToLines()[0];
                    if (context.OnlySeparatedLines && oldFirstLine.Length > 1 && oldFirstLine[0] == '-')
                    {
                        lines[0] = oldFirstLine;
                    }
                    return lines[0].Remove(0, 1).Trim();
                }

                if (lines[1].Length > 1 && lines[1][0] == '-' && lines[0].Trim() == "-")
                {
                    var oldSecondLine = oldText.SplitToLines()[1];
                    if (context.OnlySeparatedLines && oldSecondLine.Length > 1 && oldSecondLine[0] == '-')
                    {
                        lines[1] = oldSecondLine;
                    }
                    return lines[1].Remove(0, 1).Trim();
                }

                if (lines[1].Length > 4 && lines[1].StartsWith("<i>-", StringComparison.Ordinal) && lines[0].Trim() == "-")
                {
                    var oldSecondLine = oldText.SplitToLines()[1];
                    if (context.OnlySeparatedLines && oldSecondLine.Length > 1 && oldSecondLine.StartsWith("<i>-", StringComparison.Ordinal))
                    {
                        lines[1] = oldSecondLine;
                    }
                    return "<i>" + lines[1].Remove(0, 4).Trim();
                }

                if (lines[0].Length > 1 && lines[1] == "-" || lines[1] == "." || lines[1] == "!" || lines[1] == "?")
                {
                    var oldFirstLine = oldText.SplitToLines()[0];
                    if (context.OnlySeparatedLines && oldFirstLine.Length > 1 && lines[1] == "-" || lines[1] == "." || lines[1] == "!" || lines[1] == "?")
                    {
                        lines[0] = oldFirstLine;
                    }

                    if (lines[0].StartsWith('-') && oldText.Contains(Environment.NewLine + "-"))
                    {
                        lines[0] = lines[0].Remove(0, 1);
                    }

                    return lines[0].Trim();
                }

                var noTags0 = HtmlUtil.RemoveHtmlTags(lines[0]).Trim();
                var noTags1 = HtmlUtil.RemoveHtmlTags(lines[1]).Trim();
                if (noTags0 == "-")
                {
                    if (noTags1 == noTags0)
                    {
                        return string.Empty;
                    }

                    if (lines[1].Length > 1 && lines[1][0] == '-')
                    {
                        return lines[1].Remove(0, 1).Trim();
                    }

                    if (lines[1].Length > 4 && lines[1].StartsWith("<i>-", StringComparison.Ordinal))
                    {
                        return "<i>" + lines[1].Remove(0, 4).Trim();
                    }

                    return lines[1];
                }

                if (noTags1 == "-")
                {
                    if (lines[0].Length > 1 && lines[0][0] == '-')
                    {
                        return lines[0].Remove(0, 1).Trim();
                    }

                    if (lines[0].Length > 4 && lines[0].StartsWith("<i>-", StringComparison.Ordinal))
                    {
                        if (!lines[0].Contains("</i>") && lines[1].Contains("</i>"))
                        {
                            return "<i>" + lines[0].Remove(0, 4).Trim() + "</i>";
                        }

                        return "<i>" + lines[0].Remove(0, 4).Trim();
                    }

                    return lines[0];
                }
            }

            if (lines.Count == 2)
            {
                if (string.IsNullOrWhiteSpace(lines[1].RemoveChar('.').RemoveChar('?').RemoveChar('!').RemoveChar('-').RemoveChar('—')))
                {
                    text = lines[0];
                    lines = text.SplitToLines();
                }
                else if (string.IsNullOrWhiteSpace(lines[0].RemoveChar('.').RemoveChar('?').RemoveChar('!').RemoveChar('-').RemoveChar('—')))
                {
                    text = lines[1];
                    lines = text.SplitToLines();
                }
            }

            if (lines.Count == 1 && text != oldText && Utilities.GetNumberOfLines(oldText) == 2)
            {
                if ((oldText.StartsWith('-') || oldText.StartsWith("<i>-", StringComparison.Ordinal)) &&
                    (oldText.Contains("." + Environment.NewLine) || oldText.Contains(".</i>" + Environment.NewLine) ||
                     oldText.Contains("!" + Environment.NewLine) || oldText.Contains("!</i>" + Environment.NewLine) ||
                     oldText.Contains("?" + Environment.NewLine) || oldText.Contains("?</i>" + Environment.NewLine)))
                {
                    if (text.StartsWith("<i>-", StringComparison.Ordinal))
                    {
                        text = "<i>" + text.Remove(0, 4).TrimStart();
                    }
                    else
                    {
                        text = text.TrimStart('-').TrimStart();
                    }
                }
                else if ((oldText.Contains(Environment.NewLine + "-") || oldText.Contains(Environment.NewLine + "<i>-")) &&
                         (oldText.Contains("." + Environment.NewLine) || oldText.Contains(".</i>" + Environment.NewLine) ||
                          oldText.Contains("!" + Environment.NewLine) || oldText.Contains("!</i>" + Environment.NewLine) ||
                          oldText.Contains("?" + Environment.NewLine) || oldText.Contains("?</i>" + Environment.NewLine)))
                {
                    if (text.StartsWith("<i>-", StringComparison.Ordinal))
                    {
                        text = "<i>" + text.Remove(0, 4).TrimStart();
                    }
                    else
                    {
                        text = text.TrimStart('-').TrimStart();
                    }
                }
            }

            if (oldText != text)
            {
                text = text.Replace(Environment.NewLine + "<i>" + Environment.NewLine, Environment.NewLine + "<i>");
                text = text.Replace(Environment.NewLine + "</i>" + Environment.NewLine, "</i>" + Environment.NewLine);
                if (text.StartsWith("<i>" + Environment.NewLine, StringComparison.Ordinal))
                {
                    text = text.Remove(3, Environment.NewLine.Length);
                }

                if (text.EndsWith(Environment.NewLine + "</i>", StringComparison.Ordinal))
                {
                    text = text.Remove(text.Length - (Environment.NewLine.Length + 4), Environment.NewLine.Length);
                }

                text = text.Replace(Environment.NewLine + "</i>" + Environment.NewLine, "</i>" + Environment.NewLine);

                if (context.OnlySeparatedLines)
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        return text;
                    }

                    var oldLines = oldText.SplitToLines();
                    var newLines = text.SplitToLines();
                    if (oldLines.Count == 2 && newLines.Count == 1 &&
                        (oldLines[0] == newLines[0] || oldLines[1] == newLines[0]))
                    {
                        return text;
                    }

                    return oldText;
                }
            }

            if (!oldText.Contains("  "))
            {
                while (text.Contains("  "))
                {
                    text = text.Replace("  ", " ");
                }
            }

            return text;
        }
    }
}