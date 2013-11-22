using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class FinalCutProImage : SubtitleFormat
    {
        public double FrameRate { get; set; }

        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Final Cut Pro Image"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > 0;
        }

        private string IsNtsc()
        {
            if (FrameRate >= 29.976 && FrameRate <= 30.0)
                return "TRUE";
            if (FrameRate < 29.976)
                return "FALSE";
            return "TRUE";
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            FrameRate = Configuration.Settings.General.CurrentFrameRate;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(sb.ToString().Trim());

                foreach (XmlNode node in xml.DocumentElement.SelectNodes("sequence/media/video/track/clipitem"))
                {
                    try
                    {
                        XmlNode fileNode = node.SelectSingleNode("file");
                        if (fileNode != null)
                        {
                            XmlNode fileNameNode = fileNode.SelectSingleNode("name");
                            XmlNode filePathNode = fileNode.SelectSingleNode("pathurl");
                            if (fileNameNode != null)
                            {
                                var p = new Paragraph();
                                p.Text = fileNameNode.InnerText;
                                XmlNode inNode = node.SelectSingleNode("in");
                                XmlNode startNode = node.SelectSingleNode("start");
                                if (inNode != null)
                                {
                                    p.StartTime.TotalMilliseconds = FramesToMilliseconds(Convert.ToInt32(inNode.InnerText));
                                }
                                else if (startNode != null)
                                {
                                    p.StartTime.TotalMilliseconds = FramesToMilliseconds(Convert.ToInt32(startNode.InnerText));
                                }
                                XmlNode outNode = node.SelectSingleNode("out");
                                XmlNode endNode = node.SelectSingleNode("end");
                                if (outNode != null)
                                {
                                    p.EndTime.TotalMilliseconds = FramesToMilliseconds(Convert.ToInt32(outNode.InnerText));
                                }
                                else if (endNode != null)
                                {
                                    p.EndTime.TotalMilliseconds = FramesToMilliseconds(Convert.ToInt32(endNode.InnerText));
                                }
                                subtitle.Paragraphs.Add(p);
                            }
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                subtitle.Renumber(1);
            }
            catch
            {
                _errorCount = 1;
                return;
            }

        }

        private TimeCode DecodeTime(XmlAttribute duration)
        {
            // 220220/60000s
            if (duration != null)
            {

                var arr = duration.Value.TrimEnd('s').Split('/');
                if (arr.Length == 2)
                {
                    return new TimeCode(TimeSpan.FromSeconds(long.Parse(arr[0]) / double.Parse(arr[1]) ));
                }
                else if (arr.Length == 1)
                {
                    return new TimeCode(TimeSpan.FromSeconds(float.Parse(arr[0])));
                }
            }
            return new TimeCode(0, 0, 0, 0);
        }

    }
}


