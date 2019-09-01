using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle92 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Unknown 92";

        private static string ToTimeCode(TimeCode tc)
        {
            return tc.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        }

        private static TimeCode DecodeTimeCode(string s)
        {
            return new TimeCode(long.Parse(s));
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var xml = new XmlDocument();
            xml.LoadXml("<xml></xml>");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("dia");

                var cData = xml.CreateCDataSection(HtmlUtil.RemoveHtmlTags(p.Text));
                XmlNode text = xml.CreateElement("sub");
                text.AppendChild(cData);
                paragraph.AppendChild(text);

                XmlNode start = xml.CreateElement("st");
                start.InnerText = ToTimeCode(p.StartTime);
                paragraph.AppendChild(start);

                XmlNode duration = xml.CreateElement("et");
                duration.InnerText = ToTimeCode(p.EndTime);
                paragraph.AppendChild(duration);

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
            if (!allText.Contains("</dia>") || !allText.Contains("</sub>"))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(allText);
            }
            catch
            {
                _errorCount = 1;
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("dia"))
            {
                try
                {
                    string text = node.SelectSingleNode("sub").InnerText;
                    string start = node.SelectSingleNode("st").InnerText;
                    string end = node.SelectSingleNode("et").InnerText;
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
    }
}
