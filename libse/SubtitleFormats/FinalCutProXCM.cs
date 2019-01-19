using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class FinalCutProXCM : SubtitleFormat
    {
        public double FrameRate { get; set; }

        public override string Extension => ".fcpxml";

        public override string Name => "Final Cut Pro X Chapter Marker";

        public override string ToText(Subtitle subtitle, string title)
        {
            if (Configuration.Settings.General.CurrentFrameRate > 26)
            {
                FrameRate = 30;
            }
            else
            {
                FrameRate = 25;
            }

            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>" + Environment.NewLine +
                "<!DOCTYPE fcpxml>" + Environment.NewLine +
                Environment.NewLine +
                "<fcpxml version=\"1.1\">" + Environment.NewLine +
                "  <project name=\"Subtitle Edit subtitle\" uid=\"C1E80D31-57D4-4E6C-84F6-86A75DCB7A54\" eventID=\"B5C98F73-1D7E-4205-AEF3-1485842EB191\" location=\"file://localhost/Volumes/Macintosh%20HD/Final%20Cut%20Projects/Yma%20Sumac/Yma%20LIVE%20in%20Moscow/\" >" + Environment.NewLine +
                "    <resources>" + Environment.NewLine +
                "      <format id=\"r1\" name=\"FFVideoFormatDV720x480i5994\" frameDuration=\"2002/60000s\" fieldOrder=\"lower first\" width=\"720\" height=\"480\" paspH=\"10\" paspV=\"11\"/>" + Environment.NewLine +
                "      <effect id=\"r6\" name=\"Custom\" uid=\".../Titles.localized/Build In:Out.localized/Custom.localized/Custom.moti\"/>" + Environment.NewLine +
                "    </resources>" + Environment.NewLine +
                "    <sequence duration=\"10282752480/2400000s\" format=\"r1\" tcStart=\"0s\" tcFormat=\"NDF\" audioLayout=\"stereo\" audioRate=\"48k\">" + Environment.NewLine +
                "      <spine>" + Environment.NewLine +
                "        <clip offset=\"0s\" name=\"Untitled\" duration=\"147005859/24000s\" tcFormat=\"NDF\">" + Environment.NewLine +
                "          <video offset=\"0s\" ref=\"r6\" duration=\"147005859/24000s\">" + Environment.NewLine +
                "            <audio lane=\"-1\" offset=\"0s\" ref=\"r6\" duration=\"147005000/24000s\" role=\"dialog\"/>" + Environment.NewLine +
                "          </video>" + Environment.NewLine +
                "        </clip>" + Environment.NewLine +
                "      </spine>" + Environment.NewLine +
                "    </sequence>" + Environment.NewLine +
                "  </project>" + Environment.NewLine +
                "</fcpxml>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            XmlNode videoNode = xml.DocumentElement.SelectSingleNode("project/sequence/spine/clip");

            int number = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode chapterMarker = xml.CreateElement("chapter-marker");

                var attr = xml.CreateAttribute("duration");
                attr.Value = Convert.ToInt64(p.Duration.TotalSeconds * 2400000) + "/2400000s";
                chapterMarker.Attributes.Append(attr);

                attr = xml.CreateAttribute("start");
                attr.Value = Convert.ToInt64(p.StartTime.TotalSeconds * 2400000) + "/2400000s";
                chapterMarker.Attributes.Append(attr);

                attr = xml.CreateAttribute("value");
                attr.Value = p.Text.Replace(Environment.NewLine, Convert.ToChar(8232).ToString());
                chapterMarker.Attributes.Append(attr);

                attr = xml.CreateAttribute("posterOffset");
                attr.Value = "11/24s";
                chapterMarker.Attributes.Append(attr);

                videoNode.AppendChild(chapterMarker);
                number++;
            }

            string xmlAsText = ToUtf8XmlString(xml);
            xmlAsText = xmlAsText.Replace("fcpxml[]", "fcpxml");
            xmlAsText = xmlAsText.Replace("fcpxml []", "fcpxml");
            return xmlAsText;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            FrameRate = Configuration.Settings.General.CurrentFrameRate;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            xml.PreserveWhitespace = true;
            try
            {
                xml.LoadXml(sb.ToString().Trim());

                foreach (XmlNode node in xml.SelectNodes("fcpxml/project/sequence/spine/clip/chapter-marker"))
                {
                    try
                    {
                        var p = new Paragraph();
                        p.Text = node.Attributes["value"].InnerText;
                        p.Text = p.Text.Replace(Convert.ToChar(8232).ToString(), Environment.NewLine);
                        p.StartTime = DecodeTime(node.Attributes["start"]);
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + DecodeTime(node.Attributes["duration"]).TotalMilliseconds;
                        subtitle.Paragraphs.Add(p);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                subtitle.Renumber();
            }
            catch
            {
                _errorCount = 1;
            }
        }

        private static TimeCode DecodeTime(XmlAttribute duration)
        {
            // 220220/60000s
            if (duration != null)
            {
                var arr = duration.Value.TrimEnd('s').Split('/');
                if (arr.Length == 2)
                {
                    return TimeCode.FromSeconds(long.Parse(arr[0]) / double.Parse(arr[1]));
                }
                else if (arr.Length == 1)
                {
                    return TimeCode.FromSeconds(float.Parse(arr[0]));
                }
            }
            return new TimeCode();
        }

    }
}
