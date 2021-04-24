using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    internal class Rdf1 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Rdf1";

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not implemented";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xmlAsText = sb.ToString().Trim();
            if (!xmlAsText.Contains("<rdf:"))
            {
                return;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(xmlAsText);
                var nameSpaceManager = new XmlNamespaceManager(xml.NameTable);
                nameSpaceManager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                nameSpaceManager.AddNamespace("xmpDM", "http://ns.adobe.com/xmp/1.0/DynamicMedia/");
                foreach (XmlNode node in xml.DocumentElement.SelectNodes("//rdf:Description", nameSpaceManager))
                {
                    try
                    {
                        if (node.Attributes == null)
                        {
                            continue;
                        }

                        var timeStartAttribute = node.Attributes["xmpDM:startTime"];
                        var timeDurationAttribute = node.Attributes["xmpDM:duration"];
                        var textNode = node.SelectSingleNode("xmpDM:name/rdf:Alt/rdf:li", nameSpaceManager);

                        if (timeStartAttribute == null || timeDurationAttribute == null || textNode == null)
                        {
                            continue;
                        }

                        var timeCodeIn = new TimeCode(Convert.ToDouble(timeStartAttribute.InnerText));
                        var timeCodeOut = new TimeCode(timeCodeIn.TotalMilliseconds + Convert.ToDouble(timeDurationAttribute.InnerText));
                        var text = textNode.InnerText.Trim();
                        if (text.StartsWith("{\\rtf1 "))
                        {
                            // not really RTF...
                            text = text
                                .Remove(0, 6)
                                .Replace("\\par", Environment.NewLine)
                                .Replace(Environment.NewLine + " ", Environment.NewLine)
                                .TrimEnd('}').Trim();
                        }
                        var p = new Paragraph(timeCodeIn, timeCodeOut, text);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        _errorCount++;
                    }
                }
                subtitle.Renumber();
            }
            catch (Exception)
            {
                _errorCount++;
            }
        }
    }
}
