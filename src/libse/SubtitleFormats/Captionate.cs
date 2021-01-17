using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Captionate : SubtitleFormat
    {
        public override string Extension => ".xml";

        public const string NameOfFormat = "Captionate";

        public override string Name => NameOfFormat;

        public override string ToText(Subtitle subtitle, string title)
        {
            const string xmlStructure = @"<captionate>
<timeformat>hh:mm:ss:ff/30</timeformat>
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
            string xmlString;

            if (lines == null && fileName != null)
            {
                xmlString = File.ReadAllText(fileName);
            }
            else if (lines != null)
            {
                var sb = new StringBuilder();
                lines.ForEach(line => sb.AppendLine(line));
                xmlString = sb.ToString();
            }
            else
            {
                return;
            }

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
                        double startMilliseconds = DecodeTimeToMilliseconds(start);
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

        private static double DecodeTimeToMilliseconds(string time)
        {
            string[] parts = time.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
            return new TimeSpan(0, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), (int)(int.Parse(parts[3]) * 10.0)).TotalMilliseconds;
        }

        private static string EncodeTime(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds / 10.0:00}";
        }

    }
}
