//http://www.w3.org/TR/ttaf1-dfxp/
//Timed Text Markup Language (TTML) 1.0
//W3C Recommendation 18 November 2010

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TimedText10 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public const string NameOfFormat = "Timed Text 1.0";

        public override string Name => NameOfFormat;

        public static string TtmlNamespace => "http://www.w3.org/ns/ttml";
        public static string TtmlParameterNamespace => "http://www.w3.org/ns/ttml#parameter";
        public static string TtmlStylingNamespace => "http://www.w3.org/ns/ttml#styling";
        public static string TtmlMetadataNamespace => "http://www.w3.org/ns/ttml#metadata";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();

            if (xmlAsString.Contains("xmlns:tts=\"http://www.w3.org/2006/04"))
            {
                return false;
            }

            if (xmlAsString.Contains("http://www.w3.org/ns/ttml"))
            {
                xmlAsString = xmlAsString.RemoveControlCharactersButWhiteSpace();
                var xml = new XmlDocument { XmlResolver = null };
                try
                {
                    xml.LoadXml(xmlAsString);
                    var nsmgr = new XmlNamespaceManager(xml.NameTable);
                    nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                    var nds = xml.DocumentElement.SelectSingleNode("ttml:body", nsmgr);
                    var paragraphs = nds.SelectNodes("//ttml:p", nsmgr);
                    return paragraphs != null && paragraphs.Count > 0;
                }
                catch
                {
                    try
                    {
                        xml.LoadXml(xmlAsString.Replace(" & ", " &amp; ").Replace("Q&A", "Q&amp;A"));
                        var nsmgr = new XmlNamespaceManager(xml.NameTable);
                        nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                        var nds = xml.DocumentElement.SelectSingleNode("ttml:body", nsmgr);
                        var paragraphs = nds.SelectNodes("//ttml:p", nsmgr);

                        if (paragraphs != null && (paragraphs.Count > 0 && new NetflixTimedText().IsMine(lines, fileName)))
                        {
                            return false;
                        }

                        if (paragraphs != null && (paragraphs.Count > 0 && new SmpteTt2052().IsMine(lines, fileName)))
                        {
                            return false;
                        }

                        return paragraphs != null && paragraphs.Count > 0;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }
            }
            return false;
        }

        internal static string ConvertToTimeString(TimeCode time)
        {
            var timeCodeFormat = Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormat.Trim().ToLowerInvariant();
            if (timeCodeFormat == "source" && !string.IsNullOrWhiteSpace(Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource))
            {
                timeCodeFormat = Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource.Trim().ToLowerInvariant();
            }
            switch (timeCodeFormat)
            {
                case "source":
                case "seconds":
                    return string.Format(CultureInfo.InvariantCulture, "{0:0.0##}s", time.TotalSeconds);
                case "milliseconds":
                    return string.Format(CultureInfo.InvariantCulture, "{0}ms", time.TotalMilliseconds);
                case "ticks":
                    return string.Format(CultureInfo.InvariantCulture, "{0}t", TimeSpan.FromMilliseconds(time.TotalMilliseconds).Ticks);
                case "hh:mm:ss.ms":
                    return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:000}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
                case "hh:mm:ss.ms-two-digits":
                    return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:00}", time.Hours, time.Minutes, time.Seconds, (int)Math.Round(time.Milliseconds / 10.0));
                case "hh:mm:ss,ms":
                    return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00},{3:000}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
                default:
                    return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
            }
        }

        public static void AddStyleToXml(XmlDocument xml, XmlNode head, XmlNamespaceManager nsmgr, string name, string fontFamily, string fontWeight, string fontStyle, string color, string fontSize)
        {
            var styleNode = xml.CreateNode(XmlNodeType.Element, string.Empty, "style", nsmgr.LookupNamespace("ttml"));

            XmlAttribute attr = xml.CreateAttribute("xml:id", TtmlStylingNamespace);
            attr.InnerText = name;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:fontFamily", TtmlStylingNamespace);
            attr.InnerText = fontFamily;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:fontWeight", TtmlStylingNamespace);
            attr.InnerText = fontWeight;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:fontStyle", TtmlStylingNamespace);
            attr.InnerText = fontStyle;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:color", TtmlStylingNamespace);
            attr.InnerText = color;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:fontSize", TtmlStylingNamespace);
            attr.InnerText = fontSize;
            styleNode.Attributes.Append(attr);

            foreach (XmlNode innerNode in head.ChildNodes)
            {
                if (innerNode.Name == "styling")
                {
                    innerNode.AppendChild(styleNode);
                    break;
                }
            }
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            bool hasStyleHead = false;
            bool convertedFromSubStationAlpha = false;
            if (subtitle.Header != null)
            {
                try
                {
                    var x = new XmlDocument();
                    x.LoadXml(subtitle.Header);
                    var xnsmgr = new XmlNamespaceManager(x.NameTable);
                    xnsmgr.AddNamespace("ttml", TtmlNamespace);
                    hasStyleHead = x.DocumentElement.SelectSingleNode("ttml:head", xnsmgr) != null;
                }
                catch
                {
                }
                if (!hasStyleHead && (subtitle.Header.Contains("[V4+ Styles]") || subtitle.Header.Contains("[V4 Styles]")))
                {
                    subtitle.Header = SubStationAlphaHeaderToTimedText(subtitle); // save new xml with styles in header
                    convertedFromSubStationAlpha = true;
                    hasStyleHead = true;
                }
            }

            var xml = new XmlDocument();
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttml", TtmlNamespace);
            nsmgr.AddNamespace("ttp", TtmlParameterNamespace);
            nsmgr.AddNamespace("tts", TtmlStylingNamespace);
            nsmgr.AddNamespace("ttm", TtmlMetadataNamespace);
            string xmlStructure = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
            "<tt xmlns=\"" + TtmlNamespace + "\" xmlns:ttp=\"" + TtmlParameterNamespace + "\" ttp:timeBase=\"media\" xmlns:tts=\"" + TtmlStylingNamespace + "\" xml:lang=\"en\" xmlns:ttm=\"" + TtmlMetadataNamespace + "\">" + Environment.NewLine +
            "   <head>" + Environment.NewLine +
            "       <metadata>" + Environment.NewLine +
            "           <ttm:title></ttm:title>" + Environment.NewLine +
            "      </metadata>" + Environment.NewLine +
            "       <styling>" + Environment.NewLine +
            "         <style xml:id=\"s0\" tts:backgroundColor=\"black\" tts:fontStyle=\"normal\" tts:fontSize=\"16px\" tts:fontFamily=\"sansSerif\" tts:color=\"white\" />" + Environment.NewLine +
            "      </styling>" + Environment.NewLine +
            "       <layout>" + Environment.NewLine +
            // Left column
            "           <region tts:extent=\"80% 40%\" tts:origin=\"10% 10%\" tts:displayAlign=\"before\" tts:textAlign=\"start\" xml:id=\"topLeft\" />" + Environment.NewLine +
            "           <region tts:extent=\"80% 40%\" tts:origin=\"10% 30%\" tts:displayAlign=\"center\" tts:textAlign=\"start\" xml:id=\"centerLeft\" />" + Environment.NewLine +
            "           <region tts:extent=\"80% 40%\" tts:origin=\"10% 50%\" tts:displayAlign=\"after\" tts:textAlign=\"start\" xml:id=\"bottomLeft\" />" + Environment.NewLine +
            // Midle column
            "           <region tts:extent=\"80% 40%\" tts:origin=\"10% 10%\" tts:displayAlign=\"before\" tts:textAlign=\"center\" xml:id=\"topCenter\" />" + Environment.NewLine +
            "           <region tts:extent=\"80% 40%\" tts:origin=\"10% 30%\" tts:displayAlign=\"center\" tts:textAlign=\"center\" xml:id=\"centerСenter\" />" + Environment.NewLine +
            "           <region tts:extent=\"80% 40%\" tts:origin=\"10% 50%\" tts:displayAlign=\"after\" tts:textAlign=\"center\" xml:id=\"bottomCenter\" />" + Environment.NewLine +
            // Right column
            "           <region tts:extent=\"80% 40%\" tts:origin=\"10% 10%\" tts:displayAlign=\"before\" tts:textAlign=\"end\" xml:id=\"topRight\" />" + Environment.NewLine +
            "           <region tts:extent=\"80% 40%\" tts:origin=\"10% 30%\" tts:displayAlign=\"center\" tts:textAlign=\"end\" xml:id=\"centerRight\" />" + Environment.NewLine +
            "           <region tts:extent=\"80% 40%\" tts:origin=\"10% 50%\" tts:displayAlign=\"after\" tts:textAlign=\"end\" xml:id=\"bottomRight\" />" + Environment.NewLine +
            "       </layout>" + Environment.NewLine +
            "   </head>" + Environment.NewLine +
            "   <body style=\"s0\">" + Environment.NewLine +
            "       <div />" + Environment.NewLine +
            "   </body>" + Environment.NewLine +
            "</tt>";
            if (!hasStyleHead || string.IsNullOrEmpty(subtitle.Header))
            {
                xml.LoadXml(xmlStructure);
            }
            else
            {
                xml.LoadXml(subtitle.Header);
                XmlNode bodyNode = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr);
                XmlNode divNode = null;
                if (bodyNode != null)
                {
                    divNode = bodyNode.SelectSingleNode("ttml:div", nsmgr);
                }

                if (divNode == null)
                {
                    divNode = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).FirstChild;
                }

                if (divNode != null)
                {
                    // Remove all but first div
                    int innerNodeCount = 0;
                    var innerNodeList = new List<XmlNode>();
                    foreach (XmlNode innerNode in bodyNode.SelectNodes("ttml:div", nsmgr))
                    {
                        if (innerNodeCount > 0)
                        {
                            innerNodeList.Add(innerNode);
                        }

                        innerNodeCount++;
                    }
                    foreach (XmlNode child in innerNodeList)
                    {
                        bodyNode.RemoveChild(child);
                    }

                    var lst = new List<XmlNode>();
                    foreach (XmlNode child in divNode.ChildNodes)
                    {
                        lst.Add(child);
                    }

                    foreach (XmlNode child in lst)
                    {
                        divNode.RemoveChild(child);
                    }
                }
                else if (bodyNode == null)  // Don't reload xml if body node exists, otherwise we rewrite header edited by styling form
                {
                    xml.LoadXml(xmlStructure);
                }
            }

            // Declare namespaces in the root node if not declared
            xml.DocumentElement.SetAttribute("xmlns", TtmlNamespace);
            xml.DocumentElement.SetAttribute("xmlns:ttp", TtmlParameterNamespace);
            xml.DocumentElement.SetAttribute("xmlns:tts", TtmlStylingNamespace);
            xml.DocumentElement.SetAttribute("xmlns:ttm", TtmlMetadataNamespace);

            XmlNode body = xml.DocumentElement.SelectSingleNode("ttml:body", nsmgr);
            string defaultStyle = Guid.NewGuid().ToString();
            if (body.Attributes["style"] != null)
            {
                defaultStyle = body.Attributes["style"].InnerText;
            }

            XmlNode div = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).SelectSingleNode("ttml:div", nsmgr);
            if (div == null)
            {
                div = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).FirstChild;
            }

            if (div == null)
            {
                div = xml.CreateElement("div");
                body.AppendChild(div);
            }

            int no = 0;
            var headerStyles = GetStylesFromHeader(ToUtf8XmlString(xml));
            var regions = GetRegionsFromHeader(ToUtf8XmlString(xml));
            var languages = GetUsedLanguages(subtitle);
            if (languages.Count > 0)
            {
                var divParentNode = div.ParentNode;

                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    if (p.Language == null)
                    {
                        if (p.NewSection)
                        {
                            div = xml.CreateElement("div", TtmlNamespace);
                            divParentNode.AppendChild(div);
                        }
                        XmlNode paragraph = MakeParagraph(subtitle, xml, defaultStyle, no, headerStyles, regions, p);
                        div.AppendChild(paragraph);
                        no++;
                    }
                }

                foreach (string language in languages)
                {
                    div = xml.CreateElement("div", TtmlNamespace);
                    XmlAttribute attr = xml.CreateAttribute("xml:lang", "http://www.w3.org/XML/1998/namespace");
                    attr.Value = language;
                    div.Attributes.Append(attr);
                    divParentNode.AppendChild(div);
                    bool firstParagraph = true;
                    foreach (Paragraph p in subtitle.Paragraphs)
                    {
                        if (p.Language != null && p.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
                        {
                            if (p.NewSection && !firstParagraph)
                            {
                                div = xml.CreateElement("div", TtmlNamespace);
                                attr = xml.CreateAttribute("xml:lang", "http://www.w3.org/XML/1998/namespace");
                                attr.Value = language;
                                div.Attributes.Append(attr);
                                divParentNode.AppendChild(div);
                            }
                            firstParagraph = false;
                            XmlNode paragraph = MakeParagraph(subtitle, xml, defaultStyle, no, headerStyles, regions, p);
                            div.AppendChild(paragraph);
                            no++;
                        }
                    }
                }

                if (divParentNode != null && divParentNode.HasChildNodes && !divParentNode.FirstChild.HasChildNodes)
                {
                    divParentNode.RemoveChild(divParentNode.FirstChild);
                }
            }
            else
            {
                var divParentNode = div.ParentNode;

                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    if (p.NewSection)
                    {
                        div = xml.CreateElement("div", TtmlNamespace);
                        divParentNode.AppendChild(div);
                    }
                    if (convertedFromSubStationAlpha && string.IsNullOrEmpty(p.Style))
                    {
                        p.Style = p.Extra;
                    }

                    XmlNode paragraph = MakeParagraph(subtitle, xml, defaultStyle, no, headerStyles, regions, p);
                    div.AppendChild(paragraph);
                    no++;
                }

                if (divParentNode != null && divParentNode.HasChildNodes && !divParentNode.FirstChild.HasChildNodes)
                {
                    divParentNode.RemoveChild(divParentNode.FirstChild);
                }
            }

            return ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty).Replace(" xmlns:tts=\"http://www.w3.org/ns/10/ttml#style\">", ">");
        }

        public static string SubStationAlphaHeaderToTimedText(Subtitle subtitle)
        {
            var x = new XmlDocument();
            x.LoadXml(new TimedText10().ToText(new Subtitle(), "tt")); // load default xml
            var xnsmgr = new XmlNamespaceManager(x.NameTable);
            xnsmgr.AddNamespace("ttml", TtmlNamespace);
            var styleHead = x.DocumentElement.SelectSingleNode("ttml:head", xnsmgr);
            styleHead.SelectSingleNode("ttml:styling", xnsmgr).RemoveAll();
            foreach (string styleName in AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header))
            {
                try
                {
                    var ssaStyle = AdvancedSubStationAlpha.GetSsaStyle(styleName, subtitle.Header);
                    if (ssaStyle != null)
                    {
                        string fontStyle = "normal";
                        if (ssaStyle.Italic)
                        {
                            fontStyle = "italic";
                        }

                        string fontWeight = "normal";
                        if (ssaStyle.Bold)
                        {
                            fontWeight = "bold";
                        }

                        AddStyleToXml(x, styleHead, xnsmgr, ssaStyle.Name, ssaStyle.FontName, fontWeight, fontStyle, Utilities.ColorToHex(ssaStyle.Primary), ssaStyle.FontSize.ToString(CultureInfo.InvariantCulture));
                    }
                }
                catch
                {
                    // ignored
                }
            }
            return x.OuterXml;
        }

        private static XmlNode MakeParagraph(Subtitle subtitle, XmlDocument xml, string defaultStyle, int no, List<string> headerStyles, List<string> regions, Paragraph p)
        {
            XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/ns/ttml");
            string text = p.Text.RemoveControlCharactersButWhiteSpace();

            string region = GetEffect(p, "region");
            if (p != null && !string.IsNullOrEmpty(p.Region) && regions.Contains(region))
            {
                region = p.Region;
            }

            if (string.IsNullOrEmpty(region))
            {
                if (text.StartsWith("{\\an1}", StringComparison.Ordinal) && AddDefaultRegionIfNotExists(xml, "bottomLeft"))
                {
                    region = "bottomLeft";
                }

                if (text.StartsWith("{\\an2}", StringComparison.Ordinal) && AddDefaultRegionIfNotExists(xml, "bottomCenter"))
                {
                    region = "bottomCenter";
                }

                if (text.StartsWith("{\\an3}", StringComparison.Ordinal) && AddDefaultRegionIfNotExists(xml, "bottomRight"))
                {
                    region = "bottomRight";
                }

                if (text.StartsWith("{\\an4}", StringComparison.Ordinal) && AddDefaultRegionIfNotExists(xml, "centerLeft"))
                {
                    region = "centerLeft";
                }

                if (text.StartsWith("{\\an5}", StringComparison.Ordinal) && AddDefaultRegionIfNotExists(xml, "centerСenter"))
                {
                    region = "centerСenter";
                }

                if (text.StartsWith("{\\an6}", StringComparison.Ordinal) && AddDefaultRegionIfNotExists(xml, "centerRight"))
                {
                    region = "centerRight";
                }

                if (text.StartsWith("{\\an7}", StringComparison.Ordinal) && AddDefaultRegionIfNotExists(xml, "topLeft"))
                {
                    region = "topLeft";
                }

                if (text.StartsWith("{\\an8}", StringComparison.Ordinal) && AddDefaultRegionIfNotExists(xml, "topCenter"))
                {
                    region = "topCenter";
                }

                if (text.StartsWith("{\\an9}", StringComparison.Ordinal) && AddDefaultRegionIfNotExists(xml, "topRight"))
                {
                    region = "topRight";
                }
            }
            text = Utilities.RemoveSsaTags(text);
            text = HtmlUtil.FixInvalidItalicTags(text);

            // Trying to parse and convert paragraph content
            try
            {
                text = string.Join("<br/>", text.SplitToLines());
                XmlDocument paragraphContent = new XmlDocument();
                paragraphContent.LoadXml($"<root>{text.Replace("&", "&amp;")}</root>");
                ConvertParagraphNodeToTtmlNode(paragraphContent.DocumentElement, xml, paragraph);
            }
            catch  // Wrong markup, clear it
            {
                text = Regex.Replace(text, "[<>]", "");
                paragraph.AppendChild(xml.CreateTextNode(text));
            }

            XmlAttribute start = xml.CreateAttribute("begin");
            start.InnerText = ConvertToTimeString(p.StartTime);
            paragraph.Attributes.Append(start);

            XmlAttribute id = xml.CreateAttribute("xml:id");
            id.InnerText = "p" + no;
            paragraph.Attributes.Append(id);

            XmlAttribute end = xml.CreateAttribute("end");
            end.InnerText = ConvertToTimeString(p.EndTime);
            paragraph.Attributes.Append(end);

            if (!string.IsNullOrEmpty(region))
            {
                XmlAttribute regionAttribute = xml.CreateAttribute("region");
                regionAttribute.InnerText = region;
                paragraph.Attributes.Append(regionAttribute);
            }

            string xmlSpace = GetEffect(p, "xml:space");
            if (!string.IsNullOrEmpty(xmlSpace))
            {
                XmlAttribute xmlSpaceAttribute = xml.CreateAttribute("xml:space");
                xmlSpaceAttribute.InnerText = xmlSpace;
                paragraph.Attributes.Append(xmlSpaceAttribute);
            }

            string ttsFontSize = GetEffect(p, "tts:fontSize");
            if (!string.IsNullOrEmpty(ttsFontSize))
            {
                XmlAttribute ttsFontSizeAttribute = xml.CreateAttribute("tts:fontSize", TtmlStylingNamespace);
                ttsFontSizeAttribute.InnerText = ttsFontSize;
                paragraph.Attributes.Append(ttsFontSizeAttribute);
            }

            string ttsFontFamily = GetEffect(p, "tts:fontFamily");
            if (!string.IsNullOrEmpty(ttsFontFamily))
            {
                XmlAttribute ttsFontFamilyAttribute = xml.CreateAttribute("tts:fontFamily", TtmlStylingNamespace);
                ttsFontFamilyAttribute.InnerText = ttsFontFamily;
                paragraph.Attributes.Append(ttsFontFamilyAttribute);
            }

            string ttsBackgroundColor = GetEffect(p, "tts:backgroundColor");
            if (!string.IsNullOrEmpty(ttsBackgroundColor))
            {
                XmlAttribute ttsBackgroundColorAttribute = xml.CreateAttribute("tts:backgroundColor", TtmlStylingNamespace);
                ttsBackgroundColorAttribute.InnerText = ttsBackgroundColor;
                paragraph.Attributes.Append(ttsBackgroundColorAttribute);
            }

            string ttsOrigin = GetEffect(p, "tts:origin");
            if (!string.IsNullOrEmpty(ttsOrigin))
            {
                XmlAttribute ttsOriginAttribute = xml.CreateAttribute("tts:origin", TtmlStylingNamespace);
                ttsOriginAttribute.InnerText = ttsOrigin;
                paragraph.Attributes.Append(ttsOriginAttribute);
            }

            string ttsExtent = GetEffect(p, "tts:extent");
            if (!string.IsNullOrEmpty(ttsExtent))
            {
                XmlAttribute ttsExtentAttribute = xml.CreateAttribute("tts:extent", TtmlStylingNamespace);
                ttsExtentAttribute.InnerText = ttsExtent;
                paragraph.Attributes.Append(ttsExtentAttribute);
            }

            string ttsTextAlign = GetEffect(p, "tts:textAlign");
            if (!string.IsNullOrEmpty(ttsTextAlign))
            {
                XmlAttribute ttsTextAlignAttribute = xml.CreateAttribute("tts:textAlign", TtmlStylingNamespace);
                ttsTextAlignAttribute.InnerText = ttsTextAlign;
                paragraph.Attributes.Append(ttsTextAlignAttribute);
            }

            if (subtitle.Header != null && p.Style != null && headerStyles.Contains(p.Style))
            {
                if (p.Style != defaultStyle)
                {
                    XmlAttribute styleAttr = xml.CreateAttribute("style");
                    styleAttr.InnerText = p.Style;
                    paragraph.Attributes.Append(styleAttr);
                }
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
                    XmlAttribute attr = ttmlXml.CreateAttribute("tts:fontStyle", TtmlStylingNamespace);
                    attr.InnerText = "italic";
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else if (child.Name == "b")
                {
                    XmlNode span = ttmlXml.CreateElement("span");
                    XmlAttribute attr = ttmlXml.CreateAttribute("tts:fontWeight", TtmlStylingNamespace);
                    attr.InnerText = "bold";
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else if (child.Name == "u")
                {
                    XmlNode span = ttmlXml.CreateElement("span");
                    XmlAttribute attr = ttmlXml.CreateAttribute("tts:textDecoration", TtmlStylingNamespace);
                    attr.InnerText = "underline";
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else if (child.Name == "font")
                {
                    XmlNode span = ttmlXml.CreateElement("span");

                    if (child.Attributes["face"] != null)
                    {
                        XmlAttribute attr = ttmlXml.CreateAttribute("tts:fontFamily", TtmlStylingNamespace);
                        attr.InnerText = child.Attributes["face"].Value;
                        span.Attributes.Append(attr);
                    }

                    if (child.Attributes["color"] != null)
                    {
                        XmlAttribute attr = ttmlXml.CreateAttribute("tts:color", TtmlStylingNamespace);
                        attr.InnerText = child.Attributes["color"].Value;
                        span.Attributes.Append(attr);
                    }

                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else if (child.Name.StartsWith("bouten-", StringComparison.Ordinal) || child.Name == "horizontalDigit" || child.Name.StartsWith("ruby-", StringComparison.Ordinal))
                {
                    var span = ttmlXml.CreateElement("span");
                    var attr = ttmlXml.CreateAttribute("style");
                    attr.InnerText = child.Name;
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

        public static bool AddDefaultRegionIfNotExists(XmlDocument xml, string region)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttml", TtmlNamespace);

            if (xml.DocumentElement.SelectSingleNode(string.Format("ttml:head//ttml:region[@xml:id='{0}']", region), nsmgr) != null)
            {
                return true;
            }

            XmlDocument defaultXml = new XmlDocument();
            defaultXml.LoadXml(new TimedText10().ToText(new Subtitle(), "tt"));

            XmlNode regionNode = defaultXml.DocumentElement.SelectSingleNode(string.Format("ttml:head//ttml:region[@xml:id='{0}']", region), nsmgr);

            if (regionNode == null)
            {
                return false;
            }

            XmlNode importedRegionNode = xml.ImportNode(regionNode, true);
            XmlNode layoutNode = xml.DocumentElement.SelectSingleNode("ttml:head/ttml:layout", nsmgr);

            if (layoutNode == null)
            {
                return false;
            }

            layoutNode.AppendChild(importedRegionNode);

            return true;
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

            const string ns = "http://www.w3.org/ns/ttml";
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttml", ns);
            XmlNode body = xml.DocumentElement.SelectSingleNode("ttml:body", nsmgr);
            if (body == null)
            {
                return;
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
                        if (frameRateMultiplier.InnerText == "999 1000" && Math.Abs(fr - 30) < 0.01)
                        {
                            Configuration.Settings.General.CurrentFrameRate = 29.97;
                        }
                        else if (frameRateMultiplier.InnerText == "999 1000" && Math.Abs(fr - 24) < 0.01)
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
            var styles = GetStylesFromHeader(subtitle.Header);
            string defaultStyle = null;
            if (body.Attributes["style"] != null)
            {
                defaultStyle = body.Attributes["style"].InnerText;
            }

            var headerStyleNodes = new List<XmlNode>();
            try
            {
                XmlNode head = xml.DocumentElement.SelectSingleNode("ttml:head", nsmgr);
                foreach (XmlNode node in head.SelectNodes("//ttml:style", nsmgr))
                {
                    headerStyleNodes.Add(node);
                }
            }
            catch
            {
            }

            var topRegions = GetRegionsTopFromHeader(xml.OuterXml);
            XmlNode lastDiv = null;
            foreach (XmlNode node in body.SelectNodes("//ttml:p", nsmgr))
            {
                try
                {
                    // Parse and convert paragraph text
                    var pText = new StringBuilder();
                    ReadParagraph(pText, node, styles, xml);

                    // Timecodes
                    ExtractTimeCodes(node, subtitle, out var begin, out var end);

                    // Style
                    var p = new Paragraph(begin, end, pText.ToString()) { Style = LookupForAttribute("style", node, nsmgr) };

                    // Lang
                    string lang = LookupForAttribute("xml:lang", node, nsmgr) ?? LookupForAttribute("lang", node, nsmgr);
                    if (lang != null)
                    {
                        p.Language = lang;
                    }

                    // Region
                    string regionP = LookupForAttribute("xml:region", node, nsmgr) ?? LookupForAttribute("region", node, nsmgr);
                    if (regionP != null)
                    {
                        p.Region = regionP;
                    }

                    // Saving attributes
                    var effectsToSave = new List<string>
                    {
                        "xml:space",
                        "tts:fontSize",
                        "tts:fontFamily",
                        "tts:backgroundColor",
                        "tts:color",
                        "tts:origin",
                        "tts:extent",
                        "tts:textAlign"
                    };

                    foreach (string effect in effectsToSave)
                    {
                        string value = LookupForAttribute(effect, node, nsmgr);

                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            SetEffect(p, effect, value);
                        }
                    }

                    // Convert region to {\an} tag or add it to effects
                    string region = LookupForAttribute("region", node, nsmgr);
                    if (!string.IsNullOrEmpty(region))
                    {
                        bool regionCorrespondToTag = false;

                        var regionTags = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("bottomLeft", "{\\an1}"),
                            new KeyValuePair<string, string>("bottomCenter", "{\\an2}"),
                            new KeyValuePair<string, string>("bottomRight", "{\\an3}"),
                            new KeyValuePair<string, string>("centerLeft", "{\\an4}"),
                            new KeyValuePair<string, string>("centerСenter", "{\\an5}"),
                            new KeyValuePair<string, string>("centerRight", "{\\an6}"),
                            new KeyValuePair<string, string>("topLeft", "{\\an7}"),
                            new KeyValuePair<string, string>("topCenter", "{\\an8}"),
                            new KeyValuePair<string, string>("topRight", "{\\an9}")
                        };

                        foreach (var regionTag in regionTags)
                        {
                            if (region == regionTag.Key)
                            {
                                p.Text = regionTag.Value + p.Text;
                                regionCorrespondToTag = true;
                                break;
                            }
                        }

                        if (!regionCorrespondToTag)
                        {
                            if (topRegions.Contains(region))
                            {
                                p.Text = "{\\an8}" + p.Text;
                            }

                            SetEffect(p, "region", region);
                        }
                    }

                    if (node.ParentNode.Name == "div")
                    {
                        // check language
                        if (node.ParentNode.Attributes["xml:lang"] != null)
                        {
                            p.Language = node.ParentNode.Attributes["xml:lang"].InnerText;
                        }
                        else if (node.ParentNode.Attributes["lang"] != null)
                        {
                            p.Language = node.ParentNode.Attributes["lang"].InnerText;
                        }

                        // check for new div
                        if (lastDiv != null && node.ParentNode != lastDiv)
                        {
                            p.NewSection = true;
                        }

                        lastDiv = node.ParentNode;
                    }

                    p.Extra = SetExtra(p);

                    p.Text = p.Text.Trim();
                    while (p.Text.Contains(Environment.NewLine + Environment.NewLine))
                    {
                        p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    }

                    subtitle.Paragraphs.Add(p);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        internal static void ExtractTimeCodes(XmlNode paragraph, Subtitle subtitle, out TimeCode begin, out TimeCode end)
        {
            string beginAttr = TryGetAttribute(paragraph, "begin", TtmlNamespace);
            string endAttr = TryGetAttribute(paragraph, "end", TtmlNamespace);
            string durAttr = TryGetAttribute(paragraph, "dur", TtmlNamespace);

            begin = new TimeCode();
            if (beginAttr.Length > 0)
            {
                begin = GetTimeCode(beginAttr, IsFrames(beginAttr));
            }
            else if (subtitle.Paragraphs.Count > 0)
            {
                begin = new TimeCode(subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.Milliseconds);
            }

            end = new TimeCode(begin.TotalMilliseconds + 3000);
            if (endAttr.Length > 0)
            {
                end = GetTimeCode(endAttr, IsFrames(endAttr));
            }
            else if (durAttr.Length > 0)
            {
                end = new TimeCode(GetTimeCode(durAttr, IsFrames(durAttr)).TotalMilliseconds + begin.TotalMilliseconds);
            }
        }

        private static string TryGetAttribute(XmlNode node, string attr, string @namespace)
        {
            if (node.Attributes[attr, @namespace] != null)
            {
                return node.Attributes[attr, @namespace].InnerText;
            }
            else if (node.Attributes[attr] != null)
            {
                return node.Attributes[attr].InnerText;
            }

            return string.Empty;
        }

        private static string LookupForAttribute(string attr, XmlNode node, XmlNamespaceManager nsmgr)
        {
            XmlNode currentNode = node;

            while (currentNode != null && currentNode.NodeType != XmlNodeType.Document)
            {
                if (currentNode.Attributes[attr] != null)
                {
                    return currentNode.Attributes[attr].Value;
                }

                currentNode = currentNode.ParentNode;
            }

            return null;
        }

        private static void SetEffect(Paragraph paragraph, string tag, string value)
        {
            if (string.IsNullOrEmpty(paragraph.Effect))
            {
                paragraph.Effect = tag + "=" + value;
            }
            else
            {
                var list = paragraph.Effect.Split('|');
                var sb = new StringBuilder();
                bool found = false;
                foreach (var s in list)
                {
                    string addValue = s;
                    var arr = s.Split('=');
                    if (arr.Length == 2)
                    {
                        if (arr[0] == tag)
                        {
                            addValue = tag + "=" + value;
                            found = true;
                        }
                    }
                    sb.Append(addValue + "|");
                }
                if (!found)
                {
                    sb.Append("|" + tag + "=" + value);
                }
                paragraph.Effect = sb.ToString().TrimEnd('|');
            }
        }

        private static string GetEffect(Paragraph paragraph, string tag)
        {
            if (paragraph?.Effect == null)
            {
                return string.Empty;
            }

            var list = paragraph.Effect.Split('|');
            foreach (var s in list)
            {
                var arr = s.Split('=');
                if (arr.Length == 2)
                {
                    if (arr[0] == tag)
                    {
                        return arr[1];
                    }
                }
            }
            return string.Empty;
        }

        private static bool IsFrames(string timeCode)
        {
            if (timeCode.Length == 12 && (timeCode[8] == '.' || timeCode[8] == ',')) // 00:00:08.292 or 00:00:08,292
            {
                return false;
            }

            if (timeCode.Length == 11 && timeCode[8] == '.') // 00:00:08.12 (last part is milliseconds / 10)
            {
                return false;
            }

            return true;
        }

        public static string SetExtra(Paragraph p)
        {
            string style = p.Style;
            if (string.IsNullOrEmpty(style))
            {
                style = "-";
            }

            string lang = p.Language;
            if (string.IsNullOrEmpty(lang))
            {
                lang = "-";
            }

            return $"{style} / {lang}";
        }

        private static void ReadParagraph(StringBuilder pText, XmlNode node, List<string> styles, XmlDocument xml)
        {
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
                    bool isItalic = false;
                    bool isBold = false;
                    bool isUnderlined = false;
                    string fontFamily = null;
                    string color = null;

                    // Composing styles

                    if (child.Attributes["style"] != null)
                    {
                        string styleName = child.Attributes["style"].Value;
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

                    ReadParagraph(pText, child, styles, xml);

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
        }

        public static TimeCode GetTimeCode(string input, bool frames)
        {
            var s = input;
            if (s.EndsWith("ms", StringComparison.Ordinal))
            {
                Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = "milliseconds";
                s = s.TrimEnd('s');
                s = s.TrimEnd('m');
                return new TimeCode(double.Parse(s.Replace(",", "."), CultureInfo.InvariantCulture));
            }
            if (s.EndsWith('s'))
            {
                Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = "seconds";
                s = s.TrimEnd('s');
                return TimeCode.FromSeconds(double.Parse(s.Replace(",", "."), CultureInfo.InvariantCulture));
            }
            if (s.EndsWith('t'))
            {
                Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = "ticks";
                s = s.TrimEnd('t');
                var ts = TimeSpan.FromTicks(long.Parse(s, CultureInfo.InvariantCulture));
                return new TimeCode(ts.TotalMilliseconds);
            }

            var parts = s.Split(':', '.', ',', ';');
            if (s.Length == 12 && s[2] == ':' && s[5] == ':' && s[8] == '.') // 00:01:39.946
            {
                Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = "hh:mm:ss.ms";
            }
            else if (s.Length == 12 && s[2] == ':' && s[5] == ':' && s[8] == ',') // 00:01:39,946
            {
                Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = "hh:mm:ss,ms";
            }
            else if (!frames && s.Length == 11 && s[2] == ':' && s[5] == ':' && s[8] == '.') // 00:01:39.96
            {
                Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = "hh:mm:ss.ms-two-digits";
                return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]) * 10);
            }

            if (frames)
            {
                Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = "frames";
                return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), FramesToMillisecondsMax999(int.Parse(parts[3])));
            }
            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), parts.Length > 3 ? int.Parse(parts[3]) : 0);
        }

        public override List<string> AlternateExtensions => new List<string> { ".itt", ".dfxp", ".ttml" };

        public override bool HasStyleSupport => Configuration.Settings.SubtitleSettings.TimedText10ShowStyleAndLanguage;

        public static List<string> GetStylesFromHeader(string xmlAsString)
        {
            var list = new List<string>();
            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(xmlAsString);
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                XmlNode head = xml.DocumentElement.SelectSingleNode("ttml:head", nsmgr);
                foreach (XmlNode node in head.SelectNodes("//ttml:style", nsmgr))
                {
                    if (node.Attributes["xml:id"] != null)
                    {
                        list.Add(node.Attributes["xml:id"].Value);
                    }
                    else if (node.Attributes["id"] != null)
                    {
                        list.Add(node.Attributes["id"].Value);
                    }
                }
            }
            catch
            {
            }
            return list;
        }

        public static List<string> GetRegionsFromHeader(string xmlAsString)
        {
            var list = new List<string>();
            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(xmlAsString);
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                XmlNode head = xml.DocumentElement.SelectSingleNode("ttml:head", nsmgr);
                foreach (XmlNode node in head.SelectNodes("//ttml:region", nsmgr))
                {
                    if (node.Attributes["xml:id"] != null)
                    {
                        list.Add(node.Attributes["xml:id"].Value);
                    }
                    else if (node.Attributes["id"] != null)
                    {
                        list.Add(node.Attributes["id"].Value);
                    }
                }
            }
            catch
            {
            }
            return list;
        }

        public static List<string> GetRegionsTopFromHeader(string xmlAsString)
        {
            var list = new List<string>();
            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(xmlAsString);
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                XmlNode head = xml.DocumentElement.SelectSingleNode("ttml:head", nsmgr);
                foreach (XmlNode node in head.SelectNodes("//ttml:region", nsmgr))
                {
                    bool top = false;
                    foreach (XmlNode styleNode in node.ChildNodes)
                    {
                        top = GetIfTopAligned(styleNode);
                        if (top)
                        {
                            break;
                        }
                    }

                    if (!top && node.Attributes != null)
                    {
                        top = GetIfTopAligned(node);
                    }

                    if (top)
                    {
                        if (node.Attributes["xml:id"] != null)
                        {
                            list.Add(node.Attributes["xml:id"].Value);
                        }
                        else if (node.Attributes["id"] != null)
                        {
                            list.Add(node.Attributes["id"].Value);
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return list;
        }

        private static bool GetIfTopAligned(XmlNode styleNode)
        {
            if (styleNode?.Attributes == null)
            {
                return false;
            }

            var origin = string.Empty;
            if (styleNode.Attributes["tts:origin"] != null)
            {
                origin = styleNode.Attributes["tts:origin"].Value;
            }
            else if (styleNode.Attributes["origin"] != null)
            {
                origin = styleNode.Attributes["origin"].Value;
            }

            var originArr = origin.Split(' ');

            var extent = string.Empty;
            if (styleNode.Attributes["tts:extent"] != null)
            {
                extent = styleNode.Attributes["tts:extent"].Value;
            }
            else if (styleNode.Attributes["extent"] != null)
            {
                extent = styleNode.Attributes["extent"].Value;
            }

            var extentArr = extent.Split(' ');

            var displayAlign = string.Empty;
            if (styleNode.Attributes["tts:displayAlign"] != null)
            {
                displayAlign = styleNode.Attributes["tts:displayAlign"].Value;
            }
            else if (styleNode.Attributes["displayAlign"] != null)
            {
                displayAlign = styleNode.Attributes["displayAlign"].Value;
            }

            if (originArr.Length == 2 && originArr[0].EndsWith("%", StringComparison.Ordinal) && originArr[1].EndsWith("%", StringComparison.Ordinal) &&
                extentArr.Length == 2 && extentArr[0].EndsWith("%", StringComparison.Ordinal) && extentArr[1].EndsWith("%", StringComparison.Ordinal) &&
                !string.IsNullOrEmpty(displayAlign))
            {
                var yPos = Convert.ToDouble(originArr[1].TrimEnd('%'), CultureInfo.InvariantCulture);
                var yExtent = Convert.ToDouble(extentArr[1].TrimEnd('%'), CultureInfo.InvariantCulture);
                if (yPos <= 15 && yExtent < 40)
                {
                    return true;
                }
                else if (yPos <= 25 && displayAlign == "before") // before = top align
                {
                    return true;
                }
            }
            return false;
        }

        public static List<string> GetUsedLanguages(Subtitle subtitle)
        {
            var list = new List<string>();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Language != null)
                {
                    string l = p.Language.ToLowerInvariant().Trim();
                    if (!list.Contains(l))
                    {
                        list.Add(l);
                    }
                }
            }
            return list;
        }

    }
}
