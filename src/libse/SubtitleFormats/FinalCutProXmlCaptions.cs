using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Writes fcpxml with caption elements (iTT role) - the native caption workflow Final
    /// Cut Pro has used since 10.4, so File > Import > XML brings the subtitles in as real
    /// captions instead of titles. Written as fcpxml 1.8 (the first version with captions);
    /// every later Final Cut Pro imports older fcpxml versions.
    /// </summary>
    public class FinalCutProXmlCaptions : SubtitleFormat
    {
        public override string Extension => ".fcpxml";

        public override string Name => "Final Cut Pro Xml Captions";

        public override string ToText(Subtitle subtitle, string title)
        {
            var frameDuration = FinalCutProXml15.GetFrameDuration(); // e.g. "1001/24000s"
            var arr = frameDuration.TrimEnd('s').Split('/');
            var frameNumerator = long.Parse(arr[0]);
            var frameDenominator = arr.Length == 2 ? long.Parse(arr[1]) : 1;

            string FrameAlignedTime(double milliseconds)
            {
                var frames = (long)Math.Round(milliseconds * frameDenominator / (1000.0 * frameNumerator));
                if (frames <= 0)
                {
                    return "0s";
                }

                return $"{frames * frameNumerator}/{frameDenominator}s";
            }

            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            if (string.IsNullOrEmpty(language))
            {
                language = "en";
            }

            var totalMs = subtitle.Paragraphs.Count > 0 ? subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds : 0;
            var sequenceDuration = FrameAlignedTime(Math.Max(totalMs, 1000));

            var xml = new XmlDocument();
            xml.LoadXml(
                "<fcpxml version=\"1.8\">" +
                "  <resources>" +
                "    <format id=\"r1\" frameDuration=\"" + frameDuration + "\" width=\"1920\" height=\"1080\"/>" +
                "  </resources>" +
                "  <library>" +
                "    <event name=\"Subtitle Edit\">" +
                "      <project name=\"[TITLE]\">" +
                "        <sequence format=\"r1\" duration=\"" + sequenceDuration + "\" tcStart=\"0s\" tcFormat=\"NDF\">" +
                "          <spine>" +
                "            <gap name=\"Gap\" offset=\"0s\" start=\"0s\" duration=\"" + sequenceDuration + "\"/>" +
                "          </spine>" +
                "        </sequence>" +
                "      </project>" +
                "    </event>" +
                "  </library>" +
                "</fcpxml>");

            var formatName = GetFcpFormatName();
            if (formatName != null)
            {
                var formatAttribute = xml.CreateAttribute("name");
                formatAttribute.Value = formatName;
                var formatNode = xml.SelectSingleNode("fcpxml/resources/format");
                formatNode.Attributes.InsertBefore(formatAttribute, formatNode.Attributes["frameDuration"]);
            }

            xml.SelectSingleNode("fcpxml/library/event/project").Attributes["name"].Value =
                string.IsNullOrWhiteSpace(title) ? "Subtitle Edit subtitle" : title;

            var gapNode = xml.SelectSingleNode("fcpxml/library/event/project/sequence/spine/gap");
            var styleCount = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                gapNode.AppendChild(MakeCaptionNode(xml, p, language, FrameAlignedTime, ref styleCount));
            }

            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
                   "<!DOCTYPE fcpxml>" + Environment.NewLine +
                   ToUtf8XmlString(xml, omitXmlDeclaration: true);
        }

        private static XmlNode MakeCaptionNode(XmlDocument xml, Paragraph p, string language, Func<double, string> frameAlignedTime, ref int styleCount)
        {
            var caption = xml.CreateElement("caption");
            SetAttribute(xml, caption, "lane", "1");
            SetAttribute(xml, caption, "offset", frameAlignedTime(p.StartTime.TotalMilliseconds));
            SetAttribute(xml, caption, "name", HtmlUtil.RemoveHtmlTags(p.Text, true).Replace(Environment.NewLine, " "));
            SetAttribute(xml, caption, "duration", frameAlignedTime(p.DurationTotalMilliseconds));
            SetAttribute(xml, caption, "role", $"iTT?captionFormat=ITT.{language}");

            var text = p.Text;
            var placementTop = text.StartsWith("{\\an7}", StringComparison.Ordinal) ||
                               text.StartsWith("{\\an8}", StringComparison.Ordinal) ||
                               text.StartsWith("{\\an9}", StringComparison.Ordinal);
            text = Utilities.RemoveSsaTags(text);
            text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagFont, HtmlUtil.TagUnderline);

            var textNode = xml.CreateElement("text");
            SetAttribute(xml, textNode, "placement", placementTop ? "top" : "bottom");
            caption.AppendChild(textNode);

            // FCP encodes line breaks as literal newlines inside the run content, and
            // styling as separate text-style runs referencing per-style defs.
            var styleDefs = new List<XmlElement>();
            var runStyleIds = new Dictionary<(bool Italic, bool Bold), string>();
            foreach (var run in SplitStyledRuns(text))
            {
                if (!runStyleIds.TryGetValue((run.Italic, run.Bold), out var styleId))
                {
                    styleCount++;
                    styleId = "ts" + styleCount.ToString(CultureInfo.InvariantCulture);
                    runStyleIds.Add((run.Italic, run.Bold), styleId);

                    var styleDef = xml.CreateElement("text-style-def");
                    SetAttribute(xml, styleDef, "id", styleId);
                    var style = xml.CreateElement("text-style");
                    SetAttribute(xml, style, "font", ".AppleSystemUIFont");
                    SetAttribute(xml, style, "fontSize", "13");
                    SetAttribute(xml, style, "fontFace", "Regular");
                    SetAttribute(xml, style, "fontColor", "1 1 1 1");
                    SetAttribute(xml, style, "backgroundColor", "0 0 0 1");
                    if (run.Italic)
                    {
                        SetAttribute(xml, style, "italic", "1");
                    }

                    if (run.Bold)
                    {
                        SetAttribute(xml, style, "bold", "1");
                    }

                    styleDef.AppendChild(style);
                    styleDefs.Add(styleDef);
                }

                var runNode = xml.CreateElement("text-style");
                SetAttribute(xml, runNode, "ref", styleId);
                runNode.InnerText = run.Text.Replace(Environment.NewLine, "\n");
                textNode.AppendChild(runNode);
            }

            foreach (var styleDef in styleDefs)
            {
                caption.AppendChild(styleDef);
            }

            return caption;
        }

        private static void SetAttribute(XmlDocument xml, XmlElement element, string name, string value)
        {
            var attribute = xml.CreateAttribute(name);
            attribute.Value = value;
            element.Attributes.Append(attribute);
        }

        private readonly struct StyledRun
        {
            public StyledRun(string text, bool italic, bool bold)
            {
                Text = text;
                Italic = italic;
                Bold = bold;
            }

            public string Text { get; }
            public bool Italic { get; }
            public bool Bold { get; }
        }

        private static IEnumerable<StyledRun> SplitStyledRuns(string input)
        {
            var runs = new List<StyledRun>();
            var sb = new StringBuilder();
            var italic = false;
            var bold = false;
            var i = 0;

            void FlushRun()
            {
                if (sb.Length > 0)
                {
                    runs.Add(new StyledRun(sb.ToString(), italic, bold));
                    sb.Clear();
                }
            }

            while (i < input.Length)
            {
                if (input[i] == '<')
                {
                    var toggled = true;
                    if (input.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                    {
                        FlushRun();
                        italic = true;
                        i += 3;
                    }
                    else if (input.Substring(i).StartsWith("</i>", StringComparison.OrdinalIgnoreCase))
                    {
                        FlushRun();
                        italic = false;
                        i += 4;
                    }
                    else if (input.Substring(i).StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                    {
                        FlushRun();
                        bold = true;
                        i += 3;
                    }
                    else if (input.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase))
                    {
                        FlushRun();
                        bold = false;
                        i += 4;
                    }
                    else
                    {
                        toggled = false;
                    }

                    if (toggled)
                    {
                        continue;
                    }
                }

                sb.Append(input[i]);
                i++;
            }

            FlushRun();
            if (runs.Count == 0)
            {
                runs.Add(new StyledRun(string.Empty, false, false));
            }

            return runs;
        }

        private static string GetFcpFormatName()
        {
            var rate = Configuration.Settings.General.CurrentFrameRate;
            if (Math.Abs(rate - 23.976) < 0.01)
            {
                return "FFVideoFormat1080p2398";
            }

            if (Math.Abs(rate - 24) < 0.01)
            {
                return "FFVideoFormat1080p24";
            }

            if (Math.Abs(rate - 25) < 0.01)
            {
                return "FFVideoFormat1080p25";
            }

            if (Math.Abs(rate - 29.97) < 0.01)
            {
                return "FFVideoFormat1080p2997";
            }

            if (Math.Abs(rate - 30) < 0.01)
            {
                return "FFVideoFormat1080p30";
            }

            if (Math.Abs(rate - 50) < 0.01)
            {
                return "FFVideoFormat1080p50";
            }

            if (Math.Abs(rate - 59.94) < 0.01)
            {
                return "FFVideoFormat1080p5994";
            }

            if (Math.Abs(rate - 60) < 0.01)
            {
                return "FFVideoFormat1080p60";
            }

            return null; // format name is optional; frameDuration alone is valid
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var x = sb.ToString();
            if (!x.Contains("<fcpxml version="))
            {
                return;
            }

            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(x.Trim());
                _errorCount = FinalCutProXml15.LoadCaptionElements(subtitle, xml);
                subtitle.Renumber();
            }
            catch
            {
                _errorCount = 1;
            }
        }
    }
}
