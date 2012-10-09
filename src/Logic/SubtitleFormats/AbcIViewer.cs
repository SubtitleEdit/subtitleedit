using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class AbcIViewer : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "ABC iView"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            Subtitle subtitle = new Subtitle();
            this.LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > 0;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<root fps=\"25\" movie=\"program title\" language=\"GBR:English (UK)\" font=\"Arial\" style=\"normal\" size=\"48\">" + Environment.NewLine +
                "<reel start=\"\" first=\"\" last=\"\">" + Environment.NewLine +
                "</reel>" + Environment.NewLine +
                "</root>";

            XmlDocument xml = new XmlDocument();
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

                paragraph.InnerText = Utilities.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, "|"));

                reel.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        private string ToTimeCode(double totalMilliseconds)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(totalMilliseconds);
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string xmlString = sb.ToString();
            if (!xmlString.Contains("<reel"))
                return;

            XmlDocument xml = new XmlDocument();
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
                    string end = node.Attributes["end"] .InnerText;
                    string text = node.InnerText;
                    text = text.Replace("|", Environment.NewLine);
                    subtitle.Paragraphs.Add(new Paragraph(text, ParseTimeCode(start), ParseTimeCode(end)));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }

        private double ParseTimeCode(string start)
        {
            string[] arr = start.Split(':');
            TimeSpan ts = new TimeSpan(0, int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), int.Parse(arr[3]));
            return ts.TotalMilliseconds;
        }

    }
}


