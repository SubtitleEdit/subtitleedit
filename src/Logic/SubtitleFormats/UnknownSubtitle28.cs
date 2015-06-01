using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class UnknownSubtitle28 : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Unknown 28"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            this.LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > 0;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine + "<titles/>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            int id = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                id++;
                XmlNode paragraph = xml.CreateElement("title");
                XmlAttribute idAttr = xml.CreateAttribute("id");
                idAttr.InnerText = id.ToString();
                paragraph.Attributes.Append(idAttr);

                XmlNode time = xml.CreateElement("time");
                XmlAttribute start = xml.CreateAttribute("start");
                start.InnerText = ToTimeCode(p.StartTime.TotalMilliseconds);
                time.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("end");
                end.InnerText = ToTimeCode(p.EndTime.TotalMilliseconds);
                time.Attributes.Append(end);

                paragraph.AppendChild(time);

                var arr = p.Text.SplitToLines();
                for (int i = 0; i < arr.Length; i++)
                {
                    XmlNode text = xml.CreateElement("text" + (i + 1));
                    text.InnerText = arr[i];
                    paragraph.AppendChild(text);
                }
                xml.DocumentElement.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        private static string ToTimeCode(double totalMilliseconds)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(totalMilliseconds);
            return string.Format("{0:00}:{1:00}:{2:00},{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlString = sb.ToString();
            if (!xmlString.Contains("<titles") || !xmlString.Contains("<text1>"))
                return;

            var xml = new XmlDocument();
            xml.XmlResolver = null;
            try
            {
                xml.LoadXml(xmlString);
            }
            catch
            {
                _errorCount = 1;
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("title"))
            {
                try
                {
                    XmlNode timeNode = node.SelectSingleNode("time");
                    string start = timeNode.Attributes["start"].InnerText;
                    string end = timeNode.Attributes["end"].InnerText;
                    string text = string.Empty;
                    for (int i = 1; i < 10; i++)
                    {
                        XmlNode textNode = node.SelectSingleNode("text" + i);
                        if (textNode != null)
                            text = (text + Environment.NewLine + textNode.InnerText).Trim();
                    }
                    subtitle.Paragraphs.Add(new Paragraph(text, ParseTimeCode(start), ParseTimeCode(end)));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        private static double ParseTimeCode(string start)
        {
            string[] arr = start.Split(new[] { ':', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
            TimeSpan ts = new TimeSpan(0, int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), int.Parse(arr[3]));
            return ts.TotalMilliseconds;
        }

    }
}
