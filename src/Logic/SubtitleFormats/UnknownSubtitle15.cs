using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class UnknownSubtitle15 : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Unknown 15"; }
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

        private static string ToTimeCode(TimeCode tc)
        {
            int last = (int)(tc.Milliseconds / 10.0D + 0.5D);
            return tc.ToString().Substring(0, 8) + ":" + string.Format("{0:0#}", last);
        }

        private static TimeCode DecodeTimeCode(string s)
        {
            var parts = s.Split(new[] { ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
            TimeCode tc = new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]) * 100);
            return tc;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //<page>
            // <video>
            //   <cuepoint>
            //     <name>That's 123. That's the number one hundred twenty three.</name>
            //     <startTime>00:00:04:67</startTime>
            //     <endTime>00:00:07:50</endTime>
            //   </cuepoint>

            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<page><video/></page>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            int id = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("cuepoint");

                XmlNode text = xml.CreateElement("name");
                text.InnerText = HtmlUtil.RemoveHtmlTags(p.Text).Replace(Environment.NewLine, " ").Replace("  ", " ");
                paragraph.AppendChild(text);

                XmlNode start = xml.CreateElement("startTime");
                start.InnerText = ToTimeCode(p.StartTime);
                paragraph.AppendChild(start);

                XmlNode duration = xml.CreateElement("endTime");
                duration.InnerText = ToTimeCode(p.EndTime);
                paragraph.AppendChild(duration);

                xml.DocumentElement.SelectSingleNode("video").AppendChild(paragraph);
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
            if (!allText.Contains("<page") || !allText.Contains("<cuepoint"))
                return;

            var xml = new XmlDocument();
            xml.XmlResolver = null;
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

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("video/cuepoint"))
            {
                try
                {
                    string text = node.SelectSingleNode("name").InnerText;
                    string start = node.SelectSingleNode("startTime").InnerText;
                    string end = node.SelectSingleNode("endTime").InnerText;
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
