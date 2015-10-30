﻿using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core
{
    /// <summary>
    /// HTML specific string manipulations.
    /// </summary>
    public static class HtmlUtil
    {
        public const string TagItalic = "i";
        public const string TagBold = "b";
        public const string TagUnderline = "u";
        public const string TagParagraph = "p";
        public const string TagFont = "font";
        public const string TagCyrillicI = "\u0456"; // Cyrillic Small Letter Byelorussian-Ukrainian i (http://graphemica.com/%D1%96)

        private static readonly Regex TagOpenRegex = new Regex(@"<\s*(?:/\s*)?(\w+)[^>]*>", RegexOptions.Compiled);

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
            return TagOpenRegex.Replace(
                source,
                m => tags.Contains(m.Groups[1].Value, StringComparer.OrdinalIgnoreCase) ? string.Empty : m.Value);
        }

        /// <summary>
        /// Converts a string to an HTML-encoded string using named character references.
        /// </summary>
        /// <param name="source">The string to encode.</param>
        /// <returns>An encoded string.</returns>
        public static string EncodeNamed(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

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
                            encoded.Append("&#" + (int)ch + ";");
                        else
                            encoded.Append(ch);
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
                return string.Empty;

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

        public static string RemoveHtmlTags(string s, bool alsoSsaTags = false)
        {
            if (s == null || s.Length < 3)
                return s;

            if (alsoSsaTags)
                s = Utilities.RemoveSsaTags(s);

            if (!s.Contains('<'))
                return s;

            if (s.Contains("< "))
                s = FixInvalidItalicTags(s);

            return RemoveOpenCloseTags(s, TagItalic, TagBold, TagUnderline, TagParagraph, TagFont, TagCyrillicI);
        }

        public static bool IsUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < 6 || !text.Contains('.') || text.Contains(' '))
                return false;

            var allLower = text.ToLower();
            if (allLower.StartsWith("http://", StringComparison.Ordinal) || allLower.StartsWith("https://", StringComparison.Ordinal) ||
                allLower.StartsWith("www.", StringComparison.Ordinal) || allLower.EndsWith(".org", StringComparison.Ordinal) ||
                allLower.EndsWith(".com", StringComparison.Ordinal) || allLower.EndsWith(".net", StringComparison.Ordinal))
                return true;

            if (allLower.Contains(".org/") || allLower.Contains(".com/") || allLower.Contains(".net/"))
                return true;

            return false;
        }

        public static bool StartsWithUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var arr = text.Trim().TrimEnd('.').TrimEnd().Split();
            if (arr.Length == 0)
                return false;

            return IsUrl(arr[0]);
        }

        private static readonly string[] UppercaseTags = { "<I>", "<U>", "<B>", "<FONT", "</I>", "</U>", "</B>", "</FONT>" };

        public static string FixUpperTags(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            var idx = text.IndexOfAny(UppercaseTags, StringComparison.Ordinal);
            while (idx >= 0)
            {
                var endIdx = text.IndexOf('>', idx + 2);
                if (endIdx < idx)
                    break;
                var tag = text.Substring(idx, endIdx - idx).ToLowerInvariant();
                text = text.Remove(idx, endIdx - idx).Insert(idx, tag);
                idx = text.IndexOfAny(UppercaseTags, StringComparison.Ordinal);
            }
            return text;
        }

        public static string FixInvalidItalicTags(string text)
        {
            const string beginTag = "<i>";
            const string endTag = "</i>";

            text = text.Replace("< i >", beginTag);
            text = text.Replace("< i>", beginTag);
            text = text.Replace("<i >", beginTag);
            text = text.Replace("< I>", beginTag);
            text = text.Replace("<I >", beginTag);

            text = text.Replace("< / i >", endTag);
            text = text.Replace("< /i>", endTag);
            text = text.Replace("</ i>", endTag);
            text = text.Replace("< /i>", endTag);
            text = text.Replace("< /i >", endTag);
            text = text.Replace("</i >", endTag);
            text = text.Replace("</ i >", endTag);
            text = text.Replace("< / i>", endTag);
            text = text.Replace("< /I>", endTag);
            text = text.Replace("</ I>", endTag);
            text = text.Replace("< /I>", endTag);
            text = text.Replace("< / I >", endTag);

            text = text.Replace("</i> <i>", "_@_");
            text = text.Replace(" _@_", "_@_");
            text = text.Replace(" _@_ ", "_@_");
            text = text.Replace("_@_", " ");

            if (text.Contains(beginTag))
                text = text.Replace("<i/>", endTag);
            else
                text = text.Replace("<i/>", string.Empty);

            text = text.Replace(beginTag + beginTag, beginTag);
            text = text.Replace(endTag + endTag, endTag);

            int italicBeginTagCount = Utilities.CountTagInText(text, beginTag);
            int italicEndTagCount = Utilities.CountTagInText(text, endTag);
            int noOfLines = Utilities.GetNumberOfLines(text);
            if (italicBeginTagCount + italicEndTagCount > 0)
            {
                if (italicBeginTagCount == 1 && italicEndTagCount == 1 && text.IndexOf(beginTag, StringComparison.Ordinal) > text.IndexOf(endTag, StringComparison.Ordinal))
                {
                    text = text.Replace(beginTag, "___________@");
                    text = text.Replace(endTag, beginTag);
                    text = text.Replace("___________@", endTag);
                }

                if (italicBeginTagCount == 2 && italicEndTagCount == 0)
                {
                    int firstIndex = text.IndexOf(beginTag, StringComparison.Ordinal);
                    int lastIndex = text.LastIndexOf(beginTag, StringComparison.Ordinal);
                    int lastIndexWithNewLine = text.LastIndexOf(Environment.NewLine + beginTag, StringComparison.Ordinal) + Environment.NewLine.Length;
                    if (noOfLines == 2 && lastIndex == lastIndexWithNewLine && firstIndex < 2)
                        text = text.Replace(Environment.NewLine, endTag + Environment.NewLine) + endTag;
                    else if (text.Length > lastIndex + endTag.Length)
                        text = text.Substring(0, lastIndex) + endTag + text.Substring(lastIndex - 1 + endTag.Length);
                    else
                        text = text.Substring(0, lastIndex) + endTag;
                }

                if (italicBeginTagCount == 1 && italicEndTagCount == 2)
                {
                    int firstIndex = text.IndexOf(endTag, StringComparison.Ordinal);
                    if (text.StartsWith("</i>-<i>-", StringComparison.Ordinal))
                        text = text.Remove(0, 5);
                    else if (text.StartsWith("</i>- <i>-", StringComparison.Ordinal))
                        text = text.Remove(0, 5);
                    else if (text.StartsWith("</i>- <i> -", StringComparison.Ordinal))
                        text = text.Remove(0, 5);
                    else if (text.StartsWith("</i>-<i> -", StringComparison.Ordinal))
                        text = text.Remove(0, 5);
                    else if (firstIndex == 0)
                        text = text.Remove(0, 4);
                    else
                        text = text.Substring(0, firstIndex) + text.Substring(firstIndex + endTag.Length);
                }

                if (italicBeginTagCount == 2 && italicEndTagCount == 1)
                {
                    var lines = text.SplitToLines();
                    if (lines.Length == 2 && lines[0].StartsWith(beginTag, StringComparison.Ordinal) && lines[0].EndsWith(endTag, StringComparison.Ordinal) &&
                        lines[1].StartsWith(beginTag, StringComparison.Ordinal))
                    {
                        text = text.TrimEnd() + endTag;
                    }
                    else
                    {
                        int lastIndex = text.LastIndexOf(beginTag, StringComparison.Ordinal);
                        if (text.Length > lastIndex + endTag.Length)
                            text = text.Substring(0, lastIndex) + text.Substring(lastIndex - 1 + endTag.Length);
                        else
                            text = text.Substring(0, lastIndex - 1) + endTag;
                    }
                    if (text.StartsWith(beginTag, StringComparison.Ordinal) && text.EndsWith(endTag, StringComparison.Ordinal) && text.Contains(endTag + Environment.NewLine + beginTag))
                    {
                        text = text.Replace(endTag + Environment.NewLine + beginTag, Environment.NewLine);
                    }
                }

                if (italicBeginTagCount == 1 && italicEndTagCount == 0)
                {
                    int lastIndexWithNewLine = text.LastIndexOf(Environment.NewLine + beginTag, StringComparison.Ordinal) + Environment.NewLine.Length;
                    int lastIndex = text.LastIndexOf(beginTag, StringComparison.Ordinal);

                    if (text.StartsWith(beginTag, StringComparison.Ordinal))
                        text += endTag;
                    else if (noOfLines == 2 && lastIndex == lastIndexWithNewLine)
                        text += endTag;
                    else
                        text = text.Replace(beginTag, string.Empty);
                }

                if (italicBeginTagCount == 0 && italicEndTagCount == 1)
                {
                    var cleanText = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagItalic, HtmlUtil.TagBold, HtmlUtil.TagUnderline, HtmlUtil.TagCyrillicI);
                    bool isFixed = false;

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
                        int newLineIndex = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
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
                        text = text.Replace(endTag, string.Empty);
                }

                // - foo.</i>
                // - bar.</i>
                if (italicBeginTagCount == 0 && italicEndTagCount == 2 && text.Contains(endTag + Environment.NewLine, StringComparison.Ordinal) && text.EndsWith(endTag, StringComparison.Ordinal))
                {
                    text = text.Replace(endTag, string.Empty);
                    text = beginTag + text + endTag;
                }

                if (italicBeginTagCount == 0 && italicEndTagCount == 2 && text.StartsWith(endTag, StringComparison.Ordinal) && text.EndsWith(endTag, StringComparison.Ordinal))
                {
                    int firstIndex = text.IndexOf(endTag, StringComparison.Ordinal);
                    text = text.Remove(firstIndex, endTag.Length).Insert(firstIndex, beginTag);
                }

                // <i>Foo</i>
                // <i>Bar</i>
                if (italicBeginTagCount == 2 && italicEndTagCount == 2 && Utilities.GetNumberOfLines(text) == 2)
                {
                    int index = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                    if (index > 0 && text.Length > index + (beginTag.Length + endTag.Length))
                    {
                        var firstLine = text.Substring(0, index).Trim();
                        var secondLine = text.Substring(index + 2).Trim();

                        if (firstLine.Length > 10 && firstLine.StartsWith("- <i>", StringComparison.Ordinal) && firstLine.EndsWith(endTag, StringComparison.Ordinal))
                        {
                            text = "<i>- " + firstLine.Remove(0, 5) + Environment.NewLine + secondLine;
                            text = text.Replace("<i>-  ", "<i>- ");
                            index = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                            firstLine = text.Substring(0, index).Trim();
                            secondLine = text.Substring(index + 2).Trim();
                        }
                        if (secondLine.Length > 10 && secondLine.StartsWith("- <i>", StringComparison.Ordinal) && secondLine.EndsWith(endTag, StringComparison.Ordinal))
                        {
                            text = firstLine + Environment.NewLine + "<i>- " + secondLine.Remove(0, 5);
                            text = text.Replace("<i>-  ", "<i>- ");
                            index = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                            firstLine = text.Substring(0, index).Trim();
                            secondLine = text.Substring(index + 2).Trim();
                        }

                        if (Utilities.StartsAndEndsWithTag(firstLine, beginTag, endTag) && Utilities.StartsAndEndsWithTag(secondLine, beginTag, endTag))
                        {
                            text = text.Replace(beginTag, String.Empty).Replace(endTag, String.Empty).Trim();
                            text = beginTag + text + endTag;
                        }
                    }

                    //FALCONE:<i> I didn't think</i><br /><i>it was going to be you,</i>
                    var colIdx = text.IndexOf(':');
                    if (colIdx > -1 && Utilities.CountTagInText(text, beginTag) + Utilities.CountTagInText(text, endTag) == 4 && text.Length > colIdx + 1 && !char.IsDigit(text[colIdx + 1]))
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
                                text = text.Remove(0, idx + 1);
                                text = FixInvalidItalicTags(text).Trim();
                                if (text.StartsWith("<i> ", StringComparison.OrdinalIgnoreCase))
                                    text = Utilities.RemoveSpaceBeforeAfterTag(text, beginTag);
                                text = pre + " " + text;
                            }
                        }
                    }
                }

                //<i>- You think they're they gone?<i>
                //<i>- That can't be.</i>
                if ((italicBeginTagCount == 3 && italicEndTagCount == 1) && Utilities.GetNumberOfLines(text) == 2)
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
                text = text.Replace("<i></i>", string.Empty);
                text = text.Replace("<i> </i>", string.Empty);
                text = text.Replace("<i>  </i>", string.Empty);
            }
            return text;
        }

        public static string ToogleTag(string text, string tag)
        {
            if (text.Contains("<" + tag + ">"))
            {
                text = text.Replace("<" + tag + ">", string.Empty);
                text = text.Replace("</" + tag + ">", string.Empty);
            }
            else
            {
                int indexOfEndBracket = text.IndexOf('}');
                if (text.StartsWith("{\\", StringComparison.Ordinal) && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                {
                    text = string.Format("{2}<{0}>{1}</{0}>", tag, text.Remove(0, indexOfEndBracket + 1), text.Substring(0, indexOfEndBracket + 1));
                }
                else
                {
                    text = string.Format("<{0}>{1}</{0}>", tag, text);
                }
            }
            return text;
        }

    }
}