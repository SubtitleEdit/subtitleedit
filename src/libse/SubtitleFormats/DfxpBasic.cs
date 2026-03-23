using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class DfxpBasic : TimedText10
    {
        public override string Extension => ".dfxp";

        public new const string NameOfFormat = "DFXP Basic";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !(fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var text = sb.ToString();
            if (!text.Contains("<style xml:id=\"basic\"") || !text.Contains("<body style=\"basic\">"))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var xml = new XmlDocument { XmlResolver = null };
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            nsmgr.AddNamespace("ttp", "http://www.w3.org/ns/10/ttml#parameter");
            nsmgr.AddNamespace("tts", "http://www.w3.org/ns/10/ttml#style");
            nsmgr.AddNamespace("ttm", "http://www.w3.org/ns/10/ttml#metadata");

            string xmlStructure = @"<?xml version='1.0' encoding='utf-8'?>
<tt xml:lang='en' xmlns:tts='http://www.w3.org/ns/ttml#styling' xmlns:ttm='http://www.w3.org/ns/ttml#metadata' xmlns='http://www.w3.org/ns/ttml'>
  <head>
    <metadata>
      <ttm:title>untitled.dfxp</ttm:title>
    </metadata>
    <styling>
      <style xml:id='basic' tts:textAlign='center' />
    </styling>
  </head>
  <body style='basic'>
    <div>
    </div>
  </body>
</tt>".Replace("'", "\"");

            var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            xmlStructure = xmlStructure.Replace("\"en\"", "\"" + languageCode + "\"");

            xml.LoadXml(xmlStructure);
            if (!string.IsNullOrWhiteSpace(title))
            {
                var headNode = xml.DocumentElement.SelectSingleNode("//ttml:head", nsmgr);
                var metadataNode = headNode?.SelectSingleNode("ttml:metadata", nsmgr);
                var titleNode = metadataNode?.FirstChild;
                if (titleNode != null)
                {
                    titleNode.InnerText = title + ".dfxp";
                }
            }

            var div = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).SelectSingleNode("ttml:div", nsmgr);
            foreach (var p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/ns/ttml");
                string text = p.Text;

                XmlAttribute start = xml.CreateAttribute("begin");
                start.InnerText = string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:000}", p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds);
                paragraph.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("end");
                end.InnerText = string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:000}", p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds);
                paragraph.Attributes.Append(end);

                bool first = true;
                bool italicOn = false;
                foreach (string line in text.SplitToLines())
                {
                    if (!first)
                    {
                        XmlNode br = xml.CreateElement("br", "http://www.w3.org/ns/ttml");
                        paragraph.AppendChild(br);
                    }

                    var styles = new Stack<XmlNode>();
                    XmlNode currentStyle = xml.CreateTextNode(string.Empty);
                    paragraph.AppendChild(currentStyle);
                    int skipCount = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (skipCount > 0)
                        {
                            skipCount--;
                        }
                        else if (line.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                        {
                            styles.Push(currentStyle);
                            currentStyle = xml.CreateNode(XmlNodeType.Element, "span", null);
                            paragraph.AppendChild(currentStyle);
                            XmlAttribute attr = xml.CreateAttribute("tts:fontStyle", "http://www.w3.org/ns/10/ttml#style");
                            attr.InnerText = "italic";
                            currentStyle.Attributes.Append(attr);
                            skipCount = 2;
                            italicOn = true;
                        }
                        else if (line.Substring(i).StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                        {
                            currentStyle = xml.CreateNode(XmlNodeType.Element, "span", null);
                            paragraph.AppendChild(currentStyle);
                            XmlAttribute attr = xml.CreateAttribute("tts:fontWeight", "http://www.w3.org/ns/10/ttml#style");
                            attr.InnerText = "bold";
                            currentStyle.Attributes.Append(attr);
                            skipCount = 2;
                        }
                        else if (line.Substring(i).StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                        {
                            int endIndex = line.Substring(i + 1).IndexOf('>');
                            if (endIndex > 0)
                            {
                                skipCount = endIndex + 1;
                                string fontContent = line.Substring(i, skipCount);
                                if (fontContent.Contains(" color=", StringComparison.OrdinalIgnoreCase))
                                {
                                    var arr = fontContent.Substring(fontContent.IndexOf(" color=", StringComparison.Ordinal) + 7).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (arr.Length > 0)
                                    {
                                        string fontColor = arr[0].Trim('\'').Trim('"').Trim('\'');
                                        currentStyle = xml.CreateNode(XmlNodeType.Element, "span", null);
                                        paragraph.AppendChild(currentStyle);
                                        XmlAttribute attr = xml.CreateAttribute("tts:color", "http://www.w3.org/ns/10/ttml#style");
                                        attr.InnerText = fontColor;
                                        currentStyle.Attributes.Append(attr);
                                    }
                                }
                            }
                            else
                            {
                                skipCount = line.Length;
                            }
                        }
                        else if (line.Substring(i).StartsWith("</i>", StringComparison.OrdinalIgnoreCase) || line.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase) || line.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                        {
                            currentStyle = xml.CreateTextNode(string.Empty);
                            if (styles.Count > 0)
                            {
                                currentStyle = styles.Pop().CloneNode(true);
                                currentStyle.InnerText = string.Empty;
                            }
                            paragraph.AppendChild(currentStyle);
                            if (line.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                            {
                                skipCount = 6;
                            }
                            else
                            {
                                skipCount = 3;
                            }

                            italicOn = false;
                        }
                        else
                        {
                            if (i == 0 && italicOn && !line.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                            {
                                styles.Push(currentStyle);
                                currentStyle = xml.CreateNode(XmlNodeType.Element, "span", null);
                                paragraph.AppendChild(currentStyle);
                                XmlAttribute attr = xml.CreateAttribute("tts:fontStyle", "http://www.w3.org/ns/10/ttml#style");
                                attr.InnerText = "italic";
                                currentStyle.Attributes.Append(attr);
                            }
                            currentStyle.InnerText = currentStyle.InnerText + line[i];
                        }
                    }
                    first = false;
                }

                div.AppendChild(paragraph);
            }

            string xmlString = ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty).Replace(" xmlns:tts=\"http://www.w3.org/ns/10/ttml#style\">", ">").Replace("<br />", "<br/>");
            if (subtitle.Header == null)
            {
                subtitle.Header = xmlString;
            }

            return xmlString;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            base.LoadSubtitle(subtitle, lines, fileName);

            // Remove regions (except top center)
            subtitle.Paragraphs.ForEach(p => p.Text = Regex.Replace(p.Text, @"^({\\an[1-7,9]})", string.Empty));
        }

        public override bool HasStyleSupport => false;

    }
}
