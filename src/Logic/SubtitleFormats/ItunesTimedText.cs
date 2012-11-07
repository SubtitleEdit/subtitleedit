using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class ItunesTimedText : TimedText10
    {
        public override string Extension
        {
            get { return ".itt"; }
        }

        public override string Name
        {
            get { return "iTunes Timed Text"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.ToLower().EndsWith(Extension))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            XmlNode styleHead = null;
            if (subtitle.Header != null)
            {
                try
                {
                    var x = new XmlDocument();
                    x.LoadXml(subtitle.Header);
                    var xnsmgr = new XmlNamespaceManager(x.NameTable);
                    xnsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                    styleHead = x.DocumentElement.SelectSingleNode("ttml:head", xnsmgr);
                }
                catch
                {
                }
                if (styleHead == null && (subtitle.Header.Contains("[V4+ Styles]") || subtitle.Header.Contains("[V4 Styles]")))
                {
                    var x = new XmlDocument();
                    x.LoadXml(new TimedText10().ToText(new Subtitle(), "tt")); // load default xml
                    var xnsmgr = new XmlNamespaceManager(x.NameTable);
                    xnsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                    styleHead = x.DocumentElement.SelectSingleNode("ttml:head", xnsmgr);
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
                                AddStyleToXml(x, styleHead, xnsmgr, ssaStyle.Name, ssaStyle.FontName, fontWeight, fontStyle, Utilities.ColorToHex(ssaStyle.Primary), ssaStyle.FontSize.ToString());
                            }
                        }
                        catch
                        {
                        }
                    }
                    subtitle.Header = x.OuterXml; // save new xml with styles in header
                }
            }

            var xml = new XmlDocument();
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            nsmgr.AddNamespace("ttp", "http://www.w3.org/ns/10/ttml#parameter");
            nsmgr.AddNamespace("tts", "http://www.w3.org/ns/10/ttml#style");
            nsmgr.AddNamespace("ttm", "http://www.w3.org/ns/10/ttml#metadata");

            string frameRate = "24";
            string frameRateMultiplier = "999 1000";
            if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 30.0) < 0.2)
            {
                frameRate = "30";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 25.0) < 0.01)
            {
                frameRate = "25";
                frameRateMultiplier = "1 1";
            }

            string language = "en-US";
            string xmlStructure = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
            "<tt xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tt=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" xml:lang=\"" + language + "\" ttp:timeBase=\"smpte\" ttp:frameRate=\"" + frameRate + "\" ttp:frameRateMultiplier=\"" + frameRateMultiplier +"\" ttp:dropMode=\"nonDrop\">" + Environment.NewLine +
            "   <head>" + Environment.NewLine +
            "       <styling>" + Environment.NewLine +          
            "         <style tts:fontSize=\"100%\" tts:color=\"white\" tts:fontStyle=\"normal\" tts:fontWeight=\"normal\" tts:fontFamily=\"sansSerif\" xml:id=\"normal\"/>" + Environment.NewLine +
            "         <style tts:fontSize=\"100%\" tts:color=\"white\" tts:fontStyle=\"normal\" tts:fontWeight=\"bold\" tts:fontFamily=\"sansSerif\" xml:id=\"bold\"/>" + Environment.NewLine +
            "         <style tts:fontSize=\"100%\" tts:color=\"white\" tts:fontStyle=\"italic\" tts:fontWeight=\"normal\" tts:fontFamily=\"sansSerif\" xml:id=\"italic\"/>" + Environment.NewLine +
            "      </styling>" + Environment.NewLine +
            "      <layout>" + Environment.NewLine +
            "        <region xml:id=\"top\" tts:origin=\"0% 0%\" tts:extent=\"100% 15%\" tts:textAlign=\"center\" tts:displayAlign=\"before\"/>" + Environment.NewLine +
            "        <region xml:id=\"bottom\" tts:origin=\"0% 85%\" tts:extent=\"100% 15%\" tts:textAlign=\"center\" tts:displayAlign=\"after\"/>" + Environment.NewLine +
            "      </layout>" + Environment.NewLine +
            "   </head>" + Environment.NewLine +
            "   <body>" + Environment.NewLine +
            "       <div />" + Environment.NewLine +
            "   </body>" + Environment.NewLine +
            "</tt>";
            if (styleHead == null)
            {
                xml.LoadXml(xmlStructure);
            }
            else
            {
                xml.LoadXml(subtitle.Header);
                XmlNode divNode = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).SelectSingleNode("ttml:div", nsmgr);
                if (divNode == null)
                    divNode = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).FirstChild;
                if (divNode != null)
                {
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


            XmlNode body = xml.DocumentElement.SelectSingleNode("ttml:body", nsmgr);
            string defaultStyle = Guid.NewGuid().ToString();
            if (body.Attributes["style"] != null)
                defaultStyle = body.Attributes["style"].InnerText;

            XmlNode div = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).SelectSingleNode("ttml:div", nsmgr);
            if (div == null)
                div = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).FirstChild;

            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/ns/ttml");
                string text = p.Text;
                
                XmlAttribute regionP = xml.CreateAttribute("region");
                regionP.InnerText = "bottom";
                paragraph.Attributes.Append(regionP);
                if (text.StartsWith("{\\an7}") || text.StartsWith("{\\an8}") || text.StartsWith("{\\an9}"))
                {
                    regionP.InnerText = "top";
                }

                if (!string.IsNullOrEmpty(p.Extra))
                {
                    XmlAttribute styleP = xml.CreateAttribute("style");
                    styleP.InnerText = p.Extra;
                    paragraph.Attributes.Append(styleP);
                }

                bool first = true;
                foreach (string line in text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!first)
                    {
                        XmlNode br = xml.CreateElement("br", "http://www.w3.org/ns/ttml");
                        paragraph.AppendChild(br);
                    }

                    var sb = new StringBuilder();
                    System.Collections.Generic.Stack<XmlNode> styles = new Stack<XmlNode>();
                    XmlNode currentStyle = xml.CreateTextNode(string.Empty);
                    paragraph.AppendChild(currentStyle);
                    int skipCount = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (skipCount > 0)
                        {
                            skipCount--;
                        }
                        else if (line.Substring(i).StartsWith("<i>"))
                        {
                            styles.Push(currentStyle);
                            currentStyle = xml.CreateNode(XmlNodeType.Element, "span", null);
                            paragraph.AppendChild(currentStyle);
                            XmlAttribute attr = xml.CreateAttribute("tts:fontStyle", "http://www.w3.org/ns/10/ttml#style");
                            attr.InnerText = "italic";
                            currentStyle.Attributes.Append(attr);
                            skipCount = 2;
                        }
                        else if (line.Substring(i).StartsWith("<b>"))
                        {
                            currentStyle = xml.CreateNode(XmlNodeType.Element, "span", null);
                            paragraph.AppendChild(currentStyle);
                            XmlAttribute attr = xml.CreateAttribute("tts:fontWeight", "http://www.w3.org/ns/10/ttml#style");
                            attr.InnerText = "bold";
                            currentStyle.Attributes.Append(attr);
                            skipCount = 2;
                        }
                        else if (line.Substring(i).StartsWith("<font "))
                        {
                            int endIndex = line.Substring(i + 1).IndexOf(">");
                            if (endIndex > 0)
                            {
                                skipCount = endIndex + 1;
                                string fontContent = line.Substring(i, skipCount);
                                if (fontContent.Contains(" color="))
                                {
                                    string[] arr = fontContent.Substring(fontContent.IndexOf(" color=") + 7).Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
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
                        else if (line.Substring(i).StartsWith("</i>") || line.Substring(i).StartsWith("</b>") || line.Substring(i).StartsWith("</font>"))
                        {
                            currentStyle = xml.CreateTextNode(string.Empty);
                            if (styles.Count > 0)
                            {
                                currentStyle = styles.Pop().CloneNode(true);
                                currentStyle.InnerText = string.Empty;
                            }
                            paragraph.AppendChild(currentStyle);
                            if (line.Substring(i).StartsWith("</font>"))
                                skipCount = 6;
                            else
                                skipCount = 3;
                        }
                        else
                        {
                            currentStyle.InnerText = currentStyle.InnerText + line.Substring(i, 1);
                        }
                    }
                    first = false;
                }

                XmlAttribute start = xml.CreateAttribute("begin");
                start.InnerText = ConvertToTimeString(p.StartTime);
                paragraph.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("end");
                end.InnerText = ConvertToTimeString(p.EndTime);
                paragraph.Attributes.Append(end);

                if (subtitle.Header != null && p.Extra != null && GetStylesFromHeader(subtitle.Header).Contains(p.Extra))
                {
                    if (p.Extra != defaultStyle)
                    {
                        XmlAttribute styleAttr = xml.CreateAttribute("style");
                        styleAttr.InnerText = p.Extra;
                        paragraph.Attributes.Append(styleAttr);
                    }
                }

                div.AppendChild(paragraph);
                no++;
            }

            return ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty).Replace(" xmlns:tts=\"http://www.w3.org/ns/10/ttml#style\">", ">");
        }

    }
}
