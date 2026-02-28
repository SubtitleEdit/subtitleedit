using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// https://github.com/imsc-rosetta/imsc-rosetta-specification
    /// </summary>
    public class TimedTextImscRosetta : SubtitleFormat
    {
        public override string Name => "Timed Text IMSC Rosetta";

        public static string LineHeight = "125%";
        public static string FontSize = "5.3rh";
        public static string Language = string.Empty;


        private static string GetXmlStructure()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
<tt xmlns:rosetta=""https://github.com/imsc-rosetta/specification"" xmlns:ttp=""http://www.w3.org/ns/ttml#parameter"" xmlns:itts=""http://www.w3.org/ns/ttml/profile/imsc1#styling"" xmlns:ebutts=""urn:ebu:tt:style"" xmlns:ttm=""http://www.w3.org/ns/ttml#metadata"" xmlns:tts=""http://www.w3.org/ns/ttml#styling"" xml:lang=""[language]"" xml:space=""preserve"" xmlns:xml=""http://www.w3.org/XML/1998/namespace"" ttp:cellResolution=""30 15"" ttp:frameRateMultiplier=""[frameRateMultiplier]"" ttp:timeBase=""media"" ttp:frameRate=""[frameRate]"" xmlns=""http://www.w3.org/ns/ttml"">
  <head>
    <metadata>
      <rosetta:format>imsc-rosetta</rosetta:format>
      <rosetta:version>0.0.0</rosetta:version>
    </metadata>
    <styling>
      <style xml:id=""d_default"" style=""_d_default"" />
      <style xml:id=""r_default"" style=""_r_default"" tts:backgroundColor=""#00000000"" tts:fontFamily=""proportionalSansSerif"" tts:fontStyle=""normal"" tts:fontWeight=""normal"" tts:overflow=""visible"" tts:showBackground=""whenActive"" tts:wrapOption=""noWrap"" />
      <style xml:id=""_d_default"" style=""d_outline"" />
      <style xml:id=""_r_quantisationregion"" tts:origin=""10% 10%"" tts:extent=""80% 80%"" tts:fontSize=""5.333rh"" tts:lineHeight=""125%""/>
      <style xml:id=""_r_default"" style=""s_fg_white p_al_center"" tts:fontSize=""[FONT_SIZE]"" tts:lineHeight=""[LINE_HEIGHT]"" tts:luminanceGain=""1.0"" itts:fillLineGap=""false"" ebutts:linePadding=""0.25c"" />
      <style xml:id=""p_al_center"" tts:textAlign=""center"" ebutts:multiRowAlign=""center"" />
      <style xml:id=""p_al_start"" tts:textAlign=""start"" ebutts:multiRowAlign=""start"" />
      <style xml:id=""p_al_end"" tts:textAlign=""end"" ebutts:multiRowAlign=""end"" />
      <style xml:id=""s_fg_white"" tts:color=""#FFFFFF"" />
      <style xml:id=""s_outlineblack"" tts:textOutline=""#000000 0.05em"" />
      <style xml:id=""d_outline"" style=""s_outlineblack"" />
      <style xml:id=""p_font1"" tts:fontFamily=""proportionalSansSerif"" tts:fontSize=""100%"" tts:lineHeight=""[LINE_HEIGHT]"" />
      <style xml:id=""s_italic"" tts:fontStyle=""italic"" />
      <style xml:id=""s_bold"" tts:fontWeight=""bold"" />
      <style xml:id=""s_underline"" tts:textDecoration=""underline"" />
    </styling>
    <layout>
      [REGIONS]
    </layout>
  </head>
  <body>
  </body>
</tt>";
        }

        public override string Extension => ".imscr";
        public override List<string> AlternateExtensions => new List<string> { ".xml" };

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !(fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var text = sb.ToString();
            if (text.Contains("lang=\"ja\"", StringComparison.Ordinal) && text.Contains("bouten-", StringComparison.Ordinal))
            {
                return false;
            }

            return text.Contains("<rosetta:format>imsc-rosetta</rosetta:format>", StringComparison.Ordinal) && base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var xml = new XmlDocument { XmlResolver = null };
            var xmlStructure = GetXmlStructure();

            xmlStructure = xmlStructure.Replace("[LINE_HEIGHT]", LineHeight);
            xmlStructure = xmlStructure.Replace("[FONT_SIZE]", FontSize);

            var currentFrameRate = Configuration.Settings.General.CurrentFrameRate;
            if (currentFrameRate < 0.1)
            {
                currentFrameRate = Configuration.Settings.General.DefaultFrameRate;
            }

            string frameRate;
            string frameRateMultiplier;

            // Handle specific frame rates according to IMSC Rosetta specification
            if (Math.Abs(currentFrameRate - 23.976) < 0.01)
            {
                frameRate = "24";
                frameRateMultiplier = "1000 1001";
            }
            else if (Math.Abs(currentFrameRate - 24) < 0.01)
            {
                frameRate = "24";
                frameRateMultiplier = "1 1";
            }
            else if (Math.Abs(currentFrameRate - 25) < 0.01)
            {
                frameRate = "25";
                frameRateMultiplier = "1 1";
            }
            else if (Math.Abs(currentFrameRate - 29.97) < 0.01)
            {
                frameRate = "30";
                frameRateMultiplier = "1000 1001";
            }
            else if (Math.Abs(currentFrameRate - 30) < 0.01)
            {
                frameRate = "30";
                frameRateMultiplier = "1 1";
            }
            else if (Math.Abs(currentFrameRate - 50) < 0.01)
            {
                frameRate = "50";
                frameRateMultiplier = "1 1";
            }
            else if (Math.Abs(currentFrameRate - 59.94) < 0.01)
            {
                frameRate = "60";
                frameRateMultiplier = "1000 1001";
            }
            else if (Math.Abs(currentFrameRate - 60) < 0.01)
            {
                frameRate = "60";
                frameRateMultiplier = "1 1";
            }
            else
            {
                // For non-standard frame rates, round to nearest integer and use 1:1 multiplier
                frameRate = ((int)Math.Round(currentFrameRate, MidpointRounding.AwayFromZero)).ToString();
                frameRateMultiplier = "1 1";
            }

            xmlStructure = xmlStructure.Replace("[frameRate]", frameRate);
            xmlStructure = xmlStructure.Replace("[frameRateMultiplier]", frameRateMultiplier);

            var language = Language;
            if (string.IsNullOrEmpty(language))
            {
                language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            }

            xmlStructure = xmlStructure.Replace("[language]", language);

            // Determine which regions are needed
            var usedAlignments = new HashSet<string>();
            foreach (var p in subtitle.Paragraphs)
            {
                var alignment = GetAlignmentFromText(p.Text);
                usedAlignments.Add(alignment);
            }

            // Generate regions XML
            var regionsXml = GenerateRegionsXml(usedAlignments);
            xmlStructure = xmlStructure.Replace("[REGIONS]", regionsXml);

            xml.LoadXml(xmlStructure);
            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            var body = xml.DocumentElement.SelectSingleNode("ttml:body", namespaceManager);

            var divCounter = 1;
            foreach (var p in subtitle.Paragraphs)
            {
                var divNode = MakeDiv(xml, p, divCounter);
                body.AppendChild(divNode);
                divCounter++;
            }

            var xmlString = ToUtf8XmlString(xml)
                .Replace(" xmlns=\"\"", string.Empty)
                .Replace("encoding=\"utf-8\"", "encoding=\"UTF-8\"");
            subtitle.Header = xmlString;
            return xmlString;
        }

        private static XmlNode MakeDiv(XmlDocument xml, Paragraph p, int divCounter)
        {
            var timeCodeFormat = Configuration.Settings.SubtitleSettings.TimedTextImsc11TimeCodeFormat;
            if (string.IsNullOrEmpty(timeCodeFormat))
            {
                timeCodeFormat = "hh:mm:ss.ms";
            }

            XmlNode div = xml.CreateElement("div", "http://www.w3.org/ns/ttml");

            XmlAttribute id = xml.CreateAttribute("xml", "id", "http://www.w3.org/XML/1998/namespace");
            id.InnerText = $"e_{divCounter}";
            div.Attributes.Append(id);

            // Determine region based on alignment
            var alignment = GetAlignmentFromText(p.Text);
            var regionId = GetRegionIdForAlignment(alignment);

            XmlAttribute region = xml.CreateAttribute("region");
            region.InnerText = regionId;
            div.Attributes.Append(region);

            XmlAttribute style = xml.CreateAttribute("style");
            style.InnerText = "d_default";
            div.Attributes.Append(style);

            XmlAttribute begin = xml.CreateAttribute("begin");
            begin.InnerText = TimedText10.ConvertToTimeString(p.StartTime, timeCodeFormat);
            div.Attributes.Append(begin);

            XmlAttribute end = xml.CreateAttribute("end");
            end.InnerText = TimedText10.ConvertToTimeString(p.EndTime, timeCodeFormat);
            div.Attributes.Append(end);

            XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/ns/ttml");
            XmlAttribute pStyle = xml.CreateAttribute("style");
            
            // Combine horizontal alignment style with font style
            var horizontalAlignment = GetHorizontalAlignmentStyle(alignment);
            pStyle.InnerText = horizontalAlignment == "p_al_center" ? "p_font1" : $"{horizontalAlignment} p_font1";
            paragraph.Attributes.Append(pStyle);

            var text = p.Text.RemoveControlCharactersButWhiteSpace();

            try
            {
                text = Utilities.RemoveSsaTags(text);
                var lines = text.SplitToLines();
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    var paragraphContent = new XmlDocument { PreserveWhitespace = true };
                    paragraphContent.LoadXml($"<root>{line.Replace("&", "&amp;")}</root>");
                    ConvertParagraphNodeToTtmlNode(paragraphContent.DocumentElement, xml, paragraph);

                    if (i < lines.Count - 1)
                    {
                        XmlNode span = xml.CreateElement("span", "http://www.w3.org/ns/ttml");
                        XmlNode br = xml.CreateElement("br", "http://www.w3.org/ns/ttml");
                        span.AppendChild(br);
                        paragraph.AppendChild(span);
                    }
                }
            }
            catch
            {
                text = Regex.Replace(text, "[<>]", "");
                XmlNode span = xml.CreateElement("span", "http://www.w3.org/ns/ttml");
                span.AppendChild(xml.CreateTextNode(text));
                paragraph.AppendChild(span);
            }

            div.AppendChild(paragraph);
            return div;
        }

        private static void ConvertParagraphNodeToTtmlNode(XmlNode node, XmlDocument ttmlXml, XmlNode ttmlNode)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child is XmlText || child is XmlWhitespace || child is XmlSignificantWhitespace)
                {
                    XmlNode span = ttmlXml.CreateElement("span", "http://www.w3.org/ns/ttml");
                    span.AppendChild(ttmlXml.CreateTextNode(child.Value));
                    ttmlNode.AppendChild(span);
                }
                else if (child.Name == "br")
                {
                    XmlNode span = ttmlXml.CreateElement("span", "http://www.w3.org/ns/ttml");
                    XmlNode br = ttmlXml.CreateElement("br", "http://www.w3.org/ns/ttml");
                    span.AppendChild(br);
                    ttmlNode.AppendChild(span);
                }
                else if (child.Name == "i")
                {
                    XmlNode span = ttmlXml.CreateElement("span", "http://www.w3.org/ns/ttml");
                    XmlAttribute attr = ttmlXml.CreateAttribute("style");
                    attr.InnerText = "s_italic";
                    span.Attributes.Append(attr);
                    
                    // Recursively process child nodes to handle nested tags
                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                    ttmlNode.AppendChild(span);
                }
                else if (child.Name == "b")
                {
                    XmlNode span = ttmlXml.CreateElement("span", "http://www.w3.org/ns/ttml");
                    XmlAttribute attr = ttmlXml.CreateAttribute("style");
                    attr.InnerText = "s_bold";
                    span.Attributes.Append(attr);
                    
                    // Recursively process child nodes to handle nested tags
                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                    ttmlNode.AppendChild(span);
                }
                else if (child.Name == "u")
                {
                    XmlNode span = ttmlXml.CreateElement("span", "http://www.w3.org/ns/ttml");
                    XmlAttribute attr = ttmlXml.CreateAttribute("style");
                    attr.InnerText = "s_underline";
                    span.Attributes.Append(attr);
                    
                    // Recursively process child nodes to handle nested tags
                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                    ttmlNode.AppendChild(span);
                }
                else
                {
                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, ttmlNode);
                }
            }
        }

        private static List<string> GetStyles()
        {
            return TimedText10.GetStylesFromHeader(GetXmlStructure());
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
                xml.LoadXml(sb.ToString().Replace(" & ", " &amp; ").Replace("Q&A", "Q&amp;A").RemoveControlCharactersButWhiteSpace().Trim());
            }

            var frameRateAttr = xml.DocumentElement.Attributes["ttp:frameRate"];
            if (frameRateAttr != null)
            {
                if (double.TryParse(frameRateAttr.Value, out var fr))
                {
                    if (fr > 20 && fr < 100)
                    {
                        Configuration.Settings.General.CurrentFrameRate = fr;
                    }

                    var frameRateMultiplier = xml.DocumentElement.Attributes["ttp:frameRateMultiplier"];
                    if (frameRateMultiplier != null)
                    {
                        if ((frameRateMultiplier.InnerText == "999 1000" ||
                             frameRateMultiplier.InnerText == "1000 1001") && Math.Abs(fr - 30) < 0.01)
                        {
                            Configuration.Settings.General.CurrentFrameRate = 29.97;
                        }
                        else if ((frameRateMultiplier.InnerText == "999 1000" ||
                                  frameRateMultiplier.InnerText == "1000 1001") && Math.Abs(fr - 24) < 0.01)
                        {
                            Configuration.Settings.General.CurrentFrameRate = 23.976;
                        }
                        else
                        {
                            var arr = frameRateMultiplier.InnerText.Split();
                            if (arr.Length == 2 && Utilities.IsInteger(arr[0]) && Utilities.IsInteger(arr[1]) && int.Parse(arr[1]) > 0)
                            {
                                fr = double.Parse(arr[0]) / double.Parse(arr[1]);
                                if (fr > 20 && fr < 100)
                                {
                                    Configuration.Settings.General.CurrentFrameRate = fr;
                                }
                            }
                        }
                    }
                }
            }

            if (BatchSourceFrameRate.HasValue)
            {
                Configuration.Settings.General.CurrentFrameRate = BatchSourceFrameRate.Value;
            }

            Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = null;
            subtitle.Header = sb.ToString();

            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            var body = xml.DocumentElement.SelectSingleNode("ttml:body", namespaceManager);

            foreach (XmlNode divNode in body.SelectNodes("ttml:div", namespaceManager))
            {
                TimedText10.ExtractTimeCodes(divNode, subtitle, out var begin, out var end);

                var pNodes = divNode.SelectNodes("ttml:p", namespaceManager);
                if (pNodes != null && pNodes.Count > 0)
                {
                    // Extract region ID from div
                    var regionId = divNode.Attributes?["region"]?.Value ?? "R0"; // default to bottom
                    
                    // Extract paragraph style from first p node
                    var firstPNode = pNodes[0];
                    var paragraphStyle = firstPNode.Attributes?["style"]?.Value ?? string.Empty;
                    
                    // Get the alignment tag
                    var alignmentTag = GetAlignmentTagFromRegionAndStyle(regionId, paragraphStyle);
                    
                    // Read all <p> nodes and combine them with line breaks
                    var textBuilder = new StringBuilder();
                    for (int i = 0; i < pNodes.Count; i++)
                    {
                        if (i > 0)
                        {
                            textBuilder.AppendLine();
                        }
                        textBuilder.Append(ReadParagraph(pNodes[i], xml));
                    }
                    var text = textBuilder.ToString();
                    
                    // Prepend alignment tag if not default
                    if (!string.IsNullOrEmpty(alignmentTag))
                    {
                        text = alignmentTag + text;
                    }
                    
                    var p = new Paragraph(begin, end, text);
                    subtitle.Paragraphs.Add(p);
                }
            }

            subtitle.Renumber();
        }

        private static string GetAlignmentFromText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "an2"; // default: bottom-center
            }

            var trimmedText = text.TrimStart();

            // Check for ASS alignment tags
            if (trimmedText.StartsWith("{\\an1}", StringComparison.Ordinal)) return "an1";
            if (trimmedText.StartsWith("{\\an2}", StringComparison.Ordinal)) return "an2";
            if (trimmedText.StartsWith("{\\an3}", StringComparison.Ordinal)) return "an3";
            if (trimmedText.StartsWith("{\\an4}", StringComparison.Ordinal)) return "an4";
            if (trimmedText.StartsWith("{\\an5}", StringComparison.Ordinal)) return "an5";
            if (trimmedText.StartsWith("{\\an6}", StringComparison.Ordinal)) return "an6";
            if (trimmedText.StartsWith("{\\an7}", StringComparison.Ordinal)) return "an7";
            if (trimmedText.StartsWith("{\\an8}", StringComparison.Ordinal)) return "an8";
            if (trimmedText.StartsWith("{\\an9}", StringComparison.Ordinal)) return "an9";

            return "an2"; // default: bottom-center
        }

        private static string GetAlignmentTagFromRegionAndStyle(string regionId, string paragraphStyle)
        {
            // Determine vertical position from region
            var verticalPosition = regionId switch
            {
                "R0" => "bottom",
                "R1" => "middle",
                "R2" => "top",
                "R3" => "middle",
                _ => "bottom" // default
            };

            // Determine horizontal position from paragraph style
            var horizontalPosition = "center"; // default
            if (!string.IsNullOrEmpty(paragraphStyle))
            {
                if (paragraphStyle.Contains("p_al_start"))
                {
                    horizontalPosition = "left";
                }
                else if (paragraphStyle.Contains("p_al_end"))
                {
                    horizontalPosition = "right";
                }
            }

            // Convert to an1-9 tag
            var alignmentTag = (verticalPosition, horizontalPosition) switch
            {
                ("bottom", "left") => "{\\an1}",
                ("bottom", "center") => "{\\an2}",
                ("bottom", "right") => "{\\an3}",
                ("middle", "left") => "{\\an4}",
                ("middle", "center") => "{\\an5}",
                ("middle", "right") => "{\\an6}",
                ("top", "left") => "{\\an7}",
                ("top", "center") => "{\\an8}",
                ("top", "right") => "{\\an9}",
                _ => "{\\an2}" // default: bottom-center
            };

            // Only return non-default alignment tags
            return alignmentTag == "{\\an2}" ? string.Empty : alignmentTag;
        }

        private static string GetRegionIdForAlignment(string alignment)
        {
            return alignment switch
            {
                "an1" => "R0",  // bottom-left
                "an2" => "R0",  // bottom-center
                "an3" => "R0",  // bottom-right
                "an4" => "R1",  // middle-left
                "an5" => "R1",  // middle-center
                "an6" => "R1",  // middle-right
                "an7" => "R2",  // top-left
                "an8" => "R2",  // top-center
                "an9" => "R2",  // top-right
                _ => "R0"       // default: bottom
            };
        }

        private static string GetHorizontalAlignmentStyle(string alignment)
        {
            return alignment switch
            {
                "an1" => "p_al_start",   // left
                "an2" => "p_al_center",  // center
                "an3" => "p_al_end",     // right
                "an4" => "p_al_start",   // left
                "an5" => "p_al_center",  // center
                "an6" => "p_al_end",     // right
                "an7" => "p_al_start",   // left
                "an8" => "p_al_center",  // center
                "an9" => "p_al_end",     // right
                _ => "p_al_center"       // default: center
            };
        }

        private static string GenerateRegionsXml(HashSet<string> usedAlignments)
        {
            var sb = new StringBuilder();
            var regionsNeeded = new HashSet<string>();

            // Determine which of the 3 vertical regions are needed
            foreach (var alignment in usedAlignments)
            {
                var regionId = GetRegionIdForAlignment(alignment);
                regionsNeeded.Add(regionId);
            }

            // Generate only the needed regions
            if (regionsNeeded.Contains("R0"))
            {
                sb.AppendLine("      <region xml:id=\"R0\" tts:origin=\"10% 10%\" tts:extent=\"80% 80%\" tts:displayAlign=\"after\" style=\"r_default\" />");
            }

            if (regionsNeeded.Contains("R1"))
            {
                sb.AppendLine("      <region xml:id=\"R1\" tts:origin=\"10% 43.3%\" tts:extent=\"80% 46.7%\" tts:displayAlign=\"before\" style=\"r_default\" />");
            }

            if (regionsNeeded.Contains("R2"))
            {
                sb.AppendLine("      <region xml:id=\"R2\" tts:origin=\"10% 10%\" tts:extent=\"80% 80%\" tts:displayAlign=\"before\" style=\"r_default\" />");
            }

            // Ensure we always have at least the default region (R0 - bottom)
            if (regionsNeeded.Count == 0)
            {
                sb.AppendLine("      <region xml:id=\"R0\" tts:origin=\"10% 10%\" tts:extent=\"80% 80%\" tts:displayAlign=\"after\" style=\"r_default\" />");
            }

            return sb.ToString().TrimEnd();
        }


        private static string ReadParagraph(XmlNode node, XmlDocument xml)
        {
            var pText = new StringBuilder();
            var styles = GetStyles();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Text || child.NodeType == XmlNodeType.Whitespace || child.NodeType == XmlNodeType.SignificantWhitespace)
                {
                    pText.Append(child.Value);
                }
                else if (child.Name == "br" || child.Name == "tt:br")
                {
                    pText.AppendLine();
                }
                else if (child.Name == "#significant-whitespace" || child.Name == "tt:#significant-whitespace")
                {
                    pText.Append(child.InnerText);
                }
                else if (child.Name == "span" || child.Name == "tt:span")
                {
                    var isItalic = false;
                    var isBold = false;
                    var isUnderlined = false;
                    string fontFamily = null;
                    string color = null;

                    if (child.Attributes["style"] != null)
                    {
                        var styleName = child.Attributes["style"].Value;

                        // Check for combined styles (space-separated)
                        var styleNames = styleName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var style in styleNames)
                        {
                            if (style == "s_italic")
                            {
                                isItalic = true;
                            }
                            else if (style == "s_bold")
                            {
                                isBold = true;
                            }
                            else if (style == "s_underline")
                            {
                                isUnderlined = true;
                            }
                            else if (styles.Contains(style))
                            {
                                try
                                {
                                    var nsmgr = new XmlNamespaceManager(xml.NameTable);
                                    nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                                    XmlNode head = xml.DocumentElement.SelectSingleNode("ttml:head", nsmgr);
                                    foreach (XmlNode styleNode in head.SelectNodes("//ttml:style", nsmgr))
                                    {
                                        string currentStyle = null;
                                        if (styleNode.Attributes["xml:id"] != null)
                                        {
                                            currentStyle = styleNode.Attributes["xml:id"].Value;
                                        }
                                        else if (styleNode.Attributes["id"] != null)
                                        {
                                            currentStyle = styleNode.Attributes["id"].Value;
                                        }

                                        if (currentStyle == style)
                                        {
                                            if (styleNode.Attributes["tts:fontStyle"] != null && styleNode.Attributes["tts:fontStyle"].Value == "italic")
                                            {
                                                isItalic = true;
                                            }

                                            if (styleNode.Attributes["tts:fontWeight"] != null && styleNode.Attributes["tts:fontWeight"].Value == "bold")
                                            {
                                                isBold = true;
                                            }

                                            if (styleNode.Attributes["tts:textDecoration"] != null && styleNode.Attributes["tts:textDecoration"].Value == "underline")
                                            {
                                                isUnderlined = true;
                                            }

                                            if (styleNode.Attributes["tts:fontFamily"] != null)
                                            {
                                                fontFamily = styleNode.Attributes["tts:fontFamily"].Value;
                                            }

                                            if (styleNode.Attributes["tts:color"] != null)
                                            {
                                                color = styleNode.Attributes["tts:color"].Value;
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    System.Diagnostics.Debug.WriteLine(e);
                                }
                            }
                        }
                    }

                    if (child.Attributes["tts:fontStyle"] != null && child.Attributes["tts:fontStyle"].Value == "italic")
                    {
                        isItalic = true;
                    }

                    if (child.Attributes["tts:fontWeight"] != null && child.Attributes["tts:fontWeight"].Value == "bold")
                    {
                        isBold = true;
                    }

                    if (child.Attributes["tts:textDecoration"] != null && child.Attributes["tts:textDecoration"].Value == "underline")
                    {
                        isUnderlined = true;
                    }

                    if (child.Attributes["tts:fontFamily"] != null)
                    {
                        fontFamily = child.Attributes["tts:fontFamily"].Value;
                    }

                    if (child.Attributes["tts:color"] != null)
                    {
                        color = child.Attributes["tts:color"].Value;
                    }

                    if (isItalic)
                    {
                        pText.Append("<i>");
                    }

                    if (isBold)
                    {
                        pText.Append("<b>");
                    }

                    if (isUnderlined)
                    {
                        pText.Append("<u>");
                    }

                    if (!string.IsNullOrEmpty(fontFamily) || !string.IsNullOrEmpty(color))
                    {
                        pText.Append("<font");

                        if (!string.IsNullOrEmpty(fontFamily))
                        {
                            pText.Append($" face=\"{fontFamily}\"");
                        }

                        if (!string.IsNullOrEmpty(color))
                        {
                            pText.Append($" color=\"{color}\"");
                        }

                        pText.Append(">");
                    }

                    pText.Append(ReadParagraph(child, xml));

                    if (!string.IsNullOrEmpty(fontFamily) || !string.IsNullOrEmpty(color))
                    {
                        pText.Append("</font>");
                    }

                    if (isUnderlined)
                    {
                        pText.Append("</u>");
                    }

                    if (isBold)
                    {
                        pText.Append("</b>");
                    }

                    if (isItalic)
                    {
                        pText.Append("</i>");
                    }
                }
            }

            return pText.ToString();
        }
    }
}
