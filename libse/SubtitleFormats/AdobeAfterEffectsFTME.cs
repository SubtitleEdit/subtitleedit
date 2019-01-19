using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AdobeAfterEffectsFTME : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Adobe After Effects ft-MarkerExporter";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure = @"
<xml>
  <general>
    <version>1</version>
  </general>
  <layers>
    <layer name='myLayer' index='1'>
    </layer>
  </layers>
</xml>".Replace("'", "\"");

            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(xmlStructure);
            const string innerXml = "<comment value=\"\"/><time value=\"{0}\"/><duration value=\"{1}\"/>";
            XmlNode root = xml.DocumentElement.SelectSingleNode("layers/layer");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("marker");
                paragraph.InnerXml = string.Format(CultureInfo.InvariantCulture, innerXml, p.StartTime.TotalSeconds, p.Duration.TotalSeconds);
                paragraph.SelectSingleNode("comment").Attributes["value"].InnerText = HtmlUtil.RemoveHtmlTags(p.Text, true).Replace(Environment.NewLine, "||");
                root.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string allText = sb.ToString();
            if (!allText.Contains("<layers") && !allText.Contains("<marker>"))
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

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("layers/layer/marker"))
            {
                try
                {
                    double start = Convert.ToDouble(node.SelectSingleNode("time").Attributes["value"].InnerText, CultureInfo.InvariantCulture);
                    double end = start + Convert.ToDouble(node.SelectSingleNode("duration").Attributes["value"].InnerText, CultureInfo.InvariantCulture);
                    string text = node.SelectSingleNode("comment").Attributes["value"].InnerText.Replace("||", Environment.NewLine);
                    subtitle.Paragraphs.Add(new Paragraph(text, start * TimeCode.BaseUnit, end * TimeCode.BaseUnit));
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
