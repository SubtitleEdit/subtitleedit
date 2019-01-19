using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AbcIViewer : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "ABC iView";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<root fps=\"25\" movie=\"program title\" language=\"GBR:English (UK)\" font=\"Arial\" style=\"normal\" size=\"48\">" + Environment.NewLine +
                "<reel start=\"\" first=\"\" last=\"\">" + Environment.NewLine +
                "</reel>" + Environment.NewLine +
                "</root>";

            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(xmlStructure);
            XmlNode reel = xml.DocumentElement.SelectSingleNode("reel");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("title");

                XmlAttribute start = xml.CreateAttribute("start");
                start.InnerText = ToTimeCode(p.StartTime.TotalMilliseconds);
                paragraph.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("end");
                end.InnerText = ToTimeCode(p.EndTime.TotalMilliseconds);
                paragraph.Attributes.Append(end);

                paragraph.InnerText = HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, "|"), true);

                reel.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        private static string ToTimeCode(double totalMilliseconds)
        {
            var ts = TimeSpan.FromMilliseconds(totalMilliseconds);
            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}:{ts.Milliseconds:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            bool allTwoCifferMs = true;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string xmlString = sb.ToString();
            if (!xmlString.Contains("<reel"))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(xmlString);
            }
            catch
            {
                _errorCount = 1;
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("reel/title"))
            {
                try
                {
                    string start = node.Attributes["start"].InnerText;
                    string end = node.Attributes["end"].InnerText;
                    string text = node.InnerText;

                    if (allTwoCifferMs && (start.Length != 11 || end.Length != 11 || start[8] != ':' || end[8] != ':'))
                    {
                        allTwoCifferMs = false;
                    }

                    text = text.Replace("|", Environment.NewLine);
                    subtitle.Paragraphs.Add(new Paragraph(text, ParseTimeCode(start), ParseTimeCode(end)));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }

            if (allTwoCifferMs)
            {
                foreach (var p in subtitle.Paragraphs)
                {
                    p.StartTime.Milliseconds = p.StartTime.Milliseconds * 10;
                    p.EndTime.Milliseconds = p.EndTime.Milliseconds * 10;
                }
            }

            subtitle.Renumber();
        }

        private static double ParseTimeCode(string start)
        {
            var arr = start.Split(':');
            return new TimeSpan(0, int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), int.Parse(arr[3])).TotalMilliseconds;
        }

    }
}
