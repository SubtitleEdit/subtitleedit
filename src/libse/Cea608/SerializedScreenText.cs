using System.Text;

namespace Nikse.SubtitleEdit.Core.Cea608
{
    public static class SerializedScreenText
    {
        /// <summary>
        /// Plain text of a decoded caption screen, one line per row, with italic runs
        /// (from italic PACs or mid-row codes) wrapped in &lt;i&gt; tags.
        /// </summary>
        public static string GetText(SerializedRow[] screen)
        {
            var sb = new StringBuilder();
            foreach (var row in screen)
            {
                var line = GetRowText(row.Columns);
                if (line.Length > 0)
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString().Trim();
        }

        private static string GetRowText(SerializedStyledUnicodeChar[] columns)
        {
            var sb = new StringBuilder();
            var italic = false;
            var endOfText = 0; // length of sb up to and including the last non-space char
            for (var index = 0; index < columns.Length; index++)
            {
                var column = columns[index];
                var character = column.Character;
                if (character == Constants.MidRowSpace)
                {
                    character = IsMidRowSpaceShown(sb, columns, index) ? " " : string.Empty;
                }

                if (string.IsNullOrWhiteSpace(character))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(character);
                    }

                    continue;
                }

                // Spaces don't switch style (a mid-row code itself shows as a space), so tags
                // open right before the next visible char and close right after the last one.
                var charItalic = column.Style?.Italics == true;
                if (charItalic != italic)
                {
                    if (charItalic)
                    {
                        sb.Append("<i>");
                    }
                    else
                    {
                        sb.Insert(endOfText, "</i>");
                    }

                    italic = charItalic;
                }

                sb.Append(character);
                endOfText = sb.Length;
            }

            sb.Length = endOfText;
            if (italic)
            {
                sb.Append("</i>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// A mid-row code shows as a space ("that you <i>were</i> smelling"), but not next to a
        /// space already there, nor inside brackets or quotes - "(<i>music</i>)", not
        /// "( <i>music</i> )" - nor before closing punctuation: "see <i>Jaws</i>?", not
        /// "see <i>Jaws</i> ?". Subtitle Edit's own SCC writer puts the reset code right before
        /// the next char, with no space of its own.
        /// </summary>
        private static bool IsMidRowSpaceShown(StringBuilder sb, SerializedStyledUnicodeChar[] columns, int index)
        {
            var previous = GetPreviousCharIndex(sb, sb.Length);
            if (previous < 0 || char.IsWhiteSpace(sb[previous]) || IsOpeningMark(sb, previous))
            {
                return false;
            }

            var next = GetNextCharIndex(columns, index + 1);
            return next >= 0 && !string.IsNullOrWhiteSpace(columns[next].Character) && !IsClosingMark(columns, next);
        }

        private static int GetNextCharIndex(SerializedStyledUnicodeChar[] columns, int start)
        {
            for (var i = start; i < columns.Length; i++)
            {
                if (columns[i].Character != Constants.MidRowSpace)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Index of the last char in sb before "end", skipping italic tags; -1 if there is none.
        /// </summary>
        private static int GetPreviousCharIndex(StringBuilder sb, int end)
        {
            var i = end - 1;
            while (i >= 0 && sb[i] == '>')
            {
                if (i >= 2 && sb[i - 1] == 'i' && sb[i - 2] == '<')
                {
                    i -= 3;
                }
                else if (i >= 3 && sb[i - 1] == 'i' && sb[i - 2] == '/' && sb[i - 3] == '<')
                {
                    i -= 4;
                }
                else
                {
                    break;
                }
            }

            return i;
        }

        private static bool IsOpeningMark(StringBuilder sb, int index)
        {
            var ch = sb[index];
            if ("([{“‘¿¡".IndexOf(ch) >= 0)
            {
                return true;
            }

            if (ch == '"' || ch == '\'')
            {
                // A straight quote opens at the start or after a space or bracket: "<i>Jaws</i>"
                // - after a word it closes (or is an apostrophe).
                var before = GetPreviousCharIndex(sb, index);
                return before < 0 || char.IsWhiteSpace(sb[before]) || "([{".IndexOf(sb[before]) >= 0;
            }

            return false;
        }

        private static bool IsClosingMark(SerializedStyledUnicodeChar[] columns, int index)
        {
            var ch = columns[index].Character[0];
            if (".,!?;:)]}'…’”".IndexOf(ch) >= 0)
            {
                return true; // a straight apostrophe right after a word: "<i>Jaws</i>'s"
            }

            if (ch == '"')
            {
                // A straight double quote closes unless a word follows it: "<i>Jaws</i>", but
                // "Hi <i>yo</i> "ok"" keeps its space.
                var after = GetNextCharIndex(columns, index + 1);
                return after < 0 || string.IsNullOrEmpty(columns[after].Character) || !char.IsLetterOrDigit(columns[after].Character[0]);
            }

            return false;
        }
    }
}
