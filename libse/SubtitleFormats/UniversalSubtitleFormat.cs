using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UniversalSubtitleFormat : SubtitleFormat
    {
        public override string Extension => ".usf";

        public override string Name => "Universal Subtitle Format";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<USFSubtitles version=\"1.0\">" + Environment.NewLine +
                @"<metadata>
    <title>Universal Subtitle Format</title>
    <author>
      <name>SubtitleEdit</name>
      <email>nikse.dk@gmail.com</email>
      <url>https://www.nikse.dk/</url>
    </author>" + Environment.NewLine +
"   <language code=\"eng\">English</language>" + Environment.NewLine +
@"  <date>[DATE]</date>
    <comment>This is a USF file</comment>
  </metadata>
  <styles>
    <!-- Here we redefine the default style -->" + Environment.NewLine +
                "    <style name=\"Default\">" + Environment.NewLine +
                "      <fontstyle face=\"Arial\" size=\"24\" color=\"#FFFFFF\" back-color=\"#AAAAAA\" />" +
                Environment.NewLine +
                "      <position alignment=\"BottomCenter\" vertical-margin=\"20%\" relative-to=\"Window\" />" +
                @"    </style>
  </styles>

  <subtitles>
  </subtitles>
</USFSubtitles>";
            xmlStructure = xmlStructure.Replace("[DATE]", DateTime.Now.ToString("yyyy-MM-dd"));

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            xml.DocumentElement.SelectSingleNode("metadata/title").InnerText = title;
            var subtitlesNode = xml.DocumentElement.SelectSingleNode("subtitles");

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("subtitle");

                XmlAttribute start = xml.CreateAttribute("start");
                start.InnerText = p.StartTime.ToString().Replace(",", ".");
                paragraph.Attributes.Prepend(start);

                XmlAttribute stop = xml.CreateAttribute("stop");
                stop.InnerText = p.EndTime.ToString().Replace(",", ".");
                paragraph.Attributes.Append(stop);

                XmlNode text = xml.CreateElement("text");
                bool first = true;
                foreach (string line in HtmlUtil.RemoveHtmlTags(p.Text, true).SplitToLines())
                {
                    if (!first)
                    {
                        XmlNode br = xml.CreateElement("br");
                        text.AppendChild(br);
                    }
                    first = false;
                    var t = xml.CreateTextNode(string.Empty);
                    t.InnerText = line;
                    text.AppendChild(t);
                }
                paragraph.AppendChild(text);

                XmlAttribute style = xml.CreateAttribute("style");
                style.InnerText = "Default";
                text.Attributes.Append(style);

                subtitlesNode.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        private static TimeCode DecodeTimeCode(string code)
        {
            string[] parts = code.Split(new[] { ':', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                return new TimeCode(0, 0, int.Parse(code), 0); // seconds only
            }
            if (parts.Length == 2)
            {
                return new TimeCode(0, 0, int.Parse(parts[0]), int.Parse(parts[1])); // seconds + ms
            }

            //00:00:07:120
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string ms = parts[3];
            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), int.Parse(ms));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string xmlString = sb.ToString();
            if (!xmlString.Contains("<USFSubtitles") || !xmlString.Contains("<subtitles>"))
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

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("subtitles/subtitle"))
            {
                try
                {
                    string start = node.Attributes["start"].InnerText;
                    string stop = node.Attributes["stop"].InnerText;

                    var text = new StringBuilder();
                    foreach (XmlNode innerNode in node.SelectSingleNode("text").ChildNodes)
                    {
                        switch (innerNode.Name.Replace("tt:", string.Empty))
                        {
                            case "br":
                                text.AppendLine();
                                break;
                            default:
                                text.Append(innerNode.InnerText);
                                break;
                        }
                    }

                    subtitle.Paragraphs.Add(new Paragraph(DecodeTimeCode(start), DecodeTimeCode(stop), text.ToString().Trim()));
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
