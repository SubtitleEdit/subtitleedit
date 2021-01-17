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
    /// Netflix version of timed text - time code is "00:00:04.000", tag <br /> is converted to <br/>
    /// See https://backlothelp.netflix.com/hc/en-us/articles/215758617-Timed-Text-Style-Guide-General-Requirements
    ///     https://backlothelp.netflix.com/hc/en-us/articles/115001869828-Announcement-Additional-TTML-Schema-Inspections
    /// </summary>
    public class NetflixTimedText : TimedText10
    {
        public override string Extension => ".dfxp";

        public new const string NameOfFormat = "Netflix Timed Text";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !(fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            if (!sb.ToString().Contains(">Netflix Subtitle"))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            bool convertedFromSubStationAlpha = false;
            if (subtitle.Header != null)
            {
                XmlNode styleHead = null;
                try
                {
                    var x = new XmlDocument();
                    x.LoadXml(subtitle.Header);
                    var xnsmgr = new XmlNamespaceManager(x.NameTable);
                    xnsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                    if (x.DocumentElement != null)
                    {
                        styleHead = x.DocumentElement.SelectSingleNode("ttml:head", xnsmgr);
                    }
                }
                catch
                {
                    styleHead = null;
                }
                if (styleHead == null && (subtitle.Header.Contains("[V4+ Styles]") || subtitle.Header.Contains("[V4 Styles]")))
                {
                    var x = new XmlDocument();
                    x.LoadXml(new ItunesTimedText().ToText(new Subtitle(), "tt")); // load default xml
                    var xnsmgr = new XmlNamespaceManager(x.NameTable);
                    xnsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                    if (x.DocumentElement != null)
                    {
                        styleHead = x.DocumentElement.SelectSingleNode("ttml:head", xnsmgr);
                        styleHead.SelectSingleNode("ttml:styling", xnsmgr).RemoveAll();
                        foreach (string styleName in AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header))
                        {
                            try
                            {
                                var ssaStyle = AdvancedSubStationAlpha.GetSsaStyle(styleName, subtitle.Header);

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

                                AddStyleToXml(x, styleHead, xnsmgr, ssaStyle.Name, ssaStyle.FontName, fontWeight, fontStyle, Utilities.ColorToHex(ssaStyle.Primary), ssaStyle.FontSize.ToString());
                                convertedFromSubStationAlpha = true;

                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                    subtitle.Header = x.OuterXml; // save new xml with styles in header
                }
            }

            var xml = new XmlDocument { XmlResolver = null };
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            nsmgr.AddNamespace("ttp", "http://www.w3.org/ns/10/ttml#parameter");
            nsmgr.AddNamespace("tts", "http://www.w3.org/ns/10/ttml#style");
            nsmgr.AddNamespace("ttm", "http://www.w3.org/ns/10/ttml#metadata");

            const string xmlStructure = @"<?xml version='1.0' encoding='utf-8'?>
<tt xmlns='http://www.w3.org/ns/ttml' xmlns:ttm='http://www.w3.org/ns/ttml#metadata' xmlns:tts='http://www.w3.org/ns/ttml#styling' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xml:lang='en'>
    <head>
        <metadata>
            <ttm:title>Netflix Subtitle</ttm:title>
        </metadata>
        <styling>
            <style tts:fontStyle='normal' tts:fontWeight='normal' xml:id='s1' tts:color='white' tts:fontFamily='Arial' tts:fontSize='100%'></style>
        </styling>
        <layout>
            <region tts:extent='80% 40%' tts:origin='10% 10%' tts:displayAlign='before' tts:textAlign='center' xml:id='topCenter' />
            <region tts:extent='80% 40%' tts:origin='10% 50%' tts:displayAlign='after' tts:textAlign='center' xml:id='bottomCenter' />
        </layout>
    </head>
    <body>
        <div style='s1' xml:id='d1'></div>
    </body>
</tt>";
            xml.LoadXml(xmlStructure);
            if (!string.IsNullOrWhiteSpace(title))
            {
                var headNode = xml.DocumentElement.SelectSingleNode("//ttml:head", nsmgr);
                var metadataNode = headNode?.SelectSingleNode("ttml:metadata", nsmgr);
                var titleNode = metadataNode?.FirstChild;
                if (titleNode != null)
                {
                    titleNode.InnerText = "Netflix Subtitle for " + title;
                }
            }
            var div = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).SelectSingleNode("ttml:div", nsmgr);
            bool hasBottomCenterRegion = false;
            bool hasTopCenterRegion = false;
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//ttml:head/ttml:layout/ttml:region", nsmgr))
            {
                string id = null;
                if (node.Attributes["xml:id"] != null)
                {
                    id = node.Attributes["xml:id"].Value;
                }
                else if (node.Attributes["id"] != null)
                {
                    id = node.Attributes["id"].Value;
                }

                if (id != null && id == "bottomCenter")
                {
                    hasBottomCenterRegion = true;
                }

                if (id != null && id == "topCenter")
                {
                    hasTopCenterRegion = true;
                }
            }

            int no = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/ns/ttml");
                string text = p.Text;

                XmlAttribute pid = xml.CreateAttribute("xml:id");
                pid.InnerText = "p" + ++no;
                paragraph.Attributes.Append(pid);

                XmlAttribute start = xml.CreateAttribute("begin");
                start.InnerText = string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:000}", p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds);
                paragraph.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("end");
                end.InnerText = string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:000}", p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds);
                paragraph.Attributes.Append(end);

                XmlAttribute regionP = xml.CreateAttribute("region");
                if (text.StartsWith("{\\an7}", StringComparison.Ordinal) || text.StartsWith("{\\an8}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal))
                {
                    if (hasTopCenterRegion)
                    {
                        regionP.InnerText = "topCenter";
                        paragraph.Attributes.Append(regionP);
                    }
                }
                else if (hasBottomCenterRegion)
                {
                    regionP.InnerText = "bottomCenter";
                    paragraph.Attributes.Append(regionP);
                }
                if (text.StartsWith("{\\an", StringComparison.Ordinal) && text.Length > 6 && text[5] == '}')
                {
                    text = text.Remove(0, 6);
                }

                if (convertedFromSubStationAlpha)
                {
                    if (string.IsNullOrEmpty(p.Style))
                    {
                        p.Style = p.Extra;
                    }
                }

                bool first = true;
                bool italicOn = false;
                foreach (string line in text.SplitToLines())
                {
                    if (!first)
                    {
                        XmlNode br = xml.CreateElement("br", "http://www.w3.org/ns/ttml");
                        paragraph.AppendChild(br);
                    }

                    var styles = new Stack<XmlNode>();
                    XmlNode currentStyle = xml.CreateTextNode(string.Empty);
                    paragraph.AppendChild(currentStyle);
                    int skipCount = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (skipCount > 0)
                        {
                            skipCount--;
                        }
                        else if (line.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                        {
                            styles.Push(currentStyle);
                            currentStyle = xml.CreateNode(XmlNodeType.Element, "span", null);
                            paragraph.AppendChild(currentStyle);
                            XmlAttribute attr = xml.CreateAttribute("tts:fontStyle", "http://www.w3.org/ns/10/ttml#style");
                            attr.InnerText = "italic";
                            currentStyle.Attributes.Append(attr);
                            skipCount = 2;
                            italicOn = true;
                        }
                        else if (line.Substring(i).StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                        {
                            currentStyle = xml.CreateNode(XmlNodeType.Element, "span", null);
                            paragraph.AppendChild(currentStyle);
                            XmlAttribute attr = xml.CreateAttribute("tts:fontWeight", "http://www.w3.org/ns/10/ttml#style");
                            attr.InnerText = "bold";
                            currentStyle.Attributes.Append(attr);
                            skipCount = 2;
                        }
                        else if (line.Substring(i).StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                        {
                            int endIndex = line.Substring(i + 1).IndexOf('>');
                            if (endIndex > 0)
                            {
                                skipCount = endIndex + 1;
                                string fontContent = line.Substring(i, skipCount);
                                if (fontContent.Contains(" color=", StringComparison.OrdinalIgnoreCase))
                                {
                                    var arr = fontContent.Substring(fontContent.IndexOf(" color=", StringComparison.Ordinal) + 7).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (arr.Length > 0)
                                    {
                                        string fontColor = arr[0].Trim('\'').Trim('"').Trim('\'');
                                        currentStyle = xml.CreateNode(XmlNodeType.Element, "span", null);
                                        paragraph.AppendChild(currentStyle);
                                        XmlAttribute attr = xml.CreateAttribute("tts:color", "http://www.w3.org/ns/10/ttml#style");
                                        attr.InnerText = fontColor;
                                        currentStyle.Attributes.Append(attr);
                                    }
                                }
                            }
                            else
                            {
                                skipCount = line.Length;
                            }
                        }
                        else if (line.Substring(i).StartsWith("</i>", StringComparison.OrdinalIgnoreCase) || line.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase) || line.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                        {
                            currentStyle = xml.CreateTextNode(string.Empty);
                            if (styles.Count > 0)
                            {
                                currentStyle = styles.Pop().CloneNode(true);
                                currentStyle.InnerText = string.Empty;
                            }
                            paragraph.AppendChild(currentStyle);
                            if (line.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                            {
                                skipCount = 6;
                            }
                            else
                            {
                                skipCount = 3;
                            }

                            italicOn = false;
                        }
                        else
                        {
                            if (i == 0 && italicOn && !line.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                            {
                                styles.Push(currentStyle);
                                currentStyle = xml.CreateNode(XmlNodeType.Element, "span", null);
                                paragraph.AppendChild(currentStyle);
                                XmlAttribute attr = xml.CreateAttribute("tts:fontStyle", "http://www.w3.org/ns/10/ttml#style");
                                attr.InnerText = "italic";
                                currentStyle.Attributes.Append(attr);
                            }
                            currentStyle.InnerText = currentStyle.InnerText + line[i];
                        }
                    }
                    first = false;
                }

                div.AppendChild(paragraph);
            }
            string xmlString = ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty).Replace(" xmlns:tts=\"http://www.w3.org/ns/10/ttml#style\">", ">").Replace("<br />", "<br/>");
            if (subtitle.Header == null)
            {
                subtitle.Header = xmlString;
            }

            return xmlString;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            base.LoadSubtitle(subtitle, lines, fileName);

            // Remove regions (except top center)
            subtitle.Paragraphs.ForEach(p => p.Text = Regex.Replace(p.Text, @"^({\\an[1-7,9]})", string.Empty));
        }

        public override bool HasStyleSupport => false;

    }
}
