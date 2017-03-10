//http://www.w3.org/TR/ttaf1-dfxp/
//Timed Text Markup Language (TTML) 1.0
//W3C Recommendation 18 November 2010

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        public override bool IsTimeBased => true;

        public static string TTMLNamespace = "http://www.w3.org/ns/ttml";
        public static string TTMLParameterNamespace = "http://www.w3.org/ns/ttml#parameter";
        public static string TTMLStylingNamespace = "http://www.w3.org/ns/ttml#styling";
        public static string TTMLMetadataNamespace = "http://www.w3.org/ns/ttml#metadata";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();

            if (xmlAsString.Contains("xmlns:tts=\"http://www.w3.org/2006/04"))
                return false;

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
                            return false;

                        if (paragraphs != null && (paragraphs.Count > 0 && new SmpteTt2052().IsMine(lines, fileName)))
                            return false;

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

            XmlAttribute attr = xml.CreateAttribute("xml:id", TTMLStylingNamespace);
            attr.InnerText = name;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:fontFamily", TTMLStylingNamespace);
            attr.InnerText = fontFamily;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:fontWeight", TTMLStylingNamespace);
            attr.InnerText = fontWeight;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:fontStyle", TTMLStylingNamespace);
            attr.InnerText = fontStyle;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:color", TTMLStylingNamespace);
            attr.InnerText = color;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:fontSize", TTMLStylingNamespace);
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
                    xnsmgr.AddNamespace("ttml", TTMLNamespace);
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
            nsmgr.AddNamespace("ttml", TTMLNamespace);
            nsmgr.AddNamespace("ttp", TTMLParameterNamespace);
            nsmgr.AddNamespace("tts", TTMLStylingNamespace);
            nsmgr.AddNamespace("ttm", TTMLMetadataNamespace);
            string xmlStructure = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
            "<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" ttp:timeBase=\"media\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xml:lang=\"en\" xmlns:ttm=\"http://www.w3.org/ns/ttml#metadata\">" + Environment.NewLine +
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
                XmlNode divNode = bodyNode.SelectSingleNode("ttml:div", nsmgr);
                if (divNode == null)
                    divNode = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).FirstChild;
                if (divNode != null)
                {
                    // Remove all but first div
                    int innerNodeCount = 0;
                    var innerNodeList = new List<XmlNode>();
                    foreach (XmlNode innerNode in bodyNode.SelectNodes("ttml:div", nsmgr))
                    {
                        if (innerNodeCount > 0)
                            innerNodeList.Add(innerNode);
                        innerNodeCount++;
                    }
                    foreach (XmlNode child in innerNodeList)
                        bodyNode.RemoveChild(child);

                    var lst = new List<XmlNode>();
                    foreach (XmlNode child in divNode.ChildNodes)
                        lst.Add(child);
                    foreach (XmlNode child in lst)
                        divNode.RemoveChild(child);
                }
                else
                {
                    xml.LoadXml(xmlStructure);
                }
            }

            // Declare namespaces in the root node
            xml.DocumentElement.SetAttribute("xmlns", TTMLNamespace);
            xml.DocumentElement.SetAttribute("xmlns:ttp", TTMLParameterNamespace);
            xml.DocumentElement.SetAttribute("xmlns:tts", TTMLStylingNamespace);
            xml.DocumentElement.SetAttribute("xmlns:ttm", TTMLMetadataNamespace);

            XmlNode body = xml.DocumentElement.SelectSingleNode("ttml:body", nsmgr);
            string defaultStyle = Guid.NewGuid().ToString();
            if (body.Attributes["style"] != null)
                defaultStyle = body.Attributes["style"].InnerText;

            XmlNode div = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).SelectSingleNode("ttml:div", nsmgr);
            if (div == null)
                div = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).FirstChild;

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
                            div = xml.CreateElement("div", TTMLNamespace);
                            divParentNode.AppendChild(div);
                        }
                        XmlNode paragraph = MakeParagraph(subtitle, xml, defaultStyle, no, headerStyles, regions, p);
                        div.AppendChild(paragraph);
                        no++;
                    }
                }

                foreach (string language in languages)
                {
                    div = xml.CreateElement("div", TTMLNamespace);
                    XmlAttribute attr = xml.CreateAttribute("xml:lang", "http://www.w3.org/XML/1998/namespace");
                    attr.Value = language;
                    div.Attributes.Append(attr);
                    divParentNode.AppendChild(div);
                    bool firstParagraph = true;
                    foreach (Paragraph p in subtitle.Paragraphs)
                    {
                        if (p.Language == language)
                        {
                            if (p.NewSection && !firstParagraph)
                            {
                                div = xml.CreateElement("div", TTMLNamespace);
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
            }
            else
            {
                var divParentNode = div.ParentNode;

                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    if (p.NewSection)
                    {
                        div = xml.CreateElement("div", TTMLNamespace);
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
            }

            return ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty).Replace(" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\">", ">");
        }

        public static string SubStationAlphaHeaderToTimedText(Subtitle subtitle)
        {
            var x = new XmlDocument();
            x.LoadXml(new TimedText10().ToText(new Subtitle(), "tt")); // load default xml
            var xnsmgr = new XmlNamespaceManager(x.NameTable);
            xnsmgr.AddNamespace("ttml", TTMLNamespace);
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
                            fontStyle = "italic";
                        string fontWeight = "normal";
                        if (ssaStyle.Bold)
                            fontWeight = "bold";
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


        private static void ConvertParagraphNodeToTTMLNode(XmlNode node, XmlDocument ttmlXml, XmlNode ttmlNode)
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

                    ConvertParagraphNodeToTTMLNode(child, ttmlXml, br);
                }
                else if (child.Name == "i")
                {
                    XmlNode span = ttmlXml.CreateElement("span");
                    XmlAttribute attr = ttmlXml.CreateAttribute("tts:fontStyle", TTMLStylingNamespace);
                    attr.InnerText = "italic";
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTTMLNode(child, ttmlXml, span);
                }
                else if (child.Name == "b")
                {
                    XmlNode span = ttmlXml.CreateElement("span");
                    XmlAttribute attr = ttmlXml.CreateAttribute("tts:fontWeight", TTMLStylingNamespace);
                    attr.InnerText = "bold";
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTTMLNode(child, ttmlXml, span);
                }
                else if (child.Name == "u")
                {
                    XmlNode span = ttmlXml.CreateElement("span");
                    XmlAttribute attr = ttmlXml.CreateAttribute("tts:textDecoration", TTMLStylingNamespace);
                    attr.InnerText = "underline";
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTTMLNode(child, ttmlXml, span);
                }
                else if (child.Name == "font")
                {
                    XmlNode span = ttmlXml.CreateElement("span");

                    if (child.Attributes["face"] != null)
                    {
                        XmlAttribute attr = ttmlXml.CreateAttribute("tts:fontFamily", TTMLStylingNamespace);
                        attr.InnerText = child.Attributes["face"].Value;
                        span.Attributes.Append(attr);
                    }

                    if (child.Attributes["color"] != null)
                    {
                        XmlAttribute attr = ttmlXml.CreateAttribute("tts:color", TTMLStylingNamespace);
                        attr.InnerText = child.Attributes["color"].Value;
                        span.Attributes.Append(attr);
                    }

                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTTMLNode(child, ttmlXml, span);
                }
                else    // Default - skip node
                {
                    ConvertParagraphNodeToTTMLNode(child, ttmlXml, ttmlNode);
                }
            }
        }

        private static XmlNode MakeParagraph(Subtitle subtitle, XmlDocument xml, string defaultStyle, int no, List<string> headerStyles, List<string> regions, Paragraph p)
        {
            XmlNode paragraph = xml.CreateElement("p");
            string text = p.Text.RemoveControlCharactersButWhiteSpace();

            string region = GetEffect(p, "region");
            if (string.IsNullOrEmpty(region))
            {
                if (text.StartsWith("{\\an1}", StringComparison.Ordinal) && regions.Contains("bottomLeft"))
                    region = "bottomLeft";

                if (text.StartsWith("{\\an2}", StringComparison.Ordinal) && regions.Contains("bottomCenter"))
                    region = "bottomCenter";

                if (text.StartsWith("{\\an3}", StringComparison.Ordinal) && regions.Contains("bottomRight"))
                    region = "bottomRight";

                if (text.StartsWith("{\\an4}", StringComparison.Ordinal) && regions.Contains("centerLeft"))
                    region = "centerLeft";

                if (text.StartsWith("{\\an5}", StringComparison.Ordinal) && regions.Contains("centerСenter"))
                    region = "centerСenter";

                if (text.StartsWith("{\\an6}", StringComparison.Ordinal) && regions.Contains("centerRight"))
                    region = "centerRight";

                if (text.StartsWith("{\\an7}", StringComparison.Ordinal) && regions.Contains("topLeft"))
                    region = "topLeft";

                if (text.StartsWith("{\\an8}", StringComparison.Ordinal) && regions.Contains("topCenter"))
                    region = "topCenter";

                if (text.StartsWith("{\\an9}", StringComparison.Ordinal) && regions.Contains("topRight"))
                    region = "topRight";
            }
            text = Utilities.RemoveSsaTags(text);
            text = HtmlUtil.FixInvalidItalicTags(text);

            // Trying to parse and convert pararagraph content
            try
            {
                text = string.Join("<br/>", text.SplitToLines());
                XmlDocument paragraphContent = new XmlDocument();
                paragraphContent.LoadXml(string.Format("<root>{0}</root>", text));
                ConvertParagraphNodeToTTMLNode(paragraphContent.DocumentElement, xml, paragraph);
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
                XmlAttribute ttsFontSizeAttribute = xml.CreateAttribute("tts:fontSize", TTMLStylingNamespace);
                ttsFontSizeAttribute.InnerText = ttsFontSize;
                paragraph.Attributes.Append(ttsFontSizeAttribute);
            }

            string ttsFontFamily = GetEffect(p, "tts:fontFamily");
            if (!string.IsNullOrEmpty(ttsFontFamily))
            {
                XmlAttribute ttsFontFamilyAttribute = xml.CreateAttribute("tts:fontFamily", TTMLStylingNamespace);
                ttsFontFamilyAttribute.InnerText = ttsFontFamily;
                paragraph.Attributes.Append(ttsFontFamilyAttribute);
            }

            string ttsBackgroundColor = GetEffect(p, "tts:backgroundColor");
            if (!string.IsNullOrEmpty(ttsBackgroundColor))
            {
                XmlAttribute ttsBackgroundColorAttribute = xml.CreateAttribute("tts:backgroundColor", TTMLStylingNamespace);
                ttsBackgroundColorAttribute.InnerText = ttsBackgroundColor;
                paragraph.Attributes.Append(ttsBackgroundColorAttribute);
            }

            string ttsOrigin = GetEffect(p, "tts:origin");
            if (!string.IsNullOrEmpty(ttsOrigin))
            {
                XmlAttribute ttsOriginAttribute = xml.CreateAttribute("tts:origin", TTMLStylingNamespace);
                ttsOriginAttribute.InnerText = ttsOrigin;
                paragraph.Attributes.Append(ttsOriginAttribute);
            }

            string ttsExtent = GetEffect(p, "tts:extent");
            if (!string.IsNullOrEmpty(ttsExtent))
            {
                XmlAttribute ttsExtentAttribute = xml.CreateAttribute("tts:extent", TTMLStylingNamespace);
                ttsExtentAttribute.InnerText = ttsExtent;
                paragraph.Attributes.Append(ttsExtentAttribute);
            }

            string ttsTextAlign = GetEffect(p, "tts:textAlign");
            if (!string.IsNullOrEmpty(ttsTextAlign))
            {
                XmlAttribute ttsTextAlignAttribute = xml.CreateAttribute("tts:textAlign", TTMLStylingNamespace);
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

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            // Load xml
            string allText = String.Join(Environment.NewLine, lines).RemoveControlCharactersButWhiteSpace().Trim();
            
            var xml = new XmlDocument { XmlResolver = null };
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttml", TTMLNamespace);

            try
            {
                xml.LoadXml(allText);
            }
            catch
            {
                xml.LoadXml(allText.Replace(" & ", " &amp; ").Replace("Q&A", "Q&amp;A").RemoveControlCharactersButWhiteSpace().Trim());
            }

            // Extracting frame rate
            var frameRateAttr = xml.DocumentElement.Attributes["ttp:frameRate"];
            var frameRateMultiplierAttr = xml.DocumentElement.Attributes["ttp:frameRateMultiplier"];

            double frameRate = Configuration.Settings.General.CurrentFrameRate;
            double frameRateMultiplierNumerator = 1;
            double frameRateMultiplierDenominator = 1;

            int fr;
            if (frameRateAttr != null && int.TryParse(frameRateAttr.Value, out fr))
            {
                if (frameRateMultiplierAttr != null)
                {
                    string[] nd = frameRateMultiplierAttr.InnerText.Split();
                    int n, d;
                    if (nd.Length == 2 && int.TryParse(nd[0], out n) && int.TryParse(nd[1], out d) && n > 0 && d > 0)
                    {
                        frameRateMultiplierNumerator = n;
                        frameRateMultiplierDenominator = d;
                    }
                }
            }

            double frameRateMultiplier = frameRateMultiplierNumerator / frameRateMultiplierDenominator;
            double resultFrameRate = frameRate * frameRateMultiplier;

            var topRegions = GetRegionsTopFromHeader(xml.OuterXml);

            if (resultFrameRate > 20 && resultFrameRate < 100)
            {
                Configuration.Settings.General.CurrentFrameRate = frameRate;
            }

            if (BatchSourceFrameRate.HasValue)
            {
                Configuration.Settings.General.CurrentFrameRate = BatchSourceFrameRate.Value;
            }

            subtitle.Header = allText;

            Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = null;

            foreach (XmlNode pNode in xml.SelectNodes("//ttml:body//ttml:p", nsmgr))
            {
                try
                {
                    var pText = new StringBuilder();
                    ReadParagraph(pText, pNode);

                    TimeCode begin, end;
                    ExtractTimeCodes(pNode, subtitle, out begin, out end);

                    var p = new Paragraph(begin, end, pText.ToString());
                    p.Style = LookupForAttribute("style", pNode, nsmgr);

                    List<string> effects = new List<string>();
                    effects.Add("xml:space");
                    effects.Add("tts:fontSize");
                    effects.Add("tts:fontFamily");
                    effects.Add("tts:backgroundColor");
                    effects.Add("tts:color");
                    effects.Add("tts:origin");
                    effects.Add("tts:extent");
                    effects.Add("tts:textAlign");

                    foreach (string effect in effects)
                    {
                        string value = LookupForAttribute(effect, pNode, nsmgr);

                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            SetEffect(p, effect, value);
                        }
                    }

                    // Convert region to {\an} tag or add it to effects
                    string region = LookupForAttribute("region", pNode, nsmgr);
                    if (!string.IsNullOrEmpty(region))
                    {
                        bool regionCorrespondToTag = false;

                        List<KeyValuePair<string, string>> regionTags = new List<KeyValuePair<string, string>>();
                        regionTags.Add(new KeyValuePair<string, string>("bottomLeft", "{\\an1}"));
                        regionTags.Add(new KeyValuePair<string, string>("bottomCenter", "{\\an2}"));
                        regionTags.Add(new KeyValuePair<string, string>("bottomRight", "{\\an3}"));
                        regionTags.Add(new KeyValuePair<string, string>("centerLeft", "{\\an4}"));
                        regionTags.Add(new KeyValuePair<string, string>("centerСenter", "{\\an5}"));
                        regionTags.Add(new KeyValuePair<string, string>("centerRight", "{\\an6}"));
                        regionTags.Add(new KeyValuePair<string, string>("topLeft", "{\\an7}"));
                        regionTags.Add(new KeyValuePair<string, string>("topCenter", "{\\an8}"));
                        regionTags.Add(new KeyValuePair<string, string>("topRight", "{\\an9}"));

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
                            SetEffect(p, "region", region);
                        }
                    }

                    string lang = LookupForAttribute("xml:lang", pNode, nsmgr);
                    if (lang == null)
                    {
                        lang = LookupForAttribute("lang", pNode, nsmgr);
                    }
                    if (lang != null)
                    {
                        p.Language = lang;
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
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }

            subtitle.Renumber();
        }

        private void ExtractTimeCodes(XmlNode paragraph, Subtitle subtitle, out TimeCode begin, out TimeCode end)
        {
            string beginAttr = TryGetAttribute(paragraph, "begin", TTMLNamespace);
            string endAttr = TryGetAttribute(paragraph, "end", TTMLNamespace);
            string durAttr = TryGetAttribute(paragraph, "dur", TTMLNamespace);

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

        private string TryGetAttribute(XmlNode node, string attr, string @namespace)
        {
            if (node.Attributes[attr] != null)
            {
                return node.Attributes[attr].InnerText;
            }
            else if (node.Attributes[attr, @namespace] != null)
            {
                return node.Attributes[attr, @namespace].InnerText;
            }

            return string.Empty;
        }

        private string LookupForAttribute(string attr, XmlNode node, XmlNamespaceManager nsmgr)
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
            if (paragraph == null || paragraph.Effect == null)
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
                return false;
            if (timeCode.Length == 11 && timeCode[8] == '.') // 00:00:08.12 (last part is milliseconds / 10)
                return false;
            return true;
        }

        public static string SetExtra(Paragraph p)
        {
            string style = p.Style;
            if (string.IsNullOrEmpty(style))
                style = "-";
            string lang = p.Language;
            if (string.IsNullOrEmpty(lang))
                lang = "-";
            return string.Format("{0} / {1}", style, lang);
        }

        private static void ReadParagraph(StringBuilder pText, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Text)
                {
                    pText.Append(child.Value);
                }
                else if (child.Name == "br")
                {
                    pText.AppendLine();
                }
                else if (child.Name == "span")
                {
                    bool isItalic = false;
                    bool isBold = false;
                    bool isUnderlined = false;
                    string fontFamily = null;
                    string color = null;

                    // Composing styles
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
                            pText.Append(string.Format(" face=\"{0}\"", fontFamily));
                        }

                        if (!string.IsNullOrEmpty(color))
                        {
                            pText.Append(string.Format(" color=\"{0}\"", color));
                        }

                        pText.Append(">");
                    }

                    ReadParagraph(pText, child);

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

        public static TimeCode GetTimeCode(string s, bool frames)
        {
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

            var parts = s.Split(':', '.', ',');
            if (s.Length == 12 && s[2] == ':' && s[5] == ':' && s[8] == '.') // 00:01:39.946
            {
                Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = "hh:mm:ss.ms";
            }
            else if (s.Length == 12 && s[2] == ':' && s[5] == ':' && s[8] == ',') // 00:01:39.946
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
            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
        }

        public override List<string> AlternateExtensions
        {
            get
            {
                return new List<string> { ".itt", ".dfxp", ".ttml" }; // iTunes Timed Text + ...
            }
        }

        public override bool HasStyleSupport
        {
            get
            {
                return Configuration.Settings.SubtitleSettings.TimedText10ShowStyleAndLanguage;
            }
        }

        public static List<string> GetStylesFromHeader(string xmlAsString)
        {
            var styles = new List<string>();
            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(xmlAsString);
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ttml", TTMLNamespace);
                foreach (XmlNode node in xml.SelectNodes("//ttml:head//ttml:style", nsmgr))
                {
                    if (node.Attributes["xml:id"] != null)
                    {
                        styles.Add(node.Attributes["xml:id"].Value);
                    }
                    else if (node.Attributes["id"] != null)
                    {
                        styles.Add(node.Attributes["id"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            return styles;
        }

        public static List<string> GetRegionsFromHeader(string xmlAsString)
        {
            var regions = new List<string>();
            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(xmlAsString);
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ttml", TTMLNamespace);
                foreach (XmlNode node in xml.SelectNodes("//ttml:head//ttml:region", nsmgr))
                {
                    if (node.Attributes["xml:id"] != null)
                    {
                        regions.Add(node.Attributes["xml:id"].Value);
                    }
                    else if (node.Attributes["id"] != null)
                    {
                        regions.Add(node.Attributes["id"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            return regions;
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
                        if (styleNode.Attributes != null)
                        {
                            var origin = string.Empty;
                            if (styleNode.Attributes["tts:origin"] != null)
                                origin = styleNode.Attributes["tts:origin"].Value;
                            else if (styleNode.Attributes["origin"] != null)
                                origin = styleNode.Attributes["origin"].Value;
                            var arr = origin.Split(' ');
                            if (arr.Length == 2 && arr[0].EndsWith("%") && arr[1].EndsWith("%"))
                            {
                                var n1 = Convert.ToDouble(arr[0].TrimEnd('%'), CultureInfo.InvariantCulture);
                                var n2 = Convert.ToDouble(arr[1].TrimEnd('%'), CultureInfo.InvariantCulture);
                                if (Math.Abs(n1 - 10) < 2 && Math.Abs(n2 - 10) < 5)
                                {
                                    top = true;
                                    break;
                                }
                                break;
                            }
                        }
                    }

                    if (top)
                    {
                        if (node.Attributes["xml:id"] != null)
                            list.Add(node.Attributes["xml:id"].Value);
                        else if (node.Attributes["id"] != null)
                            list.Add(node.Attributes["id"].Value);
                    }
                }
            }
            catch
            {
            }
            return list;
        }

        public static List<string> GetUsedLanguages(Subtitle subtitle)
        {
            var list = new List<string>();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Language != null)
                {
                    string l = p.Language.ToLower().Trim();
                    if (!list.Contains(l))
                        list.Add(l);
                }
            }
            return list;
        }
    }
}
