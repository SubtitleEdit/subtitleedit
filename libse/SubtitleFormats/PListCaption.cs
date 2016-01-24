using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

    //<plist version="1.0">
    //  <array>
    //      <dict>
    //          <key>in</key>
    //          <real>0.0</real>
    //          <key>out</key>
    //          <real>3.1600000000000001</real>
    //          <key>text</key>
    //          <string>(Music playing.)</string>
    //          <key>text2</key>
    //          <string></string>
    //      </dict>
    //      ...

    /// <summary>
    /// “Property Lists” (or “PLists” for short) is an xml format by Apple.
    /// </summary>
    public class PListCaption : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".caption"; }
        }

        public override string Name
        {
            get { return "PList Caption xml"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if (xmlAsString.Contains("</plist>") && xmlAsString.Contains("</dict>"))
            {
                XmlDocument xml = new XmlDocument { XmlResolver = null };
                try
                {
                    xml.LoadXml(xmlAsString);
                    var paragraphs = xml.DocumentElement.SelectNodes("array/dict");
                    return paragraphs != null && paragraphs.Count > 0 && xml.DocumentElement.Name == "plist";
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
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<plist>" + Environment.NewLine +
                "   <array>" + Environment.NewLine +
                "   </array>" + Environment.NewLine +
                "</plist>";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            XmlNode div = xml.DocumentElement.SelectSingleNode("array");
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("dict");
                string text = HtmlUtil.RemoveHtmlTags(p.Text);

                XmlNode keyNode = xml.CreateElement("key");
                keyNode.InnerText = "in";
                paragraph.AppendChild(keyNode);

                XmlNode valueNode = xml.CreateElement("real");
                valueNode.InnerText = string.Format("{0:0.0###############}", p.StartTime.TotalSeconds); // 3.1600000000000001
                paragraph.AppendChild(valueNode);

                keyNode = xml.CreateElement("key");
                keyNode.InnerText = "out";
                paragraph.AppendChild(keyNode);

                valueNode = xml.CreateElement("real");
                valueNode.InnerText = string.Format("{0:0.0###############}", p.EndTime.TotalSeconds); // 3.1600000000000001
                paragraph.AppendChild(valueNode);

                int textNo = 0;
                var lines = p.Text.SplitToLines();
                foreach (string line in lines)
                {
                    textNo++;

                    keyNode = xml.CreateElement("key");
                    keyNode.InnerText = "text";
                    if (textNo > 1)
                        keyNode.InnerText = keyNode.InnerText + textNo;
                    paragraph.AppendChild(keyNode);

                    valueNode = xml.CreateElement("string");
                    valueNode.InnerText = line;
                    paragraph.AppendChild(valueNode);
                }
                div.AppendChild(paragraph);
                no++;
            }

            return ToUtf8XmlString(xml).Replace("<plist>", "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">" + Environment.NewLine +
                             "<plist version=\"1.0\">;");
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            XmlDocument xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(sb.ToString().Trim());
            string lastKey = string.Empty;
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("array/dict"))
            {
                try
                {
                    Paragraph p = new Paragraph();
                    StringBuilder pText = new StringBuilder();
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        switch (innerNode.Name)
                        {
                            case "key":
                                lastKey = innerNode.InnerText;
                                break;
                            default:
                                if (lastKey == "in")
                                    p.StartTime.TotalSeconds = double.Parse(innerNode.InnerText);
                                else if (lastKey == "out")
                                    p.EndTime.TotalSeconds = double.Parse(innerNode.InnerText);
                                else if (lastKey.StartsWith("text"))
                                    pText.AppendLine(innerNode.InnerText);
                                break;
                        }
                    }
                    p.Text = pText.ToString().Trim();
                    if (p.StartTime.TotalSeconds >= 0 && p.EndTime.TotalMilliseconds > 0 && !string.IsNullOrEmpty(p.Text))
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

    }
}
