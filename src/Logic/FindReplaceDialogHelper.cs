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
        private readonly string _findText = string.Empty;
        private readonly string _replaceText = string.Empty;
        private Regex _regEx;
        private int _findTextLength;

        public bool Success { get; set; }
        public ReplaceType FindReplaceType { get; set; }
        public int SelectedIndex { get; set; }
        public int SelectedPosition { get; set; }
        public int StartLineIndex { get; set; }
        public bool MatchInOriginal { get; set; }

        public int FindTextLength
        {
            get
            {
                return _findTextLength;
            }
        }

        public string FindText
        {
            get
            {
                return _findText;
            }
        }

        public string ReplaceText
        {
            get
            {
                return _replaceText;
            }
        }

        public FindReplaceDialogHelper(ReplaceType findType, string findText, Regex regEx, string replaceText, int startLineIndex)
        {
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
        }

        public bool Find(Subtitle subtitle, Subtitle originalSubtitle, int startIndex, int startPosition = 0)
        {
            return FindNext(subtitle, originalSubtitle, startIndex, startPosition, Configuration.Settings.General.AllowEditOfOriginalSubtitle);
        }

        public bool Find(TextBox textBox, int startIndex)
        {
            return FindNext(textBox.Text, startIndex);
        }

        private int GetTextPosition(string text, int startIndex)
        {
            if (string.IsNullOrEmpty(text))
            {
                return -1;
            }

            // handle boundary case
            if (FindReplaceType.FindType != FindType.RegEx)
            {
                if (startIndex + _findText.Length >= text.Length)
                {
                    return -1;
                }
            }

            if (FindReplaceType.FindType == FindType.CaseSensitive || FindReplaceType.FindType == FindType.Normal)
            {
                var comparison = GetComparison();
                int findLen = _findText.Length;
                var idx = text.IndexOf(_findText, startIndex, comparison);
                while (idx >= 0)
                {
                    if (FindReplaceType.WholeWord)
                    {
                        var startOk = idx == 0 || SeparatorChars.Contains(text[idx - 1]);
                        var endOk = idx + findLen == text.Length || SeparatorChars.Contains(text[idx + findLen]);
                        if (startOk && endOk)
                            return idx;
                    }
                    else
                    {
                        return idx;
                    }
                    idx = text.IndexOf(_findText, idx + findLen, comparison);
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
            position = Math.Max(0, position);
            Success = false;
            bool machInOriginalSubtitle = allowEditOfOriginalSubtitle && originalSubtitle?.Paragraphs?.Count > 0;
            int prghCount = subtitle.Paragraphs.Count;
            for (int i = startIndex; i < prghCount; i++)
            {
                // always reset position after 1st lookup
                if (i > startIndex)
                {
                    position = 0;
                }

                if (!MatchInOriginal)
                {
                    position = GetTextPosition(subtitle.Paragraphs[i].Text, position);
                    if (position >= 0)
                    {
                        return UpdateSearchResult(i, position, false);
                    }
                    else
                    {
                        position = 0;
                    }
                }
                else
                {
                    MatchInOriginal = false;
                }

                if (machInOriginalSubtitle)
                {
                    Paragraph p2 = originalSubtitle.GetParagraphOrDefault(i);
                    if (p2 == null)
                    {
                        continue;
                    }
                    position = GetTextPosition(p2.Text, position);
                    if (position >= 0)
                    {
                        return UpdateSearchResult(i, position, true);
                    }
                }

            }

            return Success;
        }

        private bool UpdateSearchResult(int paragraphIdx, int textPosition, bool foundInOriginal)
        {
            SelectedIndex = paragraphIdx;
            SelectedPosition = textPosition;
            MatchInOriginal = foundInOriginal;
            return Success = true;
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

            if (++startIndex >= text.Length)
            {
                return false;
            }

            switch (FindReplaceType.FindType)
            {
                case FindType.Normal:
                case FindType.CaseSensitive:
                    string searchText = text.Substring(startIndex);
                    int pos = GetTextPosition(searchText, 0);
                    if (pos >= 0)
                    {
                        SelectedIndex = pos + startIndex;
                        Success = true;
                    }
                    break;

                case FindType.RegEx:
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
                    break;
            }
            return Success;
        }

        public int FindCount(Subtitle subtitle, bool wholeWord)
        {
            var count = 0;
            // validate pattern if find type is regex
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
            int patternLen = pattern.Length;
            while (idx >= 0)
            {
                if (matchWholeWord)
                {
                    var startOk = (idx == 0) || (SeparatorChars.Contains(text[idx - 1]));
                    var endOk = (idx + patternLen == text.Length) || (SeparatorChars.Contains(text[idx + patternLen]));
                    if (startOk && endOk)
                        count++;
                }
                else
                {
                    count++;
                }
                idx = text.IndexOf(pattern, idx + patternLen, comparison);
            }
            return count;
        }

        private StringComparison GetComparison() => FindReplaceType.FindType == FindType.Normal ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    }
}
