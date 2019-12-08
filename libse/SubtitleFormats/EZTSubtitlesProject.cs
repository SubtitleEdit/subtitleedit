using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class EZTSubtitlesProject : SubtitleFormat
    {
        public override string Extension => ".eztxml";

        public override string Name => "EZT XML";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var xml = new XmlDocument();
            var template = @"<?xml version='1.0' encoding='utf-8'?>
<EZTSubtitlesProject version='1.3'>
  <Subtitles count='[SUBTITLE_COUNT]'>
  </Subtitles>
</EZTSubtitlesProject>".Replace("'", "\"").Replace("[SUBTITLE_COUNT]", subtitle.Paragraphs.Count.ToString(CultureInfo.InvariantCulture));
            xml.LoadXml(template);
            var subtitlesNode = xml.DocumentElement.SelectSingleNode("Subtitles");
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                var subNode = MakeSubNode(xml, p, i + 1);
                subtitlesNode.AppendChild(subNode);
            }
            return ToUtf8XmlString(xml);
        }

        private static XmlNode MakeSubNode(XmlDocument xml, Paragraph p, int index)
        {
            XmlNode subtitle = xml.CreateElement("Subtitle");

            var attrId = xml.CreateAttribute("id");
            attrId.Value = "sub" + index;
            subtitle.Attributes.Append(attrId);

            var attrNumber = xml.CreateAttribute("number");
            attrNumber.Value = index.ToString(CultureInfo.InvariantCulture);
            subtitle.Attributes.Append(attrNumber);

            var attrInCue = xml.CreateAttribute("incue");
            attrInCue.Value = p.StartTime.ToHHMMSSFF();
            subtitle.Attributes.Append(attrInCue);

            var attrOutCue = xml.CreateAttribute("outcue");
            attrOutCue.Value = p.EndTime.ToHHMMSSFF();
            subtitle.Attributes.Append(attrOutCue);

            XmlNode rows = xml.CreateElement("Rows");
            foreach (var line in HtmlUtil.RemoveHtmlTags(p.Text, true).SplitToLines())
            {
                XmlNode row = xml.CreateElement("Row");
                XmlNode text = xml.CreateElement("Text");
                text.InnerText = line;
                row.AppendChild(text);
                rows.AppendChild(row);
            }
            subtitle.AppendChild(rows);

            return subtitle;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            string xml = sb.ToString();
            if (!xml.Contains("<EZTSubtitlesProject", StringComparison.Ordinal))
            {
                return;
            }

            var doc = new XmlDocument { XmlResolver = null };
            doc.LoadXml(xml);
            var subtitles = doc.DocumentElement.SelectSingleNode("Subtitles");
            if (subtitles == null)
            {
                return;
            }

            var splitChars = new[] { ':' };
            foreach (XmlNode subNode in subtitles.SelectNodes("Subtitle"))
            {
                var inCue = subNode.Attributes["incue"];
                var outCue = subNode.Attributes["outcue"];
                var textBuilder = new StringBuilder();
                var rowsNode = subNode.SelectSingleNode("Rows");
                if (inCue == null || outCue == null || rowsNode == null)
                {
                    _errorCount++;
                    continue;
                }
                foreach (XmlNode row in rowsNode.SelectNodes("Row"))
                {
                    textBuilder.AppendLine(row.InnerText);
                }
                var text = textBuilder.ToString().TrimEnd();
                var startMs = DecodeTimeCodeFrames(inCue.InnerText, splitChars).TotalMilliseconds;
                var endMs = DecodeTimeCodeFrames(outCue.InnerText, splitChars).TotalMilliseconds;
                subtitle.Paragraphs.Add(new Paragraph(text, startMs, endMs));
            }
        }
    }
}
