namespace Nikse.SubtitleEdit.Core.Common
{
    public static class CharUtils
    {
        /// <summary>
        /// Checks if character matches [0-9]
        /// </summary>
        /// <param name="ch"></param>
        public static bool IsAsciiDigit(char ch) => ch >= '0' && ch <= '9';

        /// <summary>
        /// Checks if given character is hexadecimal
        /// </summary>
        /// <param name="ch"></param>
        public static bool IsHexadecimal(char ch) => ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f' || ch >= 'A' && ch <= 'F';

        /// <summary>
        /// Checks if character is between A-Z or a-z
        /// </summary>
        /// <remarks>char.IsAsciiLetter (.NET 7+) is not available in netstandard2.1, which this library also targets.</remarks>
        public static bool IsAsciiLetter(char ch) => ch >= 'A' && ch <= 'z' && (ch <= 'Z' || ch >= 'a');
    }
}