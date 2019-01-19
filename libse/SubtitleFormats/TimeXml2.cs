using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TimeXml2 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Xml 2";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<Subtitles/>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("Subtitle");

                XmlNode number = xml.CreateElement("Number");
                number.InnerText = p.Number.ToString(CultureInfo.InvariantCulture);
                paragraph.AppendChild(number);

                XmlNode start = xml.CreateElement("Start");
                start.InnerText = p.StartTime.ToString();
                paragraph.AppendChild(start);

                XmlNode end = xml.CreateElement("End");
                end.InnerText = p.EndTime.ToString();
                paragraph.AppendChild(end);

                XmlNode duration = xml.CreateElement("Duration");
                duration.InnerText = p.Duration.ToShortString();
                paragraph.AppendChild(duration);

                XmlNode text = xml.CreateElement("Text");
                text.InnerText = HtmlUtil.RemoveHtmlTags(p.Text);
                paragraph.AppendChild(text);

                xml.DocumentElement.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            var xmlString = sb.ToString();
            if (!xmlString.Contains("<Subtitles>") || !xmlString.Contains("<Text>") || !xmlString.Contains("<Duration>"))
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

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Subtitle"))
            {
                try
                {
                    TimeCode startTimeCode = DecodeTimeCode(node.SelectSingleNode("Start").InnerText);
                    TimeCode endTimeCode = DecodeTimeCode(node.SelectSingleNode("End").InnerText);
                    string text = node.SelectSingleNode("Text").InnerText;
                    subtitle.Paragraphs.Add(new Paragraph(startTimeCode, endTimeCode, text));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string p)
        {
            var parts = p.Split(new[] { ';', '.', ':', ',' });

            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string ms = parts[3];

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), (int.Parse(ms)));
        }

    }
}
