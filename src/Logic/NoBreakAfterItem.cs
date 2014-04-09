using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic
{
    public class NoBreakAfterItem
    {
        public Regex _regex;
        public string _text;
        private string p1;

        public NoBreakAfterItem(Regex regex)
        {
            _regex = regex;
        }

        public NoBreakAfterItem(string text)
        {
            _text = text;
        }

        public bool IsMatch(string line)
        {
            if (_regex != null)
                return _regex.IsMatch(line);

            if (!string.IsNullOrEmpty(_text) && line.EndsWith(_text))
                return true;

            return false;
        }

    }
}
