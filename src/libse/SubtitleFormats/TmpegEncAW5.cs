using Nikse.SubtitleEdit.Core.Common;
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
            string xmlStructure = GetLayout().Replace("'", "\"");
            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            XmlNode div = xml.DocumentElement.SelectSingleNode("Subtitle");
            div.InnerXml = string.Empty;
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("SubtitleItem");

                var text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                // Build the CDATA from the RAW text: going through InnerText first escaped it
                // ("Tom &amp; Jerry"), and since CDATA content is not parsed that escaping was
                // read back as literal text and escaped AGAIN on the next save - so an ampersand
                // grew an "amp;" every time the file was saved.
                var cdataText = text.Replace(Environment.NewLine, "\\n") + "\\n";
                if (cdataText.Contains("]]>", StringComparison.Ordinal))
                {
                    // cannot be expressed in one CDATA section - keep the <Text> element the reader requires
                    var textNode = xml.CreateElement("Text");
                    textNode.InnerText = cdataText;
                    paragraph.AppendChild(textNode);
                }
                else
                {
                    paragraph.InnerXml = "<Text><![CDATA[" + cdataText + "]]></Text>";
                }

                XmlAttribute layoutIndex = xml.CreateAttribute("layoutindex");
                var layoutIndexValue = GetLayoutIndexFromAssAlignment(p.Text);
                layoutIndex.InnerText = layoutIndexValue.ToString();

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
            return GetLayout().Replace("@", s);
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            string xmlAsString = JoinLinesTrimmed(lines);
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
