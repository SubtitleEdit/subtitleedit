using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle5 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Unknown 5";

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
                start.InnerText = $"{p.StartTime.TotalMilliseconds / 1000}".Replace(',', '.');
                paragraph.Attributes.Append(start);

                XmlAttribute duration = xml.CreateAttribute("dur");
                duration.InnerText = $"{p.Duration.TotalMilliseconds / 1000}".Replace(',', '.');
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
            {
                return;
            }

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
                    {
                        start = start.Replace(',', '.');
                    }

                    string end = node.Attributes["dur"].InnerText;
                    if (!string.IsNullOrEmpty(end))
                    {
                        end = end.Replace(',', '.');
                    }

                    string text = node.InnerText.Replace("&quot;", "\"");

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
