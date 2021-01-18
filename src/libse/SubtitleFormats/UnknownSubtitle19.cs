using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle19 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Unknown 19";

        private static string ToTimeCode(TimeCode time)
        {
            return $"{time.TotalSeconds:0.0}";
        }

        private static TimeCode DecodeTimeCode(string s)
        {
            return TimeCode.FromSeconds(double.Parse(s));
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //<Subtitle version="1.0" timeline="ee_disc1" name="Subtitle 1:" language="English" type="image">
            //  <Clip start="121.888" end="125.092" fileName="ee_disc1_subtitle_1/Subtitle_1.png" text="Hello.My name is Laura Knight-Jadcyzk" x="155" y="364" width="328" height="77"/>
            //  <Clip start="125.125" end="129.262" fileName="ee_disc1_subtitle_1/Subtitle_2.png" text="and welcome to the Éiriú Eolasbreathing and meditation program" x="145" y="364" width="348" height="77"/>
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<Subtitle version=\"1.0\" timeline=\"ee_disc1\" name=\"Subtitle 1:\" language=\"English\" type=\"text\"></Subtitle>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("Clip");

                XmlAttribute attr = xml.CreateAttribute("start");
                attr.InnerText = ToTimeCode(p.StartTime);
                paragraph.Attributes.Append(attr);

                attr = xml.CreateAttribute("end");
                attr.InnerText = ToTimeCode(p.EndTime);
                paragraph.Attributes.Append(attr);

                attr = xml.CreateAttribute("text");
                attr.InnerText = HtmlUtil.RemoveHtmlTags(p.Text);
                paragraph.Attributes.Append(attr);

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
            if (!allText.Contains("</Subtitle>") || !allText.Contains("<Clip "))
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

            if (xml.DocumentElement == null)
            {
                _errorCount = 1;
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Clip"))
            {
                try
                {
                    if (node.Attributes != null)
                    {
                        string text = node.Attributes.GetNamedItem("text").InnerText.Trim();
                        string start = node.Attributes.GetNamedItem("start").InnerText;
                        string end = node.Attributes.GetNamedItem("end").InnerText;
                        subtitle.Paragraphs.Add(new Paragraph(DecodeTimeCode(start), DecodeTimeCode(end), text));
                    }
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
