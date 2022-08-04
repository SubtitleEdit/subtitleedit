namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public class CalcNoSpaceOrPunctuationCpsOnly : ICalcLength
    {
        /// <summary>
        /// Calculate all text except punctuation or space (tags are not counted) for cps only.
        /// Line length calc all characters.
        /// </summary>
        public decimal CountCharacters(string text, bool forCps)
        {
            if (!forCps)
            {
                return new CalcAll().CountCharacters(text, false);
            }

            const char zeroWidthSpace = '\u200B';
            const char zeroWidthNoBreakSpace = '\uFEFF';
            var length = 0;
            var ssaTagOn = false;
            var htmlTagOn = false;
            var max = text.Length;
            for (var i = 0; i < max; i++)
            {
                var ch = text[i];
                if (ssaTagOn)
                {
                    if (ch == '}')
                    {
                        ssaTagOn = false;
                    }
                }
                else if (htmlTagOn)
                {
                    if (ch == '>')
                    {
                        htmlTagOn = false;
                    }
                }
                else if (ch == '{' && i < text.Length - 1 && text[i + 1] == '\\')
                {
                    ssaTagOn = true;
                }
                else if (ch == '<' && i < text.Length - 1 && (text[i + 1] == '/' || char.IsLetter(text[i + 1])) &&
                         text.IndexOf('>', i) > 0 && TextLengthHelper.IsKnownHtmlTag(text, i))
                {
                    htmlTagOn = true;
                }
                else if (!char.IsControl(ch) &&
                         ch != ' ' &&
                         ch != '\t' &&
                         ch != zeroWidthSpace &&
                         ch != zeroWidthNoBreakSpace &&
                         ch != '\u200E' &&
                         ch != '\u200F' &&
                         ch != '\u202A' &&
                         ch != '\u202B' &&
                         ch != '\u202C' &&
                         ch != '\u202D' &&
                         ch != '\u202E' &&
                         ch != '\u02F8' &&// ˸ Modifier Letter Raised Colon (\u02F8)
                         ch != '\uFF1A' && // ： Fullwidth Colon (\uFF1A)
                         ch != '\uFE13' && // ︓ Presentation Form for Vertical Colon (\uFE13)
                         ch != '\u2043' && // ⁃ Hyphen bullet (\u2043)
                         ch != '\u2010' && // ‐ Hyphen (\u2010)
                         ch != '\u2012' && // ‒ Figure dash (\u2012)
                         ch != '\u2013' && // – En dash (\u2013)
                         ch != '-' &&
                         ch != '\'' &&
                         ch != '"' &&
                         ch != ':' &&
                         ch != '(' &&
                         ch != ')' &&
                         ch != '{' &&
                         ch != '}' &&
                         ch != '[' &&
                         ch != ']' &&
                         ch != '…' &&
                         ch != ',' &&
                         ch != '.' &&
                         ch != '!' &&
                         ch != '?' &&
                         ch != '¡' &&
                         ch != '¿')
                {
                    length++;
                }
            }

            return length;
        }
    }
}
