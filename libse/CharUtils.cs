namespace Nikse.SubtitleEdit.Core
{
    public static class CharUtils
    {
        /// <summary>
        /// Checks if character matches [0-9]
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool IsDigit(char ch)
        {
            return (ch >= '0') && (ch <= '9');
        }

        /// <summary>
        /// Checks if character matches [0-9A-Fa-f]
        /// </summary>
        public static bool IsHexDigit(char ch)
        {
            return (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f') || (ch >= '0' && ch <= '9');
        }

    }
}
