namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public class CalcIgnoreThaiCompositeCharacters : ICalcLength
    {
        /// <summary>
        /// Calculate all text excluding Thai composite characters (tags are not counted).
        /// Netflix rule: 35 characters per line (excluding all composite characters, i.e. tone marks, top and bottom vowels are not counted.
        /// See https://partnerhelp.netflixstudios.com/hc/en-us/articles/220448308-Thai-Timed-Text-Style-Guide
        /// </summary>
        public decimal CountCharacters(string text, bool forCps)
        {
            const char zeroWidthSpace = '\u200B';
            const char zeroWidthNoBreakSpace = '\uFEFF';
            const string thaiCompositeCharacters = "ํู่๊ี้"; //TODO: not correct...
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
                         ch != zeroWidthSpace &&
                         ch != zeroWidthNoBreakSpace &&
                         !thaiCompositeCharacters.Contains(ch))
                {
                    length++;
                }
            }

            return length;
        }
    }
}
