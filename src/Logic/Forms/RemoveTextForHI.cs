using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.Forms
{
    public class RemoveTextForHI
    {
        public RemoveTextForHISettings Settings { get; set; }

        public List<int> Warnings;
        public int WarningIndex;
        private List<string> _interjectionList;

        public RemoveTextForHI(RemoveTextForHISettings removeTextForHISettings)
        {
            Settings = removeTextForHISettings;
        }

        public void ResetInterjections()
        {
            _interjectionList = null;
        }

        public string RemoveHearImpairedtagsInsideLine(string newText)
        {
            for (int i = 6; i < newText.Length; i++)
            {
                var s = newText.Substring(i);
                if (s.Length > 2 && (s.StartsWith('.') || s.StartsWith('!') || s.StartsWith('?')))
                {
                    var pre = string.Empty;

                    s = s.Remove(0, 1);
                    if (s.StartsWith(' '))
                    {
                        if (s.StartsWith(" <i>", StringComparison.Ordinal))
                            pre = " <i>";
                        else
                            pre = " ";
                    }
                    else if (s.StartsWith("<i>", StringComparison.Ordinal))
                        pre = "<i>";
                    else if (s.StartsWith("</i>", StringComparison.Ordinal))
                        pre = "</i>";

                    if (pre.Length > 0)
                    {
                        s = s.Remove(0, pre.Length);
                        if (s.StartsWith(' ') && s.Length > 1)
                        {
                            pre += " ";
                            s = s.Remove(0, 1);
                        }

                        if (HasHearImpariedTagsAtStart(s))
                        {
                            s = RemoveStartEndTags(s);
                            newText = newText.Substring(0, i + 1) + pre + " " + s;
                            newText = newText.Replace("<i></i>", string.Empty);
                            newText = newText.Replace("<i> </i>", " ").FixExtraSpaces();
                        }
                    }
                }
            }
            return newText;
        }

        public string RemoveColon(string text)
        {
            if (!Settings.RemoveTextBeforeColon || text.IndexOf(':') < 0)
                return text;

            string preAssTag = string.Empty;
            if (text.StartsWith("{\\", StringComparison.Ordinal) && text.IndexOf('}') > 0)
            {
                int indexOfEndBracket = text.IndexOf('}') + 1;
                preAssTag = text.Substring(0, indexOfEndBracket);
                text = text.Remove(0, indexOfEndBracket).TrimStart();
            }

            // House 7x01 line 52: and she would like you to do three things:
            // Okay or remove???
            var noTagText = HtmlUtil.RemoveHtmlTags(text);
            if (noTagText.IndexOf(':') > 0 && noTagText.IndexOf(':') == noTagText.Length - 1 && noTagText != noTagText.ToUpper() && noTagText.Length > 10)
                return text;

            string newText = string.Empty;
            var lines = text.Trim().SplitToLines();
            int noOfNames = 0;
            int count = 0;
            bool removedInFirstLine = false;
            bool removedInSecondLine = false;
            foreach (string line in lines)
            {
                int indexOfColon = line.IndexOf(':');
                if (indexOfColon > 0 && IsNotInsideBrackets(text, indexOfColon))
                {
                    var pre = line.Substring(0, indexOfColon);
                    var noTagPre = HtmlUtil.RemoveHtmlTags(pre, true);
                    if (Settings.RemoveTextBeforeColonOnlyUppercase && noTagPre != noTagPre.ToUpper())
                    {
                        newText = (newText + Environment.NewLine + line).Trim();
                    }
                    else
                    {
                        var st = new StripableText(pre);
                        if (count == 1 && newText.Length > 1 && removedInFirstLine && Utilities.CountTagInText(line, ':') == 1 &&
                            ".?!".IndexOf(newText[newText.Length - 1]) < 0 && newText.LineEndsWithHtmlTag(true) &&
                            line != line.ToUpper())
                        {
                            if (pre.Contains("<i>") && line.Contains("</i>"))
                                newText = newText + Environment.NewLine + "<i>" + line;
                            else if (pre.Contains("<b>") && line.Contains("</b>"))
                                newText = newText + Environment.NewLine + "<b>" + line;
                            else if (pre.Contains("<u>") && line.Contains("</u>"))
                                newText = newText + Environment.NewLine + "<u>" + line;
                            else if (pre.Contains('[') && line.Contains(']'))
                                newText = newText + Environment.NewLine + "[" + line;
                            else if (pre.Contains('(') && line.EndsWith(')'))
                                newText = newText + Environment.NewLine + "(" + line;
                            else
                                newText = newText + Environment.NewLine + line;
                        }
                        else if (count == 1 && newText.Length > 1 && indexOfColon > 15 && line.Substring(0, indexOfColon).Contains(' ') && Utilities.CountTagInText(line, ':') == 1 &&
                            ".?!".IndexOf(newText[newText.Length - 1]) < 0 && newText.LineEndsWithHtmlTag(true) &&
                            line != line.ToUpper())
                        {
                            if (pre.Contains("<i>") && line.Contains("</i>"))
                                newText = newText + Environment.NewLine + "<i>" + line;
                            else if (pre.Contains("<b>") && line.Contains("</b>"))
                                newText = newText + Environment.NewLine + "<b>" + line;
                            else if (pre.Contains("<u>") && line.Contains("</u>"))
                                newText = newText + Environment.NewLine + "<u>" + line;
                            else if (pre.Contains('[') && line.Contains(']'))
                                newText = newText + Environment.NewLine + "[" + line;
                            else if (pre.Contains('(') && line.EndsWith(')'))
                                newText = newText + Environment.NewLine + "(" + line;
                            else
                                newText = newText + Environment.NewLine + line;
                        }
                        else if (Utilities.CountTagInText(line, ':') == 1)
                        {
                            bool remove = true;
                            if (indexOfColon > 0 && indexOfColon < line.Length - 1)
                            {
                                remove = !Utilities.IsBetweenNumbers(line, indexOfColon);
                            }

                            if (!DoRemove(pre))
                                remove = false;

                            if (remove && Settings.ColonSeparateLine)
                            {
                                if (indexOfColon == line.Length - 1 || line.Substring(indexOfColon + 1).StartsWith(Environment.NewLine, StringComparison.Ordinal))
                                    remove = true;
                                else
                                    remove = false;
                            }

                            if (remove)
                            {
                                var content = line.Substring(indexOfColon + 1).Trim();
                                if (content.Length > 0)
                                {
                                    if (pre.Contains("<i>") && content.Contains("</i>"))
                                        newText = newText + Environment.NewLine + "<i>" + content;
                                    else if (pre.Contains("<b>") && content.Contains("</b>"))
                                        newText = newText + Environment.NewLine + "<b>" + content;
                                    else if (pre.Contains('[') && content.Contains(']'))
                                        newText = newText + Environment.NewLine + "[" + content;
                                    else if (pre.Contains('(') && content.EndsWith(')'))
                                        newText = newText + Environment.NewLine + "(" + content;
                                    else
                                        newText = newText + Environment.NewLine + content;

                                    if (count == 0)
                                        removedInFirstLine = true;
                                    else if (count == 1)
                                        removedInSecondLine = true;
                                }
                                newText = newText.Trim();

                                if (text.StartsWith('(') && newText.EndsWith(')') && !newText.Contains('('))
                                    newText = newText.TrimEnd(')');
                                else if (text.StartsWith('[') && newText.EndsWith(']') && !newText.Contains('['))
                                    newText = newText.TrimEnd(']');
                                else if (newText.EndsWith("</i>", StringComparison.Ordinal) && text.StartsWith("<i>", StringComparison.Ordinal) && !newText.StartsWith("<i>", StringComparison.Ordinal))
                                    newText = "<i>" + newText;
                                else if (newText.EndsWith("</b>", StringComparison.Ordinal) && text.StartsWith("<b>", StringComparison.Ordinal) && !newText.StartsWith("<b>", StringComparison.Ordinal))
                                    newText = "<b>" + newText;
                                else if (newText.EndsWith("</u>", StringComparison.Ordinal) && text.StartsWith("<u>", StringComparison.Ordinal) && !newText.StartsWith("<u>", StringComparison.Ordinal))
                                    newText = "<u>" + newText;

                                if (!IsHIDescription(st.StrippedText))
                                    noOfNames++;
                            }
                            else
                            {
                                newText = (newText + Environment.NewLine + line).Trim();
                                if (newText.EndsWith("</i>", StringComparison.Ordinal) && text.StartsWith("<i>", StringComparison.Ordinal) && !newText.StartsWith("<i>", StringComparison.Ordinal))
                                    newText = "<i>" + newText;
                                else if (newText.EndsWith("</b>", StringComparison.Ordinal) && text.StartsWith("<b>", StringComparison.Ordinal) && !newText.StartsWith("<b>", StringComparison.Ordinal))
                                    newText = "<b>" + newText;
                                else if ((newText.EndsWith("</u>", StringComparison.Ordinal) && text.StartsWith("<u>", StringComparison.Ordinal) && !newText.StartsWith("<u>", StringComparison.Ordinal)))
                                    newText = "<u>" + newText;
                            }
                        }
                        else
                        {
                            string s2 = line;
                            for (int k = 0; k < 2; k++)
                            {
                                if (s2.Contains(':'))
                                {
                                    int colonIndex = s2.IndexOf(':');
                                    string start = s2.Substring(0, colonIndex);

                                    if (!Settings.RemoveTextBeforeColonOnlyUppercase || start == start.ToUpper())
                                    {
                                        int endIndex = start.LastIndexOfAny(new[] { '.', '!', '?' });
                                        if (colonIndex > 0 && colonIndex < s2.Length - 1)
                                        {
                                            if (char.IsDigit(s2[colonIndex - 1]) && char.IsDigit(s2[colonIndex + 1]))
                                                endIndex = 0;
                                        }
                                        if (endIndex < 0)
                                            s2 = s2.Remove(0, colonIndex - endIndex);
                                        else if (endIndex > 0)
                                            s2 = s2.Remove(endIndex + 1, colonIndex - endIndex);
                                    }

                                    if (count == 0)
                                        removedInFirstLine = true;
                                    else if (count == 1)
                                        removedInSecondLine = true;
                                }
                            }
                            newText = newText + Environment.NewLine + s2;
                            newText = newText.Trim();
                        }
                    }
                }
                else
                {
                    newText = (newText + Environment.NewLine + line).Trim();

                    if (newText.EndsWith("</i>", StringComparison.Ordinal) && text.StartsWith("<i>", StringComparison.Ordinal) && !newText.StartsWith("<i>", StringComparison.Ordinal))
                        newText = "<i>" + newText;
                    else if (newText.EndsWith("</b>", StringComparison.Ordinal) && text.StartsWith("<b>", StringComparison.Ordinal) && !newText.StartsWith("<b>", StringComparison.Ordinal))
                        newText = "<b>" + newText;
                }
                count++;
            }
            newText = newText.Trim();
            if (noOfNames > 0 && Utilities.GetNumberOfLines(newText) == 2)
            {
                int indexOfDialogChar = newText.IndexOf('-');
                bool insertDash = true;
                var arr = newText.SplitToLines();
                if (arr.Length == 2 && arr[0].Length > 1 && arr[1].Length > 1)
                {
                    string arr0 = new StripableText(arr[0]).StrippedText;
                    string arr1 = new StripableText(arr[1]).StrippedText;

                    //line continuation?
                    if (arr0.Length > 0 && arr1.Length > 1 && (Utilities.LowercaseLetters + ",").Contains(arr0.Substring(arr0.Length - 1), StringComparison.Ordinal) &&
                        Utilities.LowercaseLetters.Contains(arr1[0]))
                    {
                        if (new StripableText(arr[1]).Pre.Contains("...") == false)
                            insertDash = false;
                    }

                    string tempArr0QuoteTrimmed = arr[0].TrimEnd('"');
                    if (arr0.Length > 0 && arr1.Length > 1 &&
                        !(tempArr0QuoteTrimmed.EndsWith('.') || tempArr0QuoteTrimmed.EndsWith('!') || tempArr0QuoteTrimmed.EndsWith('?') || tempArr0QuoteTrimmed.EndsWith("</i>", StringComparison.Ordinal)) &&
                        !(new StripableText(arr[1]).Pre.Contains('-')))
                    {
                        insertDash = false;
                    }

                    if (removedInFirstLine && !removedInSecondLine && !text.StartsWith('-') && !text.StartsWith("<i>-", StringComparison.Ordinal))
                    {
                        if (!insertDash || (!arr[1].StartsWith('-') && !arr[1].StartsWith("<i>-", StringComparison.Ordinal)))
                            insertDash = false;
                    }
                }

                if (insertDash)
                {
                    if (indexOfDialogChar < 0 || indexOfDialogChar > 4)
                    {
                        var st = new StripableText(newText, string.Empty, string.Empty);
                        newText = st.Pre + "- " + st.StrippedText + st.Post;
                    }

                    int indexOfNewLine = newText.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                    string second = newText.Substring(indexOfNewLine).Trim();
                    indexOfDialogChar = second.IndexOf('-');
                    if (indexOfDialogChar < 0 || indexOfDialogChar > 6)
                    {
                        var st = new StripableText(second, String.Empty, String.Empty);
                        second = st.Pre + "- " + st.StrippedText + st.Post;
                        newText = newText.Remove(indexOfNewLine) + Environment.NewLine + second;
                    }
                }
            }
            else if (!newText.Contains(Environment.NewLine) && newText.Contains('-'))
            {
                var st = new StripableText(newText);
                if (st.Pre.Contains('-'))
                    newText = st.Pre.Replace("-", string.Empty) + st.StrippedText + st.Post;
            }
            else if (Utilities.GetNumberOfLines(newText) == 2 && removedInFirstLine == false && removedInSecondLine)
            {
                string noTags = HtmlUtil.RemoveHtmlTags(newText, true).Trim();
                bool insertDash = noTags.StartsWith('-') && Utilities.CountTagInText(noTags, '-') == 1;
                if (insertDash)
                {
                    if (newText.Contains(Environment.NewLine + "<i>"))
                        newText = newText.Replace(Environment.NewLine + "<i>", Environment.NewLine + "<i>- ");
                    else
                        newText = newText.Replace(Environment.NewLine, Environment.NewLine + "- ");
                }
            }
            if (text.Contains("<i>", StringComparison.Ordinal) && !newText.Contains("<i>", StringComparison.Ordinal) && newText.EndsWith("</i>", StringComparison.Ordinal))
                newText = "<i>" + newText;

            if (string.IsNullOrWhiteSpace(newText))
                return string.Empty;

            return preAssTag + newText;
        }

        private bool IsNotInsideBrackets(string text, int colonIdx)
        {
            // <i>♪ (THE CAPITOLS: "COOL JERK") ♪</i>
            var bIdx = text.IndexOfAny(new[] { '(', '[' });

            if (bIdx >= 0 && bIdx < colonIdx)
            {
                char closeType = text[bIdx] == '(' ? ')' : ']';
                var nIdx = text.IndexOf(closeType, bIdx + 1);
                if (nIdx > colonIdx)
                {
                    return false;
                }
            }
            return true;
        }

        private bool DoRemove(string pre)
        {
            // Skip these: Barry, remember: She cannot; http://google.com; Improved by: ...
            if (pre.IndexOfAny(new[] { "Previously on", "Improved by", "http", ", " }, StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            // Okay! Narrator: Hello!
            if (pre.IndexOfAny(new[] { '!', '?' }) > 0)
                return false;

            return true;
        }

        public string RemoveTextFromHearImpaired(string text)
        {
            if (Settings.RemoveWhereContains && Settings.RemoveIfTextContains.Length > 0 && text.Contains(Settings.RemoveIfTextContains))
            {
                return string.Empty;
            }

            string oldText = text;
            text = RemoveColon(text);
            string pre = " >-\"'‘`´♪¿¡.…—";
            string post = " -\"'`´♪.!?:…—";
            if (Settings.RemoveTextBetweenCustomTags)
            {
                pre = pre.Replace(Settings.CustomStart, string.Empty);
                post = post.Replace(Settings.CustomEnd, string.Empty);
            }
            var st = new StripableText(text, pre, post);
            var sb = new StringBuilder();
            var parts = st.StrippedText.Trim().SplitToLines();
            int lineNumber = 0;
            bool removedDialogInFirstLine = false;
            int noOfNamesRemoved = 0;
            int noOfNamesRemovedNotInLineOne = 0;
            foreach (string s in parts)
            {
                var stSub = new StripableText(s, pre, post);
                string tempStrippedtext = stSub.StrippedText;
                if (lineNumber == parts.Length - 1 && st.Post.Contains('?'))
                    tempStrippedtext += "?";
                else if (stSub.Post.Contains('?'))
                    tempStrippedtext += "?";
                if (!StartAndEndsWithHearImpariedTags(tempStrippedtext))
                {
                    if (removedDialogInFirstLine && stSub.Pre.Contains("- ", StringComparison.Ordinal))
                        stSub.Pre = stSub.Pre.Replace("- ", string.Empty);

                    string newText = stSub.StrippedText;

                    newText = RemoveHearImpairedTags(newText);

                    if (stSub.StrippedText.Length - newText.Length > 2)
                    {
                        string removedText = GetRemovedString(stSub.StrippedText, newText);
                        if (!IsHIDescription(removedText))
                        {
                            noOfNamesRemoved++;
                            if (lineNumber > 0)
                                noOfNamesRemovedNotInLineOne++;
                        }
                    }
                    sb.AppendLine(stSub.Pre + newText + stSub.Post);
                }
                else
                {
                    if (!IsHIDescription(stSub.StrippedText))
                    {
                        noOfNamesRemoved++;
                        if (lineNumber > 0)
                            noOfNamesRemovedNotInLineOne++;
                    }

                    if (st.Pre.Contains("- ") && lineNumber == 0)
                    {
                        st.Pre = st.Pre.Replace("- ", string.Empty);
                        removedDialogInFirstLine = true;
                    }
                    else if (st.Pre == "-" && lineNumber == 0)
                    {
                        st.Pre = string.Empty;
                        removedDialogInFirstLine = true;
                    }

                    if (st.Pre.Contains("<i>") && stSub.Post.Contains("</i>"))
                        st.Pre = st.Pre.Replace("<i>", string.Empty);

                    if (s.Contains("<i>") && !s.Contains("</i>") && st.Post.Contains("</i>"))
                        st.Post = st.Post.Replace("</i>", string.Empty);
                }
                lineNumber++;
            }

            text = st.Pre + sb.ToString().Trim() + st.Post;
            text = text.Replace("  ", " ").Trim();
            text = text.Replace("<i></i>", string.Empty);
            text = text.Replace("<i> </i>", " ");
            text = text.Replace("<b></b>", string.Empty);
            text = text.Replace("<b> </b>", " ");
            text = RemoveEmptyFontTag(text);
            text = text.Replace("  ", " ").Trim();
            text = RemoveColon(text);
            text = RemoveLineIfAllUppercase(text);
            text = RemoveHearImpairedtagsInsideLine(text);
            if (Settings.RemoveInterjections)
                text = RemoveInterjections(text);

            st = new StripableText(text, " >-\"'‘`´♪¿¡.…—", " -\"'`´♪.!?:…—");
            text = st.StrippedText;
            if (StartAndEndsWithHearImpariedTags(text))
            {
                text = RemoveStartEndTags(text);
            }

            text = RemoveHearImpairedTags(text);

            // fix 3 lines to two liners - if only two lines
            if (noOfNamesRemoved >= 1 && Utilities.GetNumberOfLines(text) == 3)
            {
                string[] a = HtmlUtil.RemoveHtmlTags(text).Replace(" ", string.Empty).Split(new[] { '!', '?', '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (a.Length == 2)
                {
                    var temp = new StripableText(text);
                    temp.StrippedText = temp.StrippedText.Replace(Environment.NewLine, " ");
                    int splitIndex = temp.StrippedText.LastIndexOf('!');
                    if (splitIndex < 0)
                        splitIndex = temp.StrippedText.LastIndexOf('?');
                    if (splitIndex < 0)
                        splitIndex = temp.StrippedText.LastIndexOf('.');
                    if (splitIndex > 0)
                    {
                        text = temp.Pre + temp.StrippedText.Insert(splitIndex + 1, Environment.NewLine) + temp.Post;
                    }
                }
            }

            if (!text.StartsWith('-') && noOfNamesRemoved >= 1 && Utilities.GetNumberOfLines(text) == 2)
            {
                string[] arr = text.SplitToLines();
                string part0 = arr[0].Trim().Replace("</i>", string.Empty).Trim();
                if (!part0.EndsWith(',') && (!part0.EndsWith('-') || noOfNamesRemovedNotInLineOne > 0))
                {
                    if (part0.Length > 0 && @".!?".Contains(part0[part0.Length - 1]))
                    {
                        if (noOfNamesRemovedNotInLineOne > 0)
                        {
                            if (!st.Pre.Contains('-'))
                                text = "- " + text.Replace(Environment.NewLine, Environment.NewLine + "- ");
                            if (!text.Contains(Environment.NewLine + "-") && !text.Contains(Environment.NewLine + "<i>-"))
                                text = text.Replace(Environment.NewLine, Environment.NewLine + "- ");
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(text) || (st.Pre.Contains('♪') || st.Post.Contains('♪')))
                text = st.Pre + text + st.Post;

            if (oldText.TrimStart().StartsWith("- ", StringComparison.Ordinal) && text != null && !text.Contains(Environment.NewLine) &&
                (oldText.Contains(Environment.NewLine + "- ", StringComparison.Ordinal) ||
                 oldText.Contains(Environment.NewLine + " - ", StringComparison.Ordinal) ||
                 oldText.Contains(Environment.NewLine + "<i>- ", StringComparison.Ordinal) ||
                 oldText.Contains(Environment.NewLine + "<i> - ", StringComparison.Ordinal)))
            {
                text = text.TrimStart().TrimStart('-').TrimStart();
            }
            if (oldText.TrimStart().StartsWith("-", StringComparison.Ordinal) &&
                !oldText.TrimStart().StartsWith("--", StringComparison.Ordinal) &&
                text != null && !text.Contains(Environment.NewLine) &&
                (oldText.Contains(Environment.NewLine + "-", StringComparison.Ordinal) && !oldText.Contains(Environment.NewLine + "--", StringComparison.Ordinal) ||
                 oldText.Contains(Environment.NewLine + " - ", StringComparison.Ordinal) ||
                 oldText.Contains(Environment.NewLine + "<i>- ", StringComparison.Ordinal) ||
                 oldText.Contains(Environment.NewLine + "<i> - ", StringComparison.Ordinal)))
            {
                text = text.TrimStart().TrimStart('-').TrimStart();
            }

            if (oldText.TrimStart().StartsWith("<i>- ", StringComparison.Ordinal) &&
                text != null && text.StartsWith("<i>- ", StringComparison.Ordinal) && !text.Contains(Environment.NewLine, StringComparison.Ordinal) &&
                (oldText.Contains(Environment.NewLine + "- ", StringComparison.Ordinal) ||
                 oldText.Contains(Environment.NewLine + " - ", StringComparison.Ordinal) ||
                 oldText.Contains(Environment.NewLine + "<i>- ", StringComparison.Ordinal) ||
                 oldText.Contains(Environment.NewLine + "<i> - ", StringComparison.Ordinal)))
            {
                text = text.Remove(3, 2);
            }

            if (text != null && !text.Contains(Environment.NewLine, StringComparison.Ordinal) &&
                (oldText.Contains(':') && !text.Contains(':') ||
                 oldText.Contains('[') && !text.Contains('[') ||
                 oldText.Contains('(') && !text.Contains('(') ||
                 oldText.Contains('{') && !text.Contains('{')) &&
                (oldText.Contains(Environment.NewLine + "- ", StringComparison.Ordinal) ||
                 oldText.Contains(Environment.NewLine + " - ", StringComparison.Ordinal) ||
                 oldText.Contains(Environment.NewLine + "<i>- ", StringComparison.Ordinal) ||
                 oldText.Contains(Environment.NewLine + "<i> - ", StringComparison.Ordinal)))
            {
                text = text.TrimStart().TrimStart('-').TrimStart();
            }

            if (oldText != text)
            {
                // insert spaces before "-"
                text = text.Replace(Environment.NewLine + "- <i>", Environment.NewLine + "<i>- ");
                text = text.Replace(Environment.NewLine + "-<i>", Environment.NewLine + "<i>- ");
                if (text.StartsWith('-') && text.Length > 2 && text[1] != ' ' && text[1] != '-')
                    text = text.Insert(1, " ");
                if (text.StartsWith("<i>-", StringComparison.Ordinal) && text.Length > 5 && text[4] != ' ' && text[4] != '-')
                    text = text.Insert(4, " ");
                if (text.Contains(Environment.NewLine + "-", StringComparison.Ordinal))
                {
                    int index = text.IndexOf(Environment.NewLine + "-", StringComparison.Ordinal);
                    if (index + 4 < text.Length && text[index + Environment.NewLine.Length + 1] != ' ' && text[index + Environment.NewLine.Length + 1] != '-')
                        text = text.Insert(index + Environment.NewLine.Length + 1, " ");
                }
                if (text.Contains(Environment.NewLine + "<i>-", StringComparison.Ordinal))
                {
                    int index = text.IndexOf(Environment.NewLine + "<i>-", StringComparison.Ordinal);
                    if (index + 5 < text.Length && text[index + Environment.NewLine.Length + 4] != ' ' && text[index + Environment.NewLine.Length + 4] != '-')
                        text = text.Insert(index + Environment.NewLine.Length + 4, " ");
                }
            }
            return text.Trim();
        }

        private string RemoveEmptyFontTag(string text)
        {
            int indexOfStartFont = text.IndexOf("<font ", StringComparison.OrdinalIgnoreCase);
            if (indexOfStartFont < 0)
                return text;

            int indexOfEndFont = text.IndexOf("</font>", StringComparison.OrdinalIgnoreCase);
            if (indexOfEndFont > 0 && indexOfStartFont < indexOfEndFont)
            {
                int startTagBefore = text.Substring(0, indexOfEndFont).LastIndexOf('<');
                string lastTwo = text.Substring(indexOfEndFont - 2, 2);
                if (startTagBefore == indexOfStartFont && lastTwo.TrimEnd().EndsWith('>'))
                {
                    text = text.Remove(indexOfStartFont, indexOfEndFont + "</font>".Length - indexOfStartFont);
                    if (lastTwo.EndsWith(' '))
                        text = text.Insert(indexOfStartFont, " ");
                    text = text.Replace("  ", " ");
                }
            }
            return text;
        }

        private void AddWarning()
        {
            if (Warnings == null || WarningIndex < 0)
                return;

            Warnings.Add(WarningIndex);
        }

        private bool IsHIDescription(string text)
        {
            text = text.Trim(' ', '(', ')', '[', ']', '?', '{', '}');
            text = text.ToLower();

            if (text.Trim().Replace("mr. ", string.Empty).Replace("mrs. ", string.Empty).Replace("dr. ", string.Empty).Contains(' '))
                AddWarning();

            if (text == "cackles" ||
                text == "cheers" ||
                text == "clears throat" ||
                text == "chitters" ||
                text == "chuckles" ||
                text == "exclaims" ||
                text == "exhales" ||
                text == "gasps" ||
                text == "grunts" ||
                text == "groans" ||
                text == "growls" ||
                text == "explosion" ||
                text == "laughs" ||
                text == "noise" ||
                text.EndsWith("on tv", StringComparison.Ordinal) ||
                text.StartsWith("engine ", StringComparison.Ordinal) ||
                text == "roars" ||
                text == "scoff" ||
                text == "screeches" ||
                text.EndsWith("shatters", StringComparison.Ordinal) ||
                text == "shouts" ||
                text == "shrieks" ||
                text == "sigh" ||
                text == "sighs" ||
                text == "snores" ||
                text == "stutters" ||
                text == "thuds" ||
                text == "trumpets" ||
                text == "whispers" ||
                text == "whisper" ||
                text == "whistles" ||
                text.EndsWith("ing", StringComparison.Ordinal))
                return true;
            return false;
        }

        private static string GetRemovedString(string oldText, string newText)
        {
            oldText = oldText.ToLower();
            newText = newText.ToLower();
            int start = oldText.IndexOf(newText, StringComparison.Ordinal);
            string result;
            if (start > 0)
                result = oldText.Substring(0, oldText.Length - newText.Length);
            else
                result = oldText.Substring(newText.Length);
            result = result.Trim(' ', '(', ')', '[', ']', '?', '{', '}');
            return result;
        }

        private static int CompareLength(string a, string b)
        {
            return b.Length.CompareTo(a.Length);
        }

        public string RemoveInterjections(string text)
        {
            string oldText = text;

            var arr = Configuration.Settings.Tools.Interjections.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (_interjectionList == null)
            {
                _interjectionList = new List<string>();
                foreach (string s in arr)
                {
                    if (s.Length > 0)
                    {
                        if (!_interjectionList.Contains(s))
                            _interjectionList.Add(s);

                        string lower = s.ToLower();
                        if (!_interjectionList.Contains(lower))
                            _interjectionList.Add(lower);

                        string upper = s.ToUpper();
                        if (!_interjectionList.Contains(upper))
                            _interjectionList.Add(upper);

                        string pascalCasing = char.ToUpper(s[0]) + s.Substring(1);
                        if (!_interjectionList.Contains(pascalCasing))
                            _interjectionList.Add(pascalCasing);
                    }
                }
                _interjectionList.Sort(CompareLength);
            }

            bool doRepeat = true;
            while (doRepeat)
            {
                doRepeat = false;
                foreach (string s in _interjectionList)
                {
                    if (text.Contains(s))
                    {
                        var regex = new Regex("\\b" + s + "\\b");
                        var match = regex.Match(text);
                        if (match.Success)
                        {
                            int index = match.Index;
                            string temp = text.Remove(index, s.Length);
                            if (temp.Remove(0, index) == " —" && temp.EndsWith("—  —"))
                            {
                                temp = temp.TrimEnd('—').TrimEnd();
                                if (temp.TrimEnd().EndsWith(Environment.NewLine + "—"))
                                    temp = temp.TrimEnd().TrimEnd('—').TrimEnd();
                            }
                            else if (temp.Remove(0, index) == " —" && temp.EndsWith("-  —"))
                            {
                                temp = temp.TrimEnd('—').TrimEnd();
                                if (temp.TrimEnd().EndsWith(Environment.NewLine + "-"))
                                    temp = temp.TrimEnd().TrimEnd('-').TrimEnd();
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
                            string pre = string.Empty;
                            if (index > 0)
                                doRepeat = true;

                            bool removeAfter = true;

                            if (temp.Length > index - s.Length + 3 && index > s.Length)
                            {
                                if (temp.Substring(index - s.Length + 1, 3) == ", !")
                                {
                                    temp = temp.Remove(index - s.Length + 1, 2);
                                    removeAfter = false;
                                }
                                else if (temp.Substring(index - s.Length + 1, 3) == ", ?")
                                {
                                    temp = temp.Remove(index - s.Length + 1, 2);
                                    removeAfter = false;
                                }
                                else if (temp.Substring(index - s.Length + 1, 3) == ", .")
                                {
                                    temp = temp.Remove(index - s.Length + 1, 2);
                                    removeAfter = false;
                                }
                            }
                            if (removeAfter && temp.Length > index - s.Length + 2 && index > s.Length)
                            {
                                if (temp.Substring(index - s.Length, 3) == ", !")
                                {
                                    temp = temp.Remove(index - s.Length, 2);
                                    removeAfter = false;
                                }
                                else if (temp.Substring(index - s.Length, 3) == ", ?")
                                {
                                    temp = temp.Remove(index - s.Length, 2);
                                    removeAfter = false;
                                }
                                else if (temp.Substring(index - s.Length, 3) == ", .")
                                {
                                    temp = temp.Remove(index - s.Length, 2);
                                    removeAfter = false;
                                }
                                else if (index > 0 && temp.Substring(index - s.Length).StartsWith(", -—"))
                                {
                                    temp = temp.Remove(index - s.Length, 3);
                                    removeAfter = false;
                                }
                                else if (index > 0 && temp.Substring(index - s.Length).StartsWith(", --"))
                                {
                                    temp = temp.Remove(index - s.Length, 2);
                                    removeAfter = false;
                                }
                            }
                            if (removeAfter && temp.Length > index - s.Length + 2 && index > s.Length)
                            {
                                if (temp.Substring(index - s.Length + 1, 2) == "-!")
                                {
                                    temp = temp.Remove(index - s.Length + 1, 1);
                                    removeAfter = false;
                                }
                                else if (temp.Substring(index - s.Length + 1, 2) == "-?")
                                {
                                    temp = temp.Remove(index - s.Length + 1, 1);
                                    removeAfter = false;
                                }
                                else if (temp.Substring(index - s.Length + 1, 2) == "-.")
                                {
                                    temp = temp.Remove(index - s.Length + 1, 1);
                                    removeAfter = false;
                                }
                            }

                            if (index > 3 && index - 2 < temp.Length && temp.Substring(index - 2).StartsWith(",  —", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(index - 2, 1);
                                index--;
                            }
                            else if (index > 3 && index - 2 < temp.Length && temp.Substring(index - 2).StartsWith(", —", StringComparison.Ordinal))
                            {
                                temp = temp.Remove(index - 2, 1);
                                index--;
                            }

                            if (removeAfter)
                            {
                                if (index == 0)
                                {
                                    if (!string.IsNullOrEmpty(temp) && temp.StartsWith('-'))
                                        temp = temp.Remove(0, 1).Trim();
                                }
                                else if (index == 3 && !string.IsNullOrEmpty(temp) && temp.StartsWith("<i>-", StringComparison.Ordinal))
                                {
                                    temp = temp.Remove(3, 1);
                                }
                                else if (index > 0 && temp.Length > index)
                                {
                                    pre = text.Substring(0, index);
                                    temp = temp.Remove(0, index);
                                    if (pre.EndsWith('-') && temp.StartsWith('-'))
                                        temp = temp.Remove(0, 1);
                                    if (pre.EndsWith("- ") && temp.StartsWith('-'))
                                        temp = temp.Remove(0, 1);
                                }

                                while (temp.Length > 0 && (temp.StartsWith(' ') || temp.StartsWith(',') || temp.StartsWith('.') || temp.StartsWith('!') || temp.StartsWith('?')))
                                {
                                    temp = temp.Remove(0, 1);
                                    doRepeat = true;
                                }
                                if (temp.Length > 0 && s[0].ToString(CultureInfo.InvariantCulture) != s[0].ToString(CultureInfo.InvariantCulture).ToLower())
                                {
                                    temp = char.ToUpper(temp[0]) + temp.Substring(1);
                                    doRepeat = true;
                                }

                                if (pre.EndsWith(' ') && temp.StartsWith('-'))
                                    temp = temp.Remove(0, 1);

                                if (pre.EndsWith(',') && temp.StartsWith('—'))
                                    pre = pre.TrimEnd(',') + " ";
                                temp = pre + temp;
                            }

                            if (temp.EndsWith(Environment.NewLine + "- "))
                                temp = temp.Remove(temp.Length - 4, 4);

                            var st = new StripableText(temp);
                            if (st.StrippedText.Length == 0)
                                return string.Empty;

                            if (!temp.Contains(Environment.NewLine) && text.Contains(Environment.NewLine) && temp.StartsWith('-'))
                                temp = temp.Remove(0, 1).Trim();

                            text = temp;
                        }
                    }
                }
            }
            var lines = text.SplitToLines();
            if (text != oldText && lines.Length == 2)
            {
                if (lines[0] == "-" && lines[1] == "-")
                    return string.Empty;
                if (lines[0].StartsWith('-') && lines[0].Length > 1 && lines[1].Trim() == "-")
                    return lines[0].Remove(0, 1).Trim();
                if (lines[1].StartsWith('-') && lines[1].Length > 1 && lines[0].Trim() == "-")
                    return lines[1].Remove(0, 1).Trim();
                if (lines[1].StartsWith("<i>-", StringComparison.Ordinal) && lines[1].Length > 4 && lines[0].Trim() == "-")
                    return "<i>" + lines[1].Remove(0, 4).Trim();
                if (lines[0].Length > 1 && (lines[1] == "-") || lines[1] == "." || lines[1] == "!" || lines[1] == "?")
                {
                    if (oldText.Contains(Environment.NewLine + "-") && lines[0].StartsWith('-'))
                        lines[0] = lines[0].Remove(0, 1);
                    return lines[0].Trim();
                }
                if (HtmlUtil.RemoveHtmlTags(lines[0], false).Trim() == "-")
                {
                    if (HtmlUtil.RemoveHtmlTags(lines[1], false).Trim() == "-")
                        return string.Empty;
                    if (lines[1].StartsWith('-') && lines[1].Length > 1)
                        return lines[1].Remove(0, 1).Trim();
                    if (lines[1].StartsWith("<i>-", StringComparison.Ordinal) && lines[1].Length > 4)
                        return "<i>" + lines[1].Remove(0, 4).Trim();
                    return lines[1];
                }
                if (HtmlUtil.RemoveHtmlTags(lines[1], false).Trim() == "-")
                {
                    if (HtmlUtil.RemoveHtmlTags(lines[0], false).Trim() == "-")
                        return string.Empty;
                    if (lines[0].StartsWith('-') && lines[0].Length > 1)
                        return lines[0].Remove(0, 1).Trim();
                    if (lines[0].StartsWith("<i>-", StringComparison.Ordinal) && lines[0].Length > 4)
                        return "<i>" + lines[0].Remove(0, 4).Trim();
                    return lines[0];
                }
            }
            if (lines.Length == 2 && string.IsNullOrWhiteSpace(lines[1].Replace(".", string.Empty).Replace("?", string.Empty).Replace("!", string.Empty).Replace("-", string.Empty).Replace("—", string.Empty)))
            {
                text = lines[0];
                lines = text.SplitToLines();
            }
            else if (lines.Length == 2 && string.IsNullOrWhiteSpace(lines[0].Replace(".", string.Empty).Replace("?", string.Empty).Replace("!", string.Empty).Replace("-", string.Empty).Replace("—", string.Empty)))
            {
                text = lines[1];
                lines = text.SplitToLines();
            }
            if (text != oldText && lines.Length == 1 && Utilities.GetNumberOfLines(oldText) == 2)
            {
                if ((oldText.StartsWith("-", StringComparison.Ordinal) || oldText.StartsWith("<i>-", StringComparison.Ordinal)) &&
                    (oldText.Contains("." + Environment.NewLine) || oldText.Contains(".</i>" + Environment.NewLine) ||
                     oldText.Contains("!" + Environment.NewLine) || oldText.Contains("!</i>" + Environment.NewLine) ||
                     oldText.Contains("?" + Environment.NewLine) || oldText.Contains("?</i>" + Environment.NewLine)))
                {
                    if (text.StartsWith("<i>-"))
                        text = "<i>" + text.Remove(0, 4).TrimStart();
                    else
                        text = text.TrimStart('-').TrimStart();
                }
                else if ((oldText.Contains(Environment.NewLine + "-") || oldText.Contains(Environment.NewLine + "<i>-")) &&
                    (oldText.Contains("." + Environment.NewLine) || oldText.Contains(".</i>" + Environment.NewLine) ||
                     oldText.Contains("!" + Environment.NewLine) || oldText.Contains("!</i>" + Environment.NewLine) ||
                     oldText.Contains("?" + Environment.NewLine) || oldText.Contains("?</i>" + Environment.NewLine)))
                {
                    if (text.StartsWith("<i>-"))
                        text = "<i>" + text.Remove(0, 4).TrimStart();
                    else
                        text = text.TrimStart('-').TrimStart();
                }
            }
            return text;
        }

        private string RemoveStartEndTags(string text)
        {
            string newText = text;
            string s = text;
            if (s.StartsWith('[') && s.IndexOf(']') > 0 && Settings.RemoveTextBetweenSquares)
                newText = s.Remove(0, s.IndexOf(']') + 1);
            else if (s.StartsWith('{') && s.IndexOf('}') > 0 && Settings.RemoveTextBetweenBrackets)
                newText = s.Remove(0, s.IndexOf('}') + 1);
            else if (s.StartsWith('?') && s.IndexOf('?', 1) > 0 && Settings.RemoveTextBetweenQuestionMarks)
                newText = s.Remove(0, s.IndexOf('?', 1) + 1);
            else if (s.StartsWith('(') && s.IndexOf(')') > 0 && Settings.RemoveTextBetweenParentheses)
                newText = s.Remove(0, s.IndexOf(')') + 1);
            else if (s.StartsWith('[') && s.IndexOf("]:", StringComparison.Ordinal) > 0 && Settings.RemoveTextBetweenSquares)
                newText = s.Remove(0, s.IndexOf("]:", StringComparison.Ordinal) + 2);
            else if (s.StartsWith('{') && s.IndexOf("}:", StringComparison.Ordinal) > 0 && Settings.RemoveTextBetweenBrackets)
                newText = s.Remove(0, s.IndexOf("}:", StringComparison.Ordinal) + 2);
            else if (s.StartsWith('?') && s.IndexOf("?:", 1, StringComparison.Ordinal) > 0 && Settings.RemoveTextBetweenQuestionMarks)
                newText = s.Remove(0, s.IndexOf("?:", StringComparison.Ordinal) + 2);
            else if (s.StartsWith('(') && s.IndexOf("):", StringComparison.Ordinal) > 0 && Settings.RemoveTextBetweenParentheses)
                newText = s.Remove(0, s.IndexOf("):", StringComparison.Ordinal) + 2);
            else if (Settings.RemoveTextBetweenCustomTags &&
                     s.Length > 0 && Settings.CustomEnd.Length > 0 && Settings.CustomStart.Length > 0 &&
                     s.StartsWith(Settings.CustomStart) && s.LastIndexOf(Settings.CustomEnd, StringComparison.Ordinal) > 0)
                newText = s.Remove(0, s.LastIndexOf(Settings.CustomEnd, StringComparison.Ordinal) + Settings.CustomEnd.Length);
            if (newText != text)
                newText = newText.TrimStart(' ');
            return newText;
        }

        public string RemoveHearImpairedTags(string text)
        {
            string preAssTag = string.Empty;
            if (text.StartsWith("{\\", StringComparison.Ordinal) && text.IndexOf('}') > 0)
            {
                int indexOfEndBracket = text.IndexOf('}') + 1;
                preAssTag = text.Substring(0, indexOfEndBracket);
                text = text.Remove(0, indexOfEndBracket).TrimStart();
            }
            if (Settings.RemoveTextBetweenSquares)
            {
                text = RemoveTextBetweenTags("[", "]:", text);
                text = RemoveTextBetweenTags("[", "]", text);
            }
            if (Settings.RemoveTextBetweenBrackets)
            {
                text = RemoveTextBetweenTags("{", "}:", text);
                text = RemoveTextBetweenTags("{", "}", text);
            }
            if (Settings.RemoveTextBetweenQuestionMarks)
            {
                text = RemoveTextBetweenTags("?", "?:", text);
                text = RemoveTextBetweenTags("?", "?", text);
            }
            if (Settings.RemoveTextBetweenParentheses)
            {
                text = RemoveTextBetweenTags("(", "):", text);
                text = RemoveTextBetweenTags("(", ")", text);
            }
            if (Settings.RemoveTextBetweenCustomTags && Settings.CustomStart.Length > 0 && Settings.CustomEnd.Length > 0)
            {
                text = RemoveTextBetweenTags(Settings.CustomStart, Settings.CustomEnd, text);
            }
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            return preAssTag + text.TrimStart();
        }

        private bool HasHearImpairedText(string text)
        {
            return RemoveHearImpairedTags(text) != text;
        }

        public bool HasHearImpariedTagsAtStart(string text)
        {
            if (Settings.OnlyIfInSeparateLine)
                return StartAndEndsWithHearImpariedTags(text);
            return HasHearImpairedText(text);
        }

        public bool HasHearImpariedTagsAtEnd(string text)
        {
            if (Settings.OnlyIfInSeparateLine)
                return StartAndEndsWithHearImpariedTags(text);
            return HasHearImpairedText(text);
        }

        private bool StartAndEndsWithHearImpariedTags(string text)
        {
            return (text.StartsWith('[') && text.EndsWith(']') && !text.Trim('[').Contains('[') && Settings.RemoveTextBetweenSquares) ||
                   (text.StartsWith('{') && text.EndsWith('}') && !text.Trim('{').Contains('{') && Settings.RemoveTextBetweenBrackets) ||
                   (text.StartsWith('?') && text.EndsWith('?') && !text.Trim('?').Contains('?') && Settings.RemoveTextBetweenQuestionMarks) ||
                   (text.StartsWith('(') && text.EndsWith(')') && !text.Trim('(').Contains('(') && Settings.RemoveTextBetweenParentheses) ||
                   (text.StartsWith('[') && text.EndsWith("]:", StringComparison.Ordinal) && !text.Trim('[').Contains('[') && Settings.RemoveTextBetweenSquares) ||
                   (text.StartsWith('{') && text.EndsWith("}:", StringComparison.Ordinal) && !text.Trim('{').Contains('{') && Settings.RemoveTextBetweenBrackets) ||
                   (text.StartsWith('?') && text.EndsWith("?:", StringComparison.Ordinal) && !text.Trim('?').Contains('?') && Settings.RemoveTextBetweenQuestionMarks) ||
                   (text.StartsWith('(') && text.EndsWith("):", StringComparison.Ordinal) && !text.Trim('(').Contains('(') && Settings.RemoveTextBetweenParentheses) ||
                   (Settings.RemoveTextBetweenCustomTags &&
                    Settings.CustomStart.Length > 0 && Settings.CustomEnd.Length > 0 &&
                    text.StartsWith(Settings.CustomStart, StringComparison.Ordinal) && text.EndsWith(Settings.CustomEnd, StringComparison.Ordinal));
        }

        private static string RemoveTextBetweenTags(string startTag, string endTag, string text)
        {
            text = text.Trim();
            if (startTag == "?" || endTag == "?")
            {
                if (text.StartsWith(startTag) && text.EndsWith(endTag))
                    return string.Empty;
                return text;
            }

            int start = text.IndexOf(startTag, StringComparison.Ordinal);
            if (start < 0 || start == text.Length - 1)
                return text;

            int end = text.IndexOf(endTag, start + 1, StringComparison.Ordinal);
            while (start >= 0 && end > start)
            {
                text = text.Remove(start, (end - start) + 1);
                if (start > 3 && start < text.Length - 1 &&
                    text.Substring(0, start + 1).EndsWith(" :", StringComparison.Ordinal) &&
                    ".!?".Contains(text[start - 2]))
                {
                    text = text.Remove(start - 1, 2);
                }

                start = text.IndexOf(startTag, StringComparison.Ordinal);
                if (start >= 0 && start < text.Length - 1)
                    end = text.IndexOf(endTag, start + 1, StringComparison.Ordinal);
                else
                    break;
            }
            return text.FixExtraSpaces().TrimEnd();
        }

        public string RemoveLineIfAllUppercase(string text)
        {
            if (!Settings.RemoveIfAllUppercase)
                return text;

            var lines = text.SplitToLines();
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                var lineNoHtml = HtmlUtil.RemoveHtmlTags(line, true);
                var tmp = lineNoHtml.TrimEnd('.', '!', '?', ':').Trim();
                if (lineNoHtml == lineNoHtml.ToUpper())
                {
                    tmp = tmp.Trim(' ', '-', '—');
                    if (tmp.Length == 1 || tmp == "YES" || tmp == "NO" || tmp == "WHY" || tmp == "HI")
                    {
                        sb.AppendLine(line);
                    }
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString().Trim();
        }

    }
}
