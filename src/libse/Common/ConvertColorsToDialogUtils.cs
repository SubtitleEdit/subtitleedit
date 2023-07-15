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
                    bool isHtmlColor = index + "<font color".Length <= p.Text.Length && p.Text.SafeSubstring(index, "<font color".Length).ToLowerInvariant() == "<font color";
                    bool isVttColor = index + "<c.".Length <= p.Text.Length && p.Text.SafeSubstring(index, "<c.".Length).ToLowerInvariant() == "<c.";

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
                            if (p.Text.SafeSubstring(index, 1) != "-" && p.Text.SafeSubstring(index - 1, 1) != "-" 
                                && (p.Text.SafeSubstring(index - 2, 2) != "- " || p.Text.SafeSubstring(index - 3, 3) == "-- "))
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

                                if (!addNewLines && p.Text.SafeSubstring(index - 1, 1) != " " && p.Text.SafeSubstring(index - 1, 1) != "\r" && p.Text.SafeSubstring(index - 1, 1) != "\n")
                                {
                                    p.Text = p.Text.SafeSubstring(0, index) + " " + p.Text.SafeSubstring(index);
                                    index += 1;
                                }
                                else if (addNewLines && p.Text.SafeSubstring(index - 1, 1) != "\r" && p.Text.SafeSubstring(index - 1, 1) != "\n")
                                {
                                    p.Text = p.Text.SafeSubstring(0, index) + Environment.NewLine + p.Text.SafeSubstring(index);
                                    index += Environment.NewLine.Length;
                                }

                                p.Text = p.Text.SafeSubstring(0, index) + dash + p.Text.SafeSubstring(index);
                                index += dash.Length;
                            }

                            currentColor = newColor;
                        }

                        index = p.Text.IndexOf(">", index) + 1;

                        endOfColor = false;
                    }
                    else if (index + "</font>".Length <= p.Text.Length && p.Text.SafeSubstring(index, "</font>".Length).ToLowerInvariant() == "</font>")
                    {
                        // End of HTML color
                        endOfColor = true;

                        index += "</font>".Length;
                    }
                    else if (index + "</c>".Length <= p.Text.Length && p.Text.SafeSubstring(index, "</c>".Length).ToLowerInvariant() == "</c>")
                    {
                        // End of VTT color
                        endOfColor = true;

                        index += "</c>".Length;
                    }
                    else if (index + "{".Length <= p.Text.Length && p.Text.SafeSubstring(index, "{".Length) == "{")
                    {
                        // ASS tag, jump over
                        index = p.Text.IndexOf("}", index) + 1;
                    }
                    else if (index + 1 <= p.Text.Length && p.Text.SafeSubstring(index, 1) == " " || p.Text.SafeSubstring(index, 1) == "\r" || p.Text.SafeSubstring(index, 1) == "\n")
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
                                    if (p.Text.SafeSubstring(index, 1) != "-" && p.Text.SafeSubstring(index - 1, 1) != "-" 
                                        && (p.Text.SafeSubstring(index - 2, 2) != "- " || p.Text.SafeSubstring(index - 3, 3) == "-- "))
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

                                        if (!addNewLines && p.Text.SafeSubstring(index - 1, 1) != " " && p.Text.SafeSubstring(index - 1, 1) != "\r" && p.Text.SafeSubstring(index - 1, 1) != "\n")
                                        {
                                            p.Text = p.Text.SafeSubstring(0, index) + " " + p.Text.SafeSubstring(index);
                                            index += 1;
                                        }
                                        else if (addNewLines && p.Text.SafeSubstring(index - 1, 1) != "\r" && p.Text.SafeSubstring(index - 1, 1) != "\n")
                                        {
                                            p.Text = p.Text.SafeSubstring(0, index) + Environment.NewLine + p.Text.SafeSubstring(index);
                                            index += Environment.NewLine.Length;
                                        }

                                        p.Text = p.Text.SafeSubstring(0, index) + dash + p.Text.SafeSubstring(index);
                                        index += dash.Length;
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
