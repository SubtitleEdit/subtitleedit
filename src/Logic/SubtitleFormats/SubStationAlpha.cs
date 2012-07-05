using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class SubStationAlpha : SubtitleFormat
    {

        public string Errors { get; private set; }

        public override string Extension
        {
            get { return ".ssa"; }
        }

        public override string Name
        {
            get { return "Sub Station Alpha"; }
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
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string header =
@"[Script Info]
; This is a Sub Station Alpha v4 script.
Title: {0}
ScriptType: v4.00
Collisions: Normal
PlayDepth: 0

[V4 Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding
Style: Default,{1},{2},{3},65535,65535,-2147483640,-1,0,1,3,0,2,30,30,30,0,0

[Events]
Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text";

            const string timeCodeFormat = "{0}:{1:00}:{2:00}.{3:00}"; // h:mm:ss.cc
            const string paragraphWriteFormat = "Dialogue: Marked=0,{0},{1},{3},NTP,0000,0000,0000,!Effect,{2}";
            const string commentWriteFormat = "Dialogue: Marked=0,{0},{1},{3},NTP,0000,0000,0000,!Effect,{2}";

            var sb = new StringBuilder();
            System.Drawing.Color fontColor = System.Drawing.Color.FromArgb(Configuration.Settings.SubtitleSettings.SsaFontColorArgb);
            bool isValidAssHeader =!string.IsNullOrEmpty(subtitle.Header) && subtitle.Header.Contains("[V4 Styles]");
            List<string> styles = new List<string>();
            if (isValidAssHeader)
            {
                sb.AppendLine(subtitle.Header.Trim());
                sb.AppendLine("Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");
                styles = AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header);
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
                if (p.IsComment)
                    sb.AppendLine(string.Format(commentWriteFormat, start, end, FormatText(p), style));
                else
                    sb.AppendLine(string.Format(paragraphWriteFormat, start, end, FormatText(p), style));
            }
            return sb.ToString().Trim();
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
            int count = 0;
            while (text.Contains("<font ") && count < 10)
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
                count++;
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
                    {
                        subTag = subTag.Replace("#", string.Empty);

                        // switch from rrggbb to bbggrr
                        if (subTag.Length >= 6)
                            subTag = subTag.Remove(subTag.Length - 6) + subTag.Substring(subTag.Length - 2, 2) + subTag.Substring(subTag.Length - 4, 2) + subTag.Substring(subTag.Length - 6, 2);
                    }
                    fontTag = fontTag.Remove(fontStart, fontEnd - fontStart + 1);
                    text = text.Insert(start, @"{\" + ssaTagName + subTag + endSsaTag);
                }
            }
            return fontTag;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            bool eventsStarted = false;
            subtitle.Paragraphs.Clear();
            string[] format = "Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text".Split(',');
            int indexStart = 1;
            int indexEnd = 2;
            int indexStyle = 3;
            int indexText = 9;
            var errors = new StringBuilder();
            int lineNumber = 0;

            var header = new StringBuilder();
            foreach (string line in lines)
            {
                lineNumber++;
                if (!eventsStarted)
                    header.AppendLine(line);

                if (line.Trim().ToLower() == "[events]")
                {
                    eventsStarted = true;
                }
                else if (eventsStarted && line.Trim().Length > 0)
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
                    else if (!string.IsNullOrEmpty(s))
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
                            else if (i == indexText)
                                text = splittedLine[i];
                            else if (i == indexStyle)
                                style = splittedLine[i];
                            else if (i > indexText)
                                text += "," + splittedLine[i];
                        }

                        try
                        {
                            var p = new Paragraph();

                            p.StartTime = GetTimeCodeFromString(start);
                            p.EndTime = GetTimeCodeFromString(end);
                            p.Text = AdvancedSubStationAlpha.GetFormattedText(text);
                            if (!string.IsNullOrEmpty(style))
                                p.Extra = style;
                            p.IsComment = s.StartsWith("comment:");
                            subtitle.Paragraphs.Add(p);
                        }
                        catch
                        {
                            _errorCount++;
                            if (errors.Length < 2000)
                                errors.AppendLine(string.Format(Configuration.Settings.Language.Main.LineNumberXErrorReadingTimeCodeFromSourceLineY, lineNumber, line));
                        }
                    }
                }
            }
            if (header.Length > 0)
                subtitle.Header = header.ToString();
            subtitle.Renumber(1);
            Errors = errors.ToString();
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
                    p.Text = p.Text.Remove(indexOfBegin, (indexOfEnd - indexOfBegin) +1);

                    indexOfBegin = p.Text.IndexOf("{");
                }
            }
        }

    }
}
