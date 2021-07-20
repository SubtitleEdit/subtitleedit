using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AdvancedSubStationAlpha : SubtitleFormat
    {
        public string Errors { get; private set; }

        public static string DefaultStyle
        {
            get
            {
                var defaultStyle = string.Empty;
                var defaultCategory = Configuration.Settings.SubtitleSettings.AssaStyleStorageCategories.SingleOrDefault(item => item.IsDefault);
                if (defaultCategory == null || defaultCategory.Styles.Count == 0)
                {
                    defaultStyle = new SsaStyle().ToRawAss();
                }
                else
                {
                    foreach (var defaultCategoryStyle in defaultCategory.Styles)
                    {
                        defaultStyle += defaultCategoryStyle.ToRawAss();
                        if (defaultCategory.Styles.IndexOf(defaultCategoryStyle) != defaultCategory.Styles.Count - 1)
                        {
                            defaultStyle += Environment.NewLine;
                        }
                    }
                }

                return defaultStyle;
            }
        }

        public static string DefaultHeader
        {
            get
            {
                var format = new AdvancedSubStationAlpha();
                var sub = new Subtitle();
                var text = format.ToText(sub, string.Empty);
                var lines = text.SplitToLines();
                format.LoadSubtitle(sub, lines, string.Empty);
                return sub.Header;
            }
        }

        public override bool HasStyleSupport => true;

        public override string Extension => ".ass";

        public const string NameOfFormat = "Advanced Sub Station Alpha";
        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var all = sb.ToString();
            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".ass", StringComparison.OrdinalIgnoreCase) && !all.Contains("[V4 Styles]"))
            {
            }
            else if (!all.Contains("dialog:", StringComparison.OrdinalIgnoreCase) && !all.Contains("dialogue:", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            else if (!all.Contains("[V4+ Styles]") && new SubStationAlpha().IsMine(lines, fileName))
            {
                return false;
            }

            LoadSubtitle(subtitle, lines, fileName);
            Errors = null;
            if (subtitle.Paragraphs.Count > _errorCount)
            {
                if (!string.IsNullOrEmpty(subtitle.Header))
                {
                    subtitle.Header = subtitle.Header.Replace("[V4 Styles]", "[V4+ Styles]");
                }

                return true;
            }

            return false;
        }

        public const string HeaderNoStyles = @"[Script Info]
; This is an Advanced Sub Station Alpha v4+ script.
Title: {0}
ScriptType: v4.00+
Collisions: Normal
PlayDepth: 0

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
{1}

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text";

        public override string ToText(Subtitle subtitle, string title)
        {
            bool fromTtml = false;
            string header = $@"[Script Info]
; This is an Advanced Sub Station Alpha v4+ script.
Title: {0}
ScriptType: v4.00+
Collisions: Normal
PlayDepth: 0

[V4+ Styles]
{SsaStyle.DefaultAssStyleFormat}
{DefaultStyle}

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text";

            const string timeCodeFormat = "{0}:{1:00}:{2:00}.{3:00}"; // h:mm:ss.cc
            const string paragraphWriteFormat = "Dialogue: {9},{0},{1},{3},{4},{5},{6},{7},{8},{2}";
            const string commentWriteFormat = "Comment: {9},{0},{1},{3},{4},{5},{6},{7},{8},{2}";

            var sb = new StringBuilder();
            var isValidAssHeader = !string.IsNullOrEmpty(subtitle.Header) && subtitle.Header.Contains("[V4+ Styles]");
            var styles = new List<string>();
            if (isValidAssHeader)
            {
                sb.AppendLine(subtitle.Header.Trim());
                sb.AppendLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");
                styles = GetStylesFromHeader(subtitle.Header);
            }
            else if (!string.IsNullOrEmpty(subtitle.Header) && subtitle.Header.Contains("[V4 Styles]"))
            {
                LoadStylesFromSubstationAlpha(subtitle, title, header, HeaderNoStyles, sb);
            }
            else if (subtitle.Header != null && subtitle.Header.Contains("http://www.w3.org/ns/ttml"))
            {
                LoadStylesFromTimedText10(subtitle, title, header, HeaderNoStyles, sb);
                fromTtml = true;
                isValidAssHeader = !string.IsNullOrEmpty(subtitle.Header) && subtitle.Header.Contains("[V4+ Styles]");
                if (isValidAssHeader)
                {
                    styles = GetStylesFromHeader(subtitle.Header);
                }
            }
            else if (subtitle.Header != null && subtitle.Header.Contains("http://www.w3.org/2006/10/ttaf1"))
            {
                LoadStylesFromTimedTextTimedDraft2006Oct(subtitle, title, header, HeaderNoStyles, sb);
                fromTtml = true;
                isValidAssHeader = !string.IsNullOrEmpty(subtitle.Header) && subtitle.Header.Contains("[V4+ Styles]");
                if (isValidAssHeader)
                {
                    styles = GetStylesFromHeader(subtitle.Header);
                }
            }
            else
            {
                sb.AppendFormat(header, title).AppendLine();
            }
            foreach (var p in subtitle.Paragraphs)
            {
                var start = string.Format(timeCodeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds / 10);
                var end = string.Format(timeCodeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds / 10);
                var style = "Default";
                if (!string.IsNullOrEmpty(p.Extra) && isValidAssHeader && styles.Contains(p.Extra))
                {
                    style = p.Extra;
                }

                if (fromTtml && !string.IsNullOrEmpty(p.Style) && isValidAssHeader && styles.Contains(p.Style))
                {
                    style = p.Style;
                }

                var actor = "";
                if (!string.IsNullOrEmpty(p.Actor))
                {
                    actor = p.Actor;
                }

                var marginL = "0";
                if (!string.IsNullOrEmpty(p.MarginL) && Utilities.IsInteger(p.MarginL))
                {
                    marginL = p.MarginL;
                }

                var marginR = "0";
                if (!string.IsNullOrEmpty(p.MarginR) && Utilities.IsInteger(p.MarginR))
                {
                    marginR = p.MarginR;
                }

                var marginV = "0";
                if (!string.IsNullOrEmpty(p.MarginV) && Utilities.IsInteger(p.MarginV))
                {
                    marginV = p.MarginV;
                }

                var effect = "";
                if (!string.IsNullOrEmpty(p.Effect))
                {
                    effect = p.Effect;
                }

                if (p.IsComment)
                {
                    sb.AppendFormat(commentWriteFormat, start, end, FormatText(p), style, actor, marginL, marginR, marginV, effect, p.Layer).AppendLine();
                }
                else
                {
                    sb.AppendFormat(paragraphWriteFormat, start, end, FormatText(p), style, actor, marginL, marginR, marginV, effect, p.Layer).AppendLine();
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

        public static string GetHeaderAndStylesFromSubStationAlpha(string header)
        {
            var scriptInfo = string.Empty;
            if (header != null &&
                header.Contains("[Script Info]") &&
                header.Contains("ScriptType: v4.00") &&
                !header.Contains("ScriptType: v4.00+"))
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
                            sb.AppendLine("ScriptType: v4.00+");
                        }
                        else if (line.Equals("; This is a Sub Station Alpha v4 script.", StringComparison.OrdinalIgnoreCase))
                        {
                            sb.AppendLine("; This is an Advanced Sub Station Alpha v4+ script.");
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
                return DefaultHeader;
            }

            return string.Format($@"{scriptInfo.Trim() + Environment.NewLine}
[V4+ Styles]
{SsaStyle.DefaultAssStyleFormat}
{style.Trim() + Environment.NewLine}
[Events]");
        }

        public static string GetHeaderAndStylesFromAdvancedSubStationAlpha(string header, List<SsaStyle> styles)
        {
            var scriptInfo = string.Empty;
            if (header != null &&
                header.Contains("[Script Info]") &&
                header.Contains("ScriptType: v4.00+"))
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
                            sb.AppendLine("ScriptType: v4.00+");
                        }
                        else if (line.Equals("; This is a Sub Station Alpha v4 script.", StringComparison.OrdinalIgnoreCase))
                        {
                            sb.AppendLine("; This is an Advanced Sub Station Alpha v4+ script.");
                        }
                        else
                        {
                            sb.AppendLine(line);
                        }
                    }
                }
                scriptInfo = sb.ToString();
            }

            var style = new StringBuilder();
            foreach (var ssaStyle in styles)
            {
                style.AppendLine(ssaStyle.ToRawAss());
            }

            if (string.IsNullOrEmpty(scriptInfo) || style.Length == 0)
            {
                return DefaultHeader;
            }

            return string.Format($@"{scriptInfo.Trim() + Environment.NewLine}
[V4+ Styles]
{SsaStyle.DefaultAssStyleFormat}
{style.ToString().Trim() + Environment.NewLine}
[Events]");
        }

        private static void LoadStylesFromSubstationAlpha(Subtitle subtitle, string title, string header, string headerNoStyles, StringBuilder sb)
        {
            try
            {
                var ttStyles = GetStyle(subtitle.Header);
                if (!string.IsNullOrEmpty(ttStyles))
                {
                    sb.AppendFormat(headerNoStyles, title, ttStyles).AppendLine();
                    subtitle.Header = sb.ToString();
                }
                else
                {
                    sb.AppendFormat(header, title).AppendLine();
                }
            }
            catch
            {
                sb.AppendFormat(header, title).AppendLine();
            }
        }

        private static string GetStyle(string header)
        {
            var ttStyles = new StringBuilder();
            foreach (var styleName in GetStylesFromHeader(header))
            {
                try
                {
                    var ssaStyle = GetSsaStyle(styleName, header);

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

                    var underline = "0";
                    if (ssaStyle.Underline)
                    {
                        underline = "-1";
                    }

                    var scaleX = ssaStyle.ScaleX.ToString(CultureInfo.InvariantCulture);
                    var scaleY = ssaStyle.ScaleY.ToString(CultureInfo.InvariantCulture);
                    var spacing = ssaStyle.Spacing.ToString(CultureInfo.InvariantCulture);
                    var angle = ssaStyle.Angle.ToString(CultureInfo.InvariantCulture);

                    var newAlignment = "2";
                    switch (ssaStyle.Alignment)
                    {
                        case "1":
                            newAlignment = "1";
                            break;
                        case "3":
                            newAlignment = "3";
                            break;
                        case "9":
                            newAlignment = "4";
                            break;
                        case "10":
                            newAlignment = "5";
                            break;
                        case "11":
                            newAlignment = "6";
                            break;
                        case "5":
                            newAlignment = "7";
                            break;
                        case "6":
                            newAlignment = "8";
                            break;
                        case "7":
                            newAlignment = "9";
                            break;
                    }

                    ttStyles.Append("Style: ").Append(ssaStyle.Name).Append(',').Append(ssaStyle.FontName).Append(',').Append(ssaStyle.FontSize).Append(',').Append(GetSsaColorString(ssaStyle.Primary)).Append(',').Append(GetSsaColorString(ssaStyle.Secondary)).Append(',').Append(GetSsaColorString(ssaStyle.Outline)).Append(',').Append(GetSsaColorString(ssaStyle.Background)).Append(',').Append(bold).Append(',').Append(italic).Append(',').Append(underline).Append(",0,").Append(scaleX).Append(',').Append(scaleY).Append(',').Append(spacing).Append(',').Append(angle).Append(',').Append(ssaStyle.BorderStyle).Append(',').Append(ssaStyle.OutlineWidth.ToString(CultureInfo.InvariantCulture)).Append(',').Append(ssaStyle.ShadowWidth.ToString(CultureInfo.InvariantCulture)).Append(',').Append(newAlignment).Append(',').Append(ssaStyle.MarginLeft).Append(',').Append(ssaStyle.MarginRight).Append(',').Append(ssaStyle.MarginVertical).AppendLine(",1");
                }
                catch
                {
                    // ignored
                }
            }

            return ttStyles.ToString();
        }

        public static void LoadStylesFromTimedText10(Subtitle subtitle, string title, string header, string headerNoStyles, StringBuilder sb)
        {
            foreach (var p in subtitle.Paragraphs)
            {
                p.Effect = null;
            }

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
                var styleCount = 0;
                var ttStyles = new StringBuilder();
                var styleNames = new List<string>();
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
                        styleCount++;

                        var fontFamily = "Arial";
                        if (node.Attributes["tts:fontFamily"]?.Value != null)
                        {
                            fontFamily = node.Attributes["tts:fontFamily"].Value;
                            if (fontFamily.Contains(","))
                            {
                                fontFamily = fontFamily.Split(',')[0];
                            }
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

                        if (!float.TryParse(fontSize, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var fSize))
                        {
                            fSize = 20;
                        }

                        var italic = "0";
                        if (fontStyle == "italic")
                        {
                            italic = "-1";
                        }

                        var bold = "0";
                        if (fontWeight == "bold")
                        {
                            bold = "-1";
                        }

                        const string styleFormat = "Style: {0},{1},{2},{3},&H0300FFFF,&H00000000,&H02000000,{4},{5},0,0,100,100,0,0,1,2,2,2,10,10,10,1";
                        ttStyles.AppendFormat(styleFormat, name, fontFamily, fSize, GetSsaColorString(c), bold, italic).AppendLine();
                        styleNames.Add(name);
                    }
                }

                if (styleCount > 0)
                {
                    if (!styleNames.Contains("Default") && !styleNames.Contains("default") && subtitle.Paragraphs.Any(pa => string.IsNullOrEmpty(pa.Extra)))
                    {
                        ttStyles = new StringBuilder(DefaultStyle + Environment.NewLine + ttStyles);
                        foreach (var paragraph in subtitle.Paragraphs)
                        {
                            if (string.IsNullOrEmpty(paragraph.Extra))
                            {
                                paragraph.Extra = "Default";
                            }
                        }
                    }
                    sb.AppendFormat(headerNoStyles, title, ttStyles).AppendLine();
                    subtitle.Header = sb.ToString();
                }
                else
                {
                    sb.AppendFormat(header, title).AppendLine();
                }

                // Set correct style on paragraphs
                foreach (var p in subtitle.Paragraphs)
                {
                    if (p.Extra?.Contains('/') == true)
                    {
                        p.Extra = p.Extra.Split('/')[0].Trim();
                    }
                }
            }
            catch
            {
                sb.AppendFormat(header, title).AppendLine();
            }
        }

        public static void LoadStylesFromTimedTextTimedDraft2006Oct(Subtitle subtitle, string title, string header, string headerNoStyles, StringBuilder sb)
        {
            foreach (var p in subtitle.Paragraphs)
            {
                p.Effect = null;
            }

            try
            {
                var lines = subtitle.Header.SplitToLines();
                var sb2 = new StringBuilder();
                lines.ForEach(line => sb2.AppendLine(line));
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(sb2.ToString().Replace(" & ", " &amp; ").Replace("Q&A", "Q&amp;A").RemoveControlCharactersButWhiteSpace().Trim());
                subtitle.Header = xml.OuterXml;
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ttaf1", xml.DocumentElement.NamespaceURI);
                int styleCount = 0;
                var ttStyles = new StringBuilder();
                var styleNames = new List<string>();
                foreach (XmlNode node in xml.DocumentElement.SelectNodes("//ttaf1:style", nsmgr))
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
                        styleCount++;

                        var fontFamily = "Arial";
                        if (node.Attributes["tts:fontFamily"]?.Value != null)
                        {
                            fontFamily = node.Attributes["tts:fontFamily"].Value;
                            if (fontFamily.Contains(","))
                            {
                                fontFamily = fontFamily.Split(',')[0];
                            }
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

                        if (!float.TryParse(fontSize, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var fSize))
                        {
                            fSize = 20;
                        }

                        var italic = "0";
                        if (fontStyle == "italic")
                        {
                            italic = "-1";
                        }

                        var bold = "0";
                        if (fontWeight == "bold")
                        {
                            bold = "-1";
                        }

                        const string styleFormat = "Style: {0},{1},{2},{3},&H0300FFFF,&H00000000,&H02000000,{4},{5},0,0,100,100,0,0,1,2,2,2,10,10,10,1";
                        ttStyles.AppendFormat(styleFormat, name, fontFamily, fSize, GetSsaColorString(c), bold, italic).AppendLine();
                        styleNames.Add(name);
                    }
                }

                if (styleCount > 0)
                {
                    if (!styleNames.Contains("Default") && !styleNames.Contains("default") && subtitle.Paragraphs.Any(pa => string.IsNullOrEmpty(pa.Extra)))
                    {
                        ttStyles = new StringBuilder(DefaultStyle + Environment.NewLine + ttStyles);
                        foreach (var paragraph in subtitle.Paragraphs)
                        {
                            if (string.IsNullOrEmpty(paragraph.Extra))
                            {
                                paragraph.Extra = "Default";
                            }
                        }
                    }
                    sb.AppendFormat(headerNoStyles, title, ttStyles).AppendLine();
                    subtitle.Header = sb.ToString();
                }
                else
                {
                    sb.AppendFormat(header, title).AppendLine();
                }

                // Set correct style on paragraphs
                foreach (var p in subtitle.Paragraphs)
                {
                    if (p.Extra?.Contains('/') == true)
                    {
                        p.Extra = p.Extra.Split('/')[0].Trim();
                    }
                }
            }
            catch
            {
                sb.AppendFormat(header, title).AppendLine();
            }
        }

        public static string GetTagFromHeader(string tagName, string section, string header)
        {
            var sectionOn = false;
            foreach (var line in header.SplitToLines())
            {
                var s = line.Trim();
                if (s.StartsWith('['))
                {
                    if (s.Equals(section, StringComparison.OrdinalIgnoreCase))
                    {
                        sectionOn = true;
                    }
                }
                else if (sectionOn &&
                         s.StartsWith(tagName, StringComparison.OrdinalIgnoreCase) &&
                         s.RemoveChar(' ').StartsWith(tagName + ":", StringComparison.OrdinalIgnoreCase))
                {
                    return s;
                }
            }

            return null;
        }

        public static string GetTagValueFromHeader(string tagName, string section, string header)
        {
            var sectionOn = false;
            foreach (var line in header.SplitToLines())
            {
                var s = line.Trim();
                if (s.StartsWith('['))
                {
                    if (s.Equals(section, StringComparison.OrdinalIgnoreCase))
                    {
                        sectionOn = true;
                    }
                }
                else if (sectionOn &&
                         s.StartsWith(tagName, StringComparison.OrdinalIgnoreCase) &&
                         s.RemoveChar(' ').StartsWith(tagName + ":", StringComparison.OrdinalIgnoreCase))
                {

                    return s.Remove(0, s.IndexOf(':') + 1).Trim();
                }
            }

            return null;
        }

        /// <summary>
        /// Add tag to header.
        /// </summary>
        /// <param name="tagName">Tag to add</param>
        /// <param name="tagAndValue">Tag name + value of tag, e.g. "SelectedLines: 0,1,2"</param>
        /// <param name="section">Section name - include square brackets, e.g. "[Script Info]"</param>
        /// <param name="header">Header to update</param>
        /// <returns>Header with updated section</returns>
        public static string AddTagToHeader(string tagName, string tagAndValue, string section, string header)
        {
            var sectionOn = false;
            var sb = new StringBuilder();
            var added = false;
            foreach (var line in header.SplitToLines())
            {
                var s = line.Trim();
                if (s.StartsWith('['))
                {
                    if (s.Equals(section, StringComparison.OrdinalIgnoreCase))
                    {
                        sectionOn = true;
                    }
                    else if (sectionOn)
                    {
                        if (!added)
                        {
                            sb.AppendLine(tagAndValue);
                            sb.AppendLine();
                            added = true;
                        }

                        sectionOn = false;
                    }
                }
                else if (sectionOn && !added &&
                         s.StartsWith(tagName, StringComparison.OrdinalIgnoreCase) &&
                         s.RemoveChar(' ').StartsWith(tagName + ":", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine(tagAndValue);
                    added = true;
                    continue;
                }
                else if (sectionOn && !added && s.Length == 0)
                {
                    sb.AppendLine(tagAndValue);
                    sb.AppendLine();
                    added = true;
                    continue;
                }

                sb.AppendLine(line);
            }

            if (!added)
            {
                sb.AppendLine(tagAndValue);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Remove tag from header.
        /// </summary>
        /// <param name="tagName">Tag to remove</param>
        /// <param name="section">Section name - include square brackets, e.g. "[Script Info]"</param>
        /// <param name="header">Header to update</param>
        /// <returns>Header with updated section</returns>
        public static string RemoveTagFromHeader(string tagName, string section, string header)
        {
            var sectionOn = false;
            var sb = new StringBuilder();
            foreach (var line in header.SplitToLines())
            {
                var s = line.Trim();
                if (s.StartsWith('['))
                {
                    if (s.Equals(section, StringComparison.OrdinalIgnoreCase))
                    {
                        sectionOn = true;
                    }
                    else if (sectionOn)
                    {
                        sectionOn = false;
                    }
                }
                else if (sectionOn &&
                         s.StartsWith(tagName, StringComparison.OrdinalIgnoreCase) &&
                         s.RemoveChar(' ').StartsWith(tagName + ":", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                sb.AppendLine(line);
            }

            return sb.ToString();
        }

        public static List<SsaStyle> GetSsaStylesFromHeader(string header)
        {
            var styles = new List<SsaStyle>();
            foreach (var styleName in GetStylesFromHeader(header))
            {
                styles.Add(AdvancedSubStationAlpha.GetSsaStyle(styleName, header));
            }
            return styles;
        }

        public static List<string> GetStylesFromHeader(string headerLines)
        {
            var list = new List<string>();

            if (headerLines == null)
            {
                headerLines = DefaultStyle;
            }

            if (headerLines.Contains("http://www.w3.org/ns/ttml"))
            {
                var subtitle = new Subtitle { Header = headerLines };
                LoadStylesFromTimedText10(subtitle, string.Empty, headerLines, HeaderNoStyles, new StringBuilder());
                headerLines = subtitle.Header;
            }

            foreach (var line in headerLines.SplitToLines())
            {
                if (line.StartsWith("style:", StringComparison.OrdinalIgnoreCase))
                {
                    var end = line.IndexOf(',');
                    if (end > 0)
                    {
                        list.Add(line.Substring(6, end - 6).Trim());
                    }
                }
            }

            return list;
        }

        public static string FormatText(Paragraph p)
        {
            string text = p.Text.Replace(Environment.NewLine, "\\N");

            if (!text.Contains('<'))
            {
                return text;
            }

            text = text.Replace("<i>", @"{\i1}");
            text = text.Replace("</i>", @"{\i0}");
            text = text.Replace("</i>", @"{\i}");
            text = text.Replace("<u>", @"{\u1}");
            text = text.Replace("</u>", @"{\u0}");
            text = text.Replace("</u>", @"{\u}");
            text = text.Replace("<b>", @"{\b1}");
            text = text.Replace("</b>", @"{\b0}");
            text = text.Replace("</b>", @"{\b}");
            var count = 0;
            while (text.Contains("<font ") && count < 10)
            {
                var start = text.IndexOf("<font ", StringComparison.Ordinal);
                var end = text.IndexOf('>', start);
                if (end > 0)
                {
                    var fontTag = text.Substring(start + 5, end - (start + 4));
                    text = text.Remove(start, end - start + 1);
                    var indexOfEndFont = text.IndexOf("</font>", start, StringComparison.Ordinal);
                    if (indexOfEndFont > 0)
                    {
                        text = text.Remove(indexOfEndFont, 7);
                        if (indexOfEndFont < text.Length)
                        {
                            if (fontTag.Contains(" size="))
                            {
                                text = text.Insert(indexOfEndFont, "{\\fs}");
                            }
                            if (fontTag.Contains(" face="))
                            {
                                text = text.Insert(indexOfEndFont, "{\\fn}");
                            }
                            if (fontTag.Contains(" color="))
                            {
                                text = text.Insert(indexOfEndFont, "{\\c}");
                            }
                        }
                    }

                    fontTag = FormatTag(ref text, start, fontTag, "face=\"", "fn", "}");
                    fontTag = FormatTag(ref text, start, fontTag, "face='", "fn", "}");
                    fontTag = FormatTag(ref text, start, fontTag, "face=", "fn", "}");

                    fontTag = FormatTag(ref text, start, fontTag, "size=\"", "fs", "}");
                    fontTag = FormatTag(ref text, start, fontTag, "size='", "fs", "}");
                    fontTag = FormatTag(ref text, start, fontTag, "size=", "fs", "}");

                    fontTag = FormatTag(ref text, start, fontTag, "color=\"", "c&H", "&}");
                    fontTag = FormatTag(ref text, start, fontTag, "color='", "c&H", "&}");
                    FormatTag(ref text, start, fontTag, "color=", "c&H", "&}");
                }
                count++;
            }
            text = text.Replace("{\\c}", "@___@@").Replace("}{", string.Empty).Replace("@___@@", "{\\c}").Replace("{\\c}{\\c&", "{\\c&");
            while (text.EndsWith("{\\c}", StringComparison.Ordinal))
            {
                text = text.Remove(text.Length - 4);
            }
            while (text.Contains("\\fs\\fs", StringComparison.Ordinal))
            {
                text = text.Replace("\\fs\\fs", "\\fs");
            }
            while (text.Contains("\\fn\\fn", StringComparison.Ordinal))
            {
                text = text.Replace("\\fn\\fn", "\\fn");
            }
            while (text.Contains("\\c\\c&H", StringComparison.Ordinal))
            {
                text = text.Replace("\\c\\c&H", "\\c&H");
            }
            return text;
        }

        private static string FormatTag(ref string text, int start, string fontTag, string tag, string ssaTagName, string endSsaTag)
        {
            if (fontTag.Contains(tag))
            {
                var fontStart = fontTag.IndexOf(tag, StringComparison.Ordinal);

                var fontEnd = fontTag.IndexOfAny(new[] { '"', '\'' }, fontStart + tag.Length);
                if (fontEnd < 0)
                {
                    fontEnd = fontTag.IndexOfAny(new[] { ' ', '>' }, fontStart + tag.Length);
                }

                if (fontEnd > 0)
                {
                    var subTag = fontTag.Substring(fontStart + tag.Length, fontEnd - (fontStart + tag.Length));
                    if (tag.Contains("color"))
                    {
                        var c = HtmlUtil.GetColorFromString(subTag);
                        subTag = (c.B.ToString("X2") + c.G.ToString("X2") + c.R.ToString("X2")).ToLowerInvariant(); // use bbggrr
                    }
                    fontTag = fontTag.Remove(fontStart, fontEnd - fontStart + 1);
                    if (start < text.Length)
                    {
                        text = text.Insert(start, @"{\" + ssaTagName + subTag + endSsaTag);
                    }
                }
            }
            return fontTag;
        }

        public static string GetFormattedText(string input)
        {
            var text = input.Replace("\\N", Environment.NewLine).Replace("\\n", Environment.NewLine);

            var tooComplex = ContainsUnsupportedTags(text);

            if (!tooComplex)
            {
                for (var i = 0; i < 10; i++) // just look ten times...
                {
                    bool italic;
                    if (text.Contains(@"{\i1\"))
                    {
                        var start = text.IndexOf(@"{\i1\", StringComparison.Ordinal);
                        text = text.Remove(start, 4).Insert(start, "<i>{");
                    }

                    if (text.Contains(@"{\fn"))
                    {
                        var start = text.IndexOf(@"{\fn", StringComparison.Ordinal);
                        var end = text.IndexOf('}', start);
                        if (end > 0 && !text.Substring(start).StartsWith("{\\fn}", StringComparison.Ordinal))
                        {
                            var fontName = text.Substring(start + 4, end - (start + 4));
                            var extraTags = string.Empty;
                            CheckAndAddSubTags(ref fontName, ref extraTags, out var unknownTags, out italic);
                            text = text.Remove(start, end - start + 1);
                            if (italic)
                            {
                                text = text.Insert(start, "<font face=\"" + fontName + "\"" + extraTags + ">" + unknownTags + "<i>");
                            }
                            else
                            {
                                text = text.Insert(start, "<font face=\"" + fontName + "\"" + extraTags + ">" + unknownTags);
                            }

                            int indexOfEndTag = text.IndexOf("{\\fn}", start, StringComparison.Ordinal);
                            if (indexOfEndTag > 0)
                            {
                                text = text.Remove(indexOfEndTag, "{\\fn}".Length).Insert(indexOfEndTag, "</font>");
                            }
                            else
                            {
                                var indexOfNextTag1 = text.IndexOf("{\\fn", start, StringComparison.Ordinal);
                                var indexOfNextTag2 = text.IndexOf("{\\c}", start, StringComparison.Ordinal);
                                if (indexOfNextTag1 > 0)
                                {
                                    text = text.Insert(indexOfNextTag1, "</font>");
                                }
                                else if (indexOfNextTag2 > 0 && text.IndexOf("{\\", start, StringComparison.Ordinal) >= indexOfNextTag2)
                                {
                                    text = text.Insert(indexOfNextTag2, "</font>");
                                }
                                else
                                {
                                    text += "</font>";
                                }
                            }
                        }
                    }

                    if (text.Contains(@"{\fs"))
                    {
                        var start = text.IndexOf(@"{\fs", StringComparison.Ordinal);
                        var end = text.IndexOf('}', start);
                        if (end > 0 && !text.Substring(start).StartsWith("{\\fs}", StringComparison.Ordinal))
                        {
                            var fontSize = text.Substring(start + 4, end - (start + 4));
                            var extraTags = string.Empty;
                            CheckAndAddSubTags(ref fontSize, ref extraTags, out var unknownTags, out italic);
                            if (float.TryParse(fontSize, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _))
                            {
                                text = text.Remove(start, end - start + 1);
                                if (italic)
                                {
                                    text = text.Insert(start, "<font size=\"" + fontSize + "\"" + extraTags + ">" + unknownTags + "<i>");
                                }
                                else
                                {
                                    text = text.Insert(start, "<font size=\"" + fontSize + "\"" + extraTags + ">" + unknownTags);
                                }

                                int indexOfEndTag = text.IndexOf("{\\fs}", start, StringComparison.Ordinal);
                                if (indexOfEndTag > 0)
                                {
                                    text = text.Remove(indexOfEndTag, "{\\fs}".Length).Insert(indexOfEndTag, "</font>");
                                }
                                else
                                {
                                    int indexOfNextTag1 = text.IndexOf("{\\fs", start, StringComparison.Ordinal);
                                    int indexOfNextTag2 = text.IndexOf("{\\c}", start, StringComparison.Ordinal);
                                    if (indexOfNextTag1 > 0)
                                    {
                                        text = text.Insert(indexOfNextTag1, "</font>");
                                    }
                                    else if (indexOfNextTag2 > 0 && text.IndexOf("{\\", start, StringComparison.Ordinal) >= indexOfNextTag2)
                                    {
                                        text = text.Insert(indexOfNextTag2, "</font>");
                                    }
                                    else
                                    {
                                        text += "</font>";
                                    }
                                }
                            }
                        }
                    }

                    if (text.Contains(@"{\c"))
                    {
                        var start = text.IndexOf(@"{\c", StringComparison.Ordinal);
                        var end = text.IndexOf('}', start);
                        if (end > 0 && !text.Substring(start).StartsWith("{\\c}", StringComparison.Ordinal) && !text.Substring(start).StartsWith("{\\clip", StringComparison.Ordinal))
                        {
                            var color = text.Substring(start + 4, end - (start + 4));
                            var extraTags = string.Empty;
                            CheckAndAddSubTags(ref color, ref extraTags, out var unknownTags, out italic);

                            color = color.RemoveChar('&').TrimStart('H');
                            color = color.PadLeft(6, '0');

                            // switch to rrggbb from bbggrr
                            color = "#" + color.Remove(color.Length - 6) + color.Substring(color.Length - 2, 2) + color.Substring(color.Length - 4, 2) + color.Substring(color.Length - 6, 2);
                            color = color.ToLowerInvariant();

                            text = text.Remove(start, end - start + 1);
                            if (italic)
                            {
                                text = text.Insert(start, "<font color=\"" + color + "\"" + extraTags + ">" + unknownTags + "<i>");
                            }
                            else
                            {
                                text = text.Insert(start, "<font color=\"" + color + "\"" + extraTags + ">" + unknownTags);
                            }

                            var indexOfEndTag = text.IndexOf("{\\c}", start, StringComparison.Ordinal);
                            var indexOfNextColorTag = text.IndexOf("{\\c&", start, StringComparison.Ordinal);
                            if (indexOfNextColorTag > 0 && (indexOfNextColorTag < indexOfEndTag || indexOfEndTag == -1))
                            {
                                text = text.Insert(indexOfNextColorTag, "</font>");
                            }
                            else if (indexOfEndTag > 0)
                            {
                                text = text.Remove(indexOfEndTag, "{\\c}".Length).Insert(indexOfEndTag, "</font>");
                            }
                            else
                            {
                                text += "</font>";
                            }
                        }
                    }

                    if (text.Contains(@"{\1c")) // "1" specifices primary color
                    {
                        var start = text.IndexOf(@"{\1c", StringComparison.Ordinal);
                        var end = text.IndexOf('}', start);
                        if (end > 0 && !text.Substring(start).StartsWith("{\\1c}", StringComparison.Ordinal))
                        {
                            var color = text.Substring(start + 5, end - (start + 5));
                            var extraTags = string.Empty;
                            CheckAndAddSubTags(ref color, ref extraTags, out var unknownTags, out italic);

                            color = color.RemoveChar('&').TrimStart('H');
                            color = color.PadLeft(6, '0');

                            // switch to rrggbb from bbggrr
                            color = "#" + color.Remove(color.Length - 6) + color.Substring(color.Length - 2, 2) + color.Substring(color.Length - 4, 2) + color.Substring(color.Length - 6, 2);
                            color = color.ToLowerInvariant();

                            text = text.Remove(start, end - start + 1);
                            if (italic)
                            {
                                text = text.Insert(start, "<font color=\"" + color + "\"" + extraTags + ">" + unknownTags + "<i>");
                            }
                            else
                            {
                                text = text.Insert(start, "<font color=\"" + color + "\"" + extraTags + ">" + unknownTags);
                            }

                            var indexOfEndTag = text.IndexOf("{\\1c}", start, StringComparison.Ordinal);
                            var indexOfNextColorTag = text.IndexOf("{\\1c&", start, StringComparison.Ordinal);
                            if (indexOfNextColorTag > 0 && (indexOfNextColorTag < indexOfEndTag || indexOfEndTag == -1))
                            {
                                text = text.Insert(indexOfNextColorTag, "</font>");
                            }
                            else if (indexOfEndTag > 0)
                            {
                                text = text.Remove(indexOfEndTag, "{\\1c}".Length).Insert(indexOfEndTag, "</font>");
                            }
                            else
                            {
                                text += "</font>";
                            }
                        }
                    }
                }
            }

            text = text.Replace(@"{\i1}", "<i>");
            text = text.Replace(@"{\i0}", "</i>");
            text = text.Replace(@"{\i}", "</i>");
            if (Utilities.CountTagInText(text, "<i>") > Utilities.CountTagInText(text, "</i>"))
            {
                text += "</i>";
            }

            text = text.Replace(@"{\u1}", "<u>");
            text = text.Replace(@"{\u0}", "</u>");
            text = text.Replace(@"{\u}", "</u>");
            if (Utilities.CountTagInText(text, "<u>") > Utilities.CountTagInText(text, "</u>"))
            {
                text += "</u>";
            }

            text = text.Replace(@"{\b1}", "<b>");
            text = text.Replace(@"{\b0}", "</b>");
            text = text.Replace(@"{\b}", "</b>");
            if (Utilities.CountTagInText(text, "<b>") > Utilities.CountTagInText(text, "</b>"))
            {
                text += "</b>";
            }

            return text;
        }

        private static bool ContainsUnsupportedTags(string text)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains("{\\", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var unsupportedTags = new List<string>
            {
                "\\alpha",
                "\\be0",
                "\\be1",
                "\\bord",
                "\\blur",
                "\\clip",
                "\\fad",
                "\\fa",
                "\\fade",
                "\\fscx",
                "\\fscy",
                "\\fr",
                "\\iclip",
                "\\k",
                "\\K",
                "\\kf",
                "\\ko",
                "\\move",
                "\\org",
                "\\p",
                "\\pos",
                "\\s0",
                "\\s1",
                "\\t(",
                "\\xbord",
                "\\ybord",
                "\\xshad",
                "\\yshad"
            };

            foreach (var unsupportedTag in unsupportedTags)
            {
                if (text.Contains(unsupportedTag, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void CheckAndAddSubTags(ref string tagName, ref string extraTags, out string unknownTags, out bool italic)
        {
            italic = false;
            unknownTags = string.Empty;
            var indexOfSplit = tagName.IndexOf('\\');
            if (indexOfSplit > 0)
            {
                var rest = tagName.Substring(indexOfSplit).TrimStart('\\');
                tagName = tagName.Remove(indexOfSplit);

                for (int i = 0; i < 10; i++)
                {
                    if (rest.StartsWith("fs", StringComparison.Ordinal) && rest.Length > 2)
                    {
                        indexOfSplit = rest.IndexOf('\\');
                        var fontSize = rest;
                        if (indexOfSplit > 0)
                        {
                            fontSize = rest.Substring(0, indexOfSplit);
                            rest = rest.Substring(indexOfSplit).TrimStart('\\');
                        }
                        else
                        {
                            rest = string.Empty;
                        }
                        extraTags += " size=\"" + fontSize.Substring(2) + "\"";
                    }
                    else if (rest.StartsWith("fn", StringComparison.Ordinal) && rest.Length > 2)
                    {
                        indexOfSplit = rest.IndexOf('\\');
                        var fontName = rest;
                        if (indexOfSplit > 0)
                        {
                            fontName = rest.Substring(0, indexOfSplit);
                            rest = rest.Substring(indexOfSplit).TrimStart('\\');
                        }
                        else
                        {
                            rest = string.Empty;
                        }
                        extraTags += " face=\"" + fontName.Substring(2) + "\"";
                    }
                    else if (rest.StartsWith('c') && rest.Length > 2)
                    {
                        indexOfSplit = rest.IndexOf('\\');
                        var fontColor = rest;
                        if (indexOfSplit > 0)
                        {
                            fontColor = rest.Substring(0, indexOfSplit);
                            rest = rest.Substring(indexOfSplit).TrimStart('\\');
                        }
                        else
                        {
                            rest = string.Empty;
                        }

                        var color = fontColor.Substring(2);
                        color = color.RemoveChar('&').TrimStart('H');
                        color = color.PadLeft(6, '0');
                        // switch to rrggbb from bbggrr
                        color = "#" + color.Remove(color.Length - 6) + color.Substring(color.Length - 2, 2) + color.Substring(color.Length - 4, 2) + color.Substring(color.Length - 6, 2);
                        color = color.ToLowerInvariant();

                        extraTags += " color=\"" + color + "\"";
                    }
                    else if (rest.StartsWith("i1", StringComparison.Ordinal) && rest.Length > 1)
                    {
                        indexOfSplit = rest.IndexOf('\\');
                        italic = true;
                        if (indexOfSplit > 0)
                        {
                            rest = rest.Substring(indexOfSplit).TrimStart('\\');
                        }
                        else
                        {
                            rest = string.Empty;
                        }
                    }
                    else if (rest.Length > 0 && rest.Contains("\\"))
                    {
                        indexOfSplit = rest.IndexOf('\\');
                        var unknownTag = rest.Substring(0, indexOfSplit);
                        unknownTags += "\\" + unknownTag;
                        rest = rest.Substring(indexOfSplit).TrimStart('\\');
                    }
                    else if (!string.IsNullOrEmpty(rest))
                    {
                        unknownTags += "\\" + rest;
                        rest = string.Empty;
                    }
                }
            }
            if (!string.IsNullOrEmpty(unknownTags))
            {
                unknownTags = "{" + unknownTags + "}";
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

            // Layer, Start, End, Style, Actor, MarginL, MarginR, MarginV, Effect, Text
            var indexLayer = 0;
            var indexStart = 1;
            var indexEnd = 2;
            var indexStyle = 3;
            var indexActor = -1;  // convert "Actor" to "Nam" (if no "Name")
            var indexName = 4;
            var indexMarginL = 5;
            var indexMarginR = 6;
            var indexMarginV = 7;
            var indexEffect = 8;
            var indexText = 9;
            var errors = new StringBuilder();
            var lineNumber = 0;

            var header = new StringBuilder();
            var footer = new StringBuilder();
            foreach (var line in lines)
            {
                lineNumber++;
                if (!eventsStarted && !fontsStarted && !graphicsStarted &&
                    !line.Trim().Equals("[fonts]", StringComparison.InvariantCultureIgnoreCase) &&
                    !line.Trim().Equals("[graphics]", StringComparison.InvariantCultureIgnoreCase))
                {
                    header.AppendLine(line);
                }

                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith(';'))
                {
                    // skip empty and comment lines
                }
                else if (line.TrimStart().StartsWith("dialog:", StringComparison.OrdinalIgnoreCase) || line.TrimStart().StartsWith("dialogue:", StringComparison.OrdinalIgnoreCase)) // fix faulty font tags...
                {
                    eventsStarted = true;
                    fontsStarted = false;
                    graphicsStarted = false;
                }

                if (line.Trim().Equals("[events]", StringComparison.OrdinalIgnoreCase))
                {
                    if (header.ToString().IndexOf(Environment.NewLine + "[events]", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        var h = header.ToString().TrimEnd();
                        header.Clear();
                        header.AppendLine(h);
                        header.AppendLine();
                        header.AppendLine("[Events]");
                    }
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
                else if (line.Trim().Equals("[Aegisub Extradata]", StringComparison.OrdinalIgnoreCase))
                {
                    eventsStarted = false;
                    fontsStarted = false;
                    graphicsStarted = true;
                    footer.AppendLine();
                    footer.AppendLine("[Aegisub Extradata]");
                }
                else if (fontsStarted)
                {
                    footer.AppendLine(line);
                }
                else if (graphicsStarted)
                {
                    footer.AppendLine(line);
                }
                else if (eventsStarted)
                {
                    string s = line.Trim().ToLowerInvariant();
                    if (line.Length > 10 && s.StartsWith("format:", StringComparison.Ordinal))
                    {
                        indexLayer = -1;
                        indexStart = -1;
                        indexEnd = -1;
                        indexStyle = -1;
                        indexActor = -1;
                        indexName = -1;
                        indexMarginL = -1;
                        indexMarginR = -1;
                        indexMarginV = -1;
                        indexEffect = -1;
                        indexText = -1;

                        var format = s.Substring(8).Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            var formatTrimmed = format[i].Trim();
                            if (formatTrimmed.Equals("start", StringComparison.Ordinal))
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
                            else if (formatTrimmed.Equals("style", StringComparison.Ordinal))
                            {
                                indexStyle = i;
                            }
                            else if (formatTrimmed.Equals("actor", StringComparison.Ordinal))
                            {
                                indexActor = i;
                            }
                            else if (formatTrimmed.Equals("name", StringComparison.Ordinal))
                            {
                                indexName = i;
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
                            else if (formatTrimmed.Equals("effect", StringComparison.Ordinal))
                            {
                                indexEffect = i;
                            }
                            else if (formatTrimmed.Equals("layer", StringComparison.Ordinal))
                            {
                                indexLayer = i;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(s))
                    {
                        var text = string.Empty;
                        var start = string.Empty;
                        var end = string.Empty;
                        var style = string.Empty;
                        var actor = string.Empty;
                        var marginL = string.Empty;
                        var marginR = string.Empty;
                        var marginV = string.Empty;
                        var effect = string.Empty;
                        var layer = 0;

                        string[] splitLine;
                        if (s.StartsWith("dialog:", StringComparison.Ordinal))
                        {
                            splitLine = line.Remove(0, 7).Split(',');
                        }
                        else if (s.StartsWith("dialogue:", StringComparison.Ordinal))
                        {
                            splitLine = line.Remove(0, 9).Split(',');
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
                            else if (i == indexStyle)
                            {
                                style = splitLine[i].Trim();
                            }
                            else if (i == indexActor && indexName == -1)
                            {
                                actor = splitLine[i].Trim();
                            }
                            else if (i == indexName)
                            {
                                actor = splitLine[i].Trim();
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
                            else if (i == indexEffect)
                            {
                                effect = splitLine[i].Trim();
                            }
                            else if (i == indexLayer)
                            {
                                int.TryParse(splitLine[i].Replace("Comment:", string.Empty).Trim(), out layer);
                            }
                            else if (i == indexText)
                            {
                                text = splitLine[i];
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
                                Text = text
                                    .Replace("\\n", Environment.NewLine)
                                    .Replace("\\N", Environment.NewLine),
                            };

                            if (!string.IsNullOrEmpty(style))
                            {
                                p.Extra = style;
                            }

                            if (!string.IsNullOrEmpty(actor))
                            {
                                p.Actor = actor;
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
                            p.IsComment = s.StartsWith("comment:", StringComparison.Ordinal);
                            subtitle.Paragraphs.Add(p);
                        }
                        catch
                        {
                            _errorCount++;
                            if (errors.Length < 2000)
                            {
                                errors.AppendFormat(FormatLanguage.LineNumberXErrorReadingTimeCodeFromSourceLineY, lineNumber, line).AppendLine();
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
            string[] timeCode = time.Split(':', '.');
            return new TimeCode(int.Parse(timeCode[0]),
                int.Parse(timeCode[1]),
                int.Parse(timeCode[2]),
                int.Parse(timeCode[3]) * 10);
        }

        public override void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
            if (newFormat != null && newFormat.Name == SubStationAlpha.NameOfFormat)
            {
                foreach (var p in subtitle.Paragraphs)
                {
                    string s = p.Text;

                    if (s.Contains('{') && s.Contains('}'))
                    {
                        var p1Index = s.IndexOf("\\p1", StringComparison.Ordinal);
                        var p0Index = s.IndexOf("{\\p0}", StringComparison.Ordinal);
                        if (p1Index > 0 && (p0Index > p1Index || p0Index == -1))
                        {
                            var startTagIndex = s.Substring(0, p1Index).LastIndexOf('{');
                            if (startTagIndex >= 0)
                            {
                                if (p0Index > p1Index)
                                {
                                    s = s.Remove(startTagIndex, p0Index - startTagIndex + "{\\p0}".Length);
                                }
                                else
                                {
                                    s = s.Remove(startTagIndex);
                                }
                            }
                        }

                        var karaokeStart = s.IndexOf("{Kara Effector", StringComparison.Ordinal);
                        if (karaokeStart >= 0)
                        {
                            int l = s.IndexOf('}', karaokeStart + 1);
                            if (l < karaokeStart)
                            {
                                break;
                            }

                            s = s.Remove(karaokeStart, l - karaokeStart + 1);
                        }

                        s = s.Replace(@"\u0", string.Empty);
                        s = s.Replace(@"\u1", string.Empty);
                        s = s.Replace(@"\s0", string.Empty);
                        s = s.Replace(@"\s1", string.Empty);
                        s = s.Replace(@"\be0", string.Empty);
                        s = s.Replace(@"\be1", string.Empty);

                        s = RemoveTag(s, "shad");
                        s = RemoveTag(s, "fsc");
                        s = RemoveTag(s, "fsp");
                        s = RemoveTag(s, "fr");

                        s = RemoveTag(s, "t(");
                        s = RemoveTag(s, "move(");
                        s = RemoveTag(s, "Position(");
                        s = RemoveTag(s, "org(");
                        s = RemoveTag(s, "fade(");
                        s = RemoveTag(s, "fad(");
                        s = RemoveTag(s, "clip(");
                        s = RemoveTag(s, "iclip(");
                        s = RemoveTag(s, "pbo(");
                        s = RemoveTag(s, "bord");
                        s = RemoveTag(s, "pos");

                        // TODO: Alignment tags

                        s = s.Replace("{}", string.Empty);

                        p.Text = s;
                    }
                }
            }
            else
            {
                foreach (var p in subtitle.Paragraphs)
                {
                    var noTags = Utilities.RemoveSsaTags(p.Text).Trim();
                    if (noTags.Length == 0)
                    {
                        p.Text = string.Empty;
                        continue;
                    }

                    p.Text = p.Text.Replace("\\n", Environment.NewLine); // Soft line break
                    p.Text = p.Text.Replace("\\N", Environment.NewLine); // Hard line break
                    p.Text = p.Text.Replace("\\h", " "); // Hard space

                    if (noTags.StartsWith("m ", StringComparison.Ordinal))
                    {
                        var test = noTags.Remove(0, 2)
                            .RemoveChar('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', 'l', 'm', ' ', '.');
                        if (test.Length == 0)
                        {
                            p.Text = string.Empty;
                            continue;
                        }
                    }

                    p.Text = GetFormattedText(p.Text);

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
                        int indexOfEnd = p.Text.IndexOf('}');
                        p.Text = p.Text.Remove(indexOfBegin, indexOfEnd - indexOfBegin + 1);

                        indexOfBegin = p.Text.IndexOf('{');
                    }
                    p.Text = pre + p.Text;
                }
            }
        }

        private static string RemoveTag(string s, string tag)
        {
            int indexOfTag = s.IndexOf(@"\" + tag, StringComparison.Ordinal);
            if (indexOfTag > 0)
            {
                var endIndex1 = s.IndexOf('\\', indexOfTag + 1);
                var endIndex2 = s.IndexOf('}', indexOfTag + 1);
                endIndex1 = Math.Min(endIndex1, endIndex2);
                if (endIndex1 > 0)
                {
                    return s.Remove(indexOfTag, endIndex1 - indexOfTag);
                }
            }
            return s;
        }

        /// <summary>
        /// BGR color like this: &amp;HBBGGRR&amp; (where BB, GG, and RR are hex values in uppercase)
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
            var s = f.Trim().Trim('&').ToLowerInvariant();

            if (s.StartsWith('h') && s.Length < 7)
            {
                while (s.Length < 7)
                {
                    s = s.Insert(1, "0");
                }
            }

            if (s.StartsWith('h') && s.Length == 7)
            {
                s = s.Substring(1);
                string hexColor = "#" + s.Substring(4, 2) + s.Substring(2, 2) + s.Substring(0, 2);
                try
                {
                    return ColorTranslator.FromHtml(hexColor);
                }
                catch
                {
                    return defaultColor;
                }
            }
            if (s.StartsWith('h') && s.Length == 9)
            {
                if (int.TryParse(s.Substring(1, 2), NumberStyles.HexNumber, null, out var alpha))
                {
                    alpha = 255 - alpha; // ASS stores alpha in reverse (0=fully solid and 255=fully transparent)
                }
                else
                {
                    alpha = 255; // full solid color
                }
                s = s.Substring(3);
                var hexColor = "#" + s.Substring(4, 2) + s.Substring(2, 2) + s.Substring(0, 2);
                try
                {
                    var c = ColorTranslator.FromHtml(hexColor);
                    return Color.FromArgb(alpha, c);
                }
                catch
                {
                    return defaultColor;
                }
            }

            if (int.TryParse(f, out var number))
            {
                var temp = Color.FromArgb(number);
                return Color.FromArgb(255, temp.B, temp.G, temp.R);
            }

            return defaultColor;
        }

        public static string GetSsaColorString(Color c)
        {
            return $"&H{255 - c.A:X2}{c.B:X2}{c.G:X2}{c.R:X2}"; // ASS stores alpha in reverse (0=full intensity and 255=fully transparent)
        }

        public static string GetSsaColorStringForEvent(Color c)
        {
            if (c.A >= 255)
            {
                return $"&H{c.B:X2}{c.G:X2}{c.R:X2}";
            }

            var alpha = 255 - c.A; // ASS stores alpha in reverse (0=full intensity and 255=fully transparent)
            return $"alpha&H{alpha:X2}&\\&H{c.B:X2}{c.G:X2}{c.R:X2}";
        }

        public static string CheckForErrors(string header)
        {
            if (string.IsNullOrEmpty(header))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            int styleCount = -1;

            int nameIndex = -1;
            int fontNameIndex = -1;
            int fontsizeIndex = -1;
            int primaryColourIndex = -1;
            int secondaryColourIndex = -1;
            int outlineColourIndex = -1;
            int backColourIndex = -1;
            int boldIndex = -1;
            int italicIndex = -1;
            int underlineIndex = -1;
            int strikeOutIndex = -1;
            int outlineIndex = -1;
            int shadowIndex = -1;
            int alignmentIndex = -1;
            int marginLIndex = -1;
            int marginRIndex = -1;
            int marginVIndex = -1;
            int scaleXIndex = -1;
            int scaleYIndex = -1;
            int spacingIndex = -1;
            int angleIndex = -1;
            int borderStyleIndex = -1;

            foreach (string line in header.SplitToLines())
            {
                string s = line.Trim().ToLowerInvariant();
                if (s.StartsWith("format:", StringComparison.Ordinal))
                {
                    if (line.Length > 10)
                    {
                        var format = line.Substring(8).ToLowerInvariant().Split(',');
                        styleCount = format.Length;
                        for (int i = 0; i < format.Length; i++)
                        {
                            string f = format[i].Trim();
                            if (f == "name")
                            {
                                nameIndex = i;
                            }
                            else if (f == "fontname")
                            {
                                fontNameIndex = i;
                            }
                            else if (f == "fontsize")
                            {
                                fontsizeIndex = i;
                            }
                            else if (f == "primarycolour")
                            {
                                primaryColourIndex = i;
                            }
                            else if (f == "secondarycolour")
                            {
                                secondaryColourIndex = i;
                            }
                            else if (f == "outlinecolour")
                            {
                                outlineColourIndex = i;
                            }
                            else if (f == "backcolour")
                            {
                                backColourIndex = i;
                            }
                            else if (f == "bold")
                            {
                                boldIndex = i;
                            }
                            else if (f == "italic")
                            {
                                italicIndex = i;
                            }
                            else if (f == "underline")
                            {
                                underlineIndex = i;
                            }
                            else if (f == "strikeout")
                            {
                                strikeOutIndex = i;
                            }
                            else if (f == "outline")
                            {
                                outlineIndex = i;
                            }
                            else if (f == "shadow")
                            {
                                shadowIndex = i;
                            }
                            else if (f == "alignment")
                            {
                                alignmentIndex = i;
                            }
                            else if (f == "marginl")
                            {
                                marginLIndex = i;
                            }
                            else if (f == "marginr")
                            {
                                marginRIndex = i;
                            }
                            else if (f == "marginv")
                            {
                                marginVIndex = i;
                            }
                            else if (f == "scalex")
                            {
                                scaleXIndex = i;
                            }
                            else if (f == "scaley")
                            {
                                scaleYIndex = i;
                            }
                            else if (f == "spacing")
                            {
                                spacingIndex = i;
                            }
                            else if (f == "angle")
                            {
                                angleIndex = i;
                            }
                            else if (f == "borderstyle")
                            {
                                borderStyleIndex = i;
                            }
                        }
                    }
                }
                else if (s.RemoveChar(' ').StartsWith("style:", StringComparison.Ordinal))
                {
                    if (line.Length > 10)
                    {
                        string rawLine = line;
                        var format = line.Substring(6).Split(',');

                        if (format.Length != styleCount)
                        {
                            sb.AppendLine("Number of expected Style elements do not match number of Format elements: " + rawLine);
                            sb.AppendLine();
                        }
                        else
                        {
                            var dummyColor = Color.FromArgb(9, 14, 16, 26);
                            for (int i = 0; i < format.Length; i++)
                            {
                                string f = format[i].Trim().ToLowerInvariant();
                                if (i == nameIndex)
                                {
                                    if (f.Length == 0)
                                    {
                                        sb.AppendLine("'Name' is empty: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == fontNameIndex)
                                {
                                    if (f.Length == 0)
                                    {
                                        sb.AppendLine("'Fontname' is empty: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == fontsizeIndex)
                                {
                                    if (!float.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _) || f.StartsWith('-'))
                                    {
                                        sb.AppendLine("'Fontsize' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == primaryColourIndex)
                                {
                                    if (GetSsaColor(f, dummyColor) == dummyColor || f == "&h")
                                    {
                                        sb.AppendLine("'PrimaryColour' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == secondaryColourIndex)
                                {
                                    if (GetSsaColor(f, dummyColor) == dummyColor)
                                    {
                                        sb.AppendLine("'SecondaryColour' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == outlineColourIndex)
                                {
                                    if (GetSsaColor(f, dummyColor) == dummyColor)
                                    {
                                        sb.AppendLine("'OutlineColour' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == backColourIndex)
                                {
                                    if (GetSsaColor(f, dummyColor) == dummyColor)
                                    {
                                        sb.AppendLine("'BackColour' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == boldIndex)
                                {
                                    if (Utilities.AllLetters.Contains(f))
                                    {
                                        sb.AppendLine("'Bold' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == italicIndex)
                                {
                                    if (Utilities.AllLetters.Contains(f))
                                    {
                                        sb.AppendLine("'Italic' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == underlineIndex)
                                {
                                    if (Utilities.AllLetters.Contains(f))
                                    {
                                        sb.AppendLine("'Underline' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == strikeOutIndex)
                                {
                                    if (Utilities.AllLetters.Contains(f))
                                    {
                                        sb.AppendLine("'StrikeOut' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == outlineIndex)
                                {
                                    if (!float.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _) || f.StartsWith('-'))
                                    {
                                        sb.AppendLine("'Outline' (width) incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == shadowIndex)
                                {
                                    if (!float.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _) || f.StartsWith('-'))
                                    {
                                        sb.AppendLine("'Shadow' (width) incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == alignmentIndex)
                                {
                                    if (!"101123456789 ".Contains(f))
                                    {
                                        sb.AppendLine("'Alignment' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == marginLIndex)
                                {
                                    if (!int.TryParse(f, out _) || f.StartsWith('-'))
                                    {
                                        sb.AppendLine("'MarginL' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == marginRIndex)
                                {
                                    if (!int.TryParse(f, out _) || f.StartsWith('-'))
                                    {
                                        sb.AppendLine("'MarginR' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == marginVIndex)
                                {
                                    if (!int.TryParse(f, out _) || f.StartsWith('-'))
                                    {
                                        sb.AppendLine("'MarginV' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == scaleXIndex)
                                {
                                    if (!float.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _) || f.StartsWith('-'))
                                    {
                                        sb.AppendLine("'ScaleX' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == scaleYIndex)
                                {
                                    if (!float.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _) || f.StartsWith('-'))
                                    {
                                        sb.AppendLine("'ScaleY' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == spacingIndex)
                                {
                                    if (!float.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _) || f.StartsWith('-'))
                                    {
                                        sb.AppendLine("'Spacing' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == angleIndex)
                                {
                                    if (!float.TryParse(f, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _))
                                    {
                                        sb.AppendLine("'Angle' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                                else if (i == borderStyleIndex)
                                {
                                    if (f.Length != 0 && !"1234".Contains(f))
                                    {
                                        sb.AppendLine("'BorderStyle' incorrect: " + rawLine);
                                        sb.AppendLine();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Add new style to ASS header
        /// </summary>
        /// <returns>Header with new style</returns>
        public static string AddSsaStyle(SsaStyle style, string inputHeader)
        {
            var header = inputHeader;
            if (string.IsNullOrEmpty(header))
            {
                header = DefaultHeader;
            }

            var sb = new StringBuilder();
            bool stylesStarted = false;
            bool styleAdded = false;
            bool styleLinesStarted = false;
            string styleFormat = SsaStyle.DefaultAssStyleFormat;
            foreach (string line in header.SplitToLines())
            {
                if (line.Equals("[V4+ Styles]", StringComparison.OrdinalIgnoreCase) || line.Equals("[V4 Styles]", StringComparison.OrdinalIgnoreCase))
                {
                    stylesStarted = true;
                }

                if (line.StartsWith("format:", StringComparison.OrdinalIgnoreCase))
                {
                    styleFormat = line;
                }

                if (!styleAdded && styleLinesStarted && !line.Trim().StartsWith("Style:", StringComparison.Ordinal))
                {
                    sb.AppendLine(style.ToRawAss(styleFormat));
                    styleAdded = true;
                }

                if (!line.StartsWith("Style: " + style.Name + ",", StringComparison.Ordinal)) // overwrite existing style
                {
                    sb.AppendLine(line);
                }

                if (stylesStarted && line.Trim().StartsWith("Style:", StringComparison.Ordinal))
                {
                    styleLinesStarted = true;
                }
            }

            if (!styleAdded)
            {
                sb.AppendLine(style.ToRawAss(styleFormat));
            }

            return sb.ToString();
        }

        public static SsaStyle GetSsaStyle(string styleName, string header)
        {
            var style = new SsaStyle { Name = styleName };

            int nameIndex = -1;
            int fontNameIndex = -1;
            int fontsizeIndex = -1;
            int primaryColourIndex = -1;
            int secondaryColourIndex = -1;
            int tertiaryColourIndex = -1;
            int outlineColourIndex = -1;
            int backColourIndex = -1;
            int boldIndex = -1;
            int italicIndex = -1;
            int underlineIndex = -1;
            int strikOutIndex = -1;
            int outlineIndex = -1;
            int shadowIndex = -1;
            int alignmentIndex = -1;
            int marginLIndex = -1;
            int marginRIndex = -1;
            int marginVIndex = -1;
            int scaleXIndex = -1;
            int scaleYIndex = -1;
            int spacingIndex = -1;
            int angleIndex = -1;
            int borderStyleIndex = -1;

            if (header == null)
            {
                header = DefaultHeader;
            }

            foreach (string line in header.SplitToLines())
            {
                string s = line.Trim().ToLowerInvariant();
                if (s.StartsWith("format:", StringComparison.Ordinal))
                {
                    if (line.Length > 10)
                    {
                        var format = line.ToLowerInvariant().Substring(8).Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            string f = format[i].Trim().ToLowerInvariant();
                            if (f == "name")
                            {
                                nameIndex = i;
                            }
                            else if (f == "fontname")
                            {
                                fontNameIndex = i;
                            }
                            else if (f == "fontsize")
                            {
                                fontsizeIndex = i;
                            }
                            else if (f == "primarycolour")
                            {
                                primaryColourIndex = i;
                            }
                            else if (f == "secondarycolour")
                            {
                                secondaryColourIndex = i;
                            }
                            else if (f == "tertiarycolour")
                            {
                                tertiaryColourIndex = i;
                            }
                            else if (f == "outlinecolour")
                            {
                                outlineColourIndex = i;
                            }
                            else if (f == "backcolour")
                            {
                                backColourIndex = i;
                            }
                            else if (f == "bold")
                            {
                                boldIndex = i;
                            }
                            else if (f == "italic")
                            {
                                italicIndex = i;
                            }
                            else if (f == "underline")
                            {
                                underlineIndex = i;
                            }
                            else if (f == "strikeout")
                            {
                                strikOutIndex = i;
                            }
                            else if (f == "outline")
                            {
                                outlineIndex = i;
                            }
                            else if (f == "shadow")
                            {
                                shadowIndex = i;
                            }
                            else if (f == "alignment")
                            {
                                alignmentIndex = i;
                            }
                            else if (f == "marginl")
                            {
                                marginLIndex = i;
                            }
                            else if (f == "marginr")
                            {
                                marginRIndex = i;
                            }
                            else if (f == "marginv")
                            {
                                marginVIndex = i;
                            }
                            else if (f == "scalex")
                            {
                                scaleXIndex = i;
                            }
                            else if (f == "scaley")
                            {
                                scaleYIndex = i;
                            }
                            else if (f == "spacing")
                            {
                                spacingIndex = i;
                            }
                            else if (f == "angle")
                            {
                                angleIndex = i;
                            }
                            else if (f == "borderstyle")
                            {
                                borderStyleIndex = i;
                            }
                        }
                    }
                }
                else if (s.RemoveChar(' ').StartsWith("style:", StringComparison.Ordinal))
                {
                    if (line.Length > 10)
                    {
                        style.RawLine = line;
                        var format = line.Substring(6).Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            string f = format[i].Trim();
                            if (i == nameIndex)
                            {
                                style.Name = format[i].Trim();
                            }
                            else if (i == fontNameIndex)
                            {
                                style.FontName = f;
                            }
                            else if (i == fontsizeIndex)
                            {
                                if (float.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var fOut))
                                {
                                    style.FontSize = fOut;
                                }
                            }
                            else if (i == primaryColourIndex)
                            {
                                style.Primary = GetSsaColor(f, Color.White);
                            }
                            else if (i == secondaryColourIndex)
                            {
                                style.Secondary = GetSsaColor(f, Color.Yellow);
                            }
                            else if (i == tertiaryColourIndex)
                            {
                                style.Tertiary = GetSsaColor(f, Color.Yellow);
                            }
                            else if (i == outlineColourIndex)
                            {
                                style.Outline = GetSsaColor(f, Color.Black);
                            }
                            else if (i == backColourIndex)
                            {
                                style.Background = GetSsaColor(f, Color.Black);
                            }
                            else if (i == boldIndex)
                            {
                                style.Bold = f == "-1" || f == "1";
                            }
                            else if (i == italicIndex)
                            {
                                style.Italic = f == "-1" || f == "1";
                            }
                            else if (i == underlineIndex)
                            {
                                style.Underline = f == "-1" || f == "1";
                            }
                            else if (i == strikOutIndex)
                            {
                                style.Strikeout = f == "-1" || f == "1";
                            }
                            else if (i == outlineIndex)
                            {
                                if (decimal.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
                                {
                                    style.OutlineWidth = number;
                                }
                            }
                            else if (i == shadowIndex)
                            {
                                if (decimal.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
                                {
                                    style.ShadowWidth = number;
                                }
                            }
                            else if (i == alignmentIndex)
                            {
                                style.Alignment = f;
                            }
                            else if (i == marginLIndex)
                            {
                                if (int.TryParse(f, out var number))
                                {
                                    style.MarginLeft = number;
                                }
                            }
                            else if (i == marginRIndex)
                            {
                                if (int.TryParse(f, out var number))
                                {
                                    style.MarginRight = number;
                                }
                            }
                            else if (i == marginVIndex)
                            {
                                if (int.TryParse(f, out var number))
                                {
                                    style.MarginVertical = number;
                                }
                            }
                            else if (i == scaleXIndex)
                            {
                                if (decimal.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
                                {
                                    style.ScaleX = number;
                                }
                            }
                            else if (i == scaleYIndex)
                            {
                                if (decimal.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
                                {
                                    style.ScaleY = number;
                                }
                            }
                            else if (i == spacingIndex)
                            {
                                if (decimal.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
                                {
                                    style.Spacing = number;
                                }
                            }
                            else if (i == angleIndex)
                            {
                                if (decimal.TryParse(f, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
                                {
                                    style.Angle = number;
                                }
                            }
                            else if (i == borderStyleIndex)
                            {
                                style.BorderStyle = f;
                            }
                        }
                    }
                    if (styleName != null && style.Name != null && (styleName.Equals(style.Name, StringComparison.OrdinalIgnoreCase) ||
                                                                    styleName.Equals("*Default", StringComparison.OrdinalIgnoreCase) &&
                                                                    style.Name.Equals("Default", StringComparison.OrdinalIgnoreCase)))
                    {
                        style.LoadedFromHeader = true;
                        return style;
                    }
                }
            }

            return new SsaStyle { Name = styleName };
        }
    }
}