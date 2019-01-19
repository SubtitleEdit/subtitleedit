using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TmpegEncAW5 : TmpegEncXml
    {
        public override string Name => "TMPGEnc AW5";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure = Layout.Replace("'", "\"");
            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            XmlNode div = xml.DocumentElement.SelectSingleNode("Subtitle");
            div.InnerXml = string.Empty;
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("SubtitleItem");

                var text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                paragraph.InnerText = text;
                paragraph.InnerXml = "<Text><![CDATA[" + paragraph.InnerXml.Replace(Environment.NewLine, "\\n") + "\\n]]></Text>";

                XmlAttribute layoutIndex = xml.CreateAttribute("layoutindex");
                if (p.Text.TrimStart().StartsWith("<i>", StringComparison.Ordinal) && p.Text.TrimEnd().EndsWith("</i>", StringComparison.Ordinal))
                {
                    layoutIndex.InnerText = "4";
                }
                else
                {
                    layoutIndex.InnerText = "0";
                }

                paragraph.Attributes.Append(layoutIndex);

                XmlAttribute enable = xml.CreateAttribute("enable");
                enable.InnerText = "1";
                paragraph.Attributes.Append(enable);

                XmlAttribute start = xml.CreateAttribute("starttime");
                start.InnerText = p.StartTime.ToString();
                paragraph.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("endtime");
                end.InnerText = p.EndTime.ToString();
                paragraph.Attributes.Append(end);

                div.AppendChild(paragraph);
                no++;
            }

            string s = ToUtf8XmlString(xml);
            int startPos = s.IndexOf("<Subtitle>", StringComparison.Ordinal) + 10;
            s = s.Substring(startPos, s.IndexOf("</Subtitle>", StringComparison.Ordinal) - startPos).Trim();
            return Layout.Replace("@", s);
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if ((xmlAsString.Contains("<TMPGEncVMESubtitleTextFormat>") || xmlAsString.Contains("<SubtitleItem ")) && (xmlAsString.Contains("<Subtitle")))
            {
                return base.IsMine(lines, fileName);
            }
            return false;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            LoadTMpeg(subtitle, lines, true);
        }

    }
}
