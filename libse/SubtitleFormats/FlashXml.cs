using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

    //<?xml version="1.0" encoding="UTF-8"?>
    //<tt>
    //    <div>
    //      <p begin="00:00:00.700" end="00:00:05.000"><![CDATA[<sub>This is fully skinnable through XML<br/>using external images for each button</sub>]]></p>
    //      <p begin="00:00:05.200" end="00:00:10.000"><![CDATA[<sub>You can put in any order or enable/disable<br/>the control buttons</sub>]]></p>
    //      <p begin="00:00:10.200" end="00:00:15.000"><![CDATA[<sub>Test below some of the customizable<br/>properties this player has</sub>]]></p>
    //      <p begin="00:00:15.200" end="00:00:19.700"><![CDATA[<sub>Many other properties related to fonts, sizes, colors<br/>and list properties are in style.css file</sub>]]></p>
    // </div>
    //</tt>
    public class FlashXml : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Flash Xml";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if ((xmlAsString.Contains("<tt>") || xmlAsString.Contains("<tt ")) && (xmlAsString.Contains("<sub>")))
            {
                var xml = new XmlDocument { XmlResolver = null };
                try
                {
                    xml.LoadXml(xmlAsString);
                    var paragraphs = xml.DocumentElement.SelectNodes("div/p");
                    return paragraphs != null && paragraphs.Count > 0 && xml.DocumentElement.Name == "tt";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            return false;
        }

        private static string ConvertToTimeString(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds:00}";
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<tt>" + Environment.NewLine +
                "   <div>" + Environment.NewLine +
                "   </div>" + Environment.NewLine +
                "</tt>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            XmlNode div = xml.DocumentElement.SelectSingleNode("div");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("p");
                string text = HtmlUtil.RemoveHtmlTags(p.Text, true);

                paragraph.InnerText = text;
                paragraph.InnerXml = "<![CDATA[<sub>" + paragraph.InnerXml.Replace(Environment.NewLine, "<br />") + "</sub>]]>";

                XmlAttribute start = xml.CreateAttribute("begin");
                start.InnerText = ConvertToTimeString(p.StartTime);
                paragraph.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("end");
                end.InnerText = ConvertToTimeString(p.EndTime);
                paragraph.Attributes.Append(end);

                div.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            double startSeconds = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(sb.ToString().Trim());

            var pText = new StringBuilder();
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("div/p"))
            {
                try
                {
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        if (innerNode.Name == "br")
                        {
                            pText.AppendLine();
                        }
                        else
                        {
                            pText.Append(innerNode.InnerText.Trim());
                        }
                    }

                    var start = string.Empty;
                    if (node.Attributes["begin"] != null)
                    {
                        start = node.Attributes["begin"].InnerText;
                    }

                    var end = string.Empty;
                    if (node.Attributes["end"] != null)
                    {
                        end = node.Attributes["end"].InnerText;
                    }

                    var dur = string.Empty;
                    if (node.Attributes["dur"] != null)
                    {
                        dur = node.Attributes["dur"].InnerText;
                    }

                    TimeCode startCode = TimeCode.FromSeconds(startSeconds);
                    if (start.Length > 0)
                    {
                        startCode = GetTimeCode(start);
                    }

                    TimeCode endCode;
                    if (end.Length > 0)
                    {
                        endCode = GetTimeCode(end);
                    }
                    else if (dur.Length > 0)
                    {
                        endCode = new TimeCode(GetTimeCode(dur).TotalMilliseconds + startCode.TotalMilliseconds);
                    }
                    else
                    {
                        endCode = new TimeCode(startCode.TotalMilliseconds + 3000);
                    }
                    startSeconds = endCode.TotalSeconds;

                    subtitle.Paragraphs.Add(new Paragraph(startCode, endCode, pText.ToString().Replace("<sub>", string.Empty).Replace("</sub>", string.Empty)));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
                pText.Clear();
            }
            subtitle.Renumber();
        }

        private static TimeCode GetTimeCode(string s)
        {
            if (s.EndsWith('s'))
            {
                s = s.TrimEnd('s');
                return TimeCode.FromSeconds(double.Parse(s));
            }
            var parts = s.Split(new[] { ':', '.', ',' });
            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
        }

    }
}
