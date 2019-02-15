using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class VocapiaSplit : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Vocapia Split";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<AudioDoc name=\"title\">" + Environment.NewLine +
                "<SpeakerList/>" + Environment.NewLine +
                "<SegmentList/>" + Environment.NewLine +
                "</AudioDoc>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            xml.DocumentElement.Attributes["name"].InnerText = title;

            if (subtitle.Header != null && subtitle.Header.Contains("<SpeakerList"))
            {
                var header = new XmlDocument();
                try
                {
                    header.LoadXml(subtitle.Header);
                    var speakerListNode = header.DocumentElement.SelectSingleNode("SpeakerList");
                    if (speakerListNode != null)
                    {
                        xml.DocumentElement.SelectSingleNode("SpeakerList").InnerXml = speakerListNode.InnerXml;
                    }
                }
                catch
                {
                    // ignored
                }
            }

            XmlNode reel = xml.DocumentElement.SelectSingleNode("SegmentList");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("SpeechSegment");

                XmlAttribute start = xml.CreateAttribute("stime");
                start.InnerText = ToTimeCode(p.StartTime.TotalMilliseconds);
                paragraph.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("etime");
                end.InnerText = ToTimeCode(p.EndTime.TotalMilliseconds);
                paragraph.Attributes.Append(end);

                if (p.Actor != null)
                {
                    XmlAttribute spkid = xml.CreateAttribute("spkid");
                    spkid.InnerText = p.Actor;
                    paragraph.Attributes.Append(spkid);
                }

                paragraph.InnerText = HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, "<s/>"));

                reel.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        private static string ToTimeCode(double totalMilliseconds)
        {
            return $"{totalMilliseconds / TimeCode.BaseUnit:0##}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string xmlString = sb.ToString();
            if (!xmlString.Contains("<SpeechSegment"))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(xmlString);
            }
            catch
            {
                _errorCount = 1;
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("SegmentList/SpeechSegment"))
            {
                try
                {
                    string start = node.Attributes["stime"].InnerText;
                    string end = node.Attributes["etime"].InnerText;
                    string text = node.InnerText;
                    text = text.Replace("<s/>", Environment.NewLine);
                    text = text.Replace("  ", " ");
                    var p = new Paragraph(text, ParseTimeCode(start), ParseTimeCode(end));
                    var spkIdAttr = node.Attributes["spkid"];
                    if (spkIdAttr != null)
                    {
                        p.Extra = spkIdAttr.InnerText;
                        p.Actor = p.Extra;
                    }
                    subtitle.Paragraphs.Add(p);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber();
            if (subtitle.Paragraphs.Count > 0)
            {
                subtitle.Header = xmlString;
            }
        }

        private static double ParseTimeCode(string s)
        {
            return Convert.ToDouble(s) * TimeCode.BaseUnit;
        }

        public override bool HasStyleSupport => true;
    }
}
