using Nikse.SubtitleEdit.Core.Common;
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
  <ProjectConfiguration>
    <SubtitlesType>open</SubtitlesType>
    <SubtitlesMode>native</SubtitlesMode>
    <DvdSystem>None</DvdSystem>
    <VideoFormat>HD1080</VideoFormat>
    <VideoFrameRate>23.976 fps</VideoFrameRate>
    <AspectRatio>16x9</AspectRatio>
    <Letterbox>none</Letterbox>
    <TimeCodeStandard>30drop</TimeCodeStandard>
    <SafeArea units='pixels' left_edge='192' center='960' right_edge='1728' top_edge='108' bottom_edge='1007' spacing_type='row_interval' spacing='-8' sa_override='true' max_chars='42' max_chars_vertical='36' max_chars_count_halfwidth_as_half='false'>
      <BottomAlign type='above_bottom' base_line='810'/>
      <ClosedCaptions double_control_codes='true' interleave_23_976_CC_data='false' blank_between_popons='2' use_extended_char_set='true'/>
    </SafeArea>
    <Fonts number_of_fonts='2'>
      <Font1 name='Arial' height='25' metric_language='' metric='' bold='false' spacing='0' scalex='100' rtl='false' asian_font_name=''/>
      <Font2 name='Arial Unicode MS' height='54' metric_language='' metric='' bold='false' spacing='0' scalex='100' rtl='false' asian_font_name=''/>
    </Fonts>
  </ProjectConfiguration>
  <Subtitles count='[SUBTITLE_COUNT]'>
  </Subtitles>
</EZTSubtitlesProject>".Replace("'", "\"").Replace("[SUBTITLE_COUNT]", subtitle.Paragraphs.Count.ToString(CultureInfo.InvariantCulture));
            template = template.Replace("23.976", Configuration.Settings.General.CurrentFrameRate.ToString(CultureInfo.InvariantCulture));
            if (Configuration.Settings.General.CurrentFrameRate % 1.0 < 0.001)
            {
                template = template.Replace("30drop", Configuration.Settings.General.CurrentFrameRate.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                template = template.Replace("30drop", ((int)Math.Round(Configuration.Settings.General.CurrentFrameRate)).ToString(CultureInfo.InvariantCulture) + "drop");
            }
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
            var frameRateNode = doc.SelectSingleNode("//VideoFrameRate");
            if (frameRateNode != null && double.TryParse(frameRateNode.InnerText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var frameRate))
            {
                Configuration.Settings.General.CurrentFrameRate = frameRate;
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
            subtitle.Renumber();
        }
    }
}
