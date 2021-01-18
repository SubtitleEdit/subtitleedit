using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// http://trans.sourceforge.net
    /// </summary>
    public class TranscriberXml : SubtitleFormat
    {
        public override string Extension => ".trs";

        public override string Name => "Transcriber Xml";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
                //                "<!DOCTYPE Trans SYSTEM \"trans-14.dtd\">" + Environment.NewLine +
                "<Trans version=\"1\" version_date=\"981211\">" + Environment.NewLine +
                "   <Episode/>" + Environment.NewLine +
                "</Trans>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            XmlNode episode = xml.DocumentElement.SelectSingleNode("Episode");

            const string format = "{0:0.000}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //<Section type="filler" endTime="10.790" startTime="9.609">
                //<Turn endTime="10.790" startTime="9.609" speaker="sp2"> <Sync time="9.609"/> le journal , Simon Tivolle :
                //</Turn>
                //</Section>
                XmlNode section = xml.CreateElement("Section");
                XmlAttribute t = xml.CreateAttribute("type");
                t.InnerText = "filler";
                section.Attributes.Append(t);

                XmlAttribute et = xml.CreateAttribute("endTime");
                et.InnerText = string.Format(format, p.EndTime.TotalSeconds).Replace(',', '.');
                section.Attributes.Append(et);

                XmlAttribute st = xml.CreateAttribute("startTime");
                st.InnerText = string.Format(format, p.StartTime.TotalSeconds).Replace(',', '.');
                section.Attributes.Append(st);

                XmlNode turn = xml.CreateElement("Turn");
                et = xml.CreateAttribute("endTime");
                et.InnerText = string.Format(format, p.EndTime.TotalSeconds).Replace(',', '.');
                turn.Attributes.Append(et);

                st = xml.CreateAttribute("startTime");
                st.InnerText = string.Format(format, p.StartTime.TotalSeconds).Replace(',', '.');
                turn.Attributes.Append(st);

                turn.InnerText = p.Text;

                section.AppendChild(turn);
                episode.AppendChild(section);
            }

            string returnXml = ToUtf8XmlString(xml);
            returnXml = returnXml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
                                          "<!DOCTYPE Trans SYSTEM \"trans-14.dtd\">");
            return returnXml;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            if (!sb.ToString().Contains("<Trans"))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                string xmlText = sb.ToString();
                var startDocType = xmlText.IndexOf("<!DOCTYPE", StringComparison.Ordinal);
                if (startDocType > 0)
                {
                    int endDocType = xmlText.IndexOf('>', startDocType);
                    xmlText = xmlText.Remove(startDocType, endDocType - startDocType + 1);
                }
                xml.LoadXml(xmlText);

                foreach (XmlNode node in xml.SelectNodes("//Turn"))
                {
                    try
                    {
                        string endTime = node.Attributes["endTime"].InnerText;
                        string startTime = node.Attributes["startTime"].InnerText;
                        string text = node.InnerText;
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
                return;
            }
        }

    }
}
