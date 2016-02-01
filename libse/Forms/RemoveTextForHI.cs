using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Forms
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
                if (s.Length > 2 && ".?!".Contains(s[0]))
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
                        if (s.Length > 1 && s[0] == ' ')
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

        private static readonly string[] ExpectedStrings = { ". ", "! ", "? " };
        public string RemoveColon(string text)
        {
            if (!(Settings.RemoveTextBeforeColon && text.Contains(':')))
                return text;

            string preAssTag = string.Empty;
            if (text.StartsWith("{\\", StringComparison.Ordinal))
            {
                int indexOfEndBracketSuccessor = text.IndexOf('}') + 1;
                if (indexOfEndBracketSuccessor > 0)
                {
                    preAssTag = text.Substring(0, indexOfEndBracketSuccessor);
                    text = text.Remove(0, indexOfEndBracketSuccessor).TrimStart();
                }
            }

            // House 7x01 line 52: and she would like you to do three things:
            // Okay or remove???
            string noTagText = HtmlUtil.RemoveHtmlTags(text);
            if (noTagText.Length > 10 && noTagText.IndexOf(':') == noTagText.Length - 1 && noTagText != noTagText.ToUpper())
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
                if (indexOfColon <= 0 || IsInsideBrackets(line, indexOfColon))
                {
                    newText = (newText + Environment.NewLine + line).Trim();

                    if (newText.EndsWith("</i>", StringComparison.Ordinal) && text.StartsWith("<i>", StringComparison.Ordinal) && !newText.StartsWith("<i>", StringComparison.Ordinal))
                        newText = "<i>" + newText;
                    else if (newText.EndsWith("</b>", StringComparison.Ordinal) && text.StartsWith("<b>", StringComparison.Ordinal) && !newText.StartsWith("<b>", StringComparison.Ordinal))
                        newText = "<b>" + newText;
                }
                else
                {
                    var pre = line.Substring(0, indexOfColon);
                    var noTagPre = HtmlUtil.RemoveHtmlTags(pre, true);
                    if (Settings.RemoveTextBeforeColonOnlyUppercase && noTagPre != noTagPre.ToUpper())
                    {
                        string s = line;
                        string l1Trim = HtmlUtil.RemoveHtmlTags(lines[0]).TrimEnd('"');
                        if (count == 1 && lines.Length == 2 && !l1Trim.EndsWith('.') &&
                                                               !l1Trim.EndsWith('!') &&
                                                               !l1Trim.EndsWith('?'))
                        {
                            var indexOf = line.IndexOfAny(ExpectedStrings, StringComparison.Ordinal);
                            if (indexOf > 0 && indexOf < indexOfColon)
                            {
                                var toRemove = s.Substring(indexOf + 1, indexOfColon - indexOf).Trim();
                                if (toRemove.Length > 1 && toRemove == toRemove.ToUpper())
                                {
                                    s = s.Remove(indexOf + 1, indexOfColon - indexOf);
                                    s = s.Insert(indexOf + 1, " -");
                                    if (newText.StartsWith("<i>") && !newText.StartsWith("<i>-"))
                                        newText = "<i>- " + newText.Remove(0, 3);
                                    else if (!newText.StartsWith("-"))
                                        newText = "- " + newText;
                                }
                            }
                        }

                        newText = (newText + Environment.NewLine + s).Trim();
                    }
                    else
                    {
                        var newTextNoHtml = HtmlUtil.RemoveHtmlTags(newText);
                        if (Utilities.CountTagInText(line, ':') == 1)
                        {
                            if (count == 1 && newText.Length > 1 && removedInFirstLine &&
                                !".?!".Contains(newTextNoHtml[newTextNoHtml.Length - 1]) && newText.LineEndsWithHtmlTag(true) &&
                                line != line.ToUpper())
                            {
                                newText += Environment.NewLine;
                                if (pre.Contains("<i>") && line.Contains("</i>") && !line.Contains("<i>"))
                                    newText += "<i>" + line;
                                else if (pre.Contains("<b>") && line.Contains("</b>") && !line.Contains("<b>"))
                                    newText += "<b>" + line;
                                else if (pre.Contains("<u>") && line.Contains("</u>") && !line.Contains("<u>"))
                                    newText += "<u>" + line;
                                else if (pre.Contains('[') && line.Contains(']') && !line.Contains("["))
                                    newText += "[" + line;
                                else if (pre.Contains('(') && line.EndsWith(')') && !line.Contains("("))
                                    newText += "(" + line;
                                else
                                    newText += line;
                            }
                            else if (count == 1 && newText.Length > 1 && indexOfColon > 15 && line.Substring(0, indexOfColon).Contains(' ') &&
                                     !".?!".Contains(newTextNoHtml[newTextNoHtml.Length - 1]) && newText.LineEndsWithHtmlTag(true) &&
                                     line != line.ToUpper())
                            {
                                newText += Environment.NewLine;
                                if (pre.Contains("<i>") && line.Contains("</i>") && !line.Contains("<i>"))
                                    newText += "<i>" + line;
                                else if (pre.Contains("<b>") && line.Contains("</b>") && !line.Contains("<b>"))
                                    newText += "<b>" + line;
                                else if (pre.Contains("<u>") && line.Contains("</u>") && !line.Contains("<u>"))
                                    newText += "<u>" + line;
                                else if (pre.Contains('[') && line.Contains(']') && !line.Contains("["))
                                    newText += "[" + line;
                                else if (pre.Contains('(') && line.EndsWith(')') && !line.Contains("("))
                                    newText += "(" + line;
                                else
                                    newText += line;
                            }
                            else
                            {
                                var preStripable = new StripableText(pre);
                                var remove = true;

                                if (indexOfColon < line.Length - 1)
                                {
                                    if (Settings.ColonSeparateLine && !line.Substring(indexOfColon + 1).StartsWith(Environment.NewLine, StringComparison.Ordinal))
                                        remove = false;
                                    else if (Utilities.IsBetweenNumbers(line, indexOfColon))
                                        remove = false;
                                }

                                if (remove && !DoRemove(pre))
                                    remove = false;

                                string l1Trimmed = HtmlUtil.RemoveHtmlTags(lines[0]).TrimEnd('"');
                                if (count == 1 && lines.Length == 2 && !l1Trimmed.EndsWith('.') &&
                                                                       !l1Trimmed.EndsWith('!') &&
                                                                       !l1Trimmed.EndsWith('?'))
                                {
                                    remove = false;
                                }

                                if (remove)
                                {
                                    var content = line.Substring(indexOfColon + 1).Trim();
                                    if (content.Length > 0)
                                    {
                                        newText += Environment.NewLine;
                                        if (pre.Contains("<i>") && content.Contains("</i>"))
                                            newText += "<i>" + content;
                                        else if (pre.Contains("<b>") && content.Contains("</b>"))
                                            newText += "<b>" + content;
                                        else if (pre.Contains('[') && content.Contains(']'))
                                            newText += "[" + content;
                                        else if (pre.Contains('(') && content.EndsWith(')'))
                                            newText += "(" + content;
                                        else
                                            newText += content;

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

                                    if (!IsHIDescription(preStripable.StrippedText))
                                        noOfNames++;
                                }
                                else
                                {
                                    string s = line;
                                    string l1Trim = HtmlUtil.RemoveHtmlTags(lines[0]).TrimEnd('"');
                                    if (count == 1 && lines.Length == 2 && !l1Trim.EndsWith('.') &&
                                                                           !l1Trim.EndsWith('!') &&
                                                                           !l1Trim.EndsWith('?'))
                                    {
                                        int indexOf = line.IndexOf(". ", StringComparison.Ordinal);
                                        if (indexOf == -1)
                                            indexOf = line.IndexOf("! ", StringComparison.Ordinal);
                                        if (indexOf == -1)
                                            indexOf = line.IndexOf("? ", StringComparison.Ordinal);
                                        if (indexOf > 0 && indexOf < indexOfColon)
                                        {
                                            s = s.Remove(indexOf + 1, indexOfColon - indexOf);
                                            s = s.Insert(indexOf + 1, " -");
                                            if (newText.StartsWith("<i>") && !newText.StartsWith("<i>-"))
                                                newText = "<i>- " + newText.Remove(0, 3);
                                            else if (!newText.StartsWith("-"))
                                                newText = "- " + newText;
                                        }
                                    }
                                    newText = (newText + Environment.NewLine + s).Trim();
                                    if (newText.EndsWith("</i>", StringComparison.Ordinal) && text.StartsWith("<i>", StringComparison.Ordinal) && !newText.StartsWith("<i>", StringComparison.Ordinal))
                                        newText = "<i>" + newText;
                                    else if (newText.EndsWith("</b>", StringComparison.Ordinal) && text.StartsWith("<b>", StringComparison.Ordinal) && !newText.StartsWith("<b>", StringComparison.Ordinal))
                                        newText = "<b>" + newText;
                                    else if ((newText.EndsWith("</u>", StringComparison.Ordinal) && text.StartsWith("<u>", StringComparison.Ordinal) && !newText.StartsWith("<u>", StringComparison.Ordinal)))
                                        newText = "<u>" + newText;
                                }
                            }
                        }
                        else
                        {
                            char[] endChars = { '.', '?', '!' };
                            string s2 = line;
                            for (int k = 0; k < 2; k++)
                            {
                                if (s2.Contains(':'))
                                {
                                    int colonIndex = s2.IndexOf(':');
                                    string start = s2.Substring(0, colonIndex);

                                    if (!Settings.RemoveTextBeforeColonOnlyUppercase || start == start.ToUpper())
                                    {
                                        int endIndex = start.LastIndexOfAny(endChars);
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
                            newText = (newText + Environment.NewLine + s2).Trim();
                        }
                    }
                }
                count++;
            }
            newText = newText.Trim();
            if ((noOfNames > 0 || removedInFirstLine) && Utilities.GetNumberOfLines(newText) == 2)
            {
                int indexOfDialogChar = newText.IndexOf('-');
                bool insertDash = true;
                var arr = newText.SplitToLines();
                if (arr.Length == 2 && arr[0].Length > 1 && arr[1].Length > 1)
                {
                    string arr0 = new StripableText(arr[0]).StrippedText;
                    var arr1Stripable = new StripableText(arr[1]);
                    string arr1 = arr1Stripable.StrippedText;

                    if (arr0.Length > 0 && arr1.Length > 1)
                    {
                        // line continuation?
                        if (Utilities.LowercaseLetters.Contains(arr1[0])) // second line starts with lower case letter
                        {
                            char c = arr0[arr0.Length - 1];
                            if (Utilities.LowercaseLetters.Contains(c) || c == ',') // first line ends with comma or lower case letter
                            {
                                if (!arr1Stripable.Pre.Contains("..."))
                                {
                                    insertDash = false;
                                }
                            }
                        }

                        if (insertDash)
                        {
                            string arr0QuoteTrimmed = arr[0].TrimEnd('"');
                            if (arr0QuoteTrimmed.Length > 0 && !".?!".Contains(arr0QuoteTrimmed[arr0QuoteTrimmed.Length - 1]) && !arr0QuoteTrimmed.EndsWith("</i>", StringComparison.Ordinal))
                            {
                                if (!arr1Stripable.Pre.Contains('-'))
                                {
                                    insertDash = false;
                                }
                            }
                        }
                    }

                    if (insertDash && removedInFirstLine && !removedInSecondLine && !text.StartsWith('-') && !text.StartsWith("<i>-", StringComparison.Ordinal))
                    {
                        if (!arr[1].StartsWith('-') && !arr[1].StartsWith("<i>-", StringComparison.Ordinal))
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
            else if (newText.Contains('-') && !newText.Contains(Environment.NewLine))
            {
                var st = new StripableText(newText);
                if (st.Pre.Contains('-'))
                    newText = st.Pre.Replace("-", string.Empty) + st.StrippedText + st.Post;
            }
            else if (removedInSecondLine && !removedInFirstLine && Utilities.GetNumberOfLines(newText) == 2)
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
            if (text.Contains("<i>") && !newText.Contains("<i>") && newText.EndsWith("</i>", StringComparison.Ordinal))
                newText = "<i>" + newText;

            if (string.IsNullOrWhiteSpace(newText))
                return string.Empty;

            return preAssTag + newText;
        }

        private bool IsInsideBrackets(string text, int targetIndex)
        {
            // <i>♪ (THE CAPITOLS: "COOL JERK") ♪</i>
            var index = text.LastIndexOf('(', targetIndex - 1) + 1;
            if (index > 0 && text.IndexOf(')', index) > targetIndex)
                return true;

            index = text.LastIndexOf('[', targetIndex - 1) + 1;
            if (index > 0 && text.IndexOf(']', index) > targetIndex)
                return true;

            return false;
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

        private static readonly char[] TrimStartNoiseChar = { '-', ' ' };

        public string RemoveTextFromHearImpaired(string text)
        {
            if (StartsAndEndsWithHearImpairedTags(HtmlUtil.RemoveHtmlTags(text, true).TrimStart(TrimStartNoiseChar)))
            {
                return string.Empty;
            }

            if (Settings.RemoveWhereContains)
            {
                foreach (var removeIfTextContain in Settings.RemoveIfTextContains)
                {
                    if (text.Contains(removeIfTextContain))
                        return string.Empty;
                }
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
                string strippedText = stSub.StrippedText;
                if ((lineNumber == parts.Length - 1 && st.Post.Contains('?')) || stSub.Post.Contains('?'))
                    strippedText += "?";
                if (!StartsAndEndsWithHearImpairedTags(strippedText))
                {
                    if (removedDialogInFirstLine && stSub.Pre.Contains("- "))
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

                    if (stSub.Pre == "<i>- " && newText.StartsWith("</i>"))
                        sb.AppendLine("- " + newText.Remove(0, 4).Trim() + stSub.Post);
                    else
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

                    if (lineNumber == 0)
                    {
                        if (st.Pre.Contains("- "))
                        {
                            st.Pre = st.Pre.Replace("- ", string.Empty);
                            removedDialogInFirstLine = true;
                        }
                        else if (st.Pre == "-")
                        {
                            st.Pre = string.Empty;
                            removedDialogInFirstLine = true;
                        }
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
            text = text.Replace("<u></u>", string.Empty);
            text = text.Replace("<u> </u>", " ");
            text = RemoveEmptyFontTag(text);
            text = text.Replace("  ", " ").Trim();
            text = RemoveColon(text);
            text = RemoveLineIfAllUppercase(text);
            text = RemoveHearImpairedtagsInsideLine(text);
            if (Settings.RemoveInterjections)
                text = RemoveInterjections(text);

            st = new StripableText(text, " >-\"'‘`´♪¿¡.…—", " -\"'`´♪.!?:…—");
            text = st.StrippedText;
            if (StartsAndEndsWithHearImpairedTags(text))
            {
                text = RemoveStartEndTags(text);
            }

            text = RemoveHearImpairedTags(text);

            // fix 3 lines to two liners - if only two lines
            if (noOfNamesRemoved >= 1 && Utilities.GetNumberOfLines(text) == 3)
            {
                var splitChars = new[] { '.', '?', '!' };
                var splitParts = HtmlUtil.RemoveHtmlTags(text).Replace(" ", string.Empty).Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                if (splitParts.Length == 2)
                {
                    var temp = new StripableText(text);
                    temp.StrippedText = temp.StrippedText.Replace(Environment.NewLine, " ");
                    int splitIndex = temp.StrippedText.LastIndexOfAny(splitChars);
                    if (splitIndex > 0)
                    {
                        text = temp.Pre + temp.StrippedText.Insert(splitIndex + 1, Environment.NewLine) + temp.Post;
                    }
                }
            }

            if (!text.StartsWith('-') && noOfNamesRemoved >= 1 && Utilities.GetNumberOfLines(text) == 2)
            {
                var lines = text.SplitToLines();
                var part0 = lines[0].Trim().Replace("</i>", string.Empty).Trim();
                if (!part0.EndsWith(',') && (!part0.EndsWith('-') || noOfNamesRemovedNotInLineOne > 0))
                {
                    if (part0.Length > 0 && ".?!".Contains(part0[part0.Length - 1]))
                    {
                        if (noOfNamesRemovedNotInLineOne > 0)
                        {
                            if (!st.Pre.Contains('-') && !text.Contains(Environment.NewLine + "-"))
                                text = "- " + text.Replace(Environment.NewLine, Environment.NewLine + "- ");
                            if (!text.Contains(Environment.NewLine + "-") && !text.Contains(Environment.NewLine + "<i>-"))
                                text = text.Replace(Environment.NewLine, Environment.NewLine + "- ");
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(text) || (st.Pre.Contains('♪') || st.Post.Contains('♪')))
                text = st.Pre + text + st.Post;

            if (oldText.TrimStart().StartsWith("- ", StringComparison.Ordinal) &&
                text != null && !text.Contains(Environment.NewLine) &&
                (oldText.Contains(Environment.NewLine + "- ") ||
                 oldText.Contains(Environment.NewLine + " - ") ||
                 oldText.Contains(Environment.NewLine + "<i>- ") ||
                 oldText.Contains(Environment.NewLine + "<i> - ")))
            {
                text = text.TrimStart().TrimStart('-').TrimStart();
            }
            if (oldText.TrimStart().StartsWith('-') && !oldText.TrimStart().StartsWith("--", StringComparison.Ordinal) &&
                text != null && !text.Contains(Environment.NewLine) &&
                (oldText.Contains(Environment.NewLine + "-") && !oldText.Contains(Environment.NewLine + "--") ||
                 oldText.Contains(Environment.NewLine + " - ") ||
                 oldText.Contains(Environment.NewLine + "<i>- ") ||
                 oldText.Contains(Environment.NewLine + "<i> - ")))
            {
                text = text.TrimStart().TrimStart('-').TrimStart();
            }

            if (oldText.TrimStart().StartsWith("<i>- ", StringComparison.Ordinal) &&
                text != null && text.StartsWith("<i>- ", StringComparison.Ordinal) && !text.Contains(Environment.NewLine) &&
                (oldText.Contains(Environment.NewLine + "- ") ||
                 oldText.Contains(Environment.NewLine + " - ") ||
                 oldText.Contains(Environment.NewLine + "<i>- ") ||
                 oldText.Contains(Environment.NewLine + "<i> - ")))
            {
                text = text.Remove(3, 2);
            }

            if (text != null && !text.Contains(Environment.NewLine) &&
                (oldText.Contains(':') && !text.Contains(':') ||
                 oldText.Contains('[') && !text.Contains('[') ||
                 oldText.Contains('(') && !text.Contains('(') ||
                 oldText.Contains('{') && !text.Contains('{')) &&
                (oldText.Contains(Environment.NewLine + "- ") ||
                 oldText.Contains(Environment.NewLine + " - ") ||
                 oldText.Contains(Environment.NewLine + "<i>- ") ||
                 oldText.Contains(Environment.NewLine + "<i> - ")))
            {
                text = text.TrimStart().TrimStart('-').TrimStart();
            }

            string removeText = "<i>- </i>" + Environment.NewLine + "-";
            if (text.StartsWith(removeText))
            {
                text = text.Remove(0, removeText.Length).TrimStart(' ');
            }

            removeText = "<i>-</i>" + Environment.NewLine + "-";
            if (text.StartsWith(removeText))
            {
                text = text.Remove(0, removeText.Length).TrimStart(' ');
            }

            removeText = "<i>-</i>" + Environment.NewLine + "<i>-";
            if (text.StartsWith(removeText))
            {
                text = "<i>" + text.Remove(0, removeText.Length).TrimStart(' ');
            }

            removeText = "<i>- </i>" + Environment.NewLine + "<i>-";
            if (text.StartsWith(removeText))
            {
                text = "<i>" + text.Remove(0, removeText.Length).TrimStart(' ');
            }

            if (oldText != text)
            {
                // insert spaces before "-"
                text = text.Replace(Environment.NewLine + "- <i>", Environment.NewLine + "<i>- ");
                text = text.Replace(Environment.NewLine + "-<i>", Environment.NewLine + "<i>- ");
                if (text.Length > 2 && text[0] == '-' && text[1] != ' ' && text[1] != '-')
                    text = text.Insert(1, " ");
                if (text.Length > 5 && text.StartsWith("<i>-", StringComparison.Ordinal) && text[4] != ' ' && text[4] != '-')
                    text = text.Insert(4, " ");
                int index = text.IndexOf(Environment.NewLine + "-", StringComparison.Ordinal);
                if (index >= 0 && text.Length - index > 4)
                {
                    index += Environment.NewLine.Length + 1;
                    if (text[index] != ' ' && text[index] != '-')
                        text = text.Insert(index, " ");
                }
                index = text.IndexOf(Environment.NewLine + "<i>-", StringComparison.Ordinal);
                if (index >= 0 && text.Length - index > 5)
                {
                    index += Environment.NewLine.Length + 4;
                    if (text[index] != ' ' && text[index] != '-')
                        text = text.Insert(index, " ");
                }
            }
            return text.Trim();
        }

        private string RemoveEmptyFontTag(string text)
        {
            int indexOfStartFont = text.IndexOf("<font ", StringComparison.OrdinalIgnoreCase);
            if (indexOfStartFont >= 0)
            {
                int indexOfEndFont = text.IndexOf("</font>", StringComparison.OrdinalIgnoreCase);
                if (indexOfEndFont > indexOfStartFont)
                {
                    int startTagBefore = text.Substring(0, indexOfEndFont).LastIndexOf('<');
                    if (startTagBefore == indexOfStartFont)
                    {
                        var lastTwo = text.Substring(indexOfEndFont - 2, 2);
                        if (lastTwo.TrimEnd().EndsWith('>'))
                        {
                            text = text.Remove(indexOfStartFont, indexOfEndFont + "</font>".Length - indexOfStartFont);
                            if (lastTwo.EndsWith(' '))
                                text = text.Insert(indexOfStartFont, " ");
                            text = text.Replace("  ", " ");
                        }
                    }
                }
            }
            return text;
        }

        private void AddWarning()
        {
            if (Warnings != null && WarningIndex >= 0)
                Warnings.Add(WarningIndex);
        }

        private static readonly HashSet<string> HiDescriptionWords = new HashSet<string>(new[]
        {
            "cackles",
            "cheers",
            "chitters",
            "chuckles",
            "clears throat",
            "exclaims",
            "exhales",
            "explosion",
            "gasps",
            "groans",
            "growls",
            "grunts",
            "laughs",
            "noise",
            "roars",
            "scoff",
            "screeches",
            "shouts",
            "shrieks",
            "sigh",
            "sighs",
            "snores",
            "stutters",
            "thuds",
            "trumpets",
            "whisper",
            "whispers",
            "whistles"
        });

        private bool IsHIDescription(string text)
        {
            text = text.Trim(' ', '(', ')', '[', ']', '?', '{', '}').ToLower();
            if (text.Trim().Replace("mr. ", string.Empty).Replace("mrs. ", string.Empty).Replace("dr. ", string.Empty).Contains(' '))
                AddWarning();

            return (HiDescriptionWords.Contains(text) ||
                    text.StartsWith("engine ", StringComparison.Ordinal) ||
                    text.EndsWith("on tv", StringComparison.Ordinal) ||
                    text.EndsWith("shatters", StringComparison.Ordinal) ||
                    text.EndsWith("ing", StringComparison.Ordinal));
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

            return result.Trim(' ', '(', ')', '[', ']', '?', '{', '}');
        }

        private static int CompareLength(string a, string b)
        {
            return b.Length.CompareTo(a.Length);
        }

        public string RemoveInterjections(string text)
        {
            if (_interjectionList == null)
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

        private string RemoveStartEndTags(string text)
        {
            string newText = text;
            string s = text;
            int index;
            if (Settings.RemoveTextBetweenSquares && s.StartsWith('[') && (index = s.IndexOf(']', 1)) > 0)
            {
                if (++index < s.Length && s[index] == ':')
                    index++;
                newText = s.Remove(0, index);
            }
            else if (Settings.RemoveTextBetweenBrackets && s.StartsWith('{') && (index = s.IndexOf('}', 1)) > 0)
            {
                if (++index < s.Length && s[index] == ':')
                    index++;
                newText = s.Remove(0, index);
            }
            else if (Settings.RemoveTextBetweenParentheses && s.StartsWith('(') && (index = s.IndexOf(')', 1)) > 0)
            {
                if (++index < s.Length && s[index] == ':')
                    index++;
                newText = s.Remove(0, index);
            }
            else if (Settings.RemoveTextBetweenQuestionMarks && s.StartsWith('?') && (index = s.IndexOf('?', 1)) > 0)
            {
                if (++index < s.Length && s[index] == ':')
                    index++;
                newText = s.Remove(0, index);
            }
            else if (Settings.RemoveTextBetweenCustomTags &&
                     s.Length > 0 && Settings.CustomEnd.Length > 0 && Settings.CustomStart.Length > 0 &&
                     s.StartsWith(Settings.CustomStart, StringComparison.Ordinal) && (index = s.LastIndexOf(Settings.CustomEnd, StringComparison.Ordinal)) > 0)
            {
                newText = s.Remove(0, index + Settings.CustomEnd.Length);
            }
            if (newText != text)
                newText = newText.TrimStart(' ');
            return newText;
        }

        public string RemoveHearImpairedTags(string text)
        {
            string preAssTag = string.Empty;
            if (text.StartsWith("{\\", StringComparison.Ordinal) && text.IndexOf('}', 2) > 0)
            {
                int indexOfEndBracketSuccessor = text.IndexOf('}', 3) + 1;
                preAssTag = text.Substring(0, indexOfEndBracketSuccessor);
                text = text.Remove(0, indexOfEndBracketSuccessor).TrimStart();
            }
            string preNewLine = string.Empty;
            if (text.StartsWith(Environment.NewLine))
                preNewLine = Environment.NewLine;
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
            if (Settings.RemoveTextBetweenParentheses)
            {
                text = RemoveTextBetweenTags("(", "):", text);
                text = RemoveTextBetweenTags("(", ")", text);
            }
            if (Settings.RemoveTextBetweenQuestionMarks)
            {
                text = RemoveTextBetweenTags("?", "?:", text);
                text = RemoveTextBetweenTags("?", "?", text);
            }
            if (Settings.RemoveTextBetweenCustomTags && Settings.CustomStart.Length > 0 && Settings.CustomEnd.Length > 0)
            {
                text = RemoveTextBetweenTags(Settings.CustomStart, Settings.CustomEnd, text);
            }
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            return preAssTag + preNewLine + text.TrimStart();
        }

        private bool HasHearImpairedText(string text)
        {
            return RemoveHearImpairedTags(text) != text;
        }

        public bool HasHearImpariedTagsAtStart(string text)
        {
            if (Settings.OnlyIfInSeparateLine)
                return StartsAndEndsWithHearImpairedTags(text);
            return HasHearImpairedText(text);
        }

        public bool HasHearImpariedTagsAtEnd(string text)
        {
            if (Settings.OnlyIfInSeparateLine)
                return StartsAndEndsWithHearImpairedTags(text);
            return HasHearImpairedText(text);
        }

        private bool StartsAndEndsWithHearImpairedTags(string text)
        {
            return (Settings.RemoveTextBetweenSquares && text.StartsWith('[') && text.EndsWith(']') && !text.Trim('[').Contains('[')) ||
                   (Settings.RemoveTextBetweenBrackets && text.StartsWith('{') && text.EndsWith('}') && !text.Trim('{').Contains('{')) ||
                   (Settings.RemoveTextBetweenParentheses && text.StartsWith('(') && text.EndsWith(')') && !text.Trim('(').Contains('(')) ||
                   (Settings.RemoveTextBetweenQuestionMarks && text.StartsWith('?') && text.EndsWith('?') && !text.Trim('?').Contains('?')) ||
                   (Settings.RemoveTextBetweenSquares && text.StartsWith('[') && text.EndsWith("]:", StringComparison.Ordinal) && !text.Trim('[').Contains('[')) ||
                   (Settings.RemoveTextBetweenBrackets && text.StartsWith('{') && text.EndsWith("}:", StringComparison.Ordinal) && !text.Trim('{').Contains('{')) ||
                   (Settings.RemoveTextBetweenParentheses && text.StartsWith('(') && text.EndsWith("):", StringComparison.Ordinal) && !text.Trim('(').Contains('(')) ||
                   (Settings.RemoveTextBetweenQuestionMarks && text.StartsWith('?') && text.EndsWith("?:", StringComparison.Ordinal) && !text.Trim('?').Contains('?')) ||
                   (Settings.RemoveTextBetweenCustomTags &&
                    Settings.CustomStart.Length > 0 && Settings.CustomEnd.Length > 0 &&
                    text.StartsWith(Settings.CustomStart, StringComparison.Ordinal) && text.EndsWith(Settings.CustomEnd, StringComparison.Ordinal));
        }

        private static string RemoveTextBetweenTags(string startTag, string endTag, string text)
        {
            text = text.Trim();
            if (startTag == "?" || endTag == "?")
            {
                if (text.StartsWith(startTag, StringComparison.Ordinal) && text.EndsWith(endTag, StringComparison.Ordinal))
                    return string.Empty;
                return text;
            }

            int start = text.IndexOf(startTag, StringComparison.Ordinal);
            if (start < 0 || text.Length - start - startTag.Length < endTag.Length)
                return text;
            do
            {
                int end = text.IndexOf(endTag, start + startTag.Length, StringComparison.Ordinal);
                if (end < 0)
                    break;
                text = text.Remove(start, end - start + 1);
                if (start > 3 && text.Length - start > 1 &&
                    text[start] == ':' && text[start - 1] == ' ' && ".?!".Contains(text[start - 2]))
                {
                    text = text.Remove(start - 1, 2);
                }
                start = text.IndexOf(startTag, StringComparison.Ordinal);
            }
            while (start >= 0 && text.Length - start - startTag.Length >= endTag.Length);

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
                if (lineNoHtml == lineNoHtml.ToUpper() && lineNoHtml != lineNoHtml.ToLower())
                {
                    var temp = lineNoHtml.TrimEnd('.', '!', '?', ':').Trim().Trim(' ', '-', '—');
                    if (temp.Length == 1 || temp == "YES" || temp == "NO" || temp == "WHY" || temp == "HI")
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
