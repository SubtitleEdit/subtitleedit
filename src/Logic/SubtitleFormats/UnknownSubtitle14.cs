using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class UnknownSubtitle14 : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Unknown 14"; }
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
            //<Phrase TimeStart="4020" TimeEnd="6020">
            //  <Text>XYZ PRESENTS</Text>
            //</Phrase>

            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<Subtitle xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"></Subtitle>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            int id = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("Phrase");

                XmlAttribute start = xml.CreateAttribute("TimeStart");
                start.InnerText = p.StartTime.TotalMilliseconds.ToString();
                paragraph.Attributes.Append(start);

                XmlAttribute duration = xml.CreateAttribute("TimeEnd");
                duration.InnerText = p.EndTime.TotalMilliseconds.ToString();
                paragraph.Attributes.Append(duration);

                XmlNode text = xml.CreateElement("Text");
                text.InnerText = HtmlUtil.RemoveHtmlTags(p.Text).Replace(Environment.NewLine, "\\n");
                paragraph.AppendChild(text);

                xml.DocumentElement.AppendChild(paragraph);
                id++;
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string allText = sb.ToString();
            if (!allText.Contains("<Subtitle") || !allText.Contains("TimeStart="))
                return;

            XmlDocument xml = new XmlDocument();
            xml.XmlResolver = null;
            try
            {
                xml.LoadXml(allText);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                _errorCount = 1;
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Phrase"))
            {
                try
                {
                    string start = node.Attributes["TimeStart"].InnerText;
                    string end = node.Attributes["TimeEnd"].InnerText;
                    string text = node.SelectSingleNode("Text").InnerText;
                    text = text.Replace("\\n", Environment.NewLine);
                    subtitle.Paragraphs.Add(new Paragraph(DecodeTimeCode(start), DecodeTimeCode(end), text));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string timeCode)
        {
            return new TimeCode(long.Parse(timeCode));
        }

    }
}
