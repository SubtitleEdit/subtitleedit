using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SubStationAlpha : SubtitleFormat
    {
        public string Errors { get; private set; }

        public override string Extension => ".ssa";

        public const string NameOfFormat = "Sub Station Alpha";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            Errors = null;
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
Style: Default,{1},{2},{3},65535,65535,-2147483640,{9},0,1,{4},{5},2,{6},{7},{8},0,1

[Events]
Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text";

            const string headerNoStyles =
@"[Script Info]
; This is a Sub Station Alpha v4 script.
Title: {0}
ScriptType: v4.00
Collisions: Normal
PlayDepth: 0

[V4 Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding
{1}

[Events]
Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text";

            const string timeCodeFormat = "{0}:{1:00}:{2:00}.{3:00}"; // h:mm:ss.cc
            const string paragraphWriteFormat = "Dialogue: Marked={4},{0},{1},{3},{5},{6},{7},{8},{9},{2}";
            const string commentWriteFormat = "Comment: Marked={4},{0},{1},{3},{5},{6},{7},{8},{9},{2}";

            var sb = new StringBuilder();
            Color fontColor = Color.FromArgb(Configuration.Settings.SubtitleSettings.SsaFontColorArgb);
            bool isValidAssHeader = !string.IsNullOrEmpty(subtitle.Header) && subtitle.Header.Contains("[V4 Styles]");
            var styles = new List<string>();
            if (isValidAssHeader)
            {
                sb.AppendLine(subtitle.Header.Trim());
                const string formatLine = "Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text";
                if (!subtitle.Header.Contains(formatLine))
                {
                    sb.AppendLine(formatLine);
                }

                styles = AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header);
            }
            else if (!string.IsNullOrEmpty(subtitle.Header) && subtitle.Header.Contains("[V4+ Styles]"))
            {
                LoadStylesFromAdvancedSubstationAlpha(subtitle, title, subtitle.Header, headerNoStyles, sb);
                isValidAssHeader = true;
                styles = AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header);
            }
            else if (subtitle.Header != null && subtitle.Header.Contains("http://www.w3.org/ns/ttml"))
            {
                LoadStylesFromTimedText10(subtitle, title, header, headerNoStyles, sb);
            }
            else
            {
                var ssa = Configuration.Settings.SubtitleSettings;
                string boldStyle = "0"; // 0=regular
                if (ssa.SsaFontBold)
                {
                    boldStyle = "-1"; // -1 = true, 0 is false
                }

                sb.AppendLine(string.Format(header,
                                            title,
                                            ssa.SsaFontName,
                                            (int)ssa.SsaFontSize,
                                            ColorTranslator.ToWin32(fontColor),
                                            ssa.SsaOutline,
                                            ssa.SsaShadow,
                                            ssa.SsaMarginLeft,
                                            ssa.SsaMarginRight,
                                            ssa.SsaMarginTopBottom,
                                            boldStyle
                                            ));
            }
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string start = string.Format(timeCodeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds / 10);
                string end = string.Format(timeCodeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds / 10);
                string style = "Default";

                string actor = "NTP";
                if (!string.IsNullOrEmpty(p.Actor))
                {
                    actor = p.Actor;
                }

                string marginL = "0000";
                if (!string.IsNullOrEmpty(p.MarginL) && Utilities.IsInteger(p.MarginL))
                {
                    marginL = p.MarginL.PadLeft(4, '0');
                }

                string marginR = "0000";
                if (!string.IsNullOrEmpty(p.MarginR) && Utilities.IsInteger(p.MarginR))
                {
                    marginR = p.MarginR.PadLeft(4, '0');
                }

                string marginV = "0000";
                if (!string.IsNullOrEmpty(p.MarginV) && Utilities.IsInteger(p.MarginV))
                {
                    marginV = p.MarginV.PadLeft(4, '0');
                }

                string effect = "";
                if (!string.IsNullOrEmpty(p.Effect))
                {
                    effect = p.Effect;
                }

                if (!string.IsNullOrEmpty(p.Extra) && isValidAssHeader && styles.Contains(p.Extra))
                {
                    style = p.Extra;
                }

                if (style == "Default")
                {
                    style = "*Default";
                }

                if (p.IsComment)
                {
                    sb.AppendLine(string.Format(commentWriteFormat, start, end, AdvancedSubStationAlpha.FormatText(p), style, p.Layer, actor, marginL, marginR, marginV, effect));
                }
                else
                {
                    sb.AppendLine(string.Format(paragraphWriteFormat, start, end, AdvancedSubStationAlpha.FormatText(p), style, p.Layer, actor, marginL, marginR, marginV, effect));
                }
            }
            return sb.ToString().Trim() + Environment.NewLine;
        }

        private static void LoadStylesFromAdvancedSubstationAlpha(Subtitle subtitle, string title, string header, string headerNoStyles, StringBuilder sb)
        {
            try
            {
                bool styleFound = false;
                var ttStyles = new StringBuilder();
                foreach (string styleName in AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header))
                {
                    try
                    {
                        var ssaStyle = AdvancedSubStationAlpha.GetSsaStyle(styleName, subtitle.Header);

                        string bold = "0";
                        if (ssaStyle.Bold)
                        {
                            bold = "-1";
                        }

                        string italic = "0";
                        if (ssaStyle.Italic)
                        {
                            italic = "-1";
                        }

                        string newAlignment = "2";
                        switch (ssaStyle.Alignment)
                        {
                            case "1":
                                newAlignment = "1";
                                break;
                            case "3":
                                newAlignment = "3";
                                break;
                            case "4":
                                newAlignment = "9";
                                break;
                            case "5":
                                newAlignment = "10";
                                break;
                            case "6":
                                newAlignment = "11";
                                break;
                            case "7":
                                newAlignment = "5";
                                break;
                            case "8":
                                newAlignment = "6";
                                break;
                            case "9":
                                newAlignment = "7";
                                break;
                        }

                        //Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding
                        const string styleFormat = "Style: {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},0,1";
                        //                                 N   FN  FS  PC  SC  TC  BC  Bo  It  BS  O    Sh   Ali  ML   MR   MV   A Encoding

                        ttStyles.AppendLine(string.Format(styleFormat, ssaStyle.Name, ssaStyle.FontName, ssaStyle.FontSize, ssaStyle.Primary.ToArgb(), ssaStyle.Secondary.ToArgb(),
                                            ssaStyle.Outline.ToArgb(), ssaStyle.Background.ToArgb(), bold, italic, ssaStyle.BorderStyle, ssaStyle.OutlineWidth.ToString(CultureInfo.InvariantCulture), ssaStyle.ShadowWidth.ToString(CultureInfo.InvariantCulture),
                                            newAlignment, ssaStyle.MarginLeft, ssaStyle.MarginRight, ssaStyle.MarginVertical));
                        styleFound = true;

                    }
                    catch
                    {
                        // ignored
                    }
                }

                if (styleFound)
                {
                    sb.AppendLine(string.Format(headerNoStyles, title, ttStyles));
                    subtitle.Header = sb.ToString();
                }
                else
                {
                    sb.AppendLine(string.Format(header, title));
                }
            }
            catch
            {
                sb.AppendLine(string.Format(header, title));
            }
        }

        private static void LoadStylesFromTimedText10(Subtitle subtitle, string title, string header, string headerNoStyles, StringBuilder sb)
        {
            try
            {
                var lines = subtitle.Header.SplitToLines();
                var tt = new TimedText10();
                var sub = new Subtitle();
                tt.LoadSubtitle(sub, lines, string.Empty);

                var xml = new XmlDocument();
                xml.LoadXml(subtitle.Header);
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                XmlNode head = xml.DocumentElement.SelectSingleNode("ttml:head", nsmgr);
                int stylexmlCount = 0;
                var ttStyles = new StringBuilder();
                foreach (XmlNode node in head.SelectNodes("//ttml:style", nsmgr))
                {
                    string name = null;
                    if (node.Attributes["xml:id"] != null)
                    {
                        name = node.Attributes["xml:id"].Value;
                    }
                    else if (node.Attributes["id"] != null)
                    {
                        name = node.Attributes["id"].Value;
                    }

                    if (name != null)
                    {
                        stylexmlCount++;

                        string fontFamily = "Arial";
                        if (node.Attributes["tts:fontFamily"] != null)
                        {
                            fontFamily = node.Attributes["tts:fontFamily"].Value;
                        }

                        string fontWeight = "normal";
                        if (node.Attributes["tts:fontWeight"] != null)
                        {
                            fontWeight = node.Attributes["tts:fontWeight"].Value;
                        }

                        string fontStyle = "normal";
                        if (node.Attributes["tts:fontStyle"] != null)
                        {
                            fontStyle = node.Attributes["tts:fontStyle"].Value;
                        }

                        string color = "#ffffff";
                        if (node.Attributes["tts:color"] != null)
                        {
                            color = node.Attributes["tts:color"].Value.Trim();
                        }

                        Color c;
                        try
                        {
                            if (color.StartsWith("rgb(", StringComparison.Ordinal))
                            {
                                string[] arr = color.Remove(0, 4).TrimEnd(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                c = Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                            }
                            else
                            {
                                c = ColorTranslator.FromHtml(color);
                            }
                        }
                        catch
                        {
                            c = Color.White;
                        }

                        string fontSize = "20";
                        if (node.Attributes["tts:fontSize"] != null)
                        {
                            fontSize = node.Attributes["tts:fontSize"].Value.Replace("px", string.Empty).Replace("em", string.Empty);
                        }

                        if (!int.TryParse(fontSize, out var fSize))
                        {
                            fSize = 20;
                        }

                        const string styleFormat = "Style: {0},{1},{2},{3},65535,65535,-2147483640,-1,0,1,3,0,2,10,10,10,0,1";

                        ttStyles.AppendLine(string.Format(styleFormat, name, fontFamily, fSize, c.ToArgb()));
                    }
                }

                if (stylexmlCount > 0)
                {
                    sb.AppendLine(string.Format(headerNoStyles, title, ttStyles));
                    subtitle.Header = sb.ToString();
                }
                else
                {
                    sb.AppendLine(string.Format(header, title));
                }
            }
            catch
            {
                sb.AppendLine(string.Format(header, title));
            }
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Errors = null;
            bool eventsStarted = false;
            subtitle.Paragraphs.Clear();
            // "Marked", " Start", " End", " Style", " Name", " MarginL", " MarginR", " MarginV", " Effect", " Text"
            int indexLayer = 0;
            int indexStart = 1;
            int indexEnd = 2;
            int indexStyle = 3;
            const int indexName = 4;
            int indexMarginL = 5;
            int indexMarginR = 6;
            int indexMarginV = 7;
            int indexEffect = 8;
            int indexText = 9;
            var errors = new StringBuilder();
            int lineNumber = 0;

            var header = new StringBuilder();
            for (int i1 = 0; i1 < lines.Count; i1++)
            {
                string line = lines[i1];
                lineNumber++;
                if (!eventsStarted)
                {
                    header.AppendLine(line);
                }

                if (!string.IsNullOrEmpty(line) && line.TrimStart().StartsWith(';'))
                {
                    // skip comment lines
                }
                else if (line.Trim().Equals("[events]", StringComparison.OrdinalIgnoreCase))
                {
                    eventsStarted = true;
                }
                else if (eventsStarted && !string.IsNullOrWhiteSpace(line))
                {
                    string s = line.Trim().ToLowerInvariant();
                    if (line.Length > 10 && s.StartsWith("format:", StringComparison.Ordinal))
                    {
                        var format = s.Substring(8).Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            var formatTrimmed = format[i].Trim();
                            if (formatTrimmed.Equals("layer", StringComparison.Ordinal))
                            {
                                indexLayer = i;
                            }
                            else if (formatTrimmed.Equals("start", StringComparison.Ordinal))
                            {
                                indexStart = i;
                            }
                            else if (formatTrimmed.Equals("end", StringComparison.Ordinal))
                            {
                                indexEnd = i;
                            }
                            else if (formatTrimmed.Equals("text", StringComparison.Ordinal))
                            {
                                indexText = i;
                            }
                            else if (formatTrimmed.Equals("effect", StringComparison.Ordinal))
                            {
                                indexEffect = i;
                            }
                            else if (formatTrimmed.Equals("style", StringComparison.Ordinal))
                            {
                                indexStyle = i;
                            }
                            else if (formatTrimmed.Equals("marginl", StringComparison.Ordinal))
                            {
                                indexMarginL = i;
                            }
                            else if (formatTrimmed.Equals("marginr", StringComparison.Ordinal))
                            {
                                indexMarginR = i;
                            }
                            else if (formatTrimmed.Equals("marginv", StringComparison.Ordinal))
                            {
                                indexMarginV = i;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(s))
                    {
                        var text = string.Empty;
                        var start = string.Empty;
                        var end = string.Empty;
                        var style = string.Empty;
                        var marginL = string.Empty;
                        var marginR = string.Empty;
                        var marginV = string.Empty;
                        var layer = 0;
                        var effect = string.Empty;
                        var name = string.Empty;

                        string[] splittedLine;
                        if (s.StartsWith("dialog:", StringComparison.Ordinal))
                        {
                            splittedLine = line.Remove(0, 7).Split(',');
                        }
                        else if (s.StartsWith("dialogue:", StringComparison.Ordinal))
                        {
                            splittedLine = line.Remove(0, 9).Split(',');
                        }
                        else
                        {
                            splittedLine = line.Split(',');
                        }

                        for (int i = 0; i < splittedLine.Length; i++)
                        {
                            if (i == indexStart)
                            {
                                start = splittedLine[i].Trim();
                            }
                            else if (i == indexEnd)
                            {
                                end = splittedLine[i].Trim();
                            }
                            else if (i == indexLayer)
                            {
                                int.TryParse(splittedLine[i], out layer);
                            }
                            else if (i == indexEffect)
                            {
                                effect = splittedLine[i];
                            }
                            else if (i == indexText)
                            {
                                text = splittedLine[i];
                            }
                            else if (i == indexStyle)
                            {
                                style = splittedLine[i];
                            }
                            else if (i == indexMarginL)
                            {
                                marginL = splittedLine[i].Trim();
                            }
                            else if (i == indexMarginR)
                            {
                                marginR = splittedLine[i].Trim();
                            }
                            else if (i == indexMarginV)
                            {
                                marginV = splittedLine[i].Trim();
                            }
                            else if (i == indexName)
                            {
                                name = splittedLine[i];
                            }
                            else if (i > indexText)
                            {
                                text += "," + splittedLine[i];
                            }
                        }

                        try
                        {
                            var p = new Paragraph
                            {
                                StartTime = GetTimeCodeFromString(start),
                                EndTime = GetTimeCodeFromString(end),
                                Text = AdvancedSubStationAlpha.GetFormattedText(text)
                            };

                            if (!string.IsNullOrEmpty(style))
                            {
                                p.Extra = style;
                            }

                            if (!string.IsNullOrEmpty(marginL))
                            {
                                p.MarginL = marginL;
                            }

                            if (!string.IsNullOrEmpty(marginR))
                            {
                                p.MarginR = marginR;
                            }

                            if (!string.IsNullOrEmpty(marginV))
                            {
                                p.MarginV = marginV;
                            }

                            if (!string.IsNullOrEmpty(effect))
                            {
                                p.Effect = effect;
                            }

                            p.Layer = layer;
                            if (!string.IsNullOrEmpty(name))
                            {
                                p.Actor = name;
                            }

                            p.IsComment = s.StartsWith("comment:", StringComparison.Ordinal);
                            subtitle.Paragraphs.Add(p);
                        }
                        catch
                        {
                            _errorCount++;
                            if (errors.Length < 2000)
                            {
                                errors.AppendLine(string.Format(Configuration.Settings.Language.Main.LineNumberXErrorReadingTimeCodeFromSourceLineY, lineNumber, line));
                            }
                            else if (subtitle.Paragraphs.Count == 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            if (header.Length > 0)
            {
                subtitle.Header = header.ToString();
            }

            subtitle.Renumber();
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

        public override void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
            if (newFormat != null && newFormat.Name == AdvancedSubStationAlpha.NameOfFormat)
            {
                // do we need any conversion?
            }
            else
            {
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    int indexOfBegin = p.Text.IndexOf('{');
                    string pre = string.Empty;
                    while (indexOfBegin >= 0 && p.Text.IndexOf('}') > indexOfBegin)
                    {
                        string s = p.Text.Substring(indexOfBegin);
                        if (s.StartsWith("{\\an1}", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an2}", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an3}", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an4}", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an5}", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an6}", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an7}", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an8}", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an9}", StringComparison.Ordinal))
                        {
                            pre = s.Substring(0, 6);
                        }
                        else if (s.StartsWith("{\\an1\\", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an2\\", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an3\\", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an4\\", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an5\\", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an6\\", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an7\\", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an8\\", StringComparison.Ordinal) ||
                            s.StartsWith("{\\an9\\", StringComparison.Ordinal))
                        {
                            pre = s.Substring(0, 5) + "}";
                        }
                        else if (s.StartsWith("{\\a1}", StringComparison.Ordinal) || s.StartsWith("{\\a1\\", StringComparison.Ordinal) ||
                                 s.StartsWith("{\\a3}", StringComparison.Ordinal) || s.StartsWith("{\\a3\\", StringComparison.Ordinal))
                        {
                            pre = s.Substring(0, 4) + "}";
                        }
                        else if (s.StartsWith("{\\a9}", StringComparison.Ordinal) || s.StartsWith("{\\a9\\", StringComparison.Ordinal))
                        {
                            pre = "{\\an4}";
                        }
                        else if (s.StartsWith("{\\a10}", StringComparison.Ordinal) || s.StartsWith("{\\a10\\", StringComparison.Ordinal))
                        {
                            pre = "{\\an5}";
                        }
                        else if (s.StartsWith("{\\a11}", StringComparison.Ordinal) || s.StartsWith("{\\a11\\", StringComparison.Ordinal))
                        {
                            pre = "{\\an6}";
                        }
                        else if (s.StartsWith("{\\a5}", StringComparison.Ordinal) || s.StartsWith("{\\a5\\", StringComparison.Ordinal))
                        {
                            pre = "{\\an7}";
                        }
                        else if (s.StartsWith("{\\a6}", StringComparison.Ordinal) || s.StartsWith("{\\a6\\", StringComparison.Ordinal))
                        {
                            pre = "{\\an8}";
                        }
                        else if (s.StartsWith("{\\a7}", StringComparison.Ordinal) || s.StartsWith("{\\a7\\", StringComparison.Ordinal))
                        {
                            pre = "{\\an9}";
                        }
                        int indexOfEnd = p.Text.IndexOf('}');
                        p.Text = p.Text.Remove(indexOfBegin, (indexOfEnd - indexOfBegin) + 1);

                        indexOfBegin = p.Text.IndexOf('{');
                    }
                    p.Text = pre + p.Text;
                }
            }
        }

        public override bool HasStyleSupport
        {
            get
            {
                return true;
            }
        }

    }
}
