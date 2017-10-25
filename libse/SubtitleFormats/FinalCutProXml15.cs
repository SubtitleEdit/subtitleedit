using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class FinalCutProXml15 : SubtitleFormat
    {
        public double FrameRate { get; set; }

        public override string Extension => ".fcpxml";

        public override string Name => "Final Cut Pro Xml 1.5";

        internal static string GetFrameDuration()
        {
            if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 23.976) < 0.01)
            {
                return "1001/24000s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 24) < 0.01)
            {
                return "1/24s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 25) < 0.01)
            {
                return "1/25s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 29.97) < 0.01)
            {
                return "1001/30000s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 30) < 0.01)
            {
                return "1/30s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 50) < 0.01)
            {
                return "1/50s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 59.94) < 0.01)
            {
                return "1001/60000s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 60) < 0.01)
            {
                return "10/60s";
            }
            return "1/25s";
        }

        internal static string GetFrameTime(TimeCode timeCode)
        {
            if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 23.976) < 0.01)
            {
                return Convert.ToInt64(timeCode.TotalSeconds * 2400000) + "/2400000s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 24) < 0.01)
            {
                return Convert.ToInt64(timeCode.TotalSeconds * 2400000) + "/2400000s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 25) < 0.01)
            {
                return Convert.ToInt64(timeCode.TotalSeconds * 2500000) + "/2500000s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 29.97) < 0.01)
            {
                return Convert.ToInt64(timeCode.TotalSeconds * 3000000) + "/3000000s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 30) < 0.01)
            {
                return Convert.ToInt64(timeCode.TotalSeconds * 3000000) + "/3000000s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 50) < 0.01)
            {
                return Convert.ToInt64(timeCode.TotalSeconds * 5000000) + "/5000000s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 59.94) < 0.01)
            {
                return Convert.ToInt64(timeCode.TotalSeconds * 6000000) + "/6000000s";
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 60) < 0.01)
            {
                return Convert.ToInt64(timeCode.TotalSeconds * 6000000) + "/6000000s";
            }
            return Convert.ToInt64(timeCode.TotalSeconds * 2500000) + "/2500000s";
        }

        internal static string GetNdfDf()
        {
            if (Math.Abs(Configuration.Settings.General.CurrentFrameRate % 0.0) < 0.01)
            {
                return "NDF";
            }
            return "DF";
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + Environment.NewLine +
                "<fcpxml version=\"1.5\">" + Environment.NewLine +
                "   <resources>" + Environment.NewLine +
                "       <format height=\"1080\" width=\"1920\" frameDuration=\"" + GetFrameDuration() + "\" id=\"r1\"/>" + Environment.NewLine +
                "       <effect id=\"r2\" uid=\".../Titles.localized/Bumper:Opener.localized/Basic Title.localized/Basic Title.moti\" name=\"Basic Title\"/>" + Environment.NewLine +
                "   </resources>" + Environment.NewLine +
                "   <library location=\"\">" + Environment.NewLine +
                "       <event name=\"Title\">" + Environment.NewLine +
                "           <project name=\"SUBTITLES\">" + Environment.NewLine +
                "               <sequence duration=\"[SEQUENCE_DURATION]s\" format=\"r1\" tcStart=\"0s\" tcFormat=\"" + GetNdfDf() + "\" audioLayout=\"stereo\" audioRate=\"48k\">" + Environment.NewLine +
                "                   <spine>" + Environment.NewLine +
                "                    </spine>" + Environment.NewLine +
                "                </sequence>" + Environment.NewLine +
                "            </project>" + Environment.NewLine +
                "        </event>" + Environment.NewLine +
                "    </library>" + Environment.NewLine +
                "</fcpxml>";

            string xmlClipStructure =
                "<title name=\"Basic Title: [TITLEID]\" lane=\"1\" offset=\"8665300/2400s\" ref=\"r2\" duration=\"13400/2400s\" start=\"3600s\">" + Environment.NewLine +
                "    <param name=\"Position\" key=\"9999/999166631/999166633/1/100/101\" value=\"-1.67499 -470.934\"/>" + Environment.NewLine +
                "    <text>" + Environment.NewLine +
                "        <text-style ref=\"ts[NUMBER]\">THE NOISEMAKER</text-style>" + Environment.NewLine +
                "    </text>" + Environment.NewLine +
                "    <text-style-def id=\"ts[NUMBER]\">" + Environment.NewLine +
                "        <text-style font=\"Lucida Grande\" fontSize=\"36\" fontFace=\"Regular\" fontColor=\"0.793266 0.793391 0.793221 1\" baseline=\"29\" shadowColor=\"0 0 0 1\" shadowOffset=\"5 315\" alignment=\"center\"/>" + Environment.NewLine +
                "    </text-style-def>" + Environment.NewLine +
                "</title>";

            var xml = new XmlDocument();
            var sequenceDuration = 10;
            if (subtitle.Paragraphs.Count > 0)
            {
                sequenceDuration = (int)Math.Round(subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalSeconds);
            }
            xml.LoadXml(xmlStructure.Replace("[SEQUENCE_DURATION]", sequenceDuration.ToString(CultureInfo.InvariantCulture)));
            XmlNode videoNode = xml.DocumentElement.SelectSingleNode("//project/sequence/spine");
            int number = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode video = xml.CreateElement("video");
                var trimmedTitle = new StringBuilder();
                foreach (var ch in HtmlUtil.RemoveHtmlTags(p.Text, true))
                {
                    if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Contains(ch.ToString(CultureInfo.InvariantCulture)))
                        trimmedTitle.Append(ch.ToString(CultureInfo.InvariantCulture));
                }

                string temp = xmlClipStructure.Replace("[NUMBER]", number.ToString(CultureInfo.InvariantCulture)).Replace("[TITLEID]", trimmedTitle.ToString());
                video.InnerXml = temp;
                var text = Utilities.RemoveSsaTags(p.Text).Replace("<b>", string.Empty).Replace("</b>", string.Empty).Trim();
                var italic = text.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) && text.EndsWith("</i>", StringComparison.OrdinalIgnoreCase);
                if (italic)
                {
                    XmlAttribute fontItalic = xml.CreateAttribute("italic");
                    fontItalic.InnerText = "1";
                    video.SelectSingleNode("title").SelectSingleNode("text-style-def").SelectSingleNode("text-style").Attributes.Append(fontItalic);
                }
                text = Utilities.RemoveSsaTags(p.Text).Replace("<i>", string.Empty).Replace("</i>", string.Empty).Trim();
                var bold = text.StartsWith("<b>", StringComparison.OrdinalIgnoreCase) && text.EndsWith("</b>", StringComparison.OrdinalIgnoreCase);
                if (bold)
                {
                    XmlAttribute fontBold = xml.CreateAttribute("bold");
                    fontBold.InnerText = "1";
                    video.SelectSingleNode("title").SelectSingleNode("text-style-def").SelectSingleNode("text-style").Attributes.Append(fontBold);
                }

                XmlNode generatorNode = video.SelectSingleNode("title");
                generatorNode.Attributes["offset"].Value = GetFrameTime(p.StartTime);
                generatorNode.Attributes["duration"].Value = GetFrameTime(p.Duration);
                generatorNode.Attributes["start"].Value = GetFrameTime(p.StartTime);

                XmlNode param = video.SelectSingleNode("title/text/text-style");
                param.InnerText = HtmlUtil.RemoveHtmlTags(p.Text, true);

                videoNode.AppendChild(generatorNode);
                number++;
            }
            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            FrameRate = Configuration.Settings.General.CurrentFrameRate;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string x = sb.ToString();
            if (!x.Contains("<fcpxml version=\"1.5\">"))
                return;

            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(x.Trim());

                if (subtitle.Paragraphs.Count == 0)
                {
                    var textNodes = xml.SelectNodes("//project/sequence/spine/title/text");
                    if (textNodes.Count == 0)
                    {
                        textNodes = xml.SelectNodes("//project/sequence/spine/gap/title/text");
                    }
                    foreach (XmlNode node in textNodes)
                    {
                        try
                        {
                            string text = node.ParentNode.InnerText.Replace("\r\r", "\r");
                            var p = new Paragraph();
                            p.Text = text.Trim();
                            if (node.ParentNode.InnerXml.Contains("bold=\"1\""))
                                p.Text = "<b>" + p.Text + "</b>";
                            if (node.ParentNode.InnerXml.Contains("italic=\"1\""))
                                p.Text = "<i>" + p.Text + "</i>";
                            p.StartTime = DecodeTime(node.ParentNode.Attributes["offset"]);
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + DecodeTime(node.ParentNode.Attributes["duration"]).TotalMilliseconds;
                            bool add = true;
                            if (subtitle.Paragraphs.Count > 0)
                            {
                                var prev = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                                if (prev.Text == p.Text && prev.StartTime.TotalMilliseconds == p.StartTime.TotalMilliseconds)
                                    add = false;
                            }
                            if (add)
                                subtitle.Paragraphs.Add(p);
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
                subtitle.Renumber();
            }
            catch
            {
                _errorCount = 1;
            }
        }

        private static TimeCode DecodeTime(XmlAttribute duration)
        {
            // e.g. 220220/60000s
            if (duration != null)
            {
                var dur = duration.Value;
                if (dur.EndsWith("ms"))
                {
                    var arr = duration.Value.TrimEnd('s').TrimEnd('m').Split('/');
                    if (arr.Length == 2)
                    {
                        return TimeCode.FromSeconds((long.Parse(arr[0]) * 1000.0) / (double.Parse(arr[1]) * 1000.0));
                    }
                    if (arr.Length == 1)
                    {
                        return TimeCode.FromSeconds(float.Parse(arr[0]) * 1000.0);
                    }
                }
                else
                {
                    var arr = duration.Value.TrimEnd('s').Split('/');
                    if (arr.Length == 2)
                    {
                        return TimeCode.FromSeconds(long.Parse(arr[0]) / double.Parse(arr[1]));
                    }
                    if (arr.Length == 1)
                    {
                        return TimeCode.FromSeconds(float.Parse(arr[0]));
                    }
                }
            }
            return new TimeCode();
        }

    }
}
