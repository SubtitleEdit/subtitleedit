using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TimedTextNoNs : SubtitleFormat
    {
        public override string Extension => Configuration.Settings.SubtitleSettings.TimedText10FileExtension;

        public const string NameOfFormat = "Timed Text No Namespace";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xmlAsString = sb.ToString().Trim();

            if (xmlAsString.Contains("xmlns="))
            {
                return false;
            }

            xmlAsString = xmlAsString.RemoveControlCharactersButWhiteSpace();

            if (xmlAsString.Contains("profile/imsc1"))
            {
                var f = new TimedTextImsc11();
                if (f.IsMine(lines, fileName))
                {
                    return false;
                }
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(xmlAsString);
                var nds = xml.DocumentElement.SelectSingleNode("body");
                var paragraphs = nds.SelectNodes("//p");
                return paragraphs != null && paragraphs.Count > 0;
            }
            catch
            {
                try
                {
                    xml.LoadXml(FixBadXml(xmlAsString));
                    var nds = xml.DocumentElement.SelectSingleNode("body");
                    var paragraphs = nds.SelectNodes("//p");

                    if (paragraphs != null && (paragraphs.Count > 0 && new NetflixTimedText().IsMine(lines, fileName)))
                    {
                        return false;
                    }

                    if (paragraphs != null && (paragraphs.Count > 0 && new SmpteTt2052().IsMine(lines, fileName)))
                    {
                        return false;
                    }

                    return paragraphs != null && paragraphs.Count > 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }

            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var hasStyleHead = false;
            var convertedFromSubStationAlpha = false;
            if (subtitle.Header != null)
            {
                try
                {
                    var x = new XmlDocument();
                    x.LoadXml(subtitle.Header);
                    hasStyleHead = x.DocumentElement.SelectSingleNode("head") != null;
                }
                catch
                {
                    // ignore
                }
            }

            var xml = new XmlDocument();
            var xmlStructure = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
               "<tt>" + Environment.NewLine +
               "   <body>" + Environment.NewLine +
               "       <div />" + Environment.NewLine +
               "   </body>" + Environment.NewLine +
               "</tt>";
            if (!hasStyleHead || string.IsNullOrEmpty(subtitle.Header))
            {
                xml.LoadXml(xmlStructure);
            }
            else
            {
                xml.LoadXml(subtitle.Header);
                XmlNode bodyNode = xml.DocumentElement.SelectSingleNode("//body");
                XmlNode divNode = null;
                if (bodyNode != null)
                {
                    divNode = bodyNode.SelectSingleNode("div");
                }

                if (divNode == null)
                {
                    divNode = xml.DocumentElement.SelectSingleNode("//body").FirstChild;
                }

                if (divNode != null)
                {
                    // Remove all but first div
                    var innerNodeCount = 0;
                    var innerNodeList = new List<XmlNode>();
                    foreach (XmlNode innerNode in bodyNode.SelectNodes("div"))
                    {
                        if (innerNodeCount > 0)
                        {
                            innerNodeList.Add(innerNode);
                        }

                        innerNodeCount++;
                    }
                    foreach (XmlNode child in innerNodeList)
                    {
                        bodyNode.RemoveChild(child);
                    }

                    var lst = new List<XmlNode>();
                    foreach (XmlNode child in divNode.ChildNodes)
                    {
                        lst.Add(child);
                    }

                    foreach (XmlNode child in lst)
                    {
                        divNode.RemoveChild(child);
                    }
                }
                else if (bodyNode == null)  // Don't reload xml if body node exists, otherwise we rewrite header edited by styling form
                {
                    xml.LoadXml(xmlStructure);
                }
            }

            XmlNode body = xml.DocumentElement.SelectSingleNode("body");
            string defaultStyle = Guid.NewGuid().ToString();
            if (body.Attributes["style"] != null)
            {
                defaultStyle = body.Attributes["style"].InnerText;
            }

            XmlNode div = xml.DocumentElement.SelectSingleNode("//body").SelectSingleNode("div");
            if (div == null)
            {
                div = xml.DocumentElement.SelectSingleNode("//body").FirstChild;
            }

            if (div == null)
            {
                div = xml.CreateElement("div");
                body.AppendChild(div);
            }

            var no = 0;
            var divParentNode = div.ParentNode;
            foreach (var p in subtitle.Paragraphs)
            {
                if (p.NewSection)
                {
                    div = xml.CreateElement("div");
                    divParentNode.AppendChild(div);
                }
                if (convertedFromSubStationAlpha && string.IsNullOrEmpty(p.Style))
                {
                    p.Style = p.Extra;
                }

                XmlNode paragraph = MakeParagraph(xml, p);
                div.AppendChild(paragraph);
                no++;
            }

            if (divParentNode != null && divParentNode.HasChildNodes && !divParentNode.FirstChild.HasChildNodes)
            {
                divParentNode.RemoveChild(divParentNode.FirstChild);
            }

            return ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty).Replace(" xmlns:tts=\"http://www.w3.org/ns/10/ttml#style\">", ">");
        }

        private static XmlNode MakeParagraph(XmlDocument xml, Paragraph p)
        {
            XmlNode paragraph = xml.CreateElement("p");
            var text = p.Text.RemoveControlCharactersButWhiteSpace();

            text = Utilities.RemoveSsaTags(text);
            text = HtmlUtil.FixInvalidItalicTags(text);

            // Trying to parse and convert paragraph content
            try
            {
                text = string.Join("<br/>", text.SplitToLines());
                var paragraphContent = new XmlDocument();
                paragraphContent.LoadXml($"<root>{text.Replace("&", "&amp;")}</root>");
                ConvertParagraphNodeToTtmlNode(paragraphContent.DocumentElement, xml, paragraph);
            }
            catch  // Wrong markup, clear it
            {
                text = Regex.Replace(text, "[<>]", "");
                paragraph.AppendChild(xml.CreateTextNode(text));
            }

            XmlAttribute start = xml.CreateAttribute("begin");
            start.InnerText = TimedText10.ConvertToTimeString(p.StartTime);
            paragraph.Attributes.Append(start);

            XmlAttribute end = xml.CreateAttribute("end");
            end.InnerText = TimedText10.ConvertToTimeString(p.EndTime);
            paragraph.Attributes.Append(end);

            return paragraph;
        }

        internal static void ConvertParagraphNodeToTtmlNode(XmlNode node, XmlDocument ttmlXml, XmlNode ttmlNode)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child is XmlText)
                {
                    ttmlNode.AppendChild(ttmlXml.CreateTextNode(child.Value));
                }
                else if (child.Name == "br")
                {
                    XmlNode br = ttmlXml.CreateElement("br");
                    ttmlNode.AppendChild(br);

                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, br);
                }
                else if (child.Name == "i")
                {
                    XmlNode span = ttmlXml.CreateElement("span");
                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else if (child.Name == "b")
                {
                    XmlNode span = ttmlXml.CreateElement("span");
                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else if (child.Name == "u")
                {
                    XmlNode span = ttmlXml.CreateElement("span");
                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else if (child.Name == "font")
                {
                    XmlNode span = ttmlXml.CreateElement("span");

                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else // Default - skip node
                {
                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, ttmlNode);
                }
            }
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
            try
            {
                xml.LoadXml(sb.ToString().RemoveControlCharactersButWhiteSpace().Trim());
            }
            catch
            {
                xml.LoadXml(FixBadXml(sb.ToString()));
            }

            XmlNode body = xml.DocumentElement.SelectSingleNode("body");
            if (body == null)
            {
                return;
            }

            if (BatchSourceFrameRate.HasValue)
            {
                Configuration.Settings.General.CurrentFrameRate = BatchSourceFrameRate.Value;
            }

            Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = null;

            XmlNode lastDiv = null;
            foreach (XmlNode node in body.SelectNodes("//p"))
            {
                try
                {
                    // Parse and convert paragraph text
                    var pText = new StringBuilder();
                    ReadParagraph(pText, node);

                    // Time codes
                    TimedText10.ExtractTimeCodes(node, subtitle, out var begin, out var end);

                    // Style
                    var p = new Paragraph(begin, end, pText.ToString()) { Style = LookupForAttribute("style", node) };

                    if (node.ParentNode.Name == "div")
                    {
                        // check for new div
                        if (lastDiv != null && node.ParentNode != lastDiv)
                        {
                            p.NewSection = true;
                        }

                        lastDiv = node.ParentNode;
                    }

                    p.Text = p.Text.Trim();
                    while (p.Text.Contains(Environment.NewLine + Environment.NewLine))
                    {
                        p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    }

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

        private static string LookupForAttribute(string attr, XmlNode node)
        {
            XmlNode currentNode = node;

            while (currentNode != null && currentNode.NodeType != XmlNodeType.Document)
            {
                if (currentNode.Attributes[attr] != null)
                {
                    return currentNode.Attributes[attr].Value;
                }

                currentNode = currentNode.ParentNode;
            }

            return null;
        }

        private static void ReadParagraph(StringBuilder pText, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Text)
                {
                    pText.Append(child.Value);
                }
                else if (child.Name == "br" || child.Name == "tt:br")
                {
                    pText.AppendLine();
                }
                else if (child.Name == "#significant-whitespace" || child.Name == "tt:#significant-whitespace")
                {
                    pText.Append(child.InnerText);
                }
                else if (child.Name == "span" || child.Name == "tt:span")
                {
                    ReadParagraph(pText, child);
                }
            }
        }

        private static string FixBadXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return xml;
            }

            var idx = xml.IndexOf('<');
            if (idx < 0)
            {
                return xml;
            }

            var result = xml;
            if (idx > 0)
            {
                result = result.Remove(0, idx);
            }

            var fixAmpersandRegex = new Regex("&(?!amp;)");
            return fixAmpersandRegex.Replace(result, "&amp;");
        }
    }
}
