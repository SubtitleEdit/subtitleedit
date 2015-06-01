using System;
using System.Collections.Generic;
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
            xml.XmlResolver = null;
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
                subtitle.Renumber();
            }
            catch
            {
                _errorCount = 1;
                return;
            }

        }

    }
}
