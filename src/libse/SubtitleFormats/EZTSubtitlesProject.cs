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
            if (fileName != null &&
                !(fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) || 
                  fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
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
            var frameRate = Configuration.Settings.General.CurrentFrameRate;
            template = template.Replace("23.976", frameRate.ToString(CultureInfo.InvariantCulture));
            template = template.Replace("30drop", ToTimeCodeStandard(frameRate));
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
            try
            {
                doc.LoadXml(xml);
            }
            catch (Exception exception)
            {
                // A truncated or damaged file must read as "not mine", not throw out of the
                // reader (and out of IsMine, which runs for every format when opening a file).
                System.Diagnostics.Debug.WriteLine(exception.Message);
                _errorCount = 1;
                return;
            }
            var subtitles = doc.DocumentElement.SelectSingleNode("Subtitles");
            if (subtitles == null)
            {
                return;
            }
            // Cues are hh:mm:ss:ff in the time code standard, which need not match the video
            // frame rate (a 23.976 fps project can use "30drop" cues, so ff runs up to 29).
            var timeCodeStandardNode = doc.SelectSingleNode("//TimeCodeStandard");
            if (timeCodeStandardNode != null && TryParseTimeCodeStandard(timeCodeStandardNode.InnerText, out var timeCodeFrameRate))
            {
                Configuration.Settings.General.CurrentFrameRate = timeCodeFrameRate;
            }
            else
            {
                // The value carries a unit ("23.976 fps"), which double.TryParse never accepted.
                var frameRateNode = doc.SelectSingleNode("//VideoFrameRate");
                if (frameRateNode != null && TryParseTimeCodeStandard(frameRateNode.InnerText, out var frameRate))
                {
                    Configuration.Settings.General.CurrentFrameRate = frameRate;
                }
            }

            var splitChars = new[] { ':' };
            var textBuilder = new StringBuilder();
            foreach (XmlNode subNode in subtitles.SelectNodes("Subtitle"))
            {
                var inCue = subNode.Attributes["incue"];
                var outCue = subNode.Attributes["outcue"];
                textBuilder.Clear();
                var rowsNode = subNode.SelectSingleNode("Rows");
                if (inCue == null || outCue == null || rowsNode == null)
                {
                    _errorCount++;
                    continue;
                }
                foreach (XmlNode row in rowsNode.SelectNodes("Row"))
                {
                    textBuilder.AppendLine(GetRowText(row));
                }
                var text = textBuilder.ToString().TrimEnd();
                var alignment = GetAssAlignment(subNode.SelectSingleNode("VisualAttributes"));
                if (alignment != null && text.Length > 0)
                {
                    text = alignment + text;
                }
                // Subtitles without timing have "--:--:--:--" cues.
                var startMs = inCue.InnerText.StartsWith("-", StringComparison.Ordinal) ? 0 : DecodeTimeCodeFrames(inCue.InnerText, splitChars).TotalMilliseconds;
                var endMs = outCue.InnerText.StartsWith("-", StringComparison.Ordinal) ? 0 : DecodeTimeCodeFrames(outCue.InnerText, splitChars).TotalMilliseconds;
                subtitle.Paragraphs.Add(new Paragraph(text, startMs, endMs));
            }
            subtitle.Renumber();
        }

        /// <summary>
        /// Row text with italic/foreground_color from the row and from its inline spans, e.g.
        /// &lt;Row italic="true" foreground_color="red"&gt;&lt;Text&gt;Only &lt;span italic="true"&gt;THIS&lt;/span&gt;&lt;/Text&gt;&lt;/Row&gt;.
        /// </summary>
        private static string GetRowText(XmlNode row)
        {
            var textNode = row.SelectSingleNode("Text");
            if (textNode == null)
            {
                return row.InnerText;
            }

            var sb = new StringBuilder();
            foreach (XmlNode node in textNode.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    sb.Append(ApplyStyle(node, node.InnerText));
                }
                else
                {
                    sb.Append(node.InnerText);
                }
            }

            return ApplyStyle(row, sb.ToString());
        }

        private static string ApplyStyle(XmlNode node, string text)
        {
            if (text.Length == 0)
            {
                return text;
            }

            if (node.Attributes?["italic"]?.Value == "true")
            {
                text = "<i>" + text + "</i>";
            }

            var color = node.Attributes?["foreground_color"]?.Value;
            if (!string.IsNullOrEmpty(color) && color != "white")
            {
                text = "<font color=\"" + color + "\">" + text + "</font>";
            }

            return text;
        }

        private static string GetAssAlignment(XmlNode visualAttributes)
        {
            if (visualAttributes?.Attributes == null)
            {
                return null;
            }

            var verticalAlign = visualAttributes.Attributes["vertical_align"]?.Value;
            var justification = visualAttributes.Attributes["row_justification"]?.Value;
            var row = verticalAlign == "top" ? 7 : verticalAlign == "center" ? 4 : 1;
            var column = justification == "left" ? 0 : justification == "right" ? 2 : 1;
            var an = row + column;
            return an == 2 ? null : "{\\an" + an + "}";
        }

        /// <summary>
        /// EZTitles time code standards: "24", "25", "30", "30drop", "50", "60", "60drop".
        /// </summary>
        public static bool TryParseTimeCodeStandard(string text, out double frameRate)
        {
            frameRate = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var s = text.Trim().ToLowerInvariant();
            var drop = s.EndsWith("drop", StringComparison.Ordinal);
            if (drop)
            {
                s = s.Substring(0, s.Length - 4);
            }

            s = s.Replace("fps", string.Empty).Trim();
            if (!double.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var rate) || rate <= 0)
            {
                return false;
            }

            frameRate = drop ? rate * 1000.0 / 1001.0 : rate;
            if (Math.Abs(frameRate - 30000.0 / 1001.0) < 0.01)
            {
                frameRate = 29.97;
            }
            else if (Math.Abs(frameRate - 60000.0 / 1001.0) < 0.01)
            {
                frameRate = 59.94;
            }
            else if (Math.Abs(frameRate - 24000.0 / 1001.0) < 0.01)
            {
                frameRate = 23.976;
            }

            return true;
        }

        public static string ToTimeCodeStandard(double frameRate)
        {
            if (Math.Abs(frameRate - 29.97) < 0.01)
            {
                return "30drop";
            }

            if (Math.Abs(frameRate - 59.94) < 0.01)
            {
                return "60drop";
            }

            return ((int)Math.Round(frameRate)).ToString(CultureInfo.InvariantCulture);
        }
    }
}
