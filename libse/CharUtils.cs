namespace Nikse.SubtitleEdit.Core
{
    public static class CharUtils
    {
        /// <summary>
        /// Checks if character matches [0-9]
        /// </summary>
        /// <param name="ch"></param>
        public static bool IsDigit(char ch) => (ch >= '0') && (ch <= '9');

        /// <summary>
        /// Checkes if given character is hexadecimal
        /// </summary>
        /// <param name="ch"></param>
        public static bool IsHexadecimal(char ch) => (ch >= '0') && (ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F');
    }
}
