using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
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

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var text = sb.ToString();

            // "urn:ebu:tt:distribution" (the conformsToStandard urn) and ebutts:linePadding are
            // EBU-TT-D specific; plain "urn:ebu:tt" also appears in Netflix Japanese IMSC docs,
            // so it is not enough on its own.
            if (!text.Contains("urn:ebu:tt:distribution", StringComparison.Ordinal) &&
                !text.Contains("ebutts:linePadding", StringComparison.Ordinal) &&
                !(text.Contains("urn:ebu:tt:style", StringComparison.Ordinal) && text.Contains("ttp:timeBase=\"media\"", StringComparison.Ordinal)))
            {
                return false;
            }

            if (text.Contains("bouten-", StringComparison.Ordinal))
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
            var count = 1;
            foreach (var p in subtitle.Paragraphs)
            {
                div.AppendChild(MakeParagraph(xml, p, count));
                count++;
            }

            return ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty);
        }

        private static XmlNode MakeParagraph(XmlDocument xml, Paragraph p, int count)
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
            // spans per line with tt:br between lines.
            var first = true;
            foreach (var line in text.SplitToLines())
            {
                if (!first)
                {
                    paragraph.AppendChild(xml.CreateElement("br", TtmlNamespace));
                }

                foreach (var segment in SplitToStyledSegments(line))
                {
                    var span = xml.CreateElement("span", TtmlNamespace);
                    var style = xml.CreateAttribute("style");
                    style.InnerText = segment.Italic && segment.Bold
                        ? "textStyle italicStyle boldStyle"
                        : segment.Italic
                            ? "textStyle italicStyle"
                            : segment.Bold
                                ? "textStyle boldStyle"
                                : "textStyle";
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
            public StyledSegment(string text, bool italic, bool bold)
            {
                Text = text;
                Italic = italic;
                Bold = bold;
            }

            public string Text { get; }
            public bool Italic { get; }
            public bool Bold { get; }
        }

        // Splits one line into plain/italic/bold segments; any other tags (font/underline) are
        // dropped, keeping their inner text.
        private static List<StyledSegment> SplitToStyledSegments(string line)
        {
            var segments = new List<StyledSegment>();
            var sb = new StringBuilder();
            var italic = 0;
            var bold = 0;
            var i = 0;

            void Flush()
            {
                if (sb.Length > 0)
                {
                    segments.Add(new StyledSegment(sb.ToString(), italic > 0, bold > 0));
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
                                case "i": italic++; break;
                                case "/i": italic = Math.Max(0, italic - 1); break;
                                case "b": bold++; break;
                                case "/b": bold = Math.Max(0, bold - 1); break;
                            }
                        }
                        else if (!tag.StartsWith("font", StringComparison.Ordinal) && tag != "/font" &&
                                 tag != "u" && tag != "/u")
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
                segments.Add(new StyledSegment(string.Empty, false, false));
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

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
            try
            {
                xml.LoadXml(sb.ToString().RemoveControlCharactersButWhiteSpace().Trim());
            }
            catch
            {
                xml.LoadXml(sb.ToString().Replace(" & ", " &amp; ").RemoveControlCharactersButWhiteSpace().Trim());
            }

            subtitle.Header = sb.ToString();

            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("ttml", TtmlNamespace);

            var italicStyles = new HashSet<string>(StringComparer.Ordinal);
            var boldStyles = new HashSet<string>(StringComparer.Ordinal);
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
                var text = ReadParagraph(node, italicStyles, boldStyles);

                var region = node.Attributes?["region"]?.Value;
                if (region != null && topRegions.Contains(region))
                {
                    text = "{\\an8}" + text;
                }

                subtitle.Paragraphs.Add(new Paragraph(begin, end, text));
            }

            subtitle.Renumber();
        }

        private static string ReadParagraph(XmlNode node, HashSet<string> italicStyles, HashSet<string> boldStyles)
        {
            var pText = new StringBuilder();
            ReadNode(node, pText, italicStyles, boldStyles, inheritedItalic: false, inheritedBold: false);

            var text = pText.ToString()
                .Replace("   ", " ")
                .Replace("  ", " ")
                .Replace("  ", " ")
                .Replace(Environment.NewLine + " ", Environment.NewLine)
                .Replace(" " + Environment.NewLine, Environment.NewLine)
                .Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine)
                .Replace("</b>" + Environment.NewLine + "<b>", Environment.NewLine);

            return text.Trim();
        }

        private static void ReadNode(XmlNode node, StringBuilder pText, HashSet<string> italicStyles, HashSet<string> boldStyles, bool inheritedItalic, bool inheritedBold)
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

                    if (inheritedItalic)
                    {
                        pText.Append("<i>").Append(value).Append("</i>");
                    }
                    else if (inheritedBold)
                    {
                        pText.Append("<b>").Append(value).Append("</b>");
                    }
                    else
                    {
                        pText.Append(value);
                    }
                }
                else if (child.LocalName == "br")
                {
                    pText.AppendLine();
                }
                else if (child.LocalName == "span")
                {
                    var italic = inheritedItalic;
                    var bold = inheritedBold;

                    var styleRefs = child.Attributes?["style"]?.Value;
                    if (!string.IsNullOrEmpty(styleRefs))
                    {
                        foreach (var styleRef in styleRefs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            italic = italic || italicStyles.Contains(styleRef);
                            bold = bold || boldStyles.Contains(styleRef);
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

                    ReadNode(child, pText, italicStyles, boldStyles, italic, bold);
                }
                else
                {
                    ReadNode(child, pText, italicStyles, boldStyles, inheritedItalic, inheritedBold);
                }
            }
        }
    }
}
