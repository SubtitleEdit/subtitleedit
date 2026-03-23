using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle14 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Unknown 14";

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

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("Phrase");

                XmlAttribute start = xml.CreateAttribute("TimeStart");
                start.InnerText = ((long)(Math.Round(p.StartTime.TotalMilliseconds))).ToString(CultureInfo.InvariantCulture);
                paragraph.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("TimeEnd");
                end.InnerText = ((long)(Math.Round(p.EndTime.TotalMilliseconds))).ToString(CultureInfo.InvariantCulture);
                paragraph.Attributes.Append(end);

                XmlNode text = xml.CreateElement("Text");
                text.InnerText = HtmlUtil.RemoveHtmlTags(p.Text).Replace(Environment.NewLine, "\\n");
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

            string allText = sb.ToString();
            if (!allText.Contains("<Subtitle") || !allText.Contains("TimeStart="))
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
