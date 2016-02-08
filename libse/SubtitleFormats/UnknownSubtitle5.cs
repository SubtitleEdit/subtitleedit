using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle5 : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Unknown 5"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > 0;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<transcript/>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("text");

                XmlAttribute start = xml.CreateAttribute("start");
                start.InnerText = string.Format("{0}", p.StartTime.TotalMilliseconds / 1000).Replace(',', '.');
                paragraph.Attributes.Append(start);

                XmlAttribute duration = xml.CreateAttribute("dur");
                duration.InnerText = string.Format("{0}", p.Duration.TotalMilliseconds / 1000).Replace(',', '.');
                paragraph.Attributes.Append(duration);

                paragraph.InnerText = p.Text;

                xml.DocumentElement.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string allText = sb.ToString();
            if (!allText.Contains("<text") || !allText.Contains("start="))
                return;

            var xml = new XmlDocument { XmlResolver = null };
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

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("text"))
            {
                try
                {
                    string start = node.Attributes["start"].InnerText;
                    if (!string.IsNullOrEmpty(start))
                        start = start.Replace(',', '.');
                    string end = node.Attributes["dur"].InnerText;
                    if (!string.IsNullOrEmpty(end))
                        end = end.Replace(',', '.');
                    string text = node.InnerText;

                    subtitle.Paragraphs.Add(new Paragraph(text, Convert.ToDouble(start, System.Globalization.CultureInfo.InvariantCulture) * TimeCode.BaseUnit, TimeCode.BaseUnit * (Convert.ToDouble(start, System.Globalization.CultureInfo.InvariantCulture) + Convert.ToDouble(end, System.Globalization.CultureInfo.InvariantCulture))));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

    }
}
