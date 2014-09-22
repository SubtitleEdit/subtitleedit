using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic
{
    /// <summary>
    /// HTML specific string manipulations.
    /// </summary>
    internal static class HtmlUtils
    {
        public const string TAG_I = "i";
        public const string TAG_B = "b";
        public const string TAG_U = "u";
        public const string TAG_P = "p";
        public const string TAG_FONT = "font";
        public const string TAG_CYRILLIC_I = "\u0456"; // Cyrillic Small Letter Byelorussian-Ukrainian i (http://graphemica.com/%D1%96)

        /// <summary>
        /// Remove all of the specified opening and closing tags from the source HTML string.
        /// </summary>
        /// <param name="source">The source string to search for specified HTML tags.</param>
        /// <param name="tags">The HTML tags to remove.</param>
        /// <returns>A new string without the specified opening and closing tags.</returns>
        public static string RemoveOpenCloseTags(string source, params string[] tags)
        {
            // This pattern matches these tag formats:
            // <tag*>
            // < tag*>
            // </tag*>
            // < /tag*>
            // </ tag*>
            // < / tag*>
            return Regex.Replace(
                source,
                @"<\s*/?\s*([^\s>]+)[^>]*>",
                m => tags.Contains(m.Groups[1].Value, StringComparer.OrdinalIgnoreCase) ? string.Empty : m.Value,
                RegexOptions.Compiled);
        }
    }
}
