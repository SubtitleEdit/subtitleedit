using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class ConvertColorsToDialogUtils
    {
        public static void ConvertColorsToDialogInSubtitle(Subtitle subtitle, bool removeColorTags, bool dashFirstLine, bool spaceAfterDash, bool addNewLines, bool reBreakLines, string language)
        {
            int index;
            string newColor;
            string currentColor;
            bool firstLineAdded;
            bool endOfColor;

            var dash = "-";

            if (spaceAfterDash)
            {
                dash += " ";
            }

            // Fix
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                index = 0;
                currentColor = null;
                firstLineAdded = false;
                endOfColor = false;

                while (index < p.Text.Length)
                {
                    bool isHtmlColor = IsAt(p.Text, index, "<font color", StringComparison.OrdinalIgnoreCase);
                    bool isVttColor = IsAt(p.Text, index, "<c.", StringComparison.OrdinalIgnoreCase);

                    if (isHtmlColor || isVttColor)
                    {
                        // New color
                        if (isVttColor)
                        {
                            newColor = p.Text.SafeSubstring(p.Text.IndexOf(".", index) + 1, p.Text.IndexOf(">", index) - p.Text.IndexOf(".", index) - 1);
                        }
                        else
                        {
                            newColor = p.Text.SafeSubstring(p.Text.IndexOf("=", index) + 1, p.Text.IndexOf(">", index) - p.Text.IndexOf("=", index) - 1).Replace("\"", "");
                        }

                        if (currentColor == null)
                        {
                            currentColor = newColor;
                        }
                        else if (currentColor != newColor)
                        {
                            // Don't insert dash if there is already a dash, but DO insert a dash if it is an interruption
                            if (!CharAt(p.Text, index, '-') && !CharAt(p.Text, index - 1, '-')
                                && (!IsAt(p.Text, index - 2, "- ", StringComparison.Ordinal) || IsAt(p.Text, index - 3, "-- ", StringComparison.Ordinal)))
                            {
                                if (dashFirstLine && !firstLineAdded)
                                {
                                    if (p.Text.StartsWith("{"))
                                    {
                                        var lastBraceIndex = p.Text.LastIndexOf("}");
                                        p.Text = p.Text.SafeSubstring(0, lastBraceIndex + 1) + dash + p.Text.SafeSubstring(lastBraceIndex + 1);
                                    }
                                    else
                                    {
                                        var oldLength = p.Text.Length;
                                        var newLength = p.Text.TrimStart('-', ' ').Length;
                                        p.Text = dash + p.Text.TrimStart('-', ' ');
                                        index += newLength - oldLength;
                                    }

                                    index += dash.Length;

                                    firstLineAdded = true;
                                }

                                if (!addNewLines && !CharAt(p.Text, index - 1, ' ') && !CharAt(p.Text, index - 1, '\r') && !CharAt(p.Text, index - 1, '\n'))
                                {
                                    p.Text = p.Text.SafeSubstring(0, index) + " " + p.Text.SafeSubstring(index);
                                    index += 1;
                                }
                                else if (addNewLines && !CharAt(p.Text, index - 1, '\r') && !CharAt(p.Text, index - 1, '\n'))
                                {
                                    p.Text = p.Text.SafeSubstring(0, index) + Environment.NewLine + p.Text.SafeSubstring(index);
                                    index += Environment.NewLine.Length;
                                }

                                p.Text = p.Text.SafeSubstring(0, index) + dash + p.Text.SafeSubstring(index);
                                index += dash.Length;
                            }

                            currentColor = newColor;
                        }

                        // An unterminated tag has no '>': IndexOf returns -1 and "+ 1" put the
                        // scan back at 0, so the same tag matched again forever and the whole
                        // app froze. Nothing after an unterminated tag is parseable - stop.
                        var tagEnd = p.Text.IndexOf(">", index, StringComparison.Ordinal);
                        if (tagEnd < 0)
                        {
                            break;
                        }

                        index = tagEnd + 1;

                        endOfColor = false;
                    }
                    else if (IsAt(p.Text, index, "</font>", StringComparison.OrdinalIgnoreCase))
                    {
                        // End of HTML color
                        endOfColor = true;

                        index += "</font>".Length;
                    }
                    else if (IsAt(p.Text, index, "</c>", StringComparison.OrdinalIgnoreCase))
                    {
                        // End of VTT color
                        endOfColor = true;

                        index += "</c>".Length;
                    }
                    else if (CharAt(p.Text, index, '{'))
                    {
                        // ASS tag, jump over. Same trap as the '>' search above: an unclosed
                        // '{' has no '}', and "-1 + 1" restarted the scan at 0 forever.
                        var assaTagEnd = p.Text.IndexOf("}", index, StringComparison.Ordinal);
                        if (assaTagEnd < 0)
                        {
                            break;
                        }

                        index = assaTagEnd + 1;
                    }
                    else if (CharAt(p.Text, index, ' ') || CharAt(p.Text, index, '\r') || CharAt(p.Text, index, '\n'))
                    {
                        // Whitespace, ignore
                        index += 1;
                    }
                    else
                    {
                        // New white color                            
                        if (currentColor == null)
                        {
                            currentColor = "#ffffff";
                        }
                        else
                        {
                            if (endOfColor)
                            {
                                newColor = "#ffffff";

                                if (currentColor != newColor)
                                {
                                    // Don't insert dash if there is already a dash, but DO insert a dash if it is an interruption
                                    if (!CharAt(p.Text, index, '-') && !CharAt(p.Text, index - 1, '-')
                                        && (!IsAt(p.Text, index - 2, "- ", StringComparison.Ordinal) || IsAt(p.Text, index - 3, "-- ", StringComparison.Ordinal)))
                                    {
                                        if (dashFirstLine && !firstLineAdded)
                                        {
                                            if (p.Text.StartsWith("{"))
                                            {
                                                var lastBraceIndex = p.Text.LastIndexOf("}");
                                                p.Text = p.Text.SafeSubstring(0, lastBraceIndex + 1) + dash + p.Text.SafeSubstring(lastBraceIndex + 1);
                                            }
                                            else
                                            {
                                                p.Text = dash + p.Text;
                                            }

                                            index += dash.Length;

                                            firstLineAdded = true;
                                        }

                                        if (!addNewLines && !CharAt(p.Text, index - 1, ' ') && !CharAt(p.Text, index - 1, '\r') && !CharAt(p.Text, index - 1, '\n'))
                                        {
                                            if (CharAt(p.Text, index, '.'))
                                            {
                                                index++;
                                            }

                                            p.Text = p.Text.SafeSubstring(0, index) + " " + p.Text.SafeSubstring(index);
                                            index += 1;
                                        }
                                        else if (addNewLines && !CharAt(p.Text, index - 1, '\r') && !CharAt(p.Text, index - 1, '\n'))
                                        {
                                            if (!CharAt(p.Text, index + 1, '\r') && !CharAt(p.Text, index + 1, '\n') &&
                                                index < p.Text.Length-1)
                                            {
                                                p.Text = p.Text.SafeSubstring(0, index) + Environment.NewLine + p.Text.SafeSubstring(index);
                                                index += Environment.NewLine.Length;
                                            }
                                        }

                                        if (!CharAt(p.Text, index + 1, '\r') && !CharAt(p.Text, index + 1, '\n') &&
                                                index < p.Text.Length - 1)
                                        {
                                            p.Text = p.Text.SafeSubstring(0, index) + dash + p.Text.SafeSubstring(index);
                                            index += dash.Length;
                                        }
                                    }

                                    currentColor = newColor;
                                }
                            }
                        }

                        index += 1;
                        endOfColor = false;
                    }
                }

                if (removeColorTags)
                {
                    p.Text = HtmlUtil.RemoveColorTags(p.Text);

                    if (p.Text.Contains("<c."))
                    {
                        p.Text = Regex.Replace(p.Text, @"<c(\.[\w\d]+)?>(.*?)</c>", "$2");
                    }

                    p.Text = p.Text.Replace("  ", " ").Replace(" " + Environment.NewLine, Environment.NewLine);
                }
                else
                {
                    p.Text = p.Text.Replace(" </font> ", "</font> ").Replace(" </font>" + Environment.NewLine, "</font>" + Environment.NewLine);
                    p.Text = p.Text.Replace(" </c> ", "</c> ").Replace(" </c>" + Environment.NewLine, "</c>" + Environment.NewLine);
                }

                p.Text = p.Text.Trim();

                if (reBreakLines)
                {
                    p.Text = Utilities.AutoBreakLine(p.Text, language);
                }
            }
        }

        /// <summary>
        /// True when <paramref name="value"/> has <paramref name="c"/> at <paramref name="index"/>;
        /// false when the index is out of range, which is what the one-char SafeSubstring probes
        /// it replaces answered ("" never equals a one-char string). No string per probe - the
        /// scan probes every position of every paragraph.
        /// </summary>
        private static bool CharAt(string value, int index, char c)
        {
            return index >= 0 && index < value.Length && value[index] == c;
        }

        /// <summary>
        /// True when <paramref name="prefix"/> occurs at <paramref name="index"/>; false when the
        /// window falls outside the string, matching the SafeSubstring-equals probes it replaces.
        /// </summary>
        private static bool IsAt(string value, int index, string prefix, StringComparison comparison)
        {
            return index >= 0 && index <= value.Length && value.AsSpan(index).StartsWith(prefix.AsSpan(), comparison);
        }

        private static string SafeSubstring(this string value, int startIndex, int length = -1, string defaultValue = "")
        {
            try
            {
                if (length >= 0)
                {
                    return value.Substring(startIndex, length);
                }
                else
                {
                    return value.Substring(startIndex);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return defaultValue;
            }
        }

        public static void ConvertColorsToDialogInSubtitle(Subtitle subtitle, bool removeColorTags, bool addNewLines, bool reBreakLines)
        {
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);

            switch (Configuration.Settings.General.DialogStyle)
            {
                case Enums.DialogType.DashBothLinesWithoutSpace:
                    ConvertColorsToDialogInSubtitle(subtitle, removeColorTags, true, false, addNewLines, reBreakLines, language);
                    break;
                case Enums.DialogType.DashSecondLineWithSpace:
                    ConvertColorsToDialogInSubtitle(subtitle, removeColorTags, false, true, addNewLines, reBreakLines, language);
                    break;
                case Enums.DialogType.DashSecondLineWithoutSpace:
                    ConvertColorsToDialogInSubtitle(subtitle, removeColorTags, false, false, addNewLines, reBreakLines, language);
                    break;
                default:
                    ConvertColorsToDialogInSubtitle(subtitle, removeColorTags, true, true, addNewLines, reBreakLines, language);
                    break;
            }
        }
    }
}
