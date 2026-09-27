using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Image based subtitles as a list of timed images (e.g. from AVISubDetector):
    ///
    /// &lt;TimedImages targetWidth="720" targetHeight="576" aspectRatio="16:9"&gt;
    /// &lt;I s="0.600" e="3.720" x="268" y="458" w="218" h="58" i="AYZ-1.png" /&gt;
    /// &lt;/TimedImages&gt;
    ///
    /// Times are in seconds, "i" is the image file next to the xml. Loaded like BDN xml: the
    /// text of each subtitle is its image file name, "x,y" goes in Extra and the xml in Header,
    /// so the images can be OCR'ed.
    /// </summary>
    public class TimedImagesXml : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Timed Images XML";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var xmlString = JoinLines(lines);
            if (!xmlString.Contains("<TimedImages") || !xmlString.Contains(" i=\""))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();

            var xmlString = JoinLines(lines);
            if (!xmlString.Contains("<TimedImages"))
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

            if (xml.DocumentElement == null || xml.DocumentElement.Name != "TimedImages")
            {
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("I"))
            {
                var image = node.Attributes?["i"]?.Value;
                if (string.IsNullOrWhiteSpace(image) ||
                    !TryGetSeconds(node, "s", out var start) ||
                    !TryGetSeconds(node, "e", out var end))
                {
                    _errorCount++;
                    continue;
                }

                var p = new Paragraph(image.Trim(), start * TimeCode.BaseUnit, end * TimeCode.BaseUnit);
                if (TryGetInt(node, "x", out var x) && TryGetInt(node, "y", out var y))
                {
                    p.Extra = x.ToString(CultureInfo.InvariantCulture) + "," + y.ToString(CultureInfo.InvariantCulture);
                }

                subtitle.Paragraphs.Add(p);
            }

            subtitle.Header = xmlString;
            subtitle.Renumber();
        }

        /// <summary>
        /// The "targetWidth"/"targetHeight" of the video the images were made for, from the xml
        /// that <see cref="LoadSubtitle"/> puts in the subtitle header.
        /// </summary>
        public static bool TryGetVideoSize(string xmlString, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (string.IsNullOrEmpty(xmlString) || !xmlString.Contains("<TimedImages"))
            {
                return false;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(xmlString);
                var root = xml.DocumentElement;
                return root != null &&
                       int.TryParse(root.Attributes?["targetWidth"]?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out width) &&
                       int.TryParse(root.Attributes?["targetHeight"]?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out height) &&
                       width > 0 && height > 0;
            }
            catch (XmlException)
            {
                return false;
            }
        }

        private static bool TryGetSeconds(XmlNode node, string attributeName, out double seconds)
        {
            return double.TryParse(node.Attributes?[attributeName]?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds);
        }

        private static bool TryGetInt(XmlNode node, string attributeName, out int value)
        {
            return int.TryParse(node.Attributes?[attributeName]?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }
    }
}
