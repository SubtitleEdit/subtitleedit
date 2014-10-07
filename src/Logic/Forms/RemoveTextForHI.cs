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
            int i = 5;
            while (i < newText.Length)
            {
                string s = newText.Substring(i);
                if (i > 5 && s.Length > 2 && (s.StartsWith('.') || s.StartsWith('!') || s.StartsWith('?')))
                {
                    if (s[1] == ' ' || s.Substring(1).StartsWith("<i>") || s.Substring(1).StartsWith("</i>"))
                    {
                        string pre = " ";
                        if (s.Substring(1).StartsWith("<i>"))
                            pre = "<i>";
                        else if (s.Substring(1).StartsWith(" <i>"))
                            pre = " <i>";
                        else if (s.Substring(1).StartsWith("</i>"))
                            pre = "</i>";

                        s = s.Remove(0, 1 + pre.Length);
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
                            newText = newText.Replace("<i> </i>", " ");
                            newText = newText.Replace("  ", " ");
                            newText = newText.Replace("  ", " ");
                            newText = newText.Replace(" " + Environment.NewLine, Environment.NewLine);
                        }
                    }
                }
                i++;
            }
            return newText;
        }

        public string RemoveColon(string text)
        {
            if (!Settings.RemoveTextBeforeColon)
                return text;

            if (!text.Contains(':'))
                return text;

            // House 7x01 line 52: and she would like you to do three things:
            // Okay or remove???
            if (text.IndexOf(':') > 0 && text.IndexOf(':') == text.Length - 1 && text != text.ToUpper())
                return text;

            string newText = string.Empty;
            string[] parts = text.Trim().Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
            int noOfNames = 0;
            int count = 0;
            bool removedInFirstLine = false;
            bool removedInSecondLine = false;
            foreach (string s in parts)
            {
                int indexOfColon = s.IndexOf(':');
                if (indexOfColon > 0)
                {
                    string pre = s.Substring(0, indexOfColon);
                    if (Settings.RemoveTextBeforeColonOnlyUppercase && Utilities.RemoveHtmlTags(pre, true) != Utilities.RemoveHtmlTags(pre, true).ToUpper())
                    {
                        newText = newText + Environment.NewLine + s;
                        newText = newText.Trim();
                    }
                    else
                    {
                        var st = new StripableText(pre);
                        if (count == 1 && Utilities.CountTagInText(text, Environment.NewLine) == 1 && removedInFirstLine && Utilities.CountTagInText(s, ":") == 1 &&
                            !newText.EndsWith('.') && !newText.EndsWith('!') && !newText.EndsWith('?') && !newText.EndsWith(".</i>") && !newText.EndsWith("!</i>") && !newText.EndsWith("?</i>") &&
                            s != s.ToUpper())
                        {
                            if (pre.Contains("<i>") && s.Contains("</i>"))
                                newText = newText + Environment.NewLine + "<i>" + s;
                            else if (pre.Contains("<b>") && s.Contains("</b>"))
                                newText = newText + Environment.NewLine + "<b>" + s;
                            else if (pre.Contains('[') && s.Contains(']'))
                                newText = newText + Environment.NewLine + "[" + s;
                            else if (pre.Contains('(') && s.EndsWith(')'))
                                newText = newText + Environment.NewLine + "(" + s;
                            else
                                newText = newText + Environment.NewLine + s;
                        }
                        else if (count == 1 && Utilities.CountTagInText(text, Environment.NewLine) == 1 && indexOfColon > 15 && s.Substring(0, indexOfColon).Contains(' ') && Utilities.CountTagInText(s, ":") == 1 &&
                            !newText.EndsWith('.') && !newText.EndsWith('!') && !newText.EndsWith('?') && !newText.EndsWith(".</i>") && !newText.EndsWith("!</i>") && !newText.EndsWith("?</i>") &&
                            s != s.ToUpper())
                        {
                            if (pre.Contains("<i>") && s.Contains("</i>"))
                                newText = newText + Environment.NewLine + "<i>" + s;
                            else if (pre.Contains("<b>") && s.Contains("</b>"))
                                newText = newText + Environment.NewLine + "<b>" + s;
                            else if (pre.Contains('[') && s.Contains(']'))
                                newText = newText + Environment.NewLine + "[" + s;
                            else if (pre.Contains('(') && s.EndsWith(')'))
                                newText = newText + Environment.NewLine + "(" + s;
                            else
                                newText = newText + Environment.NewLine + s;
                        }
                        else if (Utilities.CountTagInText(s, ":") == 1)
                        {
                            bool remove = true;
                            if (indexOfColon > 0 && indexOfColon < s.Length - 1)
                            {
                                if (char.IsDigit(s[indexOfColon - 1]) && char.IsDigit(s[indexOfColon + 1]))
                                    remove = false;
                            }
                            if (s.StartsWith("Previously on") || s.StartsWith("<i>Previously on"))
                                remove = false;

                            if (remove && Settings.ColonSeparateLine)
                            {
                                if (indexOfColon == s.Length - 1 || s.Substring(indexOfColon + 1).StartsWith(Environment.NewLine))
                                    remove = true;
                                else
                                    remove = false;
                            }

                            if (remove)
                            {
                                string content = s.Substring(indexOfColon + 1).Trim();
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
                                else if (newText.EndsWith("</i>") && text.StartsWith("<i>") && !newText.StartsWith("<i>"))
                                    newText = "<i>" + newText;
                                else if (newText.EndsWith("</b>") && text.StartsWith("<b>") && !newText.StartsWith("<b>"))
                                    newText = "<b>" + newText;

                                if (!IsHIDescription(st.StrippedText))
                                    noOfNames++;
                            }
                            else
                            {
                                newText = newText + Environment.NewLine + s;
                                newText = newText.Trim();
                                if (newText.EndsWith("</i>") && text.StartsWith("<i>") && !newText.StartsWith("<i>"))
                                    newText = "<i>" + newText;
                                else if (newText.EndsWith("</b>") && text.StartsWith("<b>") && !newText.StartsWith("<b>"))
                                    newText = "<b>" + newText;
                            }
                        }
                        else
                        {
                            string s2 = s;
                            for (int k = 0; k < 2; k++)
                            {
                                if (s2.Contains(':'))
                                {
                                    int colonIndex = s2.IndexOf(':');
                                    string start = s2.Substring(0, colonIndex);

                                    if (!Settings.RemoveTextBeforeColonOnlyUppercase || start == start.ToUpper())
                                    {
                                        int periodIndex = start.LastIndexOf(". ", StringComparison.Ordinal);
                                        int questIndex = start.LastIndexOf("? ", StringComparison.Ordinal);
                                        int exclaIndex = start.LastIndexOf("! ", StringComparison.Ordinal);
                                        int endIndex = periodIndex;
                                        if (endIndex == -1 || questIndex > endIndex)
                                            endIndex = questIndex;
                                        if (endIndex == -1 || exclaIndex > endIndex)
                                            endIndex = exclaIndex;
                                        if (colonIndex > 0 && colonIndex < s2.Length - 1)
                                        {
                                            if (char.IsDigit(s2[colonIndex - 1]) && char.IsDigit(s2[colonIndex + 1]))
                                                endIndex = -10;
                                        }
                                        if (endIndex == -1)
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
                    newText = newText + Environment.NewLine + s;
                    newText = newText.Trim();

                    if (newText.EndsWith("</i>") && text.StartsWith("<i>") && !newText.StartsWith("<i>"))
                        newText = "<i>" + newText;
                    else if (newText.EndsWith("</b>") && text.StartsWith("<b>") && !newText.StartsWith("<b>"))
                        newText = "<b>" + newText;
                }
                count++;
            }
            newText = newText.Trim();
            if (noOfNames > 0 && Utilities.CountTagInText(newText, Environment.NewLine) == 1)
            {
                int indexOfDialogChar = newText.IndexOf('-');
                bool insertDash = true;
                string[] arr = newText.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 2 && arr[0].Length > 1 && arr[1].Length > 1)
                {
                    string arr0 = new StripableText(arr[0]).StrippedText;
                    string arr1 = new StripableText(arr[1]).StrippedText;

                    //line continuation?
                    if (arr0.Length > 0 && arr1.Length > 1 && (Utilities.LowercaseLetters + ",").Contains(arr0.Substring(arr0.Length - 1)) &&
                        Utilities.LowercaseLetters.Contains(arr1[0]))
                    {
                        if (new StripableText(arr[1]).Pre.Contains("...") == false)
                            insertDash = false;
                    }

                    if (arr0.Length > 0 && arr1.Length > 1 && !(arr[0].EndsWith('.') || arr[0].EndsWith('!') || arr[0].EndsWith('?') || arr[0].EndsWith("</i>")) &&
                        !(new StripableText(arr[1]).Pre.Contains('-')))
                    {
                        insertDash = false;
                    }

                    if (removedInFirstLine && !removedInSecondLine && !text.StartsWith('-') && !text.StartsWith("<i>-"))
                    {
                        if (!insertDash || (!arr[1].StartsWith('-') && !arr[1].StartsWith("<i>-")))
                            insertDash = false;
                    }
                }

                if (insertDash)
                {
                    if (indexOfDialogChar < 0 || indexOfDialogChar > 4)
                    {
                        var st = new StripableText(newText, "", "");
                        newText = st.Pre + "- " + st.StrippedText + st.Post;
                    }

                    int indexOfNewLine = newText.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                    string second = newText.Substring(indexOfNewLine).Trim();
                    indexOfDialogChar = second.IndexOf('-');
                    if (indexOfDialogChar < 0 || indexOfDialogChar > 6)
                    {
                        var st = new StripableText(second, "", "");
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
            else if (Utilities.CountTagInText(newText, Environment.NewLine) == 1 && removedInFirstLine == false && removedInSecondLine)
            {
                string noTags = Utilities.RemoveHtmlTags(newText, true).Trim();
                bool insertDash = noTags.StartsWith('-') && Utilities.CountTagInText(noTags, "-") == 1;
                if (insertDash)
                {
                    if (newText.Contains(Environment.NewLine + "<i>"))
                        newText = newText.Replace(Environment.NewLine + "<i>", Environment.NewLine + "<i>- ");
                    else
                        newText = newText.Replace(Environment.NewLine, Environment.NewLine + "- ");
                }
            }
            if (text.Contains("<i>") && !newText.Contains("<i>") && newText.EndsWith("</i>"))
                newText = "<i>" + newText;
            return newText;
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
            string[] parts = st.StrippedText.Trim().Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
            int lineNumber = 0;
            bool removedDialogInFirstLine = false;
            int noOfNamesRemoved = 0;
            int noOfNamesRemovedNotInLineOne = 0;
            foreach (string s in parts)
            {
                var stSub = new StripableText(s, pre, post);
                if (!StartAndEndsWithHearImpariedTags(stSub.StrippedText))
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

                    if (st.Pre.Contains("<i>") && stSub.Post.Contains("</i>"))
                        st.Pre = st.Pre.Replace("<i>", string.Empty);

                    if (s.Contains("<i>") && !s.Contains("</i>") && st.Post.Contains("</i>"))
                        st.Post = st.Post.Replace("</i>", string.Empty);
                }
                lineNumber++;
            }

            text = st.Pre + sb.ToString().Trim() + st.Post;
            text = text.Replace("<i></i>", string.Empty).Trim();
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
            if (noOfNamesRemoved >= 1 && Utilities.CountTagInText(text, Environment.NewLine) == 2)
            {
                string[] a = Utilities.RemoveHtmlTags(text).Replace(" ", string.Empty).Split(new[] { '!', '?', '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (a.Length == 2)
                {
                    var temp = new StripableText(text);
                    temp.StrippedText = temp.StrippedText.Replace(Environment.NewLine, " ");
                    int splitIndex = temp.StrippedText.LastIndexOf('!');
                    if (splitIndex == -1)
                        splitIndex = temp.StrippedText.LastIndexOf('?');
                    if (splitIndex == -1)
                        splitIndex = temp.StrippedText.LastIndexOf('.');
                    if (splitIndex > 0)
                    {
                        text = temp.Pre + temp.StrippedText.Insert(splitIndex + 1, Environment.NewLine) + temp.Post;
                    }
                }
            }

            if (!text.StartsWith('-') && noOfNamesRemoved >= 1 && Utilities.CountTagInText(text, Environment.NewLine) == 1)
            {
                string[] arr = text.Split(Utilities.NewLineChars);
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

            if (!string.IsNullOrEmpty(text))
                text = st.Pre + text + st.Post;

            if (oldText.TrimStart().StartsWith("- ") &&
                (oldText.Contains(Environment.NewLine + "- ") || oldText.Contains(Environment.NewLine + " - ")) && !text.Contains(Environment.NewLine))
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
                if (text.StartsWith("<i>-") && text.Length > 5 && text[4] != ' ' && text[4] != '-')
                    text = text.Insert(4, " ");
                if (text.Contains(Environment.NewLine + "-"))
                {
                    int index = text.IndexOf(Environment.NewLine + "-", StringComparison.Ordinal);
                    if (index + 4 < text.Length && text[index + Environment.NewLine.Length + 1] != ' ' && text[index + Environment.NewLine.Length + 1] != '-')
                        text = text.Insert(index + Environment.NewLine.Length + 1, " ");
                }
                if (text.Contains(Environment.NewLine + "<i>-"))
                {
                    int index = text.IndexOf(Environment.NewLine + "<i>-", StringComparison.Ordinal);
                    if (index + 5 < text.Length && text[index + Environment.NewLine.Length + 4] != ' ' && text[index + Environment.NewLine.Length + 4] != '-')
                        text = text.Insert(index + Environment.NewLine.Length + 4, " ");
                }
            }
            return text.Trim();
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

            if (text == "sighing" ||
                text == "cackles" ||
                text == "cheers" ||
                text == "chitters" ||
                text == "chuckles" ||
                text == "exclaims" ||
                text == "gasps" ||
                text == "grunts" ||
                text == "groans" ||
                text == "growls" ||
                text == "explosion" ||
                text == "laughs" ||
                text == "noise" ||
                text.StartsWith("engine ") ||
                text == "roars" ||
                text == "scoff" ||
                text == "screeches" ||
                text == "shouts" ||
                text == "shrieks" ||
                text == "sigh" ||
                text == "sighs" ||
                text == "singing" ||
                text == "snores" ||
                text == "stutters" ||
                text == "thuds" ||
                text == "trumpets" ||
                text == "whispers" ||
                text == "whisper" ||
                text == "whistles" ||
                text.EndsWith("ing"))
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

                            if (removeAfter)
                            {
                                if (index == 0)
                                {
                                    if (!string.IsNullOrEmpty(temp) && temp.StartsWith('-'))
                                        temp = temp.Remove(0, 1).Trim();
                                }
                                else if (index == 3 && !string.IsNullOrEmpty(temp) && temp.StartsWith("<i>-"))
                                {
                                    temp = temp.Remove(3, 1);
                                }
                                else if (index > 0)
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
            string[] lines = text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
            if (text != oldText && lines.Length == 2)
            {
                if (lines[0] == "-" && lines[1] == "-")
                    return string.Empty;
                if (lines[0].StartsWith('-') && lines[0].Length > 1 && lines[1].Trim() == "-")
                    return lines[0].Remove(0, 1).Trim();
                if (lines[1].StartsWith('-') && lines[1].Length > 1 && lines[0].Trim() == "-")
                    return lines[1].Remove(0, 1).Trim();
                if (lines[0].Length > 1 && (lines[1] == "-") || lines[1] == "." || lines[1] == "!" || lines[1] == "?")
                {
                    if (oldText.Contains(Environment.NewLine + "-") && lines[0].StartsWith('-'))
                        lines[0] = lines[0].Remove(0, 1);
                    return lines[0].Trim();
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

            return text;
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
                   (text.StartsWith('[') && text.EndsWith("]:") && !text.Trim('[').Contains('[') && Settings.RemoveTextBetweenSquares) ||
                   (text.StartsWith('{') && text.EndsWith("}:") && !text.Trim('{').Contains('{') && Settings.RemoveTextBetweenBrackets) ||
                   (text.StartsWith('?') && text.EndsWith("?:") && !text.Trim('?').Contains('?') && Settings.RemoveTextBetweenQuestionMarks) ||
                   (text.StartsWith('(') && text.EndsWith("):") && !text.Trim('(').Contains('(') && Settings.RemoveTextBetweenParentheses) ||
                   (Settings.RemoveTextBetweenCustomTags &&
                    Settings.CustomStart.Length > 0 && Settings.CustomEnd.Length > 0 &&
                    text.StartsWith(Settings.CustomStart) && text.EndsWith(Settings.CustomEnd));
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
            if (start == -1 || start == text.Length - 1)
                return text;

            int end = text.IndexOf(endTag, start + 1, StringComparison.Ordinal);
            while (start >= 0 && end > start)
            {
                text = text.Remove(start, (end - start) + 1);
                start = text.IndexOf(startTag, StringComparison.Ordinal);
                if (start >= 0 && start < text.Length - 1)
                    end = text.IndexOf(endTag, start, StringComparison.Ordinal);
                else
                    end = -1;
            }
            return text.Replace(" " + Environment.NewLine, Environment.NewLine).TrimEnd();
        }

        public string RemoveLineIfAllUppercase(string text)
        {
            if (!Settings.RemoveIfAllUppercase)
                return text;

            string[] lines = text.Replace(Environment.NewLine, "\n").Replace("\r", "\n").Split('\n');
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                string lineNoHtml = Utilities.RemoveHtmlTags(line);
                string tmp = lineNoHtml.TrimEnd('.', '!', '?', ':').Trim();
                if (lineNoHtml != lineNoHtml.ToLower() && lineNoHtml == lineNoHtml.ToUpper())
                {
                    if (tmp == "YES" || tmp == "NO" || tmp == "WHY" || tmp == "HI" || tmp.Length == 1)
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
