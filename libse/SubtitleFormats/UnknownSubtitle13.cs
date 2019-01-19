using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle13 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Unknown 13";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<subtitle><config bgAlpha=\"0.5\" bgColor=\"0x000000\" defaultColor=\"0xCCffff\" fontSize=\"16\"/></subtitle>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            int id = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("entry");

                XmlAttribute duration = xml.CreateAttribute("timeOut");
                duration.InnerText = p.EndTime.ToString();
                paragraph.Attributes.Append(duration);

                XmlAttribute start = xml.CreateAttribute("timeIn");
                start.InnerText = p.StartTime.ToString();
                paragraph.Attributes.Append(start);

                XmlAttribute idAttr = xml.CreateAttribute("id");
                idAttr.InnerText = id.ToString(CultureInfo.InvariantCulture);
                paragraph.Attributes.Append(idAttr);

                paragraph.InnerText = "<![CDATA[" + p.Text + "]]";

                xml.DocumentElement.AppendChild(paragraph);
                id++;
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string allText = sb.ToString();
            if (!allText.Contains("<subtitle>") || !allText.Contains("timeIn="))
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

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("entry"))
            {
                try
                {
                    string start = node.Attributes["timeIn"].InnerText;
                    string end = node.Attributes["timeOut"].InnerText;
                    string text = node.InnerText;
                    if (text.StartsWith("![CDATA[", StringComparison.Ordinal))
                    {
                        text = text.Remove(0, 8);
                    }

                    if (text.StartsWith("<![CDATA[", StringComparison.Ordinal))
                    {
                        text = text.Remove(0, 9);
                    }

                    if (text.EndsWith("]]", StringComparison.Ordinal))
                    {
                        text = text.Remove(text.Length - 2, 2);
                    }

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
            string[] arr = timeCode.Split(new[] { ':', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
            int hours = int.Parse(arr[0]);
            int minutes = int.Parse(arr[1]);
            int seconds = int.Parse(arr[2]);
            int ms = int.Parse(arr[3]);
            return new TimeCode(hours, minutes, seconds, ms);
        }

    }
}
