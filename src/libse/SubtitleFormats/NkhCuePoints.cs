using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class NkhCuePoints : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "NkhCuePoints";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<FLVCoreCuePoints Version=\"1\" />";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            foreach (var p in subtitle.Paragraphs)
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
                value.InnerText = p.DurationTotalMilliseconds.ToString();
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
            var allText = sb.ToString().Trim();

            var startRemove = 5;
            while (allText.Length > 0 && allText[0] != '<' && startRemove > 0)
            {
                allText = allText.Remove(0, 1);
                startRemove--;
            }

            if (!allText.Contains("<cuepoints") && allText.Contains("<subtitle"))
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

            var lastWasSubtitle = false;
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//cuepoints/cuepoint"))
            {
                var timeAttribute = node.Attributes["time"];
                if (timeAttribute != null && double.TryParse(timeAttribute.InnerText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var seconds))
                {
                    var subtitleNode = node.SelectSingleNode("subtitle");
                    if (subtitleNode != null)
                    {
                        var text = node.InnerText;
                        subtitle.Paragraphs.Add(new Paragraph(text, seconds * 1000.0, seconds * 1000.0 + Configuration.Settings.General.NewEmptyDefaultMs));
                        lastWasSubtitle = true;
                    }
                    else if (lastWasSubtitle)
                    {
                        subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds = seconds * 1000.0;
                    }
                }
                else
                {
                    _errorCount++;
                    lastWasSubtitle = false;
                }
            }

            subtitle.Renumber();
        }
    }
}
