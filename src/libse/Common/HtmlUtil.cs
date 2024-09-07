﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// HTML specific string manipulations.
    /// </summary>
    public static class HtmlUtil
    {
        public static string TagItalic => "i";
        public static string TagBold => "b";
        public static string TagUnderline => "u";
        public static string TagFont => "font";
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
            foreach (var ch in source)
            {
                if (ch == ' ')
                {
                    encoded.Append("&#");
                    encoded.Append(160); // &nbsp;
                    encoded.Append(';');
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

            if (s.Contains("< "))
            {
                s = FixInvalidItalicTags(s);
            }

            if (s.IndexOf('x') > 0)
            {
                s = s.Replace("<box>", string.Empty).Replace("</box>", string.Empty);
            }

            // v tag from WebVTT
            var indexOfVTag = s.IndexOf("<v ", StringComparison.Ordinal);
            if (indexOfVTag < 0)
            {
                indexOfVTag = s.IndexOf("<v.", StringComparison.Ordinal);
            }
            if (indexOfVTag >= 0)
            {
                var indexOfEndVTag = s.IndexOf('>', indexOfVTag);
                if (indexOfEndVTag >= 0)
                {
                    s = s.Remove(indexOfVTag, indexOfEndVTag - indexOfVTag + 1);
                    s = s.Replace("</v>", string.Empty);
                }
            }

            // v tag from WebVTT
            var indexOfCTag = s.IndexOf("<c.", StringComparison.Ordinal);
            if (indexOfCTag >= 0)
            {
                var indexOfEndVTag = s.IndexOf('>', indexOfCTag);
                if (indexOfEndVTag >= 0)
                {
                    s = s.Remove(indexOfCTag, indexOfEndVTag - indexOfCTag + 1);
                    s = s.Replace("</c>", string.Empty);
                }
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
            char[] array = new char[s.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if (ch == '<' && i < s.Length - 2)
                {
                    var next = s[i + 1];
                    var nextNext = s[i + 2];
                    if (nextNext == '>' &&
                        (next == 'i' || // <i>
                         next == 'I' || // <I>
                         next == 'b' || // <b>
                         next == 'B' || // <B>
                         next == 'u' || // <u>
                         next == 'U'))  // <U>
                    {
                        inside = true;
                        continue;
                    }

                    if (next == '/' && i < s.Length - 3)
                    {
                        var nextNextNext = s[i + 3];
                        if (nextNextNext == '>' &&
                            (nextNext == 'i' || // </i>
                             nextNext == 'I' || // </I>
                             nextNext == 'b' || // </b>
                             nextNext == 'B' || // </B>
                             nextNext == 'u' || // </u>
                             nextNext == 'U'))  // </U>
                        {
                            inside = true;
                            continue;
                        }

                        if (nextNext == 'c' && nextNextNext == '.')
                        {
                            inside = true;
                            continue;
                        }
                    }

                    if (nextNext == '/' && i < s.Length - 3)
                    { // some bad end tags sometimes seen
                        var nextNextNext = s[i + 3];
                        if (nextNextNext == '>' &&
                            (next == 'i' || // <i/>
                             next == 'I' || // <I/>
                             next == 'b' || // <b/>
                             next == 'B' || // <B/>
                             next == 'u' || // <u/>
                             next == 'U'))  // <U/>
                        {
                            inside = true;
                            continue;
                        }
                    }

                    if ((next == 'f' || next == 'F') && s.Substring(i).StartsWith("<font", StringComparison.OrdinalIgnoreCase) || // <font
                        next == '/' && (nextNext == 'f' || nextNext == 'F') && s.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase) ||  // </font>                        
                        next == ' ' && nextNext == '/' && s.Substring(i).StartsWith("< /font>", StringComparison.OrdinalIgnoreCase) ||  // < /font>
                        next == '/' && nextNext == ' ' && s.Substring(i).StartsWith("</ font>", StringComparison.OrdinalIgnoreCase))  // </ font>
                    {
                        inside = true;
                        continue;
                    }

                    if (next == 'c' && nextNext == '.')
                    {
                        inside = true;
                        continue;
                    }
                }
                if (inside && ch == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = ch;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        public static bool IsUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < 6 || text.IndexOf('.') < 0 || text.IndexOf(' ') >= 0)
            {
                return false;
            }

            var allLower = text.ToLowerInvariant();
            if (allLower.StartsWith("http://", StringComparison.Ordinal) || allLower.StartsWith("https://", StringComparison.Ordinal) ||
                allLower.StartsWith("www.", StringComparison.Ordinal) || allLower.EndsWith(".org", StringComparison.Ordinal) ||
                allLower.EndsWith(".com", StringComparison.Ordinal) || allLower.EndsWith(".net", StringComparison.Ordinal))
            {
                return true;
            }

            if (allLower.Contains(".org/") || allLower.Contains(".com/") || allLower.Contains(".net/"))
            {
                return true;
            }

            return false;
        }

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

        public static string FixUpperTags(string input)
        {
            if (string.IsNullOrEmpty(input) || input.IndexOf('<') < 0)
            {
                return input;
            }

            var text = input;
            var idx = text.IndexOfAny(UppercaseTags, StringComparison.Ordinal);
            while (idx >= 0)
            {
                var endIdx = text.IndexOf('>', idx + 2);
                if (endIdx < idx)
                {
                    break;
                }

                var tag = text.Substring(idx, endIdx - idx).ToLowerInvariant();
                text = text.Remove(idx, endIdx - idx).Insert(idx, tag);
                idx = text.IndexOfAny(UppercaseTags, StringComparison.Ordinal);
            }
            return text;
        }

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
                            
                            if (!tempText.StartsWith(']') && !tempText.StartsWith(')'))
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

        public static Color GetColorFromString(string s)
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

                    return Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
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

                    return Color.FromArgb(alpha, int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                }

                if (s.Length == 9 && s.StartsWith("#"))
                {
                    if (!int.TryParse(s.Substring(7, 2), NumberStyles.HexNumber, null, out var alpha))
                    {
                        alpha = 255; // full solid color
                    }

                    s = s.Substring(1, 6);
                    var c = ColorTranslator.FromHtml("#" + s);
                    return Color.FromArgb(alpha, c);
                }

                return ColorTranslator.FromHtml(s);
            }
            catch
            {
                return Color.White;
            }
        }

        public static string RemoveColorTags(string input)
        {
            var r = new Regex("[ ]*(COLOR|color|Color)=[\"']*[#\\dA-Za-z]*[\"']*[ ]*");
            var s = input;
            var match = r.Match(s);
            while (match.Success)
            {
                s = s.Remove(match.Index, match.Value.Length).Insert(match.Index, " ");
                if (match.Index > 4)
                {
                    var font = s.Substring(match.Index - 5);
                    if (font.StartsWith("<font >", StringComparison.OrdinalIgnoreCase))
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
                    x = Regex.Replace(x, "{\\\\fn[a-zA-Z \\d]+}", string.Empty);
                    x = Regex.Replace(x, "\\\\fn[a-zA-Z \\d]+}", "}");
                    x = Regex.Replace(x, "\\\\fn[a-zA-Z \\d]+\\\\", "\\");
                }

                return x;
            }

            var r = new Regex("[ ]*(FACE|face|Face)=[\"']*[\\d\\p{L} ]*[\"']*[ ]*");
            var s = input;
            var match = r.Match(s);
            while (match.Success)
            {
                s = s.Remove(match.Index, match.Value.Length).Insert(match.Index, " ");
                if (match.Index > 4)
                {
                    var font = s.Substring(match.Index - 5);
                    if (font.StartsWith("<font >", StringComparison.OrdinalIgnoreCase))
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
        
        public static string GetClosingPair(string tag)
        {
            switch (tag)
            {
                case "<i>" : return "</i>";
                case "<b>" : return "</b>";
                case "<u>" : return "</u>";
            }
            return tag.StartsWith("<font ", StringComparison.Ordinal) ? "</font>" : string.Empty;
        }
        
        public static char GetClosingPair(char ch) => ch == '<' ? '>' : '}';

        public static bool IsOpenTag(string tag) => tag.Length > 1 && tag[1] != '/';

        public static bool IsStartTagSymbol(char ch) => ch == '<' || ch == '{';
    }
}
