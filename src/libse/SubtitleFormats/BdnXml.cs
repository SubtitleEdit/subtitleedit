using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class BdnXml : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "BDN Xml";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<Subtitle/>";

            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(xmlStructure);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("Paragraph");

                XmlNode number = xml.CreateElement("Number");
                number.InnerText = p.Number.ToString(CultureInfo.InvariantCulture);
                paragraph.AppendChild(number);

                XmlNode start = xml.CreateElement("StartMilliseconds");
                start.InnerText = p.StartTime.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
                paragraph.AppendChild(start);

                XmlNode end = xml.CreateElement("EndMilliseconds");
                end.InnerText = p.EndTime.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
                paragraph.AppendChild(end);

                XmlNode text = xml.CreateElement("Text");
                text.InnerText = HtmlUtil.RemoveHtmlTags(p.Text, true);
                paragraph.AppendChild(text);

                xml.DocumentElement.AppendChild(paragraph);
            }
            string textUtf8;
            using (var ms = new MemoryStream())
            {
                var writer = new XmlTextWriter(ms, Encoding.UTF8) { Formatting = Formatting.Indented };
                xml.Save(writer);
                textUtf8 = Encoding.UTF8.GetString(ms.ToArray());
            }
            return textUtf8.Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string xmlString = sb.ToString();
            if (!xmlString.Contains("<BDN"))
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
            char[] splitChars = { ':', ';' };
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Events/Event"))
            {
                try
                {
                    string start = node.Attributes["InTC"].InnerText;
                    string end = node.Attributes["OutTC"].InnerText;
                    var textBuilder = new StringBuilder();
                    var position = string.Empty;
                    foreach (XmlNode graphic in node.SelectNodes("Graphic"))
                    {
                        if (graphic.Attributes["X"] != null || graphic.Attributes["Y"] != null)
                        {
                            position = graphic.Attributes["X"].InnerText + "," + graphic.Attributes["Y"].InnerText;
                        }

                        textBuilder.AppendLine(graphic.InnerText);
                    }

                    var p = new Paragraph(textBuilder.ToString().Trim(), DecodeTimeCodeFrames(start, splitChars).TotalMilliseconds, DecodeTimeCodeFrames(end, splitChars).TotalMilliseconds);
                    if (node.Attributes["Forced"] != null && node.Attributes["Forced"].Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        p.Forced = true;
                    }

                    if (!string.IsNullOrEmpty(position))
                    {
                        p.Extra = position;
                    }

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

    }
}
