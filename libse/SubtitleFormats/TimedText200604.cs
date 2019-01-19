using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TimedText200604 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Timed Text draft 2006-04";

        public bool UseCDataForParagraphText { get; set; }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Replace("http://www.w3.org/2006/04/ttaf1#styling\"xml:lang", "http://www.w3.org/2006/04/ttaf1#styling\" xml:lang").Trim();

            if (xmlAsString.Contains("http://www.w3.org/2006/10"))
            {
                return false;
            }

            if (!UseCDataForParagraphText && xmlAsString.Contains("<![CDATA["))
            {
                return false;
            }

            if (xmlAsString.Contains("http://www.w3.org/") &&
                xmlAsString.Contains("/ttaf1"))
            {
                var xml = new XmlDocument { XmlResolver = null };
                try
                {
                    xml.LoadXml(xmlAsString.RemoveControlCharactersButWhiteSpace());

                    var nsmgr = new XmlNamespaceManager(xml.NameTable);
                    nsmgr.AddNamespace("ttaf1", xml.DocumentElement.NamespaceURI);

                    XmlNode div;
                    var body = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr);
                    if (body == null)
                    {
                        div = xml.DocumentElement;
                    }
                    else
                    {
                        div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).SelectSingleNode("ttaf1:div", nsmgr);
                    }

                    if (div == null)
                    {
                        div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).FirstChild;
                    }

                    int numberOfParagraphs = div.ChildNodes.Count;
                    return numberOfParagraphs > 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return false;
                }
            }
            return false;
        }

        private static string ConvertToTimeString(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds:000}";
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<tt xmlns=\"http://www.w3.org/2006/04/ttaf1\" xmlns:tts=\"http://www.w3.org/2006/04/ttaf1#styling\">" + Environment.NewLine +
                "   <head>" + Environment.NewLine +
                "       <styling>" + Environment.NewLine +
                "         <style id=\"defaultSpeaker\" tts:fontSize=\"12px\" tts:fontFamily=\"SansSerif\" tts:fontWeight=\"normal\" tts:fontStyle=\"normal\" tts:textDecoration=\"none\" tts:color=\"white\" tts:backgroundColor=\"black\" tts:textAlign=\"center\" />" + Environment.NewLine +
                "      </styling>" + Environment.NewLine +
                "   </head>" + Environment.NewLine +
                "   <body id=\"thebody\" style=\"defaultCaption\">" + Environment.NewLine +
                "       <div />" + Environment.NewLine +
                "   </body>" + Environment.NewLine +
                "</tt>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttaf1", "http://www.w3.org/2006/04/ttaf1");
            nsmgr.AddNamespace("tts", "http://www.w3.org/2006/04/ttaf1#styling");

            XmlNode titleNode = xml.DocumentElement.SelectSingleNode("//ttaf1:head", nsmgr).FirstChild.FirstChild;
            titleNode.InnerText = title;

            XmlNode div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).SelectSingleNode("ttaf1:div", nsmgr);
            if (div == null)
            {
                div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).FirstChild;
            }

            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/2006/04/ttaf1");

                if (UseCDataForParagraphText)
                {
                    XmlCDataSection cData = xml.CreateCDataSection(p.Text);
                    paragraph.AppendChild(cData);
                }
                else
                {
                    string text = HtmlUtil.RemoveHtmlTags(p.Text);
                    bool first = true;
                    foreach (string line in text.SplitToLines())
                    {
                        if (!first)
                        {
                            XmlNode br = xml.CreateElement("br", "http://www.w3.org/2006/04/ttaf1");
                            paragraph.AppendChild(br);
                        }
                        XmlNode textNode = xml.CreateTextNode(line);
                        paragraph.AppendChild(textNode);
                        first = false;
                    }
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

                div.AppendChild(paragraph);
                no++;
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(sb.ToString().RemoveControlCharactersButWhiteSpace().Trim().Replace("http://www.w3.org/2006/04/ttaf1#styling\"xml:lang", "http://www.w3.org/2006/04/ttaf1#styling\" xml:lang"));

            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttaf1", xml.DocumentElement.NamespaceURI);

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//ttaf1:p", nsmgr))
            {
                try
                {
                    var pText = new StringBuilder();
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        switch (innerNode.Name)
                        {
                            case "br":
                                pText.AppendLine();
                                break;
                            case "span":
                                bool italic = false;
                                if (innerNode.Attributes != null)
                                {
                                    var fs = innerNode.Attributes.GetNamedItem("tts:fontStyle");
                                    if (fs != null && fs.Value == "italic")
                                    {
                                        italic = true;
                                        pText.Append("<i>");
                                    }
                                }
                                if (innerNode.HasChildNodes)
                                {
                                    foreach (XmlNode innerInnerNode in innerNode.ChildNodes)
                                    {
                                        if (innerInnerNode.Name == "br")
                                        {
                                            pText.AppendLine();
                                        }
                                        else
                                        {
                                            pText.Append(innerInnerNode.InnerText);
                                        }
                                    }
                                }
                                else
                                {
                                    pText.Append(innerNode.InnerText);
                                }
                                if (italic)
                                {
                                    pText.Append("</i>");
                                }

                                break;
                            default:
                                pText.Append(innerNode.InnerText);
                                break;
                        }
                    }
                    string start = node.Attributes["begin"].InnerText;
                    string text = pText.ToString();
                    text = text.Replace(Environment.NewLine + "</i>", "</i>" + Environment.NewLine);
                    text = text.Replace("<i></i>", string.Empty);
                    if (node.Attributes["end"] != null)
                    {
                        string end = node.Attributes["end"].InnerText;
                        subtitle.Paragraphs.Add(new Paragraph(TimedText10.GetTimeCode(start, false), TimedText10.GetTimeCode(end, false), text));
                    }
                    else if (node.Attributes["dur"] != null)
                    {
                        TimeCode duration = TimedText10.GetTimeCode(node.Attributes["dur"].InnerText, false);
                        TimeCode startTime = TimedText10.GetTimeCode(start, false);
                        var endTime = new TimeCode(startTime.TotalMilliseconds + duration.TotalMilliseconds);
                        subtitle.Paragraphs.Add(new Paragraph(startTime, endTime, text));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            bool allBelow100 = true;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                p.Text = Utilities.RemoveUnneededSpaces(p.Text, null).Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                if (p.StartTime.Milliseconds >= 100 || p.EndTime.Milliseconds >= 100)
                {
                    allBelow100 = false;
                }
            }
            if (allBelow100)
            {
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    p.StartTime.Milliseconds *= 10;
                    p.EndTime.Milliseconds *= 10;
                }
            }
            subtitle.Renumber();
        }

    }
}
