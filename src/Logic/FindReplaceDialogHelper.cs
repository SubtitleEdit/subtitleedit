using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    public class FindReplaceDialogHelper
    {
        private const string SeparatorChars = " #><-\"„”“[]'‘`´¶(){}♪,.!?:;¿¡.…—،؟\r\n\u2028";
        private readonly string _findText;
        private readonly string _replaceText;
        private Regex _regEx;
        private int _findTextLength;

        public bool Success { get; set; }
        public ReplaceType FindReplaceType { get; set; }
        public int SelectedIndex { get; set; }
        public int SelectedPosition { get; set; }
        public int ReplaceFromPosition { get; set; }
        public int StartLineIndex { get; set; }
        public bool MatchInOriginal { get; set; }
        public bool InProgress { get; set; }

        public int FindTextLength => _findTextLength;

        public string FindText => _findText;

        public string ReplaceText => _replaceText;

        public FindReplaceDialogHelper(ReplaceType findType, string findText, Regex regEx, string replaceText, int startLineIndex)
        {
            _replaceText = string.Empty;
            FindReplaceType = findType;
            _findText = findText;

            _replaceText = replaceText;
            if (_replaceText != null)
            {
                _replaceText = RegexUtils.FixNewLine(_replaceText);
            }

            _regEx = regEx;
            _findTextLength = findText.Length;
            StartLineIndex = startLineIndex;
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
                var idx = text.IndexOf(_findText, startIndex, comparison);
                while (idx >= 0)
                {
                    if (FindReplaceType.WholeWord)
                    {
                        var startOk = idx == 0 || SeparatorChars.Contains(text[idx - 1]);
                        var endOk = idx + _findText.Length == text.Length || SeparatorChars.Contains(text[idx + _findText.Length]);
                        if (startOk && endOk)
                        {
                            return idx;
                        }
                    }
                    else
                    {
                        return idx;
                    }
                    idx = text.IndexOf(_findText, idx + _findText.Length, comparison);
                }
                return -1;
            }

            var match = _regEx.Match(text, startIndex);
            if (match.Success)
            {
                string groupName = RegexUtils.GetRegExGroup(_findText);
                if (groupName != null && match.Groups[groupName] != null && match.Groups[groupName].Success)
                {
                    _findTextLength = match.Groups[groupName].Length;
                    return match.Groups[groupName].Index;
                }
                _findTextLength = match.Length;
                return match.Index;
            }
            return -1;
        }

        public bool FindNext(Subtitle subtitle, Subtitle originalSubtitle, int startIndex, int position, bool allowEditOfOriginalSubtitle)
        {
            Success = false;
            int index = 0;
            if (position < 0)
            {
                position = 0;
            }

            bool first = true;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (index >= startIndex)
                {
                    if (!first)
                    {
                        position = 0;
                    }

                    int pos;
                    if (!MatchInOriginal)
                    {
                        pos = FindPositionInText(p.Text, position);
                        if (pos >= 0)
                        {
                            MatchInOriginal = false;
                            SelectedIndex = index;
                            SelectedPosition = pos;
                            ReplaceFromPosition = pos;
                            Success = true;
                            return true;
                        }
                        position = 0;
                    }
                    if (index < subtitle.Paragraphs.Count - 1)
                    {
                        MatchInOriginal = false;
                    }

                    if (originalSubtitle != null && allowEditOfOriginalSubtitle)
                    {
                        Paragraph o = Utilities.GetOriginalParagraph(index, p, originalSubtitle.Paragraphs);
                        if (o != null)
                        {
                            pos = FindPositionInText(o.Text, position);
                            if (pos >= 0)
                            {
                                MatchInOriginal = true;
                                SelectedIndex = index;
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
            Success = false;
            int index = startIndex;
            bool first = true;
            for (var i = startIndex; i >= 0; i--)
            {
                Paragraph p = subtitle.Paragraphs[i];

                if (originalSubtitle != null && allowEditOfOriginalSubtitle)
                {
                    if (!first || MatchInOriginal)
                    {
                        Paragraph o = Utilities.GetOriginalParagraph(index, p, originalSubtitle.Paragraphs);
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
                                        SelectedIndex = index;
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

                for (var j = 0; j <= position; j++)
                {
                    if (position - j >= 0 && position < p.Text.Length)
                    {
                        var t = p.Text.Substring(position - j, j + 1);
                        int pos = FindPositionInText(t, 0);
                        if (pos >= 0)
                        {
                            pos += position - j;
                            MatchInOriginal = false;
                            SelectedIndex = index;
                            SelectedPosition = pos;
                            ReplaceFromPosition = pos;
                            Success = true;
                            return true;
                        }
                    }
                }
                position = 0;
                first = false;
                index--;
            }
            return false;
        }

        public static ContextMenu GetRegExContextMenu(TextBox textBox)
        {
            var cm = new ContextMenu();
            var l = Configuration.Settings.Language.RegularExpressionContextMenu;
            cm.MenuItems.Add(l.WordBoundary, delegate { textBox.SelectedText = "\\b"; });
            cm.MenuItems.Add(l.NonWordBoundary, delegate { textBox.SelectedText = "\\B"; });
            cm.MenuItems.Add(l.NewLine, delegate { textBox.SelectedText = "\\r\\n"; });
            cm.MenuItems.Add(l.AnyDigit, delegate { textBox.SelectedText = "\\d"; });
            cm.MenuItems.Add(l.NonDigit, delegate { textBox.SelectedText = "\\D"; });
            cm.MenuItems.Add(l.AnyCharacter, delegate { textBox.SelectedText = "."; });
            cm.MenuItems.Add(l.AnyWhitespace, delegate { textBox.SelectedText = "\\s"; });
            cm.MenuItems.Add(l.NonSpaceCharacter, delegate { textBox.SelectedText = "\\S"; });
            cm.MenuItems.Add(l.ZeroOrMore, delegate { textBox.SelectedText = "*"; });
            cm.MenuItems.Add(l.OneOrMore, delegate { textBox.SelectedText = "+"; });
            cm.MenuItems.Add(l.InCharacterGroup, delegate { textBox.SelectedText = "[test]"; });
            cm.MenuItems.Add(l.NotInCharacterGroup, delegate { textBox.SelectedText = "[^test]"; });
            return cm;
        }

        public static ContextMenu GetRegExContextMenu(ComboBox comboBox)
        {
            var cm = new ContextMenu();
            var l = Configuration.Settings.Language.RegularExpressionContextMenu;
            cm.MenuItems.Add(l.WordBoundary, delegate { comboBox.SelectedText = "\\b"; });
            cm.MenuItems.Add(l.NonWordBoundary, delegate { comboBox.SelectedText = "\\B"; });
            cm.MenuItems.Add(l.NewLine, delegate { comboBox.SelectedText = "\\r\\n"; });
            cm.MenuItems.Add(l.AnyDigit, delegate { comboBox.SelectedText = "\\d"; });
            cm.MenuItems.Add(l.NonDigit, delegate { comboBox.SelectedText = "\\D"; });
            cm.MenuItems.Add(l.AnyCharacter, delegate { comboBox.SelectedText = "."; });
            cm.MenuItems.Add(l.AnyWhitespace, delegate { comboBox.SelectedText = "\\s"; });
            cm.MenuItems.Add(l.NonSpaceCharacter, delegate { comboBox.SelectedText = "\\S"; });
            cm.MenuItems.Add(l.ZeroOrMore, delegate { comboBox.SelectedText = "*"; });
            cm.MenuItems.Add(l.OneOrMore, delegate { comboBox.SelectedText = "+"; });
            cm.MenuItems.Add(l.InCharacterGroup, delegate { comboBox.SelectedText = "[test]"; });
            cm.MenuItems.Add(l.NotInCharacterGroup, delegate { comboBox.SelectedText = "[^test]"; });
            return cm;
        }

        public static ContextMenu GetReplaceTextContextMenu(TextBox textBox)
        {
            var cm = new ContextMenu();
            cm.MenuItems.Add(Configuration.Settings.Language.RegularExpressionContextMenu.NewLineShort, delegate { textBox.SelectedText = "\\n"; });
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
                    Match match = _regEx.Match(text, startIndex);
                    if (match.Success)
                    {
                        string groupName = RegexUtils.GetRegExGroup(_findText);
                        if (groupName != null && match.Groups[groupName] != null && match.Groups[groupName].Success)
                        {
                            _findTextLength = match.Groups[groupName].Length;
                            SelectedIndex = match.Groups[groupName].Index;
                        }
                        else
                        {
                            _findTextLength = match.Length;
                            SelectedIndex = match.Index;
                        }
                        Success = true;
                    }
                    return match.Success;
                }
                string searchText = text.Substring(startIndex);
                int pos = FindPositionInText(searchText, 0);
                if (pos >= 0)
                {
                    SelectedIndex = pos + startIndex;
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
                        string groupName = RegexUtils.GetRegExGroup(_findText);
                        var last = matches[matches.Count - 1];
                        if (groupName != null && last.Groups[groupName] != null && last.Groups[groupName].Success)
                        {
                            _findTextLength = last.Groups[groupName].Length;
                            SelectedIndex = last.Groups[groupName].Index;
                        }
                        else
                        {
                            _findTextLength = last.Length;
                            SelectedIndex = last.Index;
                        }
                        Success = true;
                    }
                    return Success;
                }
                string searchText = text.Substring(0, startIndex);
                int pos = -1;
                var comparison = GetComparison();
                var idx = searchText.LastIndexOf(_findText, startIndex, comparison);
                while (idx >= 0)
                {
                    if (FindReplaceType.WholeWord)
                    {
                        var startOk = idx == 0 || SeparatorChars.Contains(searchText[idx - 1]);
                        var endOk = idx + _findText.Length == searchText.Length || SeparatorChars.Contains(searchText[idx + _findText.Length]);
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
                    idx = searchText.LastIndexOf(_findText, comparison);
                }
                if (pos >= 0)
                {
                    SelectedIndex = pos;
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
                var findText = RegexUtils.FixNewLine(_findText);
                if (!RegexUtils.IsValidRegex(findText))
                {
                    MessageBox.Show(Configuration.Settings.Language.General.RegularExpressionIsNotValid);
                    return count;
                }
                _regEx = new Regex(findText);
            }
            var comparison = GetComparison();
            foreach (var p in subtitle.Paragraphs)
            {
                if (FindReplaceType.FindType == FindType.RegEx)
                {
                    count += _regEx.Matches(p.Text).Count;
                }
                else
                {
                    if (p.Text.Length < _findText.Length)
                    {
                        continue;
                    }
                    count += GetWordCount(p.Text, _findText, wholeWord, comparison);
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

    }
}
