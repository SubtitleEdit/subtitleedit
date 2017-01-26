﻿//http://www.w3.org/TR/ttaf1-dfxp/
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
        public override string Extension
        {
            get { return ".xml"; }
        }

        public const string NameOfFormat = "Timed Text 1.0";

        public override string Name
        {
            get { return NameOfFormat; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

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
                    return paragraphs.Count > 0;
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

                        if (paragraphs.Count > 0 && new NetflixTimedText().IsMine(lines, fileName))
                            return false;

                        return paragraphs.Count > 0;
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

            XmlAttribute attr = xml.CreateAttribute("xml:id", "http://www.w3.org/ns/10/ttml#style");
            attr.InnerText = name;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:fontFamily", "http://www.w3.org/ns/10/ttml#style");
            attr.InnerText = fontFamily;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:fontWeight", "http://www.w3.org/ns/10/ttml#style");
            attr.InnerText = fontWeight;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:fontStyle", "http://www.w3.org/ns/10/ttml#style");
            attr.InnerText = fontStyle;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:color", "http://www.w3.org/ns/10/ttml#style");
            attr.InnerText = color;
            styleNode.Attributes.Append(attr);

            attr = xml.CreateAttribute("tts:fontSize", "http://www.w3.org/ns/10/ttml#style");
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
                    xnsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
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
            nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            nsmgr.AddNamespace("ttp", "http://www.w3.org/ns/10/ttml#parameter");
            nsmgr.AddNamespace("tts", "http://www.w3.org/ns/10/ttml#style");
            nsmgr.AddNamespace("ttm", "http://www.w3.org/ns/10/ttml#metadata");
            string xmlStructure = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
            "<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" ttp:timeBase=\"media\" xmlns:tts=\"http://www.w3.org/ns/ttml#style\" xml:lang=\"en\" xmlns:ttm=\"http://www.w3.org/ns/ttml#metadata\">" + Environment.NewLine +
            "   <head>" + Environment.NewLine +
            "       <metadata>" + Environment.NewLine +
            "           <ttm:title></ttm:title>" + Environment.NewLine +
            "      </metadata>" + Environment.NewLine +
            "       <styling>" + Environment.NewLine +
            "         <style id=\"s0\" tts:backgroundColor=\"black\" tts:fontStyle=\"normal\" tts:fontSize=\"16\" tts:fontFamily=\"sansSerif\" tts:color=\"white\" />" + Environment.NewLine +
            "      </styling>" + Environment.NewLine +
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
            xml.DocumentElement.SetAttribute("xmlns", "http://www.w3.org/ns/ttml");
            xml.DocumentElement.SetAttribute("xmlns:ttp", "http://www.w3.org/ns/ttml#parameter");
            xml.DocumentElement.SetAttribute("xmlns:tts", "http://www.w3.org/ns/ttml#styling");
            xml.DocumentElement.SetAttribute("xmlns:ttm", "http://www.w3.org/ns/ttml#metadata");

            XmlNode body = xml.DocumentElement.SelectSingleNode("ttml:body", nsmgr);
            string defaultStyle = Guid.NewGuid().ToString();
            if (body.Attributes["style"] != null)
                defaultStyle = body.Attributes["style"].InnerText;

            XmlNode div = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).SelectSingleNode("ttml:div", nsmgr);
            if (div == null)
                div = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).FirstChild;

            int no = 0;
            var headerStyles = GetStylesFromHeader(subtitle.Header);
            var regions = GetRegionsFromHeader(subtitle.Header);
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
                            div = xml.CreateElement("div", "http://www.w3.org/ns/ttml");
                            divParentNode.AppendChild(div);
                        }
                        XmlNode paragraph = MakeParagraph(subtitle, xml, defaultStyle, no, headerStyles, regions, p);
                        div.AppendChild(paragraph);
                        no++;
                    }
                }

                foreach (string language in languages)
                {
                    div = xml.CreateElement("div", "http://www.w3.org/ns/ttml");
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
                                div = xml.CreateElement("div", "http://www.w3.org/ns/ttml");
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
                        div = xml.CreateElement("div", "http://www.w3.org/ns/ttml");
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
            xnsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
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
                else if (child.Name == "i")
                {
                    XmlNode span = ttmlXml.CreateElement("span");
                    XmlAttribute attr = ttmlXml.CreateAttribute("tts:fontStyle", "http://www.w3.org/ns/10/ttml#style");
                    attr.InnerText = "italic";
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTTMLNode(child, ttmlXml, span);
                }
                else if (child.Name == "b")
                {
                    XmlNode span = ttmlXml.CreateElement("span");
                    XmlAttribute attr = ttmlXml.CreateAttribute("tts:fontWeight", "http://www.w3.org/ns/10/ttml#style");
                    attr.InnerText = "bold";
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTTMLNode(child, ttmlXml, span);
                }
                else if (child.Name == "u")
                {
                    XmlNode span = ttmlXml.CreateElement("span");
                    XmlAttribute attr = ttmlXml.CreateAttribute("tts:textDecoration", "http://www.w3.org/ns/10/ttml#style");
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
                        XmlAttribute attr = ttmlXml.CreateAttribute("tts:fontFamily", "http://www.w3.org/ns/10/ttml#style");
                        attr.InnerText = child.Attributes["face"].Value;
                        span.Attributes.Append(attr);
                    }

                    if (child.Attributes["color"] != null)
                    {
                        XmlAttribute attr = ttmlXml.CreateAttribute("tts:color", "http://www.w3.org/ns/10/ttml#style");
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
            XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/ns/ttml");
            string text = p.Text.RemoveControlCharactersButWhiteSpace();

            string region = GetEffect(p, "region");
            if (text.StartsWith("{\\an8}", StringComparison.Ordinal) && string.IsNullOrEmpty(region))
            {
                if (regions.Contains("top"))
                    region = "top";
                else if (regions.Contains("topCenter"))
                    region = "topCenter";
            }
            text = Utilities.RemoveSsaTags(text);

            // Trying to parse and convert pararagraph content
            try
            {
                text = string.Join("<br/>", text.SplitToLines());
                XmlDocument paragraphContent = new XmlDocument();
                paragraphContent.LoadXml(string.Format("<root>{0}</root>", text));
                ConvertParagraphNodeToTTMLNode(paragraphContent.DocumentElement, xml, paragraph);
            }
            catch(Exception e)  // Wrong markup, clear it
            {
                text = Regex.Replace(text, "[<>]", "");
                paragraph.AppendChild(xml.CreateTextNode(text));
            }

            XmlAttribute start = xml.CreateAttribute("begin");
            start.InnerText = ConvertToTimeString(p.StartTime);
            paragraph.Attributes.Append(start);

            XmlAttribute id = xml.CreateAttribute("id");
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
                XmlAttribute ttsFontSizeAttribute = xml.CreateAttribute("tts:fontSize", "http://www.w3.org/ns/10/ttml#style");
                ttsFontSizeAttribute.InnerText = ttsFontSize;
                paragraph.Attributes.Append(ttsFontSizeAttribute);
            }

            string ttsFontFamily = GetEffect(p, "tts:fontFamily");
            if (!string.IsNullOrEmpty(ttsFontFamily))
            {
                XmlAttribute ttsFontFamilyAttribute = xml.CreateAttribute("tts:fontFamily", "http://www.w3.org/ns/10/ttml#style");
                ttsFontFamilyAttribute.InnerText = ttsFontFamily;
                paragraph.Attributes.Append(ttsFontFamilyAttribute);
            }

            string ttsBackgroundColor = GetEffect(p, "tts:backgroundColor");
            if (!string.IsNullOrEmpty(ttsBackgroundColor))
            {
                XmlAttribute ttsBackgroundColorAttribute = xml.CreateAttribute("tts:backgroundColor", "http://www.w3.org/ns/10/ttml#style");
                ttsBackgroundColorAttribute.InnerText = ttsBackgroundColor;
                paragraph.Attributes.Append(ttsBackgroundColorAttribute);
            }

            string ttsOrigin = GetEffect(p, "tts:origin");
            if (!string.IsNullOrEmpty(ttsOrigin))
            {
                XmlAttribute ttsOriginAttribute = xml.CreateAttribute("tts:origin", "http://www.w3.org/ns/10/ttml#style");
                ttsOriginAttribute.InnerText = ttsOrigin;
                paragraph.Attributes.Append(ttsOriginAttribute);
            }

            string ttsExtent = GetEffect(p, "tts:extent");
            if (!string.IsNullOrEmpty(ttsExtent))
            {
                XmlAttribute ttsExtentAttribute = xml.CreateAttribute("tts:extent", "http://www.w3.org/ns/10/ttml#style");
                ttsExtentAttribute.InnerText = ttsExtent;
                paragraph.Attributes.Append(ttsExtentAttribute);
            }

            string ttsTextAlign = GetEffect(p, "tts:textAlign");
            if (!string.IsNullOrEmpty(ttsTextAlign))
            {
                XmlAttribute ttsTextAlignAttribute = xml.CreateAttribute("tts:textAlign", "http://www.w3.org/ns/10/ttml#style");
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
            _errorCount = 0;
            double startSeconds = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
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
                return;

            var frameRateAttr = xml.DocumentElement.Attributes["ttp:frameRate"];
            if (frameRateAttr != null)
            {
                double fr;
                if (double.TryParse(frameRateAttr.Value, out fr))
                {
                    if (fr > 20 && fr < 100)
                        Configuration.Settings.General.CurrentFrameRate = fr;

                    var frameRateMultiplier = xml.DocumentElement.Attributes["ttp:frameRateMultiplier"];
                    if (frameRateMultiplier != null)
                    {
                        var arr = frameRateMultiplier.InnerText.Split();
                        if (arr.Length == 2 && Utilities.IsInteger(arr[0]) && Utilities.IsInteger(arr[1]) && int.Parse(arr[1]) > 0)
                        {
                            fr = double.Parse(arr[0]) / double.Parse(arr[1]);
                            if (fr > 20 && fr < 100)
                                Configuration.Settings.General.CurrentFrameRate = fr;
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
                defaultStyle = body.Attributes["style"].InnerText;
            XmlNode lastDiv = null;

            var topRegions = GetRegionsTopFromHeader(xml.OuterXml);
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

            foreach (XmlNode node in body.SelectNodes("//ttml:p", nsmgr))
            {
                try
                {
                    var pText = new StringBuilder();
                    ReadParagraph(pText, node, styles, headerStyleNodes);

                    string start = string.Empty;
                    if (node.Attributes["begin"] != null)
                    {
                        start = node.Attributes["begin"].InnerText;
                    }
                    else if (node.Attributes["begin", ns] != null)
                    {
                        start = node.Attributes["begin", ns].InnerText;
                    }

                    string end = string.Empty;
                    if (node.Attributes["end"] != null)
                    {
                        end = node.Attributes["end"].InnerText;
                    }
                    else if (node.Attributes["end", ns] != null)
                    {
                        end = node.Attributes["end", ns].InnerText;
                    }

                    string dur = string.Empty;
                    if (node.Attributes["dur"] != null)
                    {
                        dur = node.Attributes["dur"].InnerText;
                    }
                    else if (node.Attributes["dur", ns] != null)
                    {
                        dur = node.Attributes["dur", ns].InnerText;
                    }

                    var startCode = TimeCode.FromSeconds(startSeconds);
                    if (start.Length > 0)
                    {
                        startCode = GetTimeCode(start, IsFrames(start));
                    }

                    TimeCode endCode;
                    if (end.Length > 0)
                    {
                        endCode = GetTimeCode(end, IsFrames(end));
                    }
                    else if (dur.Length > 0)
                    {
                        endCode = new TimeCode(GetTimeCode(dur, IsFrames(dur)).TotalMilliseconds + startCode.TotalMilliseconds);
                    }
                    else
                    {
                        endCode = new TimeCode(startCode.TotalMilliseconds + 3000);
                    }
                    startSeconds = endCode.TotalSeconds;

                    var p = new Paragraph(startCode, endCode, pText.ToString().Replace("   ", " ").Replace("  ", " ")) { Style = defaultStyle };
                    if (node.Attributes["style"] != null)
                        p.Style = node.Attributes["style"].InnerText;

                    if (node.Attributes["region"] != null)
                    {
                        string region = node.Attributes["region"].Value;
                        if (region == "top" || region == "topCenter" || topRegions.Contains(region))
                            p.Text = "{\\an8}" + p.Text;
                        SetEffect(p, "region", region);
                    }
                    if (node.Attributes["xml:space"] != null)
                    {
                        SetEffect(p, "xml:space", node.Attributes["xml:space"].Value);
                    }
                    if (node.Attributes["tts:fontSize"] != null)
                    {
                        SetEffect(p, "tts:fontSize", node.Attributes["tts:fontSize"].Value);
                    }
                    if (node.Attributes["tts:fontFamily"] != null)
                    {
                        SetEffect(p, "tts:fontFamily", node.Attributes["tts:fontFamily"].Value);
                    }
                    if (node.Attributes["tts:backgroundColor"] != null)
                    {
                        SetEffect(p, "tts:backgroundColor", node.Attributes["tts:backgroundColor"].Value);
                    }
                    if (node.Attributes["tts:origin"] != null)
                    {
                        SetEffect(p, "tts:origin", node.Attributes["tts:origin"].Value);
                    }
                    if (node.Attributes["tts:extent"] != null)
                    {
                        SetEffect(p, "tts:extent", node.Attributes["tts:extent"].Value);
                    }
                    if (node.Attributes["tts:textAlign"] != null)
                    {
                        SetEffect(p, "tts:textAlign", node.Attributes["tts:textAlign"].Value);
                    }

                    if (node.ParentNode.Name == "div")
                    {
                        // check language
                        if (node.ParentNode.Attributes["xml:lang"] != null)
                            p.Language = node.ParentNode.Attributes["xml:lang"].InnerText;
                        else if (node.ParentNode.Attributes["lang"] != null)
                            p.Language = node.ParentNode.Attributes["lang"].InnerText;

                        // check for new div
                        if (lastDiv != null && node.ParentNode != lastDiv)
                            p.NewSection = true;
                        lastDiv = node.ParentNode;
                    }

                    p.Extra = SetExtra(p);

                    p.Text = p.Text.Trim();
                    while (p.Text.Contains(Environment.NewLine + Environment.NewLine))
                        p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

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
            var sb = new StringBuilder();
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

        private static void ReadParagraph(StringBuilder pText, XmlNode node, List<string> styles, List<XmlNode> headerStyleNodes)
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

                    ReadParagraph(pText, child, styles, headerStyleNodes);

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
                        list.Add(node.Attributes["xml:id"].Value);
                    else if (node.Attributes["id"] != null)
                        list.Add(node.Attributes["id"].Value);
                }
            }
            catch
            {
            }
            return list;
        }

        private static bool IsStyleItalic(string styleName, IEnumerable<XmlNode> headerStyleNodes)
        {
            try
            {
                foreach (XmlNode node in headerStyleNodes)
                {
                    string id = string.Empty;
                    if (node.Attributes["xml:id"] != null)
                        id = node.Attributes["xml:id"].Value;
                    else if (node.Attributes["id"] != null)
                        id = node.Attributes["id"].Value;
                    if (!string.IsNullOrEmpty(id) && id == styleName)
                    {
                        if (node.Attributes["tts:fontStyle"] != null && node.Attributes["tts:fontStyle"].Value == "italic")
                            return true;
                        if (node.Attributes["fontStyle"] != null && node.Attributes["fontStyle"].Value == "italic")
                            return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private static bool IsStyleBold(string styleName, IEnumerable<XmlNode> headerStyleNodes)
        {
            try
            {
                foreach (XmlNode node in headerStyleNodes)
                {
                    string id = string.Empty;
                    if (node.Attributes["xml:id"] != null)
                        id = node.Attributes["xml:id"].Value;
                    else if (node.Attributes["id"] != null)
                        id = node.Attributes["id"].Value;
                    if (!string.IsNullOrEmpty(id) && id == styleName)
                    {
                        if (node.Attributes["tts:fontWeight"] != null && node.Attributes["tts:fontWeight"].Value == "bold")
                            return true;
                        if (node.Attributes["fontWeight"] != null && node.Attributes["fontWeight"].Value == "bold")
                            return true;
                    }
                }
            }
            catch
            {
            }
            return false;
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
                        list.Add(node.Attributes["xml:id"].Value);
                    else if (node.Attributes["id"] != null)
                        list.Add(node.Attributes["id"].Value);
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
