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
            foreach (var column in columns)
            {
                var character = column.Character;
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
    }
}
