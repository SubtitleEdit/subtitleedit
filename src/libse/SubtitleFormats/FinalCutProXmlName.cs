using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class FinalCutProXmlName : SubtitleFormat
    {
        public FinalCutProXmlName()
        {
            DefaultStyle = new FcpXmlStyle
            {
                FontName = "Lucida Sans",
                FontSize = 36,
                FontFace = "Regular",
                FontColor = Color.WhiteSmoke,
                Alignment = "center",
                Baseline = 29,
                Width = 1980,
                Height = 1024
            };
        }

        public FcpXmlStyle DefaultStyle { get; set; }

        public double FrameRate { get; set; }

        public override string Extension => ".fcpxml";

        public override string Name => "Final Cut Pro Xml Name";

        
        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + Environment.NewLine +
                "<fcpxml version=\"1.8\">" + Environment.NewLine +
                "   <resources>" + Environment.NewLine +
                "       <format height=\"[HEIGHT]\" width=\"[WIDTH]\" frameDuration=\"" + FinalCutProXml15.GetFrameDuration() + "\" id=\"r1\"/>" + Environment.NewLine +
                "       <effect id=\"r2\" uid=\".../Titles.localized/Bumper:Opener.localized/Basic Title.localized/Basic Title.moti\" name=\"Basic Title\"/>" + Environment.NewLine +
                "   </resources>" + Environment.NewLine +
                "   <library location=\"\">" + Environment.NewLine +
                "       <event name=\"Title\">" + Environment.NewLine +
                "           <project name=\"SUBTITLES\">" + Environment.NewLine +
                "               <sequence duration=\"[SEQUENCE_DURATION]s\" format=\"r1\" tcStart=\"0s\" tcFormat=\"" + FinalCutProXml15.GetNdfDf() + "\" audioLayout=\"stereo\" audioRate=\"48k\">" + Environment.NewLine +
                "                   <spine>" + Environment.NewLine +
                "                      <gap name=\"Gap\" offset=\"0s\" duration=\"[SEQUENCE_DURATION]s\" start=\"[FIRST_START]\">" + Environment.NewLine +
                "                      </gap>" + Environment.NewLine +
                "                    </spine>" + Environment.NewLine +
                "                </sequence>" + Environment.NewLine +
                "            </project>" + Environment.NewLine +
                "        </event>" + Environment.NewLine +
                "    </library>" + Environment.NewLine +
                "</fcpxml>";

            var xml = new XmlDocument();
            var sequenceDuration = 10;
            if (subtitle.Paragraphs.Count > 0)
            {
                sequenceDuration = (int)Math.Round(subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalSeconds);
            }

            var firstStart = "0s";
            if (subtitle.Paragraphs.Count > 0)
            {
                FinalCutProXml15.GetFrameTime(subtitle.Paragraphs[0].StartTime);
            }

            xml.LoadXml(xmlStructure
                .Replace("[WIDTH]", DefaultStyle.Width.ToString(CultureInfo.InvariantCulture))
                .Replace("[HEIGHT]", DefaultStyle.Height.ToString(CultureInfo.InvariantCulture))
                .Replace("[SEQUENCE_DURATIONSEQUENCE_DURATION]", sequenceDuration.ToString(CultureInfo.InvariantCulture))
                .Replace("[FIRST_START]", firstStart)
                );
            XmlNode gapNode = xml.DocumentElement.SelectSingleNode("//project/sequence/spine/gap");
            foreach (var p in subtitle.Paragraphs)
            {
                var text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                XmlNode video = xml.CreateElement("video");
                MakeTitleNodeWithText(video, text);
                XmlNode generatorNode = video.SelectSingleNode("title");
                generatorNode.Attributes["offset"].Value = FinalCutProXml15.GetFrameTime(p.StartTime);
                generatorNode.Attributes["duration"].Value = FinalCutProXml15.GetFrameTime(p.Duration);
                generatorNode.Attributes["start"].Value = FinalCutProXml15.GetFrameTime(p.StartTime);
                generatorNode.Attributes["name"].Value = text;
                gapNode.AppendChild(generatorNode);
            }
            return ToUtf8XmlString(xml);
        }

        private void MakeTitleNodeWithText(XmlNode video, string text)
        {
            var titleStructure = "<title name=\"\" lane=\"1\" offset=\"8665300/2400s\" ref=\"r2\" duration=\"13400/2400s\" start=\"3600s\" />";
            video.InnerXml = titleStructure;
            var titleNode = video.SelectSingleNode("title");
            titleNode.Attributes["name"].InnerText = text;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            FrameRate = Configuration.Settings.General.CurrentFrameRate;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var x = sb.ToString();
            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(x.Trim());

                if (subtitle.Paragraphs.Count == 0)
                {
                    var textNodes = xml.SelectNodes("//title/text");
                    if (textNodes.Count > 0)
                    {
                        return;
                    }

                    textNodes = xml.SelectNodes("//title");
                    foreach (XmlNode node in textNodes)
                    {
                        try
                        {
                            if (node.Attributes?["name"] == null ||
                                node.Attributes?["offset"] == null ||
                                node.Attributes?["duration"] == null)
                            {
                                continue;
                            }

                            var text = node.Attributes["name"].InnerText.Replace("\r\r", "\r");
                            var p = new Paragraph { Text = text.Trim() };
                            if (node.ParentNode.InnerXml.Contains("bold=\"1\""))
                            {
                                p.Text = "<b>" + p.Text + "</b>";
                            }

                            if (node.ParentNode.InnerXml.Contains("italic=\"1\""))
                            {
                                p.Text = "<i>" + p.Text + "</i>";
                            }

                            p.StartTime = FinalCutProXml15.DecodeTime(node.Attributes["offset"]);
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + FinalCutProXml15.DecodeTime(node.Attributes["duration"]).TotalMilliseconds;
                            var add = true;
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
    }
}