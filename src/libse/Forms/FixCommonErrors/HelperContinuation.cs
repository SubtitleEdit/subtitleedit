using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public static class HelperContinuation
    {
        private static string dashes = "-‐–—";
        private static string quotes = "'\"“”‘’«»‹›„“‚‘";
        private static string singleQuotes = "'‘’‘";
        private static string doubleQuotes = "''‘‘’’‚‚‘‘";

        public static string SanitizeString(string input)
        {
            string checkString = input;
            checkString = Regex.Replace(checkString, "<.*?>", String.Empty);
            checkString = Regex.Replace(checkString, "\\(.*?\\)", String.Empty);
            checkString = Regex.Replace(checkString, "\\[.*?\\]", String.Empty);
            checkString = Regex.Replace(checkString, "\\{.*?\\}", String.Empty);
            checkString = checkString.Trim();

            // Remove string elevation
            if (checkString.EndsWith("\r\n_") || checkString.EndsWith("\r\n.") || checkString.EndsWith("\n_") || checkString.EndsWith("\n."))
            {
                checkString = checkString.Substring(0, checkString.Length - 1).Trim();
            }

            // Remove dashes from the beginning
            if (dashes.Contains(checkString[0]))
            {
                checkString = checkString.Substring(1).Trim();
            }

            // Remove single-char quotes from the beginning
            if (quotes.Contains(checkString[0]))
            {
                if (singleQuotes.Contains(checkString[0]) && Char.IsLetter(checkString[1]) && !Char.IsUpper(checkString[1]) && Char.IsWhiteSpace(checkString[2]) && Char.IsLetter(checkString[3]))
                {
                    // 's Avonds -- don't remove
                }
                else
                {
                    checkString = checkString.Substring(1).Trim();
                }
            }

            // Remove double-char quotes from the beginning
            if (doubleQuotes.Contains(checkString.Substring(0, 2)))
            {
                checkString = checkString.Substring(2).Trim();
            }

            // Remove single-char quotes from the ending
            if (quotes.Contains(checkString[checkString.Length - 1]))
            {
                if (singleQuotes.Contains(checkString[checkString.Length - 1]))
                {
                    // Could be something like: nothin'
                    // TODO
                }

                checkString = checkString.Substring(0, checkString.Length - 1).Trim();
            }

            // Remove double-char quotes from the ending
            if (doubleQuotes.Contains(checkString.Substring(checkString.Length - 2, 2)))
            {
                checkString = checkString.Substring(0, checkString.Length - 2).Trim();
            }

            return checkString;
        }
    }
}
