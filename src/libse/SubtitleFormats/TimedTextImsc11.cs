using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// IMSC 1.1 Viewer: https://www.sandflow.com/imsc1_1/
    /// </summary>
    public class TimedTextImsc11 : SubtitleFormat
    {
        public override string Name => "Timed Text IMSC 1.1";

        private static string GetXmlStructure()
        {
            return @"<?xml version='1.0' encoding='UTF-8'?>
<tt xml:lang='en' xmlns='http://www.w3.org/ns/ttml' xmlns:ttm='http://www.w3.org/ns/ttml#metadata' xmlns:tts='http://www.w3.org/ns/ttml#styling' xmlns:ttp='http://www.w3.org/ns/ttml#parameter' xmlns:ittp='http://www.w3.org/ns/ttml/profile/imsc1#parameter' xmlns:itts='http://www.w3.org/ns/ttml/profile/imsc1#styling' ttp:profile='http://www.w3.org/ns/ttml/profile/imsc1/text' ttp:frameRate='[frameRate]' ttp:frameRateMultiplier='[frameRateMultiplier]' ttp:timeBase='media'>
  <head>
    <metadata>
      <ttm:title>[title]</ttm:title>
    </metadata>
    <styling>
      <style xml:id='style.center' tts:color='#ffffff' tts:opacity='1' tts:fontSize='100%' tts:fontFamily='default' tts:textAlign='center'/>
      <style xml:id='italic' tts:shear='16.6667%' tts:opacity='1' tts:fontSize='100%' tts:fontFamily='default'/>
    </styling>
    <layout>
      <region xml:id='region.topLeft' tts:origin='10% 10%' tts:extent='80% 40%' tts:displayAlign='before' tts:textAlign='start'/>
      <region xml:id='region.topCenter' tts:origin='10% 10%' tts:extent='80% 40%' tts:displayAlign='center'  tts:textAlign='center'/>
      <region xml:id='region.topRight' tts:origin='10% 10%' tts:extent='80% 40%' tts:displayAlign='after'  tts:textAlign='end'/>
      <region xml:id='region.centerLeft' tts:origin='10% 30%' tts:extent='80% 40%' tts:displayAlign='before' tts:textAlign='start'/>
      <region xml:id='region.centerCenter' tts:origin='10% 30%' tts:extent='80% 40%' tts:displayAlign='center'  tts:textAlign='center'/>
      <region xml:id='region.centerRight' tts:origin='10% 30%' tts:extent='80% 40%' tts:displayAlign='after'  tts:textAlign='end'/>
      <region xml:id='region.bottomLeft' tts:origin='17.583% 73.414%' tts:extent='64.844% 16.667%' tts:displayAlign='before'  tts:textAlign='start'/>
      <region xml:id='region.bottomCenter' tts:origin='17.583% 73.414%' tts:extent='64.844% 16.667%' tts:displayAlign='center'  tts:textAlign='center'/>
      <region xml:id='region.bottomRight' tts:origin='17.583% 73.414%' tts:extent='64.844% 16.667%' tts:displayAlign='after'  tts:textAlign='end'/>
    </layout>
  </head>
  <body>
    <div xml:id='d0' style='style.center'>
    </div>
  </body>
</tt>
".Replace('\'', '"');
        }

        public override string Extension => Configuration.Settings.SubtitleSettings.TimedTextImsc11FileExtension;

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

            return text.Contains("profile/imsc1") && base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var xml = new XmlDocument { XmlResolver = null };
            var xmlStructure = GetXmlStructure();
            xmlStructure = xmlStructure.Replace("[frameRate]", ((int)Math.Round(Configuration.Settings.General.CurrentFrameRate, MidpointRounding.AwayFromZero)).ToString());

            var frameDiff = Configuration.Settings.General.CurrentFrameRate % 1.0;
            if (frameDiff < 0.001)
            {
                xmlStructure = xmlStructure.Replace("[frameRateMultiplier]", "1 1");
            }
            else
            {
                xmlStructure = xmlStructure.Replace("[frameRateMultiplier]", "1000 1001");
            }

            var original = new XmlDocument();
            try
            {
                original.LoadXml(subtitle.Header);

                var namespaceManagerOriginal = new XmlNamespaceManager(original.NameTable);
                namespaceManagerOriginal.AddNamespace("ttml", TimedText10.TtmlNamespace);
                namespaceManagerOriginal.AddNamespace("ttp", TimedText10.TtmlParameterNamespace);
                namespaceManagerOriginal.AddNamespace("tts", TimedText10.TtmlStylingNamespace);
                namespaceManagerOriginal.AddNamespace("ttm", TimedText10.TtmlMetadataNamespace);

                var nodeTitle = original.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata/ttml:title", namespaceManagerOriginal);
                if (nodeTitle != null)
                {
                    xmlStructure = xmlStructure.Replace("[title]", nodeTitle.InnerXml);
                }

                var attr = original.DocumentElement.Attributes["ttp:timeBase"];
                if (attr?.Value != null && (attr.Value == "clock" || attr.Value == "smpte"))
                {
                    xmlStructure = xmlStructure.Replace("ttp:timeBase=\"media\"", $"ttp:timeBase=\"{attr.Value}\"");
                }
            }
            catch 
            {
                // ignore
            }

            var xmlTitle = string.IsNullOrEmpty(subtitle.FileName) ? title : Path.GetFileNameWithoutExtension(subtitle.FileName);
            if (string.IsNullOrEmpty(xmlTitle))
            {
                xmlTitle = "Untitled";
            }

            xmlStructure = xmlStructure.Replace("[title]", HtmlUtil.EncodeNamed(xmlTitle));


            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            xmlStructure = xmlStructure.Replace("lang=\"en\"", $"lang=\"{language}\"");
            xml.LoadXml(xmlStructure);
            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            var div = xml.DocumentElement.SelectSingleNode("ttml:body", namespaceManager).SelectSingleNode("ttml:div", namespaceManager);
            foreach (var p in subtitle.Paragraphs)
            {
                var paragraphNode = MakeParagraph(xml, p);
                div.AppendChild(paragraphNode);
            }

            var xmlString = ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty);
            subtitle.Header = xmlString;
            return xmlString;
        }

        private static XmlNode MakeParagraph(XmlDocument xml, Paragraph p)
        {
            var timeCodeFormat = Configuration.Settings.SubtitleSettings.TimedTextImsc11TimeCodeFormat;
            if (string.IsNullOrEmpty(timeCodeFormat))
            {
                timeCodeFormat = "hh:mm:ss.ms";
            }

            XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/ns/ttml");
            var text = p.Text.RemoveControlCharactersButWhiteSpace();

            XmlAttribute start = xml.CreateAttribute("begin");
            start.InnerText = TimedText10.ConvertToTimeString(p.StartTime, timeCodeFormat);
            paragraph.Attributes.Append(start);

            XmlAttribute end = xml.CreateAttribute("end");
            end.InnerText = TimedText10.ConvertToTimeString(p.EndTime, timeCodeFormat);
            paragraph.Attributes.Append(end);

            XmlAttribute region = xml.CreateAttribute("region");
            region.InnerText = GetRegionFromText(p.Text);
            paragraph.Attributes.Append(region);

            // Trying to parse and convert paragraph content
            try
            {
                text = Utilities.RemoveSsaTags(text);
                text = string.Join("<br/>", text.SplitToLines());
                var paragraphContent = new XmlDocument();
                paragraphContent.LoadXml($"<root>{text.Replace("&", "&amp;")}</root>");
                ConvertParagraphNodeToTtmlNode(paragraphContent.DocumentElement, xml, paragraph);
            }
            catch // Wrong markup, clear it
            {
                text = Regex.Replace(text, "[<>]", "");
                paragraph.AppendChild(xml.CreateTextNode(text));
            }

            return paragraph;
        }

        internal static void ConvertParagraphNodeToTtmlNode(XmlNode node, XmlDocument ttmlXml, XmlNode ttmlNode)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child is XmlText)
                {
                    ttmlNode.AppendChild(ttmlXml.CreateTextNode(child.Value));
                }
                else if (child.Name == "br")
                {
                    XmlNode br = ttmlXml.CreateElement("br");
                    ttmlNode.AppendChild(br);

                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, br);
                }
                else if (child.Name == "i")
                {
                    XmlNode span = ttmlXml.CreateElement("span");
                    XmlAttribute attr = ttmlXml.CreateAttribute("style");
                    attr.InnerText = "italic";
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else // Default - skip node
                {
                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, ttmlNode);
                }
            }
        }

        private static string GetRegionFromText(string text)
        {
            if (text.StartsWith(@"{\an7", StringComparison.Ordinal))
            {
                return "region.topLeft";
            }

            if (text.StartsWith(@"{\an8", StringComparison.Ordinal))
            {
                return "region.topCenter";
            }

            if (text.StartsWith(@"{\an9", StringComparison.Ordinal))
            {
                return "region.topRight";
            }


            if (text.StartsWith(@"{\an4", StringComparison.Ordinal))
            {
                return "region.centerLeft";
            }

            if (text.StartsWith(@"{\an5", StringComparison.Ordinal))
            {
                return "region.centerCenter";
            }

            if (text.StartsWith(@"{\an6", StringComparison.Ordinal))
            {
                return "region.centerRight";
            }


            if (text.StartsWith(@"{\an1", StringComparison.Ordinal))
            {
                return "region.bottomLeft";
            }

            if (text.StartsWith(@"{\an3", StringComparison.Ordinal))
            {
                return "region.bottomRight";
            }

            return "region.bottomCenter";
        }

        private static string GetAssStyleFromRegion(string region)
        {
            switch (region)
            {
                case "region.topLeft": return @"{\an7}";
                case "region.topCenter": return @"{\an8}";
                case "region.topRight": return @"{\an9}";
                case "region.centerLeft": return @"{\an4}";
                case "region.centerCenter": return @"{\an5}";
                case "region.centerRight": return @"{\an6}";
                case "region.bottomLeft": return @"{\an1}";
                case "region.bottomRight": return @"{\an3}";
                default: return string.Empty;
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
            foreach (XmlNode node in body.SelectNodes("//ttml:p", namespaceManager))
            {
                TimedText10.ExtractTimeCodes(node, subtitle, out var begin, out var end);
                var assStyle = string.Empty;
                var region = node.Attributes?["region"];
                if (region != null)
                {
                    assStyle = GetAssStyleFromRegion(region.InnerText);
                }

                var text = assStyle + ReadParagraph(node, xml);
                var p = new Paragraph(begin, end, text);
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
        }

        private static string ReadParagraph(XmlNode node, XmlDocument xml)
        {
            var pText = new StringBuilder();
            var styles = GetStyles();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Text)
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


                    // Composing styles

                    if (child.Attributes["style"] != null)
                    {
                        var styleName = child.Attributes["style"].Value;

                        if (styles.Contains(styleName))
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

                                    if (currentStyle == styleName)
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

                    if (child.Attributes["tts:fontStyle"] != null && child.Attributes["tts:fontStyle"].Value == "italic")
                    {
                        isItalic = true;
                    }
                    else if (child.Attributes["style"] != null && child.Attributes["style"].Value == "italic")
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


                    // Applying styles
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

            return pText.ToString().TrimEnd();
        }

        public static string RemoveTags(string text)
        {
            return text
                .Replace("<bouten-dot-before>", string.Empty)
                .Replace("</bouten-dot-before>", string.Empty)

                .Replace("<bouten-dot-after>", string.Empty)
                .Replace("</bouten-dot-after>", string.Empty)

                .Replace("<bouten-dot-outside>", string.Empty)
                .Replace("</bouten-dot-outside>", string.Empty)

                .Replace("<bouten-filled-circle-outside>", string.Empty)
                .Replace("</bouten-filled-circle-outside>", string.Empty)

                .Replace("<bouten-open-circle-outside>", string.Empty)
                .Replace("</bouten-open-circle-outside>", string.Empty)

                .Replace("<bouten-open-dot-outside>", string.Empty)
                .Replace("</bouten-open-dot-outside>", string.Empty)

                .Replace("<bouten-filled-sesame-outside>", string.Empty)
                .Replace("</bouten-filled-sesame-outside>", string.Empty)

                .Replace("<bouten-open-sesame-outside>", string.Empty)
                .Replace("</bouten-open-sesame-outside>", string.Empty)

                .Replace("<bouten-auto-outside>", string.Empty)
                .Replace("</bouten-auto-outside>", string.Empty)

                .Replace("<bouten-auto>", string.Empty)
                .Replace("</bouten-auto>", string.Empty)

                .Replace("<horizontalDigit>", string.Empty)
                .Replace("</horizontalDigit>", string.Empty)

                .Replace("<ruby-container>", string.Empty)
                .Replace("</ruby-container>", string.Empty)

                .Replace("<ruby-base>", string.Empty)
                .Replace("</ruby-base>", string.Empty)

                .Replace("<ruby-base-italic>", string.Empty)
                .Replace("</ruby-base-italic>", string.Empty)

                .Replace("<ruby-text>", string.Empty)
                .Replace("</ruby-text>", string.Empty)

                .Replace("<ruby-text-after>", string.Empty)
                .Replace("</ruby-text-after>", string.Empty)

                .Replace("<ruby-text-italic>", string.Empty)
                .Replace("</ruby-text-italic>", string.Empty);
        }

        public override void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
            foreach (var p in subtitle.Paragraphs)
            {
                p.Text = RemoveTags(p.Text);
            }
        }
    }
}
