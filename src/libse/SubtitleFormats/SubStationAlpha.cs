using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        private const string HeaderNoStyles =
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
Style: Default,{1},{2},{3},{4},{5},{6},{12:0.##},0,1,{7:0.##},{8:0.##},2,{9:0.##},{10:0.##},{11:0.##},0,1

[Events]
Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text";



            const string timeCodeFormat = "{0}:{1:00}:{2:00}.{3:00}"; // h:mm:ss.cc
            const string paragraphWriteFormat = "Dialogue: Marked={4},{0},{1},{3},{5},{6},{7},{8},{9},{2}";
            const string commentWriteFormat = "Comment: Marked={4},{0},{1},{3},{5},{6},{7},{8},{9},{2}";

            var sb = new StringBuilder();
            var isValidSsaHeader = !string.IsNullOrEmpty(subtitle.Header) && subtitle.Header.Contains("[V4 Styles]");
            var styles = new List<string>();
            if (isValidSsaHeader)
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
                LoadStylesFromAdvancedSubstationAlpha(subtitle, title, subtitle.Header, HeaderNoStyles, sb);
                isValidSsaHeader = true;
                styles = AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header);
            }
            else if (subtitle.Header != null && subtitle.Header.Contains("http://www.w3.org/ns/ttml"))
            {
                LoadStylesFromTimedText10(subtitle, title, header, HeaderNoStyles, sb);
            }
            else
            {
                var style = GetDefaultStyle();

                var boldStyle = "0"; // 0=regular
                if (style.Bold)
                {
                    boldStyle = "-1"; // -1 = true, 0 is false
                }

                sb.AppendLine(string.Format(header,
                                            title,
                                            style.FontName,
                                            style.FontSize,
                                            ColorTranslator.ToWin32(style.Primary),
                                            ColorTranslator.ToWin32(style.Secondary),
                                            ColorTranslator.ToWin32(style.Tertiary),
                                            ColorTranslator.ToWin32(style.Background),
                                            style.OutlineWidth,
                                            style.ShadowWidth,
                                            style.MarginLeft,
                                            style.MarginRight,
                                            style.MarginVertical,
                                            boldStyle
                                            ));
            }
            foreach (var p in subtitle.Paragraphs)
            {
                var start = string.Format(timeCodeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds / 10);
                var end = string.Format(timeCodeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds / 10);
                var style = "Default";
                var actor = "NTP";
                if (!string.IsNullOrEmpty(p.Actor))
                {
                    actor = p.Actor;
                }

                var marginL = "0000";
                if (!string.IsNullOrEmpty(p.MarginL) && Utilities.IsInteger(p.MarginL))
                {
                    marginL = p.MarginL.PadLeft(4, '0');
                }

                var marginR = "0000";
                if (!string.IsNullOrEmpty(p.MarginR) && Utilities.IsInteger(p.MarginR))
                {
                    marginR = p.MarginR.PadLeft(4, '0');
                }

                var marginV = "0000";
                if (!string.IsNullOrEmpty(p.MarginV) && Utilities.IsInteger(p.MarginV))
                {
                    marginV = p.MarginV.PadLeft(4, '0');
                }

                var effect = string.Empty;
                if (!string.IsNullOrEmpty(p.Effect))
                {
                    effect = p.Effect;
                }

                if (!string.IsNullOrEmpty(p.Extra) && isValidSsaHeader && styles.Contains(p.Extra))
                {
                    style = p.Extra;
                }

                if (style == "Default")
                {
                    style = "*Default";
                }

                var text = p.Text.Replace(Environment.NewLine, "\\N");
                if (p.IsComment)
                {
                    sb.AppendLine(string.Format(commentWriteFormat, start, end, AdvancedSubStationAlpha.FormatText(text), style, p.Layer, actor, marginL, marginR, marginV, effect));
                }
                else
                {
                    sb.AppendLine(string.Format(paragraphWriteFormat, start, end, AdvancedSubStationAlpha.FormatText(text), style, p.Layer, actor, marginL, marginR, marginV, effect));
                }
            }

            if (!string.IsNullOrEmpty(subtitle.Footer) &&
                (subtitle.Footer.Contains("[Fonts]" + Environment.NewLine) || subtitle.Footer.Contains("[Graphics]" + Environment.NewLine) || subtitle.Footer.Contains("[Aegisub Extradata]" + Environment.NewLine)))
            {
                sb.AppendLine();
                sb.AppendLine(subtitle.Footer);
            }

            return sb.ToString().Trim() + Environment.NewLine;
        }

        private static SsaStyle GetDefaultStyle()
        {
            SsaStyle style = null;
            var storageCategories = Configuration.Settings.SubtitleSettings.AssaStyleStorageCategories;
            if (storageCategories != null && storageCategories.Count > 0 && storageCategories.Exists(x => x.IsDefault))
            {
                var defaultStyle = storageCategories.FirstOrDefault(x => x.IsDefault)?.Styles.FirstOrDefault(x => x.Name.ToLowerInvariant() == "default");
                style = defaultStyle ?? storageCategories.FirstOrDefault(x => x.IsDefault)?.Styles[0];
                if (style != null)
                {
                    style.Tertiary = style.Outline;
                }
            }

            style = style ?? new SsaStyle();

            return style;
        }

        private static void LoadStylesFromAdvancedSubstationAlpha(Subtitle subtitle, string title, string header, string headerNoStyles, StringBuilder sb)
        {
            try
            {
                var style = GetStyle(subtitle.Header);
                if (!string.IsNullOrEmpty(style))
                {
                    sb.AppendLine(string.Format(headerNoStyles, title, style));
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

        private static string GetStyle(string header)
        {
            var ttStyles = new StringBuilder();
            foreach (var styleName in AdvancedSubStationAlpha.GetStylesFromHeader(header))
            {
                try
                {
                    var ssaStyle = AdvancedSubStationAlpha.GetSsaStyle(styleName, header);

                    var bold = "0";
                    if (ssaStyle.Bold)
                    {
                        bold = "-1";
                    }

                    var italic = "0";
                    if (ssaStyle.Italic)
                    {
                        italic = "-1";
                    }

                    var newAlignment = "2";
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
                    const string styleFormat = "Style: {0},{1},{2:0.#},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},0,1";
                    //                                 N   FN  FS  PC  SC  TC  BC  Bo  It  BS  O    Sh   Ali  ML   MR   MV   A Encoding

                    ttStyles.AppendLine(string.Format(styleFormat, ssaStyle.Name, ssaStyle.FontName, ssaStyle.FontSize, ssaStyle.Primary.ToArgb(), ssaStyle.Secondary.ToArgb(),
                        ssaStyle.Outline.ToArgb(), ssaStyle.Background.ToArgb(), bold, italic, ssaStyle.BorderStyle, ssaStyle.OutlineWidth.ToString(CultureInfo.InvariantCulture), ssaStyle.ShadowWidth.ToString(CultureInfo.InvariantCulture),
                        newAlignment, ssaStyle.MarginLeft, ssaStyle.MarginRight, ssaStyle.MarginVertical));
                }
                catch
                {
                    // ignored
                }
            }

            return ttStyles.ToString();
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

                        var fontFamily = "Arial";
                        if (node.Attributes["tts:fontFamily"] != null)
                        {
                            fontFamily = node.Attributes["tts:fontFamily"].Value;
                        }

                        var fontWeight = "normal";
                        if (node.Attributes["tts:fontWeight"] != null)
                        {
                            fontWeight = node.Attributes["tts:fontWeight"].Value;
                        }

                        var fontStyle = "normal";
                        if (node.Attributes["tts:fontStyle"] != null)
                        {
                            fontStyle = node.Attributes["tts:fontStyle"].Value;
                        }

                        var color = "#ffffff";
                        if (node.Attributes["tts:color"] != null)
                        {
                            color = node.Attributes["tts:color"].Value.Trim();
                        }

                        var c = HtmlUtil.GetColorFromString(color);

                        var fontSize = "20";
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
            var eventsStarted = false;
            var fontsStarted = false;
            var graphicsStarted = false;
            subtitle.Paragraphs.Clear();
            // "Marked", " Start", " End", " Style", " Name", " MarginL", " MarginR", " MarginV", " Effect", " Text"
            var indexLayer = 0;
            var indexStart = 1;
            var indexEnd = 2;
            var indexStyle = 3;
            const int indexName = 4;
            var indexMarginL = 5;
            var indexMarginR = 6;
            var indexMarginV = 7;
            var indexEffect = 8;
            var indexText = 9;
            var errors = new StringBuilder();
            var lineNumber = 0;

            var header = new StringBuilder();
            var footer = new StringBuilder();
            for (var i1 = 0; i1 < lines.Count; i1++)
            {
                var line = lines[i1];
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
                    fontsStarted = false;
                    graphicsStarted = false;
                }
                else if (line.Trim().Equals("[fonts]", StringComparison.OrdinalIgnoreCase))
                {
                    eventsStarted = false;
                    fontsStarted = true;
                    graphicsStarted = false;
                    footer.AppendLine();
                    footer.AppendLine("[Fonts]");
                }
                else if (line.Trim().Equals("[graphics]", StringComparison.OrdinalIgnoreCase))
                {
                    eventsStarted = false;
                    fontsStarted = false;
                    graphicsStarted = true;
                    footer.AppendLine();
                    footer.AppendLine("[Graphics]");
                }
                else if (eventsStarted && !string.IsNullOrWhiteSpace(line))
                {
                    var s = line.Trim().ToLowerInvariant();
                    if (line.Length > 10 && s.StartsWith("format:", StringComparison.Ordinal))
                    {
                        var format = s.Substring(8).Split(',');
                        for (var i = 0; i < format.Length; i++)
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
                    else if (fontsStarted)
                    {
                        footer.AppendLine(line);
                    }
                    else if (graphicsStarted)
                    {
                        footer.AppendLine(line);
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

                        string[] splitLine;
                        if (s.StartsWith("dialog:", StringComparison.Ordinal))
                        {
                            var dialog = line.Remove(0, 7);
                            if (dialog.StartsWith(' '))
                            {
                                dialog = dialog.Remove(0, 1);
                            }
                            splitLine = dialog.Split(',');
                        }
                        else if (s.StartsWith("dialogue:", StringComparison.Ordinal))
                        {
                            var dialog = line.Remove(0, 9);
                            if (dialog.StartsWith(' '))
                            {
                                dialog = dialog.Remove(0, 1);
                            }
                            splitLine = dialog.Split(',');
                        }
                        else
                        {
                            splitLine = line.Split(',');
                        }

                        for (var i = 0; i < splitLine.Length; i++)
                        {
                            if (i == indexStart)
                            {
                                start = splitLine[i].Trim();
                            }
                            else if (i == indexEnd)
                            {
                                end = splitLine[i].Trim();
                            }
                            else if (i == indexLayer)
                            {
                                int.TryParse(splitLine[i], out layer);
                            }
                            else if (i == indexEffect)
                            {
                                effect = splitLine[i];
                            }
                            else if (i == indexText)
                            {
                                text = splitLine[i];
                            }
                            else if (i == indexStyle)
                            {
                                style = splitLine[i];
                            }
                            else if (i == indexMarginL)
                            {
                                marginL = splitLine[i].Trim();
                            }
                            else if (i == indexMarginR)
                            {
                                marginR = splitLine[i].Trim();
                            }
                            else if (i == indexMarginV)
                            {
                                marginV = splitLine[i].Trim();
                            }
                            else if (i == indexName)
                            {
                                name = splitLine[i];
                            }
                            else if (i > indexText)
                            {
                                text += "," + splitLine[i];
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
                                errors.AppendLine(string.Format(FormatLanguage.LineNumberXErrorReadingTimeCodeFromSourceLineY, lineNumber, line));
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

            if (footer.Length > 0)
            {
                subtitle.Footer = footer.ToString().Trim();
            }

            subtitle.Renumber();
            Errors = errors.ToString();
        }

        private static TimeCode GetTimeCodeFromString(string time)
        {
            // h:mm:ss.cc
            var timeCode = time.Split(':', '.');
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
                foreach (var p in subtitle.Paragraphs)
                {
                    var indexOfBegin = p.Text.IndexOf('{');
                    var pre = string.Empty;
                    while (indexOfBegin >= 0 && p.Text.IndexOf('}') > indexOfBegin)
                    {
                        var s = p.Text.Substring(indexOfBegin);
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

                        var indexOfEnd = p.Text.IndexOf('}');
                        p.Text = p.Text.Remove(indexOfBegin, (indexOfEnd - indexOfBegin) + 1);

                        indexOfBegin = p.Text.IndexOf('{');
                    }

                    p.Text = pre + p.Text;
                }
            }
        }

        public override bool HasStyleSupport => true;

        public static string GetHeaderAndStylesFromAdvancedSubStationAlpha(string header, string title)
        {
            var scriptInfo = string.Empty;
            if (header != null && header.Contains("[Script Info]") && header.Contains("ScriptType: v4.00+"))
            {
                var sb = new StringBuilder();
                var scriptInfoOn = false;
                foreach (var line in header.SplitToLines())
                {
                    if (line.RemoveChar(' ').Contains("Styles]", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    if (line.Equals("[Script Info]", StringComparison.OrdinalIgnoreCase))
                    {
                        scriptInfoOn = true;
                    }

                    if (scriptInfoOn)
                    {
                        if (line.StartsWith("ScriptType:", StringComparison.OrdinalIgnoreCase))
                        {
                            sb.AppendLine("ScriptType: v4.00");
                        }
                        else if (line.Equals("; This is an Advanced Sub Station Alpha v4+ script.", StringComparison.OrdinalIgnoreCase))
                        {
                            sb.AppendLine("; This is a Sub Station Alpha v4 script.");
                        }
                        else
                        {
                            sb.AppendLine(line);
                        }
                    }
                }
                scriptInfo = sb.ToString();
            }

            var style = GetStyle(header);

            if (string.IsNullOrEmpty(scriptInfo) || string.IsNullOrEmpty(style))
            {
                var s = new Subtitle { Paragraphs = { new Paragraph("test", 0, 1000) } };
                new SubStationAlpha().ToText(s, string.Empty);
                return s.Header;
            }

            return string.Format($@"{scriptInfo.Trim() + Environment.NewLine}
[V4 Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding
{style.Trim() + Environment.NewLine}
[Events]");
        }

        public static string DefaultHeader
        {
            get
            {
                var format = new SubStationAlpha();
                var sub = new Subtitle();
                var text = format.ToText(sub, string.Empty);
                var lines = text.SplitToLines();
                format.LoadSubtitle(sub, lines, string.Empty);
                return sub.Header;
            }
        }
    }
}
