using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class GpacTtxt : SubtitleFormat
    {
        public override string Extension => ".ttxt";

        public override string Name => "GPAC TTXT";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
                "<!-- GPAC 3GPP Text Stream -->" + Environment.NewLine +
                "<TextStream version=\"1.1\">" + Environment.NewLine +
                "  <TextStreamHeader translation_y=\"0\" translation_x=\"0\" layer=\"0\" height=\"60\" width=\"400\">" + Environment.NewLine +
                "    <TextSampleDescription scroll=\"None\" continuousKaraoke=\"no\" fillTextRegion=\"no\" verticalText=\"no\" backColor=\"0 0 0 0\" verticalJustification=\"bottom\" horizontalJustification=\"center\">" + Environment.NewLine +
                "      <FontTable>" + Environment.NewLine +
                "        <FontTableEntry fontID=\"1\" fontName=\"Serif\"/>" + Environment.NewLine +
                "      </FontTable>" + Environment.NewLine +
                "      <TextBox right=\"400\" bottom=\"60\" left=\"0\" top=\"0\"/>" + Environment.NewLine +
                "      <Style fontID=\"1\" color=\"ff ff ff ff\" fontSize=\"18\" styles=\"Normal\"/>" + Environment.NewLine +
                "    </TextSampleDescription>" + Environment.NewLine +
                "  </TextStreamHeader>" + Environment.NewLine +
                "</TextStream>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode textSample = xml.CreateElement("TextSample");

                XmlAttribute preserveSpace = xml.CreateAttribute("xml:space");
                preserveSpace.Value = "preserve";
                textSample.Attributes.Append(preserveSpace);

                XmlAttribute sampleTime = xml.CreateAttribute("sampleTime");
                sampleTime.Value = p.StartTime.ToString().Replace(",", ".");
                textSample.Attributes.Append(sampleTime);

                textSample.InnerText = p.Text;

                xml.DocumentElement.AppendChild(textSample);

                textSample = xml.CreateElement("TextSample");
                preserveSpace = xml.CreateAttribute("xml:space");
                preserveSpace.Value = "preserve";
                textSample.Attributes.Append(preserveSpace);
                sampleTime = xml.CreateAttribute("sampleTime");
                sampleTime.Value = p.EndTime.ToString().Replace(",", ".");
                textSample.Attributes.Append(sampleTime);
                xml.DocumentElement.AppendChild(textSample);
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph last = null;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            if (!sb.ToString().Contains("<TextStream"))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(sb.ToString().Trim());

                foreach (XmlNode node in xml.DocumentElement.SelectNodes("TextSample"))
                {
                    if (last != null && last.EndTime.TotalMilliseconds < 1)
                    {
                        last.EndTime = GetTimeCode(node.Attributes["sampleTime"].Value);
                    }

                    var p = new Paragraph();
                    p.Text = node.InnerText;
                    p.StartTime = GetTimeCode(node.Attributes["sampleTime"].Value);
                    if (string.IsNullOrEmpty(p.Text))
                    {
                        var text = node.Attributes["text"];
                        if (text != null)
                        {
                            p.Text = text.Value;
                        }
                    }
                    if (!string.IsNullOrEmpty(p.Text))
                    {
                        subtitle.Paragraphs.Add(p);
                        last = p;
                    }
                }
                subtitle.Renumber();
            }
            catch
            {
                _errorCount = 1;
            }
        }

        private static TimeCode GetTimeCode(string timeString)
        {
            string[] timeParts = timeString.Split(new[] { ':', '.' });
            return new TimeCode(int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]), int.Parse(timeParts[3]));
        }

    }
}
