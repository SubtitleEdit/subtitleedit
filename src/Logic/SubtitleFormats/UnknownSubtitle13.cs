using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class UnknownSubtitle13 : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Unknown 13"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            Subtitle subtitle = new Subtitle();
            this.LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > 0;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<subtitle><config bgAlpha=\"0.5\" bgColor=\"0x000000\" defaultColor=\"0xCCffff\" fontSize=\"16\"/></subtitle>";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            int id = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("entry");

                XmlAttribute duration = xml.CreateAttribute("timeOut");
                duration.InnerText = p.EndTime.ToString();
                paragraph.Attributes.Append(duration);

                XmlAttribute start = xml.CreateAttribute("timeIn");
                start.InnerText = p.StartTime.ToString();
                paragraph.Attributes.Append(start);

                XmlAttribute idAttr = xml.CreateAttribute("id");
                idAttr.InnerText = id.ToString();
                paragraph.Attributes.Append(idAttr);

                paragraph.InnerText = "<![CDATA[" + p.Text + "]]";

                xml.DocumentElement.AppendChild(paragraph);
                id++;
            }

            MemoryStream ms = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(ms, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            xml.Save(writer);
            return Encoding.UTF8.GetString(ms.ToArray()).Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string allText = sb.ToString();
            if (!allText.Contains("<subtitle>") || !allText.Contains("timeIn="))
                return;

            XmlDocument xml = new XmlDocument();
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

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("entry"))
            {
                try
                {
                    string start = node.Attributes["timeIn"].InnerText;
                    string end = node.Attributes["timeOut"].InnerText;
                    string text = node.InnerText;
                    if (text.StartsWith("![CDATA["))
                        text = text.Remove(0, 8);
                    if (text.StartsWith("]]"))
                        text = text.Remove(text.Length-3, 2);

                    subtitle.Paragraphs.Add(new Paragraph(DecodeTimeCode(start), DecodeTimeCode(end), text));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }

        private TimeCode DecodeTimeCode(string timeCode)
        {
            string[] arr = timeCode.Split(":,.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int hours = int.Parse(arr[0]);
            int minutes = int.Parse(arr[1]);
            int seconds = int.Parse(arr[2]);
            int ms = int.Parse(arr[3]);
            return new TimeCode(hours, minutes, seconds, ms);
        }


    }
}


