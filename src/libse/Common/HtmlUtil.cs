using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// HTML specific string manipulations.
    /// </summary>
    public static class HtmlUtil
    {
        /// <summary>
        /// Represents the HTML tag used for italic text formatting.
        /// </summary>
        public static string TagItalic => "i";

        /// <summary>
        /// Represents the HTML tag used for bold text formatting.
        /// </summary>
        public static string TagBold => "b";

        /// <summary>
        /// Represents the HTML tag used for underlined text formatting.
        /// </summary>
        public static string TagUnderline => "u";

        /// <summary>
        /// Represents the HTML tag used for specifying font attributes.
        /// </summary>
        public static string TagFont => "font";

        /// <summary>
        /// Represents the HTML tag used for the Cyrillic character 'і'.
        /// </summary>
        public static string TagCyrillicI => "\u0456"; // Cyrillic Small Letter Byelorussian-Ukrainian i (http://graphemica.com/%D1%96)

        private static readonly Regex TagOpenRegex = new Regex(@"<\s*(?:/\s*)?(\w+)[^>]*>", RegexOptions.Compiled);

        /// <summary>
        /// Remove all of the specified opening and closing tags from the source HTML string.
        /// </summary>
        /// <param name="source">The source string to search for specified HTML tags.</param>
        /// <param name="tags">The HTML tags to remove.</param>
        /// <returns>A new string without the specified opening and closing tags.</returns>
        public static string RemoveOpenCloseTags(string source, params string[] tags)
        {
            if (string.IsNullOrEmpty(source) || source.IndexOf('<') < 0)
            {
                return source;
            }

            // This pattern matches these tag formats:
            // <tag*>
            // < tag*>
            // </tag*>
            // < /tag*>
            // </ tag*>
            // < / tag*>
            return TagOpenRegex.Replace(source, m => tags.Contains(m.Groups[1].Value, StringComparer.OrdinalIgnoreCase) ? string.Empty : m.Value);
        }

        /// <summary>
        /// Converts a string to an HTML-encoded string using named character references.
        /// </summary>
        /// <param name="source">The string to encode.</param>
        /// <returns>An encoded string.</returns>
        public static string EncodeNamed(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            var encoded = new StringBuilder(source.Length);
            foreach (var ch in source)
            {
                switch (ch)
                {
                    case '<':
                        encoded.Append("&lt;");
                        break;
                    case '>':
                        encoded.Append("&gt;");
                        break;
                    case '"':
                        encoded.Append("&quot;");
                        break;
                    case '&':
                        encoded.Append("&amp;");
                        break;
                    case '\'':
                        encoded.Append("&apos;");
                        break;
                    case ' ':
                        encoded.Append("&nbsp;");
                        break;
                    case '–':
                        encoded.Append("&ndash;");
                        break;
                    case '—':
                        encoded.Append("&mdash;");
                        break;
                    case '¡':
                        encoded.Append("&iexcl;");
                        break;
                    case '¿':
                        encoded.Append("&iquest;");
                        break;
                    case '“':
                        encoded.Append("&ldquo;");
                        break;
                    case '”':
                        encoded.Append("&rdquo;");
                        break;
                    case '‘':
                        encoded.Append("&lsquo;");
                        break;
                    case '’':
                        encoded.Append("&rsquo;");
                        break;
                    case '«':
                        encoded.Append("&laquo;");
                        break;
                    case '»':
                        encoded.Append("&raquo;");
                        break;
                    case '¢':
                        encoded.Append("&cent;");
                        break;
                    case '©':
                        encoded.Append("&copy;");
                        break;
                    case '÷':
                        encoded.Append("&divide;");
                        break;
                    case 'µ':
                        encoded.Append("&micro;");
                        break;
                    case '·':
                        encoded.Append("&middot;");
                        break;
                    case '¶':
                        encoded.Append("&para;");
                        break;
                    case '±':
                        encoded.Append("&plusmn;");
                        break;
                    case '€':
                        encoded.Append("&euro;");
                        break;
                    case '£':
                        encoded.Append("&pound;");
                        break;
                    case '®':
                        encoded.Append("&reg;");
                        break;
                    case '§':
                        encoded.Append("&sect;");
                        break;
                    case '™':
                        encoded.Append("&trade;");
                        break;
                    case '¥':
                        encoded.Append("&yen;");
                        break;
                    case 'á':
                        encoded.Append("&aacute;");
                        break;
                    case 'Á':
                        encoded.Append("&Aacute;");
                        break;
                    case 'à':
                        encoded.Append("&agrave;");
                        break;
                    case 'À':
                        encoded.Append("&Agrave;");
                        break;
                    case 'â':
                        encoded.Append("&acirc;");
                        break;
                    case 'Â':
                        encoded.Append("&Acirc;");
                        break;
                    case 'å':
                        encoded.Append("&aring;");
                        break;
                    case 'Å':
                        encoded.Append("&Aring;");
                        break;
                    case 'ã':
                        encoded.Append("&atilde;");
                        break;
                    case 'Ã':
                        encoded.Append("&Atilde;");
                        break;
                    case 'ä':
                        encoded.Append("&auml;");
                        break;
                    case 'Ä':
                        encoded.Append("&Auml;");
                        break;
                    case 'æ':
                        encoded.Append("&aelig;");
                        break;
                    case 'Æ':
                        encoded.Append("&AElig;");
                        break;
                    case 'ç':
                        encoded.Append("&ccedil;");
                        break;
                    case 'Ç':
                        encoded.Append("&Ccedil;");
                        break;
                    case 'é':
                        encoded.Append("&eacute;");
                        break;
                    case 'É':
                        encoded.Append("&Eacute;");
                        break;
                    case 'è':
                        encoded.Append("&egrave;");
                        break;
                    case 'È':
                        encoded.Append("&Egrave;");
                        break;
                    case 'ê':
                        encoded.Append("&ecirc;");
                        break;
                    case 'Ê':
                        encoded.Append("&Ecirc;");
                        break;
                    case 'ë':
                        encoded.Append("&euml;");
                        break;
                    case 'Ë':
                        encoded.Append("&Euml;");
                        break;
                    case 'í':
                        encoded.Append("&iacute;");
                        break;
                    case 'Í':
                        encoded.Append("&Iacute;");
                        break;
                    case 'ì':
                        encoded.Append("&igrave;");
                        break;
                    case 'Ì':
                        encoded.Append("&Igrave;");
                        break;
                    case 'î':
                        encoded.Append("&icirc;");
                        break;
                    case 'Î':
                        encoded.Append("&Icirc;");
                        break;
                    case 'ï':
                        encoded.Append("&iuml;");
                        break;
                    case 'Ï':
                        encoded.Append("&Iuml;");
                        break;
                    case 'ñ':
                        encoded.Append("&ntilde;");
                        break;
                    case 'Ñ':
                        encoded.Append("&Ntilde;");
                        break;
                    case 'ó':
                        encoded.Append("&oacute;");
                        break;
                    case 'Ó':
                        encoded.Append("&Oacute;");
                        break;
                    case 'ò':
                        encoded.Append("&ograve;");
                        break;
                    case 'Ò':
                        encoded.Append("&Ograve;");
                        break;
                    case 'ô':
                        encoded.Append("&ocirc;");
                        break;
                    case 'Ô':
                        encoded.Append("&Ocirc;");
                        break;
                    case 'ø':
                        encoded.Append("&oslash;");
                        break;
                    case 'Ø':
                        encoded.Append("&Oslash;");
                        break;
                    case 'õ':
                        encoded.Append("&otilde;");
                        break;
                    case 'Õ':
                        encoded.Append("&Otilde;");
                        break;
                    case 'ö':
                        encoded.Append("&ouml;");
                        break;
                    case 'Ö':
                        encoded.Append("&Ouml;");
                        break;
                    case 'ß':
                        encoded.Append("&szlig;");
                        break;
                    case 'ú':
                        encoded.Append("&uacute;");
                        break;
                    case 'Ú':
                        encoded.Append("&Uacute;");
                        break;
                    case 'ù':
                        encoded.Append("&ugrave;");
                        break;
                    case 'Ù':
                        encoded.Append("&Ugrave;");
                        break;
                    case 'û':
                        encoded.Append("&ucirc;");
                        break;
                    case 'Û':
                        encoded.Append("&Ucirc;");
                        break;
                    case 'ü':
                        encoded.Append("&uuml;");
                        break;
                    case 'Ü':
                        encoded.Append("&Uuml;");
                        break;
                    case 'ÿ':
                        encoded.Append("&yuml;");
                        break;
                    default:
                        if (ch > 127)
                        {
                            encoded.Append("&#").Append((int)ch).Append(';');
                        }
                        else
                        {
                            encoded.Append(ch);
                        }
                        break;
                }
            }
            return encoded.ToString();
        }

        /// <summary>
        /// Converts a string to an HTML-encoded string using numeric character references.
        /// </summary>
        /// <param name="source">The string to encode.</param>
        /// <returns>An encoded string.</returns>
        public static string EncodeNumeric(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            var encoded = new StringBuilder(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                var ch = source[i];
                if (ch == ' ')
                {
                    encoded.Append("&#");
                    encoded.Append(160); // &nbsp;
                    encoded.Append(';');
                }
                else if (char.IsHighSurrogate(ch) && i + 1 < source.Length && char.IsLowSurrogate(source[i + 1]))
                {
                    // Emit the full code point, not the two surrogate halves separately
                    // (a numeric reference to a lone surrogate is invalid and won't round-trip).
                    encoded.Append("&#");
                    encoded.Append(char.ConvertToUtf32(ch, source[i + 1]));
                    encoded.Append(';');
                    i++;
                }
                else if (ch > 127 || ch == '<' || ch == '>' || ch == '"' || ch == '&' || ch == '\'')
                {
                    encoded.Append("&#");
                    encoded.Append((int)ch);
                    encoded.Append(';');
                }
                else
                {
                    encoded.Append(ch);
                }
            }
            return encoded.ToString();
        }

        /// <summary>
        /// Remove all HTML tags from the input string, optionally including SSA tags.
        /// </summary>
        /// <param name="input">The input string that may contain HTML tags.</param>
        /// <param name="alsoSsaTags">A boolean value indicating whether SSA tags should also be removed.</param>
        /// <returns>A new string with all HTML tags removed, and optionally SSA tags removed.</returns>
        public static string RemoveHtmlTags(string input, bool alsoSsaTags = false)
        {
            if (input == null || input.Length < 3)
            {
                return input;
            }

            var s = input;
            if (alsoSsaTags)
            {
                s = Utilities.RemoveSsaTags(s);
            }

            if (s.IndexOf('<') < 0)
            {
                return s;
            }

            if (s.Contains("< ", StringComparison.Ordinal))
            {
                s = FixInvalidItalicTags(s);
            }

            // One pass over the '<' positions instead of up to seven substring scans:
            // the box/v/c handling below is only relevant when a tag actually starts
            // with the matching letter, which typical lines (<i>, <font>, plain text)
            // never have.
            var hasBTag = false;
            var hasVTag = false;
            var hasCTag = false;
            for (var i = s.IndexOf('<'); i >= 0 && i < s.Length - 1; i = s.IndexOf('<', i + 1))
            {
                var next = s[i + 1];
                if (next == '/' && i + 2 < s.Length)
                {
                    next = s[i + 2];
                }

                if (next == 'b')
                {
                    hasBTag = true;
                }
                else if (next == 'v')
                {
                    hasVTag = true;
                }
                else if (next == 'c')
                {
                    hasCTag = true;
                }
            }

            if (hasBTag)
            {
                s = s.Replace("<box>", string.Empty).Replace("</box>", string.Empty);
            }

            // v tag from WebVTT
            if (hasVTag)
            {
                while (true)
                {
                    var indexOfVTag = s.IndexOf("<v ", StringComparison.Ordinal);
                    if (indexOfVTag < 0)
                    {
                        indexOfVTag = s.IndexOf("<v.", StringComparison.Ordinal);
                    }
                    if (indexOfVTag < 0)
                    {
                        break;
                    }
                    var indexOfEndVTag = s.IndexOf('>', indexOfVTag);
                    if (indexOfEndVTag < 0)
                    {
                        break;
                    }
                    s = s.Remove(indexOfVTag, indexOfEndVTag - indexOfVTag + 1);
                }
                s = s.Replace("</v>", string.Empty);
            }

            // c tag from WebVTT
            if (hasCTag)
            {
                while (true)
                {
                    var indexOfCTag = s.IndexOf("<c.", StringComparison.Ordinal);
                    if (indexOfCTag < 0)
                    {
                        break;
                    }
                    var indexOfEndCTag = s.IndexOf('>', indexOfCTag);
                    if (indexOfEndCTag < 0)
                    {
                        break;
                    }
                    s = s.Remove(indexOfCTag, indexOfEndCTag - indexOfCTag + 1);
                }
                s = s.Replace("</c>", string.Empty);
            }

            return RemoveCommonHtmlTags(s);
        }

        /// <summary>
        /// Optimized method to remove common html tags, like <i>, <b>, <u>, and <font>
        /// </summary>
        /// <param name="s">Text to remove html tags from</param>
        /// <returns>Text stripped from common html tags</returns>
        private static string RemoveCommonHtmlTags(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            // Use stackalloc for strings up to 512 chars to avoid heap allocation
            char[] poolArray = null;
            Span<char> buffer = s.Length <= 512 ?
                stackalloc char[s.Length] :
                (poolArray = System.Buffers.ArrayPool<char>.Shared.Rent(s.Length));

            try
            {
                int arrayIndex = 0;
                ReadOnlySpan<char> span = s.AsSpan();

                for (var i = 0; i < span.Length;)
                {
                    // If we hit an opening bracket, check if it's a target tag
                    if (span[i] == '<')
                    {
                        if (TryGetTagLength(span.Slice(i), out int tagLength))
                        {
                            i += tagLength;
                            continue;
                        }
                    }

                    // Normal character processing
                    buffer[arrayIndex++] = span[i];
                    i++;
                }

                // Nothing was stripped (e.g. a stray '<' that is not a known tag);
                // return the original instead of allocating an identical copy.
                if (arrayIndex == span.Length)
                {
                    return s;
                }

                return new string(buffer.Slice(0, arrayIndex));
            }
            finally
            {
                if (poolArray != null)
                {
                    System.Buffers.ArrayPool<char>.Shared.Return(poolArray);
                }
            }
        }

        private static bool TryGetTagLength(ReadOnlySpan<char> slice, out int length)
        {
            length = 0;

            // Ordered by most common/shortest for micro-optimization
            // 3-character tags: <i>, <b>, <u>
            if (slice.Length >= 3 && slice[2] == '>')
            {
                var c = slice[1];
                if (c == 'i' || c == 'I' || c == 'b' || c == 'B' || c == 'u' || c == 'U')
                {
                    length = 3;
                    return true;
                }
            }

            // 4-character tags: </i>, <b/>, etc.
            if (slice.Length >= 4 && slice[3] == '>')
            {
                // Case: </x>
                if (slice[1] == '/')
                {
                    var c = slice[2];
                    if (c == 'i' || c == 'I' || c == 'b' || c == 'B' || c == 'u' || c == 'U')
                    {
                        length = 4;
                        return true;
                    }
                }
                // Case: <x/>
                if (slice[2] == '/')
                {
                    var c = slice[1];
                    if (c == 'i' || c == 'I' || c == 'b' || c == 'B' || c == 'u' || c == 'U')
                    {
                        length = 4;
                        return true;
                    }
                }
            }

            // Longer tags: <font>, </font>, and malformed variants
            if (slice.StartsWith("<font", StringComparison.OrdinalIgnoreCase))
            {
                int endIdx = slice.IndexOf('>');
                if (endIdx != -1) { length = endIdx + 1; return true; }
            }

            // Closing font tag and its malformed whitespace variants. Checked
            // directly (interned literals, no per-call array, no loop) - reached
            // for every </font> in a styled file, so it is on the hot path.
            if (slice.StartsWith("</font>".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                length = 7;
                return true;
            }
            if (slice.StartsWith("< /font>".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                slice.StartsWith("</ font>".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                length = 8;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified text is a URL.
        /// </summary>
        /// <param name="text">The text to evaluate.</param>
        /// <returns>True if the text is considered a URL, otherwise false.</returns>
        public static bool IsUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < 6 || text.IndexOf('.') < 0 || text.IndexOf(' ') >= 0)
            {
                return false;
            }

            // Ordinal-ignore-case compares instead of ToLowerInvariant(), which allocated a
            // full lowercase copy of every candidate word (this runs per word in the
            // FixCommonErrors / line-split "don't touch URLs" checks).
            if (text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || text.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("www.", StringComparison.OrdinalIgnoreCase) || text.EndsWith(".org", StringComparison.OrdinalIgnoreCase) ||
                text.EndsWith(".com", StringComparison.OrdinalIgnoreCase) || text.EndsWith(".net", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (text.Contains(".org/", StringComparison.OrdinalIgnoreCase) || text.Contains(".com/", StringComparison.OrdinalIgnoreCase) || text.Contains(".net/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if the provided text starts with a URL-like string.
        /// </summary>
        /// <param name="text">The text to examine.</param>
        /// <returns>True if the text starts with a URL-like string; otherwise, false.</returns>
        public static bool StartsWithUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var arr = text.Trim().TrimEnd('.').TrimEnd().Split();
            if (arr.Length == 0)
            {
                return false;
            }

            return IsUrl(arr[0]);
        }

        private static readonly string[] UppercaseTags = { "<I>", "<U>", "<B>", "<FONT", "</I>", "</U>", "</B>", "</FONT>" };

        /// <summary>
        /// Converts all uppercase HTML tags in the input string to lowercase.
        /// </summary>
        /// <param name="input">The input string containing HTML tags to be converted.</param>
        /// <returns>A new string with all uppercase HTML tags converted to lowercase.</returns>
        public static string FixUpperTags(string input)
        {
            if (string.IsNullOrEmpty(input) || input.IndexOf('<') < 0)
            {
                return input;
            }

            // Single forward pass - the old rescan-from-zero loop did eight IndexOf passes plus
            // two full-string copies per fixed tag, and this runs per line from AutoBreakLine.
            char[] chars = null;
            var i = 0;
            while (i < input.Length)
            {
                if (input[i] == '<' && StartsWithUppercaseTag(input, i))
                {
                    var endIdx = input.IndexOf('>', i + 2);
                    if (endIdx < 0)
                    {
                        break;
                    }

                    chars ??= input.ToCharArray();
                    for (var k = i; k < endIdx; k++)
                    {
                        chars[k] = char.ToLowerInvariant(chars[k]);
                    }

                    i = endIdx + 1;
                    continue;
                }

                i++;
            }

            return chars == null ? input : new string(chars);
        }

        private static bool StartsWithUppercaseTag(string input, int index)
        {
            // Matches UppercaseTags: <I> <U> <B> <FONT </I> </U> </B> </FONT>
            var remaining = input.Length - index;
            if (remaining < 3)
            {
                return false;
            }

            var c1 = input[index + 1];
            if (c1 == 'I' || c1 == 'U' || c1 == 'B')
            {
                return input[index + 2] == '>';
            }

            if (c1 == 'F')
            {
                return remaining >= 5 && input[index + 2] == 'O' && input[index + 3] == 'N' && input[index + 4] == 'T';
            }

            if (c1 == '/' && remaining >= 4)
            {
                var c2 = input[index + 2];
                if (c2 == 'I' || c2 == 'U' || c2 == 'B')
                {
                    return input[index + 3] == '>';
                }

                if (c2 == 'F')
                {
                    return remaining >= 7 && input[index + 3] == 'O' && input[index + 4] == 'N' && input[index + 5] == 'T' && input[index + 6] == '>';
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the provided text contains formattable content not enclosed within HTML-like tags.
        /// </summary>
        /// <param name="text">The input text to be checked.</param>
        /// <returns>True if the text contains any formattable content (letters or digits) outside of HTML-like tags; otherwise, false.</returns>
        public static bool IsTextFormattable(in string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var len = text.Length;
            var index = 0;
            while (index < len && text[index] == '<')
            {
                index = text.IndexOf('>', index + 1);
                if (index < 0) break;
                index += 1;
            }

            // buggy text of no closing present
            index = Math.Max(0, index);

            var fromLenIdx = len - 1;
            while (fromLenIdx >= 0 && text[fromLenIdx] == '>')
            {
                fromLenIdx = text.LastIndexOf('<', fromLenIdx);
                if (fromLenIdx < 0) break;
                fromLenIdx--;
            }

            fromLenIdx = fromLenIdx > 0 ? fromLenIdx : len - 1;

            // no formattable text in between
            if (fromLenIdx < index)
            {
                return false;
            }

            for (var i = index; i <= fromLenIdx; i++)
            {
                if (char.IsLetterOrDigit(text[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static readonly string[] BeginTagVariations = { "< i >", "< i>", "<i >", "< I >", "< I>", "<I >", "<i<", "<I<", "<I>" };

        private static readonly string[] EndTagVariations =
        {
            "< / i >", "< /i>", "</ i>", "< /i >", "</i >", "</ i >",
            "< / i>", "</I>", "< / I >", "< /I>", "</ I>", "< /I >", "</I >", "</ I >", "< / I>", "</i<", "</I<", "</I>"
        };

        /// <summary>
        /// Fix invalid or improperly formatted italic tags in the input HTML string.
        /// </summary>
        /// <param name="input">The input HTML string to process.</param>
        /// <returns>A string with corrected italic tags.</returns>
        public static string FixInvalidItalicTags(string input)
        {
            var text = input;

            var preTags = string.Empty;
            if (text.StartsWith("{\\", StringComparison.Ordinal))
            {
                var endIdx = text.IndexOf('}', 2);
                if (endIdx > 2)
                {
                    preTags = text.Substring(0, endIdx + 1);
                    text = text.Remove(0, endIdx + 1);
                }
            }

            const string beginTag = "<i>";
            const string endTag = "</i>";
            foreach (var beginTagVariation in BeginTagVariations)
            {
                text = text.Replace(beginTagVariation, beginTag);
            }

            foreach (var endTagVariation in EndTagVariations)
            {
                text = text.Replace(endTagVariation, endTag);
            }

            text = text.Replace("</i> <i>", "_@_");
            text = text.Replace(" _@_", "_@_");
            text = text.Replace(" _@_ ", "_@_");
            text = text.Replace("_@_", " ");
            text = text.Replace(" </i>" + Environment.NewLine, "</i>" + Environment.NewLine);

            if (text.Contains(beginTag))
            {
                text = text.Replace("<i/>", endTag);
                text = text.Replace("<I/>", endTag);
            }
            else
            {
                text = text.Replace("<i/>", string.Empty);
                text = text.Replace("<I/>", string.Empty);
            }

            text = text.Replace("]<i> ", "] <i>");
            text = text.Replace(")<i> ", ") <i>");
            text = text.Replace("] </i>", "] </i>");
            text = text.Replace(") </i>", ") </i>");

            text = text.Replace(beginTag + beginTag, beginTag);
            text = text.Replace(endTag + endTag, endTag);

            var italicBeginTagCount = Utilities.CountTagInText(text, beginTag);
            var italicEndTagCount = Utilities.CountTagInText(text, endTag);
            var noOfLines = Utilities.GetNumberOfLines(text);
            if (italicBeginTagCount + italicEndTagCount == 0)
            {
                return preTags + text;
            }

            if (italicBeginTagCount == 1 && italicEndTagCount == 1 && text.IndexOf(beginTag, StringComparison.Ordinal) > text.IndexOf(endTag, StringComparison.Ordinal))
            {
                const string pattern = "___________@";
                text = text.Replace(beginTag, pattern);
                text = text.Replace(endTag, beginTag);
                text = text.Replace(pattern, endTag);
            }

            if (italicBeginTagCount == 2 && italicEndTagCount == 0)
            {
                var firstIndex = text.IndexOf(beginTag, StringComparison.Ordinal);
                var lastIndex = text.LastIndexOf(beginTag, StringComparison.Ordinal);
                var lastIndexWithNewLine = text.LastIndexOf(Environment.NewLine + beginTag, StringComparison.Ordinal) + Environment.NewLine.Length;
                if (noOfLines == 2 && lastIndex == lastIndexWithNewLine && firstIndex < 2)
                {
                    text = text.Replace(Environment.NewLine, endTag + Environment.NewLine) + endTag;
                }
                else
                {
                    text = text.Remove(lastIndex, beginTag.Length).Insert(lastIndex, endTag);
                }
            }

            if (italicBeginTagCount == 1 && italicEndTagCount == 2)
            {
                var firstIndex = text.IndexOf(endTag, StringComparison.Ordinal);
                if (text.StartsWith("</i>-<i>-", StringComparison.Ordinal) ||
                    text.StartsWith("</i>- <i>-", StringComparison.Ordinal) ||
                    text.StartsWith("</i>- <i> -", StringComparison.Ordinal) ||
                    text.StartsWith("</i>-<i> -", StringComparison.Ordinal))
                {
                    text = text.Remove(0, 5);
                }
                else if (firstIndex == 0)
                {
                    text = text.Remove(0, 4);
                }
                else
                {
                    text = text.Substring(0, firstIndex) + text.Substring(firstIndex + endTag.Length);
                }
            }

            if (italicBeginTagCount == 2 && italicEndTagCount == 1)
            {
                var lines = text.SplitToLines();
                if (lines.Count == 2 && lines[0].StartsWith(beginTag, StringComparison.Ordinal) && lines[0].EndsWith(endTag, StringComparison.Ordinal) &&
                    lines[1].StartsWith(beginTag, StringComparison.Ordinal))
                {
                    text = text.TrimEnd() + endTag;
                }
                else
                {
                    var lastIndex = text.LastIndexOf(beginTag, StringComparison.Ordinal);
                    if (text.Length > lastIndex + endTag.Length)
                    {
                        text = text.Substring(0, lastIndex) + text.Substring(lastIndex - 1 + endTag.Length);
                    }
                    else
                    {
                        text = text.Substring(0, lastIndex - 1) + endTag;
                    }
                }
                if (text.StartsWith(beginTag, StringComparison.Ordinal) && text.EndsWith(endTag, StringComparison.Ordinal) && text.Contains(endTag + Environment.NewLine + beginTag))
                {
                    text = text.Replace(endTag + Environment.NewLine + beginTag, Environment.NewLine);
                }
            }

            if (italicBeginTagCount == 1 && italicEndTagCount == 0)
            {
                var lines = text.SplitToLines();
                var sc = StringComparison.Ordinal;
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    var italicIndex = line.LastIndexOf(beginTag, StringComparison.Ordinal);

                    // no italic in current 'i' line, try next
                    if (italicIndex < 0)
                    {
                        continue;
                    }

                    // try earlier insert if possible e.g: <b><i>foobar</b> => <b><i>foobar</i></b>
                    lines[i] = IsTextFormattable(line.Substring(italicIndex + 3))
                        ? line.Insert(CalculateEarlyInsertIndex(line), endTag)
                        : line.Replace(beginTag, string.Empty);

                    break; // break as soon as we reach here since italicBeginTagCount == 1

                    int CalculateEarlyInsertIndex(string s)
                    {
                        var len = s.Length;
                        var lastClosingTagIndex = s.LastIndexOf("</", len - 1, len - italicIndex - 3, sc);
                        while (lastClosingTagIndex > italicIndex + 3)
                        {
                            var tempClosingIdx = s.LastIndexOf("</", lastClosingTagIndex, lastClosingTagIndex - italicIndex - 3, sc);
                            if (tempClosingIdx < 0) break;
                            lastClosingTagIndex = tempClosingIdx;
                        }
                        // try finding first closing tag index and insert the new closing there
                        // to avoid having text with closed tags like <b><i>foo</b></i>
                        return lastClosingTagIndex > italicIndex ? lastClosingTagIndex : len;
                    }
                }

                // reconstruct the text from lines
                text = string.Join(Environment.NewLine, lines);
            }

            if (italicBeginTagCount == 0 && italicEndTagCount == 1)
            {
                var cleanText = RemoveOpenCloseTags(text, TagItalic, TagBold, TagUnderline, TagCyrillicI);
                var isFixed = false;

                // Foo.</i>
                if (text.EndsWith(endTag, StringComparison.Ordinal) && !cleanText.StartsWith('-') && !cleanText.Contains(Environment.NewLine + "-"))
                {
                    text = beginTag + text;
                    isFixed = true;
                }

                // - Foo</i> | - Foo.
                // - Bar.    | - Foo.</i>
                if (!isFixed && Utilities.GetNumberOfLines(cleanText) == 2)
                {
                    var newLineIndex = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                    if (newLineIndex > 0)
                    {
                        var firstLine = text.Substring(0, newLineIndex).Trim();
                        var secondLine = text.Substring(newLineIndex + 2).Trim();
                        if (firstLine.EndsWith(endTag, StringComparison.Ordinal))
                        {
                            firstLine = beginTag + firstLine;
                            isFixed = true;
                        }
                        if (secondLine.EndsWith(endTag, StringComparison.Ordinal))
                        {
                            secondLine = beginTag + secondLine;
                            isFixed = true;
                        }
                        text = firstLine + Environment.NewLine + secondLine;
                    }
                }
                if (!isFixed)
                {
                    text = text.Replace(endTag, string.Empty);
                }
            }

            // - foo.</i>
            // - bar.</i>
            if (italicBeginTagCount == 0 && italicEndTagCount == 2 && text.Contains(endTag + Environment.NewLine, StringComparison.Ordinal) && text.EndsWith(endTag, StringComparison.Ordinal))
            {
                text = text.Replace(endTag, string.Empty);
                text = beginTag + text + endTag;
            }

            if (italicBeginTagCount == 0 && italicEndTagCount == 2)
            {
                var firstIndex = text.IndexOf(endTag, StringComparison.Ordinal);
                text = text.Remove(firstIndex, endTag.Length).Insert(firstIndex, beginTag);
            }

            // <i>Foo</i>
            // <i>Bar</i>
            if (italicBeginTagCount == 2 && italicEndTagCount == 2 && noOfLines == 2)
            {
                var index = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                if (index > 0 && text.Length > index + (beginTag.Length + endTag.Length))
                {
                    var firstLine = text.Substring(0, index).Trim();
                    var secondLine = text.Substring(index + Environment.NewLine.Length).Trim();

                    if (firstLine.Length > 10 && firstLine.StartsWith("- <i>", StringComparison.Ordinal) && firstLine.EndsWith(endTag, StringComparison.Ordinal))
                    {
                        text = "<i>- " + firstLine.Remove(0, 5) + Environment.NewLine + secondLine;
                        text = text.Replace("<i>-  ", "<i>- ");
                        index = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                        firstLine = text.Substring(0, index).Trim();
                        secondLine = text.Substring(index + Environment.NewLine.Length).Trim();
                    }
                    if (secondLine.Length > 10 && secondLine.StartsWith("- <i>", StringComparison.Ordinal) && secondLine.EndsWith(endTag, StringComparison.Ordinal))
                    {
                        text = firstLine + Environment.NewLine + "<i>- " + secondLine.Remove(0, 5);
                        text = text.Replace("<i>-  ", "<i>- ");
                        index = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                        firstLine = text.Substring(0, index).Trim();
                        secondLine = text.Substring(index + Environment.NewLine.Length).Trim();
                    }

                    if (Utilities.StartsAndEndsWithTag(firstLine, beginTag, endTag) && Utilities.StartsAndEndsWithTag(secondLine, beginTag, endTag))
                    {
                        text = text.Replace(beginTag, string.Empty).Replace(endTag, string.Empty).Trim();
                        text = beginTag + text + endTag;
                    }
                }

                //FALCONE:<i> I didn't think</i><br /><i>it was going to be you,</i>
                var colIdx = text.IndexOf(':');
                if (colIdx >= 0 && Utilities.CountTagInText(text, beginTag) + Utilities.CountTagInText(text, endTag) == 4 && text.Length > colIdx + 1 && !char.IsDigit(text[colIdx + 1]))
                {
                    var firstLine = text.Substring(0, index);
                    var secondLine = text.Substring(index).TrimStart();

                    var secIdxCol = secondLine.IndexOf(':');
                    if (secIdxCol < 0 || !Utilities.IsBetweenNumbers(secondLine, secIdxCol))
                    {
                        var idx = firstLine.IndexOf(':');
                        if (idx > 1)
                        {
                            var pre = text.Substring(0, idx + 1).TrimStart();
                            var tempText = text.Remove(0, idx + 1);

                            if (!tempText.StartsWith(']') &&
                                !tempText.StartsWith(')') &&
                                !tempText.StartsWith(Environment.NewLine) &&
                                !tempText.StartsWith("</i>" + Environment.NewLine))
                            {
                                text = tempText;
                                text = FixInvalidItalicTags(text).Trim();
                                if (text.StartsWith("<i> ", StringComparison.OrdinalIgnoreCase))
                                {
                                    text = Utilities.RemoveSpaceBeforeAfterTag(text, beginTag);
                                }

                                text = pre + " " + text;
                            }
                        }
                    }
                }
            }

            //<i>- You think they're they gone?<i>
            //<i>- That can't be.</i>
            if (italicBeginTagCount == 3 && italicEndTagCount == 1 && noOfLines == 2)
            {
                var newLineIdx = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                var firstLine = text.Substring(0, newLineIdx).Trim();
                var secondLine = text.Substring(newLineIdx).Trim();

                if ((Utilities.StartsAndEndsWithTag(firstLine, beginTag, beginTag) && Utilities.StartsAndEndsWithTag(secondLine, beginTag, endTag)) ||
                    (Utilities.StartsAndEndsWithTag(secondLine, beginTag, beginTag) && Utilities.StartsAndEndsWithTag(firstLine, beginTag, endTag)))
                {
                    text = text.Replace(beginTag, string.Empty);
                    text = text.Replace(endTag, string.Empty);
                    text = text.Replace("  ", " ").Trim();
                    text = beginTag + text + endTag;
                }
            }

            if (noOfLines == 2 && italicBeginTagCount == 0 && italicEndTagCount == 4)
            {
                var lines = text.SplitToLines();
                if (lines.Count == 2 &&
                    lines[0].StartsWith("</i>", StringComparison.InvariantCulture) && lines[1].EndsWith("</i>", StringComparison.InvariantCulture) &&
                    lines[1].StartsWith("</i>", StringComparison.InvariantCulture) && lines[1].EndsWith("</i>", StringComparison.InvariantCulture))
                {
                    var s1 = lines[0].Replace("</i>", string.Empty);
                    var s2 = lines[1].Replace("</i>", string.Empty);
                    text = "<i>" + s1.Trim() + "</i>" + Environment.NewLine + "<i>" + s2.Trim() + "</i>";
                }
            }

            if (noOfLines == 2 && italicBeginTagCount == 4 && italicEndTagCount == 0)
            {
                var lines = text.SplitToLines();
                if (lines.Count == 2 &&
                    lines[0].StartsWith("<i>", StringComparison.InvariantCulture) && lines[1].EndsWith("<i>", StringComparison.InvariantCulture) &&
                    lines[1].StartsWith("<i>", StringComparison.InvariantCulture) && lines[1].EndsWith("<i>", StringComparison.InvariantCulture))
                {
                    var s1 = lines[0].Replace("<i>", string.Empty);
                    var s2 = lines[1].Replace("<i>", string.Empty);
                    text = "<i>" + s1.Trim() + "</i>" + Environment.NewLine + "<i>" + s2.Trim() + "</i>";
                }
            }

            if (noOfLines == 3)
            {
                var lines = text.SplitToLines();
                if ((italicBeginTagCount == 3 && italicEndTagCount == 2) || (italicBeginTagCount == 2 && italicEndTagCount == 3))
                {
                    var numberOfItalics = 0;
                    foreach (var line in lines)
                    {
                        if (line.StartsWith(beginTag, StringComparison.Ordinal))
                        {
                            numberOfItalics++;
                        }

                        if (line.EndsWith(endTag, StringComparison.Ordinal))
                        {
                            numberOfItalics++;
                        }
                    }
                    if (numberOfItalics == 5)
                    { // fix missing tag
                        text = "<i>" + text.Replace("<i>", string.Empty).Replace("</i>", string.Empty) + "</i>";
                    }
                }
            }

            text = text.Replace("<i></i>", string.Empty);
            text = text.Replace("</i><i>", string.Empty);
            text = text.Replace($"<i>{Environment.NewLine}</i>", Environment.NewLine);
            if (text.IndexOf('@') < 0)
            {
                text = text.Replace("</i> <i>", "@");
                text = text.Replace("<i> </i>", "@");
                text = text.Replace("<i>  </i>", "@");
                text = text.Replace("@ ", " ");
                text = text.Replace("@ ", " ");
                text = text.Replace(" @", " ");
                text = text.Replace(" @", " ");
                text = text.Replace("@", " ");
            }
            else
            {
                text = text.Replace("</i> <i>", " ");
                text = text.Replace("<i> </i>", " ");
                text = text.Replace("<i>  </i>", " ");
                text = text.Replace("  ", " ");
                text = text.Replace("  ", " ");
            }

            return preTags + text;
        }

        /// <summary>
        /// Toggles the specified HTML or SSA/ASS tag on or off in the provided text.
        /// </summary>
        /// <param name="input">The input string to apply the tag toggle.</param>
        /// <param name="tag">The HTML or SSA/ASS tag to be toggled.</param>
        /// <param name="wholeLine">Specifies whether the whole line should be toggled or just part of it.</param>
        /// <param name="assa">Indicates whether the text contains SSA/ASS tags.</param>
        /// <returns>A new string with the specified tag toggled.</returns>
        public static string ToggleTag(string input, string tag, bool wholeLine, bool assa)
        {
            var text = input;

            if (assa)
            {
                var onOffTags = new List<string> { "i", "b", "u", "s", "be" };
                if (onOffTags.Contains(tag))
                {
                    if (text.Contains($"\\{tag}1"))
                    {
                        text = text.Replace($"{{\\{tag}1}}", string.Empty);
                        text = text.Replace($"{{\\{tag}0}}", string.Empty);
                        text = text.Replace($"\\{tag}1", string.Empty);
                        text = text.Replace($"\\{tag}0", string.Empty);
                    }
                    else
                    {
                        text = wholeLine ? $"{{\\{tag}1}}{text}" : $"{{\\{tag}1}}{text}{{\\{tag}0}}";
                    }
                }
                else
                {
                    if (text.Contains($"\\{tag}"))
                    {
                        text = text.Replace($"{{\\{tag}}}", string.Empty);
                        text = text.Replace($"\\{tag}", string.Empty);
                    }
                    else
                    {
                        text = $"{{\\{tag}}}{text}";
                    }
                }

                return text;
            }

            if (text.IndexOf("<" + tag + ">", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("</" + tag + ">", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                text = text.Replace("<" + tag + ">", string.Empty);
                text = text.Replace("</" + tag + ">", string.Empty);
                text = text.Replace("<" + tag.ToUpperInvariant() + ">", string.Empty);
                text = text.Replace("</" + tag.ToUpperInvariant() + ">", string.Empty);
            }
            else
            {
                int indexOfEndBracket = text.IndexOf('}');
                if (text.StartsWith("{\\", StringComparison.Ordinal) && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                {
                    text = $"{text.Substring(0, indexOfEndBracket + 1)}<{tag}>{text.Remove(0, indexOfEndBracket + 1)}</{tag}>";
                }
                else
                {
                    text = $"<{tag}>{text}</{tag}>";
                }
            }
            return text;
        }

        /// <summary>
        /// Determines if the specified tag is present in the input HTML or SSA/ASS string.
        /// </summary>
        /// <param name="input">The input string to search for the specified tag.</param>
        /// <param name="tag">The HTML or SSA/ASS tag to check for.</param>
        /// <param name="wholeLine">Specifies whether the search should consider the whole line.</param>
        /// <param name="assa">Indicates if the input string is in SSA/ASS format.</param>
        /// <returns>True if the tag is present in the input; otherwise, false.</returns>
        public static bool IsTagOn(string input, string tag, bool wholeLine, bool assa)
        {
            var text = input;

            if (assa)
            {
                var onOffTags = new List<string> { "i", "b", "u", "s", "be" };
                if (onOffTags.Contains(tag))
                {
                    return text.Contains($"\\{tag}1");
                }

                return text.Contains($"\\{tag}");
            }

            return text.IndexOf("<" + tag + ">", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("</" + tag + ">", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Applies the specified HTML or ASSA tag to the input string.
        /// </summary>
        /// <param name="input">The input string to which the tag will be applied.</param>
        /// <param name="tag">The tag to apply to the input string.</param>
        /// <param name="wholeLine">If true, the tag is applied to the entire line; otherwise, the tag is applied to a portion of the line.</param>
        /// <param name="assa">If true, the tag is treated as an ASSA tag; otherwise, it is treated as an HTML tag.</param>
        /// <returns>A new string with the specified tag applied.</returns>
        public static string TagOn(string input, string tag, bool wholeLine, bool assa)
        {
            var text = input;

            if (assa)
            {
                var onOffTags = new List<string> { "i", "b", "u", "s", "be" };
                if (onOffTags.Contains(tag))
                {
                    text = text.Replace($"{{\\{tag}1}}", string.Empty);
                    text = text.Replace($"{{\\{tag}0}}", string.Empty);
                    text = text.Replace($"\\{tag}1", string.Empty);
                    text = text.Replace($"\\{tag}0", string.Empty);

                    text = wholeLine ? $"{{\\{tag}1}}{text}" : $"{{\\{tag}1}}{text}{{\\{tag}0}}";
                }
                else
                {
                    if (text.Contains($"\\{tag}"))
                    {
                        text = text.Replace($"{{\\{tag}}}", string.Empty);
                        text = text.Replace($"\\{tag}", string.Empty);
                    }
                    text = $"{{\\{tag}}}{text}";
                }

                return text;
            }

            text = text.Replace("<" + tag + ">", string.Empty);
            text = text.Replace("</" + tag + ">", string.Empty);
            text = text.Replace("<" + tag.ToUpperInvariant() + ">", string.Empty);
            text = text.Replace("</" + tag.ToUpperInvariant() + ">", string.Empty);
            var indexOfEndBracket = text.IndexOf('}');
            if (text.StartsWith("{\\", StringComparison.Ordinal) && indexOfEndBracket > 1 && indexOfEndBracket < 6)
            {
                text = $"{text.Substring(0, indexOfEndBracket + 1)}<{tag}>{text.Remove(0, indexOfEndBracket + 1)}</{tag}>";
            }
            else
            {
                text = $"<{tag}>{text}</{tag}>";
            }

            return text;
        }

        /// <summary>
        /// Remove the specified HTML tag from the input string.
        /// </summary>
        /// <param name="input">The input string containing HTML tags.</param>
        /// <param name="tag">The HTML tag to be removed.</param>
        /// <param name="wholeLine">Indicates whether the operation applies to the whole line.</param>
        /// <param name="assa">Indicates whether ASSA (Advanced SubStation Alpha) tags are used.</param>
        /// <returns>A new string with the specified tag removed.</returns>
        public static string TagOff(string input, string tag, bool wholeLine, bool assa)
        {
            var text = input;

            if (assa)
            {
                var onOffTags = new List<string> { "i", "b", "u", "s", "be" };
                if (onOffTags.Contains(tag))
                {
                    if (text.Contains($"\\{tag}1"))
                    {
                        text = text.Replace($"{{\\{tag}1}}", string.Empty);
                        text = text.Replace($"{{\\{tag}0}}", string.Empty);
                        text = text.Replace($"\\{tag}1", string.Empty);
                        text = text.Replace($"\\{tag}0", string.Empty);
                    }
                }
                else
                {
                    if (text.Contains($"\\{tag}"))
                    {
                        text = text.Replace($"{{\\{tag}}}", string.Empty);
                        text = text.Replace($"\\{tag}", string.Empty);
                    }
                }

                return text;
            }

            if (text.IndexOf("<" + tag + ">", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("</" + tag + ">", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                text = text.Replace("<" + tag + ">", string.Empty);
                text = text.Replace("</" + tag + ">", string.Empty);
                text = text.Replace("<" + tag.ToUpperInvariant() + ">", string.Empty);
                text = text.Replace("</" + tag.ToUpperInvariant() + ">", string.Empty);
            }
            else
            {
                var indexOfEndBracket = text.IndexOf('}');
                if (text.StartsWith("{\\", StringComparison.Ordinal) && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                {
                    text = $"{text.Substring(0, indexOfEndBracket + 1)}<{tag}>{text.Remove(0, indexOfEndBracket + 1)}</{tag}>";
                }
            }

            return text;
        }

        /// <summary>
        /// Converts a string representation of a color to a Color object. The string can be in various formats such as
        /// "rgb(r, g, b)", "rgba(r, g, b, a)", or a hex color string like "#RRGGBB" or "#RRGGBBAA".
        /// </summary>
        /// <param name="s">The string representation of the color.</param>
        /// <returns>A Color object corresponding to the input string. If the string cannot be parsed, the default color is white.</returns
        public static SKColor GetColorFromString(string s)
        {
            try
            {
                if (s.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
                {
                    var arr = s
                        .RemoveChar(' ')
                        .Remove(0, 4)
                        .TrimEnd(')')
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    return new SKColor(byte.Parse(arr[0]), byte.Parse(arr[1]), byte.Parse(arr[2]));
                }

                if (s.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase))
                {
                    var arr = s
                        .RemoveChar(' ')
                        .Remove(0, 5)
                        .TrimEnd(')')
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    var alpha = byte.MaxValue;
                    if (arr.Length == 4 && float.TryParse(arr[3], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var f))
                    {
                        if (f >= 0 && f < 1)
                        {
                            alpha = (byte)(f * byte.MaxValue);
                        }
                    }

                    return new SKColor(byte.Parse(arr[0]), byte.Parse(arr[1]), byte.Parse(arr[2]), alpha);
                }

                if (s.Length == 9 && s.StartsWith("#"))
                {
                    if (!int.TryParse(s.Substring(7, 2), NumberStyles.HexNumber, null, out var alpha))
                    {
                        alpha = 255; // full solid color
                    }

                    s = s.Substring(1, 6);
                    var c = ColorTranslator.FromHtml("#" + s);
                    return new SKColor(c.Red, c.Green, c.Blue, (byte)alpha);
                }

                return ColorTranslator.FromHtml(s);
            }
            catch
            {
                return SKColors.White;
            }
        }

        /// <summary>
        /// Remove color tags from the input string, adjusting for potentially surrounding font tags.
        /// </summary>
        /// <param name="input">The string from which to remove color tags.</param>
        /// <returns>A new string with color tags removed.</returns>
        private static readonly Regex ColorAttributeRegex = new Regex("[ ]*(COLOR|color|Color)=[\"']*[#\\dA-Za-z]*[\"']*[ ]*", RegexOptions.Compiled);

        public static string RemoveColorTags(string input)
        {
            var r = ColorAttributeRegex;
            var s = input;
            var match = r.Match(s);
            while (match.Success)
            {
                s = s.Remove(match.Index, match.Value.Length).Insert(match.Index, " ");
                if (match.Index > 4)
                {
                    if (string.Compare(s, match.Index - 5, "<font >", 0, 7, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        s = s.Remove(match.Index - 5, 7);
                        var endIndex = s.IndexOf("</font>", match.Index - 5, StringComparison.OrdinalIgnoreCase);
                        if (endIndex >= 0)
                        {
                            s = s.Remove(endIndex, 7);
                        }
                        else
                        {
                            endIndex = s.IndexOf("< /font>", match.Index - 5, StringComparison.OrdinalIgnoreCase);
                            if (endIndex >= 0)
                            {
                                s = s.Remove(endIndex, 7);
                            }
                            else
                            {
                                endIndex = s.IndexOf("</ font>", match.Index - 5, StringComparison.OrdinalIgnoreCase);
                                if (endIndex >= 0)
                                {
                                    s = s.Remove(endIndex, 7);
                                }
                            }
                        }
                    }
                    else if (s.Length > match.Index + 1 && s[match.Index + 1] == '>')
                    {
                        s = s.Remove(match.Index, 1);
                    }
                }

                match = r.Match(s);
            }

            return s.Trim();
        }

        private static readonly Regex FontFaceAttributeRegex = new Regex("[ ]*(FACE|face|Face)=[\"']*[\\d\\p{L} ]*[\"']*[ ]*", RegexOptions.Compiled);
        private static readonly Regex AssaFontNameOnlyTagRegex = new Regex("{\\\\fn[a-zA-Z \\d]+}", RegexOptions.Compiled);
        private static readonly Regex AssaFontNameLastTagRegex = new Regex("\\\\fn[a-zA-Z \\d]+}", RegexOptions.Compiled);
        private static readonly Regex AssaFontNameInnerTagRegex = new Regex("\\\\fn[a-zA-Z \\d]+\\\\", RegexOptions.Compiled);

        /// <summary>
        /// Remove font tag from HTML or ASSA.
        /// </summary>
        public static string RemoveFontName(string input)
        {
            if (!input.Contains("<font", StringComparison.OrdinalIgnoreCase))
            {
                var x = input;
                if (x.Contains("\\fn"))
                {
                    x = AssaFontNameOnlyTagRegex.Replace(x, string.Empty);
                    x = AssaFontNameLastTagRegex.Replace(x, "}");
                    x = AssaFontNameInnerTagRegex.Replace(x, "\\");
                }

                return x;
            }

            var r = FontFaceAttributeRegex;
            var s = input;
            var match = r.Match(s);
            while (match.Success)
            {
                s = s.Remove(match.Index, match.Value.Length).Insert(match.Index, " ");
                if (match.Index > 4)
                {
                    if (string.Compare(s, match.Index - 5, "<font >", 0, 7, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        s = s.Remove(match.Index - 5, 7);
                        var endIndex = s.IndexOf("</font>", match.Index - 5, StringComparison.OrdinalIgnoreCase);
                        if (endIndex >= 0)
                        {
                            s = s.Remove(endIndex, 7);
                        }
                    }
                    else if (s.Length > match.Index + 1 && s[match.Index + 1] == '>')
                    {
                        s = s.Remove(match.Index, 1);
                    }
                }

                match = r.Match(s);
            }

            return s;
        }

        /// <summary>
        /// Removes ASS and SSA alignment tags from the given string.
        /// </summary>
        /// <param name="s">The input string from which to remove the alignment tags.</param>
        /// <returns>A new string without ASS and SSA alignment tags.</returns>
        public static string RemoveAssAlignmentTags(string s)
        {
            return s.Replace("{\\an1}", string.Empty) // ASS tags alone
                .Replace("{\\an2}", string.Empty)
                .Replace("{\\an3}", string.Empty)
                .Replace("{\\an4}", string.Empty)
                .Replace("{\\an5}", string.Empty)
                .Replace("{\\an6}", string.Empty)
                .Replace("{\\an7}", string.Empty)
                .Replace("{\\an8}", string.Empty)
                .Replace("{\\an9}", string.Empty)

                .Replace("{an1\\", "{") // ASS multi tags (start)
                .Replace("{an2\\", "{")
                .Replace("{an3\\", "{")
                .Replace("{an4\\", "{")
                .Replace("{an5\\", "{")
                .Replace("{an6\\", "{")
                .Replace("{an7\\", "{")
                .Replace("{an8\\", "{")
                .Replace("{an9\\", "{")

                .Replace("\\an1\\", "\\") // ASS multi tags (middle)
                .Replace("\\an2\\", "\\")
                .Replace("\\an3\\", "\\")
                .Replace("\\an4\\", "\\")
                .Replace("\\an5\\", "\\")
                .Replace("\\an6\\", "\\")
                .Replace("\\an7\\", "\\")
                .Replace("\\an8\\", "\\")
                .Replace("\\an9\\", "\\")

                .Replace("\\an1}", "}") // ASS multi tags (end)
                .Replace("\\an2}", "}")
                .Replace("\\an3}", "}")
                .Replace("\\an4}", "}")
                .Replace("\\an5}", "}")
                .Replace("\\an6}", "}")
                .Replace("\\an7}", "}")
                .Replace("\\an8}", "}")
                .Replace("\\an9}", "}")

                .Replace("{\\a1}", string.Empty) // SSA tags
                .Replace("{\\a2}", string.Empty)
                .Replace("{\\a3}", string.Empty)
                .Replace("{\\a4}", string.Empty)
                .Replace("{\\a5}", string.Empty)
                .Replace("{\\a6}", string.Empty)
                .Replace("{\\a7}", string.Empty)
                .Replace("{\\a8}", string.Empty)
                .Replace("{\\a9}", string.Empty);
        }

        /// <summary>
        /// Remove color tags specific to Advanced SubStation Alpha (ASSA) format from the input string.
        /// </summary>
        /// <param name="input">The input string potentially containing ASSA color tags.</param>
        /// <returns>A new string with all ASSA color tags removed.</returns>
        public static string RemoveAssaColor(string input)
        {
            var text = input;
            text = Regex.Replace(text, "\\\\alpha&H.{1,2}&\\\\", "\\");
            text = Regex.Replace(text, "{\\\\1c&[abcdefghABCDEFGH\\d]*&}", string.Empty);
            text = Regex.Replace(text, "{\\\\c&[abcdefghABCDEFGH\\d]*&}", string.Empty);
            text = Regex.Replace(text, "\\\\c&[abcdefghABCDEFGH\\d]*&", string.Empty);
            text = Regex.Replace(text, "\\\\1c&[abcdefghABCDEFGH\\d]*&", string.Empty);
            return text;
        }

        /// <summary>
        /// Gets the closing HTML tag for the specified opening tag.
        /// </summary>
        /// <param name="tag">The opening HTML tag to find the closing pair for.</param>
        /// <returns>The closing HTML tag corresponding to the specified opening tag.</returns>
        public static string GetClosingPair(string tag)
        {
            switch (tag)
            {
                case "<i>": return "</i>";
                case "<b>": return "</b>";
                case "<u>": return "</u>";
            }
            return tag.StartsWith("<font ", StringComparison.Ordinal) ? "</font>" : string.Empty;
        }

        /// <summary>
        /// Get the corresponding closing character for a given opening character.
        /// </summary>
        /// <param name="ch">The opening character to find the closing pair for.</param>
        /// <returns>The corresponding closing character.</returns>
        public static char GetClosingPair(char ch) => ch == '<' ? '>' : '}';

        /// <summary>
        /// Determines if the provided HTML tag is an opening tag.
        /// </summary>
        /// <param name="tag">The HTML tag to check.</param>
        /// <returns>True if the tag is an opening tag, otherwise false.</returns
        public static bool IsOpenTag(string tag) => tag.Length > 1 && tag[1] != '/';

        /// <summary>
        /// Determines whether the specified character is a start tag symbol for HTML or similar markup.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>True if the character is a start tag symbol; otherwise, false.</returns>
        public static bool IsStartTagSymbol(char ch) => ch == '<' || ch == '{';
    }
}
