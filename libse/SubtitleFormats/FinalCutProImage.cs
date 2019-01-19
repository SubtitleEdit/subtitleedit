using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class FinalCutProImage : SubtitleFormat
    {
        public double FrameRate { get; set; }

        public override string Extension => ".xml";

        public override string Name => "Final Cut Pro Image";

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
            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(sb.ToString().Trim());

                if (xml.DocumentElement.SelectSingleNode("sequence/rate") != null && xml.DocumentElement.SelectSingleNode("sequence/rate/timebase") != null)
                {
                    try
                    {
                        var frameRate = double.Parse(xml.DocumentElement.SelectSingleNode("sequence/rate/timebase").InnerText);
                        if (frameRate > 10 && frameRate < 2000)
                        {
                            Configuration.Settings.General.CurrentFrameRate = frameRate;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }


                foreach (XmlNode node in xml.DocumentElement.SelectNodes("sequence/media/video/track/clipitem"))
                {
                    try
                    {
                        XmlNode fileNode = node.SelectSingleNode("file");
                        if (fileNode != null)
                        {
                            XmlNode fileNameNode = fileNode.SelectSingleNode("name");
                            XmlNode pathurlNode = fileNode.SelectSingleNode("pathurl");
                            if (fileNameNode != null)
                            {
                                var p = new Paragraph();
                                p.Text = fileNameNode.InnerText;
                                if (pathurlNode != null)
                                {
                                    p.Extra = pathurlNode.InnerText;
                                }

                                XmlNode inNode = node.SelectSingleNode("in");
                                XmlNode startNode = node.SelectSingleNode("start");
                                if (startNode != null)
                                {
                                    p.StartTime.TotalMilliseconds = FramesToMilliseconds(Convert.ToInt32(startNode.InnerText));
                                }
                                else if (inNode != null)
                                {
                                    p.StartTime.TotalMilliseconds = FramesToMilliseconds(Convert.ToInt32(inNode.InnerText));
                                }
                                XmlNode outNode = node.SelectSingleNode("out");
                                XmlNode endNode = node.SelectSingleNode("end");
                                if (endNode != null)
                                {
                                    p.EndTime.TotalMilliseconds = FramesToMilliseconds(Convert.ToInt32(endNode.InnerText));
                                }
                                else if (outNode != null)
                                {
                                    p.EndTime.TotalMilliseconds = FramesToMilliseconds(Convert.ToInt32(outNode.InnerText));
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
            }
        }

    }
}
