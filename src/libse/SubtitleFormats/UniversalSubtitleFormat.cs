using Nikse.SubtitleEdit.Core.Common;
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

        /// <summary>
        /// Turns the contents of a USF &lt;text&gt; element into subtitle text: line breaks for
        /// &lt;br/&gt;, Subtitle Edit's own tags for the formatting USF shares with HTML, and the
        /// plain text of anything else. Also used for USF tracks inside a Matroska file, where
        /// each block holds one such element and there is no document around it.
        /// </summary>
        public static string GetTextFromUsfNode(XmlNode textNode)
        {
            var sb = new StringBuilder();
            AppendUsfNodes(textNode, sb);
            return sb.ToString().Trim();
        }

        private static void AppendUsfNodes(XmlNode parent, StringBuilder sb)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                switch (node.Name.Replace("tt:", string.Empty).ToLowerInvariant())
                {
                    case "br":
                        sb.AppendLine();
                        break;
                    case "i":
                    case "b":
                    case "u":
                        var tag = node.Name.Replace("tt:", string.Empty).ToLowerInvariant();
                        sb.Append('<').Append(tag).Append('>');
                        AppendUsfNodes(node, sb);
                        sb.Append("</").Append(tag).Append('>');
                        break;
                    case "#text":
                        sb.Append(node.InnerText);
                        break;
                    default:
                        // Unknown inline element (karaoke, ruby, ...) - keep its text, drop the markup
                        AppendUsfNodes(node, sb);
                        if (!node.HasChildNodes)
                        {
                            sb.Append(node.InnerText);
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Reads the payload of one USF subtitle block as stored inside a Matroska
        /// <c>S_TEXT/USF</c> track: one or more &lt;text&gt; elements with no document element
        /// around them. Returns null when the payload is not USF markup after all.
        /// </summary>
        public static string GetTextFromMatroskaBlock(string blockText)
        {
            if (string.IsNullOrWhiteSpace(blockText) || blockText.IndexOf('<') < 0)
            {
                return blockText;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml("<root>" + blockText + "</root>");

                // A block is normally a single <text> element; several of them (one per
                // displayed line) is legal too. Anything else is read as bare markup.
                var textNodes = xml.DocumentElement.SelectNodes("text");
                if (textNodes == null || textNodes.Count == 0)
                {
                    return GetTextFromUsfNode(xml.DocumentElement);
                }

                var sb = new StringBuilder();
                foreach (XmlNode node in textNodes)
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }

                    sb.Append(GetTextFromUsfNode(node));
                }

                return sb.ToString().Trim();
            }
            catch
            {
                return null;
            }
        }

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

            var text = new StringBuilder();
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("subtitles/subtitle"))
            {
                try
                {
                    string start = node.Attributes["start"].InnerText;
                    string stop = node.Attributes["stop"].InnerText;

                    var paragraphText = GetTextFromUsfNode(node.SelectSingleNode("text"));
                    subtitle.Paragraphs.Add(new Paragraph(DecodeTimeCode(start), DecodeTimeCode(stop), paragraphText));
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
