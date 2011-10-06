using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class TimeXml : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Xml"; }
        }

        public override bool HasLineNumber
        {
            get { return true; }
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
                "<Subtitle/>";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("Paragraph");

                XmlNode number = xml.CreateElement("Number");
                number.InnerText = p.Number.ToString();
                paragraph.AppendChild(number);

                XmlNode start = xml.CreateElement("StartMilliseconds");
                start.InnerText = p.StartTime.TotalMilliseconds.ToString();
                paragraph.AppendChild(start);

                XmlNode end = xml.CreateElement("EndMilliseconds");
                end.InnerText = p.EndTime.TotalMilliseconds.ToString();
                paragraph.AppendChild(end);

                XmlNode text = xml.CreateElement("Text");
                text.InnerText = Utilities.RemoveHtmlTags(p.Text);
                paragraph.AppendChild(text);

                xml.DocumentElement.AppendChild(paragraph);
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

            string xmlString = sb.ToString();
            if (!xmlString.Contains("<Paragraph>") || !xmlString.Contains("<Text>"))
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

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Paragraph"))
            {
                try
                {
                    string start = node.SelectSingleNode("StartMilliseconds").InnerText;
                    string end = node.SelectSingleNode("EndMilliseconds").InnerText;
                    string text = node.SelectSingleNode("Text").InnerText;

                    subtitle.Paragraphs.Add(new Paragraph(text, Convert.ToDouble(start), Convert.ToDouble(end)));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }

    }
}


