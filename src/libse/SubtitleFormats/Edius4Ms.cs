using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Edius4Ms : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Edius 4 Ms";

        public override string ToText(Subtitle subtitle, string title)
        {
            var xmlStructure =
@"<?xml version='1.0' encoding='UTF-16' standalone='no' ?>
<edius:markerInfo xmlns:edius='http://www.grassvalley.com/ns/edius/markerListInfo'>
	<edius:formatVersion>4</edius:formatVersion>
	<edius:CreateDate>Thu Jan 27 13:54:09 2022</edius:CreateDate>
	<edius:markerLists>
	</edius:markerLists>
</edius:markerInfo>".Replace('\'', '"');

            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(xmlStructure);
            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("edius", xml.DocumentElement.NamespaceURI);
            var trackNode = xml.DocumentElement.SelectSingleNode("edius:markerLists", namespaceManager);
            var number = 1;
            foreach (var p in subtitle.Paragraphs)
            {
                XmlNode marker = xml.CreateElement("edius", "marker", "http://www.grassvalley.com/ns/edius/markerListInfo");

                XmlNode no = xml.CreateElement("edius", "no", "http://www.grassvalley.com/ns/edius/markerListInfo");
                no.InnerText = number.ToString(CultureInfo.InvariantCulture);
                marker.AppendChild(no);

                XmlNode anchor = xml.CreateElement("edius", "anchor", "http://www.grassvalley.com/ns/edius/markerListInfo");
                anchor.InnerText = "1";
                marker.AppendChild(anchor);

                XmlNode position = xml.CreateElement("edius", "position", "http://www.grassvalley.com/ns/edius/markerListInfo");
                position.InnerText = EncodeTimeCode(p.StartTime);
                marker.AppendChild(position);

                XmlNode duration = xml.CreateElement("edius", "duration", "http://www.grassvalley.com/ns/edius/markerListInfo");
                duration.InnerText = EncodeTimeCode(p.Duration);
                marker.AppendChild(duration);

                XmlNode comment = xml.CreateElement("edius", "comment", "http://www.grassvalley.com/ns/edius/markerListInfo");
                comment.InnerText = p.Text;
                marker.AppendChild(comment);

                XmlNode color = xml.CreateElement("edius", "color", "http://www.grassvalley.com/ns/edius/markerListInfo");
                color.InnerText = "0xffffffff";
                marker.AppendChild(color);

                trackNode?.AppendChild(marker);
                number++;
            }

            return ToUtf8XmlString(xml);
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            return timeCode.ToString(false).Replace(',', '.');
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            if (!sb.ToString().Contains("<edius:markerLists"))
            {
                _errorCount++;
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(sb.ToString().Trim());
                var namespaceManager = new XmlNamespaceManager(xml.NameTable);
                namespaceManager.AddNamespace("edius", xml.DocumentElement.NamespaceURI);
                foreach (XmlNode node in xml.SelectNodes("//edius:marker", namespaceManager))
                {
                    var p = new Paragraph();
                    var startNode = node.SelectSingleNode("edius:position", namespaceManager);
                    var durationNode = node.SelectSingleNode("edius:duration", namespaceManager);
                    var commentNode = node.SelectSingleNode("edius:comment", namespaceManager);
                    if (startNode != null && durationNode != null && commentNode != null &&
                        ParseTimeCode(startNode.InnerText, out var startMs) &&
                        ParseTimeCode(durationNode.InnerText, out var durationMs))
                    {
                        p.Text = commentNode.InnerText;
                        p.StartTime.TotalMilliseconds = startMs;
                        p.EndTime.TotalMilliseconds = startMs + durationMs;
                        subtitle.Paragraphs.Add(p);
                    }
                    else
                    {
                        _errorCount++;
                    }
                }

                subtitle.Renumber();
            }
            catch
            {
                _errorCount = 1;
                return;
            }

            subtitle.Renumber();
        }

        private static bool ParseTimeCode(string tc, out double ms)
        {
            var arr = tc.Split(':', '.');
            if (arr.Length == 4 && arr[3].Length == 3 &&
                int.TryParse(arr[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var hours) &&
                int.TryParse(arr[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var minute) &&
                int.TryParse(arr[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var seconds) &&
                int.TryParse(arr[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var milliseconds))
            {
                var timeCode = new TimeCode(hours, minute, seconds, milliseconds);
                ms = timeCode.TotalMilliseconds;
                return true;
            }

            ms = 0;
            return false;
        }
    }
}
