using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Xmp : SubtitleFormat
    {
        public override string Extension => ".xmp";

        public override string Name => "XMP";

        private static string NamespaceMeta => "adobe:ns:meta/";
        private static string NamespaceRdf => "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        private static string NamespaceDescription => "http://ns.adobe.com/xmp/1.0/DynamicMedia/";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure = @"<?xml version='1.0' encoding='utf-8'?>
<x:xmpmeta xmlns:x='adobe:ns:meta/'>
  <rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>
    <rdf:Description xmlns:xmpDM='http://ns.adobe.com/xmp/1.0/DynamicMedia/'>
      <xmpDM:Tracks>
        <rdf:Bag>
          <rdf:li rdf:parseType='Resource'>
            <xmpDM:frameRate>" + GetFrameRateString() + @"</xmpDM:frameRate>
            <xmpDM:markers>
              <rdf:Seq>
              </rdf:Seq>
            </xmpDM:markers>
            <xmpDM:trackName>Comment</xmpDM:trackName>
            <xmpDM:trackType>Comment</xmpDM:trackType>
          </rdf:li>
        </rdf:Bag>
      </xmpDM:Tracks>
    </rdf:Description>
  </rdf:RDF>
</x:xmpmeta>".Replace("'", "\"");

            var xml = new XmlDocument { XmlResolver = null };
            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("x", NamespaceMeta);
            namespaceManager.AddNamespace("rdf", NamespaceRdf);
            namespaceManager.AddNamespace("xmpDM", NamespaceDescription);
            xml.LoadXml(xmlStructure);
            XmlNode root = xml.DocumentElement.SelectSingleNode("rdf:RDF/rdf:Description/xmpDM:Tracks/rdf:Bag/rdf:li/xmpDM:markers/rdf:Seq", namespaceManager);
            foreach (var p in subtitle.Paragraphs)
            {
                XmlNode paragraph = CreateParagraphElement(xml, p);
                root.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        /// <summary>
        /// xmpDM:frameRate for the marker times we write: "f25" style for integer rates,
        /// "f24000s1001" style for NTSC rates. The old header hardcoded f25 while the
        /// marker times were written at the current frame rate, so any other rate gave
        /// Adobe apps wrong times.
        /// </summary>
        private static string GetFrameRateString()
        {
            var rate = Configuration.Settings.General.CurrentFrameRate;
            if (Math.Abs(rate - 23.976) < 0.01)
            {
                return "f24000s1001";
            }

            if (Math.Abs(rate - 29.97) < 0.01)
            {
                return "f30000s1001";
            }

            if (Math.Abs(rate - 59.94) < 0.01)
            {
                return "f60000s1001";
            }

            return "f" + Math.Round(rate).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses xmpDM:frameRate values like "f25", "f24000s1001" or Premiere's tick-based
        /// "f254016000000". Returns false when absent/unparseable.
        /// </summary>
        public static bool TryParseFrameRate(string input, out double numerator, out double denominator)
        {
            numerator = 0;
            denominator = 1;
            if (string.IsNullOrEmpty(input) || input.Length < 2 || input[0] != 'f')
            {
                return false;
            }

            var arr = input.Substring(1).Split('s');
            if (arr.Length > 2 || !long.TryParse(arr[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var num) || num <= 0)
            {
                return false;
            }

            long den = 1;
            if (arr.Length == 2 && (!long.TryParse(arr[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out den) || den <= 0))
            {
                return false;
            }

            numerator = num;
            denominator = den;
            return true;
        }

        private XmlNode CreateParagraphElement(XmlDocument xml, Paragraph paragraph)
        {
            var li = xml.CreateElement("rdf", "li", NamespaceRdf);
            var parseType = xml.CreateAttribute("rdf", "parseType", NamespaceRdf);
            parseType.InnerText = "Resource";
            li.Attributes.Append(parseType);

            var comment = xml.CreateElement("xmpDM", "comment", NamespaceDescription);
            comment.InnerText = paragraph.Text;
            li.AppendChild(comment);

            var duration = xml.CreateElement("xmpDM", "duration", NamespaceDescription);
            duration.InnerText = MillisecondsToFrames(paragraph.DurationTotalMilliseconds).ToString(CultureInfo.InvariantCulture);
            li.AppendChild(duration);

            var startTime = xml.CreateElement("xmpDM", "startTime", NamespaceDescription);
            startTime.InnerText = MillisecondsToFrames(paragraph.StartTime.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
            li.AppendChild(startTime);

            return li;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string allText = sb.ToString();
            if (!allText.Contains("<x:xmpmeta"))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(allText);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                _errorCount = 1;
                return;
            }

            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("x", NamespaceMeta);
            namespaceManager.AddNamespace("rdf", NamespaceRdf);
            namespaceManager.AddNamespace("xmpDM", NamespaceDescription);
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//rdf:li", namespaceManager))
            {
                try
                {
                    var startTimeNode = node.SelectSingleNode("xmpDM:startTime", namespaceManager);
                    var durationNode = node.SelectSingleNode("xmpDM:duration", namespaceManager);
                    var textNode = node.SelectSingleNode("xmpDM:comment", namespaceManager);
                    if (startTimeNode != null && durationNode != null && textNode != null)
                    {
                        var startValue = Convert.ToDouble(startTimeNode.InnerText, CultureInfo.InvariantCulture);
                        var durationValue = Convert.ToDouble(durationNode.InnerText, CultureInfo.InvariantCulture);

                        // Honor the track's declared frame rate - Premiere writes marker
                        // times as ticks with frameRate "f254016000000", which read as
                        // frame numbers would be off by ten orders of magnitude.
                        double start;
                        double end;
                        var frameRateNode = node.SelectSingleNode("ancestor::rdf:li/xmpDM:frameRate", namespaceManager);
                        if (frameRateNode != null && TryParseFrameRate(frameRateNode.InnerText, out var numerator, out var denominator))
                        {
                            start = startValue * 1000.0 * denominator / numerator;
                            end = start + durationValue * 1000.0 * denominator / numerator;
                        }
                        else
                        {
                            start = FramesToMilliseconds(startValue);
                            end = start + FramesToMilliseconds(durationValue);
                        }

                        string text = textNode.InnerText;
                        subtitle.Paragraphs.Add(new Paragraph(text, start, end));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }
    }
}
