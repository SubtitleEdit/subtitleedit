using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Enums;

namespace Nikse.SubtitleEdit.Core
{
    public class FindReplaceDialogHelper
    {
        private const string StartChars = " >-\"”“['‘`´¶(♪¿¡.…—";
        private const string EndChars = " -\"”“]'`´¶)♪.!?:…—\r\n";
        private readonly string _findText = string.Empty;
        private readonly string _replaceText = string.Empty;
        private Regex _regEx;
        private int _findTextLenght;

        public bool Success { get; set; }
        public FindType FindType { get; set; }
        public int SelectedIndex { get; set; }
        public int SelectedPosition { get; set; }
        public int WindowPositionLeft { get; set; }
        public int WindowPositionTop { get; set; }
        public int StartLineIndex { get; set; }
        public bool MatchInOriginal { get; set; }

        public int FindTextLength
        {
            get
            {
                return _findTextLenght;
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

        public FindReplaceDialogHelper(FindType findType, string findText, Regex regEx, string replaceText, int left, int top, int startLineIndex)
        {
            FindType = findType;
            _findText = findText;
            _replaceText = replaceText;
            _regEx = regEx;
            _findTextLenght = findText.Length;
            WindowPositionLeft = left;
            WindowPositionTop = top;
            StartLineIndex = startLineIndex;
        }

        public bool Find(Subtitle subtitle, Subtitle originalSubtitle, int startIndex)
        {
            return FindNext(subtitle, originalSubtitle, startIndex, 0, Configuration.Settings.General.AllowEditOfOriginalSubtitle);
        }

        public bool Find(TextBox textBox, int startIndex)
        {
            return FindNext(textBox, startIndex);
        }

        private int FindPositionInText(string text, int startIndex)
        {
            if (startIndex >= text.Length && !(FindType == FindType.RegEx && startIndex == 0))
                return -1;

            switch (FindType)
            {
                case FindType.Normal:
                    return (text.IndexOf(_findText, startIndex, System.StringComparison.OrdinalIgnoreCase));
                case FindType.CaseSensitive:
                    return (text.IndexOf(_findText, startIndex, System.StringComparison.Ordinal));
                case FindType.RegEx:
                    {
                        Match match = _regEx.Match(text, startIndex);
                        if (match.Success)
                        {
                            string groupName = Utilities.GetRegExGroup(_findText);
                            if (groupName != null && match.Groups[groupName] != null && match.Groups[groupName].Success)
                            {
                                _findTextLenght = match.Groups[groupName].Length;
                                return match.Groups[groupName].Index;
                            }
                            _findTextLenght = match.Length;
                            return match.Index;
                        }
                        return -1;
                    }
            }
            return -1;
        }

        public bool FindNext(Subtitle subtitle, Subtitle originalSubtitle, int startIndex, int position, bool allowEditOfOriginalSubtitle)
        {
            Success = false;
            int index = 0;
            if (position < 0)
                position = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (index >= startIndex)
                {
                    int pos = 0;
                    if (!MatchInOriginal)
                    {
                        pos = FindPositionInText(p.Text, position);
                        if (pos >= 0)
                        {
                            MatchInOriginal = false;
                            SelectedIndex = index;
                            SelectedPosition = pos;
                            Success = true;
                            return true;
                        }
                        position = 0;
                    }
                    MatchInOriginal = false;

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
                                Success = true;
                                return true;
                            }
                        }
                    }
                }
                index++;
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

        public bool FindNext(TextBox textBox, int startIndex)
        {
            Success = false;
            startIndex++;
            if (startIndex < textBox.Text.Length)
            {
                if (FindType == FindType.RegEx)
                {
                    Match match = _regEx.Match(textBox.Text, startIndex);
                    if (match.Success)
                    {
                        string groupName = Utilities.GetRegExGroup(_findText);
                        if (groupName != null && match.Groups[groupName] != null && match.Groups[groupName].Success)
                        {
                            _findTextLenght = match.Groups[groupName].Length;
                            SelectedIndex = match.Groups[groupName].Index;
                        }
                        else
                        {
                            _findTextLenght = match.Length;
                            SelectedIndex = match.Index;
                        }
                        Success = true;
                    }
                    return match.Success;
                }
                string searchText = textBox.Text.Substring(startIndex);
                int pos = FindPositionInText(searchText, 0);
                if (pos >= 0)
                {
                    SelectedIndex = pos + startIndex;
                    return true;
                }
            }
            return false;
        }

        public int FindCount(Subtitle subtitle, bool wholeWord)
        {
            var count = 0;
            //  validate pattern if find type is regex
            if (FindType == FindType.RegEx)
            {
                if (!Utilities.IsValidRegex(FindText))
                {
                    MessageBox.Show(Configuration.Settings.Language.General.RegularExpressionIsNotValid);
                    return count;
                }
                _regEx = new Regex(_findText);
            }

            // count matches
            foreach (var p in subtitle.Paragraphs)
            {
                if (p.Text.Length < FindText.Length)
                    continue;

                switch (FindType)
                {
                    case FindType.Normal:
                        count += GetWordCount(p.Text, _findText, wholeWord, StringComparison.OrdinalIgnoreCase);
                        break;
                    case FindType.CaseSensitive:
                        count += GetWordCount(p.Text, _findText, wholeWord, StringComparison.Ordinal);
                        break;
                    case FindType.RegEx:
                        count += _regEx.Matches(p.Text).Count;
                        break;
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
                    var startOk = (idx == 0) || (StartChars.Contains(text[idx - 1]));
                    var endOk = (idx + pattern.Length == text.Length) || (EndChars.Contains(text[idx + pattern.Length]));
                    if (startOk && endOk)
                        count++;
                }
                else
                {
                    count++;
                }
                idx = text.IndexOf(pattern, idx + pattern.Length, comparison);
            }
            return count;
        }
    }
}