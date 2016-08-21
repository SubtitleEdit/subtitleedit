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
    }
}
