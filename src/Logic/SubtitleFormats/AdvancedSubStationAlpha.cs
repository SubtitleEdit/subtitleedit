using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class AdvancedSubStationAlpha : SubtitleFormat
    {
        public static string DefaultStyle
        {
            get
            {
                return "Style: Default," + Configuration.Settings.SubtitleSettings.SsaFontName + "," +
                    ((int)Configuration.Settings.SubtitleSettings.SsaFontSize) + "," +
                    GetSsaColorString(Color.FromArgb(Configuration.Settings.SubtitleSettings.SsaFontColorArgb)) + "," +
                    "&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,2,2,10,10,10,1";
            }
        }

        public override string Extension
        {
            get { return ".ass"; }
        }

        public override string Name
        {
            get { return "Advanced Sub Station Alpha"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string all = sb.ToString();
            if (!all.Contains("[V4+ Styles]"))
                return false;

            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string header =
@"[Script Info]
; This is an Advanced Sub Station Alpha v4+ script.
Title: {0}
ScriptType: v4.00+
Collisions: Normal
PlayDepth: 0

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
" + DefaultStyle + @"

[Events]
Format: Layer, Start, End, Style, Actor, MarginL, MarginR, MarginV, Effect, Text";

            const string timeCodeFormat = "{0}:{1:00}:{2:00}.{3:00}"; // h:mm:ss.cc
            const string paragraphWriteFormat = "Dialogue: 0,{0},{1},{3},Default,0000,0000,0000,,{2}";

            var sb = new StringBuilder();
            System.Drawing.Color fontColor = System.Drawing.Color.FromArgb(Configuration.Settings.SubtitleSettings.SsaFontColorArgb);
            bool isValidAssHeader =!string.IsNullOrEmpty(subtitle.Header) && subtitle.Header.Contains("[V4+ Styles]");
            List<string> styles = new List<string>();
            if (isValidAssHeader)
            {
                sb.AppendLine(subtitle.Header.Trim());
                sb.AppendLine("Format: Layer, Start, End, Style, Actor, MarginL, MarginR, MarginV, Effect, Text");
                styles = GetStylesFromHeader(subtitle.Header);
            }
            else
            {
                sb.AppendLine(string.Format(header,
                                            title,
                                            Configuration.Settings.SubtitleSettings.SsaFontName,
                                            (int)Configuration.Settings.SubtitleSettings.SsaFontSize,
                                            System.Drawing.ColorTranslator.ToWin32(fontColor)));
            }
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string start = string.Format(timeCodeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds / 10);
                string end = string.Format(timeCodeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds / 10);
                string style = "Default";
                if (!string.IsNullOrEmpty(p.Extra) && isValidAssHeader && styles.Contains(p.Extra))
                    style = p.Extra;
                sb.AppendLine(string.Format(paragraphWriteFormat, start, end, FormatText(p), style));
            }

            if (!string.IsNullOrEmpty(subtitle.Footer) && subtitle.Footer.Contains("[fonts]" + Environment.NewLine))
            {
                sb.AppendLine();
                sb.AppendLine(subtitle.Footer);
            }
            return sb.ToString().Trim();
        }

        public static List<string> GetStylesFromHeader(string headerLines)
        {
            var list = new List<string>();

            if (headerLines == null)
                headerLines = DefaultStyle;

            foreach (string line in headerLines.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.ToLower().StartsWith("style:"))
                {
                    int end = line.IndexOf(",");
                    if (end > 0)
                        list.Add(line.Substring(6, end - 6).Trim());
                }
            }
            return list;
        }

        private static string FormatText(Paragraph p)
        {
            string text = p.Text.Replace(Environment.NewLine, "\\N");
            text = text.Replace("<i>", @"{\i1}");
            text = text.Replace("</i>", @"{\i0}");
            text = text.Replace("<u>", @"{\u1}");
            text = text.Replace("</u>", @"{\u0}");
            text = text.Replace("<b>", @"{\b1}");
            text = text.Replace("</b>", @"{\b0}");
            if (text.Contains("<font "))
            {
                int start = text.IndexOf(@"<font ");
                int end = text.IndexOf('>', start);
                if (end > 0)
                {
                    string fontTag = text.Substring(start + 4, end - (start + 4));
                    text = text.Remove(start, end - start + 1);
                    text = text.Replace("</font>", string.Empty);

                    fontTag = FormatTag(ref text, start, fontTag, "face=\"", "\"", "fn", "}");
                    fontTag = FormatTag(ref text, start, fontTag, "face='", "'", "fn", "}");

                    fontTag = FormatTag(ref text, start, fontTag, "size=\"", "\"", "fs", "}");
                    fontTag = FormatTag(ref text, start, fontTag, "size='", "'", "fs", "}");

                    fontTag = FormatTag(ref text, start, fontTag, "color=\"", "\"", "c&H", "&}");
                    fontTag = FormatTag(ref text, start, fontTag, "color='", "'", "c&H", "&}");
                }
            }
            return text;
        }

        private static string FormatTag(ref string text, int start, string fontTag, string tag, string endSign, string ssaTagName, string endSsaTag)
        {
            if (fontTag.Contains(tag))
            {
                int fontStart = fontTag.IndexOf(tag);
                int fontEnd = fontTag.IndexOf(endSign, fontStart + tag.Length);
                if (fontEnd > 0)
                {
                    string subTag = fontTag.Substring(fontStart + tag.Length, fontEnd - (fontStart + tag.Length));
                    if (tag.Contains("color"))
                        subTag = subTag.Replace("#", string.Empty);
                    fontTag = fontTag.Remove(fontStart, fontEnd - fontStart + 1);
                    text = text.Insert(start, @"{\" + ssaTagName + subTag + endSsaTag);
                }
            }
            return fontTag;
        }

        private static string GetFormattedText(string text)
        {
            text = text.Replace("\\N", Environment.NewLine).Replace("\\n", Environment.NewLine);

            text = text.Replace(@"{\i1}", "<i>");
            text = text.Replace(@"{\i0}", "</i>");
            if (Utilities.CountTagInText(text, "<i>") > Utilities.CountTagInText(text, "</i>"))
                text += "</i>";

            text = text.Replace(@"{\u1}", "<u>");
            text = text.Replace(@"{\u0}", "</u>");
            if (Utilities.CountTagInText(text, "<u>") > Utilities.CountTagInText(text, "</u>"))
                text += "</u>";

            text = text.Replace(@"{\b1}", "<b>");
            text = text.Replace(@"{\b0}", "</b>");
            if (Utilities.CountTagInText(text, "<b>") > Utilities.CountTagInText(text, "</b>"))
                text += "</b>";

            for (int i = 0; i < 5; i++) // just look five times...
            {
                if (text.Contains(@"{\fn"))
                {
                    int start = text.IndexOf(@"{\fn");
                    int end = text.IndexOf('}', start);
                    if (end > 0)
                    {
                        string fontName = text.Substring(start + 4, end - (start + 4));
                        text = text.Remove(start, end - start + 1);
                        text = text.Insert(start, "<font name=\"" + fontName + "\">");
                        text += "</font>";
                    }
                }

                if (text.Contains(@"{\fs"))
                {
                    int start = text.IndexOf(@"{\fs");
                    int end = text.IndexOf('}', start);
                    if (end > 0)
                    {
                        string fontSize = text.Substring(start + 4, end - (start + 4));
                        if (Utilities.IsInteger(fontSize))
                        {
                            text = text.Remove(start, end - start + 1);
                            text = text.Insert(start, "<font size=\"" + fontSize + "\">");
                            text += "</font>";
                        }
                    }
                }

                if (text.Contains(@"{\c"))
                {
                    int start = text.IndexOf(@"{\c");
                    int end = text.IndexOf('}', start);
                    if (end > 0)
                    {
                        string color = text.Substring(start + 4, end - (start + 4));
                        color = color.Replace("&", string.Empty).TrimStart('H');
                        text = text.Remove(start, end - start + 1);
                        text = text.Insert(start, "<font color=\"" + color + "\">");
                        text += "</font>";
                    }
                }
            }

            return text;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            bool eventsStarted = false;
            bool fontsStarted = false;
            subtitle.Paragraphs.Clear();
            string[] format = "Layer, Start, End, Style, Actor, MarginL, MarginR, MarginV, Effect, Text".Split(',');
            int indexStart = 1;
            int indexEnd = 2;
            int indexStyle = 3;
            int indexText = 9;

            var header = new StringBuilder();
            var fonts = new StringBuilder();
            foreach (string line in lines)
            {
                if (!eventsStarted && !fontsStarted)
                    header.AppendLine(line);

                if (line.Trim().ToLower().StartsWith("dialogue:")) // fix faulty font tags...
                {
                    eventsStarted = true;
                    fontsStarted = false;
                }

                if (line.Trim().ToLower() == "[events]")
                {
                    eventsStarted = true;
                    fontsStarted = false;
                }
                else if (line.Trim().ToLower() == "[fonts]")
                {
                    eventsStarted = false;
                    fontsStarted = true;
                    fonts.AppendLine("[fonts]");
                }
                else if (fontsStarted)
                {
                    fonts.AppendLine(line);
                }
                else if (eventsStarted)
                {
                    string s = line.Trim().ToLower();
                    if (s.StartsWith("format:"))
                    {
                        if (line.Length > 10)
                        {
                            format = line.ToLower().Substring(8).Split(',');
                            for (int i = 0; i < format.Length; i++)
                            {
                                if (format[i].Trim().ToLower() == "start")
                                    indexStart = i;
                                else if (format[i].Trim().ToLower() == "end")
                                    indexEnd = i;
                                else if (format[i].Trim().ToLower() == "text")
                                    indexText = i;
                                else if (format[i].Trim().ToLower() == "style")
                                    indexStyle = i;
                            }
                        }
                    }
                    else
                    {
                        string text = string.Empty;
                        string start = string.Empty;
                        string end = string.Empty;
                        string style = string.Empty;

                        string[] splittedLine;

                        if (s.StartsWith("dialogue:"))
                            splittedLine = line.Substring(10).Split(',');
                        else
                            splittedLine = line.Split(',');

                        for (int i = 0; i < splittedLine.Length; i++)
                        {
                            if (i == indexStart)
                                start = splittedLine[i].Trim();
                            else if (i == indexEnd)
                                end = splittedLine[i].Trim();
                            else if (i == indexStyle)
                                style = splittedLine[i].Trim();
                            else if (i == indexText)
                                text = splittedLine[i];
                            else if (i > indexText)
                                text += "," + splittedLine[i];
                        }

                        try
                        {
                            var p = new Paragraph();

                            p.StartTime = GetTimeCodeFromString(start);
                            p.EndTime = GetTimeCodeFromString(end);
                            p.Text = GetFormattedText(text);
                            if (!string.IsNullOrEmpty(style))
                                p.Extra = style;
                            subtitle.Paragraphs.Add(p);
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
            }
            if (header.Length > 0)
                subtitle.Header = header.ToString();
            if (fonts.Length > 0)
                subtitle.Footer = fonts.ToString();
            subtitle.Renumber(1);
        }

        private static TimeCode GetTimeCodeFromString(string time)
        {
            // h:mm:ss.cc
            string[] timeCode = time.Split(':', '.');
            return new TimeCode(int.Parse(timeCode[0]),
                                int.Parse(timeCode[1]),
                                int.Parse(timeCode[2]),
                                int.Parse(timeCode[3]) * 10);
        }

        public override void RemoveNativeFormatting(Subtitle subtitle)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                int indexOfBegin = p.Text.IndexOf("{");
                while (indexOfBegin >= 0 && p.Text.IndexOf("}") > indexOfBegin)
                {
                    int indexOfEnd = p.Text.IndexOf("}");
                    p.Text = p.Text.Remove(indexOfBegin, (indexOfEnd - indexOfBegin) + 1);

                    indexOfBegin = p.Text.IndexOf("{");
                }
            }
        }

        /// <summary>
        /// BGR color like this: &HBBGGRR& (where BB, GG, and RR are hex values in uppercase)
        /// </summary>
        /// <param name="f">Input string</param>
        /// <param name="defaultColor">Default color</param>
        /// <returns>Input string as color, or default color if problems</returns>
        public static Color GetSsaColor(string f, Color defaultColor)
        {
            //Red = &H0000FF&
            //Green = &H00FF00&
            //Blue = &HFF0000&
            //White = &HFFFFFF&
            //Black = &H000000&
            string s = f.Trim().Trim('&');
            if (s.ToLower().StartsWith("h") && s.Length == 7)
            {
                s = s.Substring(1);
                string hexColor = "#" + s.Substring(4, 2) + s.Substring(2, 2) + s.Substring(0, 2);
                try
                {
                    return System.Drawing.ColorTranslator.FromHtml(hexColor);
                }
                catch
                {
                    return defaultColor;
                }
            }
            else if (s.ToLower().StartsWith("h") && s.Length == 9)
            {
                s = s.Substring(3);
                string hexColor = "#" + s.Substring(4, 2) + s.Substring(2, 2) + s.Substring(0, 2);
                try
                {
                    var c = System.Drawing.ColorTranslator.FromHtml(hexColor);

                    return c;
                }
                catch
                {
                    return defaultColor;
                }
            }
            else
            {
                int number;
                if (int.TryParse(f, out number))
                { 
                    Color temp = Color.FromArgb(number);
                    return Color.FromArgb(255, temp.B, temp.G, temp.R);
                }
            }
            return defaultColor;
        }

        public static string GetSsaColorString(Color c)
        {
            return string.Format("&H00{0:x2}{1:x2}{2:x2}", c.B, c.G, c.R).ToUpper();
        }

    }
}
