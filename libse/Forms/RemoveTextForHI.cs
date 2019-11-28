using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public class RemoveTextForHI
    {
        public RemoveTextForHISettings Settings { get; set; }

        public List<int> Warnings { get; set; }
        public int WarningIndex { get; set; }

        // interjection
        private readonly InterjectionRemoveContext _interjectionRemoveContext;
        private readonly RemoveInterjection _removeInterjection;
        private IList<string> _interjections;

        public RemoveTextForHI(RemoveTextForHISettings removeTextForHISettings)
        {
            Settings = removeTextForHISettings;
            _removeInterjection = new RemoveInterjection();
            _interjectionRemoveContext = new InterjectionRemoveContext();
        }

        public string RemoveHearingImpairedTagsInsideLine(string newText)
        {
            const string endChars = ".?!";
            for (int i = 6; i < newText.Length; i++)
            {
                var s = newText.Substring(i);
                if (s.Length > 2 && endChars.Contains(s[0]))
                {
                    var pre = string.Empty;

                    s = s.Remove(0, 1);
                    if (s.StartsWith(' '))
                    {
                        pre = s.StartsWith(" <i>", StringComparison.Ordinal) ? " <i>" : " ";
                    }
                    else if (s.StartsWith("<i>", StringComparison.Ordinal))
                    {
                        pre = "<i>";
                    }
                    else if (s.StartsWith("</i>", StringComparison.Ordinal))
                    {
                        pre = "</i>";
                    }

                    if (pre.Length > 0)
                    {
                        s = s.Remove(0, pre.Length);
                        if (s.Length > 1 && s[0] == ' ')
                        {
                            pre += " ";
                            s = s.Remove(0, 1);
                        }

                        if (HasHearImpairedTagsAtStartOrEnd(s))
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
            {
                return text;
            }

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
            if (noTagText.Length > 10 && noTagText.IndexOf(':') == noTagText.Length - 1 && noTagText != noTagText.ToUpperInvariant())
            {
                return preAssTag + text;
            }

            string newText = string.Empty;
            var lines = text.Trim().SplitToLines();
            int noOfNames = 0;
            int count = 0;
            bool removedInFirstLine = false;
            bool removedInSecondLine = false;
            foreach (string line in lines)
            {
                int indexOfColon = line.IndexOf(':');
                bool isLastColon = count == lines.Count - 1 && !HtmlUtil.RemoveHtmlTags(line).TrimEnd(':').Contains(':');
                if (indexOfColon <= 0 || IsInsideBrackets(line, indexOfColon) || (isLastColon && Utilities.CountTagInText(HtmlUtil.RemoveHtmlTags(line), ' ') > 1))
                {
                    newText = (newText + Environment.NewLine + line).Trim();

                    if (newText.EndsWith("</i>", StringComparison.Ordinal) && text.StartsWith("<i>", StringComparison.Ordinal) && !newText.StartsWith("<i>", StringComparison.Ordinal))
                    {
                        newText = "<i>" + newText;
                    }
                    else if (newText.EndsWith("</b>", StringComparison.Ordinal) && text.StartsWith("<b>", StringComparison.Ordinal) && !newText.StartsWith("<b>", StringComparison.Ordinal))
                    {
                        newText = "<b>" + newText;
                    }
                }
                else
                {
                    var pre = line.Substring(0, indexOfColon);
                    var noTagPre = HtmlUtil.RemoveHtmlTags(pre, true);
                    if (Settings.RemoveTextBeforeColonOnlyUppercase && noTagPre != noTagPre.ToUpperInvariant())
                    {
                        bool remove = true;
                        newText = RemovePartialBeforeColon(line, indexOfColon, newText, count, ref removedInFirstLine, ref removedInSecondLine, ref remove);
                        if (remove)
                        {
                            string s = line;
                            string l1Trim = HtmlUtil.RemoveHtmlTags(lines[0]).TrimEnd('"');
                            if (count == 1 && lines.Count == 2 && !l1Trim.EndsWith('.') &&
                                                                   !l1Trim.EndsWith('!') &&
                                                                   !l1Trim.EndsWith('?'))
                            {
                                var indexOf = line.IndexOfAny(ExpectedStrings, StringComparison.Ordinal);
                                if (indexOf > 0 && indexOf < indexOfColon)
                                {
                                    var toRemove = s.Substring(indexOf + 1, indexOfColon - indexOf).Trim();
                                    if (toRemove.Length > 1 && toRemove == toRemove.ToUpperInvariant())
                                    {
                                        s = s.Remove(indexOf + 1, indexOfColon - indexOf);
                                        s = s.Insert(indexOf + 1, " -");
                                        if (newText.StartsWith("<i>", StringComparison.Ordinal) && !newText.StartsWith("<i>-", StringComparison.Ordinal))
                                        {
                                            newText = "<i>- " + newText.Remove(0, 3);
                                        }
                                        else if (!newText.StartsWith('-'))
                                        {
                                            newText = "- " + newText;
                                        }
                                    }
                                }
                            }
                            newText = (newText + Environment.NewLine + s).Trim();
                        }
                    }
                    else
                    {
                        var newTextNoHtml = HtmlUtil.RemoveHtmlTags(newText);
                        if (Utilities.CountTagInText(line, ':') == 1)
                        {
                            if (count == 1 && newText.Length > 1 && removedInFirstLine &&
                                !".?!♪♫".Contains(newTextNoHtml[newTextNoHtml.Length - 1]) && newText.LineEndsWithHtmlTag(true) &&
                                line != line.ToUpperInvariant())
                            {
                                newText += Environment.NewLine;
                                if (pre.Contains("<i>") && line.Contains("</i>") && !line.Contains("<i>"))
                                {
                                    newText += "<i>" + line;
                                }
                                else if (pre.Contains("<b>") && line.Contains("</b>") && !line.Contains("<b>"))
                                {
                                    newText += "<b>" + line;
                                }
                                else if (pre.Contains("<u>") && line.Contains("</u>") && !line.Contains("<u>"))
                                {
                                    newText += "<u>" + line;
                                }
                                else if (pre.Contains('[') && line.Contains(']') && !line.Contains("["))
                                {
                                    newText += "[" + line;
                                }
                                else if (pre.Contains('(') && line.EndsWith(')') && !line.Contains("("))
                                {
                                    newText += "(" + line;
                                }
                                else
                                {
                                    newText += line;
                                }
                            }
                            else if (count == 1 && newTextNoHtml.Length > 1 && indexOfColon > 15 && line.Substring(0, indexOfColon).Contains(' ') &&
                                     !".?!♪♫".Contains(newTextNoHtml[newTextNoHtml.Length - 1]) && newText.LineEndsWithHtmlTag(true) &&
                                     line != line.ToUpperInvariant())
                            {
                                newText += Environment.NewLine;
                                if (pre.Contains("<i>") && line.Contains("</i>") && !line.Contains("<i>"))
                                {
                                    newText += "<i>" + line;
                                }
                                else if (pre.Contains("<b>") && line.Contains("</b>") && !line.Contains("<b>"))
                                {
                                    newText += "<b>" + line;
                                }
                                else if (pre.Contains("<u>") && line.Contains("</u>") && !line.Contains("<u>"))
                                {
                                    newText += "<u>" + line;
                                }
                                else if (pre.Contains('[') && line.Contains(']') && !line.Contains("["))
                                {
                                    newText += "[" + line;
                                }
                                else if (pre.Contains('(') && line.EndsWith(')') && !line.Contains("("))
                                {
                                    newText += "(" + line;
                                }
                                else
                                {
                                    newText += line;
                                }
                            }
                            else
                            {
                                var preStrippable = new StrippableText(pre);
                                var remove = true;

                                if (indexOfColon < line.Length - 1)
                                {
                                    if (Settings.ColonSeparateLine && !line.Substring(indexOfColon + 1).StartsWith(Environment.NewLine, StringComparison.Ordinal))
                                    {
                                        remove = false;
                                    }
                                    else if (Utilities.IsBetweenNumbers(line, indexOfColon))
                                    {
                                        remove = false;
                                    }
                                }

                                if (remove && !ShouldRemoveNarrator(pre))
                                {
                                    remove = false;
                                }

                                string l1Trimmed = HtmlUtil.RemoveHtmlTags(lines[0]).TrimEnd('"');
                                if (count == 1 && lines.Count == 2 && !l1Trimmed.EndsWith('.') &&
                                    !l1Trimmed.EndsWith('!') &&
                                    !l1Trimmed.EndsWith('?') &&
                                    !l1Trimmed.EndsWith('♪') &&
                                    !l1Trimmed.EndsWith('♫') &&
                                    !l1Trimmed.EndsWith("--", StringComparison.Ordinal) &&
                                    !l1Trimmed.EndsWith("—", StringComparison.Ordinal))
                                {
                                    remove = false;
                                }

                                if (remove)
                                {
                                    newText = RemovePartialBeforeColon(line, indexOfColon, newText, count, ref removedInFirstLine, ref removedInSecondLine, ref remove);

                                    if (remove)
                                    {
                                        var content = line.Substring(indexOfColon + 1).Trim();
                                        if (content.Length > 0)
                                        {
                                            if (content.StartsWith("</font>") &&
                                                Utilities.CountTagInText(preStrippable.Pre, "<font ") >
                                                Utilities.CountTagInText(preStrippable.Pre, "</font>"))
                                            {
                                                content = content.Remove(0, "</font>".Length);
                                            }
                                            else if (content.StartsWith("</i>") &&
                                                     Utilities.CountTagInText(preStrippable.Pre, "<i>") >
                                                     Utilities.CountTagInText(preStrippable.Pre, "</i>"))
                                            {
                                                content = content.Remove(0, "</i>".Length);
                                            }
                                            else if (content.StartsWith("</b>") &&
                                                     Utilities.CountTagInText(preStrippable.Pre, "<b>") >
                                                     Utilities.CountTagInText(preStrippable.Pre, "</b>"))
                                            {
                                                content = content.Remove(0, "</b>".Length);
                                            }

                                            if (count == 0 && !string.IsNullOrEmpty(content) && content[0].ToString() != content[0].ToString().ToUpperInvariant())
                                            {
                                                content = content[0].ToString().ToUpperInvariant() + content.Remove(0, 1);
                                            }
                                            else if (count == 1 && !string.IsNullOrEmpty(content) && content[0].ToString() != content[0].ToString().ToUpperInvariant())
                                            {
                                                content = content[0].ToString().ToUpperInvariant() + content.Remove(0, 1);
                                            }

                                            newText += Environment.NewLine;
                                            if (pre.Contains("<i>") && content.Contains("</i>"))
                                            {
                                                newText += "<i>" + content;
                                            }
                                            else if (pre.Contains("<b>") && content.Contains("</b>"))
                                            {
                                                newText += "<b>" + content;
                                            }
                                            else if (pre.Contains('[') && content.Contains(']'))
                                            {
                                                newText += "[" + content;
                                            }
                                            else if (pre.Contains('(') && content.EndsWith(')'))
                                            {
                                                newText += "(" + content;
                                            }
                                            else
                                            {
                                                newText += content;
                                            }

                                            if (count == 0)
                                            {
                                                removedInFirstLine = true;
                                            }
                                            else if (count == 1)
                                            {
                                                removedInSecondLine = true;
                                            }
                                        }
                                    }

                                    newText = newText.Trim();

                                    if (text.StartsWith('(') && newText.EndsWith(')') && !newText.Contains('('))
                                    {
                                        newText = newText.TrimEnd(')');
                                    }
                                    else if (text.StartsWith('[') && newText.EndsWith(']') && !newText.Contains('['))
                                    {
                                        newText = newText.TrimEnd(']');
                                    }
                                    else if (newText.EndsWith("</i>", StringComparison.Ordinal) && text.StartsWith("<i>", StringComparison.Ordinal) && !newText.StartsWith("<i>", StringComparison.Ordinal))
                                    {
                                        newText = "<i>" + newText;
                                    }
                                    else if (newText.EndsWith("</b>", StringComparison.Ordinal) && text.StartsWith("<b>", StringComparison.Ordinal) && !newText.StartsWith("<b>", StringComparison.Ordinal))
                                    {
                                        newText = "<b>" + newText;
                                    }
                                    else if (newText.EndsWith("</u>", StringComparison.Ordinal) && text.StartsWith("<u>", StringComparison.Ordinal) && !newText.StartsWith("<u>", StringComparison.Ordinal))
                                    {
                                        newText = "<u>" + newText;
                                    }

                                    if (!IsHIDescription(preStrippable.StrippedText))
                                    {
                                        noOfNames++;
                                    }
                                }
                                else
                                {
                                    string s = line;
                                    string l1Trim = HtmlUtil.RemoveHtmlTags(lines[0]).TrimEnd('"');
                                    if (count == 1 && lines.Count == 2 && !l1Trim.EndsWith('.') &&
                                                                           !l1Trim.EndsWith('!') &&
                                                                           !l1Trim.EndsWith('?'))
                                    {
                                        int indexOf = line.IndexOf(". ", StringComparison.Ordinal);
                                        if (indexOf == -1)
                                        {
                                            indexOf = line.IndexOf("! ", StringComparison.Ordinal);
                                        }

                                        if (indexOf == -1)
                                        {
                                            indexOf = line.IndexOf("? ", StringComparison.Ordinal);
                                        }

                                        if (indexOf > 0 && indexOf < indexOfColon)
                                        {
                                            s = s.Remove(indexOf + 1, indexOfColon - indexOf);
                                            s = s.Insert(indexOf + 1, " -");
                                            if (newText.StartsWith("<i>", StringComparison.Ordinal) && !newText.StartsWith("<i>-", StringComparison.Ordinal))
                                            {
                                                newText = "<i>- " + newText.Remove(0, 3);
                                            }
                                            else if (!newText.StartsWith('-'))
                                            {
                                                newText = "- " + newText;
                                            }
                                        }
                                    }
                                    newText = (newText + Environment.NewLine + s).Trim();
                                    if (newText.EndsWith("</i>", StringComparison.Ordinal) && text.StartsWith("<i>", StringComparison.Ordinal) && !newText.StartsWith("<i>", StringComparison.Ordinal))
                                    {
                                        newText = "<i>" + newText;
                                    }
                                    else if (newText.EndsWith("</b>", StringComparison.Ordinal) && text.StartsWith("<b>", StringComparison.Ordinal) && !newText.StartsWith("<b>", StringComparison.Ordinal))
                                    {
                                        newText = "<b>" + newText;
                                    }
                                    else if ((newText.EndsWith("</u>", StringComparison.Ordinal) && text.StartsWith("<u>", StringComparison.Ordinal) && !newText.StartsWith("<u>", StringComparison.Ordinal)))
                                    {
                                        newText = "<u>" + newText;
                                    }
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

                                    if (!Settings.RemoveTextBeforeColonOnlyUppercase || start == start.ToUpperInvariant())
                                    {
                                        int endIndex = start.LastIndexOfAny(endChars);
                                        if (colonIndex > 0 && colonIndex < s2.Length - 1)
                                        {
                                            if (char.IsDigit(s2[colonIndex - 1]) && char.IsDigit(s2[colonIndex + 1]))
                                            {
                                                endIndex = 0;
                                            }
                                        }
                                        if (endIndex < 0)
                                        {
                                            s2 = s2.Remove(0, colonIndex - endIndex);
                                        }
                                        else if (endIndex > 0)
                                        {
                                            s2 = s2.Remove(endIndex + 1, colonIndex - endIndex);
                                        }
                                    }

                                    if (count == 0)
                                    {
                                        removedInFirstLine = true;
                                    }
                                    else if (count == 1)
                                    {
                                        removedInSecondLine = true;
                                    }
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
                if (arr.Count == 2 && arr[0].Length > 1 && arr[1].Length > 1)
                {
                    string arr0 = new StrippableText(arr[0]).StrippedText;
                    var arr1Strippable = new StrippableText(arr[1]);
                    string arr1 = arr1Strippable.StrippedText;

                    if (arr0.Length > 0 && arr1.Length > 1)
                    {
                        // line continuation?
                        if (char.IsLower(arr1[0]) && arr[1][0] != '(' && arr[1][0] != '[') // second line starts with lower case letter
                        {
                            char c = arr0[arr0.Length - 1];
                            if (char.IsLower(c) || c == ',') // first line ends with comma or lower case letter
                            {
                                if (!arr1Strippable.Pre.Contains("..."))
                                {
                                    insertDash = false;
                                }
                            }
                        }

                        if (insertDash)
                        {
                            string arr0QuoteTrimmed = arr[0].TrimEnd('"');
                            if (arr0QuoteTrimmed.Length > 0 && !".?!♪♫".Contains(arr0QuoteTrimmed[arr0QuoteTrimmed.Length - 1]) && !arr0QuoteTrimmed.EndsWith("</i>", StringComparison.Ordinal) && !arr0QuoteTrimmed.EndsWith("--", StringComparison.Ordinal) && !arr0QuoteTrimmed.EndsWith("—", StringComparison.Ordinal))
                            {
                                if (!arr1Strippable.Pre.Contains('-'))
                                {
                                    insertDash = false;
                                }
                            }
                        }
                    }

                    if (insertDash && removedInFirstLine && !removedInSecondLine && !text.StartsWith('-') && !text.StartsWith("<i>-", StringComparison.Ordinal))
                    {
                        if (!arr[1].StartsWith('-') && !arr[1].StartsWith("<i>-", StringComparison.Ordinal))
                        {
                            insertDash = false;
                        }
                    }
                }

                if (insertDash)
                {
                    if (indexOfDialogChar < 0 || indexOfDialogChar > 4)
                    {
                        var st = new StrippableText(newText, string.Empty, string.Empty);
                        newText = st.Pre + "- " + st.StrippedText + st.Post;
                    }

                    int indexOfNewLine = newText.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                    string second = newText.Substring(indexOfNewLine).Trim();
                    indexOfDialogChar = second.IndexOf(" -", StringComparison.Ordinal);
                    if (indexOfDialogChar < 0 || indexOfDialogChar > 6)
                    {
                        indexOfDialogChar = second.IndexOf("- ", StringComparison.Ordinal);
                    }
                    if ((indexOfDialogChar < 0 || indexOfDialogChar > 6) && !(second.StartsWith('-') || second.StartsWith("<i>-", StringComparison.Ordinal)))
                    {
                        var st = new StrippableText(second, String.Empty, String.Empty);
                        second = st.Pre + "- " + st.StrippedText + st.Post;
                        var firstLine = newText.Remove(indexOfNewLine);
                        newText = firstLine + Environment.NewLine + second;

                        if (firstLine.Length > 0 && HtmlUtil.RemoveHtmlTags(text, true).StartsWith('-') &&
                            !HtmlUtil.RemoveHtmlTags(newText, true).StartsWith('-'))
                        {
                            firstLine = HtmlUtil.RemoveHtmlTags(firstLine, true);
                            if (firstLine.Length > 0 && "!?.".Contains(firstLine[firstLine.Length - 1]))
                            {
                                newText = "- " + newText.Trim();
                            }
                        }
                    }
                }
            }
            else if (newText.Contains('-') && !newText.Contains(Environment.NewLine))
            {
                var st = new StrippableText(newText);
                if (st.Pre.Contains('-') && !st.Pre.Contains("--"))
                {
                    newText = st.Pre.RemoveChar('-') + st.StrippedText + st.Post;
                }
            }
            else if (removedInSecondLine && !removedInFirstLine && Utilities.GetNumberOfLines(newText) == 2)
            {
                string noTags = HtmlUtil.RemoveHtmlTags(newText, true).Trim();
                bool insertDash = noTags.StartsWith('-') && Utilities.CountTagInText(noTags, '-') == 1;
                if (insertDash)
                {
                    if (newText.Contains(Environment.NewLine + "<i>"))
                    {
                        newText = newText.Replace(Environment.NewLine + "<i>", Environment.NewLine + "<i>- ");
                    }
                    else
                    {
                        newText = newText.Replace(Environment.NewLine, Environment.NewLine + "- ");
                    }
                }
            }
            if (text.Contains("<i>") && !newText.Contains("<i>") && newText.EndsWith("</i>", StringComparison.Ordinal))
            {
                newText = "<i>" + newText;
            }

            if (string.IsNullOrWhiteSpace(newText))
            {
                return string.Empty;
            }

            return preAssTag + newText;
        }

        private string RemovePartialBeforeColon(string line, int indexOfColon, string newText, int count, ref bool removedInFirstLine, ref bool removedInSecondLine, ref bool remove)
        {
            var lastIndexOfPeriod = line.Substring(0, indexOfColon).LastIndexOf("... ", StringComparison.Ordinal);
            if (lastIndexOfPeriod > 10)
            {
                var s = line.Substring(lastIndexOfPeriod, indexOfColon - lastIndexOfPeriod);
                s = s.Trim('.', '-', ' ', '!', '?', '"', '\'');
                if (IsHIDescription(s) || Settings.NameList != null && Settings.NameList.ContainsCaseInsensitive(s))
                {
                    var partialRemove = false;
                    if (Settings.RemoveTextBeforeColonOnlyUppercase)
                    {
                        if (s == s.ToUpperInvariant())
                        {
                            partialRemove = true;
                        }
                    }
                    else
                    {
                        partialRemove = true;
                    }

                    if (partialRemove)
                    {
                        newText = line.Remove(lastIndexOfPeriod + 4, indexOfColon - lastIndexOfPeriod - 3);
                        if (newText.Substring(lastIndexOfPeriod + 3).StartsWith("  "))
                        {
                            newText = newText.Remove(lastIndexOfPeriod + 3, 1);
                        }

                        if (count == 0)
                        {
                            removedInFirstLine = true;
                        }
                        else if (count == 1)
                        {
                            removedInSecondLine = true;
                        }
                    }

                    remove = false;
                }
            }

            return newText;
        }

        private static bool IsInsideBrackets(string text, int targetIndex)
        {
            // <i>♪ (THE CAPITOLS: "COOL JERK") ♪</i>
            var index = text.LastIndexOf('(', targetIndex - 1) + 1;
            if (index > 0 && text.IndexOf(')', index) > targetIndex)
            {
                return true;
            }

            index = text.LastIndexOf('[', targetIndex - 1) + 1;
            if (index > 0 && text.IndexOf(']', index) > targetIndex)
            {
                return true;
            }

            return false;
        }

        private static bool ShouldRemoveNarrator(string pre)
        {
            // Skip these: Barry, remember: She cannot; http://google.com; Improved by: ...
            if (pre.IndexOfAny(new[] { "Previously on", "Improved by", "http", ", " }, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            // Okay! Narrator: Hello!
            return pre.IndexOfAny(new[] { '!', '?', '¿', '¡' }) < 0;
        }

        private static readonly char[] TrimStartNoiseChar = { '-', ' ' };

        public string RemoveTextFromHearImpaired(string input)
        {
            if (StartsAndEndsWithHearImpairedTags(HtmlUtil.RemoveHtmlTags(input, true).TrimStart(TrimStartNoiseChar)))
            {
                return string.Empty;
            }

            if (Settings.RemoveWhereContains)
            {
                foreach (var removeIfTextContain in Settings.RemoveIfTextContains)
                {
                    if (input.Contains(removeIfTextContain))
                    {
                        return string.Empty;
                    }
                }
            }

            var text = RemoveColon(input);
            string pre = " >-\"'‘`´♪¿¡.…—";
            string post = " -\"'`´♪.!?:…—";
            if (Settings.RemoveTextBetweenCustomTags)
            {
                pre = pre.Replace(Settings.CustomStart, string.Empty);
                post = post.Replace(Settings.CustomEnd, string.Empty);
            }
            var st = new StrippableText(text, pre, post);
            var sb = new StringBuilder();
            var parts = st.StrippedText.Trim().SplitToLines();
            int lineNumber = 0;
            bool removedDialogInFirstLine = false;
            int noOfNamesRemoved = 0;
            int noOfNamesRemovedNotInLineOne = 0;
            foreach (string s in parts)
            {
                var stSub = new StrippableText(s, pre, post);
                string strippedText = stSub.StrippedText;
                if (lineNumber == parts.Count - 1 && st.Post.Contains('?') || stSub.Post.Contains('?'))
                {
                    strippedText += "?";
                }

                if (!StartsAndEndsWithHearImpairedTags(strippedText))
                {
                    if (removedDialogInFirstLine && stSub.Pre.Contains("- "))
                    {
                        stSub.Pre = stSub.Pre.Replace("- ", string.Empty);
                    }

                    string newText = stSub.StrippedText;

                    newText = RemoveHearImpairedTags(newText);

                    if (stSub.StrippedText.Length - newText.Length > 2)
                    {
                        string removedText = GetRemovedString(stSub.StrippedText, newText);
                        if (!IsHIDescription(removedText))
                        {
                            noOfNamesRemoved++;
                            if (lineNumber > 0)
                            {
                                noOfNamesRemovedNotInLineOne++;
                            }
                        }
                    }

                    if (stSub.Pre == "<i>- " && newText.StartsWith("</i>", StringComparison.Ordinal))
                    {
                        sb.AppendLine("- " + newText.Remove(0, 4).Trim() + stSub.Post);
                    }
                    else
                    {
                        sb.AppendLine(stSub.Pre + newText + stSub.Post);
                    }
                }
                else
                {
                    if (!IsHIDescription(stSub.StrippedText))
                    {
                        noOfNamesRemoved++;
                        if (lineNumber > 0)
                        {
                            noOfNamesRemovedNotInLineOne++;
                        }
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
                    {
                        st.Pre = st.Pre.Replace("<i>", string.Empty);
                    }

                    if (s.Contains("<i>") && !s.Contains("</i>") && st.Post.Contains("</i>"))
                    {
                        st.Post = st.Post.Replace("</i>", string.Empty);
                    }

                    if (lineNumber == parts.Count - 1)
                    {
                        if (st.Post.Replace("♪", string.Empty).Replace("♫", string.Empty).Trim().Length == 0)
                        {
                            st.Post = string.Empty;
                        }
                    }
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
            text = RemoveHearingImpairedTagsInsideLine(text);
            if (Settings.RemoveInterjections)
            {
                if (_interjections == null)
                {
                    ReloadInterjection();
                }

                // reusable context
                _interjectionRemoveContext.Text = text;
                _interjectionRemoveContext.OnlySeparatedLines = Settings.RemoveInterjectionsOnlySeparateLine;
                _interjectionRemoveContext.Interjections = _interjections;
                text = _removeInterjection.Invoke(_interjectionRemoveContext);
            }

            st = new StrippableText(text, pre, post);
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
                var splitParts = HtmlUtil.RemoveHtmlTags(text).RemoveChar(' ').Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                if (splitParts.Length == 2)
                {
                    var temp = new StrippableText(text);
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
                            {

                                text = "- " + text;
                                if (!text.Contains(Environment.NewLine + "<i>- "))
                                {
                                    text = text.Replace(Environment.NewLine, Environment.NewLine + "- ");
                                }
                            }
                            if (!text.Contains(Environment.NewLine + "-") && !text.Contains(Environment.NewLine + "<i>-"))
                            {
                                text = text.Replace(Environment.NewLine, Environment.NewLine + "- ");
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(text) || (st.Pre.Contains('♪') || st.Post.Contains('♪')))
            {
                text = st.Pre + text + st.Post;
            }

            if ((input.TrimStart().StartsWith("-", StringComparison.Ordinal) || input.TrimStart().StartsWith("<i>-", StringComparison.Ordinal)) &&
                text != null && !text.Contains(Environment.NewLine) &&
                (input.Contains(Environment.NewLine + "-") ||
                 input.Contains(Environment.NewLine + " - ") ||
                 input.Contains(Environment.NewLine + "<i>-") ||
                 input.Contains(Environment.NewLine + "<i> - ")))
            {
                if (text.StartsWith("<i>-", StringComparison.Ordinal))
                {
                    text = "<i>" + text.Remove(0, 4).Trim();
                }
                else
                {
                    text = text.TrimStart().TrimStart('-').TrimStart();
                }
            }
            if (input.TrimStart().StartsWith('-') && !input.TrimStart().StartsWith("--", StringComparison.Ordinal) &&
                text != null && !text.Contains(Environment.NewLine) &&
                (input.Contains(Environment.NewLine + "-") && !input.Contains(Environment.NewLine + "--") ||
                 input.Contains(Environment.NewLine + " - ") ||
                 input.Contains(Environment.NewLine + "<i>- ") ||
                 input.Contains(Environment.NewLine + "<i> - ")))
            {
                text = text.TrimStart().TrimStart('-').TrimStart();
            }

            if (input.TrimStart().StartsWith("<i>- ", StringComparison.Ordinal) &&
                text != null && text.StartsWith("<i>- ", StringComparison.Ordinal) && !text.Contains(Environment.NewLine) &&
                (input.Contains(Environment.NewLine + "- ") ||
                 input.Contains(Environment.NewLine + " - ") ||
                 input.Contains(Environment.NewLine + "<i>- ") ||
                 input.Contains(Environment.NewLine + "<i> - ")))
            {
                text = text.Remove(3, 2);
            }

            if (text != null && !text.Contains(Environment.NewLine) &&
                (input.Contains(':') && !text.Contains(':') ||
                 input.Contains('[') && !text.Contains('[') ||
                 input.Contains('(') && !text.Contains('(') ||
                 input.Contains('{') && !text.Contains('{')) &&
                (input.Contains(Environment.NewLine + "- ") ||
                 input.Contains(Environment.NewLine + " - ") ||
                 input.Contains(Environment.NewLine + "<i>- ") ||
                 input.Contains(Environment.NewLine + "<i> - ")))
            {
                text = text.TrimStart().TrimStart('-').TrimStart();
            }

            string removeText = "<i>- </i>" + Environment.NewLine + "-";
            if (text.StartsWith(removeText, StringComparison.Ordinal))
            {
                text = text.Remove(0, removeText.Length).TrimStart(' ');
            }

            removeText = "<i>-</i>" + Environment.NewLine + "-";
            if (text.StartsWith(removeText, StringComparison.Ordinal))
            {
                text = text.Remove(0, removeText.Length).TrimStart(' ');
            }

            removeText = "<i>-</i>" + Environment.NewLine + "<i>-";
            if (text.StartsWith(removeText, StringComparison.Ordinal))
            {
                text = "<i>" + text.Remove(0, removeText.Length).TrimStart(' ');
            }

            removeText = "<i>- </i>" + Environment.NewLine + "<i>-";
            if (text.StartsWith(removeText, StringComparison.Ordinal))
            {
                text = "<i>" + text.Remove(0, removeText.Length).TrimStart(' ');
            }

            if (input != text)
            {
                // insert spaces before "-"
                text = text.Replace(Environment.NewLine + "- <i>", Environment.NewLine + "<i>- ");
                text = text.Replace(Environment.NewLine + "-<i>", Environment.NewLine + "<i>- ");
                if (text.Length > 2 && text[0] == '-' && text[1] != ' ' && text[1] != '-')
                {
                    text = text.Insert(1, " ");
                }

                if (text.Length > 5 && text.StartsWith("<i>-", StringComparison.Ordinal) && text[4] != ' ' && text[4] != '-')
                {
                    text = text.Insert(4, " ");
                }

                int index = text.IndexOf(Environment.NewLine + "-", StringComparison.Ordinal);
                if (index >= 0 && text.Length - index > 4)
                {
                    index += Environment.NewLine.Length + 1;
                    if (text[index] != ' ' && text[index] != '-')
                    {
                        text = text.Insert(index, " ");
                    }
                }
                index = text.IndexOf(Environment.NewLine + "<i>-", StringComparison.Ordinal);
                if (index >= 0 && text.Length - index > 5)
                {
                    index += Environment.NewLine.Length + 4;
                    if (text[index] != ' ' && text[index] != '-')
                    {
                        text = text.Insert(index, " ");
                    }
                }
            }
            return text.Trim();
        }

        private static string RemoveEmptyFontTag(string text)
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
                            {
                                text = text.Insert(indexOfStartFont, " ");
                            }

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
            {
                Warnings.Add(WarningIndex);
            }
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
            text = text.Trim(' ', '(', ')', '[', ']', '?', '{', '}').ToLowerInvariant();
            if (text.Trim().Replace("mr. ", string.Empty).Replace("mrs. ", string.Empty).Replace("dr. ", string.Empty).Contains(' '))
            {
                AddWarning();
            }

            return (HiDescriptionWords.Contains(text) ||
                    text.StartsWith("engine ", StringComparison.Ordinal) ||
                    text.EndsWith("on tv", StringComparison.Ordinal) ||
                    text.EndsWith("shatters", StringComparison.Ordinal) ||
                    text.EndsWith("ing", StringComparison.Ordinal));
        }

        private static string GetRemovedString(string oldText, string newText)
        {
            oldText = oldText.ToLowerInvariant();
            newText = newText.ToLowerInvariant();

            int start = oldText.IndexOf(newText, StringComparison.Ordinal);
            string result;
            if (start > 0)
            {
                result = oldText.Substring(0, oldText.Length - newText.Length);
            }
            else
            {
                result = oldText.Substring(newText.Length);
            }

            return result.Trim(' ', '(', ')', '[', ']', '?', '{', '}');
        }

        private string RemoveStartEndTags(string text)
        {
            string newText = text;
            string s = text;
            int index;
            if (Settings.RemoveTextBetweenSquares && s.StartsWith('[') && (index = s.IndexOf(']', 1)) > 0)
            {
                if (++index < s.Length && s[index] == ':')
                {
                    index++;
                }

                newText = s.Remove(0, index);
            }
            else if (Settings.RemoveTextBetweenBrackets && s.StartsWith('{') && (index = s.IndexOf('}', 1)) > 0)
            {
                if (++index < s.Length && s[index] == ':')
                {
                    index++;
                }

                newText = s.Remove(0, index);
            }
            else if (Settings.RemoveTextBetweenParentheses && s.StartsWith('(') && (index = s.IndexOf(')', 1)) > 0)
            {
                if (++index < s.Length && s[index] == ':')
                {
                    index++;
                }

                newText = s.Remove(0, index);
            }
            else if (Settings.RemoveTextBetweenQuestionMarks && s.StartsWith('?') && (index = s.IndexOf('?', 1)) > 0)
            {
                if (++index < s.Length && s[index] == ':')
                {
                    index++;
                }

                newText = s.Remove(0, index);
            }
            else if (Settings.RemoveTextBetweenCustomTags &&
                     s.Length > 0 && Settings.CustomEnd.Length > 0 && Settings.CustomStart.Length > 0 &&
                     s.StartsWith(Settings.CustomStart, StringComparison.Ordinal) && (index = s.LastIndexOf(Settings.CustomEnd, StringComparison.Ordinal)) > 0)
            {
                newText = s.Remove(0, index + Settings.CustomEnd.Length);
            }
            if (newText != text)
            {
                newText = newText.TrimStart(' ');
            }

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
            if (text.StartsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                preNewLine = Environment.NewLine;
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
            {
                return string.Empty;
            }

            return preAssTag + preNewLine + text.TrimStart();
        }

        private bool HasHearImpairedText(string text)
        {
            return RemoveHearImpairedTags(text) != text;
        }

        public bool HasHearImpairedTagsAtStartOrEnd(string text)
        {
            return Settings.OnlyIfInSeparateLine ? StartsAndEndsWithHearImpairedTags(text) : HasHearImpairedText(text);
        }

        private bool StartsAndEndsWithHearImpairedTags(string text)
        {
            if (Settings.RemoveTextBetweenSquares && StartsAndEndsWithHearImpairedTags(text, '[', ']'))
            {
                return true;
            }

            if (Settings.RemoveTextBetweenBrackets && StartsAndEndsWithHearImpairedTags(text, '{', '}'))
            {
                return true;
            }

            if (Settings.RemoveTextBetweenParentheses && StartsAndEndsWithHearImpairedTags(text, '(', ')'))
            {
                return true;
            }

            if (Settings.RemoveTextBetweenQuestionMarks && StartsAndEndsWithHearImpairedTags(text, '?', '?'))
            {
                return true;
            }

            return Settings.RemoveTextBetweenCustomTags &&
                   Settings.CustomStart.Length > 0 && Settings.CustomEnd.Length > 0 &&
                   text.StartsWith(Settings.CustomStart, StringComparison.Ordinal) && text.EndsWith(Settings.CustomEnd, StringComparison.Ordinal);
        }

        private static bool StartsAndEndsWithHearImpairedTags(string text, char startTag, char endTag)
        {
            if (text.Length > 1 && text.StartsWith(startTag) && (text.EndsWith(endTag) || text.EndsWith(endTag + ":")))
            {
                var lastIndex = text.LastIndexOf(endTag);
                var s = text.Substring(1, lastIndex - 1);
                var restNumberOfStartTags = Utilities.CountTagInText(s, startTag);
                var restNumberOfEndTags = Utilities.CountTagInText(s, endTag);
                if (restNumberOfStartTags == 1 && restNumberOfEndTags == 1)
                {
                    return s.IndexOf(startTag) < s.IndexOf(endTag);
                }
                if (restNumberOfStartTags == 0 && restNumberOfEndTags == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static string RemoveTextBetweenTags(string startTag, string endTag, string text)
        {
            text = text.Trim();
            if (startTag == "?" || endTag == "?")
            {
                if (text.StartsWith(startTag, StringComparison.Ordinal) && text.EndsWith(endTag, StringComparison.Ordinal))
                {
                    return string.Empty;
                }

                return text;
            }

            int start = text.IndexOf(startTag, StringComparison.Ordinal);
            if (start < 0 || text.Length - start - startTag.Length < endTag.Length)
            {
                return text;
            }

            do
            {
                int end = text.IndexOf(endTag, start + startTag.Length, StringComparison.Ordinal);
                if (end < 0)
                {
                    break;
                }

                text = text.Remove(start, end - start + 1);
                if (start > 3 && text.Length - start > 1 &&
                    text[start] == ':' && text[start - 1] == ' ' && ".?!".Contains(text[start - 2]))
                {
                    text = text.Remove(start - 1, 2);
                }
                else if (start == 0 && text.Length > 1 && text[0] == ':')
                {
                    text = text.Remove(0, 1).TrimStart();
                }
                else if (text.Substring(0, start).EndsWith(", ") && text.Remove(0, start).TrimStart().StartsWith(','))
                {
                    text = text.Substring(0, start).TrimEnd() + " " + text.Remove(0, start).TrimStart(' ', ',');
                    text = text.Trim();
                }

                start = text.IndexOf(startTag, StringComparison.Ordinal);
            }
            while (start >= 0 && text.Length - start - startTag.Length >= endTag.Length);

            return text.FixExtraSpaces().TrimEnd();
        }

        public string RemoveLineIfAllUppercase(string text)
        {
            if (!Settings.RemoveIfAllUppercase)
            {
                return text;
            }

            var sb = new StringBuilder();
            char[] endTrimChars = { '.', '!', '?', ':' };
            char[] trimChars = { ' ', '-', '—' };
            foreach (var line in text.SplitToLines())
            {
                var lineNoHtml = HtmlUtil.RemoveHtmlTags(line, true);
                if (lineNoHtml == lineNoHtml.ToUpperInvariant() && lineNoHtml != lineNoHtml.ToLowerInvariant())
                {
                    var temp = lineNoHtml.TrimEnd(endTrimChars).Trim().Trim(trimChars);
                    if (temp.Length == 1 || temp == "YES" || temp == "NO" || temp == "WHY" || temp == "HI" || temp == "OK")
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

        public static IList<string> GetInterjectionList()
        {
            var interjectionList = new HashSet<string>();
            foreach (var s in Configuration.Settings.Tools.Interjections.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (s.Length <= 0)
                {
                    continue;
                }

                interjectionList.Add(s);
                interjectionList.Add(s.ToUpperInvariant());
                interjectionList.Add(s.ToLowerInvariant());
                interjectionList.Add(s.CapitalizeFirstLetter());
            }

            var sortedList = new List<string>(interjectionList);
            sortedList.Sort((a, b) => b.Length.CompareTo(a.Length));
            return sortedList;
        }

        public void ReloadInterjection()
        {
            _interjections = GetInterjectionList();
        }
    }
}
