using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class CaraokeXml : SubtitleFormat
    {
        public override string Extension => ".crk";

        public override string Name => "Caraoke Xml";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<caraoke name=\"\" filename=\"\"><paragraph attr=\"\" /></caraoke>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            var paragraph = xml.DocumentElement.SelectSingleNode("paragraph");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode item = xml.CreateElement("item");

                var start = xml.CreateAttribute("tc1");
                start.InnerText = p.StartTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
                item.Attributes.Append(start);

                var end = xml.CreateAttribute("tc2");
                end.InnerText = p.EndTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
                item.Attributes.Append(end);

                var attr = xml.CreateAttribute("attr");
                attr.InnerText = string.Empty;
                item.Attributes.Append(attr);

                item.InnerText = p.Text;

                paragraph.AppendChild(item);
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string xmlAsText = sb.ToString();

            if (!xmlAsText.Contains("<caraoke"))
            {
                return;
            }

            xmlAsText = xmlAsText.Replace("< /", "</");

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(xmlAsText);
            }
            catch (Exception ex)
            {
                _errorCount = 1;
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//item"))
            {
                try
                {
                    string start = node.Attributes["tc1"].InnerText;
                    string end = node.Attributes["tc2"].InnerText;
                    string text = node.InnerText;

                    subtitle.Paragraphs.Add(new Paragraph(text, Convert.ToDouble(start, System.Globalization.CultureInfo.InvariantCulture), Convert.ToDouble(end, System.Globalization.CultureInfo.InvariantCulture)));
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
