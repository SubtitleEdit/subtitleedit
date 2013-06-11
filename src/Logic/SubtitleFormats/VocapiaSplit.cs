using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class VocapiaSplit : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Vocapia Split"; }
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
                "<AudioDoc name=\"title\">" + Environment.NewLine +
                "<SegmentList/>" + Environment.NewLine +
                "</AudioDoc>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            xml.DocumentElement.Attributes["name"].InnerText = title;
            XmlNode reel = xml.DocumentElement.SelectSingleNode("SegmentList");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("SpeechSegment");

                XmlAttribute start = xml.CreateAttribute("stime");
                start.InnerText = ToTimeCode(p.StartTime.TotalMilliseconds);
                paragraph.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("etime");
                end.InnerText = ToTimeCode(p.EndTime.TotalMilliseconds);
                paragraph.Attributes.Append(end);

                paragraph.InnerText = Utilities.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, "<s/>"));

                reel.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        private string ToTimeCode(double totalMilliseconds)
        {
            return string.Format("{0:0##}", totalMilliseconds / 1000.0);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string xmlString = sb.ToString();
            if (!xmlString.Contains("<SpeechSegment"))
                return;

            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(xmlString);
            }
            catch
            {
                _errorCount = 1;
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("SegmentList/SpeechSegment"))
            {
                try
                {
                    string start = node.Attributes["stime"].InnerText;
                    string end = node.Attributes["etime"] .InnerText;
                    string text = node.InnerText;
                    text = text.Replace("<s/>", Environment.NewLine);
                    text = text.Replace("  ", " ");
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

        private double ParseTimeCode(string s)
        {
            return Convert.ToDouble(s) * 1000.0;
        }

    }
}


