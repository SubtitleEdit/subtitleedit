using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UTSubtitleXml : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "UT Subtitle xml";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
                "<uts>" + Environment.NewLine +
                "</uts>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            XmlNode root = xml.DocumentElement;

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //<ut secOut="26.4" secIn="21.8">
                //  <![CDATA[Pozdrav i dobrodošli natrag<br>u drugi dio naše emisije]]>
                //</ut>
                XmlNode ut = xml.CreateElement("ut");

                XmlAttribute et = xml.CreateAttribute("secOut");
                et.InnerText = string.Format("{0:0.0##}", p.EndTime.TotalSeconds).Replace(",", ".");
                ut.Attributes.Append(et);

                XmlAttribute st = xml.CreateAttribute("secIn");
                st.InnerText = string.Format("{0:0.0##}", p.StartTime.TotalSeconds).Replace(",", ".");
                ut.Attributes.Append(st);

                // Build the CDATA from the RAW text: going through InnerText first escaped the
                // markup ("&lt;i&gt;"), and since CDATA content is not parsed those entities
                // came back literally, turning every <i> into "&lt;i&gt;" on read.
                var cdataText = p.Text.Replace(Environment.NewLine, "<br>");
                if (cdataText.Contains("]]>", StringComparison.Ordinal))
                {
                    // Cannot be expressed in one CDATA section - fall back to escaped text.
                    ut.InnerText = cdataText;
                }
                else
                {
                    ut.InnerXml = "<![CDATA[" + cdataText + "]]>";
                }

                root.AppendChild(ut);
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            if (!JoinLines(lines).Contains("<uts") || !JoinLines(lines).Contains("secOut="))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                string xmlText = JoinLines(lines);
                xml.LoadXml(xmlText);

                foreach (XmlNode node in xml.SelectNodes("//ut"))
                {
                    try
                    {
                        string endTime = node.Attributes["secOut"].InnerText;
                        string startTime = node.Attributes["secIn"].InnerText;
                        string text = node.InnerText;
                        text = text.Replace("<br>", Environment.NewLine).Replace("<br />", Environment.NewLine);
                        var p = new Paragraph();
                        p.StartTime.TotalSeconds = Convert.ToDouble(startTime, System.Globalization.CultureInfo.InvariantCulture);
                        p.EndTime.TotalSeconds = Convert.ToDouble(endTime, System.Globalization.CultureInfo.InvariantCulture);
                        p.Text = text;
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                _errorCount = 1;
            }
        }

    }
}
