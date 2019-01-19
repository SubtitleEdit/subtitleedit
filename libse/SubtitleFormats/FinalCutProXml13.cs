using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class FinalCutProXml13 : SubtitleFormat
    {
        public double FrameRate { get; set; }

        public override string Extension => ".fcpxml";

        public override string Name => "Final Cut Pro Xml 1.3";

        public override string ToText(Subtitle subtitle, string title)
        {
            if (Configuration.Settings.General.CurrentFrameRate > 26)
            {
                FrameRate = 30;
            }
            else
            {
                FrameRate = 25;
            }

            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>" + Environment.NewLine +
                "<!DOCTYPE fcpxml>" + Environment.NewLine +
                Environment.NewLine +
                "<fcpxml version=\"1.3\">" + Environment.NewLine +
                "  <project name=\"Subtitle Edit subtitle\" uid=\"C1E80D31-57D4-4E6C-84F6-86A75DCB7A54\" eventID=\"B5C98F73-1D7E-4205-AEF3-1485842EB191\" location=\"file://localhost/Volumes/Macintosh%20HD/Final%20Cut%20Projects/Yma%20Sumac/Yma%20LIVE%20in%20Moscow/\" >" + Environment.NewLine +
                "    <resources>" + Environment.NewLine +
                "      <format id=\"r1\" name=\"FFVideoFormatDV720x480i5994\" frameDuration=\"2002/60000s\" fieldOrder=\"lower first\" width=\"720\" height=\"480\" paspH=\"10\" paspV=\"11\"/>" + Environment.NewLine +
                "      <projectRef id=\"r2\" name=\"Yma DVD\" uid=\"B5C98F73-1D7E-4205-AEF3-1485842EB191\"/>" + Environment.NewLine +
                "      <asset id=\"r3\" name=\"Live In Moscow MERGED-quicktime\" uid=\"E2951D8A4091478C718D981E70B29220\" projectRef=\"r2\" src=\"file://localhost/Volumes/Macintosh%20HD/Final%20Cut%20Events/Yma%20DVD/Original%20Media/Live%20In%20Moscow%20MERGED-quicktime.mov\" start=\"0s\" duration=\"128865737/30000s\" hasVideo=\"1\"/>" + Environment.NewLine +
                "      <format id=\"r4\" name=\"FFVideoFormatRateUndefined\" width=\"640\" height=\"480\"/>" + Environment.NewLine +
                "      <asset id=\"r5\" name=\"Moscow opening credit frame 2\" uid=\"492B77C679B1EEDA87E214703CD9B236\" projectRef=\"r2\" src=\"file://localhost/Volumes/Macintosh%20HD/Final%20Cut%20Events/Yma%20DVD/Original%20Media/Moscow%20opening%20credit%20frame%202.png\" start=\"0s\" duration=\"0s\" hasVideo=\"1\"/>" + Environment.NewLine +
                "      <effect id=\"r6\" name=\"Custom\" uid=\".../Titles.localized/Build In:Out.localized/Custom.localized/Custom.moti\"/>" + Environment.NewLine +
                "    </resources>" + Environment.NewLine +
                "    <sequence duration=\"10282752480/2400000s\" format=\"r1\" tcStart=\"0s\" tcFormat=\"NDF\" audioLayout=\"stereo\" audioRate=\"48k\">" + Environment.NewLine +
                "      <spine>" + Environment.NewLine +
                "      </spine>" + Environment.NewLine +
                "    </sequence>" + Environment.NewLine +
                "  </project>" + Environment.NewLine +
                "</fcpxml>";

            string xmlClipStructure =
                "  <video lane=\"6\" offset=\"4130126/60000s\" ref=\"r4\" name=\"Basic Subtitle\" duration=\"288288/60000s\" start=\"216003788/60000s\">" + Environment.NewLine +
                "    <param name=\"Text\" key=\"9999/999166889/999166904/2/369\" value=\"\"/>" + Environment.NewLine +
                "  </video>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            XmlNode videoNode = xml.DocumentElement.SelectSingleNode("project/sequence/spine");

            int number = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode clip = xml.CreateElement("clip");
                clip.InnerXml = xmlClipStructure;
                var attr = xml.CreateAttribute("name");
                attr.Value = title;
                clip.Attributes.Append(attr);

                attr = xml.CreateAttribute("duration");
                attr.Value = Convert.ToInt64(p.Duration.TotalSeconds * 2400000) + "/2400000s";
                clip.Attributes.Append(attr);

                attr = xml.CreateAttribute("start");
                attr.Value = Convert.ToInt64(p.StartTime.TotalSeconds * 2400000) + "/2400000s";
                clip.Attributes.Append(attr);

                attr = xml.CreateAttribute("audioStart");
                attr.Value = "0s";
                clip.Attributes.Append(attr);

                attr = xml.CreateAttribute("audioDuration");
                attr.Value = Convert.ToInt64(p.Duration.TotalSeconds * 2400000) + "/2400000s";
                clip.Attributes.Append(attr);

                attr = xml.CreateAttribute("tcFormat");
                attr.Value = "NDF";
                clip.Attributes.Append(attr);

                XmlNode titleNode = clip.SelectSingleNode("video");
                titleNode.Attributes["offset"].Value = Convert.ToInt64(p.StartTime.TotalSeconds * 60000) + "/60000s";
                titleNode.Attributes["name"].Value = HtmlUtil.RemoveHtmlTags(p.Text);
                titleNode.Attributes["duration"].Value = Convert.ToInt64(p.Duration.TotalSeconds * 60000) + "/60000s";
                titleNode.Attributes["start"].Value = Convert.ToInt64(p.StartTime.TotalSeconds * 60000) + "/60000s";

                XmlNode param = clip.SelectSingleNode("video/param");
                param.Attributes["value"].InnerText = HtmlUtil.RemoveHtmlTags(p.Text);

                videoNode.AppendChild(clip);
                number++;
            }

            string xmlAsText = ToUtf8XmlString(xml);
            xmlAsText = xmlAsText.Replace("fcpxml[]", "fcpxml");
            xmlAsText = xmlAsText.Replace("fcpxml []", "fcpxml");
            return xmlAsText;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            FrameRate = Configuration.Settings.General.CurrentFrameRate;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string x = sb.ToString();
            if (!x.Contains("<fcpxml version=\"1.3\"") && !x.Contains("<fcpxml version=\"1.2\""))
            {
                return;
            }

            var xml = new XmlDocument();
            try
            {
                xml.XmlResolver = null;
                xml.LoadXml(x.Trim());
                foreach (XmlNode node in xml.SelectNodes("//project/sequence/spine/clip/video/param[@name='Text']"))
                {
                    try
                    {
                        string text = node.Attributes["value"].InnerText;
                        Paragraph p = new Paragraph();
                        p.Text = text.Trim();
                        p.StartTime = DecodeTime(node.ParentNode.Attributes["offset"]);
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + DecodeTime(node.ParentNode.Attributes["duration"]).TotalMilliseconds;
                        subtitle.Paragraphs.Add(p);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                if (subtitle.Paragraphs.Count == 0)
                {
                    foreach (XmlNode node in xml.SelectNodes("//project/sequence/spine/clip/title/text"))
                    {
                        try
                        {
                            string text = node.ParentNode.InnerText;
                            Paragraph p = new Paragraph();
                            p.Text = text.Trim();
                            p.StartTime = DecodeTime(node.ParentNode.Attributes["offset"]);
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + DecodeTime(node.ParentNode.Attributes["duration"]).TotalMilliseconds;
                            bool add = true;
                            if (subtitle.Paragraphs.Count > 0)
                            {
                                var prev = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                                if (prev.Text == p.Text && prev.StartTime.TotalMilliseconds == p.StartTime.TotalMilliseconds)
                                {
                                    add = false;
                                }
                            }
                            if (add)
                            {
                                subtitle.Paragraphs.Add(p);
                            }
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
                subtitle.Renumber();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                _errorCount = 1;
                return;
            }
        }

        private static TimeCode DecodeTime(XmlAttribute duration)
        {
            // 220220/60000s
            if (duration != null)
            {
                var arr = duration.Value.TrimEnd('s').Split('/');
                if (arr.Length == 2)
                {
                    return TimeCode.FromSeconds(long.Parse(arr[0]) / double.Parse(arr[1]));
                }
                else if (arr.Length == 1)
                {
                    return TimeCode.FromSeconds(float.Parse(arr[0]));
                }
            }
            return new TimeCode();
        }

    }
}
