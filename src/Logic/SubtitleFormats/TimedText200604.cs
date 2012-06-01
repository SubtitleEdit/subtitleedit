using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class TimedText200604 : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Timed Text draft 2006-04"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
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
            if (xmlAsString.Contains("http://www.w3.org/") &&
                xmlAsString.Contains("/ttaf1"))
            {
                var xml = new XmlDocument();
                try
                {
                    xml.LoadXml(xmlAsString);

                    var nsmgr = new XmlNamespaceManager(xml.NameTable);
                    nsmgr.AddNamespace("ttaf1", xml.DocumentElement.NamespaceURI);
                    XmlNode div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).FirstChild;
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
            return string.Format("{0:00}:{1:00}:{2:00}.{3:000}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
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

            XmlNode div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).FirstChild;
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/2006/04/ttaf1");
                string text = Utilities.RemoveHtmlTags(p.Text);

                bool first = true;
                foreach (string line in text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
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

                XmlAttribute start = xml.CreateAttribute("begin");
                start.InnerText = ConvertToTimeString(p.StartTime);
                paragraph.Attributes.Append(start);

                XmlAttribute id = xml.CreateAttribute("id");
                id.InnerText = "p" + no.ToString();
                paragraph.Attributes.Append(id);

                XmlAttribute end = xml.CreateAttribute("end");
                end.InnerText = ConvertToTimeString(p.EndTime);
                paragraph.Attributes.Append(end);

                div.AppendChild(paragraph);
                no++;
            }

            var ms = new MemoryStream();
            var writer = new XmlTextWriter(ms, Encoding.UTF8) { Formatting = Formatting.Indented };
            xml.Save(writer);
            return Encoding.UTF8.GetString(ms.ToArray()).Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument();
            xml.LoadXml(sb.ToString());

            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttaf1", xml.DocumentElement.NamespaceURI);
            XmlNode div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).FirstChild;
            foreach (XmlNode node in div.ChildNodes)
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
                                    pText.Append("</i>");
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
                        subtitle.Paragraphs.Add(new Paragraph(GetTimeCode(start), GetTimeCode(end), text));
                    }
                    else if (node.Attributes["dur"] != null)
                    {
                        TimeCode duration = GetTimeCode(node.Attributes["dur"].InnerText);
                        TimeCode startTime = GetTimeCode(start);
                        TimeCode endTime = new TimeCode(TimeSpan.FromMilliseconds(startTime.TotalMilliseconds + duration.TotalMilliseconds));
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
                if (p.StartTime.Milliseconds >= 100 || p.EndTime.Milliseconds >= 100)
                    allBelow100 = false;
            }
            if (allBelow100)
            {
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    p.StartTime.Milliseconds *= 10;
                    p.EndTime.Milliseconds *= 10;
                }
            }
            subtitle.Renumber(1);
        }

        private static TimeCode GetTimeCode(string s)
        {
            TimeSpan ts;
            s = s.ToLower().Trim();
            if (s.EndsWith("s"))
            {
                s = s.TrimEnd('s');
                ts = TimeSpan.FromSeconds(double.Parse(s));
            }
            else
            {
                string[] parts = s.Split(new[] { ':', '.', ',' });
                ts = new TimeSpan(0, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
            }
            return new TimeCode(ts);
        }
    }
}


