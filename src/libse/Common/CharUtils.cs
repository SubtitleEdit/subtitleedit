namespace Nikse.SubtitleEdit.Core.Common
{
    public static class CharUtils
    {
        /// <summary>
        /// Determines whether the given character is within the ANSI range.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>
        /// <c>true</c> if the given character is within the ANSI range (0-127); otherwise, <c>false</c>.
        /// </returns> 
        public static bool IsAnsi(char ch) => ch >= 0 && ch <= 127;
        
        /// <summary>
        /// Checks if character matches [0-9]
        /// </summary>
        /// <param name="ch"></param>
        public static bool IsDigit(char ch) => ch >= '0' && ch <= '9';

        /// <summary>
        /// Checks if given character is hexadecimal
        /// </summary>
        /// <param name="ch"></param>
        public static bool IsHexadecimal(char ch) => ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f' || ch >= 'A' && ch <= 'F';

        /// <summary>
        /// Checks if character is between A-Z or a-z
        /// </summary>
        public static bool IsEnglishAlphabet(char ch) => ch >= 'A' && ch <= 'z' && (ch <= 'Z' || ch >= 'a');
    }
}