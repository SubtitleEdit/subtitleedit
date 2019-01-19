using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class FcpXmlStyle
    {
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public string FontFace { get; set; }
        public Color FontColor { get; set; }
        public string Alignment { get; set; }
        public int Baseline { get; set; }
        public bool Italic { get; set; }
        public bool Bold { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public FcpXmlStyle()
        {
        }

        public FcpXmlStyle(FcpXmlStyle style)
        {
            FontName = style.FontName;
            FontSize = style.FontSize;
            FontFace = style.FontFace;
            FontColor = style.FontColor;
            Alignment = style.Alignment;
            Baseline = style.Baseline;
            Italic = style.Italic;
            Bold = style.Bold;
        }
    }

    public class FinalCutProXml15 : SubtitleFormat
    {
        internal string FcpXmlVersion { get; set; } = "1.5";

        public FinalCutProXml15()
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

        public override string Name => "Final Cut Pro Xml " + FcpXmlVersion;

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
                return "1/60s";
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
            if (Math.Abs(Configuration.Settings.General.CurrentFrameRate % 1.0) < 0.01)
            {
                return "NDF";
            }
            return "DF";
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + Environment.NewLine +
                "<fcpxml version=\"" + FcpXmlVersion + "\">" + Environment.NewLine +
                "   <resources>" + Environment.NewLine +
                "       <format height=\"[HEIGHT]\" width=\"[WIDTH]\" frameDuration=\"" + GetFrameDuration() + "\" id=\"r1\"/>" + Environment.NewLine +
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

            var xml = new XmlDocument();
            var sequenceDuration = 10;
            if (subtitle.Paragraphs.Count > 0)
            {
                sequenceDuration = (int)Math.Round(subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalSeconds);
            }
            xml.LoadXml(xmlStructure
                .Replace("[WIDTH]", DefaultStyle.Width.ToString(CultureInfo.InvariantCulture))
                .Replace("[HEIGHT]", DefaultStyle.Height.ToString(CultureInfo.InvariantCulture))
                .Replace("[SEQUENCE_DURATION]", sequenceDuration.ToString(CultureInfo.InvariantCulture)))
                ;
            XmlNode videoNode = xml.DocumentElement.SelectSingleNode("//project/sequence/spine");
            int number = 1;

            var sbTrimmedTitle = new StringBuilder();
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sbTrimmedTitle.Clear();
                sb.Clear();
                XmlNode video = xml.CreateElement("video");
                foreach (var ch in HtmlUtil.RemoveHtmlTags(p.Text, true))
                {
                    if (CharUtils.IsEnglishAlphabet(ch) || char.IsDigit(ch))
                    {
                        sbTrimmedTitle.Append(ch);
                    }
                }

                var styles = new List<FcpXmlStyle> { DefaultStyle };
                var text = Utilities.RemoveSsaTags(p.Text).Trim();
                var italicIndexesBefore = new Stack<int>();
                var boldIndexesBefore = new Stack<int>();
                var fontIndexesBefore = new Stack<int>();
                var styleTextPairs = new Dictionary<int, string>();
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];
                    if (ch != '<')
                    {
                        sb.Append(ch);
                        continue;
                    }

                    string subIText = text.Substring(i);

                    if (subIText.StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                    {
                        AddTextAndStyle(styles, sb, styleTextPairs);
                        italicIndexesBefore.Push(styles.Count - 1);
                        var newStyle = new FcpXmlStyle(styles[styles.Count - 1]) { Italic = true };
                        styles.Add(newStyle);
                        i += 2;
                    }
                    else if (subIText.StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                    {
                        AddTextAndStyle(styles, sb, styleTextPairs);
                        boldIndexesBefore.Push(styles.Count - 1);
                        var newStyle = new FcpXmlStyle(styles[styles.Count - 1]) { Bold = true };
                        styles.Add(newStyle);
                        i += 2;
                    }
                    else if (subIText.StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                    {
                        AddTextAndStyle(styles, sb, styleTextPairs);
                        fontIndexesBefore.Push(styles.Count - 1);
                        var s = text.Substring(i);
                        int end = s.IndexOf('>');
                        if (end > 0)
                        {
                            string f = s.Substring(0, end);
                            var colorStart = f.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);
                            if (colorStart >= 0)
                            {
                                int colorEnd = colorStart + " color=".Length + 1;
                                if (colorEnd < f.Length)
                                {
                                    colorEnd = f.IndexOf('"', colorEnd);
                                    if (colorEnd > 0 || colorEnd == -1)
                                    {
                                        if (colorEnd == -1)
                                        {
                                            s = f.Substring(colorStart);
                                        }
                                        else
                                        {
                                            s = f.Substring(colorStart, colorEnd - colorStart);
                                        }

                                        s = s.Remove(0, " color=".Length);
                                        s = s.Trim('"');
                                        s = s.Trim('\'');
                                        try
                                        {
                                            var fontColor = ColorTranslator.FromHtml(s);
                                            var newStyle = new FcpXmlStyle(styles[styles.Count - 1]);
                                            newStyle.FontColor = fontColor;
                                            styles.Add(newStyle);
                                        }
                                        catch
                                        {
                                            // just re-add last style
                                            styles.Add(new FcpXmlStyle(styles[styles.Count - 1]));
                                        }
                                    }
                                }
                            }
                            i += end;
                        }
                        else
                        {
                            i += text.Length;
                        }
                    }
                    else if (subIText.StartsWith("</i>", StringComparison.OrdinalIgnoreCase))
                    {
                        AddTextAndStyle(styles, sb, styleTextPairs);
                        var newStyle = new FcpXmlStyle(styles[styles.Count - 1]);
                        if (italicIndexesBefore.Count > 0)
                        {
                            var beforeIdx = italicIndexesBefore.Pop();
                            newStyle.Italic = styles[beforeIdx].Italic;
                        }
                        styles.Add(newStyle);
                        i += 3;
                    }
                    else if (subIText.StartsWith("</b>", StringComparison.OrdinalIgnoreCase))
                    {
                        AddTextAndStyle(styles, sb, styleTextPairs);
                        var newStyle = new FcpXmlStyle(styles[styles.Count - 1]);
                        if (boldIndexesBefore.Count > 0)
                        {
                            var beforeIdx = boldIndexesBefore.Pop();
                            newStyle.Bold = styles[beforeIdx].Bold;
                        }
                        styles.Add(newStyle);
                        i += 3;
                    }
                    else if (subIText.StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                    {
                        AddTextAndStyle(styles, sb, styleTextPairs);
                        var newStyle = new FcpXmlStyle(styles[styles.Count - 1]);
                        if (fontIndexesBefore.Count > 0)
                        {
                            var beforeIdx = fontIndexesBefore.Pop();
                            newStyle.FontColor = styles[beforeIdx].FontColor;
                            newStyle.FontSize = styles[beforeIdx].FontSize;
                        }
                        styles.Add(newStyle);
                        i += 6;
                    }
                }
                AddTextAndStyle(styles, sb, styleTextPairs);
                WriteCurrentTextSegment(styles, styleTextPairs, video, number++, sbTrimmedTitle.ToString(), xml);
                XmlNode generatorNode = video.SelectSingleNode("title");
                generatorNode.Attributes["offset"].Value = GetFrameTime(p.StartTime);
                generatorNode.Attributes["duration"].Value = GetFrameTime(p.Duration);
                generatorNode.Attributes["start"].Value = GetFrameTime(p.StartTime);
                videoNode.AppendChild(generatorNode);
            }
            return ToUtf8XmlString(xml);
        }

        private static void AddTextAndStyle(List<FcpXmlStyle> styles, StringBuilder sb, Dictionary<int, string> styleTextPairs)
        {
            if (sb.Length > 0)
            {
                if (styleTextPairs.ContainsKey(styles.Count - 1))
                {
                    styles.Add(styles[styles.Count - 1]);
                }

                styleTextPairs.Add(styles.Count - 1, sb.ToString());
                sb.Clear();
            }
        }

        private void WriteCurrentTextSegment(List<FcpXmlStyle> styles, Dictionary<int, string> styleTextPairs, XmlNode video, int number, string title, XmlDocument xml)
        {
            string xmlClipStructure =
                "<title name=\"Basic Title: [TITLEID]\" lane=\"1\" offset=\"8665300/2400s\" ref=\"r2\" duration=\"13400/2400s\" start=\"3600s\">" + Environment.NewLine +
                "    <param name=\"Position\" key=\"9999/999166631/999166633/1/100/101\" value=\"-1.67499 -470.934\"/>" + Environment.NewLine +
                "    <text>" + Environment.NewLine +
                //"        <text-style ref=\"ts[NUMBER]\">THE NOISEMAKER</text-style>" + Environment.NewLine +
                "    </text>" + Environment.NewLine +
                //"    <text-style-def id=\"ts[NUMBER]\">" + Environment.NewLine +
                //"        <text-style font=\"[FONT_NAME]\" fontSize=\"[FONT_SIZE]\" fontFace=\"[FONT_FACE]\" fontColor=\"[FONT_COLOR]\" baseline=\"[BASELINE]\" shadowColor=\"0 0 0 1\" shadowOffset=\"5 315\" alignment=\"center\"/>" + Environment.NewLine +
                //"    </text-style-def>" + Environment.NewLine +
                "</title>";

            var textStyleStructure = "<text-style font=\"[FONT_NAME]\" fontSize=\"[FONT_SIZE]\" fontFace=\"[FONT_FACE]\" fontColor=\"[FONT_COLOR]\" baseline=\"[BASELINE]\" shadowColor=\"0 0 0 1\" shadowOffset=\"5 315\" alignment=\"[ALIGNMENT]\" [ITALIC] [BOLD] />";

            string temp = xmlClipStructure.Replace("[TITLEID]", title);
            video.InnerXml = temp;

            var titleNode = video.SelectSingleNode("//title");
            var textNode = video.SelectSingleNode("//text");
            var styleCount = 1;
            foreach (var pair in styleTextPairs)
            {
                var id = "ts" + number + "-" + styleCount;
                XmlNode textStyleNode = xml.CreateElement("text-style");
                XmlAttribute refAttribute = xml.CreateAttribute("ref");
                refAttribute.InnerText = id;
                textStyleNode.Attributes.Append(refAttribute);
                textStyleNode.InnerText = pair.Value;
                textNode.AppendChild(textStyleNode);

                XmlNode styleNode = xml.CreateElement("text-style-def");
                XmlAttribute idAttribute = xml.CreateAttribute("id");
                idAttribute.InnerText = id;
                styleNode.Attributes.Append(idAttribute);
                var tempStr = textStyleStructure;
                if (styles[pair.Key].Italic)
                {
                    tempStr = tempStr.Replace("[ITALIC]", "italic=\"1\"");
                }
                else
                {
                    tempStr = tempStr.Replace(" [ITALIC]", string.Empty);
                }
                if (styles[pair.Key].Bold)
                {
                    tempStr = tempStr.Replace("[BOLD]", "bold=\"1\"");
                }
                else
                {
                    tempStr = tempStr.Replace(" [BOLD]", string.Empty);
                }
                styleNode.InnerXml = tempStr
                .Replace("[NUMBER]", number.ToString(CultureInfo.InvariantCulture) + "-" + styleCount)
                .Replace("[TITLEID]", title)
                .Replace("[FONT_NAME]", styles[pair.Key].FontName)
                .Replace("[FONT_SIZE]", styles[pair.Key].FontSize.ToString(CultureInfo.InvariantCulture))
                .Replace("[FONT_FACE]", styles[pair.Key].FontFace)
                .Replace("[FONT_COLOR]", ToColorString(styles[pair.Key].FontColor))
                .Replace("[ALIGNMENT]", styles[pair.Key].Alignment)
                .Replace("[BASELINE]", styles[pair.Key].Baseline.ToString(CultureInfo.InvariantCulture));
                titleNode.AppendChild(styleNode);
                styleCount++;
            }
        }

        private string ToColorString(Color fontColor)
        {
            //  0.793266 0.793391 0.793221 1

            var r = (double)fontColor.R / byte.MaxValue;
            var g = (double)fontColor.G / byte.MaxValue;
            var b = (double)fontColor.B / byte.MaxValue;
            var a = (double)fontColor.A / byte.MaxValue;
            var result = $"{r:0.######} {g:0.######} {b:0.######} {a:0.######}";
            return result;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            FrameRate = Configuration.Settings.General.CurrentFrameRate;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string x = sb.ToString();
            if (!x.Contains("<fcpxml version=\"" + FcpXmlVersion + "\">"))
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
                    if (textNodes.Count == 0)
                    {
                        textNodes = xml.SelectNodes("//title/text");
                    }
                    foreach (XmlNode node in textNodes)
                    {
                        try
                        {
                            string text = node.ParentNode.InnerText.Replace("\r\r", "\r");
                            var p = new Paragraph();
                            p.Text = text.Trim();
                            if (node.ParentNode.InnerXml.Contains("bold=\"1\""))
                            {
                                p.Text = "<b>" + p.Text + "</b>";
                            }

                            if (node.ParentNode.InnerXml.Contains("italic=\"1\""))
                            {
                                p.Text = "<i>" + p.Text + "</i>";
                            }

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