using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Tmx14 : SubtitleFormat
    {
        public override string Extension => ".tmx";

        public override string Name => "Translation Memory xml";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<tmx version=\"1.4\">" + Environment.NewLine +
                "  <header creationtool=\"Subtitle Edit\" creationtoolversion=\"3.5\" datatype=\"html\" segtype=\"sentence\" adminlang=\"en-us\" srclang=\"EN\" o-encoding=\"utf-8\">" + Environment.NewLine +
                "    <note>This is a subtitle</note>" + Environment.NewLine +
                "  </header>" + Environment.NewLine +
                "  <body />" + Environment.NewLine +
                "</tmx>";

            string lang = LanguageAutoDetect.AutoDetectLanguageName("en_US", subtitle);
            if (lang.StartsWith("en_", StringComparison.Ordinal))
            {
                lang = "EN";
            }
            else if (lang.Length == 5)
            {
                lang = lang.Substring(3);
            }

            string paragraphInnerXml =
                "  <prop type=\"start\">[START]</prop>" + Environment.NewLine +
                "  <prop type=\"end\">[END]</prop>" + Environment.NewLine +
                "  <tuv xml:lang=\"" + lang + "\">" + Environment.NewLine +
                "    <seg>[TEXT]</seg>" + Environment.NewLine +
                "  </tuv>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            XmlNode body = xml.DocumentElement.SelectSingleNode("body");
            int count = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("tu");

                XmlAttribute tuid = xml.CreateAttribute("tuid");
                tuid.InnerText = count.ToString("D4");
                paragraph.Attributes.Append(tuid);

                XmlAttribute datatype = xml.CreateAttribute("datatype");
                datatype.InnerText = "html";
                paragraph.Attributes.Append(datatype);

                string innerXml = paragraphInnerXml;
                innerXml = innerXml.Replace("[START]", p.StartTime.ToString());
                innerXml = innerXml.Replace("[END]", p.EndTime.ToString());
                paragraph.InnerXml = innerXml;

                XmlNode textNode = paragraph.SelectSingleNode("tuv/seg");
                textNode.InnerText = HtmlUtil.RemoveHtmlTags(p.Text, true);
                textNode.InnerXml = textNode.InnerXml.Replace(Environment.NewLine, "<br />");

                body.AppendChild(paragraph);
                count++;
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string xmlString = sb.ToString();
            if (!xmlString.Contains("<tmx") || !xmlString.Contains("<seg>"))
            {
                return;
            }

            var xml = new XmlDocument();
            try
            {
                xml.XmlResolver = null; // skip any DTD
                xml.LoadXml(xmlString);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                _errorCount = 1;
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//tu"))
            {
                try
                {
                    XmlNode start = node.SelectSingleNode("prop[@type='start']");
                    XmlNode end = node.SelectSingleNode("prop[@type='end']");
                    XmlNode seg = node.SelectSingleNode("tuv/seg");

                    if (seg != null)
                    {
                        string text = seg.InnerText.Replace("<br />", Environment.NewLine);
                        text = text.Replace("<br/>", Environment.NewLine);
                        text = text.Replace("<br>", Environment.NewLine);
                        text = text.Replace("<BR />", Environment.NewLine);
                        text = text.Replace("<BR/>", Environment.NewLine);
                        text = text.Replace("<BR>", Environment.NewLine);
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

        private static TimeCode DecodeTimeCode(XmlNode node)
        {
            var tc = new TimeCode();
            if (node != null)
            {
                string[] arr = node.InnerText.Split(new[] { ':', '.', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                tc = new TimeCode(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), int.Parse(arr[3]));
            }
            return tc;
        }

    }
}
