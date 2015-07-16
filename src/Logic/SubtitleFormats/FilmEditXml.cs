using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class FilmEditXml : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Film Edit xml"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if (xmlAsString.Contains("</filmeditxml>") && xmlAsString.Contains("</subtitle>"))
            {
                var xml = new XmlDocument();
                xml.XmlResolver = null;
                try
                {
                    xml.LoadXml(xmlAsString);
                    var paragraphs = xml.DocumentElement.SelectNodes("subtitle");
                    return paragraphs != null && paragraphs.Count > 0 && xml.DocumentElement.Name == "filmeditxml";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<filmeditxml>" + Environment.NewLine +
                "<font>Arial</font>" + Environment.NewLine +
                "<points>22</points>" + Environment.NewLine +
                "<width>720</width>" + Environment.NewLine +
                "<height>576</height>" + Environment.NewLine +
                "<virtualwidth>586</virtualwidth>" + Environment.NewLine +
                "<virtualheight>330</virtualheight>" + Environment.NewLine +
                "<par>1420</par>" + Environment.NewLine +
                "<fps>25</fps>" + Environment.NewLine +
                "<dropped>False</dropped>" + Environment.NewLine +
                "</filmeditxml>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            XmlNode div = xml.DocumentElement;
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("subtitle");
                string text = HtmlUtil.RemoveHtmlTags(p.Text);

                XmlNode num = xml.CreateElement("num");
                num.InnerText = no.ToString();
                paragraph.AppendChild(num);

                XmlNode dur = xml.CreateElement("dur");
                num.InnerText = EncodeDuration(p.Duration);
                paragraph.AppendChild(num);

                XmlNode textNode = xml.CreateElement("text");
                textNode.InnerText = p.Text.Replace(Environment.NewLine, "\\N");
                paragraph.AppendChild(textNode);

                XmlNode timeIn = xml.CreateElement("in");
                timeIn.InnerText = EncodeTimeCode(p.StartTime);
                paragraph.AppendChild(timeIn);

                XmlNode timeOut = xml.CreateElement("out");
                timeOut.InnerText = EncodeTimeCode(p.EndTime);
                paragraph.AppendChild(timeOut);

                XmlNode align = xml.CreateElement("align");
                align.InnerText = "C";
                paragraph.AppendChild(align);

                XmlNode posx = xml.CreateElement("posx");
                posx.InnerText = "0";
                paragraph.AppendChild(posx);

                XmlNode post = xml.CreateElement("posy");
                post.InnerText = "308";
                paragraph.AppendChild(post);

                XmlNode memo = xml.CreateElement("memo");
                paragraph.AppendChild(memo);

                div.AppendChild(paragraph);
                no++;
            }

            return ToUtf8XmlString(xml);
        }

        private static string EncodeDuration(TimeCode timeCode)
        {
            return string.Format("{0:00}:{1:00}", timeCode.Seconds, MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds));
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", timeCode.Hours, timeCode.Minutes, timeCode.Seconds, MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument();
            xml.XmlResolver = null;
            xml.LoadXml(sb.ToString().Trim());
            string lastKey = string.Empty;
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("subtitle"))
            {
                try
                {
                    var p = new Paragraph();
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        switch (innerNode.Name)
                        {
                            case "text":
                                p.Text = innerNode.InnerText.Replace("\\N", Environment.NewLine);
                                break;
                            case "in":
                                p.StartTime = DecodeTime(innerNode.InnerText);
                                break;
                            case "out":
                                p.EndTime = DecodeTime(innerNode.InnerText);
                                break;
                        }
                    }
                    if (p.StartTime.TotalSeconds >= 0 && p.EndTime.TotalMilliseconds > 0 && !string.IsNullOrEmpty(p.Text))
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

        private static TimeCode DecodeTime(string s)
        {
            var arr = s.Split(':');
            if (arr.Length == 4)
            {
                return new TimeCode(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), FramesToMillisecondsMax999(int.Parse(arr[3])));
            }
            return new TimeCode(0, 0, 0, 0);
        }

    }
}
