using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic.Enums;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic
{
    public class FindReplaceDialogHelper
    {
        readonly string _findText = string.Empty;
        readonly string _replaceText = string.Empty;
        readonly Regex _regEx;
        int _findTextLenght;

        public bool Success { get; set; }
        public FindType FindType { get; set; }
        public int SelectedIndex { get; set; }
        public int SelectedPosition { get; set; }
        public int WindowPositionLeft { get; set; }
        public int WindowPositionTop { get; set; }
        public int StartLineIndex { get; set; }
        public bool MatchInOriginal { get; set; }
        public List<string> FindTextHistory { get; private set; }

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

        public FindReplaceDialogHelper(FindType findType, string findText, List<string> history, Regex regEx, string replaceText, int left, int top, int startLineIndex)
        {
            FindType = findType;
            _findText = findText;
            _replaceText = replaceText;
            _regEx = regEx;
            _findTextLenght = findText.Length;
            WindowPositionLeft = left;
            WindowPositionTop = top;
            StartLineIndex = startLineIndex;
            FindTextHistory = history;
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
            cm.MenuItems.Add("Word boundary (\\b)", delegate { textBox.SelectedText = "\\b"; }); //TODO: Add language tags
            cm.MenuItems.Add("Non word boundary (\\B)", delegate { textBox.SelectedText = "\\B"; });
            cm.MenuItems.Add("New line (\\r\\n)", delegate { textBox.SelectedText = "\\r\\n"; });
            cm.MenuItems.Add("Any digit (\\d)", delegate { textBox.SelectedText = "\\d"; });
            cm.MenuItems.Add("Any character (.)", delegate { textBox.SelectedText = "."; });
            cm.MenuItems.Add("Any whitespace", delegate { textBox.SelectedText = "\\s"; });
            cm.MenuItems.Add("Zero or more (*)", delegate { textBox.SelectedText = "*"; });
            cm.MenuItems.Add("One or more", delegate { textBox.SelectedText = "+"; });
            cm.MenuItems.Add("In character goup ([test])", delegate { textBox.SelectedText = "[test]"; });
            cm.MenuItems.Add("Not in character goup ([^test])", delegate { textBox.SelectedText = "[^test]"; });
            return cm;
        }

        public static ContextMenu GetReplaceTextContextMenu(TextBox textBox)
        {
            var cm = new ContextMenu();
            cm.MenuItems.Add("New line (\\n)", delegate { textBox.SelectedText = "\\n"; }); //TODO: Add language tags
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

        internal void AddHistory(string findText)
        {
            if (FindTextHistory.Contains(findText))
                FindTextHistory.Remove(findText);
            FindTextHistory.Add(findText);
        }
    }
}
