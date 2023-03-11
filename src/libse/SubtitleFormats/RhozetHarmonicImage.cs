using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class RhozetHarmonicImage : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Rhozet Harmonic Image";

        private static string ToTimeCode(TimeCode time)
        {
            return time.ToHHMMSSFF();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //<?xml version="1.0"?>
            //<TitlerData>
            //<Data StartTimecode='00:00:55:02' EndTimecode='00:00:56:19' Image='filepath\shHD_jpn0001.png'/>

            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<TitlerData></TitlerData>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            XmlNode paragraph = xml.CreateElement("Data");

            xml.DocumentElement.AppendChild(paragraph);

            foreach (var p in subtitle.Paragraphs)
            {
                paragraph = xml.CreateElement("Data");

                var start = xml.CreateAttribute("StartTimecode");
                start.InnerText = ToTimeCode(p.StartTime);
                paragraph.Attributes.Append(start);

                var end = xml.CreateAttribute("EndTimecode");
                end.InnerText = ToTimeCode(p.EndTime);
                paragraph.Attributes.Append(end);

                var text = xml.CreateAttribute("Image");
                text.InnerText = HtmlUtil.RemoveHtmlTags(p.Text);
                paragraph.Attributes.Append(text);

                xml.DocumentElement.AppendChild(paragraph);
            }

            var s = "<?xml version=\"1.0\"?>" + Environment.NewLine + ToUtf8XmlString(xml, true).Replace("\"", "__@____").Replace("'", "&apos;").Replace("__@____", "'").Replace(" />", "/>");
            while (s.Contains(Environment.NewLine + " "))
            {
                s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
            }

            return s;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string allText = sb.ToString();
            if (!allText.Contains("<TitlerData") || !allText.Contains("<Data"))
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

            char[] splitChars = { ':', ';' };
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Data"))
            {
                try
                {
                    if (node.Attributes?["Image"] != null &&
                        node.Attributes?["StartTimecode"] != null &&
                        node.Attributes?["EndTimecode"] != null)
                    {
                        var text = node.Attributes.GetNamedItem("Image").InnerText.Trim();
                        if (text.StartsWith("filepath\\"))
                        {
                            text = text.Remove(0, "filepath\\".Length);
                        }

                        var start = node.Attributes.GetNamedItem("StartTimecode").InnerText;
                        var end = node.Attributes.GetNamedItem("EndTimecode").InnerText;
                        subtitle.Paragraphs.Add(new Paragraph(DecodeTimeCodeFrames(start, splitChars), DecodeTimeCodeFrames(end, splitChars), text));
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
