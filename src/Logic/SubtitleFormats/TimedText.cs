using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class TimedText : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Timed Text draft"; }
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
            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if (xmlAsString.Contains("http://www.w3.org/") &&
                xmlAsString.Contains("/ttaf1"))
            {
                XmlDocument xml = new XmlDocument();
                try
                {
                    xml.LoadXml(xmlAsString);

                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
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
            else
            {
                return false;
            }
        }

        private static string ConvertToTimeString(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<tt xmlns=\"http://www.w3.org/2006/10/ttaf1\" xmlns:ttp=\"http://www.w3.org/2006/10/ttaf1#parameter\" ttp:timeBase=\"media\" xmlns:tts=\"http://www.w3.org/2006/10/ttaf1#style\" xml:lang=\"en\" xmlns:ttm=\"http://www.w3.org/2006/10/ttaf1#metadata\">" + Environment.NewLine +
                "   <head>" + Environment.NewLine +
                "       <metadata>" + Environment.NewLine +
                "           <ttm:title></ttm:title>" + Environment.NewLine +
                "      </metadata>" + Environment.NewLine +
                "       <styling>" + Environment.NewLine +
                "         <style id=\"s0\" tts:backgroundColor=\"black\" tts:fontStyle=\"normal\" tts:fontSize=\"16\" tts:fontFamily=\"sansSerif\" tts:color=\"white\" />" + Environment.NewLine +
                "      </styling>" + Environment.NewLine +
                "   </head>" + Environment.NewLine +
                "   <body tts:textAlign=\"center\" style=\"s0\">" + Environment.NewLine +
                "       <div />" + Environment.NewLine +
                "   </body>" + Environment.NewLine +
                "</tt>";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttaf1", "http://www.w3.org/2006/10/ttaf1");
            nsmgr.AddNamespace("ttp", "http://www.w3.org/2006/10/ttaf1#parameter");
            nsmgr.AddNamespace("tts", "http://www.w3.org/2006/10/ttaf1#style");
            nsmgr.AddNamespace("ttm", "http://www.w3.org/2006/10/ttaf1#metadata");

            XmlNode titleNode = xml.DocumentElement.SelectSingleNode("//ttaf1:head", nsmgr).FirstChild.FirstChild;
            titleNode.InnerText = title;

            XmlNode div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).FirstChild;
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/2006/10/ttaf1");
                string text = Utilities.RemoveHtmlTags(p.Text);

                bool first = true;
                foreach (string line in text.Split(Environment.NewLine.ToCharArray(),  StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!first)
                    {
                        XmlNode br = xml.CreateElement("br", "http://www.w3.org/2006/10/ttaf1");
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

            MemoryStream ms = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(ms, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            xml.Save(writer);
            return Encoding.UTF8.GetString(ms.ToArray()).Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(sb.ToString());

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttaf1", xml.DocumentElement.NamespaceURI);
            XmlNode div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).FirstChild;
            foreach (XmlNode node in div.ChildNodes)
            {
                try
                {
                    StringBuilder pText = new StringBuilder();
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        switch (innerNode.Name.ToString())
                        {
                            case "br":
                                pText.AppendLine();
                                break;
                            default:
                                pText.Append(innerNode.InnerText);
                                break;
                        }
                    }
                    string start = node.Attributes["begin"].InnerText;
                    string end = node.Attributes["end"].InnerText;

                    subtitle.Paragraphs.Add(new Paragraph(GetTimeCode(start),GetTimeCode(end), pText.ToString()));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
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
                string[] parts = s.Split(new char[] { ':', '.', ',' });
                ts = new TimeSpan(0, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
            }
            return new TimeCode(ts);
        }
    }
}


