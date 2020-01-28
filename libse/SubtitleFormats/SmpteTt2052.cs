using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// SMPTE-TT 2052
    /// </summary>
    public class SmpteTt2052 : TimedText10
    {
        public override string Extension => ".xml";

        public new const string NameOfFormat = "SMPTE-TT 2052";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !(fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            if (!sb.ToString().Contains("http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt#cea608"))
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

                                AddStyleToXml(x, styleHead, xnsmgr, ssaStyle.Name, ssaStyle.FontName, fontWeight, fontStyle, Utilities.ColorToHex(ssaStyle.Primary), ssaStyle.FontSize.ToString(CultureInfo.InvariantCulture));
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
            nsmgr.AddNamespace("ttm", "http://www.w3.org/ns/10/ttml#metadata");
            nsmgr.AddNamespace("smpte", "http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt");
            nsmgr.AddNamespace("m608", "http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt#cea608");

            const string xmlStructure = @"<?xml version='1.0' encoding='utf-8'?>
<tt xml:lang='[LANG]' xmlns='http://www.w3.org/ns/ttml' xmlns:tts='http://www.w3.org/ns/ttml#styling' xmlns:ttm='http://www.w3.org/ns/ttml#metadata' xmlns:ttp='http://www.w3.org/ns/ttml#parameter' ttp:timeBase='media' ttp:frameRate='24' ttp:frameRateMultiplier='1000 1001' xmlns:smpte='http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt' xmlns:m608='http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt#cea608'>
    <head>
        <metadata>
            <ttm:title>SMPTE-TT 2052 subtitle</ttm:title>
            <ttm:desc>SMPTE Timed Text document created by Subtitle Edit</ttm:desc>
            <smpte:information xmlns:m608='http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt#cea608' origin='http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt#cea608' mode='Preserved' m608:channel='CC1' m608:programName='Demo' m608:captionService='F1C1CC'/>
        </metadata>
        <styling>
            <style xml:id='basic' tts:color='white' tts:fontFamily='Arial' tts:backgroundColor='transparent' tts:fontSize='21' tts:fontWeight='normal' tts:fontStyle='normal' />
        </styling>
        <layout>
            <region xml:id='bottom' tts:backgroundColor='transparent' tts:showBackground='whenActive' tts:origin='10% 55%' tts:extent='80% 80%' tts:displayAlign='after' />
            <region xml:id='top' tts:backgroundColor='transparent' tts:showBackground='whenActive' tts:origin='10% 10%' tts:extent='80% 80%' tts:displayAlign='before' />
        </layout>
    </head>
    <body>
        <div></div>
    </body>
</tt>";
            var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            xml.LoadXml(xmlStructure.Replace("[LANG]", languageCode));
            if (!string.IsNullOrWhiteSpace(title))
            {
                var headNode = xml.DocumentElement.SelectSingleNode("//ttml:head", nsmgr);
                var metadataNode = headNode?.SelectSingleNode("ttml:metadata", nsmgr);
                var titleNode = metadataNode?.FirstChild;
                if (titleNode != null)
                {
                    titleNode.InnerText = title;
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

                if (id != null && id == "bottom")
                {
                    hasBottomCenterRegion = true;
                }

                if (id != null && (id == "topCenter" || id == "top"))
                {
                    hasTopCenterRegion = true;
                }
            }

            foreach (var p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/ns/ttml");
                string text = p.Text;

                XmlAttribute start = xml.CreateAttribute("begin");
                start.InnerText = string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}:{3:00}", p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds));
                paragraph.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("end");
                end.InnerText = string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}:{3:00}", p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds));
                paragraph.Attributes.Append(end);

                XmlAttribute style = xml.CreateAttribute("style");
                style.InnerText = "basic";
                paragraph.Attributes.Append(style);

                XmlAttribute textAlign = xml.CreateAttribute("textAlign");
                textAlign.InnerText = "center";
                paragraph.Attributes.Append(textAlign);

                XmlAttribute regionP = xml.CreateAttribute("region");
                if (text.StartsWith("{\\an7}", StringComparison.Ordinal) || text.StartsWith("{\\an8}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal))
                {
                    if (hasTopCenterRegion)
                    {
                        regionP.InnerText = "top";
                        paragraph.Attributes.Append(regionP);
                    }
                }
                else if (hasBottomCenterRegion)
                {
                    regionP.InnerText = "bottom";
                    paragraph.Attributes.Append(regionP);
                }
                if (text.StartsWith("{\\an", StringComparison.Ordinal) && text.Length > 6 && text[5] == '}')
                {
                    text = text.Remove(0, 6);
                }

                XmlAttribute styleAttribute = xml.CreateAttribute("style");
                styleAttribute.InnerText = "basic";
                paragraph.Attributes.Append(styleAttribute);

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
                        else if (line.Substring(i).StartsWith("<i>", StringComparison.Ordinal))
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
                        else if (line.Substring(i).StartsWith("<b>", StringComparison.Ordinal))
                        {
                            currentStyle = xml.CreateNode(XmlNodeType.Element, "span", null);
                            paragraph.AppendChild(currentStyle);
                            XmlAttribute attr = xml.CreateAttribute("tts:fontWeight", "http://www.w3.org/ns/10/ttml#style");
                            attr.InnerText = "bold";
                            currentStyle.Attributes.Append(attr);
                            skipCount = 2;
                        }
                        else if (line.Substring(i).StartsWith("<font ", StringComparison.Ordinal))
                        {
                            int endIndex = line.Substring(i + 1).IndexOf('>');
                            if (endIndex > 0)
                            {
                                skipCount = endIndex + 1;
                                string fontContent = line.Substring(i, skipCount);
                                if (fontContent.Contains(" color="))
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
                        else if (line.Substring(i).StartsWith("</i>", StringComparison.Ordinal) || line.Substring(i).StartsWith("</b>", StringComparison.Ordinal) || line.Substring(i).StartsWith("</font>", StringComparison.Ordinal))
                        {
                            currentStyle = xml.CreateTextNode(string.Empty);
                            if (styles.Count > 0)
                            {
                                currentStyle = styles.Pop().CloneNode(true);
                                currentStyle.InnerText = string.Empty;
                            }
                            paragraph.AppendChild(currentStyle);
                            if (line.Substring(i).StartsWith("</font>", StringComparison.Ordinal))
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
                            if (i == 0 && italicOn && !(line.Substring(i).StartsWith("<i>", StringComparison.Ordinal)))
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
            var oldTimedText10TimeCodeFormat = Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormat;
            Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormat = "hh:mm:ss.ff";
            base.LoadSubtitle(subtitle, lines, fileName);
            Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormat = oldTimedText10TimeCodeFormat;
        }

        public override bool HasStyleSupport => false;

    }
}
