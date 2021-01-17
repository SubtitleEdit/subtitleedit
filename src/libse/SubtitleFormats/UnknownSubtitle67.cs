using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle67 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Unknown 67";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<Data autoStart=\"true\" contentPath=\"\" />";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("Cue");

                XmlAttribute start = xml.CreateAttribute("value");
                start.InnerText = ((long)(Math.Round(p.StartTime.TotalMilliseconds))).ToString(CultureInfo.InvariantCulture);
                paragraph.Attributes.Append(start);

                XmlAttribute duration = xml.CreateAttribute("lineBreakBefore");
                duration.InnerText = "true";
                paragraph.Attributes.Append(duration);

                paragraph.InnerText = p.Text;

                xml.DocumentElement.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string allText = sb.ToString();
            if (!allText.Contains("<Cue") && allText.Contains("value="))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(allText);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                _errorCount = 1;
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Cue"))
            {
                try
                {
                    string start = node.Attributes["value"].InnerText;
                    if (!string.IsNullOrEmpty(start))
                    {
                        start = start.Replace(",", ".");
                    }

                    string text = node.InnerText;

                    subtitle.Paragraphs.Add(new Paragraph(text, Convert.ToDouble(start, CultureInfo.InvariantCulture), 0));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);
                if (i < subtitle.Paragraphs.Count - 1)
                {
                    Paragraph next = subtitle.Paragraphs[i + 1];
                    if (p.EndTime.TotalMilliseconds > next.StartTime.TotalMilliseconds)
                    {
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }
                }
            }
            subtitle.Renumber();
        }

    }
}
