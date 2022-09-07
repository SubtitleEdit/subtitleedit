using System;

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
                    if (index + "<font color".Length <= p.Text.Length && p.Text.Substring(index, "<font color".Length).ToLowerInvariant() == "<font color")
                    {
                        // New color
                        newColor = p.Text.Substring(p.Text.IndexOf("=", index) + 1, p.Text.IndexOf(">", index) - p.Text.IndexOf("=", index) - 1).Replace("\"", "");

                        if (currentColor == null)
                        {
                            currentColor = newColor;
                        }
                        else if (currentColor != newColor)
                        {
                            if (dashFirstLine && !firstLineAdded)
                            {
                                p.Text = dash + p.Text;
                                index += dash.Length;

                                firstLineAdded = true;
                            }

                            if (!addNewLines && p.Text.Substring(index - 1, 1) != " " && p.Text.Substring(index - 1, 1) != "\r" && p.Text.Substring(index - 1, 1) != "\n")
                            {
                                p.Text = p.Text.Substring(0, index) + " " + p.Text.Substring(index);
                                index += 1;
                            } 
                            else if (addNewLines && p.Text.Substring(index - 1, 1) != "\r" && p.Text.Substring(index - 1, 1) != "\n")
                            {
                                p.Text = p.Text.Substring(0, index) + Environment.NewLine + p.Text.Substring(index);
                                index += Environment.NewLine.Length;
                            }

                            p.Text = p.Text.Substring(0, index) + dash + p.Text.Substring(index);
                            index += dash.Length;

                            currentColor = newColor;
                        }

                        index = p.Text.IndexOf(">", index) + 1;

                        endOfColor = false;
                    }
                    else if (index + "</font>".Length <= p.Text.Length && p.Text.Substring(index, "</font>".Length).ToLowerInvariant() == "</font>")
                    {
                        // End of color
                        endOfColor = true;

                        index += "</font>".Length;
                    }
                    else if (index + 1 <= p.Text.Length && p.Text.Substring(index, 1) == " " || p.Text.Substring(index, 1) == "\r" || p.Text.Substring(index, 1) == "\n")
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
                                    if (dashFirstLine && !firstLineAdded)
                                    {
                                        p.Text = dash + p.Text;
                                        index += dash.Length;

                                        firstLineAdded = true;
                                    }

                                    if (!addNewLines && p.Text.Substring(index - 1, 1) != " " && p.Text.Substring(index - 1, 1) != "\r" && p.Text.Substring(index - 1, 1) != "\n")
                                    {
                                        p.Text = p.Text.Substring(0, index) + " " + p.Text.Substring(index);
                                        index += 1;
                                    }
                                    else if (addNewLines && p.Text.Substring(index - 1, 1) != "\r" && p.Text.Substring(index - 1, 1) != "\n")
                                    {
                                        p.Text = p.Text.Substring(0, index) + Environment.NewLine + p.Text.Substring(index);
                                        index += Environment.NewLine.Length;
                                    }

                                    p.Text = p.Text.Substring(0, index) + dash + p.Text.Substring(index);
                                    index += dash.Length;

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
                    p.Text = p.Text.Replace("  ", " ").Replace(" " + Environment.NewLine, Environment.NewLine);
                } 
                else
                {
                    p.Text = p.Text.Replace(" </font> ", "</font> ").Replace(" </font>" + Environment.NewLine, "</font>" + Environment.NewLine);
                }

                p.Text = p.Text.Trim();

                if (reBreakLines)
                {
                    p.Text = Utilities.AutoBreakLine(p.Text, language);
                }
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
