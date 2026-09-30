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
        /// space already there, nor inside brackets - "(<i>music</i>)", not "( <i>music</i> )".
        /// </summary>
        private static bool IsMidRowSpaceShown(StringBuilder sb, SerializedStyledUnicodeChar[] columns, int index)
        {
            if (sb.Length == 0 || char.IsWhiteSpace(sb[sb.Length - 1]) || "([{".IndexOf(sb[sb.Length - 1]) >= 0)
            {
                return false;
            }

            for (var i = index + 1; i < columns.Length; i++)
            {
                var next = columns[i].Character;
                if (next == Constants.MidRowSpace)
                {
                    continue;
                }

                return !string.IsNullOrWhiteSpace(next) && ")]}".IndexOf(next[0]) < 0;
            }

            return false;
        }
    }
}
