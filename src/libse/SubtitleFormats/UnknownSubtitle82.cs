using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    internal class UnknownSubtitle82 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Unknown 82";

   public override string ToText(Subtitle subtitle, string title)
        {
            const string xmpTemplate = @"<?xml version='1.0' encoding='utf-8'?>
<timedtext format='3'>
    <body />
</timedtext>";

            var xml = new XmlDocument();
            xml.LoadXml(xmpTemplate.Replace('\'', '"'));
            var paragraphInsertNode = xml.DocumentElement.SelectSingleNode("body");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("p");
                paragraph.InnerText = p.Text;//.Replace(Environment.NewLine, " ");

                XmlAttribute tAttribute = xml.CreateAttribute("t");
                tAttribute.InnerText = Convert.ToInt64(p.StartTime.TotalMilliseconds).ToString();
                paragraph.Attributes.Append(tAttribute);

                XmlAttribute dAttribute = xml.CreateAttribute("d");
                dAttribute.InnerText = Convert.ToInt64(p.Duration.TotalMilliseconds).ToString();
                paragraph.Attributes.Append(dAttribute);

                paragraphInsertNode.AppendChild(paragraph);
            }
            return ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xmlAsText = sb.ToString().Trim();
            if (!xmlAsText.Contains("</timedtext>") || !xmlAsText.Contains("<p "))
            {
                return;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(xmlAsText);
                foreach (XmlNode node in xml.DocumentElement.SelectNodes("body/p"))
                {
                    try
                    {
                        var timeCodeIn = new TimeCode(Convert.ToDouble(node.Attributes["t"].InnerText));
                        var timeCodeOut = new TimeCode(timeCodeIn.TotalMilliseconds + Convert.ToDouble(node.Attributes["d"].InnerText));
                        var p = new Paragraph(timeCodeIn, timeCodeOut, node.InnerText);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        _errorCount++;
                    }
                }
                subtitle.Renumber();
            }
            catch (Exception)
            {
                _errorCount++;
            }
        }

    }
}
