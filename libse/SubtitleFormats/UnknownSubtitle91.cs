using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle91 : SubtitleFormat
    {
        //<captions>
        //<caption time_in = "0.64" time_out="4.74" action=""><![CDATA[Line 1]]></caption>
        //<caption time_in = "4.75" time_out="8.05" action=""><![CDATA[line 2]]></caption>
        //<captions>

        public override string Extension => ".xml";

        public override string Name => "Unknown 91";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
               "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
               "<captions />";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("caption");

                XmlAttribute attr = xml.CreateAttribute("time_in");
                attr.InnerText = EncodeTimeCode(p.StartTime);
                paragraph.Attributes.Append(attr);

                attr = xml.CreateAttribute("time_out");
                attr.InnerText = EncodeTimeCode(p.EndTime);
                paragraph.Attributes.Append(attr);

                var cData = xml.CreateCDataSection(HtmlUtil.RemoveHtmlTags(p.Text));
                paragraph.AppendChild(cData);

                xml.DocumentElement.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return time.TotalSeconds.ToString(CultureInfo.InvariantCulture);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string allText = sb.ToString();
            if (!allText.Contains("</captions>") || !allText.Contains("<caption "))
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

            if (xml.DocumentElement == null)
            {
                _errorCount = 1;
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("caption"))
            {
                try
                {
                    if (node.Attributes != null)
                    {
                        string text = node.InnerText.Trim();
                        string start = node.Attributes.GetNamedItem("time_in").InnerText;
                        string end = node.Attributes.GetNamedItem("time_out").InnerText;
                        subtitle.Paragraphs.Add(new Paragraph(DecodeTimeCode(start), DecodeTimeCode(end), text));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string s)
        {
            var seconds = Convert.ToDouble(s, CultureInfo.InvariantCulture);
            return new TimeCode(seconds * 1000.0);
        }
    }
}
