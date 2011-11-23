using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

//http://www.w3.org/TR/ttaf1-dfxp/
//Timed Text Markup Language (TTML) 1.0
//W3C Recommendation 18 November 2010

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class TimedText10 : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Timed Text 1.0"; }
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
            if (xmlAsString.Contains("http://www.w3.org/ns/ttml"))
            {
                XmlDocument xml = new XmlDocument();
                try
                {
                    xml.LoadXml(xmlAsString);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
                    nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                    var nds = xml.DocumentElement.SelectSingleNode("ttml:body", nsmgr);
                    var paragraphs = nds.SelectNodes("//ttml:p", nsmgr);
                    return paragraphs.Count > 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
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

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            nsmgr.AddNamespace("ttp", "http://www.w3.org/ns/10/ttml#parameter");
            nsmgr.AddNamespace("tts", "http://www.w3.org/ns/10/ttml#style");
            nsmgr.AddNamespace("ttm", "http://www.w3.org/ns/10/ttml#metadata");

            XmlNode titleNode = xml.DocumentElement.SelectSingleNode("//ttml:head", nsmgr).FirstChild.FirstChild;
            titleNode.InnerText = title;

            XmlNode div = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).FirstChild;
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/ns/ttml");
                string text = Utilities.RemoveHtmlTags(p.Text);

                bool first = true;
                foreach (string line in text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!first)
                    {
                        XmlNode br = xml.CreateElement("br", "http://www.w3.org/ns/ttml");
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
            double startSeconds = 0;

            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(sb.ToString());

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            XmlNode body = xml.DocumentElement.SelectSingleNode("ttml:body", nsmgr);
            foreach (XmlNode node in body.SelectNodes("//ttml:p", nsmgr))
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
                                pText.Append(innerNode.InnerText.Trim());
                                break;
                        }
                    }

                    string start = string.Empty;
                    if (node.Attributes["begin"] != null)
                    {
                        start = node.Attributes["begin"].InnerText;
                    }

                    string end = string.Empty;
                    if (node.Attributes["end"] != null)
                    {
                        end = node.Attributes["end"].InnerText;
                    }

                    string dur = string.Empty;
                    if (node.Attributes["dur"] != null)
                    {
                        dur = node.Attributes["dur"].InnerText;
                    }

                    TimeCode startCode = new TimeCode(TimeSpan.FromSeconds(startSeconds));
                    if (start != string.Empty)
                    {
                        startCode = GetTimeCode(start);
                    }

                    TimeCode endCode;
                    if (end != string.Empty)
                    {
                        endCode = GetTimeCode(end);
                    }
                    else if (dur != string.Empty)
                    {
                        endCode = new TimeCode(TimeSpan.FromMilliseconds(GetTimeCode(dur).TotalMilliseconds + startCode.TotalMilliseconds));
                    }
                    else
                    {
                        endCode = new TimeCode(TimeSpan.FromMilliseconds(startCode.TotalMilliseconds + 3000));
                    }
                    startSeconds = endCode.TotalSeconds;

                    subtitle.Paragraphs.Add(new Paragraph(startCode, endCode, pText.ToString()));
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
            if (s.EndsWith("s"))
            {
                s = s.TrimEnd('s');
                TimeSpan ts = TimeSpan.FromSeconds(double.Parse(s));
                return new TimeCode(ts);
            }
            else
            {
                string[] parts = s.Split(new char[] { ':', '.', ',' });
                TimeSpan ts = new TimeSpan(0, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                return new TimeCode(ts);
            }
        }
    }
}


