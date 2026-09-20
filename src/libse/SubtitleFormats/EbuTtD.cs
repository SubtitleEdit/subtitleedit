using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// EBU-TT-D (EBU Tech 3380) - the TTML profile required for subtitle distribution by many
    /// European broadcasters (BBC iPlayer, ARD/ZDF mediatheks, NPO, ...) and mandated by HbbTV.
    /// Media timebase only, percentage-based regions, all text inside tt:span elements, styling
    /// via style references incl. ebutts:linePadding.
    /// Spec: https://tech.ebu.ch/publications/tech3380
    /// </summary>
    public class EbuTtD : SubtitleFormat
    {
        public override string Name => "EBU-TT-D";

        public override string Extension => ".xml";

        private const string TtmlNamespace = "http://www.w3.org/ns/ttml";
        private const string TtmlStylingNamespace = "http://www.w3.org/ns/ttml#styling";

        // The eight teletext colours (the RGB corners). Coloured text is written as referential
        // styles named after these; the reader maps exact corner values back to the names so a
        // round trip keeps <font color="yellow"> instead of degrading to hex.
        private static readonly (string Name, byte R, byte G, byte B)[] TeletextColors =
        {
            ("black", 0, 0, 0),
            ("red", 255, 0, 0),
            ("green", 0, 255, 0),
            ("yellow", 255, 255, 0),
            ("blue", 0, 0, 255),
            ("magenta", 255, 0, 255),
            ("cyan", 0, 255, 255),
            ("white", 255, 255, 255),
        };

        // The eight names above mean the teletext corners here, not the CSS palette (CSS "green"
        // is #008000), so resolve them before the generic parser gets a say.
        private static SkiaSharp.SKColor ParseColor(string colorValue)
        {
            var trimmed = colorValue.Trim();
            foreach (var (name, r, g, b) in TeletextColors)
            {
                if (string.Equals(trimmed, name, StringComparison.OrdinalIgnoreCase))
                {
                    return new SkiaSharp.SKColor(r, g, b);
                }
            }

            return HtmlUtil.GetColorFromString(trimmed); // white if unparsable
        }

        private static string GetXmlStructure()
        {
            return @"<?xml version='1.0' encoding='UTF-8'?>
<tt xmlns='http://www.w3.org/ns/ttml' xmlns:ttm='http://www.w3.org/ns/ttml#metadata' xmlns:tts='http://www.w3.org/ns/ttml#styling' xmlns:ttp='http://www.w3.org/ns/ttml#parameter' xmlns:ebuttdt='urn:ebu:tt:datatypes' xmlns:ebutts='urn:ebu:tt:style' xmlns:ebuttm='urn:ebu:tt:metadata' ttp:timeBase='media' ttp:cellResolution='32 15' xml:lang='en'>
  <head>
    <metadata>
      <ebuttm:documentMetadata>
        <ebuttm:conformsToStandard>urn:ebu:tt:distribution:2018-04</ebuttm:conformsToStandard>
      </ebuttm:documentMetadata>
    </metadata>
    <styling>
      <style xml:id='defaultStyle' tts:fontFamily='sansSerif' tts:fontSize='100%' tts:lineHeight='125%' tts:textAlign='center' tts:color='#ffffff' tts:backgroundColor='transparent' tts:fontStyle='normal' tts:fontWeight='normal'/>
      <style xml:id='textStyle' tts:color='#ffffff' tts:backgroundColor='#000000c2' ebutts:linePadding='0.5c'/>
      <style xml:id='italicStyle' tts:fontStyle='italic'/>
      <style xml:id='boldStyle' tts:fontWeight='bold'/>
    </styling>
    <layout>
      <region xml:id='bottom' tts:origin='10% 10%' tts:extent='80% 80%' tts:displayAlign='after' tts:overflow='visible'/>
      <region xml:id='top' tts:origin='10% 10%' tts:extent='80% 80%' tts:displayAlign='before' tts:overflow='visible'/>
    </layout>
  </head>
  <body style='defaultStyle'>
    <div>
    </div>
  </body>
</tt>
".Replace('\'', '"');
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !(fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) ||
                                      fileName.EndsWith(".ttml", StringComparison.OrdinalIgnoreCase) ||
                                      fileName.EndsWith(".dfxp", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var text = JoinLines(lines);

            // "urn:ebu:tt:distribution" (the conformsToStandard urn) and ebutts:linePadding are
            // EBU-TT-D specific; plain "urn:ebu:tt" also appears in Netflix Japanese IMSC docs,
            // so it is not enough on its own.
            if (!text.Contains("urn:ebu:tt:distribution", StringComparison.Ordinal) &&
                !text.Contains("ebutts:linePadding", StringComparison.Ordinal) &&
                !(text.Contains("urn:ebu:tt:style", StringComparison.Ordinal) && text.Contains("ttp:timeBase=\"media\"", StringComparison.Ordinal)))
            {
                return false;
            }

            if (text.Contains("lang=\"ja\"", StringComparison.Ordinal) && NetflixImsc11Japanese.ContainsJapaneseProfileStyling(text))
            {
                return false; // Netflix IMSC 1.1 Japanese
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var xml = new XmlDocument { XmlResolver = null };
            var xmlStructure = GetXmlStructure();

            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            xmlStructure = xmlStructure.Replace("xml:lang=\"en\"", $"xml:lang=\"{language}\"");
            xml.LoadXml(xmlStructure);

            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("ttml", TtmlNamespace);

            var div = xml.DocumentElement.SelectSingleNode("ttml:body", namespaceManager).SelectSingleNode("ttml:div", namespaceManager);
            var colorStyles = new WriterColorStyles();
            var count = 1;
            foreach (var p in subtitle.Paragraphs)
            {
                div.AppendChild(MakeParagraph(xml, p, count, colorStyles));
                count++;
            }

            if (colorStyles.Styles.Count > 0)
            {
                var styling = xml.DocumentElement.SelectSingleNode("ttml:head/ttml:styling", namespaceManager);
                foreach (var kvp in colorStyles.Styles)
                {
                    var styleNode = xml.CreateElement("style", TtmlNamespace);
                    var idAttribute = xml.CreateAttribute("xml:id");
                    idAttribute.InnerText = kvp.Key;
                    styleNode.Attributes.Append(idAttribute);
                    var colorAttribute = xml.CreateAttribute("tts", "color", TtmlStylingNamespace);
                    colorAttribute.InnerText = kvp.Value;
                    styleNode.Attributes.Append(colorAttribute);
                    styling.AppendChild(styleNode);
                }
            }

            return ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty);
        }

        private static XmlNode MakeParagraph(XmlDocument xml, Paragraph p, int count, WriterColorStyles colorStyles)
        {
            XmlNode paragraph = xml.CreateElement("p", TtmlNamespace);

            var idAttribute = xml.CreateAttribute("xml:id");
            idAttribute.InnerText = $"sub{count}";
            paragraph.Attributes.Append(idAttribute);

            var start = xml.CreateAttribute("begin");
            start.InnerText = ToTimeCode(p.StartTime);
            paragraph.Attributes.Append(start);

            var end = xml.CreateAttribute("end");
            end.InnerText = ToTimeCode(p.EndTime);
            paragraph.Attributes.Append(end);

            var raw = p.Text.RemoveControlCharactersButWhiteSpace();
            var region = xml.CreateAttribute("region");
            region.InnerText = GetRegionFromText(raw);
            paragraph.Attributes.Append(region);
            var text = Utilities.RemoveSsaTags(raw);

            // EBU-TT-D: all character content must live inside tt:span elements, one or more
            // spans per line with tt:br between lines. Tag state carries across lines so an
            // <i> opened on line one still applies to line two.
            var first = true;
            var tagState = new TagState();
            foreach (var line in text.SplitToLines())
            {
                if (!first)
                {
                    paragraph.AppendChild(xml.CreateElement("br", TtmlNamespace));
                }

                foreach (var segment in SplitToStyledSegments(line, tagState))
                {
                    var span = xml.CreateElement("span", TtmlNamespace);
                    var style = xml.CreateAttribute("style");
                    var styleValue = "textStyle";
                    if (segment.Italic)
                    {
                        styleValue += " italicStyle";
                    }

                    if (segment.Bold)
                    {
                        styleValue += " boldStyle";
                    }

                    var colorStyleId = segment.Color == null ? null : colorStyles.GetStyleId(segment.Color);
                    if (colorStyleId != null)
                    {
                        styleValue += " " + colorStyleId;
                    }

                    style.InnerText = styleValue;
                    span.Attributes.Append(style);
                    span.AppendChild(xml.CreateTextNode(segment.Text));
                    paragraph.AppendChild(span);
                }

                first = false;
            }

            return paragraph;
        }

        private readonly struct StyledSegment
        {
            public StyledSegment(string text, bool italic, bool bold, string color)
            {
                Text = text;
                Italic = italic;
                Bold = bold;
                Color = color;
            }

            public string Text { get; }
            public bool Italic { get; }
            public bool Bold { get; }
            public string Color { get; }
        }

        private class TagState
        {
            public int Italic;
            public int Bold;

            // One entry per open <font>; null for font tags without a color attribute so the
            // matching </font> still pops correctly.
            public readonly List<string> FontColors = new List<string>();

            public string CurrentColor
            {
                get
                {
                    for (var i = FontColors.Count - 1; i >= 0; i--)
                    {
                        if (FontColors[i] != null)
                        {
                            return FontColors[i];
                        }
                    }

                    return null;
                }
            }
        }

        private static readonly Regex FontColorRegex = new Regex("color\\s*=\\s*(?:\"([^\"]*)\"|'([^']*)'|([^\\s>]+))", RegexOptions.Compiled);

        // Collects the referential colour styles used by a document being written; the eight
        // teletext colours get their names as style ids, anything else a hex id. White is the
        // textStyle default, so all-white text stays untagged (GetStyleId returns null).
        private class WriterColorStyles
        {
            public readonly List<KeyValuePair<string, string>> Styles = new List<KeyValuePair<string, string>>();
            private readonly HashSet<string> _defined = new HashSet<string>(StringComparer.Ordinal);

            public string GetStyleId(string colorValue)
            {
                var color = ParseColor(colorValue);
                string styleId = null;
                foreach (var (name, r, g, b) in TeletextColors)
                {
                    if (color.Red == r && color.Green == g && color.Blue == b)
                    {
                        if (name == "white")
                        {
                            return null;
                        }

                        styleId = "color" + char.ToUpperInvariant(name[0]) + name.Substring(1);
                        break;
                    }
                }

                if (styleId == null)
                {
                    styleId = $"color{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
                }

                if (_defined.Add(styleId))
                {
                    Styles.Add(new KeyValuePair<string, string>(styleId, $"#{color.Red:x2}{color.Green:x2}{color.Blue:x2}"));
                }

                return styleId;
            }
        }

        // Splits one line into styled segments (italic/bold/font color); other tags (underline,
        // font size/face) are dropped, keeping their inner text.
        private static List<StyledSegment> SplitToStyledSegments(string line, TagState state)
        {
            var segments = new List<StyledSegment>();
            var sb = new StringBuilder();
            var i = 0;

            void Flush()
            {
                if (sb.Length > 0)
                {
                    segments.Add(new StyledSegment(sb.ToString(), state.Italic > 0, state.Bold > 0, state.CurrentColor));
                    sb.Clear();
                }
            }

            while (i < line.Length)
            {
                if (line[i] == '<')
                {
                    var endTag = line.IndexOf('>', i);
                    if (endTag > i)
                    {
                        var tag = line.Substring(i + 1, endTag - i - 1).Trim().ToLowerInvariant();
                        if (tag == "i" || tag == "b" || tag == "/i" || tag == "/b")
                        {
                            Flush();
                            switch (tag)
                            {
                                case "i": state.Italic++; break;
                                case "/i": state.Italic = Math.Max(0, state.Italic - 1); break;
                                case "b": state.Bold++; break;
                                case "/b": state.Bold = Math.Max(0, state.Bold - 1); break;
                            }
                        }
                        else if (tag.StartsWith("font", StringComparison.Ordinal))
                        {
                            Flush();
                            var match = FontColorRegex.Match(tag);
                            var color = match.Success
                                ? (match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Success ? match.Groups[2].Value : match.Groups[3].Value)
                                : null;
                            state.FontColors.Add(string.IsNullOrWhiteSpace(color) ? null : color);
                        }
                        else if (tag == "/font")
                        {
                            Flush();
                            if (state.FontColors.Count > 0)
                            {
                                state.FontColors.RemoveAt(state.FontColors.Count - 1);
                            }
                        }
                        else if (tag != "u" && tag != "/u")
                        {
                            sb.Append(line, i, endTag - i + 1); // not a tag we know - keep as text
                        }

                        i = endTag + 1;
                        continue;
                    }
                }

                sb.Append(line[i]);
                i++;
            }

            Flush();
            if (segments.Count == 0)
            {
                segments.Add(new StyledSegment(string.Empty, false, false, null));
            }

            return segments;
        }

        private static string GetRegionFromText(string text)
        {
            if (text.StartsWith("{\\an7", StringComparison.Ordinal) ||
                text.StartsWith("{\\an8", StringComparison.Ordinal) ||
                text.StartsWith("{\\an9", StringComparison.Ordinal))
            {
                return "top";
            }

            return "bottom";
        }

        private static string ToTimeCode(TimeCode time)
        {
            var ts = time.TimeSpan;
            return $"{ts.Days * 24 + ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var xml = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
            try
            {
                xml.LoadXml(JoinLines(lines).RemoveControlCharactersButWhiteSpace().Trim());
            }
            catch
            {
                try
                {
                    xml.LoadXml(JoinLines(lines).Replace(" & ", " &amp; ").RemoveControlCharactersButWhiteSpace().Trim());
                }
                catch (Exception exception)
                {
                    // The retry is the last chance to make sense of the file; a truncated or
                    // damaged one must read as "not mine", not throw out of the reader (and out
                    // of IsMine, which runs for every format when a file is opened).
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                    _errorCount = 1;
                    return;
                }
            }

            subtitle.Header = JoinLines(lines);

            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("ttml", TtmlNamespace);

            var italicStyles = new HashSet<string>(StringComparer.Ordinal);
            var boldStyles = new HashSet<string>(StringComparer.Ordinal);
            var colorStyles = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (XmlNode styleNode in xml.DocumentElement.SelectNodes("//ttml:style", namespaceManager))
            {
                var id = styleNode.Attributes?["xml:id"]?.Value ?? styleNode.Attributes?["id"]?.Value;
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                if (styleNode.Attributes?["tts:fontStyle"]?.Value == "italic")
                {
                    italicStyles.Add(id);
                }

                if (styleNode.Attributes?["tts:fontWeight"]?.Value == "bold")
                {
                    boldStyles.Add(id);
                }

                var colorValue = styleNode.Attributes?["tts:color"]?.Value;
                if (!string.IsNullOrEmpty(colorValue))
                {
                    // Null value = explicit white; a span referencing it resets an inherited
                    // colour back to the default (and gets no font tag).
                    colorStyles[id] = GetFontColorTag(colorValue);
                }
            }

            var topRegions = new HashSet<string>(StringComparer.Ordinal);
            foreach (XmlNode regionNode in xml.DocumentElement.SelectNodes("//ttml:region", namespaceManager))
            {
                var id = regionNode.Attributes?["xml:id"]?.Value ?? regionNode.Attributes?["id"]?.Value;
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                if (regionNode.Attributes?["tts:displayAlign"]?.Value == "before")
                {
                    topRegions.Add(id);
                }
            }

            var body = xml.DocumentElement.SelectSingleNode("ttml:body", namespaceManager);
            if (body == null)
            {
                _errorCount++;
                return;
            }

            foreach (XmlNode node in body.SelectNodes("//ttml:p", namespaceManager))
            {
                TimedText10.ExtractTimeCodes(node, subtitle, out var begin, out var end);
                var text = ReadParagraph(node, italicStyles, boldStyles, colorStyles);

                var region = node.Attributes?["region"]?.Value;
                if (region != null && topRegions.Contains(region))
                {
                    text = "{\\an8}" + text;
                }

                subtitle.Paragraphs.Add(new Paragraph(begin, end, text));
            }

            subtitle.Renumber();
        }

        /// <summary>
        /// Maps a tts:color value to the font tag colour SE should show: a teletext colour name
        /// for the exact RGB corners, lowercase #rrggbb for anything else, null for white (the
        /// document default - no tag) and for unparsable values.
        /// </summary>
        private static string GetFontColorTag(string ttsColorValue)
        {
            if (string.IsNullOrWhiteSpace(ttsColorValue))
            {
                return null;
            }

            var color = ParseColor(ttsColorValue);
            foreach (var (name, r, g, b) in TeletextColors)
            {
                if (color.Red == r && color.Green == g && color.Blue == b)
                {
                    return name == "white" ? null : name;
                }
            }

            return $"#{color.Red:x2}{color.Green:x2}{color.Blue:x2}";
        }

        private readonly struct TextRun
        {
            public TextRun(string text, bool italic, bool bold, string color, bool isBreak)
            {
                Text = text;
                Italic = italic;
                Bold = bold;
                Color = color;
                IsBreak = isBreak;
            }

            public string Text { get; }
            public bool Italic { get; }
            public bool Bold { get; }
            public string Color { get; }
            public bool IsBreak { get; }
        }

        private static string ReadParagraph(XmlNode node, HashSet<string> italicStyles, HashSet<string> boldStyles, Dictionary<string, string> colorStyles)
        {
            var runs = new List<TextRun>();
            ReadNode(node, runs, italicStyles, boldStyles, colorStyles, inheritedItalic: false, inheritedBold: false, inheritedColor: null);

            var text = BuildText(runs)
                .Replace("   ", " ")
                .Replace("  ", " ")
                .Replace("  ", " ")
                .Replace(Environment.NewLine + " ", Environment.NewLine)
                .Replace(" " + Environment.NewLine, Environment.NewLine);

            return text.Trim();
        }

        // Turns the flat run list into tagged text. Tags nest font > i > b and stay open across
        // runs (and line breaks) with the same formatting, so two italic lines come out as one
        // <i>...</i> block. Whitespace-only runs (XML indentation, spaces between spans) carry
        // no formatting of their own - they are buffered and emitted after any closing tags but
        // before any opening ones, keeping tags tight around the visible text.
        private static string BuildText(List<TextRun> runs)
        {
            var sb = new StringBuilder();
            var pending = new StringBuilder();
            var fontOpen = false;
            var italicOpen = false;
            var boldOpen = false;
            string openColor = null;

            foreach (var run in runs)
            {
                if (run.IsBreak)
                {
                    pending.Append(Environment.NewLine);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(run.Text))
                {
                    pending.Append(run.Text);
                    continue;
                }

                var fontChange = openColor != run.Color;
                var italicChange = italicOpen != run.Italic;
                var boldChange = boldOpen != run.Bold;

                if (boldOpen && (boldChange || italicChange || fontChange))
                {
                    sb.Append("</b>");
                    boldOpen = false;
                }

                if (italicOpen && (italicChange || fontChange))
                {
                    sb.Append("</i>");
                    italicOpen = false;
                }

                if (fontOpen && fontChange)
                {
                    sb.Append("</font>");
                    fontOpen = false;
                    openColor = null;
                }

                if (pending.Length > 0)
                {
                    sb.Append(pending);
                    pending.Clear();
                }

                if (run.Color != null && !fontOpen)
                {
                    sb.Append("<font color=\"").Append(run.Color).Append("\">");
                    fontOpen = true;
                    openColor = run.Color;
                }

                if (run.Italic && !italicOpen)
                {
                    sb.Append("<i>");
                    italicOpen = true;
                }

                if (run.Bold && !boldOpen)
                {
                    sb.Append("<b>");
                    boldOpen = true;
                }

                sb.Append(run.Text);
            }

            if (boldOpen)
            {
                sb.Append("</b>");
            }

            if (italicOpen)
            {
                sb.Append("</i>");
            }

            if (fontOpen)
            {
                sb.Append("</font>");
            }

            sb.Append(pending); // trailing whitespace stays outside the tags; Trim removes it

            return sb.ToString();
        }

        private static void ReadNode(XmlNode node, List<TextRun> runs, HashSet<string> italicStyles, HashSet<string> boldStyles, Dictionary<string, string> colorStyles, bool inheritedItalic, bool inheritedBold, string inheritedColor)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Text || child.NodeType == XmlNodeType.SignificantWhitespace)
                {
                    var value = child.Value;
                    if (child.NodeType == XmlNodeType.SignificantWhitespace)
                    {
                        value = " ";
                    }

                    runs.Add(new TextRun(value, inheritedItalic, inheritedBold, inheritedColor, isBreak: false));
                }
                else if (child.LocalName == "br")
                {
                    runs.Add(new TextRun(string.Empty, false, false, null, isBreak: true));
                }
                else if (child.LocalName == "span")
                {
                    var italic = inheritedItalic;
                    var bold = inheritedBold;
                    var color = inheritedColor;

                    var styleRefs = child.Attributes?["style"]?.Value;
                    if (!string.IsNullOrEmpty(styleRefs))
                    {
                        foreach (var styleRef in styleRefs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            italic = italic || italicStyles.Contains(styleRef);
                            bold = bold || boldStyles.Contains(styleRef);
                            if (colorStyles.TryGetValue(styleRef, out var styleColor))
                            {
                                color = styleColor; // last colour-bearing ref wins, per TTML
                            }
                        }
                    }

                    if (child.Attributes?["tts:fontStyle"]?.Value == "italic")
                    {
                        italic = true;
                    }

                    if (child.Attributes?["tts:fontWeight"]?.Value == "bold")
                    {
                        bold = true;
                    }

                    var inlineColor = child.Attributes?["tts:color"]?.Value;
                    if (!string.IsNullOrEmpty(inlineColor))
                    {
                        color = GetFontColorTag(inlineColor);
                    }

                    ReadNode(child, runs, italicStyles, boldStyles, colorStyles, italic, bold, color);
                }
                else
                {
                    ReadNode(child, runs, italicStyles, boldStyles, colorStyles, inheritedItalic, inheritedBold, inheritedColor);
                }
            }
        }
    }
}
