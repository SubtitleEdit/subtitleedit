using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Eeg708 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "EEG 708";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<EEG708Captions/>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("Caption");
                XmlAttribute start = xml.CreateAttribute("timecode");
                start.InnerText = EncodeTimeCode(p.StartTime);
                paragraph.Attributes.Append(start);
                XmlNode text = xml.CreateElement("Text");
                text.InnerText = p.Text;
                paragraph.AppendChild(text);
                xml.DocumentElement.AppendChild(paragraph);

                paragraph = xml.CreateElement("Caption");
                start = xml.CreateAttribute("timecode");
                start.InnerText = EncodeTimeCode(p.EndTime);
                paragraph.Attributes.Append(start);
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
            if (!allText.Contains("<EEG708Captions") || !allText.Contains("<Caption"))
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

            Paragraph lastParagraph = null;
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Caption"))
            {
                try
                {
                    string start = node.Attributes["timecode"].InnerText;
                    if (lastParagraph != null)
                    {
                        lastParagraph.EndTime = DecodeTimeCodeFramesFourParts(start.Split(':'));
                    }

                    XmlNode text = node.SelectSingleNode("Text");
                    if (text != null)
                    {
                        string s = text.InnerText;
                        s = s.Replace("<br />", Environment.NewLine).Replace("<br/>", Environment.NewLine);
                        TimeCode startTime = DecodeTimeCodeFramesFourParts(start.Split(':'));
                        lastParagraph = new Paragraph(s, startTime.TotalMilliseconds, startTime.TotalMilliseconds + 3000);
                        subtitle.Paragraphs.Add(lastParagraph);
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

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

    }
}
