using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class FinalCutProXml14Text : SubtitleFormat
    {
        public double FrameRate { get; set; }

        public override string Extension => ".fcpxml";

        public override string Name => "Final Cut Pro Xml 1.4 Text";

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
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>" + Environment.NewLine +
                "<!DOCTYPE fcpxml>" + Environment.NewLine +
                Environment.NewLine +
                "<fcpxml version=\"1.4\">" + Environment.NewLine +
                "   <resources>" + Environment.NewLine +
                "       <format height=\"1080\" width=\"1440\" frameDuration=\"" + FinalCutProXml15.GetFrameDuration() + "\" id=\"r1\"/>" + Environment.NewLine +
                "       <effect id=\"r2\" uid=\".../Titles.localized/Bumper:Opener.localized/Basic Title.localized/Basic Title.moti\" name=\"Basic Title\"/>" + Environment.NewLine +
                "   </resources>" + Environment.NewLine +
                "   <library location=\"\">" + Environment.NewLine +
                "       <event name=\"Title\" uid=\"15A02C43-1B7A-4CF8-8007-5C266E77A91E\">" + Environment.NewLine +
                "           <project name=\"SUBTITLES\" uid=\"E04A4539-1369-47C8-AC44-F459A96AC90F\">" + Environment.NewLine +
                "               <sequence duration=\"929s\" format=\"r1\" tcStart=\"0s\" tcFormat=\"" + FinalCutProXml15.GetNdfDf() + "\" audioLayout=\"stereo\" audioRate=\"48k\">" + Environment.NewLine +
                "                   <spine>" + Environment.NewLine +
                "                       <gap name=\"Gap\" offset=\"0s\" duration=\"929s\" start=\"3600s\">" + Environment.NewLine +
                "                       </gap>" + Environment.NewLine +
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
            xml.LoadXml(xmlStructure);
            XmlNode videoNode = xml.DocumentElement.SelectSingleNode("//project/sequence/spine/gap");
            int number = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode video = xml.CreateElement("video");
                var trimmedTitle = new StringBuilder();
                foreach (var ch in HtmlUtil.RemoveHtmlTags(p.Text, true))
                {
                    if (CharUtils.IsEnglishAlphabet(ch) || char.IsDigit(ch))
                    {
                        trimmedTitle.Append(ch);
                    }
                }
                string temp = xmlClipStructure.Replace("[NUMBER]", number.ToString(CultureInfo.InvariantCulture)).Replace("[TITLEID]", trimmedTitle.ToString());
                video.InnerXml = temp;

                XmlNode generatorNode = video.SelectSingleNode("title");
                if (IsNearleWholeNumber(p.StartTime.TotalSeconds))
                {
                    generatorNode.Attributes["offset"].Value = Convert.ToInt64(p.StartTime.TotalSeconds) + "s";
                }
                else
                {
                    generatorNode.Attributes["offset"].Value = FinalCutProXml15.GetFrameTime(p.StartTime);
                }

                if (IsNearleWholeNumber(p.Duration.TotalSeconds))
                {
                    generatorNode.Attributes["duration"].Value = Convert.ToInt64(p.Duration.TotalSeconds) + "s";
                }
                else
                {
                    generatorNode.Attributes["duration"].Value = FinalCutProXml15.GetFrameTime(p.Duration);
                }

                if (IsNearleWholeNumber(p.StartTime.TotalSeconds))
                {
                    generatorNode.Attributes["start"].Value = Convert.ToInt64(p.StartTime.TotalSeconds) + "s";
                }
                else
                {
                    generatorNode.Attributes["start"].Value = FinalCutProXml15.GetFrameTime(p.StartTime);
                }

                XmlNode param = video.SelectSingleNode("title/text/text-style");
                param.InnerText = HtmlUtil.RemoveHtmlTags(p.Text);

                videoNode.AppendChild(generatorNode);
                number++;
            }

            string xmlAsText = ToUtf8XmlString(xml);
            xmlAsText = xmlAsText.Replace("fcpxml[]", "fcpxml");
            xmlAsText = xmlAsText.Replace("fcpxml []", "fcpxml");
            return xmlAsText;
        }

        private static bool IsNearleWholeNumber(double number)
        {
            double rest = number - Convert.ToInt64(number);
            return rest < 0.001;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            FrameRate = Configuration.Settings.General.CurrentFrameRate;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string x = sb.ToString();
            if (!x.Contains("<fcpxml version=\"1.4\">"))
            {
                return;
            }

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
                            string text = node.ParentNode.InnerText;
                            var p = new Paragraph();
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
            catch
            {
                _errorCount = 1;
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
                if (arr.Length == 1)
                {
                    return TimeCode.FromSeconds(float.Parse(arr[0]));
                }
            }
            return new TimeCode();
        }

    }
}
