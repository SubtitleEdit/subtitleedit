using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class NoBreakAfterItem : IComparable<NoBreakAfterItem>
    {
        public Regex Regex { get; }
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
            // Make sure that both *line and *Text are not null.
            if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(Text))
            {
                return false;
            }

            if (Regex != null)
            {
                return Regex.IsMatch(line);
            }

            return EndsWithWholeWord(line.AsSpan());
        }

        /// <summary>
        /// Span overload so <see cref="Utilities.CanBreak"/> - which asks every list entry about
        /// every candidate break point in the line - does not have to allocate the substring
        /// before the break point. Only a regex entry still needs a string.
        /// </summary>
        public bool IsMatch(ReadOnlySpan<char> line)
        {
            if (line.IsEmpty || string.IsNullOrEmpty(Text))
            {
                return false;
            }

            if (Regex != null)
            {
                return Regex.IsMatch(line.ToString());
            }

            return EndsWithWholeWord(line);
        }

        private bool EndsWithWholeWord(ReadOnlySpan<char> line)
        {
            if (!line.EndsWith(Text.AsSpan(), StringComparison.Ordinal))
            {
                return false;
            }

            var indexBeforeText = line.Length - Text.Length - 1;
            return indexBeforeText < 0 || line[indexBeforeText] == ' ';
        }

        public override string ToString()
        {
            return Text;
        }

        public int CompareTo(NoBreakAfterItem obj)
        {
            if (obj == null)
            {
                return -1;
            }

            if (obj.Text == null && Text == null)
            {
                return 0;
            }
            else if (obj.Text == null)
            {
                return -1;
            }

            return string.Compare(Text, obj.Text, StringComparison.Ordinal);
        }
    }
}
