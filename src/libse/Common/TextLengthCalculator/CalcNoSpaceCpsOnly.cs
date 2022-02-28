namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public  class CalcNoSpaceCpsOnly : ICalcLength
    {
        /// <summary>
        /// Calculate all text excluding space (tags are not counted).
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
                         ch != zeroWidthSpace &&
                         ch != zeroWidthNoBreakSpace &&
                         ch != '\u200E' &&
                         ch != '\u200F' &&
                         ch != '\u202A' &&
                         ch != '\u202B' &&
                         ch != '\u202C' &&
                         ch != '\u202D' &&
                         ch != '\u202E')
                {
                    length++;
                }
            }

            return length;
        }
    }
}
