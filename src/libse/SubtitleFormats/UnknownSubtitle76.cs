using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle76 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Unknown 76";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if (xmlAsString.Contains("<timedtext"))
            {
                var xml = new XmlDocument { XmlResolver = null };
                try
                {
                    xml.LoadXml(xmlAsString);
                    if (xml.DocumentElement != null && xml.DocumentElement.Name == "timedtext")
                    {
                        return xml.DocumentElement.SelectNodes("text").Count > 0;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            return false;
        }

        internal static string ConvertToTimeString(TimeCode time)
        {
            return Convert.ToInt64(Math.Round(time.TotalMilliseconds)).ToString(CultureInfo.InvariantCulture);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var xml = new XmlDocument();
            xml.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\" ?><timedtext></timedtext>");

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("text");
                paragraph.InnerText = p.Text;

                XmlAttribute dur = xml.CreateAttribute("d");
                dur.InnerText = ConvertToTimeString(p.Duration);
                paragraph.Attributes.Append(dur);

                XmlAttribute start = xml.CreateAttribute("t");
                start.InnerText = ConvertToTimeString(p.StartTime);
                paragraph.Attributes.Append(start);

                xml.DocumentElement.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(sb.ToString().Trim());

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("text"))
            {
                try
                {
                    string start = node.Attributes["t"].InnerText;
                    string dur = node.Attributes["d"].InnerText;
                    TimeCode startTimeCode = GetTimeCode(start);
                    var endTimeCode = new TimeCode(startTimeCode.TotalMilliseconds + GetTimeCode(dur).TotalMilliseconds);
                    var p = new Paragraph(startTimeCode, endTimeCode, node.InnerText.Replace("   ", " ").Replace("  ", " "));
                    subtitle.Paragraphs.Add(p);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        public static TimeCode GetTimeCode(string s)
        {
            return new TimeCode(Convert.ToDouble(s)); // ms
        }

    }
}
