using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class FLVCoreCuePoints : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "FLVCoreCuePoints";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<FLVCoreCuePoints Version=\"1\" />";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("CuePoint");

                XmlNode startTime = xml.CreateElement("Time");
                startTime.InnerText = p.StartTime.TotalMilliseconds.ToString();
                paragraph.AppendChild(startTime);

                XmlNode paragraphType = xml.CreateElement("Type");
                paragraphType.InnerText = "event";
                paragraph.AppendChild(paragraphType);

                XmlNode name = xml.CreateElement("Name");
                name.InnerText = p.Text;
                paragraph.AppendChild(name);

                XmlNode parameters = xml.CreateElement("Parameters");

                XmlNode parameter = xml.CreateElement("Parameter");
                name = xml.CreateElement("Name");
                name.InnerText = "source";
                XmlNode value = xml.CreateElement("Value");
                value.InnerText = "transcription";
                parameter.AppendChild(name);
                parameter.AppendChild(value);
                parameters.AppendChild(parameter);

                parameter = xml.CreateElement("Parameter");
                name = xml.CreateElement("Name");
                name.InnerText = "duration";
                value = xml.CreateElement("Value");
                value.InnerText = p.Duration.TotalMilliseconds.ToString();
                parameter.AppendChild(name);
                parameter.AppendChild(value);
                parameters.AppendChild(parameter);

                parameter = xml.CreateElement("Parameter");
                name = xml.CreateElement("Name");
                name.InnerText = "confidence";
                value = xml.CreateElement("Value");
                value.InnerText = "50";
                parameter.AppendChild(name);
                parameter.AppendChild(value);
                parameters.AppendChild(parameter);

                paragraph.AppendChild(parameters);

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
            if (!allText.Contains("<FLVCoreCuePoints") && allText.Contains("<CuePoint"))
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

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("CuePoint"))
            {
                try
                {
                    string text = node.SelectSingleNode("Name").InnerText;
                    double start = int.Parse(node.SelectSingleNode("Time").InnerText);
                    double duration = Utilities.GetOptimalDisplayMilliseconds(text);
                    foreach (XmlNode parameter in node.SelectNodes("Parameters/Parameter"))
                    {
                        if (parameter.SelectSingleNode("Name").InnerText == "duration")
                        {
                            duration = int.Parse(parameter.SelectSingleNode("Value").InnerText);
                        }
                    }
                    subtitle.Paragraphs.Add(new Paragraph(text, start, start + duration));
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
