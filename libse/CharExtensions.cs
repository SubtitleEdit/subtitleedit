namespace Nikse.SubtitleEdit.Core
{
    public static class CharExtensions
    {
        /// <summary>
        /// Checks if character matches [0-9]
        /// </summary>
        /// <param name="ch"></param>
        public static bool IsAsciiDigit(this char ch) => '0' <= ch && ch <= '9';

        /// <summary>
        /// Checks if given character is hexadecimal
        /// </summary>
        /// <param name="ch"></param>
        public static bool IsAsciiHexDigit(this char ch) => '0' <= ch && ch <= '9' || 'a' <= ch && ch <= 'f' || 'A' <= ch && ch <= 'F';

        /// <summary>
        /// Checks if character is between A-Z or a-z
        /// </summary>
        public static bool IsAsciiLetter(this char ch) => 'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z';
    }
}
