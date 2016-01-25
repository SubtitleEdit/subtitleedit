using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core
{
    public class NoBreakAfterItem : IComparable
    {
        public readonly Regex Regex;
        public readonly string Text;

        public NoBreakAfterItem(Regex regex, string text)
        {
            Regex = regex;
            Text = text;
        }

        public NoBreakAfterItem(string text)
        {
            Text = text;
        }

        public bool IsMatch(string line)
        {
            if (Regex != null)
                return Regex.IsMatch(line);

            if (!string.IsNullOrEmpty(Text) && line.EndsWith(Text, StringComparison.Ordinal))
                return true;

            return false;
        }

        public override string ToString()
        {
            return Text;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return -1;

            var o = obj as NoBreakAfterItem;
            if (o == null)
                return -1;

            if (o.Text == null && this.Text == null)
                return 0;

            if (o.Text == null)
                return -1;

            return string.Compare(Text, o.Text, StringComparison.Ordinal);
        }
    }
}
