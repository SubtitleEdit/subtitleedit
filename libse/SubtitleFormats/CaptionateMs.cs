using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class CaptionateMs : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Captionate MS";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string xmlStructure = @"<captionate>
<timeformat>ms</timeformat>
<namesareprefixed>namesareprefixed</namesareprefixed>
<captioninfo>
<trackinfo>
<track>
<displayname>Default</displayname>
<type/>
<languagecode/>
<targetwpm>140</targetwpm>
<stringdata/>
</track>
</trackinfo>
<speakerinfo>
</speakerinfo>
</captioninfo>
<captions></captions></captionate>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            Paragraph last = null;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (last != null)
                {
                    if (last.EndTime.TotalMilliseconds + 500 < p.StartTime.TotalMilliseconds)
                    {
                        var blank = new Paragraph { StartTime = { TotalMilliseconds = last.EndTime.TotalMilliseconds } };
                        AddParagraph(xml, blank);
                    }
                }

                AddParagraph(xml, p);
                last = p;
            }

            return ToUtf8XmlString(xml, true);
        }

        private static void AddParagraph(XmlDocument xml, Paragraph p)
        {
            XmlNode paragraph = xml.CreateElement("caption");

            XmlAttribute start = xml.CreateAttribute("time");
            start.InnerText = EncodeTime(p.StartTime);
            paragraph.Attributes.Append(start);

            if (!string.IsNullOrWhiteSpace(p.Text))
            {
                XmlNode tracks = xml.CreateElement("tracks");
                paragraph.AppendChild(tracks);

                XmlNode track0 = xml.CreateElement("track0");
                track0.InnerText = HtmlUtil.RemoveHtmlTags(p.Text, true);
                track0.InnerXml = track0.InnerXml.Replace(Environment.NewLine, "<br />");
                tracks.AppendChild(track0);
            }
            xml.DocumentElement.SelectSingleNode("captions").AppendChild(paragraph);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string xmlString = sb.ToString();
            if (!xmlString.Contains("<captionate>") || !xmlString.Contains("</caption>"))
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

            Paragraph p = null;
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("captions/caption"))
            {
                try
                {
                    if (node.Attributes["time"] != null)
                    {
                        string start = node.Attributes["time"].InnerText;
                        double startMilliseconds = double.Parse(start);
                        if (p != null)
                        {
                            p.EndTime.TotalMilliseconds = startMilliseconds - 1;
                        }

                        if (node.SelectSingleNode("tracks/track0") != null)
                        {
                            string text = node.SelectSingleNode("tracks/track0").InnerText;
                            text = HtmlUtil.RemoveHtmlTags(text);
                            text = text.Replace("<br>", Environment.NewLine).Replace("<br />", Environment.NewLine).Replace("<BR>", Environment.NewLine);
                            p = new Paragraph(text, startMilliseconds, startMilliseconds + 3000);
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                subtitle.Paragraphs.Add(p);
                            }
                        }
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

        private static string EncodeTime(TimeCode time)
        {
            return time.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        }

    }
}
