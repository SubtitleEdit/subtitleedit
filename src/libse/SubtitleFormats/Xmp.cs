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
            <xmpDM:frameRate>f25</xmpDM:frameRate>
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
            duration.InnerText = MillisecondsToFrames(paragraph.Duration.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
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
                        double start = FramesToMilliseconds(Convert.ToDouble(startTimeNode.InnerText, CultureInfo.InvariantCulture));
                        double end = start + FramesToMilliseconds(Convert.ToDouble(durationNode.InnerText, CultureInfo.InvariantCulture));
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
