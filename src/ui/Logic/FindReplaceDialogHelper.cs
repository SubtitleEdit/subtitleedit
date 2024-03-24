using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Controls.Interfaces;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Logic
{
    public class FindReplaceDialogHelper
    {
        private const string SeparatorChars = " #><-\"„”“[]'‘`´¶(){}♪,.!?:;¿¡.…—،؟\r\n\u2028";
        private Regex _regEx;

        public bool Success { get; set; }
        public ReplaceType FindReplaceType { get; set; }
        public int SelectedLineIndex { get; set; }
        public int SelectedPosition { get; set; }
        public int ReplaceFromPosition { get; set; }
        public int StartLineIndex { get; set; }
        public string StartFindText { get; set; }
        public bool MatchInOriginal { get; set; }
        public bool InProgress { get; set; }

        public int FindTextLength { get; set; }

        public string FindText { get; set; }

        public string ReplaceText { get; }

        public FindReplaceDialogHelper(ReplaceType findType, string findText, Regex regEx, string replaceText, int startLineIndex)
        {
            ReplaceText = string.Empty;
            FindReplaceType = findType;
            FindText = findText;

            ReplaceText = replaceText;
            if (ReplaceText != null)
            {
                ReplaceText = RegexUtils.FixNewLine(ReplaceText);
            }

            _regEx = regEx;
            FindTextLength = findText.Length;
            StartLineIndex = startLineIndex;
            if (StartLineIndex == 0)
            {
                StartFindText = findText;
            }

            MatchInOriginal = false;
        }

        public bool Find(Subtitle subtitle, Subtitle originalSubtitle, int startIndex, int startPosition = 0)
        {
            return FindNext(subtitle, originalSubtitle, startIndex, startPosition, Configuration.Settings.General.AllowEditOfOriginalSubtitle);
        }

        public bool Find(TextBox textBox, int startIndex)
        {
            return FindNext(textBox.Text, startIndex);
        }

        private int FindPositionInText(string text, int startIndex)
        {
            if (startIndex >= text.Length && !(FindReplaceType.FindType == FindType.RegEx && startIndex == 0))
            {
                return -1;
            }

            if (FindReplaceType.FindType == FindType.CaseSensitive || FindReplaceType.FindType == FindType.Normal)
            {
                var comparison = GetComparison();
                var idx = text.IndexOf(FindText, startIndex, comparison);
                while (idx >= 0)
                {
                    if (FindReplaceType.WholeWord)
                    {
                        var startOk = idx == 0 || SeparatorChars.Contains(text[idx - 1]);
                        var endOk = idx + FindText.Length == text.Length || SeparatorChars.Contains(text[idx + FindText.Length]);
                        if (startOk && endOk)
                        {
                            return idx;
                        }
                    }
                    else
                    {
                        return idx;
                    }
                    idx = text.IndexOf(FindText, idx + FindText.Length, comparison);
                }
                return -1;
            }


            Match match;
            try
            {
                if (_regEx == null)
                {
                    _regEx = new Regex(FindText, RegexOptions.None, TimeSpan.FromSeconds(5));
                }

                match = _regEx.Match(text, startIndex);
            }
            catch (RegexMatchTimeoutException exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine +
                                Environment.NewLine +
                                "Input: " + exception.Input);
                return -1;
            }

            if (match.Success)
            {
                var groupName = RegexUtils.GetRegExGroup(FindText);
                if (groupName != null && match.Groups[groupName] != null && match.Groups[groupName].Success)
                {
                    FindTextLength = match.Groups[groupName].Length;
                    return match.Groups[groupName].Index;
                }
                FindTextLength = match.Length;
                return match.Index;
            }

            return -1;
        }

        public bool FindNext(Subtitle subtitle, Subtitle originalSubtitle, int startIndex, int position, bool allowEditOfOriginalSubtitle)
        {
            Success = false;
            var index = 0;
            if (position < 0)
            {
                position = 0;
            }

            var first = true;
            foreach (var p in subtitle.Paragraphs)
            {
                if (index >= startIndex)
                {
                    if (!first)
                    {
                        position = 0;
                        MatchInOriginal = false;
                    }

                    int pos;
                    if (!MatchInOriginal && FindReplaceType.SearchTranslation)
                    {
                        pos = FindPositionInText(p.Text, position);
                        if (pos >= 0)
                        {
                            MatchInOriginal = false;
                            SelectedLineIndex = index;
                            SelectedPosition = pos;
                            ReplaceFromPosition = pos;
                            Success = true;
                            return true;
                        }
                        position = 0;

                        if (index < subtitle.Paragraphs.Count - 1)
                        {
                            MatchInOriginal = false;
                        }
                    }

                    if (originalSubtitle != null && allowEditOfOriginalSubtitle && FindReplaceType.SearchOriginal)
                    {
                        var o = Utilities.GetOriginalParagraph(index, p, originalSubtitle.Paragraphs);
                        if (o != null)
                        {
                            pos = FindPositionInText(o.Text, position);
                            if (pos >= 0)
                            {
                                MatchInOriginal = true;
                                SelectedLineIndex = index;
                                SelectedPosition = pos;
                                ReplaceFromPosition = pos;
                                Success = true;
                                return true;
                            }
                        }
                    }

                    first = false;
                }

                index++;
            }

            return false;
        }

        public bool FindPrevious(Subtitle subtitle, Subtitle originalSubtitle, int startIndex, int position, bool allowEditOfOriginalSubtitle)
        {
            //TODO: us whole word
            Success = false;
            var index = startIndex;
            var first = true;
            for (var i = startIndex; i >= 0; i--)
            {
                var p = subtitle.Paragraphs[i];

                if (originalSubtitle != null && allowEditOfOriginalSubtitle)
                {
                    if ((!first || MatchInOriginal) && FindReplaceType.SearchTranslation)
                    {
                        var o = Utilities.GetOriginalParagraph(index, p, originalSubtitle.Paragraphs);
                        if (o != null)
                        {
                            if (!first)
                            {
                                position = o.Text.Length - 1;
                            }

                            for (var j = 0; j <= position; j++)
                            {
                                if (position - j >= 0 && position - j + j < o.Text.Length)
                                {
                                    var t = o.Text.Substring(position - j, j);
                                    int pos = FindPositionInText(t, 0);
                                    if (pos >= 0)
                                    {
                                        pos += position - j;
                                        MatchInOriginal = true;
                                        SelectedLineIndex = index;
                                        SelectedPosition = pos;
                                        ReplaceFromPosition = pos;
                                        Success = true;
                                        return true;
                                    }
                                }
                            }
                        }
                        position = p.Text.Length - 1;
                    }
                }

                MatchInOriginal = false;
                if (!first)
                {
                    position = p.Text.Length - 1;
                }

                if (originalSubtitle != null && allowEditOfOriginalSubtitle && FindReplaceType.SearchOriginal)
                {

                    for (var j = 0; j <= position; j++)
                    {
                        if (position - j >= 0 && position < p.Text.Length)
                        {
                            var t = p.Text.Substring(position - j, j + 1);
                            var pos = FindPositionInText(t, 0);
                            var startWholeWord = position - j < 1;
                            if (!startWholeWord && position - j - 1 > 0)
                            {
                                startWholeWord = SeparatorChars.Contains(p.Text[position - j - 1]);
                            }

                            var startWholeWorkOkay = !FindReplaceType.WholeWord || startWholeWord;

                            if (pos >= 0 && startWholeWorkOkay)
                            {
                                pos += position - j;
                                MatchInOriginal = false;
                                SelectedLineIndex = index;
                                SelectedPosition = pos;
                                ReplaceFromPosition = pos;
                                Success = true;
                                return true;
                            }
                        }
                    }
                }

                position = 0;
                first = false;
                index--;
            }

            return false;
        }

        public static ContextMenuStrip GetRegExContextMenu(ISelectedText textBox)
        {
            var cm = new ContextMenuStrip();
            var l = LanguageSettings.Current.RegularExpressionContextMenu;
            cm.Items.Add(l.WordBoundary, null, delegate { textBox.SelectedText = "\\b"; });
            cm.Items.Add(l.NonWordBoundary, null, delegate { textBox.SelectedText = "\\B"; });
            cm.Items.Add(l.NewLine, null, delegate { textBox.SelectedText = "\\r\\n"; });
            cm.Items.Add(l.AnyDigit, null, delegate { textBox.SelectedText = "\\d"; });
            cm.Items.Add(l.NonDigit, null, delegate { textBox.SelectedText = "\\D"; });
            cm.Items.Add(l.AnyCharacter, null, delegate { textBox.SelectedText = "."; });
            cm.Items.Add(l.AnyWhitespace, null, delegate { textBox.SelectedText = "\\s"; });
            cm.Items.Add(l.NonSpaceCharacter, null, delegate { textBox.SelectedText = "\\S"; });
            cm.Items.Add(l.ZeroOrMore, null, delegate { textBox.SelectedText = "*"; });
            cm.Items.Add(l.OneOrMore, null, delegate { textBox.SelectedText = "+"; });
            cm.Items.Add(l.InCharacterGroup, null, delegate { textBox.SelectedText = "[test]"; });
            cm.Items.Add(l.NotInCharacterGroup, null, delegate { textBox.SelectedText = "[^test]"; });
            return cm;
        }

        public static ContextMenuStrip GetReplaceTextContextMenu(TextBox textBox)
        {
            var cm = new ContextMenuStrip();
            cm.Items.Add(LanguageSettings.Current.RegularExpressionContextMenu.NewLineShort, null, delegate { textBox.SelectedText = "\\n"; });
            return cm;
        }

        public static ContextMenuStrip GetReplaceTextContextMenu(SETextBox textBox)
        {
            var cm = new ContextMenuStrip();
            cm.Items.Add(LanguageSettings.Current.RegularExpressionContextMenu.NewLineShort, null, delegate { textBox.SelectedText = "\\n"; });
            return cm;
        }

        public bool FindNext(string text, int startIndex)
        {
            Success = false;
            startIndex++;
            if (startIndex < text.Length)
            {
                if (FindReplaceType.FindType == FindType.RegEx)
                {
                    var match = _regEx.Match(text, startIndex);
                    if (match.Success)
                    {
                        var groupName = RegexUtils.GetRegExGroup(FindText);
                        if (groupName != null && match.Groups[groupName] != null && match.Groups[groupName].Success)
                        {
                            FindTextLength = match.Groups[groupName].Length;
                            SelectedLineIndex = match.Groups[groupName].Index;
                        }
                        else
                        {
                            FindTextLength = match.Length;
                            SelectedLineIndex = match.Index;
                        }
                        Success = true;
                    }
                    return match.Success;
                }

                var searchText = text.Substring(startIndex);
                var pos = FindPositionInText(searchText, 0);
                if (pos >= 0)
                {
                    SelectedLineIndex = pos + startIndex;
                    return true;
                }
            }

            return false;
        }

        public bool FindPrevious(string text, int startIndex)
        {
            Success = false;
            startIndex--;
            if (startIndex < text.Length)
            {
                if (FindReplaceType.FindType == FindType.RegEx)
                {
                    var matches = _regEx.Matches(text.Substring(0, startIndex));
                    if (matches.Count > 0)
                    {
                        string groupName = RegexUtils.GetRegExGroup(FindText);
                        var last = matches[matches.Count - 1];
                        if (groupName != null && last.Groups[groupName] != null && last.Groups[groupName].Success)
                        {
                            FindTextLength = last.Groups[groupName].Length;
                            SelectedLineIndex = last.Groups[groupName].Index;
                        }
                        else
                        {
                            FindTextLength = last.Length;
                            SelectedLineIndex = last.Index;
                        }
                        Success = true;
                    }

                    return Success;
                }

                if (startIndex <= 0)
                {
                    return false;
                }

                var searchText = text.Substring(0, startIndex);
                var pos = -1;
                var comparison = GetComparison();
                var idx = searchText.LastIndexOf(FindText, startIndex, comparison);
                while (idx >= 0)
                {
                    if (FindReplaceType.WholeWord)
                    {
                        var startOk = idx == 0 || SeparatorChars.Contains(searchText[idx - 1]);
                        var endOk = idx + FindText.Length == searchText.Length || SeparatorChars.Contains(searchText[idx + FindText.Length]);
                        if (startOk && endOk)
                        {
                            pos = idx;
                            break;
                        }
                    }
                    else
                    {
                        pos = idx;
                        break;
                    }
                    searchText = text.Substring(0, idx);
                    idx = searchText.LastIndexOf(FindText, comparison);
                }

                if (pos >= 0)
                {
                    SelectedLineIndex = pos;
                    return true;
                }
            }

            return false;
        }


        public int FindCount(Subtitle subtitle, bool wholeWord)
        {
            var count = 0;
            //  validate pattern if find type is regex
            if (FindReplaceType.FindType == FindType.RegEx)
            {
                var findText = RegexUtils.FixNewLine(FindText);
                if (!RegexUtils.IsValidRegex(findText))
                {
                    MessageBox.Show(LanguageSettings.Current.General.RegularExpressionIsNotValid);
                    return count;
                }
                _regEx = new Regex(findText, RegexOptions.None, TimeSpan.FromSeconds(5));
            }
            var comparison = GetComparison();
            foreach (var p in subtitle.Paragraphs)
            {
                if (FindReplaceType.FindType == FindType.RegEx)
                {
                    try
                    {
                        count += _regEx.Matches(p.Text).Count;
                    }
                    catch (RegexMatchTimeoutException exception)
                    {
                        MessageBox.Show(exception.Message + Environment.NewLine +
                                        Environment.NewLine +
                                        "Input: " + exception.Input);
                        return count;
                    }
                }
                else
                {
                    if (p.Text.Length < FindText.Length)
                    {
                        continue;
                    }
                    count += GetWordCount(p.Text, FindText, wholeWord, comparison);
                }
            }
            return count;
        }

        private int GetWordCount(string text, string pattern, bool matchWholeWord, StringComparison comparison)
        {
            var idx = text.IndexOf(pattern, comparison);
            var count = 0;
            while (idx >= 0)
            {
                if (matchWholeWord)
                {
                    var startOk = (idx == 0) || (SeparatorChars.Contains(text[idx - 1]));
                    var endOk = (idx + pattern.Length == text.Length) || (SeparatorChars.Contains(text[idx + pattern.Length]));
                    if (startOk && endOk)
                    {
                        count++;
                    }
                }
                else
                {
                    count++;
                }
                idx = text.IndexOf(pattern, idx + pattern.Length, comparison);
            }
            return count;
        }

        private StringComparison GetComparison() => FindReplaceType.FindType == FindType.Normal ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        public void SetRegex(Regex regex)
        {
            _regEx = regex;
        }
    }
}
