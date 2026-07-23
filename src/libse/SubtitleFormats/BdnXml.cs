using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class BdnXml : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "BDN Xml";

        public override string ToText(Subtitle subtitle, string title)
        {
            var xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<Subtitle/>";

            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(xmlStructure);

            foreach (var p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("Paragraph");

                XmlNode number = xml.CreateElement("Number");
                number.InnerText = p.Number.ToString(CultureInfo.InvariantCulture);
                paragraph.AppendChild(number);

                XmlNode start = xml.CreateElement("StartMilliseconds");
                start.InnerText = p.StartTime.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
                paragraph.AppendChild(start);

                XmlNode end = xml.CreateElement("EndMilliseconds");
                end.InnerText = p.EndTime.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
                paragraph.AppendChild(end);

                XmlNode text = xml.CreateElement("Text");
                text.InnerText = HtmlUtil.RemoveHtmlTags(p.Text, true);
                paragraph.AppendChild(text);

                xml.DocumentElement.AppendChild(paragraph);
            }
            string textUtf8;
            using (var ms = new MemoryStream())
            {
                var writer = new XmlTextWriter(ms, Encoding.UTF8) { Formatting = Formatting.Indented };
                xml.Save(writer);
                textUtf8 = Encoding.UTF8.GetString(ms.ToArray());
            }
            return textUtf8.Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            var xmlString = sb.ToString();
            if (!xmlString.Contains("<BDN"))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(xmlString);
            }
            catch
            {
                _errorCount = 1;
                return;
            }

            char[] splitChars = { ':', ';' };

            var convertToSmpte = false;
            var frameRateElement = xml.DocumentElement.SelectSingleNode("Description/Format");
            if (frameRateElement?.Attributes["FrameRate"] != null)
            {
                if (decimal.TryParse(frameRateElement.Attributes["FrameRate"].InnerText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var fr))
                {
                    convertToSmpte = fr % 1 != 0;
                }
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Events/Event"))
            {
                try
                {
                    var start = node.Attributes["InTC"].InnerText;
                    var end = node.Attributes["OutTC"].InnerText;
                    var textBuilder = new StringBuilder();
                    var position = string.Empty;
                    foreach (XmlNode graphic in node.SelectNodes("Graphic"))
                    {
                        if (graphic.Attributes["X"] != null && graphic.Attributes["Y"] != null)
                        {
                            position = graphic.Attributes["X"].InnerText + "," + graphic.Attributes["Y"].InnerText;
                        }

                        textBuilder.AppendLine(graphic.InnerText);
                    }

                    var p = new Paragraph(textBuilder.ToString().Trim(), DecodeTimeCodeFrames(start, splitChars).TotalMilliseconds, DecodeTimeCodeFrames(end, splitChars).TotalMilliseconds);
                    if (node.Attributes["Forced"] != null && node.Attributes["Forced"].Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        p.Forced = true;
                    }

                    if (!string.IsNullOrEmpty(position))
                    {
                        p.Extra = position;
                    }

                    subtitle.Paragraphs.Add(p);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }

            if (convertToSmpte)
            {
                foreach (var p in subtitle.Paragraphs)
                {
                    p.StartTime.TotalMilliseconds *= 1.001;
                    p.EndTime.TotalMilliseconds *= 1.001;
                }
            }

            // keep the source xml around so callers can read the video size, see TryGetVideoSize
            subtitle.Header = xmlString;

            subtitle.Renumber();
        }

        /// <summary>
        /// Reads the video size from a BDN xml "Description/Format" node, e.g. VideoFormat="1080p" or VideoFormat="1920x1080".
        /// </summary>
        /// <param name="xmlString">Raw BDN xml - typically <see cref="Subtitle.Header"/> after a <see cref="LoadSubtitle"/> call.</param>
        public static bool TryGetVideoSize(string xmlString, out int width, out int height)
        {
            width = 0;
            height = 0;

            if (string.IsNullOrEmpty(xmlString) || !xmlString.Contains("<BDN"))
            {
                return false;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(xmlString);
                var videoFormat = xml.DocumentElement?.SelectSingleNode("Description/Format")?.Attributes?["VideoFormat"]?.InnerText;
                if (string.IsNullOrWhiteSpace(videoFormat))
                {
                    return false;
                }

                switch (videoFormat.Trim().ToLowerInvariant())
                {
                    case "480i":
                    case "480p":
                        width = 720;
                        height = 480;
                        return true;
                    case "576i":
                    case "576p":
                        width = 720;
                        height = 576;
                        return true;
                    case "720p":
                        width = 1280;
                        height = 720;
                        return true;
                    case "1080i":
                    case "1080p":
                        width = 1920;
                        height = 1080;
                        return true;
                    case "2160p":
                        width = 3840;
                        height = 2160;
                        return true;
                }

                // "1920x1080" style, as written by the Subtitle Edit BDN xml exporter for non-standard sizes
                var parts = videoFormat.Split('x', 'X');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var w) &&
                    int.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var h) &&
                    w > 0 && h > 0)
                {
                    width = w;
                    height = h;
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return false;
        }
    }
}
