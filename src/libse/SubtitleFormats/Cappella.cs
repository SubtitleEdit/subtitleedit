using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Cappella : SubtitleFormat
    {
        public override string Extension => ".detx";

        public override string Name => "Cappella";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure = @"<?xml version='1.0' encoding='utf-8'?>
            <detx copyright='Chinkel S.A., 2007 - 2016'>
            <header>
                <cappella version='3.7.0'/>
                <last_position timecode='00:00:00:00' track='0'/>
            </header>
            <roles>
                <role color='#000000' description='subtitle' id='sub' name='sub'/>
            </roles>
            <body />
            </detx>".Replace('\'', '\"');
            if (subtitle.Paragraphs.Count > 0)
            {
                xmlStructure = xmlStructure.Replace("00:00:00:00", ToTimeCode(subtitle.Paragraphs[subtitle.Paragraphs.Count -1].EndTime));
            }

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("line");
                XmlAttribute roleAttr = xml.CreateAttribute("role");
                roleAttr.InnerText = "sub";
                paragraph.Attributes.Append(roleAttr);
                XmlAttribute trackAttr = xml.CreateAttribute("track");
                trackAttr.InnerText = "0";
                paragraph.Attributes.Append(trackAttr);

                XmlNode time = xml.CreateElement("lipsync");
                XmlAttribute start = xml.CreateAttribute("timecode");
                start.InnerText = ToTimeCode(p.StartTime);
                time.Attributes.Append(start);
                XmlAttribute type = xml.CreateAttribute("type");
                type.InnerText = "in_open";
                time.Attributes.Append(type);
                paragraph.AppendChild(time);

                XmlNode text = xml.CreateElement("text");
                text.InnerText = HtmlUtil.RemoveHtmlTags(Utilities.UnbreakLine(p.Text), true);
                paragraph.AppendChild(text);

                time = xml.CreateElement("lipsync");
                start = xml.CreateAttribute("timecode");
                start.InnerText = ToTimeCode(p.EndTime);
                time.Attributes.Append(start);
                type = xml.CreateAttribute("type");
                type.InnerText = "out_open";
                time.Attributes.Append(type);
                paragraph.AppendChild(time);

                xml.DocumentElement.SelectSingleNode("body").AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        private static string ToTimeCode(TimeCode tc)
        {
            return tc.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlString = sb.ToString();
            if (!xmlString.Contains("<detx") || !xmlString.Contains("<lipsync "))
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

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("body/line"))
            {
                try
                {
                    Paragraph p = new Paragraph();
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        if (innerNode.Name == "text")
                        {
                            p.Text = (p.Text + Environment.NewLine + innerNode.InnerText).Trim();
                        }
                        else if (innerNode.Name == "lipsync")
                        {
                            var typeNode = innerNode.Attributes["type"];
                            var timeCodeNode = innerNode.Attributes["timecode"];
                            if (typeNode != null && timeCodeNode != null)
                            {
                                if (typeNode.InnerText == "in_open")
                                {
                                    p.StartTime.TotalMilliseconds = TimeCode.ParseHHMMSSFFToMilliseconds(timeCodeNode.InnerText);
                                }
                                else if (typeNode.InnerText == "mpb")
                                {
                                    p.EndTime.TotalMilliseconds = TimeCode.ParseHHMMSSFFToMilliseconds(timeCodeNode.InnerText);
                                    if (!string.IsNullOrWhiteSpace(p.Text))
                                    {
                                        subtitle.Paragraphs.Add(p);
                                    }

                                    p = new Paragraph();
                                    p.StartTime.TotalMilliseconds = TimeCode.ParseHHMMSSFFToMilliseconds(timeCodeNode.InnerText) + Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                                }
                                else if (typeNode.InnerText == "out_open")
                                {
                                    p.EndTime.TotalMilliseconds = TimeCode.ParseHHMMSSFFToMilliseconds(timeCodeNode.InnerText);
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(p.Text))
                    {
                        subtitle.Paragraphs.Add(p);
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
